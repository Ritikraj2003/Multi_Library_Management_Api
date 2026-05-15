using Microsoft.EntityFrameworkCore;
using Multi_Library_Management_Api.Data;
using Multi_Library_Management_Api.Interfaces;
using Multi_Library_Management_Api.Models;

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
