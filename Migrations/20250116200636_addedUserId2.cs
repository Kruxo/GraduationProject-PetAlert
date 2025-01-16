using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class addedUserId2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LostPets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_LostPets_UserId",
                table: "LostPets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LostPets_AspNetUsers_UserId",
                table: "LostPets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LostPets_AspNetUsers_UserId",
                table: "LostPets");

            migrationBuilder.DropIndex(
                name: "IX_LostPets_UserId",
                table: "LostPets");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "LostPets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
