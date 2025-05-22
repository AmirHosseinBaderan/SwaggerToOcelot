using System.Text.Json;

string swaggerFilePath = "swagger.json"; // Path to Swagger JSON file
string ocelotFilePath = "ocelot.json"; // Output Ocelot config file

if (!File.Exists(swaggerFilePath))
{
    Console.WriteLine("Swagger file not found!");
    return;
}

// Read Swagger JSON
var swaggerJson = File.ReadAllText(swaggerFilePath);
var swaggerDoc = JsonSerializer.Deserialize<SwaggerDocument>(swaggerJson);

if (swaggerDoc?.Paths == null)
{
    Console.WriteLine("Invalid Swagger format!");
    return;
}

// Convert paths to Ocelot routes
var ocelotConfig = new OcelotConfig
{
    Routes = swaggerDoc.Paths.Select(p => new OcelotRoute
    {
        DownstreamPathTemplate = p.Key,
        DownstreamScheme = "http",
        DownstreamHostAndPorts = new[] { new HostPort { Host = "backend-service", Port = 8080 } },
        UpstreamPathTemplate = p.Key,
        UpstreamHttpMethod = p.Value.Keys.ToArray()
    }).ToList()
};

// Write Ocelot JSON
var ocelotJson = JsonSerializer.Serialize(ocelotConfig, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(ocelotFilePath, ocelotJson);

Console.WriteLine("Ocelot configuration generated successfully!");

// Models
record SwaggerDocument(Dictionary<string, Dictionary<string, object>> Paths);
record OcelotConfig(List<OcelotRoute> Routes);
record OcelotRoute(string DownstreamPathTemplate, string DownstreamScheme, HostPort[] DownstreamHostAndPorts, string UpstreamPathTemplate, string[] UpstreamHttpMethod);
record HostPort(string Host, int Port);
