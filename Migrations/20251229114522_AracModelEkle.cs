using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtikDonusum.Migrations
{
    /// <inheritdoc />
    public partial class AracModelEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Vehicles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "Vehicles");
        }
    }
}
