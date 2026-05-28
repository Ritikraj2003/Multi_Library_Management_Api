using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;
using System.Text;
using System.Net.Http.Headers;

namespace Multi_Library_Management_Api.Repository
{
    public class GeneralSettingRepository : IGeneralSettingRepository
    {
        private readonly AppDbContext _context;
        public GeneralSettingRepository(AppDbContext context) => _context = context;

        public async Task<Response<List<GeneralSetting>>> GetByLibraryIdAsync(int libraryId)
        {
            var response = new Response<List<GeneralSetting>>();
            try
            {
                var settings = await _context.GeneralSettings
                    .Where(gs => gs.LibraryId == libraryId)
                    .ToListAsync();
                response.Data = settings;
                response.Success = true;
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

        public async Task<Response<GeneralSetting>> UpsertAsync(int libraryId, string key, string value)
        {
            var response = new Response<GeneralSetting>();
            try
            {
                var setting = await _context.GeneralSettings
                    .FirstOrDefaultAsync(gs => gs.LibraryId == libraryId && gs.Key == key);

                if (setting != null)
                {
                    setting.Value = value;
                    setting.UpdatedDate = DateTime.Now;
                    _context.GeneralSettings.Update(setting);
                }
                else
                {
                    setting = new GeneralSetting
                    {
                        LibraryId = libraryId,
                        Key = key,
                        Value = value,
                        CreatedDate = DateTime.Now
                    };
                    _context.GeneralSettings.Add(setting);
                }

                await _context.SaveChangesAsync();
                response.Data = setting;
                response.Success = true;
                response.Message = "Setting saved successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

                        public async Task<Response<bool>> UpsertListAsync(List<Multi_Library_Management_Api.Controllers.UpsertSettingDto> dtos)
        {
            var response = new Response<bool>();
            try
            {
                var dtosList = dtos.ToList();
                var razorKeyDto = dtosList.FirstOrDefault(d => d.Key == "keyId");
                var razorSecretDto = dtosList.FirstOrDefault(d => d.Key == "keySecret");
                if (razorKeyDto != null && razorSecretDto != null)
                {
                    bool isValid = await ValidateRazorpayKeys(razorKeyDto.Value, razorSecretDto.Value);
                    dtosList.Add(new Multi_Library_Management_Api.Controllers.UpsertSettingDto { 
                        LibraryId = razorKeyDto.LibraryId, 
                        Key = "isRazorpayVerified", 
                        Value = isValid ? "true" : "false" 
                    });
                }
                foreach (var dto in dtosList)
                {
                    var setting = await _context.GeneralSettings.FirstOrDefaultAsync(gs => gs.LibraryId == dto.LibraryId && gs.Key == dto.Key);
                    if (setting != null)
                    {
                        setting.Value = dto.Value;
                        setting.UpdatedDate = DateTime.Now;
                        _context.GeneralSettings.Update(setting);
                    }
                    else
                    {
                        _context.GeneralSettings.Add(new GeneralSetting { LibraryId = dto.LibraryId, Key = dto.Key, Value = dto.Value, CreatedDate = DateTime.Now });
                    }
                }
                await _context.SaveChangesAsync();
                response.Data = true;
                response.Success = true;
                response.Message = "Settings saved successfully.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }

                public async Task<bool> ValidateRazorpayKeys(string keyId, string keySecret)
        {
            using (var client = new HttpClient())
            {
                var byteArray = Encoding.ASCII.GetBytes($"{keyId}:{keySecret}");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                var response = await client.GetAsync("https://api.razorpay.com/v1/payments?count=1");
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<Response<bool>> DeleteAsync(int id)
        {
            var response = new Response<bool>();
            try
            {
                var setting = await _context.GeneralSettings.FindAsync(id);
                if (setting == null) { response.Success = false; response.Message = "Setting not found."; return response; }
                
                _context.GeneralSettings.Remove(setting);
                await _context.SaveChangesAsync();
                response.Data = true;
                response.Success = true;
                response.Message = "Setting deleted.";
            }
            catch (Exception ex) { response.Success = false; response.Message = ex.Message; }
            return response;
        }
    }
}

