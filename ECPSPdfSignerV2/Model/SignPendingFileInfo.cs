namespace ECPSPdfSignerV2.Model 
{
    public class SignPendingFileInfo
    {
        public Guid ID { get; set; }
        public Guid CaseID { get; set; }
        public string FileNo { get; set; } = string.Empty;
        public string CaseType { get; set; } = string.Empty;

        public byte[]? Document { get; set; }
        public string? DocumentFileName { get; set; }

        public string? DocumentFileSize { get; set; }
        public string? DocumentContentType { get; set; }
        public string? DocumentExtension { get; set; }
    }
}
