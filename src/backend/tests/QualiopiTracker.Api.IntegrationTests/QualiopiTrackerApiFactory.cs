namespace QualiopiTracker.Api.IntegrationTests;

/// <summary>Runs the API against its own throwaway SQLite file.</summary>
public class QualiopiTrackerApiFactory : WebApplicationFactory<Program>
{
    public static readonly string TestSigningKey =
        Convert.ToBase64String(Enumerable.Range(1, 32).Select(i => (byte)i).ToArray());

    private readonly string _databasePath =
        Path.Combine(Path.GetTempPath(), "qualiopi-tracker-tests", $"{Guid.NewGuid():N}.db");

    protected virtual string EnvironmentName => "Development";

    protected virtual bool SeedDemoData => false;

    protected virtual string SigningKey => TestSigningKey;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Database:SeedDemoData", SeedDemoData.ToString());
        builder.UseSetting("Jwt:SigningKey", SigningKey);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }
}

public sealed class DemoDataApiFactory : QualiopiTrackerApiFactory
{
    protected override bool SeedDemoData => true;
}

public sealed class ProductionDemoDataApiFactory : QualiopiTrackerApiFactory
{
    protected override string EnvironmentName => "Production";

    protected override bool SeedDemoData => true;
}

public sealed class MissingSigningKeyApiFactory : QualiopiTrackerApiFactory
{
    protected override string SigningKey => string.Empty;
}
