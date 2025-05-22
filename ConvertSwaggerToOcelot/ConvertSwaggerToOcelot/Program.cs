using System.Text.Json;

string GetValidInput(string prompt)
{
    string? input;
    do
    {
        Console.Write(prompt);
        input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
        {
            Console.WriteLine("Input cannot be empty. Please try again.");
        }
    } while (string.IsNullOrEmpty(input));

    return input;
}

string swaggerFilePath = GetValidInput("Enter Swagger JSON file path: ");
string ocelotFilePath = GetValidInput("Enter Ocelot JSON output file path: ");

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
OcelotConfig ocelotConfig = new([.. swaggerDoc.Paths.Select(p => new OcelotRoute(

    DownstreamPathTemplate: p.Key,
    DownstreamScheme: "http",
    DownstreamHostAndPorts: [new HostPort(Host: "backend-service", Port: 8080)],
    UpstreamPathTemplate: p.Key,
    UpstreamHttpMethod: [.. p.Value.Keys]
))]);

// Write Ocelot JSON
var ocelotJson = JsonSerializer.Serialize(ocelotConfig, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(ocelotFilePath, ocelotJson);

Console.WriteLine($"Ocelot configuration generated successfully! Saved at: {ocelotFilePath}");

// Models
record SwaggerDocument(Dictionary<string, Dictionary<string, object>> Paths);
record OcelotConfig(List<OcelotRoute> Routes);
record OcelotRoute(string DownstreamPathTemplate, string DownstreamScheme, HostPort[] DownstreamHostAndPorts, string UpstreamPathTemplate, string[] UpstreamHttpMethod);
record HostPort(string Host, int Port);
