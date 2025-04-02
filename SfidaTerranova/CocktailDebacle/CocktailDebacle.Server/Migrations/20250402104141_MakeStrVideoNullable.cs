using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocktailDebacle.Server.Migrations
{
    /// <inheritdoc />
    public partial class MakeStrVideoNullable : Migration
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
                    IdDrink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrDrink = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrDrinkAlternate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrVideo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIBA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrAlcoholic = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrGlass = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsES = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsDE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsFR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsIT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsZH_HANS = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrInstructionsZH_HANT = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrDrinkThumb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient6 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient7 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient8 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient9 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient10 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient11 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient12 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient13 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient14 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrIngredient15 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure5 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure6 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure7 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure8 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure9 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure10 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure11 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure12 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure13 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure14 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrMeasure15 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrImageSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrImageAttribution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrCreativeCommonsConfirmed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateModified = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbUser", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cocktails");

            migrationBuilder.DropTable(
                name: "DbUser");
        }
    }
}
