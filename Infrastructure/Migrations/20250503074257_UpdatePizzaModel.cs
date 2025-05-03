using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePizzaModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Pizzas",
                newName: "PhotoPath");

            migrationBuilder.AddColumn<string>(
                name: "Description_Ingredients",
                table: "Pizzas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_Text",
                table: "Pizzas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description_Weight",
                table: "Pizzas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description_Ingredients",
                table: "Pizzas");

            migrationBuilder.DropColumn(
                name: "Description_Text",
                table: "Pizzas");

            migrationBuilder.DropColumn(
                name: "Description_Weight",
                table: "Pizzas");

            migrationBuilder.RenameColumn(
                name: "PhotoPath",
                table: "Pizzas",
                newName: "Description");
        }
    }
}
