using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JobHunter.Tests;

// Chay toan bo pipeline that (Program.cs) qua TestServer, tro toi
// JobHunterDB_Test (SQL Server that trong Docker, KHONG dung SQLite —
// can test dung CHECK constraint theo dung CLAUDE.md). DB nay duoc
// tao/seed bang scripts/setup-test-db.ps1, chay 1 lan truoc khi test.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost,1433;Database=JobHunterDB_Test;User Id=sa;Password=REDACTED_LOCAL_DEV_PASSWORD;TrustServerCertificate=True;",
            });
        });
    }
}
