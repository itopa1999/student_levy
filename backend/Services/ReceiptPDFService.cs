using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace backend.Services
{
    public class ReceiptPDFService
    {
    private readonly IConverter _converter;

    public ReceiptPDFService(IConverter converter)
    {
        _converter = converter;
    }

    public byte[] GenerateTransactionReceipt(string htmlContent)
    {
        var pdfDocument = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4
            },
            Objects = {
                new ObjectSettings()
                {
                    PagesCount = true,
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" },
                    FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]" }
                }
            }
        };

        var pdf = _converter.Convert(pdfDocument);
        return pdf;
    }
}
}