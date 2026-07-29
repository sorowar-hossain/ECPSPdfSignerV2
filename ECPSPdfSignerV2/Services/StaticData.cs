namespace ECPSPdfSignerV2.Services  
{
    public static class SD
    {
        public static readonly string _rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
        public static readonly string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Rajuk Files");

        #region User

        public static string _UserName { get; set; } = string.Empty;
        public static string _CurrentUserRole { get; set; } = string.Empty;
        public static List<string> _UserRoles { get; set; } = new();

        #endregion

        #region API URLs

        public static readonly string _baseURL = "http://ecpdev.uru.gov.bd/web-api/";
        //public static readonly string _baseURL = "http://localhost:7186/web-api/";
        public static readonly string _fileUploadURL = "Files/upload-signed-file";
        public static readonly string _fileDownloadURL = "Files/download/";
        public static readonly string _loginURL = "Accounts/login";
        public static readonly string _pendingFilesURL = "DigitalSign/signing-pending-file";
        public static readonly string _signedFilesURL = "DigitalSign/signing-completed-file";
        public static readonly string _MembersDesignationFlow = "DigitalSign/members-info";
        public static readonly string _UserInfoURL = "Accounts/user-info";
        public static readonly string _SetUserRoleURL = "Accounts/select-role";
        #endregion
    }
}
