using Microsoft.Extensions.Logging;
using UniMcp.Client;
using UniMcp.Server;

namespace UniMcp.Tests
{
	public class ServerBroadcasterTests
	{
		private ILogger<ServerBroadcaster> broadcasterLogger;
		private UnityBridgeServer server;
		private UnityBridgeState state;
		private ServerBroadcaster? broadcaster;
		private ServerDiscovery? discovery;

		[SetUp]
		public void SetUp()
		{
			var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
			this.broadcasterLogger = loggerFactory.CreateLogger<ServerBroadcaster>();
			var serverLogger = loggerFactory.CreateLogger<UnityBridgeServer>();
			this.server = new UnityBridgeServer(serverLogger);
			this.state = new UnityBridgeState();
		}

		[TearDown]
		public void TearDown()
		{
			this.server.Dispose();
			this.broadcaster?.Dispose();
			this.discovery?.Dispose();
		}

		[Test]
		[Timeout(1000)]
		public async Task Client_IsCreatedFromBroadcast_WhenDiscoveryStartsFirst()
		{
			this.discovery = new ServerDiscovery(this.state);
			this.broadcaster = new ServerBroadcaster(this.broadcasterLogger, this.server);

			// Wait for client to be created
			while (this.state.Clients.Count == 0)
			{
				await Task.Delay(10);
			}
		}

		[Test]
		[Timeout(2000)] // Broadcasting happens every second, so this test will take slightly longer than a second to run
		public async Task Client_IsCreatedFromBroadcast_WhenBroadcastStartsFirst()
		{
			this.broadcaster = new ServerBroadcaster(this.broadcasterLogger, this.server);
			this.discovery = new ServerDiscovery(this.state);

			// Wait for client to be created
			while (this.state.Clients.Count == 0)
			{
				await Task.Delay(10);
			}
		}

		[Test]
		[Timeout(1000)]
		public async Task MultipleServerBroadcasters_DoNotConflict()
		{
			this.broadcaster = new ServerBroadcaster(this.broadcasterLogger, this.server);
			this.discovery = new ServerDiscovery(this.state);
			using var secondBroadcaster = new ServerBroadcaster(this.broadcasterLogger, this.server);

			while (this.state.Clients.Count < 1)
			{
				await Task.Delay(10);
			}
		}
	}
}
