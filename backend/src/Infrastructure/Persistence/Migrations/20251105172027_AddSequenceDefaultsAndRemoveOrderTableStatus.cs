using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSequenceDefaultsAndRemoveOrderTableStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tables_Seats_Positive",
                table: "Tables");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Tables",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR TableNoSeq",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR OrderNoSeq",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR MenuItemNoSeq",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValueSql: "NEXT VALUE FOR CategoryNoSeq",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tables_Seats_Positive",
                table: "Tables",
                sql: "[Seats] >=1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Tables_Seats_Positive",
                table: "Tables");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Tables",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR TableNoSeq");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR OrderNoSeq");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "MenuItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR MenuItemNoSeq");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "Categories",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValueSql: "NEXT VALUE FOR CategoryNoSeq");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tables_Seats_Positive",
                table: "Tables",
                sql: "[Seats] >= 1");
        }
    }
}
