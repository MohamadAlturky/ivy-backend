using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddingMedicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(
                        type: "bit",
                        nullable: false,
                        defaultValue: false
                    ),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "FK_MedicalHistories_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "MedicalHistoryItems",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedicalHistoryId = table.Column<int>(type: "int", nullable: false),
                    MediaUrl = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(
                        type: "bit",
                        nullable: false,
                        defaultValue: false
                    ),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistoryItems_MedicalHistories_MedicalHistoryId",
                        column: x => x.MedicalHistoryId,
                        principalTable: "MedicalHistories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_CreatedAt",
                table: "MedicalHistories",
                column: "CreatedAt"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_CreatedByUserId",
                table: "MedicalHistories",
                column: "CreatedByUserId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_IsDeleted",
                table: "MedicalHistories",
                column: "IsDeleted"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_PatientId",
                table: "MedicalHistories",
                column: "PatientId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_Type",
                table: "MedicalHistories",
                column: "Type"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItems_IsDeleted",
                table: "MedicalHistoryItems",
                column: "IsDeleted"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItems_MediaType",
                table: "MedicalHistoryItems",
                column: "MediaType"
            );

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistoryItems_MedicalHistoryId",
                table: "MedicalHistoryItems",
                column: "MedicalHistoryId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MedicalHistoryItems");

            migrationBuilder.DropTable(name: "MedicalHistories");
        }
    }
}
