using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PLPServer
{
    public class ServersDocumentFilter(IConfiguration configuration, ILogger<ServersDocumentFilter> logger) : IDocumentFilter
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<ServersDocumentFilter> logger = logger;

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //TODO: make it so that the default configration precedent overrides urls (env var URLS?)
            var serverUrl = _configuration["API_BASE_URLS"] ?? _configuration["urls"];

            logger.LogInformation("Using serverUrls: {}", serverUrl);

            List<OpenApiServer> servers;

            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("no valid env var API_BASE_URLS or applicationUrls availabe!");
            }

            servers = serverUrl.Split(";").Select(i => new OpenApiServer { Url = i }).ToList() ?? [];
            /*servers = [
                    new OpenApiServer { Url = serverUrl },
            ];*/

            swaggerDoc.Servers = servers;
        }
    }
}