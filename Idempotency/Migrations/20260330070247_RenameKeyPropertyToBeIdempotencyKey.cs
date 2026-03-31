using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Idempotency.Migrations
{
    /// <inheritdoc />
    public partial class RenameKeyPropertyToBeIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Key",
                table: "IdempotencyRecords",
                newName: "IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_IdempotencyRecords_Key",
                table: "IdempotencyRecords",
                newName: "IX_IdempotencyRecords_IdempotencyKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdempotencyKey",
                table: "IdempotencyRecords",
                newName: "Key");

            migrationBuilder.RenameIndex(
                name: "IX_IdempotencyRecords_IdempotencyKey",
                table: "IdempotencyRecords",
                newName: "IX_IdempotencyRecords_Key");
        }
    }
}
