using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FitManager.Data;
using FitManager.Models;
using FitManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FitManager.Services;

public class WhatsAppService : IWhatsAppService
{
    private readonly ICompanyService    _companyService;
    private readonly IReceiptPdfService _pdfService;
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(
        ICompanyService companyService,
        IReceiptPdfService pdfService,
        ApplicationDbContext db,
        IHttpClientFactory httpFactory,
        IWebHostEnvironment env,
        ILogger<WhatsAppService> logger)
    {
        _companyService = companyService;
        _pdfService     = pdfService;
        _db             = db;
        _httpFactory    = httpFactory;
        _env            = env;
        _logger         = logger;
    }

    // ── Envío completo: sync contacto + todos los eventos activos ─────────────
    public async Task<(bool success, string? error)> SendPaymentNotificationAsync(int paymentId)
    {
        var (settings, payment, events) = await LoadAsync(paymentId);
        if (settings == null) return (false, "WhatsApp no está configurado o habilitado.");
        if (payment  == null) return (false, "Pago no encontrado.");
        if (!events.Any())    return (true,  null); // sin eventos activos → ok

        // ¿Hay evento de Aviso de Vencimiento activo?
        var hasDueDateReminder = events.Any(e => e.SystemEvent == WhatsAppSystemEvent.DueDateReminder);

        // 1. Sincronizar contacto — incluye fechaVencimiento si el aviso está activo
        await SyncContactAsync(settings, payment.Member!, payment, hasDueDateReminder);

        // 2. Generar PDF (para el evento de confirmación de pago)
        var (pdfBase64, pdfName) = GeneratePdf(settings, payment);

        // 3. Disparar cada evento activo
        return await FireAllEventsAsync(settings, payment, pdfBase64, pdfName, events);
    }

    // ── Reenvío: solo dispara eventos activos (sin sync contacto) ─────────────
    public async Task<(bool success, string? error)> ResendPaymentNotificationAsync(int paymentId)
    {
        var (settings, payment, events) = await LoadAsync(paymentId);
        if (settings == null) return (false, "WhatsApp no está configurado o habilitado.");
        if (payment  == null) return (false, "Pago no encontrado.");
        if (!events.Any())    return (false, "No hay eventos activos configurados.");

        var (pdfBase64, pdfName) = GeneratePdf(settings, payment);
        return await FireAllEventsAsync(settings, payment, pdfBase64, pdfName, events);
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private async Task<(CompanySettings? settings, Payment? payment, List<WhatsAppEvent> events)>
        LoadAsync(int paymentId)
    {
        var settings = await _companyService.GetAsync();
        if (!settings.WhatsAppEnabled
            || string.IsNullOrWhiteSpace(settings.WhatsAppApiKey)
            || string.IsNullOrWhiteSpace(settings.WhatsAppApiUrl))
            return (null, null, new());

        var payment = await _db.Payments
            .Include(p => p.Member)
            .Include(p => p.Plan)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        var events = await _db.WhatsAppEvents
            .Where(e => e.IsActive)
            .OrderBy(e => e.SystemEvent)
            .ToListAsync();

        return (settings, payment, events);
    }

    private (string base64, string name) GeneratePdf(CompanySettings settings, Payment payment)
    {
        var vm = new PaymentReceiptViewModel
        {
            Company = settings,
            Member  = payment.Member!,
            Payment = payment
        };
        var bytes  = _pdfService.Generate(vm, _env.WebRootPath);
        var base64 = Convert.ToBase64String(bytes);
        var name   = $"Pago_{payment.Member!.LastName}_{payment.Member.FirstName}_{payment.PaymentDate:ddMMyyyy}.pdf";
        return (base64, name);
    }

    private async Task<(bool success, string? error)> FireAllEventsAsync(
        CompanySettings settings, Payment payment,
        string pdfBase64, string pdfName, List<WhatsAppEvent> events)
    {
        var errors = new List<string>();
        foreach (var evt in events)
        {
            // Solo el evento de Confirmación de Pago lleva PDF adjunto
            var pdf  = evt.SystemEvent == WhatsAppSystemEvent.PaymentConfirmation ? pdfBase64 : null;
            var pdfN = evt.SystemEvent == WhatsAppSystemEvent.PaymentConfirmation ? pdfName   : null;

            var (ok, err) = await FireEventAsync(settings, payment, pdf, pdfN, evt.EventName);
            if (!ok && err != null)
            {
                _logger.LogWarning("Evento WhatsApp '{Name}' falló para pago {Id}: {Err}",
                    evt.EventName, payment.Id, err);
                errors.Add($"[{evt.Label}] {err}");
            }
        }
        return errors.Any()
            ? (false, string.Join(" | ", errors))
            : (true, null);
    }

    // ── Aviso de Deuda: disparo manual desde pantalla de vencimientos ────────────
    public async Task<(bool success, string? error)> SendDebtNoticeAsync(int memberId, DateTime endDate)
    {
        var settings = await _companyService.GetAsync();
        if (!settings.WhatsAppEnabled
            || string.IsNullOrWhiteSpace(settings.WhatsAppApiKey)
            || string.IsNullOrWhiteSpace(settings.WhatsAppApiUrl))
            return (false, "WhatsApp no está configurado o habilitado.");

        var evt = await _db.WhatsAppEvents
            .FirstOrDefaultAsync(e => e.SystemEvent == WhatsAppSystemEvent.DebtNotice && e.IsActive);
        if (evt == null)
            return (false, "El evento 'Aviso de Deuda' no está activo. Actívalo en Configuración → WhatsApp.");

        var member = await _db.Members.FindAsync(memberId);
        if (member == null) return (false, "Socio no encontrado.");

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"{settings.WhatsAppApiUrl}/api/v1/eventos/disparar");
            req.Headers.Add("X-API-Key", settings.WhatsAppApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(new
            {
                eventoNombre       = evt.EventName,
                externalContactoId = member.Id.ToString(),
                parametros         = new
                {
                    nombre           = $"{member.LastName} {member.FirstName}",
                    fechaVencimiento = endDate.ToString("dd/MM/yyyy")
                }
            }), Encoding.UTF8, "application/json");

            var client   = _httpFactory.CreateClient("whatsapp");
            var response = await client.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"HTTP {(int)response.StatusCode}: {body}");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error SendDebtNotice miembro {Id}", memberId);
            return (false, ex.Message);
        }
    }

    private async Task<(bool success, string? error)> SyncContactAsync(
        CompanySettings settings, Member member,
        Payment? payment = null, bool includeDueDate = false)
    {
        try
        {
            // campos: incluye fechaVencimiento si el evento de aviso de vencimiento está activo
            object campos = includeDueDate && payment != null
                ? new { fechaVencimiento = payment.DueDate.ToString("dd/MM/yyyy") }
                : new { };

            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"{settings.WhatsAppApiUrl}/api/v1/contactos/sync");
            req.Headers.Add("X-API-Key", settings.WhatsAppApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(new
            {
                externalId    = member.Id.ToString(),
                nombre        = $"{member.LastName} {member.FirstName}",
                telefono      = member.Phone ?? "",
                campos        = campos,
                sistemaOrigen = settings.Name
            }), Encoding.UTF8, "application/json");

            var client   = _httpFactory.CreateClient("whatsapp");
            var response = await client.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"Sync contacto HTTP {(int)response.StatusCode}: {body}");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sync contacto WhatsApp miembro {Id}", member.Id);
            return (false, ex.Message);
        }
    }

    private async Task<(bool success, string? error)> FireEventAsync(
        CompanySettings settings, Payment payment,
        string? pdfBase64, string? pdfName, string eventName)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post,
                $"{settings.WhatsAppApiUrl}/api/v1/eventos/disparar");
            req.Headers.Add("X-API-Key", settings.WhatsAppApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(new
            {
                eventoNombre       = eventName,
                externalContactoId = payment.Member!.Id.ToString(),
                pdfBase64          = pdfBase64,
                pdfNombre          = pdfName,
                parametros         = new
                {
                    nombre = payment.Member.FirstName,
                    fecha  = payment.PaymentDate.ToString("dd/MM/yyyy"),
                    fechaVencimiento = payment.DueDate.ToString("dd/MM/yyyy")
                }
            }), Encoding.UTF8, "application/json");

            var client   = _httpFactory.CreateClient("whatsapp");
            var response = await client.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                return (false, $"HTTP {(int)response.StatusCode}: {body}");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disparo evento WhatsApp '{Name}' pago {Id}", eventName, payment.Id);
            return (false, ex.Message);
        }
    }
}
