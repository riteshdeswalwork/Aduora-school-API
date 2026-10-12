using Microsoft.EntityFrameworkCore;
using sms.api.Constants;
using sms.api.Data;
using sms.api.Interfaces.Admin;
using sms.api.Interfaces.Auth;
using sms.api.Interfaces.Academic;
using sms.api.Interfaces.Fee;
using sms.api.Models;
using sms.api.Models.Enums;

namespace sms.api.Managers.Admin
{
    public class DataInitializationManager : IDataInitializationManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserManager _userManager;
        private readonly IDepartmentManager _departmentManager;
        private readonly IDesignationManager _designationManager;
        private readonly IAdminManager _adminManager;
        private readonly IStaffManager _staffManager;
        private readonly IStudentManager _studentManager;
        private readonly IClassManager _classManager;
        private readonly ISubjectManager _subjectManager;
        private readonly IFeeStructureManager _feeStructureManager;
        private readonly ISchoolInfoManager _schoolInfoManager;
        private readonly IPeriodManager _periodManager;
        private readonly IAcademicYearManager _academicYearManager;
        private readonly ITimetableManager _timetableManager;

        public DataInitializationManager(
            ApplicationDbContext context,
            IUserManager userManager,
            IDepartmentManager departmentManager,
            IDesignationManager designationManager,
            IAdminManager adminManager,
            IStaffManager staffManager,
            IStudentManager studentManager,
            IClassManager classManager,
            ISubjectManager subjectManager,
            IFeeStructureManager feeStructureManager,
            ISchoolInfoManager schoolInfoManager,
            IPeriodManager periodManager,
            IAcademicYearManager academicYearManager,
            ITimetableManager timetableManager)
        {
            _context = context;
            _userManager = userManager;
            _departmentManager = departmentManager;
            _designationManager = designationManager;
            _adminManager = adminManager;
            _staffManager = staffManager;
            _studentManager = studentManager;
            _classManager = classManager;
            _subjectManager = subjectManager;
            _feeStructureManager = feeStructureManager;
            _schoolInfoManager = schoolInfoManager;
            _periodManager = periodManager;
            _academicYearManager = academicYearManager;
            _timetableManager = timetableManager;
        }

        public async Task<object> Step1SchoolInfoAsync() => await InitializeSchoolInfo();
        public async Task<object> Step2DepartmentsAsync() => await InitializeDepartments();
        public async Task<object> Step3DesignationsAsync() => await InitializeDesignations();
        public async Task<object> Step4AdminAsync() => await InitializeAdmin();
        public async Task<object> Step4bRoleUsersAsync() => await InitializeRoleUsers();
        public async Task<object> Step5PermissionsAsync()
        {
            var pageCodes = PagePermissions.GetAllPageCodesForSeed();

            foreach (var pageCode in pageCodes)
            {
                var parentName = pageCode.Replace(PagePermissions.PagePrefix, "").Replace("-", " ");
                Permission? parent = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == pageCode);
                if (parent == null)
                {
                    parent = new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = parentName,
                        Code = pageCode,
                        Module = "Portal",
                        Action = "View",
                        Description = "Access to " + parentName,
                        IsActive = true,
                        IsSystemPermission = true,
                        ParentPermissionId = null
                    };
                    _context.Permissions.Add(parent);
                    await _context.SaveChangesAsync();
                }

                var actions = PageActionPermissions.GetActionsForPage(pageCode);
                foreach (var action in actions)
                {
                    var actionCode = PageActionPermissions.GetActionCode(pageCode, action);
                    if (string.IsNullOrEmpty(actionCode)) continue;
                    if (await _context.Permissions.AnyAsync(p => p.Code == actionCode)) continue;

                    var actionName = action.Substring(0, 1).ToUpperInvariant() + action.Substring(1) + " " + parentName;
                    _context.Permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Name = actionName,
                        Code = actionCode,
                        Module = "Portal",
                        Action = actionName,
                        Description = actionName,
                        IsActive = true,
                        IsSystemPermission = true,
                        ParentPermissionId = parent.Id
                    });
                }

                var tabs = PageTabPermissions.GetTabsForPage(pageCode);
                foreach (var tabKey in tabs)
                {
                    var tabCode = PageTabPermissions.GetTabCode(pageCode, tabKey);
                    if (string.IsNullOrEmpty(tabCode)) continue;
                    Permission? tab = await _context.Permissions.FirstOrDefaultAsync(p => p.Code == tabCode);
                    if (tab == null)
                    {
                        var tabName = tabKey.Replace("-", " ");
                        tab = new Permission
                        {
                            Id = Guid.NewGuid(),
                            Name = tabName,
                            Code = tabCode,
                            Module = "Portal",
                            Action = "Tab",
                            Description = tabName,
                            IsActive = true,
                            IsSystemPermission = true,
                            ParentPermissionId = parent.Id
                        };
                        _context.Permissions.Add(tab);
                        await _context.SaveChangesAsync();
                    }

                    var tabActions = PageTabPermissions.GetActionsForTab(pageCode, tabKey);
                    foreach (var ta in tabActions)
                    {
                        var taCode = PageTabPermissions.GetTabActionCode(pageCode, tabKey, ta);
                        if (string.IsNullOrEmpty(taCode)) continue;
                        if (await _context.Permissions.AnyAsync(p => p.Code == taCode)) continue;
                        var taName = ta.Substring(0, 1).ToUpperInvariant() + ta.Substring(1) + " " + tabKey.Replace("-", " ");
                        _context.Permissions.Add(new Permission
                        {
                            Id = Guid.NewGuid(),
                            Name = taName,
                            Code = taCode,
                            Module = "Portal",
                            Action = taName,
                            Description = taName,
                            IsActive = true,
                            IsSystemPermission = true,
                            ParentPermissionId = tab.Id
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();

            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Full access to portal",
                    IsActive = true,
                    IsSystemRole = true
                };
                _context.Roles.Add(adminRole);
                await _context.SaveChangesAsync();
            }

            var allPerms = await _context.Permissions
                .Where(p => p.IsActive && (pageCodes.Contains(p.Code) || p.ParentPermissionId != null))
                .Select(p => p.Id)
                .Distinct()
                .ToListAsync();
            foreach (var permId in allPerms)
            {
                if (await _context.RolePermissions.AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permId)) continue;
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = adminRole.Id,
                    PermissionId = permId,
                    IsActive = true
                });
            }
            await _context.SaveChangesAsync();

            return new
            {
                message = "Page, action, and tab permissions seeded (AllPages + AdditionalSeedPageCodes); Admin role has full access.",
                permissionsCount = allPerms.Count,
                roleId = adminRole.Id,
                pageRootCount = pageCodes.Length,
                additionalRoutePages = PagePermissions.AdditionalSeedPageCodes
            };
        }

        public async Task<object> Step5RolesAsync()
        {
            if (!await _context.Permissions.AnyAsync(p => p.Code.StartsWith(PagePermissions.PagePrefix)))
                await Step5PermissionsAsync();

            var permissionList = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => new { p.Id, p.Code })
                .ToListAsync();
            var rows = permissionList.Select(p => (p.Id, p.Code)).ToList();

            var rolesSummary = new List<object>();
            var rolesCreated = 0;
            var rolesMetadataUpdated = 0;

            foreach (var def in SystemRoleDefinitions.All)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == def.Name.ToLower());
                if (role == null)
                {
                    role = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = def.Name,
                        Description = def.Description,
                        DataScope = def.DataScope,
                        IsActive = true,
                        IsSystemRole = true
                    };
                    _context.Roles.Add(role);
                    rolesCreated++;
                }
                else
                {
                    var metaChanged = false;
                    if (role.Description != def.Description)
                    {
                        role.Description = def.Description;
                        metaChanged = true;
                    }
                    if (role.DataScope != def.DataScope)
                    {
                        role.DataScope = def.DataScope;
                        metaChanged = true;
                    }
                    if (!role.IsSystemRole)
                    {
                        role.IsSystemRole = true;
                        metaChanged = true;
                    }
                    if (!role.IsActive)
                    {
                        role.IsActive = true;
                        metaChanged = true;
                    }
                    if (metaChanged)
                    {
                        role.UpdatedAt = DateTime.UtcNow;
                        rolesMetadataUpdated++;
                    }
                }

                var permIds = ResolvePermissionIdsForDefinition(rows, def);
                await SyncRolePermissionsToSetAsync(role.Id, permIds);
                rolesSummary.Add(new { name = def.Name, permissionCount = permIds.Count });
            }

            await _context.SaveChangesAsync();

            var teacherRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "teacher");
            Role? staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name.ToLower() == "staff");
            var legacyStaffSynced = false;
            if (teacherRole != null)
            {
                if (staffRole == null)
                {
                    const string staffDescCreate = "Teaching portal access (aligned with Teacher; legacy Staff name).";
                    staffRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Staff",
                        Description = staffDescCreate,
                        DataScope = DataScopeType.AssignedClasses,
                        IsActive = true,
                        IsSystemRole = true
                    };
                    _context.Roles.Add(staffRole);
                    await _context.SaveChangesAsync();
                }

                var teacherPermIds = await _context.RolePermissions
                    .Where(rp => rp.RoleId == teacherRole.Id && rp.IsActive)
                    .Select(rp => rp.PermissionId)
                    .ToListAsync();
                await SyncRolePermissionsToSetAsync(staffRole.Id, teacherPermIds);
                const string staffDesc = "Teaching portal access (aligned with Teacher; legacy Staff name).";
                if (!staffRole.IsSystemRole || staffRole.Description != staffDesc || staffRole.DataScope != DataScopeType.AssignedClasses)
                {
                    staffRole.IsSystemRole = true;
                    staffRole.Description = staffDesc;
                    staffRole.DataScope = DataScopeType.AssignedClasses;
                    staffRole.UpdatedAt = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                legacyStaffSynced = true;
            }

            return new
            {
                message = "System roles ensured; permissions synchronized (existing roles updated where needed).",
                rolesCreated,
                rolesMetadataUpdated,
                roles = rolesSummary,
                legacyStaffSyncedWithTeacher = legacyStaffSynced
            };
        }

        private static List<Guid> ResolvePermissionIdsForDefinition(
            IReadOnlyList<(Guid Id, string Code)> permissions,
            SystemRoleDefinitions.Definition def)
        {
            if (def.GrantAllPermissions)
                return permissions.Select(p => p.Id).Distinct().ToList();

            var set = new HashSet<Guid>();

            foreach (var page in def.FullAccessPageCodes)
            {
                foreach (var p in permissions)
                {
                    if (string.Equals(p.Code, page, StringComparison.OrdinalIgnoreCase) ||
                        p.Code.StartsWith(page + ".", StringComparison.Ordinal))
                        set.Add(p.Id);
                }
            }

            foreach (var page in def.ViewOnlyPageCodes)
            {
                foreach (var p in permissions)
                {
                    if (string.Equals(p.Code, page, StringComparison.OrdinalIgnoreCase))
                        set.Add(p.Id);
                    else if (p.Code.StartsWith(page + ".", StringComparison.Ordinal) &&
                             p.Code.EndsWith(".view", StringComparison.OrdinalIgnoreCase))
                        set.Add(p.Id);
                }
            }

            foreach (var spec in def.TabAccessSpecs)
            {
                var tabPrefix = $"{spec.PageCode}.tab.{spec.TabKey.ToLowerInvariant()}.";
                foreach (var p in permissions)
                {
                    if (string.Equals(p.Code, spec.PageCode, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.Code, spec.PageCode + ".view", StringComparison.OrdinalIgnoreCase))
                        set.Add(p.Id);
                    else if (!p.Code.StartsWith(tabPrefix, StringComparison.Ordinal))
                        continue;
                    else if (spec.FullAccess || p.Code.EndsWith(".view", StringComparison.OrdinalIgnoreCase))
                        set.Add(p.Id);
                }
            }

            return set.ToList();
        }

        private async Task SyncRolePermissionsToSetAsync(Guid roleId, IReadOnlyCollection<Guid> desiredIds)
        {
            var desired = desiredIds.ToHashSet();
            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            var toRemove = existing.Where(rp => !desired.Contains(rp.PermissionId)).ToList();
            if (toRemove.Count > 0)
                _context.RolePermissions.RemoveRange(toRemove);

            var kept = existing.Where(rp => desired.Contains(rp.PermissionId)).Select(rp => rp.PermissionId).ToHashSet();
            foreach (var pid in desired)
            {
                if (kept.Contains(pid)) continue;
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = pid,
                    IsActive = true
                });
            }
        }

        public async Task<object> Step5AssignAllPermissionsToRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null) throw new InvalidOperationException("Role not found");

            var permissionIds = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            if (existing.Count > 0)
                _context.RolePermissions.RemoveRange(existing);

            foreach (var pid in permissionIds)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = pid,
                    IsActive = true
                });
            }
            await _context.SaveChangesAsync();
            return new { message = "All permissions assigned to role", roleId, count = permissionIds.Count };
        }

        public async Task<object> Step6AcademicYearsAsync() => await InitializeAcademicYears();
        public async Task<object> Step7PeriodsAsync() => await InitializePeriods();
        public async Task<object> Step8ClassesAsync() => await InitializeClasses();
        public async Task<object> Step9SubjectsAsync() => await InitializeSubjects();
        public async Task<object> Step10FeeStructuresAsync() => await InitializeFeeStructures();
        public async Task<object> Step11StaffAsync() => await InitializeStaff();
        public async Task<object> Step12StudentsAsync() => await InitializeStudents();
        public async Task<object> Step13AcademicYearDataAsync() => await SeedPreviousAndCurrentAcademicYearDataAsync();
        public async Task<object> Step14VisitorEntriesAsync() => await SeedVisitorEntriesAsync();
        public async Task<object> Step15TimetableAsync() => await SeedTimetableAsync();
        public async Task<object> Step16HomeworkAsync() => await SeedHomeworkAsync();
        public async Task<object> Step17LectureTopicsAsync() => await SeedLectureTopicsAsync();
        public async Task<object> Step18TransferCertificatesAsync() => await SeedTransferCertificatesAsync();
        public async Task<object> Step19AchievementsTalentsAsync() => await SeedAchievementsTalentsAsync();
        public async Task<object> Step20CoCurricularAsync() => await SeedCoCurricularAsync();
        public async Task<object> Step21PtmFeedbackAsync() => await SeedPtmFeedbackAsync();
        public async Task<object> Step22ChatMessagesAsync() => await SeedChatMessagesAsync();
        public async Task<object> Step23EnquiriesAsync() => await SeedEnquiriesAsync();
        public async Task<object> Step24WeekOffsAsync() => await SeedWeekOffsAsync();
        public async Task<object> Step25DocumentPrintJobsAsync() => await SeedDocumentPrintJobsAsync();
        public async Task<object> Step26WorkAllocationsAsync() => await SeedWorkAllocationsAsync();
        public async Task<object> Step27StudentIdCardRequestsAsync() => await SeedStudentIdCardRequestsAsync();
        public async Task<object> Step28DisciplineIncidentsAsync() => await SeedDisciplineIncidentsAsync();
        public async Task<object> Step29StudentBehaviorIncidentsAsync() => await SeedStudentBehaviorIncidentsAsync();
        public async Task<object> Step30ScholarshipsAsync() => await SeedScholarshipsAsync();
        public async Task<object> Step31HostelsAsync() => await SeedHostelsAsync();
        public async Task<object> Step32AssetsAsync() => await SeedAssetsAsync();
        public async Task<object> Step33InventoryAsync() => await SeedInventoryAsync();
        public async Task<object> Step34GalleryMediaAsync() => await SeedGalleryMediaAsync();
        public async Task<object> Step35ScheduledServicesAsync() => await SeedScheduledServicesAsync();
        public async Task<object> Step36InviteesAsync() => await SeedInviteesAsync();
        public async Task<object> Step37BusManagementAsync() => await SeedBusManagementAsync();
        public async Task<object> Step38LibraryAsync() => await SeedLibraryAsync();
        public async Task<object> Step39OnlinePaymentsAsync() => await SeedOnlinePaymentsAsync();

        private async Task<object> InitializeAcademicYears()
        {
            var now = DateTime.UtcNow;
            int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
            int endYear = startYear + 1;
            string currentYearCode = $"{startYear}-{endYear.ToString().Substring(2)}";
            string previousYearCode = $"{startYear - 1}-{startYear.ToString().Substring(2)}";

            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (admin?.User == null)
                throw new InvalidOperationException("Run step-4-admin first.");

            var createdByUserId = admin.UserId;

            if (await _context.AcademicYears.AnyAsync(ay => ay.YearCode == currentYearCode))
                return new { message = "Academic years already exist", previousYearCode, currentYearCode };

            var prevDto = await _academicYearManager.GetAcademicYearByCodeAsync(previousYearCode);
            if (prevDto == null)
            {
                await _academicYearManager.CreateAcademicYearAsync(new sms.api.DTOs.CreateAcademicYearDTO
                {
                    YearCode = previousYearCode,
                    YearName = $"Academic Year {startYear - 1}-{startYear}",
                    StartDate = new DateTime(startYear - 1, 4, 1),
                    EndDate = new DateTime(startYear, 3, 31),
                    IsCurrentYear = false,
                    Description = "Previous academic year",
                    NumberOfTerms = 2,
                    TermNames = "First Term,Second Term",
                    FinancialYear = previousYearCode
                }, createdByUserId);
            }

            var currDto = await _academicYearManager.GetAcademicYearByCodeAsync(currentYearCode);
            if (currDto == null)
            {
                await _academicYearManager.CreateAcademicYearAsync(new sms.api.DTOs.CreateAcademicYearDTO
                {
                    YearCode = currentYearCode,
                    YearName = $"Academic Year {startYear}-{endYear}",
                    StartDate = new DateTime(startYear, 4, 1),
                    EndDate = new DateTime(endYear, 3, 31),
                    IsCurrentYear = true,
                    Description = "Current academic year",
                    NumberOfTerms = 2,
                    TermNames = "First Term,Second Term",
                    FinancialYear = currentYearCode
                }, createdByUserId);
            }

            return new { message = "Academic years created", previousYearCode, currentYearCode };
        }

        private async Task<object> InitializeSchoolInfo()
        {
            var existingSchoolInfo = await _context.SchoolInfos.OrderBy(s => s.CreatedAt).FirstOrDefaultAsync();
            if (existingSchoolInfo != null)
            {
                return new { message = "School info already exists", schoolInfo = existingSchoolInfo };
            }

            var schoolInfo = new SchoolInfo
            {
                Id = Guid.NewGuid(),
                SchoolName = "Excellence Academy",
                Mission = "Providing quality education and nurturing future leaders",
                Vision = "To be a leading educational institution that empowers students to achieve their full potential",
                Address = "123 Education Street, Knowledge City, State 12345",
                PhoneNumber = "+1 (555) 123-4567",
                Email = "info@excellenceacademy.edu",
                Website = "www.excellenceacademy.edu",
                EstablishedYear = 2010,
                Principal = "Dr. Sarah Johnson",
                VicePrincipal = "Mr. John Anderson",
                AffiliationBoard = "State Education Board",
                Description = "A leading educational institution committed to excellence",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SchoolInfos.Add(schoolInfo);
            await _context.SaveChangesAsync();
            return new { message = "School info created", schoolInfo };
        }

        private async Task<object> InitializeDepartments()
        {
            var existingDepartments = await _context.Departments.AnyAsync();
            if (existingDepartments)
            {
                return new { message = "Departments already exist" };
            }

            var departments = new List<Department>
            {
                new Department { Id = Guid.NewGuid(), Name = "Administration", Description = "Administrative Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Academic", Description = "Academic Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Science", Description = "Science Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Mathematics", Description = "Mathematics Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "English", Description = "English Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Social Studies", Description = "Social Studies Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Physical Education", Description = "Physical Education Department", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Department { Id = Guid.NewGuid(), Name = "Arts", Description = "Arts Department", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var dept in departments)
            {
                await _departmentManager.CreateAsync(dept);
            }

            return new { message = "Departments created", count = departments.Count, departments = departments.Select(d => new { d.Id, d.Name }) };
        }

        private async Task<object> InitializeDesignations()
        {
            var existingDesignations = await _context.Designations.AnyAsync();
            if (existingDesignations)
            {
                return new { message = "Designations already exist" };
            }

            var departments = await _context.Departments.ToListAsync();
            var adminDept = departments.FirstOrDefault(d => d.Name == "Administration");
            var academicDept = departments.FirstOrDefault(d => d.Name == "Academic");

            var designations = new List<Designation>
            {
                new Designation { Id = Guid.NewGuid(), Name = "Principal", DepartmentId = adminDept?.Id, Description = "School Principal", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Vice Principal", DepartmentId = adminDept?.Id, Description = "Vice Principal", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Head Teacher", DepartmentId = academicDept?.Id, Description = "Head Teacher", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Senior Teacher", DepartmentId = academicDept?.Id, Description = "Senior Teacher", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Teacher", DepartmentId = academicDept?.Id, Description = "Teacher", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Assistant Teacher", DepartmentId = academicDept?.Id, Description = "Assistant Teacher", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Librarian", DepartmentId = academicDept?.Id, Description = "Librarian", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Designation { Id = Guid.NewGuid(), Name = "Lab Assistant", DepartmentId = academicDept?.Id, Description = "Laboratory Assistant", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var designation in designations)
            {
                _context.Designations.Add(designation);
            }
            await _context.SaveChangesAsync();

            return new { message = "Designations created", count = designations.Count, designations = designations.Select(d => new { d.Id, d.Name }) };
        }

        private async Task<object> InitializeAdmin()
        {
            var existingAdmin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (existingAdmin != null)
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Admin",
                        Description = "Full access to portal",
                        IsActive = true,
                        IsSystemRole = true
                    };
                    _context.Roles.Add(adminRole);
                    await _context.SaveChangesAsync();
                }
                var adminUserId = existingAdmin.User?.Id ?? Guid.Empty;
                if (adminUserId != Guid.Empty)
                {
                    var hasLink = await _context.UserRoles.AnyAsync(ur => ur.UserId == adminUserId && ur.RoleId == adminRole.Id && ur.IsActive);
                    if (!hasLink)
                    {
                        _context.UserRoles.Add(new UserRole
                        {
                            Id = Guid.NewGuid(),
                            UserId = adminUserId,
                            RoleId = adminRole.Id,
                            IsActive = true,
                            AssignedAt = DateTime.UtcNow,
                            AssignedByUserId = adminUserId
                        });
                        await _context.SaveChangesAsync();
                    }
                }
                return new { message = "Admin already exists", admin = new { existingAdmin.Id, existingAdmin.User?.FirstName, existingAdmin.User?.LastName, existingAdmin.User?.Email } };
            }

            // Create admin user first
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "System",
                LastName = "Administrator",
                Username = "admin",
                Email = "admin@excellenceacademy.edu",
                Role = "Admin",
                PhoneNumber = "+1 (555) 123-4567",
                Address = "123 Education Street, Knowledge City",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set password (you should hash this properly)
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            // Create admin profile
            var admin = new Models.Admin
            {
                Id = Guid.NewGuid(),
                UserId = adminUser.Id,
                AdminLevel = "Super Admin",
                Department = "Administration"
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            var adminRoleEnsure = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRoleEnsure == null)
            {
                adminRoleEnsure = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = "Admin",
                    Description = "Full access to portal",
                    IsActive = true,
                    IsSystemRole = true
                };
                _context.Roles.Add(adminRoleEnsure);
                await _context.SaveChangesAsync();
            }
            if (!await _context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRoleEnsure.Id && ur.IsActive))
            {
                _context.UserRoles.Add(new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = adminUser.Id,
                    RoleId = adminRoleEnsure.Id,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                    AssignedByUserId = adminUser.Id
                });
                await _context.SaveChangesAsync();
            }

            return new {
                message = "Admin created successfully",
                admin = new { admin.Id, adminUser.FirstName, adminUser.LastName, adminUser.Email },
                credentials = new { username = "admin", password = "Admin@123" }
            };
        }

        private async Task<object> InitializePeriods()
        {
            var existingPeriods = await _context.Periods.AnyAsync();
            if (existingPeriods)
            {
                return new { message = "Periods already exist" };
            }

            var adminUser = await _context.Users.Where(u => u.Role == "Admin").OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
            if (adminUser == null)
                throw new InvalidOperationException("Admin user must exist before creating periods. Run InitializeAdmin first.");

            var createdByUserId = adminUser.Id;
            var dayOfWeek = 1; // Monday - same slots for all weekdays
            var periodData = new[]
            {
                (Name: "Period 1", Start: (8, 0, 0), End: (8, 45, 0), Type: "Regular"),
                (Name: "Period 2", Start: (8, 45, 0), End: (9, 30, 0), Type: "Regular"),
                (Name: "Break", Start: (9, 30, 0), End: (9, 45, 0), Type: "Break"),
                (Name: "Period 3", Start: (9, 45, 0), End: (10, 30, 0), Type: "Regular"),
                (Name: "Period 4", Start: (10, 30, 0), End: (11, 15, 0), Type: "Regular"),
                (Name: "Period 5", Start: (11, 15, 0), End: (12, 0, 0), Type: "Regular"),
                (Name: "Lunch Break", Start: (12, 0, 0), End: (12, 45, 0), Type: "Lunch"),
                (Name: "Period 6", Start: (12, 45, 0), End: (13, 30, 0), Type: "Regular"),
                (Name: "Period 7", Start: (13, 30, 0), End: (14, 15, 0), Type: "Regular"),
                (Name: "Period 8", Start: (14, 15, 0), End: (15, 0, 0), Type: "Regular")
            };

            var periods = new List<Period>();
            for (var i = 0; i < periodData.Length; i++)
            {
                var p = periodData[i];
                periods.Add(new Period
                {
                    Id = Guid.NewGuid(),
                    Name = p.Name,
                    Description = p.Name,
                    StartTime = new TimeSpan(p.Start.Item1, p.Start.Item2, p.Start.Item3),
                    EndTime = new TimeSpan(p.End.Item1, p.End.Item2, p.End.Item3),
                    DayOfWeek = dayOfWeek,
                    PeriodType = p.Type,
                    OrderIndex = i + 1,
                    IsSchoolWide = true,
                    IsActive = true,
                    CreatedByUserId = createdByUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            foreach (var period in periods)
            {
                _context.Periods.Add(period);
            }
            await _context.SaveChangesAsync();

            return new { message = "Periods created", count = periods.Count, periods = periods.Select(p => new { p.Id, p.Name, p.StartTime, p.EndTime }) };
        }

        private async Task<object> InitializeClasses()
        {
            var existingClasses = await _context.Classes.AnyAsync();
            if (existingClasses)
            {
                return new { message = "Classes already exist" };
            }

            var classes = new List<Class>
            {
                new Class { Id = Guid.NewGuid(), Name = "Class 1", GradeLevel = "1", Section = "A", Capacity = 30, RoomNumber = "101", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 1", GradeLevel = "1", Section = "B", Capacity = 30, RoomNumber = "102", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 2", GradeLevel = "2", Section = "A", Capacity = 30, RoomNumber = "201", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 2", GradeLevel = "2", Section = "B", Capacity = 30, RoomNumber = "202", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 3", GradeLevel = "3", Section = "A", Capacity = 30, RoomNumber = "301", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 3", GradeLevel = "3", Section = "B", Capacity = 30, RoomNumber = "302", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 4", GradeLevel = "4", Section = "A", Capacity = 30, RoomNumber = "401", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 4", GradeLevel = "4", Section = "B", Capacity = 30, RoomNumber = "402", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 5", GradeLevel = "5", Section = "A", Capacity = 30, RoomNumber = "501", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 5", GradeLevel = "5", Section = "B", Capacity = 30, RoomNumber = "502", AcademicYear = "2024-25", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var cls in classes)
            {
                _context.Classes.Add(cls);
            }
            await _context.SaveChangesAsync();

            return new { message = "Classes created", count = classes.Count, classes = classes.Select(c => new { c.Id, c.Name, c.Section, c.RoomNumber }) };
        }

        private async Task<object> InitializeSubjects()
        {
            var existingSubjects = await _context.Subjects.AnyAsync();
            if (existingSubjects)
            {
                return new { message = "Subjects already exist" };
            }

            var departments = await _context.Departments.ToListAsync();
            var mathDept = departments.FirstOrDefault(d => d.Name == "Mathematics");
            var scienceDept = departments.FirstOrDefault(d => d.Name == "Science");
            var englishDept = departments.FirstOrDefault(d => d.Name == "English");
            var socialDept = departments.FirstOrDefault(d => d.Name == "Social Studies");
            var peDept = departments.FirstOrDefault(d => d.Name == "Physical Education");
            var artsDept = departments.FirstOrDefault(d => d.Name == "Arts");

            var subjects = new List<Subject>
            {
                new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH", Description = "Mathematics", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "English", Code = "ENG", Description = "English Language", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Science", Code = "SCI", Description = "General Science", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Social Studies", Code = "SS", Description = "Social Studies", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Physical Education", Code = "PE", Description = "Physical Education", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Art", Code = "ART", Description = "Art and Craft", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Computer Science", Code = "CS", Description = "Computer Science", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Subject { Id = Guid.NewGuid(), Name = "Hindi", Code = "HIN", Description = "Hindi Language", IsActive = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var subject in subjects)
            {
                _context.Subjects.Add(subject);
            }
            await _context.SaveChangesAsync();

            return new { message = "Subjects created", count = subjects.Count, subjects = subjects.Select(s => new { s.Id, s.Name, s.Code }) };
        }

        private async Task<object> InitializeFeeStructures()
        {
            var existingFeeStructures = await _context.FeeStructures.AnyAsync();
            if (existingFeeStructures)
            {
                return new { message = "Fee structures already exist" };
            }

            var academicYear = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            if (academicYear == null)
                throw new InvalidOperationException("Run step-6-academic-years first.");

            var yearCode = academicYear.YearCode;
            var classes = await _context.Classes.ToListAsync();
            var feeStructures = new List<FeeStructure>();

            foreach (var cls in classes)
            {
                var baseFee = GetBaseFeeForGrade(cls.GradeLevel);

                var feeStructure = new FeeStructure
                {
                    Id = Guid.NewGuid(),
                    ClassId = cls.Id,
                    Name = $"Fee Structure for {cls.Name} {cls.Section}",
                    Description = $"Annual fee structure for {cls.Name} Section {cls.Section}",
                    Amount = baseFee,
                    FeeType = "Yearly",
                    AcademicYear = yearCode,
                    AcademicYearId = academicYear.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                feeStructures.Add(feeStructure);
                _context.FeeStructures.Add(feeStructure);
            }
            await _context.SaveChangesAsync();

            return new { message = "Fee structures created", count = feeStructures.Count };
        }

        private decimal GetBaseFeeForGrade(string gradeLevel)
        {
            return gradeLevel switch
            {
                "1" => 5000m,
                "2" => 5500m,
                "3" => 6000m,
                "4" => 6500m,
                "5" => 7000m,
                _ => 5000m
            };
        }

        private async Task<object> InitializeStaff()
        {
            var existingStaff = await _context.Staff.AnyAsync();
            if (existingStaff)
            {
                return new { message = "Staff already exist" };
            }

            var departments = await _context.Departments.ToListAsync();
            var designations = await _context.Designations.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();

            var academicDept = departments.FirstOrDefault(d => d.Name == "Academic");
            var teacherDesignation = designations.FirstOrDefault(d => d.Name == "Teacher");
            var seniorTeacherDesignation = designations.FirstOrDefault(d => d.Name == "Senior Teacher");

            var staffMembers = new List<Models.Staff>();
            var staffUsers = new List<User>();

            // Create sample staff members
            var staffData = new[]
            {
                new { FirstName = "John", LastName = "Smith", Email = "john.smith@excellenceacademy.edu", Subject = "Mathematics", Username = "john.smith" },
                new { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@excellenceacademy.edu", Subject = "English", Username = "sarah.johnson" },
                new { FirstName = "Michael", LastName = "Brown", Email = "michael.brown@excellenceacademy.edu", Subject = "Science", Username = "michael.brown" },
                new { FirstName = "Emily", LastName = "Davis", Email = "emily.davis@excellenceacademy.edu", Subject = "Social Studies", Username = "emily.davis" },
                new { FirstName = "David", LastName = "Wilson", Email = "david.wilson@excellenceacademy.edu", Subject = "Physical Education", Username = "david.wilson" }
            };

            foreach (var staffInfo in staffData)
            {
                // Create user for staff
                var staffUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = staffInfo.FirstName,
                    LastName = staffInfo.LastName,
                    Username = staffInfo.Username,
                    Email = staffInfo.Email,
                    Role = "Staff",
                    PhoneNumber = $"+1 (555) {Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}",
                    Address = $"{Random.Shared.Next(100, 999)} Education Street, Knowledge City",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                staffUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123");
                _context.Users.Add(staffUser);
                await _context.SaveChangesAsync();
                staffUsers.Add(staffUser);

                // Create staff profile
                var staff = new Models.Staff
                {
                    Id = Guid.NewGuid(),
                    UserId = staffUser.Id,
                    EmployeeId = $"EMP{DateTime.UtcNow.Year}{Random.Shared.Next(1000, 9999)}",
                    DesignationId = teacherDesignation?.Id,
                    DateOfJoining = DateTime.UtcNow.AddYears(-Random.Shared.Next(1, 10)),
                    Address = $"{Random.Shared.Next(100, 999)} Education Street, Knowledge City",
                    PhoneNumber = $"+1 (555) {Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}",
                    Qualification = "Bachelor's Degree",
                    Salary = Random.Shared.Next(30000, 60000),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();
                staffMembers.Add(staff);
            }

            return new {
                message = "Staff created successfully",
                count = staffMembers.Count,
                staff = staffMembers.Select(s => new { s.Id, s.EmployeeId }),
                defaultPassword = "Staff@123"
            };
        }

        private async Task<object> InitializeStudents()
        {
            var existingStudents = await _context.Students.AnyAsync();
            if (existingStudents)
            {
                return new { message = "Students already exist" };
            }

            var classes = await _context.Classes.ToListAsync();
            var students = new List<Models.Student>();
            var studentUsers = new List<User>();

            // Create sample students for each class
            var firstNames = new[] { "Alex", "Emma", "Liam", "Olivia", "Noah", "Ava", "William", "Sophia", "James", "Isabella", "Benjamin", "Mia", "Lucas", "Charlotte", "Henry", "Amelia", "Alexander", "Harper", "Mason", "Evelyn" };
            var lastNames = new[] { "Anderson", "Brown", "Clark", "Davis", "Evans", "Garcia", "Harris", "Jackson", "Johnson", "Jones", "Lee", "Martin", "Miller", "Moore", "Rodriguez", "Smith", "Taylor", "Thomas", "White", "Wilson" };

            foreach (var cls in classes)
            {
                // Create 5-10 students per class
                var studentCount = Random.Shared.Next(5, 11);

                for (int i = 0; i < studentCount; i++)
                {
                    var firstName = firstNames[Random.Shared.Next(firstNames.Length)];
                    var lastName = lastNames[Random.Shared.Next(lastNames.Length)];
                    var email = $"{firstName.ToLower()}.{lastName.ToLower()}@student.excellenceacademy.edu";
                    var username = $"{firstName.ToLower()}.{lastName.ToLower()}";

                    // Create user for student
                    var studentUser = new User
                    {
                        Id = Guid.NewGuid(),
                        FirstName = firstName,
                        LastName = lastName,
                        Username = username,
                        Email = email,
                        Role = "Student",
                        PhoneNumber = $"+1 (555) {Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}",
                        Address = $"{Random.Shared.Next(100, 999)} Student Street, Knowledge City",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    studentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123");
                    _context.Users.Add(studentUser);
                    await _context.SaveChangesAsync();
                    studentUsers.Add(studentUser);

                    // Create student profile
                    var student = new Models.Student
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentUser.Id,
                        RegistrationNumber = $"REG{DateTime.UtcNow.Year}{Random.Shared.Next(1000, 9999)}",
                        RollNumber = $"ROLL{Random.Shared.Next(100, 999)}",
                        DateOfBirth = new DateTime(DateTime.UtcNow.Year - int.Parse(cls.GradeLevel) - 5, Random.Shared.Next(1, 13), Random.Shared.Next(1, 28)),
                        Gender = Random.Shared.Next(0, 2) == 0 ? "Male" : "Female",
                        GuardianName = $"{firstNames[Random.Shared.Next(firstNames.Length)]} {lastName}",
                        GuardianContact = $"+1 (555) {Random.Shared.Next(100, 999)}-{Random.Shared.Next(1000, 9999)}",
                        CurrentGrade = cls.GradeLevel,
                        EnrollmentDate = DateTime.UtcNow.AddMonths(-Random.Shared.Next(1, 12)),
                        AdmissionDate = DateTime.UtcNow.AddMonths(-Random.Shared.Next(1, 12)),
                        Status = StudentStatus.Active
                    };

                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                    students.Add(student);
                }
            }

            return new {
                message = "Students created successfully",
                count = students.Count,
                studentsPerClass = students.GroupBy(s => s.CurrentGrade).Select(g => new { Grade = g.Key, Count = g.Count() }),
                defaultPassword = "Student@123"
            };
        }

        private async Task EnsureStaffExistsAsync()
        {
            if (await _context.Staff.AnyAsync()) return;
            await InitializeStaff();
        }

        private async Task EnsureStudentsExistAsync()
        {
            if (await _context.Students.AnyAsync()) return;
            var classes = await _context.Classes.ToListAsync();
            if (!classes.Any()) return;
            await InitializeStudents();
        }

        private async Task<object> SeedPreviousAndCurrentAcademicYearDataAsync()
        {
            // Requires steps 1–12 to be run first (school, departments, designations, admin, periods, classes, subjects, fee structures, staff, students).

            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (admin?.User == null)
                throw new InvalidOperationException("Run initialize-admin first.");

            var createdByUserId = admin.UserId;

            // 1b. Ensure staff and students exist (create if missing) - pure system me data
            await EnsureStaffExistsAsync();
            await EnsureStudentsExistAsync();

            // 2. Compute previous and current academic year (April–March)
            var now = DateTime.UtcNow;
            int startYear = now.Month >= 4 ? now.Year : now.Year - 1;
            int endYear = startYear + 1;
            string currentYearCode = $"{startYear}-{endYear.ToString().Substring(2)}";
            string previousYearCode = $"{startYear - 1}-{startYear.ToString().Substring(2)}";

            // 3. Get or create AcademicYear for previous and current
            var currentYearDto = await _academicYearManager.GetAcademicYearByCodeAsync(currentYearCode);
            if (currentYearDto == null)
            {
                await _academicYearManager.CreateAcademicYearAsync(new sms.api.DTOs.CreateAcademicYearDTO
                {
                    YearCode = currentYearCode,
                    YearName = $"Academic Year {startYear}-{endYear}",
                    StartDate = new DateTime(startYear, 4, 1),
                    EndDate = new DateTime(endYear, 3, 31),
                    IsCurrentYear = true,
                    Description = "Current academic year",
                    NumberOfTerms = 2,
                    TermNames = "First Term,Second Term",
                    FinancialYear = currentYearCode
                }, createdByUserId);
                currentYearDto = await _academicYearManager.GetAcademicYearByCodeAsync(currentYearCode);
            }

            var previousYearDto = await _academicYearManager.GetAcademicYearByCodeAsync(previousYearCode);
            if (previousYearDto == null)
            {
                await _academicYearManager.CreateAcademicYearAsync(new sms.api.DTOs.CreateAcademicYearDTO
                {
                    YearCode = previousYearCode,
                    YearName = $"Academic Year {startYear - 1}-{startYear}",
                    StartDate = new DateTime(startYear - 1, 4, 1),
                    EndDate = new DateTime(startYear, 3, 31),
                    IsCurrentYear = false,
                    Description = "Previous academic year",
                    NumberOfTerms = 2,
                    TermNames = "First Term,Second Term",
                    FinancialYear = previousYearCode
                }, createdByUserId);
                previousYearDto = await _academicYearManager.GetAcademicYearByCodeAsync(previousYearCode);
            }

            var currentYearEntity = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.YearCode == currentYearCode);
            var previousYearEntity = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.YearCode == previousYearCode);
            if (currentYearEntity == null || previousYearEntity == null)
                throw new InvalidOperationException("Academic years could not be created.");

            // 4. Ensure base data exists
            var students = await _context.Students.Include(s => s.User).ToListAsync();
            var classes = await _context.Classes.ToListAsync();
            var staffList = await _context.Staff.ToListAsync();
            var subjects = await _context.Subjects.ToListAsync();
            if (!students.Any() || !classes.Any())
                throw new InvalidOperationException("Could not create students or classes. Ensure departments, designations, and subjects exist.");

            int classesPrev = 0, classesCurr = 0, enrollmentsPrev = 0, enrollmentsCurr = 0;
            int feesPrev = 0, feesCurr = 0, paymentsPrev = 0, paymentsCurr = 0;
            int holidaysCount = 0, eventsCount = 0, announcementsCount = 0;

            // 5. Create classes for previous year if not exist
            var existingPrevClasses = await _context.Classes.Where(c => c.AcademicYear == previousYearCode).ToListAsync();
            if (!existingPrevClasses.Any() && classes.Any())
            {
                var templateClasses = classes.GroupBy(c => new { c.Name, c.GradeLevel, c.Section }).Select(g => g.First()).Take(10).ToList();
                foreach (var t in templateClasses)
                {
                    _context.Classes.Add(new Class
                    {
                        Id = Guid.NewGuid(),
                        Name = t.Name,
                        GradeLevel = t.GradeLevel,
                        Section = t.Section,
                        Capacity = t.Capacity,
                        RoomNumber = t.RoomNumber,
                        AcademicYear = previousYearCode,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        MaxCapacity = t.MaxCapacity,
                        CurrentEnrollment = 0
                    });
                    classesPrev++;
                }
                await _context.SaveChangesAsync();
            }

            var prevYearClasses = await _context.Classes.Where(c => c.AcademicYear == previousYearCode).ToListAsync();
            var currYearClasses = await _context.Classes.Where(c => c.AcademicYear == currentYearCode).ToListAsync();
            if (!currYearClasses.Any())
                currYearClasses = classes.Where(c => c.AcademicYear == currentYearCode || string.IsNullOrEmpty(c.AcademicYear)).ToList();
            if (!currYearClasses.Any())
                currYearClasses = classes.Take(10).ToList();

            // 6. Fee structures for previous year
            var feeStructuresPrev = await _context.FeeStructures.Where(f => f.AcademicYearId == previousYearEntity.Id).ToListAsync();
            if (!feeStructuresPrev.Any() && prevYearClasses.Any())
            {
                foreach (var cls in prevYearClasses)
                {
                    _context.FeeStructures.Add(new FeeStructure
                    {
                        Id = Guid.NewGuid(),
                        ClassId = cls.Id,
                        Name = $"Fee {previousYearCode} {cls.Name}",
                        Description = $"Annual fee for {cls.Name} {previousYearCode}",
                        Amount = GetBaseFeeForGrade(cls.GradeLevel),
                        FeeType = "Yearly",
                        AcademicYear = previousYearCode,
                        AcademicYearId = previousYearEntity.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
                feeStructuresPrev = await _context.FeeStructures.Where(f => f.AcademicYearId == previousYearEntity.Id).ToListAsync();
            }

            var feeStructuresCurr = await _context.FeeStructures.Where(f => f.AcademicYearId == currentYearEntity.Id).ToListAsync();
            if (!feeStructuresCurr.Any())
                feeStructuresCurr = await _context.FeeStructures.Take(10).ToListAsync();

            // 7. Class enrollments for both years
            var enrollmentsExistPrev = await _context.ClassEnrollments.AnyAsync(e => e.AcademicYear == previousYearCode);
            if (!enrollmentsExistPrev && prevYearClasses.Any())
            {
                var studentList = students.Take(Math.Min(students.Count, prevYearClasses.Count * 5)).ToList();
                foreach (var student in studentList)
                {
                    var cls = prevYearClasses[Random.Shared.Next(prevYearClasses.Count)];
                    _context.ClassEnrollments.Add(new ClassEnrollment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        ClassId = cls.Id,
                        AcademicYearId = previousYearEntity.Id,
                        AcademicYear = previousYearCode,
                        EnrollmentDate = previousYearEntity.StartDate.AddDays(Random.Shared.Next(0, 30)),
                        IsActive = true,
                        Status = "Active"
                    });
                    enrollmentsPrev++;
                }
                await _context.SaveChangesAsync();
            }

            var enrollmentsExistCurr = await _context.ClassEnrollments.AnyAsync(e => e.AcademicYear == currentYearCode);
            if (!enrollmentsExistCurr && currYearClasses.Any())
            {
                var studentList = students.Take(Math.Min(students.Count, currYearClasses.Count * 5)).ToList();
                foreach (var student in studentList)
                {
                    var cls = currYearClasses[Random.Shared.Next(currYearClasses.Count)];
                    _context.ClassEnrollments.Add(new ClassEnrollment
                    {
                        Id = Guid.NewGuid(),
                        StudentId = student.Id,
                        ClassId = cls.Id,
                        AcademicYearId = currentYearEntity.Id,
                        AcademicYear = currentYearCode,
                        EnrollmentDate = currentYearEntity.StartDate.AddDays(Random.Shared.Next(0, 30)),
                        IsActive = true,
                        Status = "Active"
                    });
                    enrollmentsCurr++;
                }
                await _context.SaveChangesAsync();
            }

            // 8. Student fees and payments (previous full, current half)
            var feeStructureListPrev = await _context.FeeStructures.Where(f => f.AcademicYear == previousYearCode).ToListAsync();
            var feeStructureListCurr = feeStructuresCurr;
            var studentIdsWithFeePrev = await _context.StudentFees.Where(sf => sf.AcademicYearId == previousYearEntity.Id).Select(sf => sf.StudentId).Distinct().ToListAsync();
            var studentsForFeePrev = students.Where(s => !studentIdsWithFeePrev.Contains(s.Id)).Take(20).ToList();
            foreach (var student in studentsForFeePrev)
            {
                var fs = feeStructureListPrev.FirstOrDefault(f => f.ClassId == currYearClasses.FirstOrDefault()?.Id) ?? feeStructureListPrev.FirstOrDefault();
                if (fs == null) continue;
                var total = GetBaseFeeForGrade("1");
                var paid = total; // full payment for previous year
                _context.StudentFees.Add(new StudentFee
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    FeeStructureId = fs.Id,
                    AcademicYearId = previousYearEntity.Id,
                    TotalAmount = total,
                    Amount = total,
                    PaidAmount = paid,
                    OutstandingAmount = 0,
                    DueDate = previousYearEntity.EndDate,
                    Status = "Paid",
                    CreatedAt = DateTime.UtcNow
                });
                feesPrev++;
            }

            var studentIdsWithFeeCurr = await _context.StudentFees.Where(sf => sf.AcademicYearId == currentYearEntity.Id).Select(sf => sf.StudentId).Distinct().ToListAsync();
            var studentsForFeeCurr = students.Where(s => !studentIdsWithFeeCurr.Contains(s.Id)).Take(20).ToList();
            foreach (var student in studentsForFeeCurr)
            {
                var fs = feeStructureListCurr.FirstOrDefault();
                if (fs == null) continue;
                var total = GetBaseFeeForGrade("1");
                var paid = total / 2; // half payment for current year
                _context.StudentFees.Add(new StudentFee
                {
                    Id = Guid.NewGuid(),
                    StudentId = student.Id,
                    FeeStructureId = fs.Id,
                    AcademicYearId = currentYearEntity.Id,
                    TotalAmount = total,
                    Amount = total,
                    PaidAmount = paid,
                    OutstandingAmount = total - paid,
                    DueDate = currentYearEntity.EndDate,
                    Status = "Partial",
                    CreatedAt = DateTime.UtcNow
                });
                feesCurr++;
            }
            await _context.SaveChangesAsync();

            // 9. Holidays and events for both years
            if (!await _context.Holidays.AnyAsync())
            {
                foreach (var ay in new[] { previousYearEntity, currentYearEntity })
                {
                    _context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), AcademicYearId = ay.Id, CreatedByUserId = createdByUserId, Name = "Diwali", StartDate = ay.StartDate.AddMonths(6), EndDate = ay.StartDate.AddMonths(6).AddDays(2), HolidayType = "Religious", IsSchoolWide = true, CreatedAt = DateTime.UtcNow });
                    // Narrow range: full "2 months before year end" made almost every spring day a holiday when the year ends in March.
                    _context.Holidays.Add(new Holiday { Id = Guid.NewGuid(), AcademicYearId = ay.Id, CreatedByUserId = createdByUserId, Name = "Summer Break", StartDate = ay.EndDate.AddDays(-14), EndDate = ay.EndDate, HolidayType = "Academic", IsSchoolWide = true, CreatedAt = DateTime.UtcNow });
                    holidaysCount += 2;
                }
                await _context.SaveChangesAsync();
            }

            if (!await _context.Events.AnyAsync())
            {
                foreach (var ay in new[] { previousYearEntity, currentYearEntity })
                {
                    _context.Events.Add(new Event { Id = Guid.NewGuid(), AcademicYearId = ay.Id, CreatedByUserId = createdByUserId, Title = "Annual Day", Description = "School annual function", StartDate = ay.StartDate.AddMonths(8), EndDate = ay.StartDate.AddMonths(8).AddDays(1), EventType = "Cultural", IsSchoolWide = true, IsActive = true, CreatedAt = DateTime.UtcNow });
                    eventsCount += 1;
                }
                await _context.SaveChangesAsync();
            }

            if (!await _context.Announcements.AnyAsync())
            {
                foreach (var ay in new[] { previousYearEntity, currentYearEntity })
                {
                    for (int i = 0; i < 3; i++)
                    {
                        _context.Announcements.Add(new Announcement
                        {
                            Id = Guid.NewGuid(),
                            AcademicYearId = ay.Id,
                            Title = $"Sample Announcement {ay.YearCode} {i + 1}",
                            Content = "This is sample content for every page to have data.",
                            Priority = "Normal",
                            Category = "General",
                            PublishDate = DateTime.UtcNow.AddDays(-i),
                            IsPublished = true,
                            CreatedBy = createdByUserId,
                            CreatedAt = DateTime.UtcNow
                        });
                        announcementsCount++;
                    }
                }
                await _context.SaveChangesAsync();
            }

            // 10. Class attendance + records (previous year full, current year half)
            int attendanceCount = 0, attendanceRecordsCount = 0;
            var allClassesForAttendance = (await _context.Classes.Take(5).ToListAsync());
            foreach (var cls in allClassesForAttendance)
            {
                var ay = cls.AcademicYear == currentYearCode ? currentYearEntity : previousYearEntity;
                var yearCode = cls.AcademicYear == currentYearCode ? currentYearCode : previousYearCode;
                int daysToCreate = yearCode == currentYearCode ? 15 : 30;
                for (int d = 0; d < daysToCreate; d++)
                {
                    var attDate = ay.StartDate.AddDays(d * 3);
                    if (attDate > DateTime.UtcNow.Date) break;
                    var classAtt = new ClassAttendance
                    {
                        Id = Guid.NewGuid(),
                        ClassId = cls.Id,
                        AcademicYearId = ay.Id,
                        Date = attDate,
                        CreatedById = createdByUserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ClassAttendances.Add(classAtt);
                    await _context.SaveChangesAsync();
                    attendanceCount++;
                    var enrolled = await _context.ClassEnrollments.Where(ce => ce.ClassId == cls.Id && ce.AcademicYearId == ay.Id).Select(ce => ce.StudentId).Take(10).ToListAsync();
                    foreach (var sid in enrolled)
                    {
                        _context.ClassAttendanceRecords.Add(new ClassAttendanceRecord
                        {
                            Id = Guid.NewGuid(),
                            ClassAttendanceId = classAtt.Id,
                            StudentId = sid,
                            AcademicYearId = ay.Id,
                            Status = "Present",
                            Remarks = ""
                        });
                        attendanceRecordsCount++;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // 11. Exams + ExamResult (both years)
            int examsCount = 0, examResultsCount = 0;
            if (subjects.Any() && currYearClasses.Any())
            {
                foreach (var ay in new[] { (previousYearEntity, previousYearCode), (currentYearEntity, currentYearCode) })
                {
                    var cls = currYearClasses.First();
                    var subj = subjects.First();
                    var exam = new Exam
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Term 1 Exam {ay.Item2}",
                        Description = "Sample exam",
                        ExamDate = ay.Item1.StartDate.AddMonths(4),
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(11),
                        TotalMarks = 100,
                        PassingMarks = 33,
                        ExamType = "Mid Term",
                        AcademicYearId = ay.Item1.Id,
                        AcademicYear = ay.Item2,
                        Term = "First Term",
                        ClassId = cls.Id,
                        SubjectId = subj.Id,
                        CreatedByUserId = createdByUserId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Exams.Add(exam);
                    await _context.SaveChangesAsync();
                    examsCount++;
                    var enrollees = await _context.ClassEnrollments.Where(ce => ce.ClassId == cls.Id && ce.AcademicYearId == ay.Item1.Id).Select(ce => ce.StudentId).Take(5).ToListAsync();
                    foreach (var sid in enrollees)
                    {
                        var marks = Random.Shared.Next(33, 100);
                        _context.ExamResults.Add(new ExamResult
                        {
                            Id = Guid.NewGuid(),
                            ExamId = exam.Id,
                            StudentId = sid,
                            AcademicYearId = ay.Item1.Id,
                            MarksObtained = marks,
                            Percentage = marks,
                            IsPass = marks >= 33,
                            Grade = "Pass",
                            CreatedAt = DateTime.UtcNow
                        });
                        examResultsCount++;
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // 12. StudentLeave + StaffLeave (sab data)
            int studentLeavesCount = 0, staffLeavesCount = 0;
            if (!await _context.StudentLeaves.AnyAsync() && students.Any())
            {
                foreach (var s in students.Take(5))
                {
                    _context.StudentLeaves.Add(new StudentLeave
                    {
                        Id = Guid.NewGuid(),
                        StudentId = s.Id,
                        AcademicYearId = currentYearEntity.Id,
                        StartDate = currentYearEntity.StartDate.AddMonths(1),
                        EndDate = currentYearEntity.StartDate.AddMonths(1).AddDays(2),
                        Reason = "Family function",
                        Status = "Approved",
                        CreatedAt = DateTime.UtcNow
                    });
                    studentLeavesCount++;
                }
                await _context.SaveChangesAsync();
            }
            if (!await _context.StaffLeaves.AnyAsync() && staffList.Any())
            {
                foreach (var st in staffList.Take(3))
                {
                    _context.StaffLeaves.Add(new StaffLeave
                    {
                        Id = Guid.NewGuid(),
                        StaffId = st.Id,
                        AcademicYearId = currentYearEntity.Id,
                        LeaveType = "Sick",
                        StartDate = currentYearEntity.StartDate.AddMonths(2),
                        EndDate = currentYearEntity.StartDate.AddMonths(2).AddDays(1),
                        Reason = "Medical",
                        Status = "Approved",
                        CreatedAt = DateTime.UtcNow
                    });
                    staffLeavesCount++;
                }
                await _context.SaveChangesAsync();
            }

            // 13. StudentAcademicHistory (both years)
            int academicHistoryCount = 0;
            if (!await _context.StudentAcademicHistories.AnyAsync() && students.Any())
            {
                var classIdForHistory = currYearClasses.FirstOrDefault()?.Id ?? classes.FirstOrDefault()?.Id ?? Guid.Empty;
            if (classIdForHistory != Guid.Empty)
            {
                foreach (var s in students.Take(15))
                {
                    foreach (var ay in new[] { previousYearEntity, currentYearEntity })
                    {
                        _context.StudentAcademicHistories.Add(new StudentAcademicHistory
                        {
                            Id = Guid.NewGuid(),
                            StudentId = s.Id,
                            AcademicYearId = ay.Id,
                            ClassId = classIdForHistory,
                            RollNumber = $"R{Random.Shared.Next(1, 99)}",
                            Section = "A",
                            Grade = ay.YearCode,
                            OverallPercentage = (decimal)(Random.Shared.Next(60, 95)),
                            PromotionStatus = ay.Id == currentYearEntity.Id ? "Pending" : "Promoted",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                        academicHistoryCount++;
                    }
                }
            }
                await _context.SaveChangesAsync();
            }

            return new
            {
                message = "Pure system me data seed ho gaya: students, staff, classes, fees, attendance, exams, leaves, academic history, holidays, events, announcements.",
                previousYearCode,
                currentYearCode,
                classesCreatedForPreviousYear = classesPrev,
                enrollmentsCreated = new { previous = enrollmentsPrev, current = enrollmentsCurr },
                studentFeesCreated = new { previous = feesPrev, current = feesCurr },
                holidaysCreated = holidaysCount,
                eventsCreated = eventsCount,
                announcementsCreated = announcementsCount,
                classAttendanceSessions = attendanceCount,
                classAttendanceRecords = attendanceRecordsCount,
                examsCreated = examsCount,
                examResultsCreated = examResultsCount,
                studentLeavesCreated = studentLeavesCount,
                staffLeavesCreated = staffLeavesCount,
                studentAcademicHistoriesCreated = academicHistoryCount
            };
        }

        private async Task<object> SeedVisitorEntriesAsync()
        {
            if (await _context.VisitorEntries.AnyAsync())
                return new { message = "Visitor entries already exist" };

            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var staffList = await _context.Staff.Take(2).Select(s => s.Id).ToListAsync();
            var dept = await _context.Departments.FirstOrDefaultAsync();
            var createdBy = admin?.UserId ?? Guid.Empty;

            var visitors = new[]
            {
                new VisitorEntry { Id = Guid.NewGuid(), VisitorName = "Ramesh Kumar", ContactNumber = "9876543210", Purpose = "Parent meeting", HostStaffId = staffList.FirstOrDefault(), DepartmentId = dept?.Id, TimeIn = DateTime.UtcNow.AddHours(-2), Status = "In", CheckInById = createdBy, IsActive = true, CreatedAt = DateTime.UtcNow },
                new VisitorEntry { Id = Guid.NewGuid(), VisitorName = "Priya Sharma", ContactNumber = "9123456789", Purpose = "Admission enquiry", TimeIn = DateTime.UtcNow.AddHours(-1), TimeOut = DateTime.UtcNow, Status = "Out", CheckInById = createdBy, IsActive = true, CreatedAt = DateTime.UtcNow },
                new VisitorEntry { Id = Guid.NewGuid(), VisitorName = "Amit Singh", ContactNumber = "9988776655", Purpose = "Document submission", HostStaffId = staffList.FirstOrDefault(), TimeIn = DateTime.UtcNow, Status = "In", CheckInById = createdBy, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.VisitorEntries.AddRange(visitors);
            await _context.SaveChangesAsync();
            return new { message = "Visitor entries created", count = visitors.Length };
        }

        private async Task<object> SeedTimetableAsync()
        {
            if (await _context.TimetableEntries.AnyAsync())
                return new { message = "Timetable entries already exist" };

            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (admin?.User == null) throw new InvalidOperationException("Run step-4-admin first.");
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var periods = await _context.Periods.Where(p => p.PeriodType == "Regular").OrderBy(p => p.OrderIndex).Take(5).ToListAsync();
            var classes = await _context.Classes.Take(3).ToListAsync();
            var subjects = await _context.Subjects.Take(3).ToListAsync();
            var staffList = await _context.Staff.Take(3).ToListAsync();
            if (year == null || !periods.Any() || !classes.Any()) throw new InvalidOperationException("Run steps 6, 7, 8 first.");

            int count = 0;
            foreach (var period in periods)
            {
                foreach (var cls in classes)
                {
                    var subj = subjects[count % subjects.Count];
                    var teacher = staffList[count % staffList.Count];
                    var dto = new sms.api.Models.DTOs.CreateTimetableEntryDTO { PeriodId = period.Id, ClassId = cls.Id, AcademicYearId = year.Id, SubjectId = subj.Id, TeacherId = teacher.Id, IsActive = true };
                    await _timetableManager.CreateTimetableEntryAsync(dto, admin.UserId);
                    count++;
                }
            }
            return new { message = "Timetable entries created", count };
        }

        private async Task<object> SeedHomeworkAsync()
        {
            if (await _context.HomeworkAssignments.AnyAsync())
                return new { message = "Homework assignments already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var classes = await _context.Classes.Take(2).ToListAsync();
            var subjects = await _context.Subjects.Take(2).ToListAsync();
            var staffList = await _context.Staff.Take(1).ToListAsync();
            if (year == null || !classes.Any()) throw new InvalidOperationException("Run steps 6, 8 first.");

            var list = new List<HomeworkAssignment>();
            for (int i = 1; i <= 4; i++)
            {
                var cls = classes[i % classes.Count];
                var subj = subjects[i % subjects.Count];
                list.Add(new HomeworkAssignment
                {
                    Id = Guid.NewGuid(),
                    AcademicYearId = year.Id,
                    ClassId = cls.Id,
                    SubjectId = subj.Id,
                    Title = $"Homework {i}: {subj.Name}",
                    Description = "Complete exercises from chapter " + i,
                    IssuedDate = DateTime.UtcNow.AddDays(-i),
                    DueDate = DateTime.UtcNow.AddDays(2),
                    AssignedById = staffList.FirstOrDefault()?.Id,
                    Status = "Published",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.HomeworkAssignments.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Homework assignments created", count = list.Count };
        }

        private async Task<object> SeedLectureTopicsAsync()
        {
            if (await _context.LectureTopics.AnyAsync())
                return new { message = "Lecture topics already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var classes = await _context.Classes.Take(2).ToListAsync();
            var subjects = await _context.Subjects.Take(2).ToListAsync();
            if (year == null || !classes.Any()) throw new InvalidOperationException("Run steps 6, 8 first.");

            var list = new List<LectureTopic>();
            for (int i = 1; i <= 5; i++)
            {
                var cls = classes[i % classes.Count];
                var subj = subjects[i % subjects.Count];
                list.Add(new LectureTopic
                {
                    Id = Guid.NewGuid(),
                    AcademicYearId = year.Id,
                    ClassId = cls.Id,
                    SubjectId = subj.Id,
                    LectureDate = DateTime.UtcNow.Date.AddDays(-i),
                    Topic = $"Unit {i}: Introduction",
                    ChapterOrUnit = $"Chapter {i}",
                    Details = "Covered in class.",
                    Status = "Completed",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.LectureTopics.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Lecture topics created", count = list.Count };
        }

        private async Task<object> SeedTransferCertificatesAsync()
        {
            if (await _context.TransferCertificates.AnyAsync())
                return new { message = "Transfer certificates already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            if (year == null || admin?.User == null) throw new InvalidOperationException("Run steps 4, 6 first.");

            var tc = new TransferCertificate
            {
                Id = Guid.NewGuid(),
                AcademicYearId = year.Id,
                StudentId = student?.Id,
                TcNumber = "TC-" + DateTime.UtcNow.Year + "-001",
                IssueDate = DateTime.UtcNow.Date,
                Reason = "Transfer to another city",
                Status = "Approved",
                GeneratedByUserId = admin.UserId,
                ApprovedByUserId = admin.UserId,
                ApprovedAt = DateTime.UtcNow,
                LastClassStudied = "Class 5",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.TransferCertificates.Add(tc);
            await _context.SaveChangesAsync();
            return new { message = "Transfer certificate(s) created", count = 1 };
        }

        private async Task<object> SeedAchievementsTalentsAsync()
        {
            if (await _context.AchievementTalents.AnyAsync())
                return new { message = "Achievements/Talents already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var students = await _context.Students.Take(3).Select(s => s.Id).ToListAsync();
            if (year == null) throw new InvalidOperationException("Run step-6-academic-years first.");

            var list = new List<AchievementTalent>
            {
                new AchievementTalent { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = students.FirstOrDefault(), Title = "Math Olympiad Bronze", Category = "Academic", Level = "School", AchievementDate = DateTime.UtcNow.AddMonths(-2), Type = "Achievement", IsActive = true },
                new AchievementTalent { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = students.Skip(1).FirstOrDefault(), Title = "Music Competition First", Category = "Arts", Level = "District", AchievementDate = DateTime.UtcNow.AddMonths(-1), Type = "Talent", IsActive = true }
            };
            _context.AchievementTalents.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Achievements/Talents created", count = list.Count };
        }

        private async Task<object> SeedCoCurricularAsync()
        {
            if (await _context.CoCurricularActivities.AnyAsync())
                return new { message = "Co-curricular activities already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            if (year == null) throw new InvalidOperationException("Run step-6-academic-years first.");

            var list = new List<CoCurricularActivity>
            {
                new CoCurricularActivity { Id = Guid.NewGuid(), AcademicYearId = year.Id, Name = "Annual Sports Day", Category = "Sports", ActivityDate = DateTime.UtcNow.AddMonths(1), Venue = "School Ground", Status = "Planned", IsActive = true, CreatedAt = DateTime.UtcNow },
                new CoCurricularActivity { Id = Guid.NewGuid(), AcademicYearId = year.Id, Name = "Science Exhibition", Category = "Science", ActivityDate = DateTime.UtcNow.AddMonths(2), Venue = "Main Hall", Status = "Planned", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.CoCurricularActivities.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Co-curricular activities created", count = list.Count };
        }

        private async Task<object> SeedPtmFeedbackAsync()
        {
            if (await _context.PtmFeedbacks.AnyAsync())
                return new { message = "PTM feedback already exist" };

            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var staffList = await _context.Staff.Take(2).ToListAsync();
            var students = await _context.Students.Take(2).ToListAsync();
            if (year == null || !staffList.Any()) throw new InvalidOperationException("Run steps 6, 11 first.");

            var list = new List<PtmFeedback>();
            for (int i = 0; i < 3; i++)
            {
                list.Add(new PtmFeedback
                {
                    Id = Guid.NewGuid(),
                    AcademicYearId = year.Id,
                    TeacherStaffId = staffList[i % staffList.Count].Id,
                    StudentId = students.ElementAtOrDefault(i % students.Count)?.Id,
                    FeedbackDate = DateTime.UtcNow.AddDays(-i),
                    Rating = 4 + (i % 2),
                    Comments = "Good progress.",
                    MeetingTopic = "Term 1 Progress",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            _context.PtmFeedbacks.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "PTM feedback created", count = list.Count };
        }

        private async Task<object> SeedChatMessagesAsync()
        {
            if (await _context.ChatMessages.AnyAsync())
                return new { message = "Chat messages already exist" };

            var users = await _context.Users.Where(u => u.IsActive).Take(3).Select(u => u.Id).ToListAsync();
            if (users.Count < 2) throw new InvalidOperationException("Need at least 2 users (run steps 4, 11, 12).");

            var list = new List<ChatMessage>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    FromUserId = users[i % users.Count],
                    ToUserId = users[(i + 1) % users.Count],
                    Content = "Sample message " + (i + 1) + " for Chat page.",
                    SentAt = DateTime.UtcNow.AddMinutes(-i * 10),
                    DeliveredAt = DateTime.UtcNow.AddMinutes(-i * 10),
                    ReadAt = i < 3 ? DateTime.UtcNow : null
                });
            }
            _context.ChatMessages.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Chat messages created", count = list.Count };
        }

        private async Task<object> SeedEnquiriesAsync()
        {
            if (await _context.Enquiries.AnyAsync()) return new { message = "Enquiries already exist" };
            var list = new List<Enquiry>
            {
                new Enquiry { Id = Guid.NewGuid(), Name = "Rahul Verma", ParentName = "Suresh Verma", ContactNumber = "9876543210", Email = "rahul.v@email.com", SeekingClass = "Class 1", Status = "Open", EnquiryDate = DateTime.UtcNow.AddDays(-2), IsActive = true, CreatedAt = DateTime.UtcNow },
                new Enquiry { Id = Guid.NewGuid(), Name = "Neha Gupta", ContactNumber = "9123456789", CurrentSchool = "ABC School", SeekingClass = "Class 3", Priority = "Hot", Status = "Open", EnquiryDate = DateTime.UtcNow.AddDays(-1), IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.Enquiries.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Enquiries created", count = list.Count };
        }

        private async Task<object> SeedWeekOffsAsync()
        {
            if (await _context.WeekOffs.AnyAsync()) return new { message = "Week offs already exist" };
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            if (admin?.User == null || year == null) throw new InvalidOperationException("Run steps 4, 6 first.");
            var list = new List<WeekOff>
            {
                new WeekOff { Id = Guid.NewGuid(), Name = "Sunday", Description = "Weekly off", DayOfWeek = 0, IsSchoolWide = true, AcademicYearId = year.Id, EffectiveFrom = year.StartDate, CreatedByUserId = admin.UserId, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.WeekOffs.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Week offs created", count = list.Count };
        }

        private async Task<object> SeedDocumentPrintJobsAsync()
        {
            if (await _context.DocumentPrintJobs.AnyAsync()) return new { message = "Document print jobs already exist" };
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (admin?.User == null) throw new InvalidOperationException("Run step 4 first.");
            var list = new List<DocumentPrintJob>
            {
                new DocumentPrintJob { Id = Guid.NewGuid(), DocumentType = "Report Card", TemplateName = "Term1", JobPriority = "Normal", Status = "Queued", RequestedByUserId = admin.UserId, RequestedAt = DateTime.UtcNow, IsDuplex = false },
                new DocumentPrintJob { Id = Guid.NewGuid(), DocumentType = "ID Card", TemplateName = "StudentCard", Status = "Completed", RequestedByUserId = admin.UserId, RequestedAt = DateTime.UtcNow.AddHours(-1), PrintedAt = DateTime.UtcNow }
            };
            _context.DocumentPrintJobs.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Document print jobs created", count = list.Count };
        }

        private async Task<object> SeedWorkAllocationsAsync()
        {
            if (await _context.WorkAllocations.AnyAsync()) return new { message = "Work allocations already exist" };
            var staff = await _context.Staff.Take(2).ToListAsync();
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (!staff.Any() || admin?.User == null) throw new InvalidOperationException("Run steps 4, 11 first.");
            var list = new List<WorkAllocation>
            {
                new WorkAllocation { Id = Guid.NewGuid(), Title = "Annual Report Compilation", Description = "Compile year-end report", AssignedToStaffId = staff[0].Id, AssignedByUserId = admin.UserId, EstimatedHours = 8, ActualHours = 0, DueDate = DateTime.UtcNow.AddDays(7), Status = "Open", Priority = "High", IsActive = true },
                new WorkAllocation { Id = Guid.NewGuid(), Title = "Parent Meeting Prep", Description = "Prepare slides for PTM", AssignedToStaffId = staff.Count > 1 ? staff[1].Id : staff[0].Id, AssignedByUserId = admin.UserId, EstimatedHours = 4, ActualHours = 2, CompletionPercentage = 50, Status = "In Progress", IsActive = true }
            };
            _context.WorkAllocations.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Work allocations created", count = list.Count };
        }

        private async Task<object> SeedStudentIdCardRequestsAsync()
        {
            if (await _context.StudentIdCardRequests.AnyAsync()) return new { message = "Student ID card requests already exist" };
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (year == null || admin?.User == null) throw new InvalidOperationException("Run steps 4, 6 first.");
            var req = new StudentIdCardRequest { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = student?.Id, CardTech = "Standard", RequestDate = DateTime.UtcNow.Date, Status = "Pending", RequestedByUserId = admin.UserId, IsActive = true };
            _context.StudentIdCardRequests.Add(req);
            await _context.SaveChangesAsync();
            return new { message = "Student ID card request(s) created", count = 1 };
        }

        private async Task<object> SeedDisciplineIncidentsAsync()
        {
            if (await _context.DisciplineIncidents.AnyAsync()) return new { message = "Discipline incidents already exist" };
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            if (year == null) throw new InvalidOperationException("Run step 6 first.");
            var list = new List<DisciplineIncident>
            {
                new DisciplineIncident { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = student?.Id, IncidentDate = DateTime.UtcNow.AddDays(-5), IncidentType = "Late to class", Severity = "Low", Description = "Repeated late arrival.", ActionTaken = "Warning issued", Status = "Closed", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.DisciplineIncidents.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Discipline incidents created", count = list.Count };
        }

        private async Task<object> SeedStudentBehaviorIncidentsAsync()
        {
            if (await _context.StudentBehaviorIncidents.AnyAsync()) return new { message = "Student behavior incidents already exist" };
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            var cls = await _context.Classes.FirstOrDefaultAsync();
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (year == null || admin?.User == null) throw new InvalidOperationException("Run steps 4, 6 first.");
            var list = new List<StudentBehaviorIncident>
            {
                new StudentBehaviorIncident { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = student?.Id, ClassId = cls?.Id, IncidentDate = DateTime.UtcNow.AddDays(-3), BehaviorType = "Disruption", SeverityIndex = 1, Description = "Minor disruption in class.", Status = "Open", ReportedByUserId = admin.UserId, IsActive = true }
            };
            _context.StudentBehaviorIncidents.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Student behavior incidents created", count = list.Count };
        }

        private async Task<object> SeedScholarshipsAsync()
        {
            if (await _context.Scholarships.AnyAsync()) return new { message = "Scholarships already exist" };
            var list = new List<Scholarship>
            {
                new Scholarship { Id = Guid.NewGuid(), Name = "Merit Scholarship", Provider = "School Trust", Amount = 5000, GrantType = "Merit", MinimumGpaRequirement = 8.5m, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Scholarship { Id = Guid.NewGuid(), Name = "Need-Based Aid", Provider = "Education Fund", Amount = 3000, GrantType = "Need-Based", MinimumGpaRequirement = 7.0m, IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.Scholarships.AddRange(list);
            await _context.SaveChangesAsync();
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            if (year != null && student != null && admin?.User != null)
            {
                var app = new ScholarshipApplication { Id = Guid.NewGuid(), AcademicYearId = year.Id, ScholarshipId = list[0].Id, StudentId = student.Id, AppliedOn = DateTime.UtcNow, Status = "Pending", IsActive = true };
                _context.ScholarshipApplications.Add(app);
                await _context.SaveChangesAsync();
            }
            return new { message = "Scholarships and application(s) created", count = list.Count };
        }

        private async Task<object> SeedHostelsAsync()
        {
            if (await _context.Hostels.AnyAsync()) return new { message = "Hostels already exist" };
            var staff = await _context.Staff.FirstOrDefaultAsync();
            var hostel = new Hostel { Id = Guid.NewGuid(), Name = "Boys Hostel", BuildingName = "Block A", WardenId = staff?.Id, HostelType = "Boys", ContactNumber = "9876543210", IsActive = true, CreatedAt = DateTime.UtcNow };
            _context.Hostels.Add(hostel);
            await _context.SaveChangesAsync();
            var rooms = new List<HostelRoom>
            {
                new HostelRoom { Id = Guid.NewGuid(), HostelId = hostel.Id, RoomNumber = "101", Floor = 1, Capacity = 2, IsAc = true, MonthlyRent = 3000, Status = "Available", IsActive = true, CreatedAt = DateTime.UtcNow },
                new HostelRoom { Id = Guid.NewGuid(), HostelId = hostel.Id, RoomNumber = "102", Floor = 1, Capacity = 2, IsAc = false, MonthlyRent = 2500, Status = "Available", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.HostelRooms.AddRange(rooms);
            await _context.SaveChangesAsync();
            return new { message = "Hostels and rooms created", count = 1 + rooms.Count };
        }

        private async Task<object> SeedAssetsAsync()
        {
            if (await _context.Assets.AnyAsync()) return new { message = "Assets already exist" };
            var list = new List<Asset>
            {
                new Asset { Id = Guid.NewGuid(), Name = "Projector - Room 101", Category = "AV Equipment", SerialNumber = "PRJ-001", PurchaseDate = DateTime.UtcNow.AddYears(-1), Status = "Active", Location = "Room 101", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Asset { Id = Guid.NewGuid(), Name = "Laptop - Admin", Category = "IT", SerialNumber = "LAP-001", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.Assets.AddRange(list);
            await _context.SaveChangesAsync();
            var rec = new AssetMaintenanceRecord { Id = Guid.NewGuid(), AssetId = list[0].Id, ServiceDate = DateTime.UtcNow.AddMonths(-2), NextDueDate = DateTime.UtcNow.AddMonths(4), Vendor = "Tech Services", Status = "Completed", PostServiceHealthGrade = 5, IsActive = true };
            _context.AssetMaintenanceRecords.Add(rec);
            await _context.SaveChangesAsync();
            return new { message = "Assets and maintenance record(s) created", count = list.Count + 1 };
        }

        private async Task<object> SeedInventoryAsync()
        {
            if (await _context.InventoryItems.AnyAsync()) return new { message = "Inventory items already exist" };
            var list = new List<InventoryItem>
            {
                new InventoryItem { Id = Guid.NewGuid(), Name = "Whiteboard Markers", Category = "Stationery", Sku = "STM-001", Quantity = 50, CriticalThreshold = 10, Unit = "pcs", UnitCost = 20, IsActive = true },
                new InventoryItem { Id = Guid.NewGuid(), Name = "A4 Paper Ream", Category = "Stationery", Sku = "PAP-001", Quantity = 100, CriticalThreshold = 20, Unit = "ream", UnitCost = 250, IsActive = true }
            };
            _context.InventoryItems.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Inventory items created", count = list.Count };
        }

        private async Task<object> SeedGalleryMediaAsync()
        {
            if (await _context.GalleryMedia.AnyAsync()) return new { message = "Gallery media already exist" };
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var list = new List<GalleryMedia>
            {
                new GalleryMedia { Id = Guid.NewGuid(), Title = "Annual Day 2024", AlbumCategory = "Events", MediaType = "Photo", Url = "/uploads/sample1.jpg", EventDate = DateTime.UtcNow.AddMonths(-1), UploadedByUserId = admin?.UserId, IsPublic = true, IsActive = true, MediaDensity = 1, TotalAssetSize = 0, CreatedAt = DateTime.UtcNow },
                new GalleryMedia { Id = Guid.NewGuid(), Title = "Sports Meet", AlbumCategory = "Sports", MediaType = "Photo", EventDate = DateTime.UtcNow.AddMonths(-2), IsActive = true, MediaDensity = 1, TotalAssetSize = 0, CreatedAt = DateTime.UtcNow }
            };
            _context.GalleryMedia.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Gallery media created", count = list.Count };
        }

        private async Task<object> SeedScheduledServicesAsync()
        {
            if (await _context.ScheduledServices.AnyAsync()) return new { message = "Scheduled services already exist" };
            var staff = await _context.Staff.FirstOrDefaultAsync();
            var list = new List<ScheduledService>
            {
                new ScheduledService { Id = Guid.NewGuid(), Title = "Fire Safety Check", Category = "Safety", AssignedToStaffId = staff?.Id, ScheduledFor = DateTime.UtcNow.AddDays(7), Frequency = "Monthly", Status = "Scheduled", IsActive = true, CreatedAt = DateTime.UtcNow },
                new ScheduledService { Id = Guid.NewGuid(), Title = "HVAC Maintenance", Category = "Maintenance", ScheduledFor = DateTime.UtcNow.AddDays(14), Frequency = "Once", Status = "Scheduled", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.ScheduledServices.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Scheduled services created", count = list.Count };
        }

        private async Task<object> SeedInviteesAsync()
        {
            if (await _context.Invitees.AnyAsync()) return new { message = "Invitees already exist" };
            var evt = await _context.Events.FirstOrDefaultAsync();
            var list = new List<Invitee>
            {
                new Invitee { Id = Guid.NewGuid(), EventId = evt?.Id, Name = "Parent Representative", Email = "parent.rep@email.com", ContactNumber = "9876500000", Role = "Parent", RsvpStatus = "Confirmed", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Invitee { Id = Guid.NewGuid(), EventId = evt?.Id, Name = "Chief Guest", Email = "guest@email.com", Role = "Guest", RsvpStatus = "Pending", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            _context.Invitees.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Invitees created", count = list.Count };
        }

        private async Task<object> SeedBusManagementAsync()
        {
            if (await _context.Buses.AnyAsync()) return new { message = "Buses already exist" };
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var bus = new sms.api.Models.Bus { Id = Guid.NewGuid(), BusNumber = "BUS-001", BusName = "School Bus A", RegistrationNumber = "DL01AB1234", Capacity = 40, DriverName = "Driver Kumar", FuelType = "Diesel", IsActive = true };
            _context.Buses.Add(bus);
            await _context.SaveChangesAsync();
            var route = new BusRoute { Id = Guid.NewGuid(), RouteName = "Route 1 - North Zone", RouteCode = "R1", BusId = bus.Id, AcademicYearId = year?.Id, TotalDistance = 15, CreatedByUserId = admin?.UserId ?? Guid.Empty };
            _context.BusRoutes.Add(route);
            await _context.SaveChangesAsync();
            var stops = new List<BusStop>
            {
                new BusStop { Id = Guid.NewGuid(), StopName = "City Center", StopCode = "STOP-01", RouteId = route.Id, StopOrder = 1, Latitude = "28.61", Longitude = "77.20", DistanceFromSchool = 5, MonthlyFee = 500 },
                new BusStop { Id = Guid.NewGuid(), StopName = "Green Park", StopCode = "STOP-02", RouteId = route.Id, StopOrder = 2, Latitude = "28.55", Longitude = "77.21", DistanceFromSchool = 3, MonthlyFee = 400 }
            };
            _context.BusStops.AddRange(stops);
            await _context.SaveChangesAsync();
            return new { message = "Bus, route and stops created", count = 1 + 1 + stops.Count };
        }

        private async Task<object> SeedLibraryAsync()
        {
            if (await _context.Books.AnyAsync()) return new { message = "Library data already exist" };
            var admin = await _context.Admins.Include(a => a.User).OrderBy(a => a.Id).FirstOrDefaultAsync();
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            if (admin?.User == null || year == null) throw new InvalidOperationException("Run steps 4, 6 first.");
            var cat = new BookCategory { Id = Guid.NewGuid(), Name = "Fiction", Code = "FIC", Description = "Fiction books", CreatedByUserId = admin.UserId, IsActive = true, CreatedAt = DateTime.UtcNow };
            var pub = new Publisher { Id = Guid.NewGuid(), Name = "School Publications", IsActive = true, CreatedByUserId = admin.UserId, CreatedAt = DateTime.UtcNow };
            _context.BookCategories.Add(cat);
            _context.Publishers.Add(pub);
            await _context.SaveChangesAsync();
            var book = new Book { Id = Guid.NewGuid(), Title = "Sample Textbook", ISBN = "978-0-00-000000-0", AccessionNumber = "ACC-001", CategoryId = cat.Id, PublisherId = pub.Id, Language = "English", IsAvailable = true };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            var user = await _context.Users.Where(u => u.IsActive).FirstOrDefaultAsync();
            if (user != null)
            {
                var member = new LibraryMember { Id = Guid.NewGuid(), UserId = user.Id, AcademicYearId = year.Id, MembershipNumber = "LM-" + DateTime.UtcNow.Year + "-001", MemberType = "Student", MembershipStartDate = year.StartDate, MaxBooksAllowed = 3, MaxIssueDays = 14, IsActive = true };
                _context.LibraryMembers.Add(member);
                await _context.SaveChangesAsync();
            }
            return new { message = "Library categories, publisher, book and member created", count = 4 };
        }

        private async Task<object> SeedOnlinePaymentsAsync()
        {
            if (await _context.OnlinePaymentTransactions.AnyAsync()) return new { message = "Online payment transactions already exist" };
            var year = await _context.AcademicYears.OrderBy(ay => ay.StartDate).FirstOrDefaultAsync();
            var student = await _context.Students.FirstOrDefaultAsync();
            if (year == null) throw new InvalidOperationException("Run step 6 first.");
            var list = new List<OnlinePaymentTransaction>
            {
                new OnlinePaymentTransaction { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = student?.Id, Amount = 2500, ConvenienceFees = 0, TaxBreakdown = 0, Currency = "INR", PaymentFor = "Fee", Gateway = "Razorpay", Status = "Success", PaidAt = DateTime.UtcNow.AddDays(-5), ReceiptNumber = "RCP-001" },
                new OnlinePaymentTransaction { Id = Guid.NewGuid(), AcademicYearId = year.Id, StudentId = student?.Id, Amount = 5000, ConvenienceFees = 50, TaxBreakdown = 0, Currency = "INR", PaymentFor = "Fee", Status = "Initiated", ReceiptNumber = "RCP-002" }
            };
            _context.OnlinePaymentTransactions.AddRange(list);
            await _context.SaveChangesAsync();
            return new { message = "Online payment transactions created", count = list.Count };
        }

        // Step 4B: Initialize demo users for each system role (RBAC)
        private async Task<object> InitializeRoleUsers()
        {
            var sampleUsers = new List<(string RoleName, string FirstName, string LastName, string Email, string DefaultPassword)>
            {
                ("Admin", "System", "Admin", "admin@excellenceacademy.edu", "Admin@123"),
                ("Principal", "Dr.", "Principal", "principal@excellenceacademy.edu", "Principal@123"),
                ("Teacher", "James", "Teacher1", "james.teacher@excellenceacademy.edu", "Teacher@123"),
                ("Teacher", "Sarah", "Teacher2", "sarah.teacher@excellenceacademy.edu", "Teacher@123"),
                ("Teacher", "Robert", "Teacher3", "robert.teacher@excellenceacademy.edu", "Teacher@123"),
                ("HR", "Maria", "HR", "maria.hr@excellenceacademy.edu", "HR@123"),
                ("Accountant", "David", "Accountant", "david.accountant@excellenceacademy.edu", "Accountant@123"),
                ("Receptionist", "Lisa", "Receptionist", "lisa.receptionist@excellenceacademy.edu", "Receptionist@123"),
                ("Transport Manager", "Mike", "Transport", "mike.transport@excellenceacademy.edu", "TransportManager@123"),
                ("Librarian", "Amanda", "Librarian", "amanda.librarian@excellenceacademy.edu", "Librarian@123"),
                ("Driver", "John", "Driver1", "john.driver@excellenceacademy.edu", "Driver@123"),
                ("Driver", "George", "Driver2", "george.driver@excellenceacademy.edu", "Driver@123"),
                ("Parent", "Robert", "Parent1", "robert.parent@excellenceacademy.edu", "Parent@123"),
                ("Parent", "Jennifer", "Parent2", "jennifer.parent@excellenceacademy.edu", "Parent@123"),
                ("Staff", "David", "Wilson", "david.wilson@excellenceacademy.edu", "Staff@123")
            };

            var createdUsers = new List<object>();

            foreach (var (roleName, firstName, lastName, email, defaultPassword) in sampleUsers)
            {
                // Skip if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == email))
                {
                    createdUsers.Add(new { role = roleName, email, status = "Already exists" });
                    continue;
                }

                // Create user
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Username = $"{firstName.ToLower()}.{lastName.ToLower()}",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                    Role = roleName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // Assign user to system role via UserRoles table
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role != null)
                {
                    var userRole = new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = newUser.Id,
                        RoleId = role.Id,
                        IsActive = true,
                        AssignedAt = DateTime.UtcNow,
                        AssignedByUserId = newUser.Id
                    };
                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                createdUsers.Add(new
                {
                    role = roleName,
                    username = newUser.Username,
                    email,
                    password = defaultPassword,
                    status = "Created"
                });
            }

            return new
            {
                message = "Role-based demo users initialized successfully",
                totalProcessed = sampleUsers.Count,
                createdCount = createdUsers.Count(u => u.GetType().GetProperty("status")?.GetValue(u)?.ToString() == "Created"),
                details = createdUsers
            };
        }
    }
}
