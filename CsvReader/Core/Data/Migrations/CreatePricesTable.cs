using FluentMigrator;

namespace CsvReader.Core.Data.Migrations;

[Migration(20240205185192)]
public class CreatePricesTable : Migration
{
    public override void Up()
    {
        Create.Table("Prices")
            .WithColumn("ID").AsString(16).PrimaryKey()
            .WithColumn("SKU").AsString(16).Unique()
            .WithColumn("PurchaseNettPrice").AsDecimal().WithDefaultValue(0m);
    }

    public override void Down()
    {
        Delete.Table("Prices");
    }
}