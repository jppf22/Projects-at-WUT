using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyagers.Migrations
{
    /// <inheritdoc />
    public partial class CreatedSeparateOwnersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Trip");

            migrationBuilder.CreateTable(
                name: "TripOwners",
                columns: table => new
                {
                    TripID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripOwners", x => new { x.TripID, x.UserID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripOwners");

            migrationBuilder.AddColumn<string>(
                name: "UserID",
                table: "Trip",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
