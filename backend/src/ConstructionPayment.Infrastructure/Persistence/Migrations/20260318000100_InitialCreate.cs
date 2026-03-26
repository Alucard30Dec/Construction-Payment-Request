using System;
using ConstructionPayment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionPayment.Infrastructure.Persistence.Migrations
{
    [DbContextAttribute(typeof(AppDbContext))]
    [Migration("20260318000100_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    Location = table.Column<string>(maxLength: 500, nullable: true),
                    Department = table.Column<string>(maxLength: 100, nullable: true),
                    ProjectManager = table.Column<string>(maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 250, nullable: false),
                    TaxCode = table.Column<string>(maxLength: 50, nullable: false),
                    Address = table.Column<string>(maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(maxLength: 200, nullable: true),
                    Phone = table.Column<string>(maxLength: 50, nullable: true),
                    Email = table.Column<string>(maxLength: 200, nullable: true),
                    BankAccountNumber = table.Column<string>(maxLength: 100, nullable: true),
                    BankName = table.Column<string>(maxLength: 200, nullable: true),
                    BankBranch = table.Column<string>(maxLength: 200, nullable: true),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Username = table.Column<string>(maxLength: 50, nullable: false),
                    FullName = table.Column<string>(maxLength: 200, nullable: false),
                    Email = table.Column<string>(maxLength: 200, nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 500, nullable: false),
                    Role = table.Column<int>(nullable: false),
                    Department = table.Column<string>(maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalMatrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MinAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    RequireDirectorApproval = table.Column<bool>(nullable: false),
                    Department = table.Column<string>(maxLength: 100, nullable: true),
                    ProjectId = table.Column<Guid>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalMatrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalMatrices_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ContractNumber = table.Column<string>(maxLength: 100, nullable: false),
                    Name = table.Column<string>(maxLength: 300, nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    SignedDate = table.Column<DateTime>(nullable: false),
                    ContractValue = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    ContractType = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(maxLength: 1000, nullable: true),
                    AttachmentPath = table.Column<string>(maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true),
                    Action = table.Column<string>(maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(maxLength: 100, nullable: false),
                    OldValue = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RequestCode = table.Column<string>(maxLength: 100, nullable: false),
                    Title = table.Column<string>(maxLength: 500, nullable: false),
                    ProjectId = table.Column<Guid>(nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    ContractId = table.Column<Guid>(nullable: true),
                    RequestType = table.Column<string>(maxLength: 100, nullable: false),
                    InvoiceNumber = table.Column<string>(maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<DateTime>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    AmountBeforeVat = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    VatRate = table.Column<decimal>(precision: 5, scale: 2, nullable: false),
                    VatAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    AmountAfterVat = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    AdvanceDeduction = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    RetentionAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    OtherDeduction = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    RequestedAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<int>(nullable: false),
                    CurrentStatus = table.Column<int>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    SubmittedAt = table.Column<DateTime>(nullable: true),
                    ApprovedAt = table.Column<DateTime>(nullable: true),
                    PaidAt = table.Column<DateTime>(nullable: true),
                    Notes = table.Column<string>(maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentConfirmations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentRequestId = table.Column<Guid>(nullable: false),
                    PaymentDate = table.Column<DateTime>(nullable: false),
                    PaymentReferenceNumber = table.Column<string>(maxLength: 100, nullable: true),
                    BankTransactionNumber = table.Column<string>(maxLength: 100, nullable: true),
                    PaidAmount = table.Column<decimal>(precision: 18, scale: 2, nullable: false),
                    AccountingNote = table.Column<string>(maxLength: 2000, nullable: true),
                    PaymentStatus = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentConfirmations_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestApprovalHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentRequestId = table.Column<Guid>(nullable: false),
                    ApproverUserId = table.Column<Guid>(nullable: false),
                    StepOrder = table.Column<int>(nullable: false),
                    Action = table.Column<int>(nullable: false),
                    ActionAt = table.Column<DateTime>(nullable: false),
                    Comment = table.Column<string>(maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestApprovalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRAppHist_PaymentRequest",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRequestApprovalHistories_Users_ApproverUserId",
                        column: x => x.ApproverUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRequestAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PaymentRequestId = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(nullable: false),
                    UploadedByUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequestAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_PaymentRequests_PaymentRequestId",
                        column: x => x.PaymentRequestId,
                        principalTable: "PaymentRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRequestAttachments_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalMatrices_IsActive_MinAmount_MaxAmount",
                table: "ApprovalMatrices",
                columns: new[] { "IsActive", "MinAmount", "MaxAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalMatrices_ProjectId",
                table: "ApprovalMatrices",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractNumber",
                table: "Contracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId",
                table: "Contracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SupplierId",
                table: "Contracts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfirmations_PaymentRequestId",
                table: "PaymentConfirmations",
                column: "PaymentRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestApprovalHistories_ApproverUserId",
                table: "PaymentRequestApprovalHistories",
                column: "ApproverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRAppHist_Request_Step_ActionAt",
                table: "PaymentRequestApprovalHistories",
                columns: new[] { "PaymentRequestId", "StepOrder", "ActionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_PaymentRequestId",
                table: "PaymentRequestAttachments",
                column: "PaymentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_UploadedByUserId",
                table: "PaymentRequestAttachments",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ContractId",
                table: "PaymentRequests",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CreatedByUserId",
                table: "PaymentRequests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CurrentStatus",
                table: "PaymentRequests",
                column: "CurrentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ProjectId",
                table: "PaymentRequests",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_RequestCode",
                table: "PaymentRequests",
                column: "RequestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_SupplierId_InvoiceNumber",
                table: "PaymentRequests",
                columns: new[] { "SupplierId", "InvoiceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_SupplierId",
                table: "PaymentRequests",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Code",
                table: "Projects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                table: "Suppliers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_TaxCode",
                table: "Suppliers",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ApprovalMatrices");
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "PaymentConfirmations");
            migrationBuilder.DropTable(name: "PaymentRequestApprovalHistories");
            migrationBuilder.DropTable(name: "PaymentRequestAttachments");
            migrationBuilder.DropTable(name: "PaymentRequests");
            migrationBuilder.DropTable(name: "Contracts");
            migrationBuilder.DropTable(name: "Users");
            migrationBuilder.DropTable(name: "Projects");
            migrationBuilder.DropTable(name: "Suppliers");
        }
    }
}
