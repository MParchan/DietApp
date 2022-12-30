using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietApp.Migrations
{
    public partial class addCaloricBalance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CaloricBalance",
                table: "DailyConsumption",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloricBalance",
                table: "DailyConsumption");
        }
    }
}
