using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ivy.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddingRoleClinicAndPermissionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClinicRole",
                table: "AdminRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClinicPermission",
                table: "AdminPermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClinicRole",
                table: "AdminRoles");

            migrationBuilder.DropColumn(
                name: "IsClinicPermission",
                table: "AdminPermissions");
        }
    }
}
