using FluentAssertions;
using Microsoft.Extensions.Logging;
using UniMcp.Client;
using UniMcp.Client.Commands;
using UniMcp.Server;

namespace UniMcp.Tests
{
	public class UnityBridgeTests
	{
		private ILogger<UnityBridgeServer> serverLogger;
		private ClientState state;
		private UnityBridgeServer? server;
		private UnityBridgeClient? client;

		[SetUp]
		public void SetUp()
		{
			var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			this.serverLogger = loggerFactory.CreateLogger<UnityBridgeServer>();
			this.state = new ClientState();

		}

		[TearDown]
		public void TearDown()
		{
			this.client?.Dispose();
			this.server?.Dispose();
		}

		[Test]
		[Timeout(1000)]
		public async Task Command_ShouldBeSentToClient()
		{
			this.server = new UnityBridgeServer(this.serverLogger);
			this.client = new UnityBridgeClient(this.server.EndPoint.Port, this.state);

			var command = new CompileCommand();
			this.server.RunCommand(command);

			while (this.client.Command == null)
			{
				await Task.Delay(10);
			}

			this.client.Command.Should().NotBeNull();
		}

		[Test]
		[Timeout(1000)]
		public async Task Command_ShouldCompleteWithResult()
		{
			this.server = new UnityBridgeServer(this.serverLogger);
			this.client = new UnityBridgeClient(this.server.EndPoint.Port, this.state);

			var command = new CompileCommand();
			this.server.RunCommand(command);

			// Wait for command to be received by client
			while (this.client.Command == null)
			{
				await Task.Delay(10);
			}

			var expectedResult = "Compilation successful";
			this.client.Command.Complete(expectedResult);

			var result = await command.Result;
			result.Should().Be(expectedResult);
		}
	}
}
