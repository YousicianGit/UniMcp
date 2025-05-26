using ModelContextProtocol.Server;
using System.ComponentModel;
using UniMcp.Client.Commands;

namespace UniMcp.Server.Tools
{
	[McpServerToolType]
	internal static class UnityTestRunner
	{
		[McpServerTool]
		[Description("Run tests")]
		public static async Task<string> RunTests(
			ICommandExecutor executor,
			[Description("Run tests matching this filter (test name or namespace)")] string testFilter)
		{
			var command = new RunTestsCommand(testFilter);
			executor.Run(command);
			return await command.Result;
		}
	}
}

