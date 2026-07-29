using EcpsService.Models.DTO;
 
namespace ECPSPdfSignerV2.Model 
{
    public class FileToSignResponseModel
    {
        public int status {  get; set; }
        public List<SignPendingFileDTO> data { get; set; } = new();
    }
}
