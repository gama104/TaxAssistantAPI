using Microsoft.EntityFrameworkCore;
using IRSAssistantAPI.Infrastructure.Data;

namespace IRSAssistantAPI;

public class TestDatabaseConnection
{
    public static async Task TestConnection()
    {
        var connectionString = "Server=tcp:irsassistant-demo-server.database.windows.net,1433;Initial Catalog=irsassistant-demo-db;User Id=irsadmin;Password=9%44WyUuM#Jw;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=true;";
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new ApplicationDbContext(options);
        
        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"Database connection: {(canConnect ? "✅ SUCCESS" : "❌ FAILED")}");
            
            if (canConnect)
            {
                var tables = await context.Database.GetDbConnection().GetSchemaAsync("Tables");
                Console.WriteLine($"Tables found: {tables.Rows.Count}");
                foreach (System.Data.DataRow row in tables.Rows)
                {
                    Console.WriteLine($"- {row["TABLE_NAME"]}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database connection failed: {ex.Message}");
        }
    }
}
