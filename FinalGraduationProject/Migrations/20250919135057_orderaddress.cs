using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalGraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class orderaddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                 name: "AddressId",
                 table: "Orders",
                 type: "bigint",
                 nullable: true,
                 defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressId",
                table: "Orders",
                column: "AddressId"
                );

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AddressId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Orders");
        }
    }
}
