using ModelContextProtocol.Server;
using System.ComponentModel;
using UniMcp.Client.Commands;

namespace UniMcp.Server.Tools
{
	[McpServerToolType]
	internal static class UnityCompile
	{
		[McpServerTool]
		[Description("Compile C# code")]
		public static async Task<string> Compile(ICommandExecutor executor)
		{
			var command = new CompileCommand();
			executor.Run(command);
			return await command.Result;
		}
	}
}
