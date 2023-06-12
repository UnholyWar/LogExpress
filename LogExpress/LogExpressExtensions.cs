using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogExpress
{
    public static class LogExpressExtensions
    {


        public static IServiceCollection AddLogExpress(this IServiceCollection services, Action<LogExpressOptions> configureOptions)
        {
            services.AddSingleton<LogExpress>(provider =>
            {
                var options = new LogExpressOptions();
                configureOptions(options);
                // Oluşturulan options nesnesine erişim
                var connectionString = options.ConnectionString;
                var tableName = options.TableName;
                // LogExpress nesnesini oluşturma ve döndürme
                var requestDelegate = provider.GetRequiredService<RequestDelegate>();
                return new LogExpress(requestDelegate, options);
            });

            return services;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"> </param>
        /// <param name="configuration">
        ///  "LogExpressConnectionConfigurations": {
        /// "tableName": "MyLogExpressTable",
        /// "connectionString": "connectionString"}
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddLogExpress(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<LogExpress>(provider => new LogExpress(provider.GetRequiredService<RequestDelegate>(), configuration));
            return services;
        }



        public static IApplicationBuilder UseLogExpress(this IApplicationBuilder app)
        {
            // LogExpress middleware'i uygulama pipeline'ına ekleme
            app.UseMiddleware<LogExpress>();
            return app;
        }

      
    }
}
