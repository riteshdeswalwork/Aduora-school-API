using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sms.api.Interfaces;
using sms.api.Models;

namespace sms.api.Controllers.Admin
{
    [Route("api/school-info")]
    [ApiController]
    public class SchoolInfoController : ControllerBase
    {
        private readonly ISchoolInfoManager _schoolInfoManager;

        public SchoolInfoController(ISchoolInfoManager schoolInfoManager)
        {
            _schoolInfoManager = schoolInfoManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchoolInfo()
        {
            try
            {
                var schoolInfo = await _schoolInfoManager.GetSchoolInfoAsync();
                if (schoolInfo == null)
                {
                    return NotFound(new { message = "School information not found" });
                }
                return Ok(schoolInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateOrUpdateSchoolInfo([FromBody] SchoolInfo schoolInfo)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _schoolInfoManager.CreateOrUpdateSchoolInfoAsync(schoolInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSchoolInfo([FromBody] SchoolInfo schoolInfo)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _schoolInfoManager.CreateOrUpdateSchoolInfoAsync(schoolInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSchoolInfo(Guid id)
        {
            try
            {
                var result = await _schoolInfoManager.DeleteSchoolInfoAsync(id);
                if (!result)
                    return NotFound(new { message = "School information not found" });

                return Ok(new { message = "School information deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
