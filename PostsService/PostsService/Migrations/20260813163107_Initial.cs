using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostsService.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "posts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    caption = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    creation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_messages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    topic_name = table.Column<string>(type: "text", nullable: false),
                    partition = table.Column<int>(type: "integer", nullable: false),
                    offset = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_messages", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "processed_messages");
        }
    }
}
