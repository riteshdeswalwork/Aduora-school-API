# Pagination + Stored Procedure Migration Plan

Goal: every GET endpoint that can return a large/unbounded dataset is paginated,
and its data is fetched via a SQL Server stored procedure (through Dapper, see
`Services/Data/ISqlDataAccess.cs`) instead of an EF Core LINQ query - fewer
round trips, server-side filtering/paging, no change-tracking overhead.

This is a large, multi-session migration across ~75 manager files. This file
is the single tracked checklist so progress survives across sessions. Update
the Status column as work lands; do not mark something Done until it has been
rebuilt, started, and smoke-tested against the real endpoint.

Legend: ⬜ Not started · 🔶 In progress · ✅ Done (SP + pagination + verified) · ➖ Skipped (reason noted)

## Infrastructure

| Item | Status | Notes |
|---|---|---|
| Dapper package added | ✅ | `Dapper 2.1.89` |
| `ISqlDataAccess` / `SqlDataAccess` helper | ✅ | `Services/Data/` - QueryStoredProcAsync, QueryPagedStoredProcAsync, ExecuteStoredProcAsync |
| Registered in `Program.cs` DI | ✅ | |
| `StoredProcedures/` SQL scripts folder | ✅ | One .sql file per proc, applied directly against SchoolDbtest |

## Tier 1 - Already paginated, highest traffic (convert LINQ → SP, same response contract, zero frontend risk)

**Status: ✅ all 4 done and verified (2026-10-13).** Next up: Tier 2 (34 controllers).

| Domain | Manager method | Controller route | Status |
|---|---|---|---|
| Students | `IStudentManager.GetPagedAsync` | `GET /api/students/paged` | ✅ `sp_Students_GetPaged` - verified: shape, totalCount(78), search unchanged |
| Staff | `IStaffManager.GetPagedAsync` | `GET /api/staff/paged` | ✅ `sp_Staff_GetPaged` - verified: shape, totalCount(5), search unchanged |
| Fee Payments | `IFeePaymentManager.GetPaymentsPagedAsync` | `GET /api/fee-payments/paged` | ✅ `sp_FeePayments_GetPaged` - endpoint verified (correct empty shape; DB has 0 payment rows right now so the full nested-row/receipts mapping is untested against real data - re-verify once real payments exist) |
| Student Fees | `IStudentFeeManager.GetStudentFeesPagedAsync` | `GET /api/fees/paged` | ✅ `sp_StudentFees_GetPaged` - verified against real data: totalCount(40), nested student/feeStructure/class correct |

## Tier 2 - Controller-level paged queries (pagination exists, but inline LINQ in controller - move to Manager + SP)

34 controllers total (see `grep -rl pageNumber Controllers`). Converted one at a
time; each gets a new `GetPagedAsync` on its Manager interface (previously the
controller queried `_context` directly for these), a `sp_<Entity>_GetPaged.sql`,
and the controller action reduced to a one-line call - response shape unchanged.

| Controller | Status |
|---|---|
| Enquiries | ✅ `sp_Enquiries_GetPaged` - verified (totalCount=2, real rows) |
| Visitor Entries | ✅ `sp_VisitorEntries_GetPaged` - verified (totalCount=3, real rows) |
| Homework Assignments | ✅ `sp_HomeworkAssignments_GetPaged` - verified (totalCount=4, real rows) |
| Document Print Jobs | ✅ `sp_DocumentPrintJobs_GetPaged` - verified (totalCount=2, real rows) |
| Work Allocations | ✅ `sp_WorkAllocations_GetPaged` - verified (totalCount=2, real rows) |
| Student ID Card Requests | ✅ `sp_StudentIdCardRequests_GetPaged` - verified (totalCount=1, real rows) |
| PTM Feedback | ✅ `sp_PtmFeedbacks_GetPaged` - verified (totalCount=3, real rows) |
| Student Behavior Incidents | ✅ `sp_StudentBehaviorIncidents_GetPaged` - verified (shape, totalCount=0, 0 rows in DB currently) |
| Discipline Incidents | ✅ `sp_DisciplineIncidents_GetPaged` - verified (totalCount=1, real rows) |
| Achievement/Talents | ✅ `sp_AchievementTalents_GetPaged` - verified (totalCount=2, real rows) |
| Co-Curricular Activities/Participation | ✅ `sp_CoCurricularActivities_GetPaged` (totalCount=2), `sp_CoCurricularParticipations_GetPaged` (shape, 0 rows) |
| Scholarship/Applications | ✅ `sp_Scholarships_GetPaged` (totalCount=2), `sp_ScholarshipApplications_GetPaged` (totalCount=1) |
| Hostel/Hostel Rooms/Allocations | ✅ `sp_Hostels_GetPaged` (1), `sp_HostelRooms_GetPaged` (2), `sp_HostelAllocations_GetPaged` (0, shape) - all verified |
| Asset/Asset Maintenance | ✅ `sp_Assets_GetPaged` (2), `sp_AssetMaintenanceRecords_GetPaged` (1) - verified |
| Inventory Items/Transactions | ✅ `sp_InventoryItems_GetPaged` (2), `sp_InventoryTransactions_GetPaged` (0, shape) - verified |
| Biometric Devices/Logs/UserMaps | ⬜ |
| Gallery Media | ⬜ |
| Invitees | ⬜ |
| Lecture Topics | ⬜ |
| Scheduled Services | ⬜ |
| Bus | ⬜ |
| Transfer Certificates | ⬜ |
| OnlinePaymentTransaction | ⬜ |

## Tier 3 - Currently unpaginated, genuinely unbounded lists (needs NEW pagination + SP + frontend update)

| Domain | Current method | Why it needs paging |
|---|---|---|
| Library | `GetAllIssuesAsync`, `GetAllFinesAsync`, `GetAllMembersAsync`, `GetAllReservationsAsync` | Grows forever, one row per issue/fine/member/reservation ever |
| Leaves | `GetAllLeavesAsync` (`IStaffLeaveManager`/`IStudentLeaveManager`) | One row per leave request ever filed |
| Chat | Chat message history | Unbounded message history |
| Notifications | In-app notification log | Unbounded per user |

Status: ⬜ not started (all)

## Tier 4 - Small/bounded lists - explicitly NOT being paginated

Periods-per-day, subjects-per-teacher, departments, designations, academic years,
classes-per-staff, periods, holidays, week-offs, buses/routes (a school has a
handful), roles/permissions. These are naturally capped at a few dozen rows by
the domain itself (one school, one academic year) - pagination would add
complexity with no benefit. Left as plain EF Core LINQ.

## How a Tier 1/2/3 conversion is done (template)

1. Read the existing Manager method + its SQL (via EF logging) to know exactly
   what it selects/filters/orders/joins.
2. Write `StoredProcedures/<Name>.sql`: `CREATE OR ALTER PROCEDURE` with the
   same filters as parameters, `OFFSET/FETCH` for paging, and a second
   `SELECT COUNT(*) ...` result set for the total.
3. Run the script against SchoolDbtest (`sqlcmd`/the existing PowerShell runner).
4. Update the Manager method to call `ISqlDataAccess.QueryPagedStoredProcAsync<T>`
   instead of the LINQ query. Keep the method's return type/shape identical.
5. Rebuild (`dotnet build -c Release`), run the backend locally, hit the exact
   endpoint with curl/Playwright, diff the JSON shape against what it returned
   before the change.
6. Flip this file's Status cell to ✅ only after that verification passes.
