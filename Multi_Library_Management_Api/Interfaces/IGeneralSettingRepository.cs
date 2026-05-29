using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Interfaces
{
    public interface IGeneralSettingRepository
    {
        Task<Response<GeneralSetting?>> GetByLibraryIdAsync(int libraryId);
        Task<Response<bool>> IsRazorpayVerifiedAsync(int libraryId);
        Task<Response<GeneralSetting>> UpsertEmailSettingsAsync(UpsertEmailSettingsDto dto);
        Task<Response<GeneralSetting>> UpsertRazorpaySettingsAsync(UpsertRazorpaySettingsDto dto);
    }
}
