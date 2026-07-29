using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
using Syncfusion.Pdf;
using System.Security.Cryptography.X509Certificates;
using EcpsService.Models.DTO;

namespace ECPSPdfSignerV2.Services  
{
    public static class PDFSigner
    {
        public static async Task<bool> SignPDF(string documentToSign, string documentSigned, string signatureImagePath, List<MembersDesignationFlowDTO> data, string role)
        {
            if (role == "ReviewerCM" || role == "ReviewerSP")
            {
                if (!File.Exists(documentToSign) || !File.Exists(signatureImagePath))
                    return false;

                var pdf = await File.ReadAllBytesAsync(documentToSign);
                var rajukLogo = await File.ReadAllBytesAsync(signatureImagePath);

                if (pdf == null || pdf.Length == 0 || rajukLogo == null || rajukLogo.Length == 0)
                    return false;

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2U1hhQlJBfVddXnxLflFyVWBTe116d1dWESFaRnZdRl1kSXpTdUFnW3lacXVd");

                float SignatureWidth = 125;
                float SignatureHeight = 40;
                float SignatureImageSize = 35;
                float MarginX = 5;
                float MarginY = 5;

                using FileStream docStr = new FileStream(documentToSign, FileMode.Open, FileAccess.Read);
                using PdfLoadedDocument loaded = new PdfLoadedDocument(docStr);
                var pageCount = loaded.PageCount;
                docStr.Close();
                // Get the certificate
                using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, "OID.2.5.4.51=Doha House, C=BD, L=Dhaka, O=Dohatec New Media, OU=Certifying Authority, CN=Dohatec CA 2016, PostalCode=1000, STREET=43 Purana Paltan Line", false);
                X509Certificate2 cert = certs[0];

                PdfCertificate pdfCertificate = new PdfCertificate(cert);

                List<string> UsersFlow = new List<string>();
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        UsersFlow.Add(item.memberID.ToString());
                    }
                }

                string flowUser = await SecureStorage.GetAsync("userID");

                for (int i = 0; i < pageCount; i++)
                {
                    var filePath = i == 0 ? documentToSign : documentSigned;

                    using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                    PdfLoadedPage page = loadedDocument.Pages[i] as PdfLoadedPage;

                    float app_x = 0;
                    float app_y = 0;
                    float xx = 0;
                    float yy = 0;

                    //int signatureCount = loadedDocument.Form?.Fields.Count/loadedDocument.PageCount ?? 0;
                    int signatureCount = 0;

                    if (page.Rotation == 0 || page.Rotation == PdfPageRotateAngle.RotateAngle180)
                    {
                        float pageWidth = page.Size.Width;
                        float Pcolumns = (int)(pageWidth / (SignatureWidth + MarginX)) - 1;
                        float columns = UsersFlow.Count;

                        var flowCount = UsersFlow.IndexOf(flowUser.ToLower());
                        signatureCount = flowCount;
                        float rows = 0;

                        if (columns <= Pcolumns)
                        {
                            rows = (int)(signatureCount / columns);
                        }
                        else
                        {
                            if (signatureCount < Pcolumns)
                            {
                                rows = (int)(signatureCount / columns);
                            }
                            else
                            {
                                rows = (int)(columns / signatureCount);
                                signatureCount = signatureCount + 1;
                            }
                        }

                        app_x = 0 + (signatureCount % columns) * (SignatureWidth + MarginX) + MarginX;
                        app_y = page.Size.Height - (rows + 1) * (SignatureHeight + MarginY);
                    }

                    // Create and configure the signature
                    PdfSignature signature = new PdfSignature(loadedDocument, page, pdfCertificate, "Signature");
                    using FileStream imageStream = new FileStream(signatureImagePath, FileMode.Open, FileAccess.Read);
                    PdfBitmap signatureImage = new PdfBitmap(imageStream);

                    signature.Bounds = new Syncfusion.Drawing.RectangleF(app_x, app_y, SignatureWidth, SignatureHeight);
                    signature.ContactInfo = cert.SubjectName.Name;
                    signature.LocationInfo = cert.SubjectName.Name;
                    signature.Reason = "Identification of the signer";

                    if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
                    {
                        signature.Appearance.Normal.Graphics.RotateTransform(-90);
                    }

                    PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 5);
                    signature.Appearance.Normal.Graphics.DrawImage(signatureImage, xx, yy, SignatureImageSize, SignatureImageSize);
                    signature.Appearance.Normal.Graphics.DrawString("Digitally Signed by", font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 3);
                    signature.Appearance.Normal.Graphics.DrawString(cert.SubjectName.Name.Substring(3).Split(',').First(), font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 10);
                    signature.Appearance.Normal.Graphics.DrawString((role.ToLower() == "reviewer (cm)" ? "BC Committee" : role.ToLower() == "reviewer (sp)" ? "LSP Committee" : role), font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 17);
                    signature.Appearance.Normal.Graphics.DrawString("Date: " + DateTime.UtcNow, font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 24);
                    //signature.Appearance.Normal.Graphics.DrawString("Location: Bangladesh", font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 31);

                    //// Save the signed document
                    using MemoryStream stream = new MemoryStream();
                    loadedDocument.Save(stream);
                    stream.Position = 0;
                    loadedDocument.Close(true);
                    using (FileStream fileStream = File.Create(documentSigned))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                return true;
            }
            else 
            {
                if (!File.Exists(documentToSign) || !File.Exists(signatureImagePath))
                    return false;

                var pdf = await File.ReadAllBytesAsync(documentToSign);
                var rajukLogo = await File.ReadAllBytesAsync(signatureImagePath);

                if (pdf == null || pdf.Length == 0 || rajukLogo == null || rajukLogo.Length == 0)
                    return false;

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2U1hhQlJBfVddXnxLflFyVWBTe116d1dWESFaRnZdRl1kSXpTdUFnW3lacXVd");

                float SignatureWidth = 125;
                float SignatureHeight = 40;
                float SignatureImageSize = 35;
                float MarginX = 5;
                float MarginY = 5;
                using FileStream docStr = new FileStream(documentToSign, FileMode.Open, FileAccess.Read);
                using PdfLoadedDocument loaded = new PdfLoadedDocument(docStr);
                var pageCount = loaded.PageCount;
                docStr.Close();

                // Get the certificate
                using X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindByIssuerDistinguishedName, "OID.2.5.4.51=Doha House, C=BD, L=Dhaka, O=Dohatec New Media, OU=Certifying Authority, CN=Dohatec CA 2016, PostalCode=1000, STREET=43 Purana Paltan Line", false);
                X509Certificate2 cert = certs[0];
                PdfCertificate pdfCertificate = new PdfCertificate(cert);

                int signatureCount = loaded.Form?.Fields.Count ?? 0;
                for(var i = 0; i < pageCount; i++)
                {
                    var filePath = i == 0 ? documentToSign : documentSigned;

                    using FileStream docStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    using PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                    PdfLoadedPage page = loadedDocument.Pages[i] as PdfLoadedPage;

                    float app_x = 0;
                    float app_y = 0;
                    float xx = 0;
                    float yy = 0;

                    if (page.Rotation == 0 || page.Rotation == PdfPageRotateAngle.RotateAngle180)
                    {
                        float pageWidth = page.Size.Width;
                        float columns = (int)(pageWidth / (SignatureWidth + MarginX));
                        float rows = (int)(signatureCount / columns);

                        app_x = (signatureCount % columns) * (SignatureWidth + MarginX) + MarginX;
                        app_y = page.Size.Height - (rows + 1) * (SignatureHeight + MarginY);
                    }
                    else if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
                    {
                        (SignatureHeight, SignatureWidth) = (SignatureWidth, SignatureHeight);
                        float pageWidth = page.Size.Height;
                        float columns = (int)(pageWidth / (SignatureHeight + MarginX));
                        float rows = (int)(signatureCount / columns);

                        app_y = page.Size.Height - ((signatureCount + 1) % columns) * (SignatureHeight + MarginX);
                        app_x = page.Size.Width - (rows + 1) * (SignatureWidth + MarginY);
                    }

                    // Create and configure the signature
                    PdfSignature signature = new PdfSignature(loadedDocument, page, pdfCertificate, "Signature");
                    using FileStream imageStream = new FileStream(signatureImagePath, FileMode.Open, FileAccess.Read);
                    PdfBitmap signatureImage = new PdfBitmap(imageStream);

                    signature.Bounds = new Syncfusion.Drawing.RectangleF(app_x, app_y, SignatureWidth, SignatureHeight);
                    signature.ContactInfo = cert.SubjectName.Name;
                    signature.LocationInfo = cert.SubjectName.Name;
                    signature.Reason = "Identification of the signer";

                    if (page.Rotation == PdfPageRotateAngle.RotateAngle90)
                    {
                        signature.Appearance.Normal.Graphics.RotateTransform(-90);
                    }

                    PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 5);
                    signature.Appearance.Normal.Graphics.DrawImage(signatureImage, xx, yy, SignatureImageSize, SignatureImageSize);
                    signature.Appearance.Normal.Graphics.DrawString("Digitally Signed by", font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 3);
                    signature.Appearance.Normal.Graphics.DrawString(cert.SubjectName.Name.Substring(3).Split(',').First(), font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 10);
                    signature.Appearance.Normal.Graphics.DrawString((role.ToLower() == "reviewer (cm)" ? "BC Committee" : role.ToLower() == "reviewer (sp)" ? "LSP Committee" : role), font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 17);
                    signature.Appearance.Normal.Graphics.DrawString("Date: " + DateTime.UtcNow, font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 24);
                    //signature.Appearance.Normal.Graphics.DrawString("Location: Bangladesh", font, PdfBrushes.Black, xx + SignatureImageSize + MarginX, yy + 31);

                    //// Save the signed document
                    using MemoryStream stream = new MemoryStream();
                    loadedDocument.Save(stream);
                    stream.Position = 0;
                    loadedDocument.Close(true);
                    using (FileStream fileStream = File.Create(documentSigned))
                    {
                        stream.CopyTo(fileStream);
                    }
                }
                return true;
            }
        }
    }
}