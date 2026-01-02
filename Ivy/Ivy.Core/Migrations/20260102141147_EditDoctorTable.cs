using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Core.Migrations
{
    /// <inheritdoc />
    public partial class EditDoctorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorMedicalSpeciality_Doctors_DoctorId",
                table: "DoctorMedicalSpeciality");

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "DoctorMedicalSpeciality",
                newName: "DoctorDynamicProfileHistoryId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorMedicalSpeciality_DoctorId",
                table: "DoctorMedicalSpeciality",
                newName: "IX_DoctorMedicalSpeciality_DoctorDynamicProfileHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorMedicalSpeciality_DoctorDynamicProfileHistories_DoctorDynamicProfileHistoryId",
                table: "DoctorMedicalSpeciality",
                column: "DoctorDynamicProfileHistoryId",
                principalTable: "DoctorDynamicProfileHistories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorMedicalSpeciality_DoctorDynamicProfileHistories_DoctorDynamicProfileHistoryId",
                table: "DoctorMedicalSpeciality");

            migrationBuilder.RenameColumn(
                name: "DoctorDynamicProfileHistoryId",
                table: "DoctorMedicalSpeciality",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorMedicalSpeciality_DoctorDynamicProfileHistoryId",
                table: "DoctorMedicalSpeciality",
                newName: "IX_DoctorMedicalSpeciality_DoctorId");

            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorMedicalSpeciality_Doctors_DoctorId",
                table: "DoctorMedicalSpeciality",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
