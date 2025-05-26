# UniMcp

UniMcp enables AI assistants to interact with Unity through a standardized interface.

## Functions

- Run tests with a filter
- Compile the code

## Installation

1. Install [.NET 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run the server.

2. Check out the [UniMcp repository](https://github.com/YousicianGit/UniMcp)

3. Add `UniMcp.Client` to Unity Package Manager from the repository you checked out:
```
"com.yousician.unimcp.client": "file:../../UniMcp/UniMcp.Client",
```

4. In Unity, open Tools > UniMcp and press the configure button of your IDE.

> [!NOTE]
> If your IDE is not listed, you can press "Copy Config to Clipboard" and paste it into your IDE.

5. Start the MCP server in your IDE.
In most IDEs this is done by pressing a "Refresh" button in the MCP server settings.

## Usage

Once configured, the server will be automatically started by your IDE.
The Unity bridge will be started automatically by the Unity Editor.
Now you can ask your favorite AI agent to compile the code or run tests.

## Development

The Unity client is set up to also work without Unity to enable debugging together with the server.

The server is a console application that's based on the [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk). You can interact with it directly by writing messages to STDIN.

### Example messages

List available tools:

```json
{"jsonrpc":"2.0", "id":1, "method":"tools/list"}
```

Run all tests:

```json
{"jsonrpc":"2.0", "id":1, "method":"tools/call", "params": {"name": "RunTests", "arguments": {"testFilter": ""}}}
```

Compile the code:
```json
{"jsonrpc":"2.0", "id":1, "method":"tools/call", "params": {"name": "Compile" }}
```

### Debugging

When debugging the server, the MCP framework will eat all exceptions.
You can put a breakpoint [here](https://github.com/modelcontextprotocol/csharp-sdk/blob/47de86ab91008d0374c94c48eecdf5002d725462/src/ModelContextProtocol/Server/AIFunctionMcpServerTool.cs#L241) to see the issue.

### Logs

The server writes logs to `~/UniMcp.log`.
The client's logs are visible in the UniMcp editor window.

### Adding New Tools

To add new tools to the server:
1. Create a new tool description in `UniMcp.Server.Tools`
2. Implement the appropriate functionality in `UniMcp.Client.Commands`

