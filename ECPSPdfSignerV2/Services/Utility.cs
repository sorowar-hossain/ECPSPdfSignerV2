namespace ECPSPdfSignerV2.Services 
{ 
    public static class Utility
    {
        public static void DeleteFile(string localFilePath)
        {
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }
        public static string GetFolderPath()
        {
            var filesFolder = SD.folderPath;

            if(! Directory.Exists(filesFolder))
            {
                Directory.CreateDirectory(filesFolder);
            }

            return filesFolder;
        }

        public static void ClearFolder()
        {
            var filesFolder = SD.folderPath;

            if(! Directory.Exists(filesFolder)) 
                return;

            var files = Directory.GetFiles(filesFolder);

            foreach( var file in files)
            {
                if(File.Exists(file))
                {
                    File.Delete(file);  
                }
            }
        }
    }
}
