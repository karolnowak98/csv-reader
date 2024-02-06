using FluentMigrator;

namespace CsvReader.Core.Data.Migrations;

[Migration(20240205096391)]
public class CreateProductsTable : Migration
{
    public override void Up()
    {
        Create.Table("Products")
            .WithColumn("ID").AsString(16).PrimaryKey()
            .WithColumn("SKU").AsString(16).Unique()
            .WithColumn("Name").AsString(255)
            .WithColumn("EAN").AsString(13)
            .WithColumn("ProducerName").AsString(255)
            .WithColumn("Category").AsString(1023)
            .WithColumn("IsWire").AsBoolean()
            .WithColumn("Available").AsBoolean()
            .WithColumn("IsVendor").AsBoolean()
            .WithColumn("DefaultImage").AsString(1023).WithDefaultValue("");
    }

    public override void Down()
    {
        Delete.Table("Products");
    }
}