using System;
using ConstructionPayment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionPayment.Infrastructure.Persistence.Migrations
{
    [DbContextAttribute(typeof(AppDbContext))]
    [Migration("20260326000100_RoleProfilesAndGeneralAttachments")]
    public partial class RoleProfilesAndGeneralAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Code = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 150, nullable: false),
                    Description = table.Column<string>(maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RoleProfileId = table.Column<Guid>(nullable: false),
                    PermissionCode = table.Column<string>(maxLength: 120, nullable: false),
                    IsAllowed = table.Column<bool>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionGrants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionGrants_RoleProfiles_RoleProfileId",
                        column: x => x.RoleProfileId,
                        principalTable: "RoleProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "RoleProfileId",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "PaymentRequestAttachments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerType",
                table: "PaymentRequestAttachments",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentConfirmationId",
                table: "PaymentRequestAttachments",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentRequestId",
                table: "PaymentRequestAttachments",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleProfileId",
                table: "Users",
                column: "RoleProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionGrants_RoleProfileId_PermissionCode",
                table: "RolePermissionGrants",
                columns: new[] { "RoleProfileId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleProfiles_Code",
                table: "RoleProfiles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleProfiles_IsActive",
                table: "RoleProfiles",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_ContractId",
                table: "PaymentRequestAttachments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_OwnerType",
                table: "PaymentRequestAttachments",
                column: "OwnerType");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequestAttachments_PaymentConfirmationId",
                table: "PaymentRequestAttachments",
                column: "PaymentConfirmationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_RoleProfiles_RoleProfileId",
                table: "Users",
                column: "RoleProfileId",
                principalTable: "RoleProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestAttachments_Contracts_ContractId",
                table: "PaymentRequestAttachments",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequestAttachments_PaymentConfirmations_PaymentConfirmationId",
                table: "PaymentRequestAttachments",
                column: "PaymentConfirmationId",
                principalTable: "PaymentConfirmations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_RoleProfiles_RoleProfileId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestAttachments_Contracts_ContractId",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequestAttachments_PaymentConfirmations_PaymentConfirmationId",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleProfileId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestAttachments_ContractId",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestAttachments_OwnerType",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequestAttachments_PaymentConfirmationId",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropColumn(
                name: "RoleProfileId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropColumn(
                name: "OwnerType",
                table: "PaymentRequestAttachments");

            migrationBuilder.DropColumn(
                name: "PaymentConfirmationId",
                table: "PaymentRequestAttachments");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentRequestId",
                table: "PaymentRequestAttachments",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.DropTable(
                name: "RolePermissionGrants");

            migrationBuilder.DropTable(
                name: "RoleProfiles");
        }
    }
}
