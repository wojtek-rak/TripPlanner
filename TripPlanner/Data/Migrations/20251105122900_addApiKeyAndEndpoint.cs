using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripPlanner.Migrations
{
    /// <inheritdoc />
    public partial class addApiKeyAndEndpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "AspNetUsers",
                type: "varchar(1000)",
                unicode: false,
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Endpoint",
                table: "AspNetUsers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAzureOpenAI",
                table: "AspNetUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpenAIModel",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Endpoint",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAzureOpenAI",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OpenAIModel",
                table: "AspNetUsers");
        }
    }
}
