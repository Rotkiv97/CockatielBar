using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cocktails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailConfirmed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordConfirmed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonalizedExperience = table.Column<bool>(type: "bit", nullable: false),
                    AcceptCookis = table.Column<bool>(type: "bit", nullable: false),
                    online = table.Column<bool>(type: "bit", nullable: false),
                    Leanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImgProfile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RecommenderSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommenderSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommenderSystems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCocktailsCreate",
                columns: table => new
                {
                    CocktailsCreateId = table.Column<int>(type: "int", nullable: false),
                    User1Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCocktailsCreate", x => new { x.CocktailsCreateId, x.User1Id });
                    table.ForeignKey(
                        name: "FK_UserCocktailsCreate_Cocktails_CocktailsCreateId",
                        column: x => x.CocktailsCreateId,
                        principalTable: "Cocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCocktailsCreate_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCocktailsLike",
                columns: table => new
                {
                    CocktailsLikeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCocktailsLike", x => new { x.CocktailsLikeId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserCocktailsLike_Cocktails_CocktailsLikeId",
                        column: x => x.CocktailsLikeId,
                        principalTable: "Cocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCocktailsLike_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecommenderSystems_UserId",
                table: "RecommenderSystems",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCocktailsCreate_User1Id",
                table: "UserCocktailsCreate",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserCocktailsLike_UserId",
                table: "UserCocktailsLike",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserId",
                table: "Users",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecommenderSystems");

            migrationBuilder.DropTable(
                name: "UserCocktailsCreate");

            migrationBuilder.DropTable(
                name: "UserCocktailsLike");

            migrationBuilder.DropTable(
                name: "Cocktails");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
