using System.Text.Json;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

var options = ClientOptions.Parse(args);
var componentPath = options.ComponentPath ?? FindDefaultComponent();

if (!File.Exists(componentPath))
{
    throw new FileNotFoundException(
        $"Component not found at '{componentPath}'. Build TextAnalyzer.Component first or pass --component <path>.",
        componentPath);
}

Console.WriteLine(JsonSerializer.Serialize(new
{
    eventName = "starting",
    transport = options.HttpEndpoint is null ? "stdio" : "streamable-http",
    component = componentPath,
}));

var transport = CreateTransport(options);
await using var client = await McpClient.CreateAsync(transport);

var initialTools = await client.ListToolsAsync();
WriteTools("initial-tools", initialTools);

var loadTool = FindTool(initialTools, "load-component");
var componentUri = new Uri(Path.GetFullPath(componentPath)).AbsoluteUri;
var loadResult = await loadTool.CallAsync(new Dictionary<string, object?>
{
    ["path"] = componentUri,
});
WriteResult("load-component", loadResult);
EnsureSuccessful("load-component", loadResult);

var tools = await client.ListToolsAsync();
WriteTools("loaded-tools", tools);
var analyzerTool = tools.FirstOrDefault(static tool =>
    string.Equals(tool.Name, "analyze", StringComparison.OrdinalIgnoreCase))
    ?? tools.FirstOrDefault(static tool =>
        tool.Name.Contains("analy", StringComparison.OrdinalIgnoreCase)
        && !tool.Name.Contains("load-component", StringComparison.OrdinalIgnoreCase))
    ?? throw new InvalidOperationException(
        "Wassette loaded the component but no analyzer tool was discovered.");

var successResult = await analyzerTool.CallAsync(new Dictionary<string, object?>
{
    ["input"] = "  Hello,   world!  \r\nNew line.  ",
});
WriteResult("analyze-success", successResult);
EnsureSuccessful("analyze-success", successResult);
EnsureContains("analyze-success", successResult, "normalized");

var errorResult = await analyzerTool.CallAsync(new Dictionary<string, object?>
{
    ["input"] = "   ",
});
WriteResult("analyze-error", errorResult);
EnsureContains("analyze-error", errorResult, "error", "err");

Console.WriteLine(JsonSerializer.Serialize(new
{
    eventName = "completed",
    analyzerTool = analyzerTool.Name,
    status = "ok",
}));

if (transport is IAsyncDisposable disposableTransport)
{
    await disposableTransport.DisposeAsync();
}

static IClientTransport CreateTransport(ClientOptions options)
{
    if (options.HttpEndpoint is not null)
    {
        return new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(options.HttpEndpoint),
            Name = "Wassette Streamable HTTP",
            TransportMode = HttpTransportMode.StreamableHttp,
        });
    }

    var environment = StdioClientTransportOptions.GetDefaultEnvironmentVariables();
    foreach (var name in new[] { "DOTNET_ROOT", "NUGET_PACKAGES" })
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (value is not null)
        {
            environment[name] = value;
        }
    }

    return new StdioClientTransport(new StdioClientTransportOptions
    {
        Name = "Wassette stdio",
        Command = options.WassetteCommand,
        Arguments = ["run"],
        InheritEnvironmentVariables = false,
        EnvironmentVariables = environment,
        StandardErrorLines = line => Console.Error.WriteLine($"[wassette] {line}"),
    });
}

static McpClientTool FindTool(IList<McpClientTool> tools, string name)
{
    return tools.FirstOrDefault(tool =>
        string.Equals(tool.Name, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new InvalidOperationException(
            $"Wassette did not expose the required '{name}' tool. Discovered: {string.Join(", ", tools.Select(tool => tool.Name))}");
}

static void WriteTools(string eventName, IList<McpClientTool> tools)
{
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        eventName,
        tools = tools.Select(tool => new { name = tool.Name, description = tool.Description }),
    }));
}

static void WriteResult(string eventName, CallToolResult result)
{
    Console.WriteLine(JsonSerializer.Serialize(new
    {
        eventName,
        result,
    }, new JsonSerializerOptions { WriteIndented = true }));
}

static void EnsureSuccessful(string operation, CallToolResult result)
{
    if (result.IsError is true)
    {
        throw new InvalidOperationException($"{operation} returned an MCP error.");
    }
}

static void EnsureContains(string operation, CallToolResult result, params string[] expectedFragments)
{
    var serialized = JsonSerializer.Serialize(result);
    if (!expectedFragments.Any(fragment =>
        serialized.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException(
            $"{operation} did not contain any expected result marker ({string.Join(", ", expectedFragments)}).");
    }
}

static string FindDefaultComponent()
{
    var candidates = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TextAnalyzer.Component", "bin", "Debug", "net10.0", "wasi-wasm", "native", "text-analyzer.wasm"),
        Path.Combine(Environment.CurrentDirectory, "src", "TextAnalyzer.Component", "bin", "Debug", "net10.0", "wasi-wasm", "native", "text-analyzer.wasm"),
    };

    return candidates
        .Select(Path.GetFullPath)
        .FirstOrDefault(File.Exists)
        ?? candidates[1];
}

internal sealed class ClientOptions
{
    public required string WassetteCommand { get; init; }
    public string? ComponentPath { get; init; }
    public string? HttpEndpoint { get; init; }

    public static ClientOptions Parse(string[] args)
    {
        string? componentPath = null;
        string? httpEndpoint = null;
        var wassetteCommand = Environment.GetEnvironmentVariable("WASSETTE_COMMAND") ?? "wassette";

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--component" when ++index < args.Length:
                    componentPath = args[index];
                    break;
                case "--http" when ++index < args.Length:
                    httpEndpoint = args[index];
                    break;
                case "--wassette" when ++index < args.Length:
                    wassetteCommand = args[index];
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"Unknown or incomplete argument '{args[index]}'.");
            }
        }

        return new ClientOptions
        {
            WassetteCommand = wassetteCommand,
            ComponentPath = componentPath,
            HttpEndpoint = httpEndpoint,
        };
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            "Usage: dotnet run --project src/Wassette.McpClient -- [--component path] [--wassette command] [--http endpoint]");
    }
}
