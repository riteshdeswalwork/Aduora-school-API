using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using sms.api.Interfaces.Admin;

namespace sms.api.Controllers.Admin
{
    /// <summary>
    /// Step-by-step seed APIs so every Adora page has data. Call in order 1 → 2 → … → 39 (Swagger only).
    /// All use hardcoded data to fill the database.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DataInitializationController : ControllerBase
    {
        private readonly IDataInitializationManager _dataInitializationManager;

        public DataInitializationController(IDataInitializationManager dataInitializationManager)
        {
            _dataInitializationManager = dataInitializationManager;
        }

        [HttpPost("step-1-school-info")]
        [AllowAnonymous]
        public async Task<IActionResult> Step1SchoolInfo()
        {
            try
            {
                var result = await _dataInitializationManager.Step1SchoolInfoAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 1", error = ex.Message });
            }
        }

        [HttpPost("step-2-departments")]
        [AllowAnonymous]
        public async Task<IActionResult> Step2Departments()
        {
            try
            {
                var result = await _dataInitializationManager.Step2DepartmentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 2", error = ex.Message });
            }
        }

        [HttpPost("step-3-designations")]
        [AllowAnonymous]
        public async Task<IActionResult> Step3Designations()
        {
            try
            {
                var result = await _dataInitializationManager.Step3DesignationsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 3", error = ex.Message });
            }
        }

        [HttpPost("step-4-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> Step4Admin()
        {
            try
            {
                var result = await _dataInitializationManager.Step4AdminAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 4", error = ex.Message });
            }
        }

        /// <summary>
        /// Seeds portal <c>page.*</c> permissions from <c>PagePermissions.GetAllPageCodesForSeed()</c>
        /// (AllPages + AdditionalSeedPageCodes: fee-management, manage-calendar, send-notifications), including actions and tabs.
        /// Ensures Admin has all seeded permissions. Idempotent.
        /// </summary>
        [HttpPost("step-5-permissions")]
        [AllowAnonymous]
        public async Task<IActionResult> Step5Permissions()
        {
            try
            {
                var result = await _dataInitializationManager.Step5PermissionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 5", error = ex.Message });
            }
        }

        [HttpPost("step-5-roles")]
        [AllowAnonymous]
        public async Task<IActionResult> Step5Roles()
        {
            try
            {
                var result = await _dataInitializationManager.Step5RolesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 5 roles", error = ex.Message });
            }
        }

        [HttpPost("step-5-assign-all-permissions/{roleId}")]
        [AllowAnonymous]
        public async Task<IActionResult> Step5AssignAllPermissions(Guid roleId)
        {
            try
            {
                var result = await _dataInitializationManager.Step5AssignAllPermissionsToRoleAsync(roleId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error assigning all permissions in step 5", error = ex.Message });
            }
        }

        [HttpPost("step-6-academic-years")]
        [AllowAnonymous]
        public async Task<IActionResult> Step6AcademicYears()
        {
            try
            {
                var result = await _dataInitializationManager.Step6AcademicYearsAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 6", error = ex.Message });
            }
        }

        [HttpPost("step-7-periods")]
        [AllowAnonymous]
        public async Task<IActionResult> Step7Periods()
        {
            try
            {
                var result = await _dataInitializationManager.Step7PeriodsAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 7", error = ex.Message });
            }
        }

        [HttpPost("step-8-classes")]
        [AllowAnonymous]
        public async Task<IActionResult> Step8Classes()
        {
            try
            {
                var result = await _dataInitializationManager.Step8ClassesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 8", error = ex.Message });
            }
        }

        [HttpPost("step-9-subjects")]
        [AllowAnonymous]
        public async Task<IActionResult> Step9Subjects()
        {
            try
            {
                var result = await _dataInitializationManager.Step9SubjectsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 9", error = ex.Message });
            }
        }

        [HttpPost("step-10-fee-structures")]
        [AllowAnonymous]
        public async Task<IActionResult> Step10FeeStructures()
        {
            try
            {
                var result = await _dataInitializationManager.Step10FeeStructuresAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 10", error = ex.Message });
            }
        }

        [HttpPost("step-11-staff")]
        [AllowAnonymous]
        public async Task<IActionResult> Step11Staff()
        {
            try
            {
                var result = await _dataInitializationManager.Step11StaffAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 11", error = ex.Message });
            }
        }

        [HttpPost("step-12-students")]
        [AllowAnonymous]
        public async Task<IActionResult> Step12Students()
        {
            try
            {
                var result = await _dataInitializationManager.Step12StudentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 12", error = ex.Message });
            }
        }

        [HttpPost("step-13-academic-year-data")]
        [AllowAnonymous]
        public async Task<IActionResult> Step13AcademicYearData()
        {
            try
            {
                var result = await _dataInitializationManager.Step13AcademicYearDataAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 13", error = ex.Message });
            }
        }

        [HttpPost("step-14-visitor-entries")]
        [AllowAnonymous]
        public async Task<IActionResult> Step14VisitorEntries()
        {
            try
            {
                var result = await _dataInitializationManager.Step14VisitorEntriesAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 14", error = ex.Message });
            }
        }

        [HttpPost("step-15-timetable")]
        [AllowAnonymous]
        public async Task<IActionResult> Step15Timetable()
        {
            try
            {
                var result = await _dataInitializationManager.Step15TimetableAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 15", error = ex.Message });
            }
        }

        [HttpPost("step-16-homework")]
        [AllowAnonymous]
        public async Task<IActionResult> Step16Homework()
        {
            try
            {
                var result = await _dataInitializationManager.Step16HomeworkAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 16", error = ex.Message });
            }
        }

        [HttpPost("step-17-lecture-topics")]
        [AllowAnonymous]
        public async Task<IActionResult> Step17LectureTopics()
        {
            try
            {
                var result = await _dataInitializationManager.Step17LectureTopicsAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 17", error = ex.Message });
            }
        }

        [HttpPost("step-18-transfer-certificates")]
        [AllowAnonymous]
        public async Task<IActionResult> Step18TransferCertificates()
        {
            try
            {
                var result = await _dataInitializationManager.Step18TransferCertificatesAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 18", error = ex.Message });
            }
        }

        [HttpPost("step-19-achievements-talents")]
        [AllowAnonymous]
        public async Task<IActionResult> Step19AchievementsTalents()
        {
            try
            {
                var result = await _dataInitializationManager.Step19AchievementsTalentsAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 19", error = ex.Message });
            }
        }

        [HttpPost("step-20-co-curricular")]
        [AllowAnonymous]
        public async Task<IActionResult> Step20CoCurricular()
        {
            try
            {
                var result = await _dataInitializationManager.Step20CoCurricularAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 20", error = ex.Message });
            }
        }

        [HttpPost("step-21-ptm-feedback")]
        [AllowAnonymous]
        public async Task<IActionResult> Step21PtmFeedback()
        {
            try
            {
                var result = await _dataInitializationManager.Step21PtmFeedbackAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 21", error = ex.Message });
            }
        }

        [HttpPost("step-22-chat-messages")]
        [AllowAnonymous]
        public async Task<IActionResult> Step22ChatMessages()
        {
            try
            {
                var result = await _dataInitializationManager.Step22ChatMessagesAsync();
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in step 22", error = ex.Message });
            }
        }

        [HttpPost("step-23-enquiries")]
        [AllowAnonymous]
        public async Task<IActionResult> Step23Enquiries()
        {
            try { return Ok(await _dataInitializationManager.Step23EnquiriesAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 23", error = ex.Message }); }
        }

        [HttpPost("step-24-week-offs")]
        [AllowAnonymous]
        public async Task<IActionResult> Step24WeekOffs()
        {
            try { return Ok(await _dataInitializationManager.Step24WeekOffsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 24", error = ex.Message }); }
        }

        [HttpPost("step-25-document-print-jobs")]
        [AllowAnonymous]
        public async Task<IActionResult> Step25DocumentPrintJobs()
        {
            try { return Ok(await _dataInitializationManager.Step25DocumentPrintJobsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 25", error = ex.Message }); }
        }

        [HttpPost("step-26-work-allocations")]
        [AllowAnonymous]
        public async Task<IActionResult> Step26WorkAllocations()
        {
            try { return Ok(await _dataInitializationManager.Step26WorkAllocationsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 26", error = ex.Message }); }
        }

        [HttpPost("step-27-student-id-card-requests")]
        [AllowAnonymous]
        public async Task<IActionResult> Step27StudentIdCardRequests()
        {
            try { return Ok(await _dataInitializationManager.Step27StudentIdCardRequestsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 27", error = ex.Message }); }
        }

        [HttpPost("step-28-discipline-incidents")]
        [AllowAnonymous]
        public async Task<IActionResult> Step28DisciplineIncidents()
        {
            try { return Ok(await _dataInitializationManager.Step28DisciplineIncidentsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 28", error = ex.Message }); }
        }

        [HttpPost("step-29-student-behavior-incidents")]
        [AllowAnonymous]
        public async Task<IActionResult> Step29StudentBehaviorIncidents()
        {
            try { return Ok(await _dataInitializationManager.Step29StudentBehaviorIncidentsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 29", error = ex.Message }); }
        }

        [HttpPost("step-30-scholarships")]
        [AllowAnonymous]
        public async Task<IActionResult> Step30Scholarships()
        {
            try { return Ok(await _dataInitializationManager.Step30ScholarshipsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 30", error = ex.Message }); }
        }

        [HttpPost("step-31-hostels")]
        [AllowAnonymous]
        public async Task<IActionResult> Step31Hostels()
        {
            try { return Ok(await _dataInitializationManager.Step31HostelsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 31", error = ex.Message }); }
        }

        [HttpPost("step-32-assets")]
        [AllowAnonymous]
        public async Task<IActionResult> Step32Assets()
        {
            try { return Ok(await _dataInitializationManager.Step32AssetsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 32", error = ex.Message }); }
        }

        [HttpPost("step-33-inventory")]
        [AllowAnonymous]
        public async Task<IActionResult> Step33Inventory()
        {
            try { return Ok(await _dataInitializationManager.Step33InventoryAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 33", error = ex.Message }); }
        }

        [HttpPost("step-34-gallery-media")]
        [AllowAnonymous]
        public async Task<IActionResult> Step34GalleryMedia()
        {
            try { return Ok(await _dataInitializationManager.Step34GalleryMediaAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 34", error = ex.Message }); }
        }

        [HttpPost("step-35-scheduled-services")]
        [AllowAnonymous]
        public async Task<IActionResult> Step35ScheduledServices()
        {
            try { return Ok(await _dataInitializationManager.Step35ScheduledServicesAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 35", error = ex.Message }); }
        }

        [HttpPost("step-36-invitees")]
        [AllowAnonymous]
        public async Task<IActionResult> Step36Invitees()
        {
            try { return Ok(await _dataInitializationManager.Step36InviteesAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 36", error = ex.Message }); }
        }

        [HttpPost("step-37-bus-management")]
        [AllowAnonymous]
        public async Task<IActionResult> Step37BusManagement()
        {
            try { return Ok(await _dataInitializationManager.Step37BusManagementAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 37", error = ex.Message }); }
        }

        [HttpPost("step-38-library")]
        [AllowAnonymous]
        public async Task<IActionResult> Step38Library()
        {
            try { return Ok(await _dataInitializationManager.Step38LibraryAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 38", error = ex.Message }); }
        }

        [HttpPost("step-39-online-payments")]
        [AllowAnonymous]
        public async Task<IActionResult> Step39OnlinePayments()
        {
            try { return Ok(await _dataInitializationManager.Step39OnlinePaymentsAsync()); }
            catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Error in step 39", error = ex.Message }); }
        }
    }
}
