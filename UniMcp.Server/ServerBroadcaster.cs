using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using UniMcp.Client;
using UniMcp.Client.Commands;

namespace UniMcp.Server
{
	/// <summary>
	/// Broadcasts server information using UDP to enable client discovery
	/// </summary>
	internal class ServerBroadcaster : IDisposable, ICommandExecutor
	{
		private readonly CancellationTokenSource cancellationTokenSource = new();
		private readonly ILogger<ServerBroadcaster> logger;
		private readonly UnityBridgeServer server;
		private readonly UdpClient udpBroadcaster;

		public ServerBroadcaster(ILogger<ServerBroadcaster> logger, UnityBridgeServer server)
		{
			this.logger = logger;
			this.server = server;

			this.udpBroadcaster = SocketUtilities.CreateUdpBroadcastClient();

			Task.Run(this.BroadcastLoop);
		}

		public void Dispose()
		{
			this.cancellationTokenSource.Cancel();
			this.udpBroadcaster.Close();
			this.cancellationTokenSource.Dispose();
		}

		public void Run(McpCommand command)
		{
			this.server.RunCommand(command);
		}

		private async Task BroadcastLoop()
		{
			var broadcastEndPoint = new IPEndPoint(IPAddress.Loopback, ServerDiscovery.DiscoveryPort);
			var cancellationToken = cancellationTokenSource.Token;
			var bytes = Encoding.UTF8.GetBytes(
				$"{ServerDiscovery.MessagePrefix}:{this.server.Id}:{this.server.EndPoint.Port}");

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					await this.udpBroadcaster.SendAsync(bytes, broadcastEndPoint, cancellationToken);

					await Task.Delay(1000, cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (Exception e)
				{
					logger.LogError($"Broadcast error:\n{e}");
					await Task.Delay(5000, cancellationToken);
				}
			}
		}
	}
}
