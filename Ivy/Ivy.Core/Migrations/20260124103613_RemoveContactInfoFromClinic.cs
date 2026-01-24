using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Core.Migrations
{
    /// <inheritdoc />
    public partial class RemoveContactInfoFromClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clinics_ContactEmail_Unique",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_ContactPhoneNumber",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "ContactPhoneNumber",
                table: "Clinics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Clinics",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhoneNumber",
                table: "Clinics",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ContactEmail_Unique",
                table: "Clinics",
                column: "ContactEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ContactPhoneNumber",
                table: "Clinics",
                column: "ContactPhoneNumber");
        }
    }
}
