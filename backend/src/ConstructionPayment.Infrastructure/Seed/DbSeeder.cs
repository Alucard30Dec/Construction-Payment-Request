using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Domain.Entities;
using ConstructionPayment.Domain.Enums;
using ConstructionPayment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ConstructionPayment.Infrastructure.Seed;

public static class DbSeeder
{
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid EmployeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ManagerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid DirectorId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AccountantId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid ViewerId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly Guid AdminRoleProfileId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
    private static readonly Guid EmployeeRoleProfileId = Guid.Parse("aaaa2222-2222-2222-2222-222222222222");
    private static readonly Guid DepartmentManagerRoleProfileId = Guid.Parse("aaaa3333-3333-3333-3333-333333333333");
    private static readonly Guid DirectorRoleProfileId = Guid.Parse("aaaa4444-4444-4444-4444-444444444444");
    private static readonly Guid AccountantRoleProfileId = Guid.Parse("aaaa5555-5555-5555-5555-555555555555");
    private static readonly Guid ViewerRoleProfileId = Guid.Parse("aaaa6666-6666-6666-6666-666666666666");

    private static readonly Guid ProjectAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid ProjectBId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
    private static readonly Guid ProjectCId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");

    private static readonly Guid Supplier1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
    private static readonly Guid Supplier2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
    private static readonly Guid Supplier3Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3");
    private static readonly Guid Supplier4Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4");
    private static readonly Guid Supplier5Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5");

    private static readonly Guid Contract1Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
    private static readonly Guid Contract2Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
    private static readonly Guid Contract3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");
    private static readonly Guid Contract4Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc4");
    private static readonly Guid Contract5Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc5");

    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHasherService passwordHasher, CancellationToken cancellationToken = default)
    {
        await SeedRoleProfilesAsync(dbContext, cancellationToken);
        await SeedUsersAsync(dbContext, passwordHasher, cancellationToken);
        await SeedSuppliersAsync(dbContext, cancellationToken);
        await SeedProjectsAsync(dbContext, cancellationToken);
        await SeedContractsAsync(dbContext, cancellationToken);
        await SeedApprovalMatricesAsync(dbContext, cancellationToken);
        await SeedPaymentRequestsAsync(dbContext, cancellationToken);
    }

    private static async Task SeedRoleProfilesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var templates = new[]
        {
            new { Id = AdminRoleProfileId, Code = UserRole.Admin.ToString(), Name = "Admin", Description = "Toan quyen he thong", Role = UserRole.Admin },
            new { Id = EmployeeRoleProfileId, Code = UserRole.Employee.ToString(), Name = "Employee", Description = "Nhan vien lap ho so thanh toan", Role = UserRole.Employee },
            new { Id = DepartmentManagerRoleProfileId, Code = UserRole.DepartmentManager.ToString(), Name = "Department Manager", Description = "Truong bo phan duyet cap 1", Role = UserRole.DepartmentManager },
            new { Id = DirectorRoleProfileId, Code = UserRole.Director.ToString(), Name = "Director", Description = "Giam doc duyet cap 2", Role = UserRole.Director },
            new { Id = AccountantRoleProfileId, Code = UserRole.Accountant.ToString(), Name = "Accountant", Description = "Ke toan xac nhan thanh toan", Role = UserRole.Accountant },
            new { Id = ViewerRoleProfileId, Code = UserRole.Viewer.ToString(), Name = "Viewer", Description = "Chi xem du lieu", Role = UserRole.Viewer }
        };

        var utcNow = DateTime.UtcNow;
        var existingProfiles = await dbContext.RoleProfiles
            .Include(x => x.PermissionGrants)
            .ToListAsync(cancellationToken);

        var profileByCode = existingProfiles.ToDictionary(
            x => x.Code,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        foreach (var template in templates)
        {
            if (!profileByCode.TryGetValue(template.Code, out var profile))
            {
                profile = new RoleProfile
                {
                    Id = template.Id,
                    Code = template.Code,
                    Name = template.Name,
                    Description = template.Description,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow
                };

                dbContext.RoleProfiles.Add(profile);
                profileByCode[template.Code] = profile;
            }
            else
            {
                profile.Code = template.Code;
                profile.Name = template.Name;
                profile.Description = template.Description;
                profile.IsSystem = true;
                profile.IsActive = true;
                profile.UpdatedAt = utcNow;
                if (profile.CreatedAt == default)
                {
                    profile.CreatedAt = utcNow;
                }
            }

            var allowedCodes = RolePermissionDefaults.Get(template.Role)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var grantsByCode = profile.PermissionGrants.ToDictionary(
                x => x.PermissionCode,
                x => x,
                StringComparer.OrdinalIgnoreCase);

            foreach (var permission in PermissionCatalog.All)
            {
                var isAllowed = allowedCodes.Contains(permission.Code);
                if (!grantsByCode.TryGetValue(permission.Code, out var grant))
                {
                    profile.PermissionGrants.Add(new RolePermissionGrant
                    {
                        RoleProfileId = profile.Id,
                        PermissionCode = permission.Code,
                        IsAllowed = isAllowed,
                        CreatedAt = utcNow,
                        UpdatedAt = utcNow
                    });
                    continue;
                }

                if (grant.IsAllowed != isAllowed)
                {
                    grant.IsAllowed = isAllowed;
                    grant.UpdatedAt = utcNow;
                }
            }

            var staleGrants = profile.PermissionGrants
                .Where(x => !PermissionCatalog.CodeSet.Contains(x.PermissionCode))
                .ToArray();
            if (staleGrants.Length > 0)
            {
                dbContext.RolePermissionGrants.RemoveRange(staleGrants);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedUsersAsync(AppDbContext dbContext, IPasswordHasherService passwordHasher, CancellationToken cancellationToken)
    {
        var roleProfileIdByCode = await dbContext.RoleProfiles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.Code.ToLower(), x => x.Id, cancellationToken);

        var users = new List<AppUser>
        {
            new()
            {
                Id = AdminId,
                Username = "admin",
                FullName = "System Admin",
                Email = "admin@construction.local",
                Role = UserRole.Admin,
                RoleProfileId = ResolveRoleProfileId(UserRole.Admin, roleProfileIdByCode),
                Department = "IT",
                IsActive = true
            },
            new()
            {
                Id = EmployeeId,
                Username = "employee",
                FullName = "Nguyen Van Employee",
                Email = "employee@construction.local",
                Role = UserRole.Employee,
                RoleProfileId = ResolveRoleProfileId(UserRole.Employee, roleProfileIdByCode),
                Department = "Xay dung",
                IsActive = true
            },
            new()
            {
                Id = ManagerId,
                Username = "manager",
                FullName = "Tran Thi Manager",
                Email = "manager@construction.local",
                Role = UserRole.DepartmentManager,
                RoleProfileId = ResolveRoleProfileId(UserRole.DepartmentManager, roleProfileIdByCode),
                Department = "Xay dung",
                IsActive = true
            },
            new()
            {
                Id = DirectorId,
                Username = "director",
                FullName = "Le Van Director",
                Email = "director@construction.local",
                Role = UserRole.Director,
                RoleProfileId = ResolveRoleProfileId(UserRole.Director, roleProfileIdByCode),
                Department = "Board",
                IsActive = true
            },
            new()
            {
                Id = AccountantId,
                Username = "accountant",
                FullName = "Pham Thi Accountant",
                Email = "accountant@construction.local",
                Role = UserRole.Accountant,
                RoleProfileId = ResolveRoleProfileId(UserRole.Accountant, roleProfileIdByCode),
                Department = "Ke toan",
                IsActive = true
            },
            new()
            {
                Id = ViewerId,
                Username = "viewer",
                FullName = "Viewer User",
                Email = "viewer@construction.local",
                Role = UserRole.Viewer,
                RoleProfileId = ResolveRoleProfileId(UserRole.Viewer, roleProfileIdByCode),
                Department = "Control",
                IsActive = true
            }
        };

        var passwords = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin"] = "admin123",
            ["employee"] = "employee123",
            ["manager"] = "manager123",
            ["director"] = "director123",
            ["accountant"] = "accountant123",
            ["viewer"] = "viewer123"
        };

        var utcNow = DateTime.UtcNow;

        foreach (var user in users)
        {
            var existing = await dbContext.Users
                .FirstOrDefaultAsync(x => x.Username.ToLower() == user.Username.ToLower(), cancellationToken);

            if (existing is null)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, passwords[user.Username]);
                user.CreatedAt = utcNow;
                user.UpdatedAt = utcNow;
                dbContext.Users.Add(user);
                continue;
            }

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.RoleProfileId = user.RoleProfileId;
            existing.Department = user.Department;
            existing.IsActive = true;
            existing.PasswordHash = passwordHasher.HashPassword(existing, passwords[user.Username]);
            existing.UpdatedAt = utcNow;

            if (existing.CreatedAt == default)
            {
                existing.CreatedAt = utcNow;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedSuppliersAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Suppliers.AnyAsync(cancellationToken))
        {
            return;
        }

        var suppliers = new List<Supplier>
        {
            new()
            {
                Id = Supplier1Id,
                Code = "NCC001",
                Name = "Cong ty TNHH Vat Tu Minh Phat",
                TaxCode = "0101234501",
                Address = "Ha Noi",
                ContactPerson = "Pham Minh",
                Phone = "0901000001",
                Email = "ncc1@example.com",
                BankAccountNumber = "12345678901",
                BankName = "Vietcombank",
                BankBranch = "Ha Noi",
                Notes = "Nha cung cap vat tu chinh",
                IsActive = true
            },
            new()
            {
                Id = Supplier2Id,
                Code = "NCC002",
                Name = "Cong ty Co phan Xay Lap Tien Phat",
                TaxCode = "0101234502",
                Address = "Hai Phong",
                ContactPerson = "Nguyen Hai",
                Phone = "0901000002",
                Email = "ncc2@example.com",
                BankAccountNumber = "12345678902",
                BankName = "BIDV",
                BankBranch = "Hai Phong",
                Notes = "Thi cong ket cau",
                IsActive = true
            },
            new()
            {
                Id = Supplier3Id,
                Code = "NCC003",
                Name = "Cong ty CP Co Dien MEP Star",
                TaxCode = "0101234503",
                Address = "Da Nang",
                ContactPerson = "Tran Star",
                Phone = "0901000003",
                Email = "ncc3@example.com",
                BankAccountNumber = "12345678903",
                BankName = "Techcombank",
                BankBranch = "Da Nang",
                Notes = "MEP",
                IsActive = true
            },
            new()
            {
                Id = Supplier4Id,
                Code = "NCC004",
                Name = "Cong ty Dich Vu Van Tai Hoang Gia",
                TaxCode = "0101234504",
                Address = "TP HCM",
                ContactPerson = "Le Hoang",
                Phone = "0901000004",
                Email = "ncc4@example.com",
                BankAccountNumber = "12345678904",
                BankName = "ACB",
                BankBranch = "TP HCM",
                Notes = "Dich vu van tai",
                IsActive = true
            },
            new()
            {
                Id = Supplier5Id,
                Code = "NCC005",
                Name = "Cong ty Tu Van Giam Sat Alpha",
                TaxCode = "0101234505",
                Address = "Can Tho",
                ContactPerson = "Do Alpha",
                Phone = "0901000005",
                Email = "ncc5@example.com",
                BankAccountNumber = "12345678905",
                BankName = "MB Bank",
                BankBranch = "Can Tho",
                Notes = "Tu van",
                IsActive = true
            }
        };

        foreach (var supplier in suppliers)
        {
            supplier.CreatedAt = DateTime.UtcNow;
            supplier.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.Suppliers.AddRange(suppliers);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedProjectsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Projects.AnyAsync(cancellationToken))
        {
            return;
        }

        var projects = new List<Project>
        {
            new()
            {
                Id = ProjectAId,
                Code = "DA001",
                Name = "Toa nha van phong ABC",
                Location = "Ha Noi",
                Department = "Xay dung",
                ProjectManager = "Tran Thi Manager",
                IsActive = true
            },
            new()
            {
                Id = ProjectBId,
                Code = "DA002",
                Name = "Nha may san xuat XYZ",
                Location = "Hai Phong",
                Department = "MEP",
                ProjectManager = "Le Van Director",
                IsActive = true
            },
            new()
            {
                Id = ProjectCId,
                Code = "DA003",
                Name = "Khu do thi Green Town",
                Location = "Da Nang",
                Department = "Xay dung",
                ProjectManager = "Tran Thi Manager",
                IsActive = true
            }
        };

        foreach (var project in projects)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.Projects.AddRange(projects);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedContractsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.Contracts.AnyAsync(cancellationToken))
        {
            return;
        }

        var contracts = new List<Contract>
        {
            new()
            {
                Id = Contract1Id,
                ContractNumber = "HD-2026-001",
                Name = "Hop dong cung cap vat tu phan tho",
                SupplierId = Supplier1Id,
                ProjectId = ProjectAId,
                SignedDate = DateTime.UtcNow.AddMonths(-8),
                ContractValue = 1200000000,
                ContractType = ContractType.Procurement,
                Notes = "Tien do thanh toan theo dot",
                IsActive = true
            },
            new()
            {
                Id = Contract2Id,
                ContractNumber = "HD-2026-002",
                Name = "Hop dong thi cong ket cau",
                SupplierId = Supplier2Id,
                ProjectId = ProjectAId,
                SignedDate = DateTime.UtcNow.AddMonths(-7),
                ContractValue = 2500000000,
                ContractType = ContractType.Construction,
                Notes = "Bao gom nhan cong + vat tu",
                IsActive = true
            },
            new()
            {
                Id = Contract3Id,
                ContractNumber = "HD-2026-003",
                Name = "Hop dong MEP tong the",
                SupplierId = Supplier3Id,
                ProjectId = ProjectBId,
                SignedDate = DateTime.UtcNow.AddMonths(-6),
                ContractValue = 3200000000,
                ContractType = ContractType.Service,
                Notes = "MEP stage 1",
                IsActive = true
            },
            new()
            {
                Id = Contract4Id,
                ContractNumber = "HD-2026-004",
                Name = "Hop dong van tai vat tu",
                SupplierId = Supplier4Id,
                ProjectId = ProjectCId,
                SignedDate = DateTime.UtcNow.AddMonths(-4),
                ContractValue = 450000000,
                ContractType = ContractType.Service,
                Notes = "Theo chuyen",
                IsActive = true
            },
            new()
            {
                Id = Contract5Id,
                ContractNumber = "HD-2026-005",
                Name = "Hop dong tu van giam sat",
                SupplierId = Supplier5Id,
                ProjectId = ProjectCId,
                SignedDate = DateTime.UtcNow.AddMonths(-5),
                ContractValue = 780000000,
                ContractType = ContractType.Consulting,
                Notes = "Bao cao dinh ky",
                IsActive = true
            }
        };

        foreach (var contract in contracts)
        {
            contract.CreatedAt = DateTime.UtcNow;
            contract.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.Contracts.AddRange(contracts);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedApprovalMatricesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.ApprovalMatrices.AnyAsync(cancellationToken))
        {
            return;
        }

        var items = new List<ApprovalMatrix>
        {
            new()
            {
                MinAmount = 0,
                MaxAmount = 500000000,
                RequireDirectorApproval = false,
                Department = "Xay dung",
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 500000001,
                MaxAmount = 99999999999,
                RequireDirectorApproval = true,
                Department = "Xay dung",
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 0,
                MaxAmount = 300000000,
                RequireDirectorApproval = false,
                Department = "MEP",
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 300000001,
                MaxAmount = 99999999999,
                RequireDirectorApproval = true,
                Department = "MEP",
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 0,
                MaxAmount = 99999999999,
                RequireDirectorApproval = false,
                Department = null,
                ProjectId = ProjectCId,
                IsActive = true
            }
        };

        foreach (var item in items)
        {
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;
        }

        dbContext.ApprovalMatrices.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPaymentRequestsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        if (await dbContext.PaymentRequests.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var requests = new List<PaymentRequest>
        {
            CreateRequest("PR-2026-001", "Thanh toan dot 1 vat tu phan tho", ProjectAId, Supplier1Id, Contract1Id, "VatTu", "INV-001", now.AddDays(-20), now.AddDays(10), 100000000, 10, 0, 0, 0, PaymentRequestStatus.Draft, EmployeeId),
            CreateRequest("PR-2026-002", "Thanh toan nhan cong thang 1", ProjectAId, Supplier2Id, Contract2Id, "NhanCong", "INV-002", now.AddDays(-25), now.AddDays(5), 150000000, 8, 0, 5000000, 0, PaymentRequestStatus.Draft, EmployeeId),
            CreateRequest("PR-2026-003", "Thanh toan van tai vat tu", ProjectCId, Supplier4Id, Contract4Id, "DichVu", "INV-003", now.AddDays(-30), now.AddDays(-2), 220000000, 10, 10000000, 0, 0, PaymentRequestStatus.PendingDepartmentApproval, EmployeeId),
            CreateRequest("PR-2026-004", "Thanh toan ket cau dot 2", ProjectAId, Supplier2Id, Contract2Id, "ThiCong", "INV-004", now.AddDays(-28), now.AddDays(3), 700000000, 10, 0, 0, 0, PaymentRequestStatus.PendingDirectorApproval, EmployeeId),
            CreateRequest("PR-2026-005", "Thanh toan MEP dot 1", ProjectBId, Supplier3Id, Contract3Id, "MEP", "INV-005", now.AddDays(-18), now.AddDays(7), 250000000, 10, 0, 10000000, 5000000, PaymentRequestStatus.PendingAccounting, EmployeeId),
            CreateRequest("PR-2026-006", "Thanh toan MEP dot 2", ProjectBId, Supplier3Id, Contract3Id, "MEP", "INV-006", now.AddDays(-22), now.AddDays(-1), 820000000, 10, 20000000, 0, 0, PaymentRequestStatus.PendingAccounting, EmployeeId),
            CreateRequest("PR-2026-007", "Thanh toan tu van giam sat", ProjectCId, Supplier5Id, Contract5Id, "TuVan", "INV-007", now.AddDays(-17), now.AddDays(14), 120000000, 10, 0, 0, 0, PaymentRequestStatus.Rejected, EmployeeId),
            CreateRequest("PR-2026-008", "Thanh toan van tai bo sung", ProjectCId, Supplier4Id, Contract4Id, "DichVu", "INV-008", now.AddDays(-15), now.AddDays(12), 90000000, 10, 0, 0, 2000000, PaymentRequestStatus.ReturnedForEdit, EmployeeId),
            CreateRequest("PR-2026-009", "Thanh toan vat tu hoan cong", ProjectAId, Supplier1Id, Contract1Id, "VatTu", "INV-009", now.AddDays(-40), now.AddDays(-10), 180000000, 10, 10000000, 0, 0, PaymentRequestStatus.Paid, EmployeeId),
            CreateRequest("PR-2026-010", "Thanh toan dat da cong trinh", ProjectAId, Supplier2Id, Contract2Id, "ThiCong", "INV-010", now.AddDays(-12), now.AddDays(8), 300000000, 10, 0, 0, 0, PaymentRequestStatus.PendingDepartmentApproval, EmployeeId)
        };

        foreach (var request in requests)
        {
            request.CreatedAt = now;
            request.UpdatedAt = now;

            if (request.CurrentStatus != PaymentRequestStatus.Draft && request.CurrentStatus != PaymentRequestStatus.ReturnedForEdit)
            {
                request.SubmittedAt = now.AddDays(-3);
            }

            if (request.CurrentStatus is PaymentRequestStatus.PendingAccounting or PaymentRequestStatus.Paid)
            {
                request.ApprovedAt = now.AddDays(-1);
            }

            if (request.CurrentStatus == PaymentRequestStatus.Paid)
            {
                request.PaidAt = now.AddDays(-1);
            }
        }

        dbContext.PaymentRequests.AddRange(requests);
        await dbContext.SaveChangesAsync(cancellationToken);

        var requestMap = requests.ToDictionary(x => x.RequestCode, x => x);

        var histories = new List<PaymentRequestApprovalHistory>
        {
            // PR-003
            CreateHistory(requestMap["PR-2026-003"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet cap 1"),

            // PR-004
            CreateHistory(requestMap["PR-2026-004"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-004"].Id, ManagerId, 1, ApprovalAction.Approve, "Da kiem tra"),

            // PR-005
            CreateHistory(requestMap["PR-2026-005"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-005"].Id, ManagerId, 1, ApprovalAction.Approve, "Dong y thanh toan"),

            // PR-006
            CreateHistory(requestMap["PR-2026-006"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-006"].Id, ManagerId, 1, ApprovalAction.Approve, "Da doi chieu"),
            CreateHistory(requestMap["PR-2026-006"].Id, DirectorId, 2, ApprovalAction.Approve, "Dong y cap 2"),

            // PR-007
            CreateHistory(requestMap["PR-2026-007"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-007"].Id, ManagerId, 1, ApprovalAction.Reject, "Ho so chua day du bien ban nghiem thu"),

            // PR-008
            CreateHistory(requestMap["PR-2026-008"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-008"].Id, ManagerId, 1, ApprovalAction.ReturnForEdit, "Can bo sung hoa don VAT"),

            // PR-009
            CreateHistory(requestMap["PR-2026-009"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
            CreateHistory(requestMap["PR-2026-009"].Id, ManagerId, 1, ApprovalAction.Approve, "Dong y"),
            CreateHistory(requestMap["PR-2026-009"].Id, AccountantId, 3, ApprovalAction.ConfirmPayment, "Da chuyen khoan"),

            // PR-010
            CreateHistory(requestMap["PR-2026-010"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet"),
        };

        dbContext.PaymentRequestApprovalHistories.AddRange(histories);

        var paidRequest = requestMap["PR-2026-009"];
        dbContext.PaymentConfirmations.Add(new PaymentConfirmation
        {
            PaymentRequestId = paidRequest.Id,
            PaymentDate = now.AddDays(-1),
            PaymentReferenceNumber = "PAY-REF-009",
            BankTransactionNumber = "BANK-TRX-009",
            PaidAmount = paidRequest.RequestedAmount,
            AccountingNote = "Da thanh toan du",
            PaymentStatus = PaymentStatus.Paid,
            CreatedAt = now,
            UpdatedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static PaymentRequest CreateRequest(
        string requestCode,
        string title,
        Guid projectId,
        Guid supplierId,
        Guid contractId,
        string requestType,
        string invoiceNumber,
        DateTime invoiceDate,
        DateTime dueDate,
        decimal amountBeforeVat,
        decimal vatRate,
        decimal advanceDeduction,
        decimal retentionAmount,
        decimal otherDeduction,
        PaymentRequestStatus status,
        Guid createdByUserId)
    {
        var vatAmount = Math.Round(amountBeforeVat * vatRate / 100m, 2, MidpointRounding.AwayFromZero);
        var amountAfterVat = Math.Round(amountBeforeVat + vatAmount, 2, MidpointRounding.AwayFromZero);
        var requestedAmount = Math.Round(amountAfterVat - advanceDeduction - retentionAmount - otherDeduction, 2, MidpointRounding.AwayFromZero);

        return new PaymentRequest
        {
            RequestCode = requestCode,
            Title = title,
            ProjectId = projectId,
            SupplierId = supplierId,
            ContractId = contractId,
            RequestType = requestType,
            InvoiceNumber = invoiceNumber,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            Description = title,
            AmountBeforeVat = amountBeforeVat,
            VatRate = vatRate,
            VatAmount = vatAmount,
            AmountAfterVat = amountAfterVat,
            AdvanceDeduction = advanceDeduction,
            RetentionAmount = retentionAmount,
            OtherDeduction = otherDeduction,
            RequestedAmount = requestedAmount,
            PaymentMethod = PaymentMethod.BankTransfer,
            CurrentStatus = status,
            CreatedByUserId = createdByUserId,
            Notes = "Du lieu mau"
        };
    }

    private static PaymentRequestApprovalHistory CreateHistory(
        Guid paymentRequestId,
        Guid approverUserId,
        int stepOrder,
        ApprovalAction action,
        string? comment)
    {
        return new PaymentRequestApprovalHistory
        {
            PaymentRequestId = paymentRequestId,
            ApproverUserId = approverUserId,
            StepOrder = stepOrder,
            Action = action,
            ActionAt = DateTime.UtcNow,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Guid? ResolveRoleProfileId(UserRole role, IReadOnlyDictionary<string, Guid> roleProfileIdByCode)
    {
        return roleProfileIdByCode.TryGetValue(role.ToString().ToLower(), out var roleProfileId)
            ? roleProfileId
            : null;
    }
}
