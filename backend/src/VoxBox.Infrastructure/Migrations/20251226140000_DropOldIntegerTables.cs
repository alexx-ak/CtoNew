using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoxBox.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DropOldIntegerTables : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Drop existing tables with integer-based schema if they exist
        migrationBuilder.Sql("IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users");
        migrationBuilder.Sql("IF OBJECT_ID('Tenants', 'U') IS NOT NULL DROP TABLE Tenants");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Down migration would recreate the old integer-based tables
        // This is intentionally left empty since we're migrating to GUIDs permanently
    }
}