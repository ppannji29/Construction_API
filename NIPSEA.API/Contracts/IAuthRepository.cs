using NIPSEA.API.Entity;

namespace NIPSEA.API.Contracts
{
    public interface IAuthRepository
    {
        public Task<IEnumerable<User>> GetAuth();
        public Task<User> GetUser(string Email);
        public Task<User> GetUserByUserID(Guid UserID);
    }
}
