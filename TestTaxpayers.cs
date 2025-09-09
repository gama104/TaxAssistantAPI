using Microsoft.EntityFrameworkCore;
using IRSAssistantAPI.Infrastructure.Data;

namespace IRSAssistantAPI;

public class TestTaxpayers
{
    public static async Task TestDatabase()
    {
        var connectionString = "Server=tcp:irsassistant-demo-server.database.windows.net,1433;Initial Catalog=Irs;User Id=irsadmin;Password=9%44WyUuM#Jw;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;MultipleActiveResultSets=true;";
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new ApplicationDbContext(options);
        
        try
        {
            Console.WriteLine("Testing database connection...");
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"Can connect: {canConnect}");
            
            if (canConnect)
            {
                var count = await context.Taxpayers.CountAsync();
                Console.WriteLine($"Total taxpayers: {count}");
                
                var taxpayers = await context.Taxpayers.Take(3).ToListAsync();
                Console.WriteLine($"Retrieved {taxpayers.Count} taxpayers:");
                
                foreach (var taxpayer in taxpayers)
                {
                    Console.WriteLine($"- {taxpayer.FirstName} {taxpayer.LastName} (ID: {taxpayer.Id}, IsActive: {taxpayer.IsActive})");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
