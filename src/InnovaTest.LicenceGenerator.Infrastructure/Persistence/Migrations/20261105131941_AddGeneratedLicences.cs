using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovaTest.LicenceGenerator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedLicences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneratedLicences",
                columns: table => new
                {
                    LicenceId = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LicenceType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Licence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedLicences", x => x.LicenceId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeneratedLicences");
        }
    }
}
