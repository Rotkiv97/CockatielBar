using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class UserCocktailLikeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCocktailsLike_DbUser_UserId",
                table: "UserCocktailsLike");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserCocktailsLike",
                newName: "UsersLikedId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCocktailsLike_UserId",
                table: "UserCocktailsLike",
                newName: "IX_UserCocktailsLike_UsersLikedId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCocktailsLike_DbUser_UsersLikedId",
                table: "UserCocktailsLike",
                column: "UsersLikedId",
                principalTable: "DbUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserCocktailsLike_DbUser_UsersLikedId",
                table: "UserCocktailsLike");

            migrationBuilder.RenameColumn(
                name: "UsersLikedId",
                table: "UserCocktailsLike",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserCocktailsLike_UsersLikedId",
                table: "UserCocktailsLike",
                newName: "IX_UserCocktailsLike_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserCocktailsLike_DbUser_UserId",
                table: "UserCocktailsLike",
                column: "UserId",
                principalTable: "DbUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
