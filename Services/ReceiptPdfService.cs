using FitManager.Models;
using FitManager.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FitManager.Services;

public interface IReceiptPdfService
{
    byte[] Generate(PaymentReceiptViewModel vm, string webRootPath);
}

public class ReceiptPdfService : IReceiptPdfService
{
    public byte[] Generate(PaymentReceiptViewModel vm, string webRootPath)
    {
        var p   = vm.Payment;
        var m   = vm.Member;
        var co  = vm.Company;

        var receiptNum = !string.IsNullOrWhiteSpace(p.ReceiptNumber)
            ? p.ReceiptNumber
            : $"REC-{p.Id:D6}";

        var methodLabel = p.Method switch
        {
            PaymentMethod.Cash     => "Efectivo",
            PaymentMethod.Card     => "Tarjeta",
            PaymentMethod.Transfer => "Transferencia",
            _                      => "Otro"
        };
        var statusLabel = p.Status switch
        {
            PaymentStatus.Paid      => "Pagado",
            PaymentStatus.Pending   => "Pendiente",
            PaymentStatus.Overdue   => "Vencido",
            _                       => "Cancelado"
        };

        // ── Logo bytes (si existe) ───────────────────────────────────────────
        byte[]? logoBytes = null;
        if (!string.IsNullOrEmpty(co?.LogoPath))
        {
            var logoPath = Path.Combine(webRootPath, co.LogoPath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(logoPath))
                logoBytes = File.ReadAllBytes(logoPath);
        }

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor("#1a1a1a"));

                page.Content().Column(col =>
                {
                    // ── Encabezado empresa ───────────────────────────────────────
                    col.Item().BorderBottom(2).BorderColor("#1a1a1a").PaddingBottom(10).Row(row =>
                    {
                        // Logo
                        row.ConstantItem(60).Height(60).Element(e =>
                        {
                            if (logoBytes != null)
                                e.Image(logoBytes).FitArea();
                            else
                                e.AlignCenter().AlignMiddle()
                                 .Text(co?.Name?.Substring(0, 1) ?? "F")
                                 .FontSize(30).Bold().FontColor("#444444");
                        });

                        row.ConstantItem(12); // spacer

                        // Datos empresa
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(co?.Name ?? "FitManager").FontSize(18).Bold();
                            if (!string.IsNullOrWhiteSpace(co?.Slogan))
                                c.Item().Text(co.Slogan).FontSize(10).FontColor("#666666");

                            var addr = string.Join(", ", new[] { co?.Address, co?.City, co?.Province }
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
                            if (!string.IsNullOrEmpty(addr))
                                c.Item().Text(addr).FontSize(9).FontColor("#555555");

                            var contactLine = new List<string>();
                            if (!string.IsNullOrWhiteSpace(co?.Phone)) contactLine.Add(co.Phone);
                            if (!string.IsNullOrWhiteSpace(co?.Email)) contactLine.Add(co.Email);
                            if (contactLine.Any())
                                c.Item().Text(string.Join("  |  ", contactLine)).FontSize(9).FontColor("#555555");

                            if (!string.IsNullOrWhiteSpace(co?.TaxId))
                                c.Item().Text($"CUIT: {co.TaxId}").FontSize(9).FontColor("#555555");
                        });

                        // Comprobante info
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Text("Comprobante de Pago")
                             .FontSize(14).Bold().FontColor("#1a1a1a");
                            c.Item().Text($"N° {receiptNum}")
                             .FontSize(11).FontColor("#555555");
                            c.Item().Text($"Emitido: {DateTime.Now:dd/MM/yyyy HH:mm}")
                             .FontSize(9).FontColor("#888888");
                        });
                    });

                    col.Item().Height(12);

                    // ── Datos del socio ──────────────────────────────────────────
                    col.Item().Text("DATOS DEL SOCIO")
                       .FontSize(9).Bold().FontColor("#888888").LetterSpacing(0.8f);
                    col.Item().Height(4);
                    col.Item().Background("#f6f6f6").Padding(10).Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Column(f =>
                            {
                                f.Item().Text("Nombre completo").FontSize(8).FontColor("#888888");
                                f.Item().Text(m?.FullName ?? "—").Bold();
                            });
                            r.RelativeItem().Column(f =>
                            {
                                f.Item().Text("N° de Socio").FontSize(8).FontColor("#888888");
                                f.Item().Text(m?.MemberNumber ?? "—").Bold();
                            });
                        });
                        if (!string.IsNullOrWhiteSpace(m?.DNI) || !string.IsNullOrWhiteSpace(m?.Phone))
                        {
                            c.Item().Height(6);
                            c.Item().Row(r =>
                            {
                                if (!string.IsNullOrWhiteSpace(m?.DNI))
                                    r.RelativeItem().Column(f =>
                                    {
                                        f.Item().Text("DNI / Documento").FontSize(8).FontColor("#888888");
                                        f.Item().Text(m.DNI).Bold();
                                    });
                                if (!string.IsNullOrWhiteSpace(m?.Phone))
                                    r.RelativeItem().Column(f =>
                                    {
                                        f.Item().Text("Teléfono").FontSize(8).FontColor("#888888");
                                        f.Item().Text(m.Phone).Bold();
                                    });
                            });
                        }
                        if (!string.IsNullOrWhiteSpace(m?.Email))
                        {
                            c.Item().Height(6);
                            c.Item().Column(f =>
                            {
                                f.Item().Text("Email").FontSize(8).FontColor("#888888");
                                f.Item().Text(m.Email).Bold();
                            });
                        }
                    });

                    col.Item().Height(12);

                    // ── Detalle del pago ─────────────────────────────────────────
                    col.Item().Text("DETALLE DEL PAGO")
                       .FontSize(9).Bold().FontColor("#888888").LetterSpacing(0.8f);
                    col.Item().Height(4);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                        });

                        // Header
                        table.Header(header =>
                        {
                            foreach (var h in new[] { "Concepto", "Método", "Fecha Pago", "Estado" })
                                header.Cell().Background("#2c2c2c").Padding(6)
                                      .Text(h).FontColor(Colors.White).Bold().FontSize(9);
                        });

                        // Data row
                        var concept = p.Description ?? p.Plan?.Name ?? "Cuota mensual";
                        table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(7).Text(concept);
                        table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(7).Text(methodLabel);
                        table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(7).Text(p.PaymentDate.ToString("dd/MM/yyyy"));
                        table.Cell().BorderBottom(1).BorderColor("#eeeeee").Padding(7).Text(statusLabel)
                             .FontColor(p.Status == PaymentStatus.Paid ? "#155724" : "#856404").Bold();

                        // Amount / due-date row
                        table.Cell().ColumnSpan(2).Background("#f9f9f9").Padding(8).Column(c =>
                        {
                            c.Item().Text("Fecha de vencimiento:").FontSize(8).FontColor("#888888");
                            c.Item().Text(p.DueDate.ToString("dd/MM/yyyy")).Bold();
                        });
                        table.Cell().ColumnSpan(2).Background("#f0f9f0").Padding(8).AlignRight().Column(c =>
                        {
                            c.Item().Text("TOTAL").FontSize(8).FontColor("#555555").Bold();
                            c.Item().Text($"$ {p.Amount:N2}").FontSize(16).Bold().FontColor("#1a7a1a");
                        });
                    });

                    // ── Notas ────────────────────────────────────────────────────
                    if (!string.IsNullOrWhiteSpace(p.Notes))
                    {
                        col.Item().Height(10);
                        col.Item().BorderTop(1).BorderColor("#cccccc").PaddingTop(8).Column(c =>
                        {
                            c.Item().Text("OBSERVACIONES").FontSize(9).Bold().FontColor("#888888");
                            c.Item().Height(4);
                            c.Item().Text(p.Notes).FontSize(10).FontColor("#555555");
                        });
                    }

                    // ── Footer ───────────────────────────────────────────────────
                    col.Item().Height(20);
                    col.Item().BorderTop(1).BorderColor("#dddddd").PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Este comprobante es válido como constancia de pago no fiscal.")
                             .FontSize(8).FontColor("#aaaaaa");
                            if (!string.IsNullOrWhiteSpace(co?.Phone) || !string.IsNullOrWhiteSpace(co?.Email))
                            {
                                var contact = string.Join("  ", new[] { co?.Phone, co?.Email }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                                c.Item().Text($"Consultas: {contact}").FontSize(8).FontColor("#aaaaaa");
                            }
                        });
                        row.ConstantItem(180).AlignRight().Column(c =>
                        {
                            c.Item().Height(30);
                            c.Item().BorderTop(1).BorderColor("#999999").Width(180)
                             .PaddingTop(4).AlignCenter()
                             .Text("Firma y sello").FontSize(8).FontColor("#666666");
                        });
                    });
                });
            });
        }).GeneratePdf();
    }
}
