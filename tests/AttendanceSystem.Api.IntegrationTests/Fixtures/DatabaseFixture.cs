using System.Diagnostics.CodeAnalysis;
using AttendanceSystem.Api.IntegrationTests.Fixtures;
using AttendanceSystem.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

[assembly: AssemblyFixture(typeof(DatabaseFixture))]

namespace AttendanceSystem.Api.IntegrationTests.Fixtures;

/// <summary>
/// One SQL Server container for the whole test run, built from the same scripts as docker compose
/// (without the demo data). Tests seed their own rows, so they never share or reset data.
/// </summary>
public sealed class DatabaseFixture : IAsyncLifetime
{
    public const string Image = "mcr.microsoft.com/mssql/server:2022-CU27-GDR1-ubuntu-22.04";
    public const string DatabaseName = "Attendance";

    private static readonly string[] AttendanceScripts =
        ["legacy-schema.sql", "01-missing-tables.sql", "02-integrity.sql", "03-reference-data.sql"];

    private readonly MsSqlContainer _container = new MsSqlBuilder(Image).Build();

    /// <summary>Connection string to the initialized <c>Attendance</c> database.</summary>
    public string ConnectionString { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _container.StartAsync(cancellationToken);

        var masterConnectionString = _container.GetConnectionString();
        ConnectionString = new SqlConnectionStringBuilder(masterConnectionString) { InitialCatalog = DatabaseName }.ConnectionString;

        await RunScriptAsync(masterConnectionString, "00-create-database.sql", cancellationToken);
        foreach (var script in AttendanceScripts)
        {
            await RunScriptAsync(ConnectionString, script, cancellationToken);
        }
    }

    /// <summary>A new context on the test database; the caller disposes it.</summary>
    public AttendanceDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AttendanceDbContext>().UseSqlServer(ConnectionString).Options);

    public ValueTask DisposeAsync() => _container.DisposeAsync();

    [SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Runs the repository's own schema scripts; no user input.")]
    private static async Task RunScriptAsync(string connectionString, string fileName, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "DatabaseScripts", fileName);
        var script = await File.ReadAllTextAsync(path, cancellationToken);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        foreach (var batch in SqlScript.SplitBatches(script))
        {
            await using var command = new SqlCommand(batch, connection);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
