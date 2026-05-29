using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using Multi_Library_Management_Api.Models.DTOs;

namespace Multi_Library_Management_Api.Repository
{
    public class GeneralSettingRepository : IGeneralSettingRepository
    {
        private readonly AppDbContext _context;
        private readonly IRazorpayService _razorpayService;

        public GeneralSettingRepository(AppDbContext context, IRazorpayService razorpayService)
        {
            _context = context;
            _razorpayService = razorpayService;
        }

        public async Task<Response<GeneralSetting?>> GetByLibraryIdAsync(int libraryId)
        {
            var response = new Response<GeneralSetting?>();
            try
            {
                var setting = await _context.GeneralSettings
                    .FirstOrDefaultAsync(gs => gs.LibraryId == libraryId);

                response.Data = setting;
                response.Success = true;
                response.Message = setting != null
                    ? "Settings retrieved successfully."
                    : "No settings found for this library.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<bool>> IsRazorpayVerifiedAsync(int libraryId)
        {
            var response = new Response<bool>();
            try
            {
                var isVerified = await _context.GeneralSettings
                    .Where(gs => gs.LibraryId == libraryId)
                    .Select(gs => gs.IsRazorpayVerified)
                    .FirstOrDefaultAsync();

                response.Data = isVerified;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Data = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<GeneralSetting>> UpsertEmailSettingsAsync(UpsertEmailSettingsDto dto)
        {
            var response = new Response<GeneralSetting>();
            try
            {
                var setting = await GetOrCreateAsync(dto.LibraryId);

                setting.Email = dto.Email;
                setting.EmailSmtp = dto.EmailSmtp;
                setting.EmailPort = dto.EmailPort;
                setting.EmailAppPassword = dto.EmailAppPassword;
                setting.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                response.Data = setting;
                response.Success = true;
                response.Message = "Email settings saved successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<GeneralSetting>> UpsertRazorpaySettingsAsync(UpsertRazorpaySettingsDto dto)
        {
            var response = new Response<GeneralSetting>();
            try
            {
                var setting = await GetOrCreateAsync(dto.LibraryId);

                setting.RazorpayKey = dto.RazorpayKey;
                setting.RazorpaySecretKey = dto.RazorpaySecretKey;
                setting.IsRazorpayVerified = await _razorpayService.ValidateRazorpayKeysAsync(
                    dto.RazorpayKey, dto.RazorpaySecretKey);
                setting.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                response.Data = setting;
                response.Success = true;
                response.Message = setting.IsRazorpayVerified
                    ? "Razorpay settings saved and verified successfully."
                    : "Razorpay settings saved but keys could not be verified. Please check Key ID and Secret.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private async Task<GeneralSetting> GetOrCreateAsync(int libraryId)
        {
            var setting = await _context.GeneralSettings
                .FirstOrDefaultAsync(gs => gs.LibraryId == libraryId);

            if (setting != null)
                return setting;

            setting = new GeneralSetting
            {
                LibraryId = libraryId,
                CreatedDate = DateTime.UtcNow
            };
            _context.GeneralSettings.Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }
    }
}
