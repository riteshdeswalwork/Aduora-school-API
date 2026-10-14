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
| Biometric Devices/Logs/UserMaps | ✅ `sp_BiometricDevices_GetPaged`, `sp_BiometricLogs_GetPaged`, `sp_BiometricUserMaps_GetPaged` - all verified (shape, 0 rows in DB currently) |
| Gallery Media | ✅ `sp_GalleryMedia_GetPaged` (totalCount=2) |
| Invitees | ✅ `sp_Invitees_GetPaged` (totalCount=2) |
| Lecture Topics | ✅ `sp_LectureTopics_GetPaged` (totalCount=5) |
| Scheduled Services | ✅ `sp_ScheduledServices_GetPaged` (totalCount=2) |
| Bus (maintenance/paged - was true in-memory paging, now real SQL paging) | ✅ `sp_BusMaintenanceRecords_GetPaged` (totalCount=0, shape) |
| Transfer Certificates | ✅ `sp_TransferCertificates_GetPaged` (totalCount=1) |
| OnlinePaymentTransaction | ✅ `sp_OnlinePaymentTransactions_GetPaged` (totalCount=2) |

**Tier 2 status: ✅ all 34 done and verified (2026-10-14).**

## Tier 3 - Currently unpaginated, genuinely unbounded lists (needs NEW pagination + SP; additive, no frontend break)

| Domain | Current method | Status |
|---|---|---|
| Notifications inbox | `NotificationController.GetInbox` | ✅ Already had page/pageSize - converted its LINQ to `sp_InAppNotifications_GetInboxPaged` (3 result sets: total, unread, page). Verified (empty inbox, correct shape). |
| Chat message history | `ChatController.GetHistory` | ➖ Already bounded (`Take(Math.Clamp(take, 10, 200))`, max 200 rows) - not an unbounded-list risk the way the others are. Left as-is; convert to SP later if desired, low priority. |
| Leaves (combined) | `LeaveController.GetAll` - merges `IStudentLeaveManager`/`IStaffLeaveManager` `GetAllAsync()` into one in-memory list via `dynamic` mapping, Admin-only | ✅ New additive route `GET /api/leaves/paged?pageNumber&pageSize&status` (Admin) via new `ICombinedLeaveManager`/`CombinedLeaveManager` -> `sp_Leaves_GetAllPaged`: `UNION ALL` of StudentLeaves + StaffLeaves normalized to one flat row (student rows `StaffId=NULL`, staff rows `StudentId=NULL`, `Person_*` = Students/Staff + User, `ApprovedBy_*` = approving User) into `#Combined`, then page (CreatedAt DESC, Id) + count. `@Status` applied to both halves; `@AcademicYearId` = `ApplicationDbContext.CurrentAcademicYearId`, mirroring the EF global filter on both leave entities. Old unpaged `GET /api/leaves` left untouched. **Verified against REAL data (2026-10-14):** totalCount=8 = 5 StudentLeaves + 3 StaffLeaves (direct `COUNT(*)`); the 3 staff rows have `staff{id,user{firstName,lastName,fullName}}` populated + `student` null + `applicantType=Staff`, the 5 student rows the mirror image; each of the 8 items serializes to byte-identical JSON vs the old `GET /api/leaves` item with the same id, in the same order; pageSize=3 gives 3/3/2 across 3 pages whose concatenation equals the full list; `status=approved` -> 8, `status=Pending` -> 0; no token -> 401. |
| Library: Book Issues | `LibraryManager.GetAllBookIssuesAsync` (`/api/library/issues`) | ✅ `sp_BookIssues_GetPaged` added as new `/issues/paged` route (`GetBookIssuesPagedAsync`; search on title/ISBN/issue no./membership no./member name, filters `libraryMemberId`, `isReturned`; honours academic-year filter on BookIssue + LibraryMember). Old `/issues` route untouched (still `[]`/200). Proc returns raw columns, C# rebuilds the `BookIssue` graph and reuses `MapBookIssueToDTO` (computed props stay in C#). Verified API shape live (table empty, totalCount=0); SQL joins/sort/paging/filters verified on temp rows in a rolled-back transaction. ⚠️ Found a genuine pre-existing EF mapping bug while building this: `BookIssue`↔`LibraryMember` is missing `.WithMany(lm => lm.BookIssues)`, so EF created an unused shadow FK (`LibraryMemberId1`) and the real `LibraryMember.BookIssues` collection has always been empty in prod - `LibraryMember.HasOverdueBooks` on the OLD `/members` route always reports `false` regardless of reality. Not fixed here (out of scope for an additive paging change); tracked as a follow-up decision. Independently re-confirmed by the Library Members work below. |
| Library: Library Fines | `LibraryManager.GetAllLibraryFinesAsync` (`/api/library/fines`) | ✅ Done (2026-10-14). New additive route `GET /api/library/fines/paged` (pageNumber, pageSize, searchTerm on FineNumber/FineType/Description/Reason, libraryMemberId, status exact case-insensitive) → `LibraryManager.GetLibraryFinesPagedAsync` → `sp_LibraryFines_GetPaged` (INNER JOIN LibraryMembers→Users, LEFT JOIN BookIssues→Books, FineDate DESC). Flat rows are rebuilt into a `LibraryFine` entity graph and passed through the existing `MapLibraryFineToDTO`, so computed props aren't duplicated in SQL. Old `/fines` untouched. Verified on live API: 200 + `{items:[],totalCount:0,...}` with and without all filters, old route still `[]`. ⚠️ Table is empty - row mapping not yet exercised against real data. Note: table also has EF shadow FKs `LibraryMemberId1`/`BookIssueId1`; SP joins on the attributed `LibraryMemberId`/`BookIssueId` like the original Include. |
| Library: Library Members | `LibraryManager.GetAllLibraryMembersAsync` (`/api/library/members`) | ✅ New additive route `GET /api/library/members/paged` (`pageNumber`, `pageSize`, `searchTerm`, `memberType`, `includeInactive`) → `LibraryManager.GetLibraryMembersPagedAsync` → `sp_LibraryMembers_GetPaged` (3 result sets: count, page of members + user name, raw `IsActive/IsReturned/DueDate` of the page's BookIssues). Rebuilds `LibraryMember` + `User` + minimal `BookIssues` in C# and reuses the existing `MapLibraryMemberToDTO`, so computed `HasOverdueBooks`/`CanIssueBooks`/etc. are unchanged code. Honours the `X-Academic-Year-Id` global query filter via `@AcademicYearId`. Old `/members` route untouched. Verified on the live API (table empty → `totalCount:0`, correct shape, all filter combos 200). Child-collection reconstruction additionally verified against real rows via a throwaway harness that inserted members/issues **inside a rolled-back transaction** and diffed against the original EF method: full DTO JSON parity incl. order, paging, search, memberType, includeInactive and AY filter. **Known divergence (intentional, flag for review):** the EF model maps `LibraryMember.BookIssues` to a shadow FK `BookIssues.LibraryMemberId1` (relationship is `.WithMany()` without the inverse nav), which app code never populates - so the original route's `HasOverdueBooks` is effectively always `false`. The SP joins the real FK `LibraryMemberId`, so `/members/paged` reports overdue books correctly; with `LibraryMemberId1` populated the two match exactly. Same shadow-FK issue exists for `BookReservations`/`LibraryFines` collections. |
| Library: Book Reservations | `LibraryManager.GetAllBookReservationsAsync` (`/api/library/reservations`) | ✅ New `GET /api/library/reservations/paged` (old route untouched) via `sp_BookReservations_GetPaged` (page-then-count). Joins Book, LibraryMember→User, FulfilledBy; C# rebuilds the entity + navs and reuses `MapBookReservationToDTO` (computed props stay in C#). Filters: status (default `<> 'Deleted'`), libraryMemberId, search (ReservationNumber/Book.Title/member name), plus `_context.CurrentAcademicYearId` mirroring the EF global filter. Verified: table empty → `totalCount:0`, correct shape; temp-seeded rows (cleaned up) gave byte-identical JSON vs the old route. |

**Tier 3 status: ✅ all 5 done and verified (2026-10-14)** - Notifications inbox, Leaves (combined), Library Book Issues, Library Fines, Library Members, Library Book Reservations. Chat message history remains explicitly skipped (already bounded, see above).

**EF mapping bug found during this migration (not fixed, tracked for a decision):** `BookIssue`↔`LibraryMember` is missing `.WithMany(lm => lm.BookIssues)` on the relationship, so EF generated an unused shadow FK column `BookIssues.LibraryMemberId1` instead of using the real `LibraryMemberId` for that navigation. The app has never populated `LibraryMemberId1`, so `LibraryMember.BookIssues` has always been empty in production and `LibraryMember.HasOverdueBooks` on the existing (old) `/members` route always reports `false` regardless of actual overdue books. The new `/members/paged` route joins on the real FK and reports this correctly, so the two routes now intentionally disagree on this one computed field until the mapping is fixed. The same shadow-FK pattern exists on `BookReservations` and `LibraryFines`. Left as-is pending a decision: fix the EF mapping (would also silently change old-route behavior) or keep tracked as a known discrepancy.

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
