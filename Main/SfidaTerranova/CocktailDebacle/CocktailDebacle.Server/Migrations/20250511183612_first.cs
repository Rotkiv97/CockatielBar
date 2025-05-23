using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cocktails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserIdCocktail = table.Column<int>(type: "int", nullable: false),
                    PublicCocktail = table.Column<bool>(type: "bit", nullable: true),
                    dateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Likes = table.Column<int>(type: "int", nullable: false),
                    IdDrink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrDrink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrDrinkAlternate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrTags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrVideo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIBA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrAlcoholic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrGlass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsES = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsDE = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsFR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsIT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsZH_HANS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrInstructionsZH_HANT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrDrinkThumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient11 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient12 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient13 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient14 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrIngredient15 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure7 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure8 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure9 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure10 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure11 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure12 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure13 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure14 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrMeasure15 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrImageSource = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrImageAttribution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StrCreativeCommonsConfirmed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateModified = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cocktails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcceptCookies = table.Column<bool>(type: "bit", nullable: true),
                    CustomSearch = table.Column<bool>(type: "bit", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TokenExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ImgProfileUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileParallaxImg = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio_link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOfMajorityAge = table.Column<bool>(type: "bit", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbUserHistorySearch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SearchText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SearchDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUserHistorySearch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbUserHistorySearch_DbUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DbUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCocktailsLike",
                columns: table => new
                {
                    CocktailsLikeId = table.Column<int>(type: "int", nullable: false),
                    UserLikesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCocktailsLike", x => new { x.CocktailsLikeId, x.UserLikesId });
                    table.ForeignKey(
                        name: "FK_UserCocktailsLike_Cocktails_CocktailsLikeId",
                        column: x => x.CocktailsLikeId,
                        principalTable: "Cocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCocktailsLike_DbUser_UserLikesId",
                        column: x => x.UserLikesId,
                        principalTable: "DbUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserUser",
                columns: table => new
                {
                    FollowerId = table.Column<int>(type: "int", nullable: false),
                    FollowedId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUser", x => new { x.FollowerId, x.FollowedId });
                    table.ForeignKey(
                        name: "FK_UserUser_DbUser_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "DbUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserUser_DbUser_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "DbUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DbUserHistorySearch_UserId",
                table: "DbUserHistorySearch",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCocktailsLike_UserLikesId",
                table: "UserCocktailsLike",
                column: "UserLikesId");

            migrationBuilder.CreateIndex(
                name: "IX_UserUser_FollowedId",
                table: "UserUser",
                column: "FollowedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbUserHistorySearch");

            migrationBuilder.DropTable(
                name: "UserCocktailsLike");

            migrationBuilder.DropTable(
                name: "UserUser");

            migrationBuilder.DropTable(
                name: "Cocktails");

            migrationBuilder.DropTable(
                name: "DbUser");
        }
    }
}
