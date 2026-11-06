using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualiopiTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderIndicators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProviderIndicators",
                columns: table => new
                {
                    ProviderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IndicatorNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    NotApplicableReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderIndicators", x => new { x.ProviderId, x.IndicatorNumber });
                    table.ForeignKey(
                        name: "FK_ProviderIndicators_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderIndicators");
        }
    }
}
