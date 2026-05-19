using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IronMind.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLastNotifiedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "ReminderSchedules",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNotifiedAt",
                table: "ReminderSchedules");
        }
    }
}
