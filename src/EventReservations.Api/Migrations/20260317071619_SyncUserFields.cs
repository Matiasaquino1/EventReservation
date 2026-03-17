using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventReservations.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "username");
        }
    }
}
