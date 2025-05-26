using UniMcp.Client.Commands;

namespace UniMcp.Server;

internal interface ICommandExecutor
{
	void Run(McpCommand command);
}
