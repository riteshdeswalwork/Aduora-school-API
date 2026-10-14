-- Paged, SP-backed equivalent of LeaveController.GetAll (GET /api/leaves, Admin),
-- exposed as GET /api/leaves/paged. The original loads ALL StudentLeaves and ALL
-- StaffLeaves via EF, maps both to one anonymous shape and sorts by CreatedAt DESC
-- in memory. This proc does the same server-side: a UNION ALL across the two
-- differently-shaped tables, normalized to one flat row shape (student rows get
-- StaffId = NULL, staff rows get StudentId = NULL; Person_* carries whichever of
-- Students/Staff + its User applies; ApprovedBy_* carries the approving User).
--
-- @AcademicYearId mirrors the EF global query filter on StudentLeave/StaffLeave
-- (CurrentAcademicYearId == null || e.AcademicYearId == CurrentAcademicYearId).
-- @Status (optional) is applied identically to both halves of the union.
--
-- Two result sets, in the order ISqlDataAccess.QueryPagedStoredProcAsync expects:
-- (1) the page of rows, (2) the total count.
CREATE OR ALTER PROCEDURE dbo.sp_Leaves_GetAllPaged
    @PageNumber INT,
    @PageSize INT,
    @Status NVARCHAR(50) = NULL,
    @AcademicYearId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @StatusNorm NVARCHAR(50) = NULLIF(LOWER(LTRIM(RTRIM(@Status))), '');

    SELECT *
    INTO #Combined
    FROM (
        SELECT
            sl.Id,
            sl.StudentId                AS StudentId,
            CAST(NULL AS UNIQUEIDENTIFIER) AS StaffId,
            sl.StartDate, sl.EndDate, sl.Reason, sl.Status,
            sl.ApprovedById             AS ApprovedByStaffId,
            sl.ApprovedAt,
            sl.RejectionReason          AS ApprovalRemarks,
            sl.CreatedAt, sl.UpdatedAt,
            CAST('Student' AS NVARCHAR(10)) AS ApplicantType,
            stu.Id                      AS Person_Id,
            pu.Id                       AS Person_UserId,
            pu.FirstName                AS Person_FirstName,
            pu.LastName                 AS Person_LastName,
            au.Id                       AS ApprovedBy_Id,
            au.FirstName                AS ApprovedBy_FirstName,
            au.LastName                 AS ApprovedBy_LastName
        FROM StudentLeaves sl
        LEFT JOIN Students stu ON stu.Id = sl.StudentId
        LEFT JOIN Users pu ON pu.Id = stu.UserId
        LEFT JOIN Users au ON au.Id = sl.ApprovedById
        WHERE (@AcademicYearId IS NULL OR sl.AcademicYearId = @AcademicYearId)
          AND (@StatusNorm IS NULL OR LOWER(LTRIM(RTRIM(sl.Status))) = @StatusNorm)

        UNION ALL

        SELECT
            fl.Id,
            CAST(NULL AS UNIQUEIDENTIFIER) AS StudentId,
            fl.StaffId                  AS StaffId,
            fl.StartDate, fl.EndDate, fl.Reason, fl.Status,
            fl.ApprovedById             AS ApprovedByStaffId,
            fl.ApprovedAt,
            fl.RejectionReason          AS ApprovalRemarks,
            fl.CreatedAt, fl.UpdatedAt,
            CAST('Staff' AS NVARCHAR(10)) AS ApplicantType,
            st.Id                       AS Person_Id,
            pu.Id                       AS Person_UserId,
            pu.FirstName                AS Person_FirstName,
            pu.LastName                 AS Person_LastName,
            au.Id                       AS ApprovedBy_Id,
            au.FirstName                AS ApprovedBy_FirstName,
            au.LastName                 AS ApprovedBy_LastName
        FROM StaffLeaves fl
        LEFT JOIN Staff st ON st.Id = fl.StaffId
        LEFT JOIN Users pu ON pu.Id = st.UserId
        LEFT JOIN Users au ON au.Id = fl.ApprovedById
        WHERE (@AcademicYearId IS NULL OR fl.AcademicYearId = @AcademicYearId)
          AND (@StatusNorm IS NULL OR LOWER(LTRIM(RTRIM(fl.Status))) = @StatusNorm)
    ) x;

    -- Result set 1: the page (CreatedAt DESC like the original; Id as a stable tie-breaker).
    SELECT *
    FROM #Combined
    ORDER BY CreatedAt DESC, Id
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    -- Result set 2: total count (same filters, unpaged).
    SELECT COUNT(*) AS TotalCount FROM #Combined;

    DROP TABLE #Combined;
END
