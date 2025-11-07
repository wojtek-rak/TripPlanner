using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripPlanner.Migrations
{
    /// <inheritdoc />
    public partial class TripNewProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Destination",
                table: "Trips",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Trips",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Trips",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                table: "Trips",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOwner",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "Trips");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Trips",
                newName: "Destination");
        }
    }
}
