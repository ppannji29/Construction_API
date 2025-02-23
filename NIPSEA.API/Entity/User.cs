namespace NIPSEA.API.Entity
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string? Password { get; set; }
    }

    public class User
    {
        public Guid UserID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid ModifiedBy { get; set; }
        public string ModifiedByName { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
