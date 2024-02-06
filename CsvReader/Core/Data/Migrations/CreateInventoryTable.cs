using FluentMigrator;

namespace CsvReader.Core.Data.Migrations;

[Migration(20240205096693)]
public class CreateInventoryTable : Migration
{
    public override void Up()
    {
        Create.Table("Inventory")
            .WithColumn("ProductID").AsString(16).ForeignKey("Products", "ID")
            .WithColumn("SKU").AsString(16).Unique()
            .WithColumn("Unit").AsString(50).WithDefaultValue("szt.")
            .WithColumn("Qty").AsDecimal().WithDefaultValue(0m)
            .WithColumn("ShippingCost").AsDecimal().WithDefaultValue(0m);
    }

    public override void Down()
    {
        Delete.Table("Inventory");
    }
}