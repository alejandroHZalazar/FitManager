namespace FitManager.Services;

public interface IWhatsAppService
{
    /// <summary>
    /// Sincroniza el contacto y envía el evento de pago con el PDF adjunto.
    /// Llamar inmediatamente después de registrar un pago.
    /// </summary>
    Task<(bool success, string? error)> SendPaymentNotificationAsync(int paymentId);

    /// <summary>
    /// Reenvía solo el evento de pago (sin sincronizar contacto).
    /// Para el botón "Reenviar WhatsApp" en el historial.
    /// </summary>
    Task<(bool success, string? error)> ResendPaymentNotificationAsync(int paymentId);

    /// <summary>
    /// Dispara el evento "Aviso de Deuda" para un socio con plan vencido.
    /// Se llama manualmente desde la pantalla de vencimientos ya ocurridos.
    /// </summary>
    Task<(bool success, string? error)> SendDebtNoticeAsync(int memberId, DateTime endDate);
}
