using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class Adioc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbCocktails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbCocktails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUsers", x => x.Id);
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
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false),
                    PersonalizedExperience = table.Column<bool>(type: "bit", nullable: false),
                    AcceptCookis = table.Column<bool>(type: "bit", nullable: false),
                    Online = table.Column<bool>(type: "bit", nullable: false),
                    Leanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ImgProfile = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbUser_DbUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DbUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DbUser_DbUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "DbUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DbRecommenderSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbRecommenderSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbRecommenderSystems_DbUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DbUser",
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
                        name: "FK_UserCocktailsCreate_DbCocktails_CocktailsCreateId",
                        column: x => x.CocktailsCreateId,
                        principalTable: "DbCocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCocktailsCreate_DbUser_User1Id",
                        column: x => x.User1Id,
                        principalTable: "DbUser",
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
                        name: "FK_UserCocktailsLike_DbCocktails_CocktailsLikeId",
                        column: x => x.CocktailsLikeId,
                        principalTable: "DbCocktails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCocktailsLike_DbUser_UserId",
                        column: x => x.UserId,
                        principalTable: "DbUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DbRecommenderSystems_UserId",
                table: "DbRecommenderSystems",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DbUser_UserId",
                table: "DbUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DbUser_UsersId",
                table: "DbUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCocktailsCreate_User1Id",
                table: "UserCocktailsCreate",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserCocktailsLike_UserId",
                table: "UserCocktailsLike",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DbRecommenderSystems");

            migrationBuilder.DropTable(
                name: "UserCocktailsCreate");

            migrationBuilder.DropTable(
                name: "UserCocktailsLike");

            migrationBuilder.DropTable(
                name: "DbCocktails");

            migrationBuilder.DropTable(
                name: "DbUser");

            migrationBuilder.DropTable(
                name: "DbUsers");
        }
    }
}
