using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UniMcp.Server;
using Karambolo.Extensions.Logging.File;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(options =>
{
	// Configure all logs to go to stderr to separate it from stdio,
	// which is used to talk to MCP clients
	options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Logging.AddFile(options =>
{
	options.RootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
	options.TextBuilder = new ServerIdLogTextBuilder();
	options.Files =
	[
		new LogFileOptions
		{
			Path = "UniMcp.log",
		},
	];
});

builder.Services.AddSingleton<UnityBridgeServer>();
builder.Services.AddSingleton<ServerBroadcaster>();
builder.Services.AddSingleton<ICommandExecutor, ServerBroadcaster>();

builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithToolsFromAssembly();

await builder.Build().RunAsync();
