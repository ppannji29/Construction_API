namespace NIPSEA.API.Entity
{
    public class ProjectStage
    {
        public int ProjectStageID { get; set; }
        public string Name { get; set; }
        public Guid Createdby { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
    }

    public class ProjectStageDto
    {
        public string Name { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
