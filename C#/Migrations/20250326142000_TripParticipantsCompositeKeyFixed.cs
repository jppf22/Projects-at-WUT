using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Voyagers.Migrations
{
    /// <inheritdoc />
    public partial class TripParticipantsCompositeKeyFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TripParticipants",
                columns: table => new
                {
                    TripID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripParticipants", x => new { x.TripID, x.UserID });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripParticipants");
        }
    }
}
