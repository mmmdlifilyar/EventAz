using EventAz.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventAz.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261029170000_AddPromoCodeIsActive")]
public sealed class AddPromoCodeIsActive : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_PromoCodes_Code", table: "PromoCodes");

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            table: "PromoCodes",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");

        migrationBuilder.CreateIndex(
            name: "IX_PromoCodes_Code",
            table: "PromoCodes",
            column: "Code",
            unique: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "PromoCodes",
            type: "bit",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsActive", table: "PromoCodes");
        migrationBuilder.DropIndex(name: "IX_PromoCodes_Code", table: "PromoCodes");

        migrationBuilder.AlterColumn<string>(
            name: "Code",
            table: "PromoCodes",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.CreateIndex(
            name: "IX_PromoCodes_Code",
            table: "PromoCodes",
            column: "Code",
            unique: true);
    }
}
