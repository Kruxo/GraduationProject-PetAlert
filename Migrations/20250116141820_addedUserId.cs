using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraduationProject.Migrations
{
    /// <inheritdoc />
    public partial class addedUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FoundPets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_FoundPets_UserId",
                table: "FoundPets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FoundPets_AspNetUsers_UserId",
                table: "FoundPets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FoundPets_AspNetUsers_UserId",
                table: "FoundPets");

            migrationBuilder.DropIndex(
                name: "IX_FoundPets_UserId",
                table: "FoundPets");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FoundPets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
