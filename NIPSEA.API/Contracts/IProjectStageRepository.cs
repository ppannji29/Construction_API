using NIPSEA.API.Entity;

namespace NIPSEA.API.Contracts
{
    public interface IProjectStageRepository
    {
        public Task<IEnumerable<ProjectStage>> GetProjectStage();
        public Task<IEnumerable<ProjectStage>> RefreshProjectStage();
        public Task<int> CreateNewProjectStage(ProjectStageDto dto);
    }
}
