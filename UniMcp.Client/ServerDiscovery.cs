using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniMcp.Client
{
	/// <summary>
	/// Handles discovery of Unity bridge servers using UDP broadcasts and starts clients when it finds one.
	/// </summary>
	internal class ServerDiscovery : IDisposable
	{
		/// <summary>
		/// The port used for UDP discovery broadcasts
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global used from UniMcp.Server
		public const int DiscoveryPort = 64627;

		// ReSharper disable once MemberCanBePrivate.Global used from UniMcp.Server
		public const string MessagePrefix = "UniMcp";

		private readonly CancellationTokenSource cancellationTokenSource = new();
		private readonly Dictionary<int, UnityBridgeClient> clients = new();
		private readonly UdpClient udpClient;
		private readonly UnityBridgeState state;

		public IEnumerable<UnityBridgeClient> Clients => this.clients.Values.ToArray();

		public ServerDiscovery(UnityBridgeState state)
		{
			this.state = state;

			this.udpClient = SocketUtilities.CreateUdpBroadcastClient();
			this.udpClient.Client.Bind(new IPEndPoint(IPAddress.Loopback, DiscoveryPort));

			Task.Run(this.DiscoveryLoop);
		}

		public void Dispose()
		{
			this.cancellationTokenSource.Cancel();
			this.udpClient.Close();
			this.cancellationTokenSource.Dispose();

			foreach (var kvp in this.clients)
			{
				kvp.Value.Dispose();
			}
		}

		private async Task DiscoveryLoop()
		{
			var cancellationToken = this.cancellationTokenSource.Token;
			this.state.Discovery.UpdateState("Server discovery started", State.Disconnected);

			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var result = await this.udpClient.ReceiveAsync();
					var message = Encoding.UTF8.GetString(result.Buffer);

					// Parse the message format: "UniMcp:[server-id]:[port]"
					if (message.StartsWith(MessagePrefix))
					{
						var parts = message.Split(':');
						if (parts.Length == 3 && int.TryParse(parts[2], out var port))
						{
							var clientState = this.state.GetClient(port);
							clientState.Name = parts[1];

							if (!this.clients.ContainsKey(port))
							{
								this.state.Discovery.UpdateState($"Discovered MCP server on port {port}",
									State.Connected);
								var client = new UnityBridgeClient(port, clientState);
								this.clients.Add(port, client);
							}
						}
						else
						{
							this.state.Discovery.UpdateState($"Unsupported message:\n{message}", State.Warning);
						}
					}
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (SocketException e) when (e.SocketErrorCode == SocketError.OperationAborted)
				{
					break;
				}
				catch (Exception e)
				{
					if (!this.cancellationTokenSource.IsCancellationRequested)
					{
						this.state.Discovery.UpdateState($"Discovery error:\n{e}", State.Error);
						await Task.Delay(5000, cancellationToken);
					}
				}
			}

			this.state.Discovery.UpdateState("Server discovery stopped", State.Disconnected);
		}
	}
}
