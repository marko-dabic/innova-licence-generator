using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnovaTest.LicenceGenerator.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialLicenceActivations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LicenceActivations",
                columns: table => new
                {
                    LicenceIdHash = table.Column<string>(type: "char(64)", nullable: false),
                    LicenceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivatedToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenceActivations", x => x.LicenceIdHash);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenceActivations");
        }
    }
}
