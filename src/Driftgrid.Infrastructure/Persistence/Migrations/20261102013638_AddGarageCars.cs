using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Driftgrid.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "garage_cars",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    make = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    model = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    horsepower = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_garage_cars", x => x.id);
                    table.ForeignKey(
                        name: "FK_garage_cars_pilot_profiles_owner_id",
                        column: x => x.owner_id,
                        principalTable: "pilot_profiles",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_garage_cars_owner_id_created_at_utc",
                table: "garage_cars",
                columns: new[] { "owner_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "garage_cars");
        }
    }
}
