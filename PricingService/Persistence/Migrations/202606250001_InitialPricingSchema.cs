using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace PricingService.Persistence.Migrations;

[DbContext(typeof(PricingDbContext))]
[Migration("202606250001_InitialPricingSchema")]
public sealed class InitialPricingSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Tariffs",
            columns: table => new
            {
                Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Tariffs", tariff => tariff.Code));

        migrationBuilder.CreateTable(
            name: "BasePremiumRules",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TariffCode = table.Column<string>(type: "character varying(64)", nullable: false),
                CoverCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                Condition = table.Column<string>(type: "text", nullable: true),
                Expression = table.Column<string>(type: "text", nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BasePremiumRules", rule => rule.Id);
                table.ForeignKey(
                    name: "FK_BasePremiumRules_Tariffs_TariffCode",
                    column: rule => rule.TariffCode,
                    principalTable: "Tariffs",
                    principalColumn: "Code",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DiscountMarkupRules",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TariffCode = table.Column<string>(type: "character varying(64)", nullable: false),
                Condition = table.Column<string>(type: "text", nullable: false),
                Multiplier = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DiscountMarkupRules", rule => rule.Id);
                table.ForeignKey(
                    name: "FK_DiscountMarkupRules_Tariffs_TariffCode",
                    column: rule => rule.TariffCode,
                    principalTable: "Tariffs",
                    principalColumn: "Code",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_BasePremiumRules_TariffCode_CoverCode_Priority",
            table: "BasePremiumRules",
            columns: new[] { "TariffCode", "CoverCode", "Priority" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_DiscountMarkupRules_TariffCode_Priority",
            table: "DiscountMarkupRules",
            columns: new[] { "TariffCode", "Priority" },
            unique: true);

        migrationBuilder.InsertData(
            table: "Tariffs",
            columns: new[] { "Code" },
            values: new object[] { "TRI" });

        migrationBuilder.InsertData(
            table: "BasePremiumRules",
            columns: new[] { "Id", "TariffCode", "CoverCode", "Condition", "Expression", "Priority" },
            values: new object?[,]
            {
                { 1, "TRI", "C1", (string?)null, "(NUM_OF_ADULTS) * (DESTINATION == \"EUR\" ? 26.00M : 34.00M)", 0 },
                { 2, "TRI", "C2", (string?)null, "(NUM_OF_ADULTS + NUM_OF_CHILDREN) * 26.00M", 0 },
                { 3, "TRI", "C3", (string?)null, "(NUM_OF_ADULTS + NUM_OF_CHILDREN) * 10.00M", 0 }
            });

        migrationBuilder.InsertData(
            table: "DiscountMarkupRules",
            columns: new[] { "Id", "TariffCode", "Condition", "Multiplier", "Priority" },
            values: new object[] { 1, "TRI", "DESTINATION == \"WORLD\"", 1.5m, 0 });

        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"BasePremiumRules\"', 'Id'), (SELECT MAX(\"Id\") FROM \"BasePremiumRules\"));");
        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"DiscountMarkupRules\"', 'Id'), (SELECT MAX(\"Id\") FROM \"DiscountMarkupRules\"));");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "BasePremiumRules");
        migrationBuilder.DropTable(name: "DiscountMarkupRules");
        migrationBuilder.DropTable(name: "Tariffs");
    }
}
