namespace EcpsService.Models.DTO
{
    public class SignPendingFileDTO
    {
        public Guid caseID { get; set; }
        public string? fileNo { get; set; }
        public string? caseType { get; set; }
        public string? zoneSubZone { get; set; }
        public string? meetingNo { get; set; }
        public int numberOfFloors { get; set; }
        public Guid? subZoneID { get; set; } 
    }

    public class MembersDesignationFlowParseDTO
    {
        public int status { get; set; }
        public List<MembersDesignationFlowDTO> data { get; set; }
        public string message { get; set; }
    }

    public class MembersDesignationFlowDTO
    {
        public Guid memberID { get; set; }
        public string memberDesignation { get; set; }
	}
    public class UserRoles
    {
        public List<string> data { get; set; } = new();
    }
}
