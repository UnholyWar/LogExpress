using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using static LogExpress.LogExpressExtensions;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace LogExpress
{
    public class LogExpress
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private readonly LogExpressOptions _action;
        private readonly string _connectionString;
        private readonly string _tableName;
        public LogExpress(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _config = configuration;
            var connectionConfigurations = configuration.GetSection("LogExpressConnectionConfigurations");
            _connectionString = connectionConfigurations.GetSection("connectionString").Value;
            _tableName = connectionConfigurations.GetSection("tableName").Value;
        }
        public LogExpress(RequestDelegate next, LogExpressOptions action)
        {
            _next = next;
            _tableName = action.TableName;
            _connectionString = action.ConnectionString;

        }




        public async Task<bool> IsEndPointNull(HttpContext context)
        {
            var endpointFeature = context.Features.Get<IEndpointFeature>();
            var endpoint = endpointFeature?.Endpoint;
            if (endpoint != null)
                return true;
            return false;
        }
        public async Task<string> CheckParameters(HttpContext context)
        {
            StringBuilder resultBuilder = new StringBuilder();

            if (context.Request.HasFormContentType)
            {
                var form = await context.Request.ReadFormAsync();
                foreach (var formParam in form)
                {
                    if (formParam.Value.Count > 0 && formParam.Value[0] != null)
                    {
                        string paramName = formParam.Key;
                        string paramValue = formParam.Value[0].ToString();
                        resultBuilder.AppendLine($"{paramName}: {paramValue}");
                    }
                }
            }
            else if (context.Request.Query.Any())
            {
                var queryParams = context.Request.Query;
                foreach (var queryParam in queryParams)
                {
                    if (queryParam.Value.Count > 0 && queryParam.Value[0] != null)
                    {
                        string paramName = queryParam.Key;
                        string paramValue = queryParam.Value[0].ToString();
                        resultBuilder.AppendLine($"{paramName}: {paramValue}");
                    }
                }
            }
            else if (context.GetRouteData().Values.Any())
            {
                var routeData = context.GetRouteData().Values;
                foreach (var routeParam in routeData)
                {
                    if (routeParam.Value != null)
                    {
                        string paramName = routeParam.Key;
                        string paramValue = routeParam.Value.ToString();
                        resultBuilder.AppendLine($"{paramName}: {paramValue}");
                    }
                }
            }
            else
            {
                // Hiçbir parametre verisi bulunmuyor
            }

            string result = resultBuilder.ToString();
            return result;
        }
        public async Task<LogExAttribute> HasAttribute(HttpContext context)
        {
            var endpointFeature = context.Features.Get<IEndpointFeature>();
            var endpoint = endpointFeature?.Endpoint;
            var metadata = endpoint.Metadata;

            return metadata.OfType<LogExAttribute>().FirstOrDefault();
        }

        public async Task DbRouter(LogExpressConnectionConfigurations log)
        {
            switch (_connectionString.ToUpper())
            {
                case "JSON":
                    await JsonSave(log); break;
                default:
                    break;
            }
        }
        public async Task JsonSave(LogExpressConnectionConfigurations log)
        {
            string logExpFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "LogExp");
            if (!Directory.Exists(logExpFolderPath))
                Directory.CreateDirectory(logExpFolderPath);
            string logFileName = $"LogExpress.json";
            string logFilePath = Path.Combine(logExpFolderPath, logFileName);
            if (!File.Exists(logFilePath))
                File.WriteAllText(logFilePath, "[]");
            string logJsonString = File.ReadAllText(logFilePath);
            List<LogExpressConnectionConfigurations> logList = JsonConvert.DeserializeObject<List<LogExpressConnectionConfigurations>>(logJsonString);
            if (logList == null)
            {
                logList = new List<LogExpressConnectionConfigurations>();
                logList.Add(log);
                var updatedLogJsonString = JsonConvert.SerializeObject(logList, Formatting.Indented);
                File.WriteAllText(logFilePath, updatedLogJsonString);
            }
            else
            {
                // En son objenin Id'sini bul
                int lastId = logList.LastOrDefault()?.Id ?? 0;
                // Yeni log nesnesini oluştur ve Id değerini bir fazlasıyla ayarla
                log.Id = lastId + 1;
                // Yeni log nesnesini logList'e ekle
                logList.Add(log);
                // Listeyi JSON formatına dönüştür ve dosyaya yaz
                var updatedLogJsonString = JsonConvert.SerializeObject(logList, Formatting.Indented);
                File.WriteAllText(logFilePath, updatedLogJsonString);
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (await IsEndPointNull(context))
            {
                LogExpressConnectionConfigurations logExpressConnectionConfigurations = new LogExpressConnectionConfigurations();
                var isAttribute = await HasAttribute(context);

                if (context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value == null)
                {
                    logExpressConnectionConfigurations.UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                }
                else if (context.Session.Id != null)
                {
                    logExpressConnectionConfigurations.UserId = context.Session.Id;
                }
                else
                {
                    logExpressConnectionConfigurations.UserId = "Unauthorized";
                }

                //logExpressConnectionConfigurations.LogFullTime = isAttribute.Time;
                logExpressConnectionConfigurations.LogFullTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                logExpressConnectionConfigurations.Parameters = await CheckParameters(context);
                logExpressConnectionConfigurations.HttpMethod = context.Request.Method;
                logExpressConnectionConfigurations.EndPointName = context.Features.Get<IEndpointFeature>().Endpoint.DisplayName;
                logExpressConnectionConfigurations.Response = context.Response.ContentType;
                await DbRouter(logExpressConnectionConfigurations);


            }

            await _next(context);
        }
    }

}

