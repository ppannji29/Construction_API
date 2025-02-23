using NIPSEA.API.Entity;

namespace NIPSEA.API.Contracts
{
    public interface IConstructionRepository
    {
        public Task<bool> InsertNewConstruction(IEnumerable<CreateConstructionDto> constructionDto);
        public Task<bool> UpdateNewConstruction(UpdConstructionDto constructionDto);
        public Task<IEnumerable<Construction>> GetConstructionListingFilterByPackage(FilterListing filter);
        public Task<bool> DeleteConstructionByProjectID(int ProjectID);
        public Task<Construction> GetConstructionDetails(int ProjectID);
    }
}
