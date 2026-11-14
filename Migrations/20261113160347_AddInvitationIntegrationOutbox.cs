using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Foll_Care.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationIntegrationOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integration_outbox_messages",
                schema: "care",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_outbox_messages", x => x.event_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_integration_outbox_messages_next_attempt_at_utc",
                schema: "care",
                table: "integration_outbox_messages",
                column: "next_attempt_at_utc",
                filter: "published_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_integration_outbox_messages_operation_id",
                schema: "care",
                table: "integration_outbox_messages",
                column: "operation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_outbox_messages",
                schema: "care");
        }
    }
}
