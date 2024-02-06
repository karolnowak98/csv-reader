using CsvReader.Utils;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SqlServerConnection") 
                       ?? throw new ApplicationException("Sql server connection string is not valid!");

EnsureDatabaseCreated(connectionString);

builder.Services.AddDependencies(connectionString);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb.AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    migrationRunner.MigrateUp();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
return;

void EnsureDatabaseCreated(string connString)
{
    var connectionBuilder = new SqlConnectionStringBuilder(connString);
    var databaseName = connectionBuilder.InitialCatalog;

    connectionBuilder.InitialCatalog = "master";

    using var connection = new SqlConnection(connectionBuilder.ConnectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = $"IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = '{databaseName}') CREATE DATABASE {databaseName}";
    command.ExecuteNonQuery();
}