using EventAz.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventAz.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261029180000_AddUniqueReviewPerUserEvent")]
public sealed class AddUniqueReviewPerUserEvent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "Reviews",
            type: "nvarchar(450)",
            maxLength: 450,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_Reviews_UserId_EventId",
            table: "Reviews",
            columns: new[] { "UserId", "EventId" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Reviews_UserId_EventId", table: "Reviews");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "Reviews",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)",
            oldMaxLength: 450);
    }
}
