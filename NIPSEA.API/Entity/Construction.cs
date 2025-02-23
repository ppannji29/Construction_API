namespace NIPSEA.API.Entity
{
    public class Construction
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int ProjectStageID { get; set; }
        public string StageName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public DateOnly ProjectStartDate { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class FilterListing
    {
        public string? ProjectName { get; set; } = "";
        public int? CategoryID { get; set; } = 0;
        public int? ProjectStageID { get; set; } = 0;
        public DateOnly? ProjectStartDate { get; set; } = null!;
    }

    public class CreateConstructionDto
    {
        public string ProjectName {  get; set; }
        public int ProjectStageID { get; set; }
        public string StageName { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public DateOnly ProjectStartDate { get; set; }
        public string Description { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class UpdConstructionDto
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int ProjectStageID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public DateOnly ProjectStartDate { get; set; }
        public string Description { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
