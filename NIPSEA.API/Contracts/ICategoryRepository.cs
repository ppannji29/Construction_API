using NIPSEA.API.Entity;

namespace NIPSEA.API.Contracts
{
    public interface ICategoryRepository
    {
        public Task<IEnumerable<Category>> GetCategory();
        public Task<IEnumerable<Category>> RefreshCategory();
        public Task<int> CreateNewCategory(CategoryDto dto);
    }
}
