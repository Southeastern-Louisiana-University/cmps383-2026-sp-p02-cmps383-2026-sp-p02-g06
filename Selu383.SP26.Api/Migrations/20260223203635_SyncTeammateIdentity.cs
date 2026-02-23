using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Selu383.SP26.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncTeammateIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_ManagerId",
                table: "Locations",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AspNetUsers_ManagerId",
                table: "Locations",
                column: "ManagerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AspNetUsers_ManagerId",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_ManagerId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Locations");
        }
    }
}
