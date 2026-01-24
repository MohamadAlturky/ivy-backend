using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddingAdminClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Doctors",
                type: "float",
                nullable: false,
                defaultValue: 3.0,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Admins",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_ClinicId",
                table: "Admins",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Admins_Clinics_ClinicId",
                table: "Admins",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admins_Clinics_ClinicId",
                table: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_Admins_ClinicId",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Admins");

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Doctors",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 3.0);
        }
    }
}
