using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietApp.Migrations
{
    public partial class addBMRandBMI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BMI",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "BMR",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BMI",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BMR",
                table: "AspNetUsers");
        }
    }
}
