-- Replaces StaffManager.GetPagedAsync (EF LINQ) for GET /api/staff/paged.
-- Mirrors the original filter (search across name/email/username/employee id),
-- sort (FirstName, LastName) and paging exactly. Result set 1: the page of
-- rows. Result set 2: a single row with the total count.
CREATE OR ALTER PROCEDURE dbo.sp_Staff_GetPaged
    @PageNumber INT,
    @PageSize INT,
    @SearchTerm NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), '');
    DECLARE @Like NVARCHAR(204) = '%' + @Term + '%';

    SELECT
        s.Id, s.UserId, s.DesignationId, s.EmployeeId, s.DateOfJoining,
        s.Address, s.PhoneNumber, s.Qualification, s.Salary, s.CreatedAt, s.UpdatedAt,
        u.Id AS User_Id, u.FirstName AS User_FirstName, u.LastName AS User_LastName,
        u.Email AS User_Email, u.Username AS User_Username, u.Role AS User_Role,
        u.IsActive AS User_IsActive, u.ProfilePicture AS User_ProfilePicture,
        d.Id AS Designation_Id, d.Name AS Designation_Name, d.Description AS Designation_Description,
        COALESCE(dept.Name, 'Not assigned') AS Designation_DepartmentName
    INTO #Page
    FROM Staff s
    LEFT JOIN Users u ON u.Id = s.UserId
    LEFT JOIN Designations d ON d.Id = s.DesignationId
    LEFT JOIN Departments dept ON dept.Id = d.DepartmentId
    WHERE
        @Term IS NULL
        OR u.FirstName LIKE @Like OR u.LastName LIKE @Like
        OR u.Email LIKE @Like OR u.Username LIKE @Like
        OR s.EmployeeId LIKE @Like
    ORDER BY u.FirstName, u.LastName
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT * FROM #Page;

    SELECT COUNT(*) AS TotalCount
    FROM Staff s
    LEFT JOIN Users u ON u.Id = s.UserId
    WHERE
        @Term IS NULL
        OR u.FirstName LIKE @Like OR u.LastName LIKE @Like
        OR u.Email LIKE @Like OR u.Username LIKE @Like
        OR s.EmployeeId LIKE @Like;

    DROP TABLE #Page;
END
