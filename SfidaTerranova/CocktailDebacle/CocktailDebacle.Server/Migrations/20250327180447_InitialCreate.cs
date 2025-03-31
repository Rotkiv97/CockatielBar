using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DbUser_DbUser_UserId",
                table: "DbUser");

            migrationBuilder.DropForeignKey(
                name: "FK_DbUser_DbUsers_UsersId",
                table: "DbUser");

            migrationBuilder.DropTable(
                name: "DbRecommenderSystems");

            migrationBuilder.DropTable(
                name: "DbUsers");

            migrationBuilder.DropTable(
                name: "UserCocktailsCreate");

            migrationBuilder.DropTable(
                name: "UserCocktailsLike");

            migrationBuilder.DropTable(
                name: "DbCocktails");

            migrationBuilder.DropIndex(
                name: "IX_DbUser_UserId",
                table: "DbUser");

            migrationBuilder.DropIndex(
                name: "IX_DbUser_UsersId",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "ImgProfile",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "Leanguage",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "Online",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "PersonalizedExperience",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "DbUser");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "DbUser");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "DbUser",
                newName: "PasswordHash");

            migrationBuilder.AlterColumn<bool>(
                name: "AcceptCookies",
                table: "DbUser",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "DbUser",
                newName: "Password");

            migrationBuilder.AlterColumn<bool>(
                name: "AcceptCookies",
                table: "DbUser",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImgProfile",
                table: "DbUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Leanguage",
                table: "DbUser",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Online",
                table: "DbUser",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PersonalizedExperience",
                table: "DbUser",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "DbUser",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsersId",
                table: "DbUser",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                name: "IX_DbUser_UserId",
                table: "DbUser",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DbUser_UsersId",
                table: "DbUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_DbRecommenderSystems_UserId",
                table: "DbRecommenderSystems",
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

            migrationBuilder.AddForeignKey(
                name: "FK_DbUser_DbUser_UserId",
                table: "DbUser",
                column: "UserId",
                principalTable: "DbUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DbUser_DbUsers_UsersId",
                table: "DbUser",
                column: "UsersId",
                principalTable: "DbUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
