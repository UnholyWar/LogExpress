
using LogExpress;

namespace WebApiDeneme
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddMemoryCache();
            builder.Services.AddLogExpress(option =>
            {
                option.ConnectionString = "JSON";
                option.TableName = "MyLogExpressTable";
            }) ;
            var app = builder.Build();
            app.UseHttpsRedirection();
            app.MapControllers();
            app.UseLogExpress();
            app.Run();
        }
    }
}