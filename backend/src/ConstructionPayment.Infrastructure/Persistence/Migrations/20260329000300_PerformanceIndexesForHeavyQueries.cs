using ConstructionPayment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionPayment.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260329000300_PerformanceIndexesForHeavyQueries")]
    public partial class PerformanceIndexesForHeavyQueries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Role_IsActive_CreatedAt",
                table: "Users",
                columns: new[] { "Role", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatedAt",
                table: "Contracts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_IsActive_CreatedAt",
                table: "Contracts",
                columns: new[] { "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_SupplierId_CreatedAt",
                table: "Contracts",
                columns: new[] { "SupplierId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId_CreatedAt",
                table: "Contracts",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CreatedAt",
                table: "PaymentRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_CurrentStatus_CreatedAt",
                table: "PaymentRequests",
                columns: new[] { "CurrentStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_ProjectId_CreatedAt",
                table: "PaymentRequests",
                columns: new[] { "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_SupplierId_CreatedAt",
                table: "PaymentRequests",
                columns: new[] { "SupplierId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalMatrices_ProjectId_IsActive_MinAmount",
                table: "ApprovalMatrices",
                columns: new[] { "ProjectId", "IsActive", "MinAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalMatrices_Department_IsActive_MinAmount",
                table: "ApprovalMatrices",
                columns: new[] { "Department", "IsActive", "MinAmount" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "Action", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityName_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "EntityName", "CreatedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Role_IsActive_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_CreatedAt",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_IsActive_CreatedAt",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_SupplierId_CreatedAt",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ProjectId_CreatedAt",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_CreatedAt",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_CurrentStatus_CreatedAt",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_ProjectId_CreatedAt",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_SupplierId_CreatedAt",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalMatrices_ProjectId_IsActive_MinAmount",
                table: "ApprovalMatrices");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalMatrices_Department_IsActive_MinAmount",
                table: "ApprovalMatrices");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action_CreatedAt",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_EntityName_CreatedAt",
                table: "AuditLogs");
        }
    }
}
