namespace NIPSEA.API.Entity
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CategoryDto
    {
        public string Name { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
