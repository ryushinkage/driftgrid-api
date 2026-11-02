using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Driftgrid.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pilot_profiles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    callsign = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pilot_profiles", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_pilot_profiles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            
            migrationBuilder.Sql(
                """
                INSERT INTO pilot_profiles
                    (user_id, callsign, bio,
                     created_at_utc, updated_at_utc)
                SELECT
                    u."Id",
                    'Pilot-' || LEFT(
                        REPLACE(u."Id"::text, '-', ''), 8),
                    NULL,
                    NOW(),
                    NOW()
                FROM "AspNetUsers" u
                INNER JOIN "AspNetUserRoles" ur
                    ON u."Id" = ur."UserId"
                INNER JOIN "AspNetRoles" r
                    ON ur."RoleId" = r."Id"
                WHERE r."NormalizedName" = 'PILOT'
                ON CONFLICT (user_id) DO NOTHING;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pilot_profiles");
        }
    }
}
