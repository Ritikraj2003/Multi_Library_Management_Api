using Multi_Library_Management_Api.Models;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IGeneralSettingRepository
    {
        Task<Response<List<GeneralSetting>>> GetByLibraryIdAsync(int libraryId);
        Task<Response<GeneralSetting>> UpsertAsync(int libraryId, string key, string value);
        Task<Response<bool>> DeleteAsync(int id);
    }
}
