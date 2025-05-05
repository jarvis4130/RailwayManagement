using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RailwayManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayPaymentIdToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayPaymentId",
                table: "Payments");
        }
    }
}
