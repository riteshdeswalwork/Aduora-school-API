using System;
namespace sms.api.Interfaces.Admin
{
    /// <summary>
    /// Step-by-step seed APIs. Call in order: 1 → 2 → … → 13 (core), then 14–22 (page-wise data).
    /// All steps use hardcoded data to fill the database so Adora pages have data.
    /// </summary>
    public interface IDataInitializationManager
    {
        Task<object> Step1SchoolInfoAsync();
        Task<object> Step2DepartmentsAsync();
        Task<object> Step3DesignationsAsync();
        Task<object> Step4AdminAsync();
        Task<object> Step4bRoleUsersAsync();
        Task<object> Step5PermissionsAsync();
        Task<object> Step5RolesAsync();
        Task<object> Step5AssignAllPermissionsToRoleAsync(Guid roleId);
        Task<object> Step6AcademicYearsAsync();
        Task<object> Step7PeriodsAsync();
        Task<object> Step8ClassesAsync();
        Task<object> Step9SubjectsAsync();
        Task<object> Step10FeeStructuresAsync();
        Task<object> Step11StaffAsync();
        Task<object> Step12StudentsAsync();
        Task<object> Step13AcademicYearDataAsync();
        Task<object> Step14VisitorEntriesAsync();
        Task<object> Step15TimetableAsync();
        Task<object> Step16HomeworkAsync();
        Task<object> Step17LectureTopicsAsync();
        Task<object> Step18TransferCertificatesAsync();
        Task<object> Step19AchievementsTalentsAsync();
        Task<object> Step20CoCurricularAsync();
        Task<object> Step21PtmFeedbackAsync();
        Task<object> Step22ChatMessagesAsync();
        Task<object> Step23EnquiriesAsync();
        Task<object> Step24WeekOffsAsync();
        Task<object> Step25DocumentPrintJobsAsync();
        Task<object> Step26WorkAllocationsAsync();
        Task<object> Step27StudentIdCardRequestsAsync();
        Task<object> Step28DisciplineIncidentsAsync();
        Task<object> Step29StudentBehaviorIncidentsAsync();
        Task<object> Step30ScholarshipsAsync();
        Task<object> Step31HostelsAsync();
        Task<object> Step32AssetsAsync();
        Task<object> Step33InventoryAsync();
        Task<object> Step34GalleryMediaAsync();
        Task<object> Step35ScheduledServicesAsync();
        Task<object> Step36InviteesAsync();
        Task<object> Step37BusManagementAsync();
        Task<object> Step38LibraryAsync();
        Task<object> Step39OnlinePaymentsAsync();
    }
}
