using ConstructionPayment.Application.Authorization;
using ConstructionPayment.Application.Interfaces;
using ConstructionPayment.Domain.Common;
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
    private static readonly Guid Employee2Id = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid Manager2Id = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid Accountant2Id = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private static readonly Guid AdminRoleProfileId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
    private static readonly Guid EmployeeRoleProfileId = Guid.Parse("aaaa2222-2222-2222-2222-222222222222");
    private static readonly Guid DepartmentManagerRoleProfileId = Guid.Parse("aaaa3333-3333-3333-3333-333333333333");
    private static readonly Guid DirectorRoleProfileId = Guid.Parse("aaaa4444-4444-4444-4444-444444444444");
    private static readonly Guid AccountantRoleProfileId = Guid.Parse("aaaa5555-5555-5555-5555-555555555555");
    private static readonly Guid ViewerRoleProfileId = Guid.Parse("aaaa6666-6666-6666-6666-666666666666");

    private static readonly Guid ProjectAId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private static readonly Guid ProjectBId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
    private static readonly Guid ProjectCId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3");
    private static readonly Guid ProjectDId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4");
    private static readonly Guid ProjectEId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5");

    private static readonly Guid Supplier1Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
    private static readonly Guid Supplier2Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2");
    private static readonly Guid Supplier3Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb3");
    private static readonly Guid Supplier4Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb4");
    private static readonly Guid Supplier5Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb5");
    private static readonly Guid Supplier6Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb6");
    private static readonly Guid Supplier7Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb7");

    private static readonly Guid Contract1Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
    private static readonly Guid Contract2Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc2");
    private static readonly Guid Contract3Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc3");
    private static readonly Guid Contract4Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc4");
    private static readonly Guid Contract5Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc5");
    private static readonly Guid Contract6Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc6");
    private static readonly Guid Contract7Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc7");
    private static readonly Guid Contract8Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc8");
    private static readonly Guid Contract9Id = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc9");
    private static readonly Guid Contract10Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd1");
    private static readonly Guid Contract11Id = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddd2");

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
                Id = Employee2Id,
                Username = "employee2",
                FullName = "Le Thi Employee MEP",
                Email = "employee2@construction.local",
                Role = UserRole.Employee,
                RoleProfileId = ResolveRoleProfileId(UserRole.Employee, roleProfileIdByCode),
                Department = "MEP",
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
                Id = Manager2Id,
                Username = "manager2",
                FullName = "Hoang Van Manager MEP",
                Email = "manager2@construction.local",
                Role = UserRole.DepartmentManager,
                RoleProfileId = ResolveRoleProfileId(UserRole.DepartmentManager, roleProfileIdByCode),
                Department = "MEP",
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
                Id = Accountant2Id,
                Username = "accountant2",
                FullName = "Backup Accountant",
                Email = "accountant2@construction.local",
                Role = UserRole.Accountant,
                RoleProfileId = ResolveRoleProfileId(UserRole.Accountant, roleProfileIdByCode),
                Department = "Ke toan",
                IsActive = false
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
            ["employee2"] = "employee2123",
            ["manager"] = "manager123",
            ["manager2"] = "manager2123",
            ["director"] = "director123",
            ["accountant"] = "accountant123",
            ["accountant2"] = "accountant2123",
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
            existing.IsActive = user.IsActive;
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
            },
            new()
            {
                Id = Supplier6Id,
                Code = "NCC006",
                Name = "Cong ty CP Noi That Delta",
                TaxCode = "0101234506",
                Address = "Binh Duong",
                ContactPerson = "Nguyen Delta",
                Phone = "0901000006",
                Email = "ncc6@example.com",
                BankAccountNumber = "12345678906",
                BankName = "VPBank",
                BankBranch = "Binh Duong",
                Notes = "Hoan thien va noi that",
                IsActive = true
            },
            new()
            {
                Id = Supplier7Id,
                Code = "NCC007",
                Name = "Cong ty Bao Tri Co Khi Beta",
                TaxCode = "0101234507",
                Address = "Bac Ninh",
                ContactPerson = "Tran Beta",
                Phone = "0901000007",
                Email = "ncc7@example.com",
                BankAccountNumber = "12345678907",
                BankName = "Sacombank",
                BankBranch = "Bac Ninh",
                Notes = "Nha cung cap tam dung hop tac",
                IsActive = false
            }
        };

        var existingByCode = await dbContext.Suppliers
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var template in suppliers)
        {
            if (!existingByCode.TryGetValue(template.Code, out var supplier))
            {
                dbContext.Suppliers.Add(template);
                continue;
            }

            supplier.Name = template.Name;
            supplier.TaxCode = template.TaxCode;
            supplier.Address = template.Address;
            supplier.ContactPerson = template.ContactPerson;
            supplier.Phone = template.Phone;
            supplier.Email = template.Email;
            supplier.BankAccountNumber = template.BankAccountNumber;
            supplier.BankName = template.BankName;
            supplier.BankBranch = template.BankBranch;
            supplier.Notes = template.Notes;
            supplier.IsActive = template.IsActive;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedProjectsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
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
                ProjectManager = "Hoang Van Manager MEP",
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
            },
            new()
            {
                Id = ProjectDId,
                Code = "DA004",
                Name = "Trung tam logistics Delta",
                Location = "Binh Duong",
                Department = "Hoan thien",
                ProjectManager = "Nguyen Delta",
                IsActive = true
            },
            new()
            {
                Id = ProjectEId,
                Code = "DA005",
                Name = "Tuyen duong noi bo K1",
                Location = "Quang Ninh",
                Department = "Ha tang",
                ProjectManager = "Project Manager Legacy",
                IsActive = false
            }
        };

        var existingByCode = await dbContext.Projects
            .ToDictionaryAsync(x => x.Code, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var template in projects)
        {
            if (!existingByCode.TryGetValue(template.Code, out var project))
            {
                dbContext.Projects.Add(template);
                continue;
            }

            project.Name = template.Name;
            project.Location = template.Location;
            project.Department = template.Department;
            project.ProjectManager = template.ProjectManager;
            project.IsActive = template.IsActive;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedContractsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var referenceDate = DateTime.UtcNow.Date;
        var contracts = new List<Contract>
        {
            new()
            {
                Id = Contract1Id,
                ContractNumber = "HD-2026-001",
                Name = "Hop dong cung cap vat tu phan tho",
                SupplierId = Supplier1Id,
                ProjectId = ProjectAId,
                SignedDate = referenceDate.AddMonths(-8),
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
                SignedDate = referenceDate.AddMonths(-7),
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
                SignedDate = referenceDate.AddMonths(-6),
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
                SignedDate = referenceDate.AddMonths(-4),
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
                SignedDate = referenceDate.AddMonths(-5),
                ContractValue = 780000000,
                ContractType = ContractType.Consulting,
                Notes = "Bao cao dinh ky",
                IsActive = true
            },
            new()
            {
                Id = Contract6Id,
                ContractNumber = "HD-2026-006",
                Name = "Hop dong hoan thien noi that tang 1",
                SupplierId = Supplier1Id,
                ProjectId = ProjectAId,
                SignedDate = referenceDate.AddMonths(-3),
                ContractValue = 680000000,
                ContractType = ContractType.Procurement,
                Notes = "Hop dong thu hai cung du an va nha cung cap",
                IsActive = true
            },
            new()
            {
                Id = Contract7Id,
                ContractNumber = "HD-2026-007",
                Name = "Hop dong ket cau bo sung",
                SupplierId = Supplier2Id,
                ProjectId = ProjectAId,
                SignedDate = referenceDate.AddMonths(-2),
                ContractValue = 350000000,
                ContractType = ContractType.Construction,
                Notes = "Dang tam ngung",
                IsActive = false
            },
            new()
            {
                Id = Contract8Id,
                ContractNumber = "HD-2026-008",
                Name = "Hop dong vat tu phong sach",
                SupplierId = Supplier1Id,
                ProjectId = ProjectBId,
                SignedDate = referenceDate.AddMonths(-2),
                ContractValue = 540000000,
                ContractType = ContractType.Procurement,
                Notes = "Cung du an khac de test filter project",
                IsActive = true
            },
            new()
            {
                Id = Contract9Id,
                ContractNumber = "HD-2026-009",
                Name = "Hop dong noi that khoi van hanh",
                SupplierId = Supplier6Id,
                ProjectId = ProjectDId,
                SignedDate = referenceDate.AddMonths(-4),
                ContractValue = 980000000,
                ContractType = ContractType.Other,
                Notes = "Dung de test project co matrix rieng",
                IsActive = true
            },
            new()
            {
                Id = Contract10Id,
                ContractNumber = "HD-2026-010",
                Name = "Hop dong bao tri ha tang giai doan 1",
                SupplierId = Supplier7Id,
                ProjectId = ProjectEId,
                SignedDate = referenceDate.AddMonths(-9),
                ContractValue = 410000000,
                ContractType = ContractType.Service,
                Notes = "Contract mau inactive",
                IsActive = false
            },
            new()
            {
                Id = Contract11Id,
                ContractNumber = "HD-2026-011",
                Name = "Hop dong van tai dat da dot 2",
                SupplierId = Supplier4Id,
                ProjectId = ProjectCId,
                SignedDate = referenceDate.AddMonths(-1),
                ContractValue = 260000000,
                ContractType = ContractType.Service,
                Notes = "Them hop dong thu hai cung supplier va project",
                IsActive = true
            }
        };

        var existingByNumber = await dbContext.Contracts
            .ToDictionaryAsync(x => x.ContractNumber, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var template in contracts)
        {
            if (!existingByNumber.TryGetValue(template.ContractNumber, out var contract))
            {
                dbContext.Contracts.Add(template);
                continue;
            }

            contract.Name = template.Name;
            contract.SupplierId = template.SupplierId;
            contract.ProjectId = template.ProjectId;
            contract.SignedDate = template.SignedDate;
            contract.ContractValue = template.ContractValue;
            contract.ContractType = template.ContractType;
            contract.Notes = template.Notes;
            contract.AttachmentPath = template.AttachmentPath;
            contract.IsActive = template.IsActive;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedApprovalMatricesAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
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
            },
            new()
            {
                MinAmount = 0,
                MaxAmount = 200000000,
                RequireDirectorApproval = false,
                Department = null,
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 200000001,
                MaxAmount = 99999999999,
                RequireDirectorApproval = true,
                Department = null,
                ProjectId = null,
                IsActive = true
            },
            new()
            {
                MinAmount = 0,
                MaxAmount = 150000000,
                RequireDirectorApproval = false,
                Department = null,
                ProjectId = ProjectDId,
                IsActive = true
            },
            new()
            {
                MinAmount = 150000001,
                MaxAmount = 99999999999,
                RequireDirectorApproval = true,
                Department = null,
                ProjectId = ProjectDId,
                IsActive = true
            },
            new()
            {
                MinAmount = 0,
                MaxAmount = 99999999999,
                RequireDirectorApproval = false,
                Department = "Xay dung",
                ProjectId = ProjectAId,
                IsActive = false
            }
        };

        var existing = await dbContext.ApprovalMatrices.ToListAsync(cancellationToken);
        var existingByKey = existing.ToDictionary(BuildApprovalMatrixKey);

        foreach (var template in items)
        {
            var key = BuildApprovalMatrixKey(template);
            if (!existingByKey.TryGetValue(key, out var item))
            {
                dbContext.ApprovalMatrices.Add(template);
                continue;
            }

            item.MinAmount = template.MinAmount;
            item.MaxAmount = template.MaxAmount;
            item.RequireDirectorApproval = template.RequireDirectorApproval;
            item.Department = template.Department;
            item.ProjectId = template.ProjectId;
            item.IsActive = template.IsActive;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPaymentRequestsAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
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
            CreateRequest("PR-2026-010", "Thanh toan dat da cong trinh", ProjectAId, Supplier2Id, Contract2Id, "ThiCong", "INV-010", now.AddDays(-12), now.AddDays(8), 300000000, 10, 0, 0, 0, PaymentRequestStatus.PendingDepartmentApproval, EmployeeId),
            CreateRequest("PR-2026-011", "Tam ung mua vat tu du phong", ProjectBId, Supplier3Id, null, "TamUng", "INV-011", now.AddDays(-8), now.AddDays(18), 80000000, 10, 0, 0, 0, PaymentRequestStatus.Draft, Employee2Id, PaymentMethod.Cash),
            CreateRequest("PR-2026-012", "Thanh toan noi that dot 1", ProjectAId, Supplier1Id, Contract6Id, "NoiThat", "INV-012", now.AddDays(-16), now.AddDays(6), 210000000, 10, 5000000, 0, 0, PaymentRequestStatus.PendingDepartmentApproval, EmployeeId),
            CreateRequest("PR-2026-013", "Thanh toan thi cong phat sinh", ProjectAId, Supplier2Id, Contract2Id, "PhatSinh", "INV-013", now.AddDays(-19), now.AddDays(4), 430000000, 10, 0, 15000000, 0, PaymentRequestStatus.PendingAccounting, EmployeeId, PaymentMethod.Other),
            CreateRequest("PR-2026-014", "Thanh toan noi that khoi van hanh dot 1", ProjectDId, Supplier6Id, Contract9Id, "NoiThat", "INV-014", now.AddDays(-13), now.AddDays(9), 120000000, 10, 0, 0, 0, PaymentRequestStatus.PendingAccounting, Employee2Id),
            CreateRequest("PR-2026-015", "Thanh toan noi that khoi van hanh dot 2", ProjectDId, Supplier6Id, Contract9Id, "NoiThat", "INV-015", now.AddDays(-34), now.AddDays(-4), 360000000, 10, 20000000, 5000000, 0, PaymentRequestStatus.Paid, Employee2Id),
            CreateRequest("PR-2026-016", "Bo sung ho so tu van giam sat", ProjectCId, Supplier5Id, null, "TuVan", "INV-016", now.AddDays(-9), now.AddDays(11), 95000000, 10, 0, 0, 1000000, PaymentRequestStatus.ReturnedForEdit, EmployeeId, PaymentMethod.Cash),
            CreateRequest("PR-2026-017", "Thanh toan MEP tang ky thuat", ProjectBId, Supplier3Id, Contract3Id, "MEP", "INV-017", now.AddDays(-27), now.AddDays(2), 650000000, 10, 0, 0, 0, PaymentRequestStatus.Rejected, Employee2Id),
            CreateRequest("PR-2026-018", "Ho so da huy de lap lai", ProjectAId, Supplier1Id, Contract1Id, "VatTu", "INV-018", now.AddDays(-11), now.AddDays(15), 110000000, 10, 0, 0, 0, PaymentRequestStatus.Cancelled, EmployeeId),
            CreateRequest("PR-2026-019", "Thanh toan van tai dat da dot 2", ProjectCId, Supplier4Id, Contract11Id, "DichVu", "INV-019", now.AddDays(-7), now.AddDays(16), 140000000, 10, 0, 3000000, 0, PaymentRequestStatus.Draft, Employee2Id, PaymentMethod.Other),
            CreateRequest("PR-2026-020", "Thanh toan vat tu phong sach", ProjectBId, Supplier1Id, Contract8Id, "VatTu", "INV-020", now.AddDays(-21), now.AddDays(1), 175000000, 10, 0, 0, 0, PaymentRequestStatus.PendingAccounting, Employee2Id)
        };

        var existingByCode = await dbContext.PaymentRequests
            .ToDictionaryAsync(x => x.RequestCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var template in requests)
        {
            ApplyStatusTimestamps(template, now);

            if (!existingByCode.TryGetValue(template.RequestCode, out var request))
            {
                dbContext.PaymentRequests.Add(template);
                continue;
            }

            request.Title = template.Title;
            request.ProjectId = template.ProjectId;
            request.SupplierId = template.SupplierId;
            request.ContractId = template.ContractId;
            request.RequestType = template.RequestType;
            request.InvoiceNumber = template.InvoiceNumber;
            request.InvoiceDate = template.InvoiceDate;
            request.DueDate = template.DueDate;
            request.Description = template.Description;
            request.AmountBeforeVat = template.AmountBeforeVat;
            request.VatRate = template.VatRate;
            request.VatAmount = template.VatAmount;
            request.AmountAfterVat = template.AmountAfterVat;
            request.AdvanceDeduction = template.AdvanceDeduction;
            request.RetentionAmount = template.RetentionAmount;
            request.OtherDeduction = template.OtherDeduction;
            request.RequestedAmount = template.RequestedAmount;
            request.PaymentMethod = template.PaymentMethod;
            request.CurrentStatus = template.CurrentStatus;
            request.CreatedByUserId = template.CreatedByUserId;
            request.SubmittedAt = template.SubmittedAt;
            request.ApprovedAt = template.ApprovedAt;
            request.PaidAt = template.PaidAt;
            request.Notes = template.Notes;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var seededRequestCodes = requests.Select(x => x.RequestCode).ToArray();
        var requestMap = await dbContext.PaymentRequests
            .Where(x => seededRequestCodes.Contains(x.RequestCode))
            .ToDictionaryAsync(x => x.RequestCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var histories = new List<PaymentRequestApprovalHistory>
        {
            CreateHistory(requestMap["PR-2026-003"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet cap 1", now.AddDays(-6)),
            CreateHistory(requestMap["PR-2026-004"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-8)),
            CreateHistory(requestMap["PR-2026-004"].Id, ManagerId, 1, ApprovalAction.Approve, "Da kiem tra", now.AddDays(-7)),
            CreateHistory(requestMap["PR-2026-005"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-5)),
            CreateHistory(requestMap["PR-2026-005"].Id, Manager2Id, 1, ApprovalAction.Approve, "Dong y thanh toan", now.AddDays(-4)),
            CreateHistory(requestMap["PR-2026-006"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-11)),
            CreateHistory(requestMap["PR-2026-006"].Id, Manager2Id, 1, ApprovalAction.Approve, "Da doi chieu", now.AddDays(-10)),
            CreateHistory(requestMap["PR-2026-006"].Id, DirectorId, 2, ApprovalAction.Approve, "Dong y cap 2", now.AddDays(-9)),
            CreateHistory(requestMap["PR-2026-007"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-4)),
            CreateHistory(requestMap["PR-2026-007"].Id, ManagerId, 1, ApprovalAction.Reject, "Ho so chua day du bien ban nghiem thu", now.AddDays(-3)),
            CreateHistory(requestMap["PR-2026-008"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-3)),
            CreateHistory(requestMap["PR-2026-008"].Id, ManagerId, 1, ApprovalAction.ReturnForEdit, "Can bo sung hoa don VAT", now.AddDays(-2)),
            CreateHistory(requestMap["PR-2026-009"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-20)),
            CreateHistory(requestMap["PR-2026-009"].Id, ManagerId, 1, ApprovalAction.Approve, "Dong y", now.AddDays(-18)),
            CreateHistory(requestMap["PR-2026-009"].Id, AccountantId, 3, ApprovalAction.ConfirmPayment, "Da chuyen khoan", now.AddDays(-16)),
            CreateHistory(requestMap["PR-2026-010"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-2)),
            CreateHistory(requestMap["PR-2026-012"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet thanh toan noi that", now.AddDays(-5)),
            CreateHistory(requestMap["PR-2026-013"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet phat sinh", now.AddDays(-7)),
            CreateHistory(requestMap["PR-2026-013"].Id, ManagerId, 1, ApprovalAction.Approve, "Dong y thanh toan phat sinh", now.AddDays(-6)),
            CreateHistory(requestMap["PR-2026-014"].Id, Employee2Id, 0, ApprovalAction.Submit, "Gui duyet noi that", now.AddDays(-4)),
            CreateHistory(requestMap["PR-2026-014"].Id, Manager2Id, 1, ApprovalAction.Approve, "Da kiem tra khoi luong", now.AddDays(-3)),
            CreateHistory(requestMap["PR-2026-015"].Id, Employee2Id, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-14)),
            CreateHistory(requestMap["PR-2026-015"].Id, Manager2Id, 1, ApprovalAction.Approve, "Dong y cap 1", now.AddDays(-13)),
            CreateHistory(requestMap["PR-2026-015"].Id, DirectorId, 2, ApprovalAction.Approve, "Dong y cap 2", now.AddDays(-12)),
            CreateHistory(requestMap["PR-2026-015"].Id, AccountantId, 3, ApprovalAction.ConfirmPayment, "Da thanh toan du", now.AddDays(-11)),
            CreateHistory(requestMap["PR-2026-016"].Id, EmployeeId, 0, ApprovalAction.Submit, "Gui duyet bo sung", now.AddDays(-2)),
            CreateHistory(requestMap["PR-2026-016"].Id, ManagerId, 1, ApprovalAction.ReturnForEdit, "Bo sung hop dong va de nghi nghiem thu", now.AddDays(-1)),
            CreateHistory(requestMap["PR-2026-017"].Id, Employee2Id, 0, ApprovalAction.Submit, "Gui duyet", now.AddDays(-9)),
            CreateHistory(requestMap["PR-2026-017"].Id, Manager2Id, 1, ApprovalAction.Approve, "Dong y cap 1", now.AddDays(-8)),
            CreateHistory(requestMap["PR-2026-017"].Id, DirectorId, 2, ApprovalAction.Reject, "Can lam ro gia tri phat sinh", now.AddDays(-7)),
            CreateHistory(requestMap["PR-2026-018"].Id, EmployeeId, 0, ApprovalAction.Submit, "Lap thu roi huy", now.AddDays(-3)),
            CreateHistory(requestMap["PR-2026-020"].Id, Employee2Id, 0, ApprovalAction.Submit, "Gui duyet vat tu phong sach", now.AddDays(-6)),
            CreateHistory(requestMap["PR-2026-020"].Id, Manager2Id, 1, ApprovalAction.Approve, "Dong y", now.AddDays(-5)),
        };

        var seededRequestIds = requestMap.Values.Select(x => x.Id).ToHashSet();
        var existingHistories = await dbContext.PaymentRequestApprovalHistories
            .Where(x => seededRequestIds.Contains(x.PaymentRequestId))
            .ToListAsync(cancellationToken);
        var existingHistoryKeys = existingHistories
            .Select(BuildHistoryKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var history in histories)
        {
            var key = BuildHistoryKey(history);
            if (existingHistoryKeys.Add(key))
            {
                dbContext.PaymentRequestApprovalHistories.Add(history);
            }
        }

        var confirmations = new List<PaymentConfirmation>
        {
            CreateConfirmation(requestMap["PR-2026-009"].Id, now.AddDays(-16), "PAY-REF-009", "BANK-TRX-009", requestMap["PR-2026-009"].RequestedAmount, "Da thanh toan du", PaymentStatus.Paid),
            CreateConfirmation(requestMap["PR-2026-013"].Id, now.AddDays(-2), "PAY-REF-013", "BANK-TRX-013", requestMap["PR-2026-013"].RequestedAmount - 50000000, "Da thanh toan mot phan", PaymentStatus.PartiallyPaid),
            CreateConfirmation(requestMap["PR-2026-014"].Id, now.AddDays(-1), "PAY-REF-014", "BANK-TRX-014", 0, "Da tao lenh thanh toan, chua giai ngan", PaymentStatus.Unpaid),
            CreateConfirmation(requestMap["PR-2026-015"].Id, now.AddDays(-11), "PAY-REF-015", "BANK-TRX-015", requestMap["PR-2026-015"].RequestedAmount, "Da thanh toan du", PaymentStatus.Paid),
        };

        var existingConfirmations = await dbContext.PaymentConfirmations
            .Where(x => seededRequestIds.Contains(x.PaymentRequestId))
            .ToDictionaryAsync(x => x.PaymentRequestId, cancellationToken);

        foreach (var template in confirmations)
        {
            if (!existingConfirmations.TryGetValue(template.PaymentRequestId, out var confirmation))
            {
                dbContext.PaymentConfirmations.Add(template);
                continue;
            }

            confirmation.PaymentDate = template.PaymentDate;
            confirmation.PaymentReferenceNumber = template.PaymentReferenceNumber;
            confirmation.BankTransactionNumber = template.BankTransactionNumber;
            confirmation.PaidAmount = template.PaidAmount;
            confirmation.AccountingNote = template.AccountingNote;
            confirmation.PaymentStatus = template.PaymentStatus;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static PaymentRequest CreateRequest(
        string requestCode,
        string title,
        Guid projectId,
        Guid supplierId,
        Guid? contractId,
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
        Guid createdByUserId,
        PaymentMethod paymentMethod = PaymentMethod.BankTransfer)
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
            PaymentMethod = paymentMethod,
            CurrentStatus = status,
            CreatedByUserId = createdByUserId,
            Notes = "Du lieu mau"
        };
    }

    private static void ApplyStatusTimestamps(PaymentRequest request, DateTime now)
    {
        request.SubmittedAt = null;
        request.ApprovedAt = null;
        request.PaidAt = null;

        if (request.CurrentStatus is PaymentRequestStatus.PendingDepartmentApproval
            or PaymentRequestStatus.PendingDirectorApproval
            or PaymentRequestStatus.PendingAccounting
            or PaymentRequestStatus.Rejected
            or PaymentRequestStatus.ReturnedForEdit
            or PaymentRequestStatus.Paid
            or PaymentRequestStatus.Cancelled)
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

    private static PaymentRequestApprovalHistory CreateHistory(
        Guid paymentRequestId,
        Guid approverUserId,
        int stepOrder,
        ApprovalAction action,
        string? comment,
        DateTime actionAt)
    {
        return new PaymentRequestApprovalHistory
        {
            PaymentRequestId = paymentRequestId,
            ApproverUserId = approverUserId,
            StepOrder = stepOrder,
            Action = action,
            ActionAt = actionAt,
            Comment = comment,
            CreatedAt = actionAt,
            UpdatedAt = actionAt
        };
    }

    private static PaymentConfirmation CreateConfirmation(
        Guid paymentRequestId,
        DateTime paymentDate,
        string? paymentReferenceNumber,
        string? bankTransactionNumber,
        decimal paidAmount,
        string? accountingNote,
        PaymentStatus paymentStatus)
    {
        return new PaymentConfirmation
        {
            PaymentRequestId = paymentRequestId,
            PaymentDate = paymentDate,
            PaymentReferenceNumber = paymentReferenceNumber,
            BankTransactionNumber = bankTransactionNumber,
            PaidAmount = paidAmount,
            AccountingNote = accountingNote,
            PaymentStatus = paymentStatus,
            CreatedAt = paymentDate,
            UpdatedAt = paymentDate
        };
    }

    private static string BuildApprovalMatrixKey(ApprovalMatrix matrix)
    {
        return $"{matrix.ProjectId?.ToString() ?? "global"}|{matrix.Department?.Trim().ToLowerInvariant() ?? "all"}|{matrix.MinAmount:0.##}|{matrix.MaxAmount:0.##}";
    }

    private static string BuildHistoryKey(PaymentRequestApprovalHistory history)
    {
        return $"{history.PaymentRequestId}|{history.ApproverUserId}|{history.StepOrder}|{history.Action}|{history.Comment?.Trim().ToLowerInvariant() ?? string.Empty}";
    }

    private static Guid? ResolveRoleProfileId(UserRole role, IReadOnlyDictionary<string, Guid> roleProfileIdByCode)
    {
        return roleProfileIdByCode.TryGetValue(role.ToString().ToLower(), out var roleProfileId)
            ? roleProfileId
            : null;
    }
}
