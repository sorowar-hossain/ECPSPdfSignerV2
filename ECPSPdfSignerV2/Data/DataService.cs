using EcpsService.Models.DTO;

namespace ECPSPdfSignerV2.Data 
{
    public class DataService
    {
        private DatabaseLocal _dbLocal;
        
        private APIService _apiService;

        public DataService(DatabaseLocal dbLocal, APIService service) 
        {
            _dbLocal = dbLocal;
            _apiService = service;
        } 

        public async Task<List<SignPendingFileDTO>?> GetPendingFilesAsync()
        {
            return  await _apiService.GetPendingFilesAsync() ;
        }
    }
}
