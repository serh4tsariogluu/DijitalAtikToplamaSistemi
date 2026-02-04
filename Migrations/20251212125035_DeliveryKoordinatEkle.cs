using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AtikDonusum.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryKoordinatEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Boylam",
                table: "Deliveries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Enlem",
                table: "Deliveries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Boylam",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "Enlem",
                table: "Deliveries");
        }
    }
}
