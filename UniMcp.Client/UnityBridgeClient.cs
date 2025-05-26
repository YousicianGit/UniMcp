using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UniMcp.Client.Commands;

namespace UniMcp.Client
{
	internal sealed class UnityBridgeClient : IDisposable
	{
		private readonly CancellationTokenSource cancellationTokenSource = new();
		private readonly ClientState state;

		public int Port { get; }

		public McpCommand? Command { get; private set; }

		public UnityBridgeClient(int port, ClientState state)
		{
			this.Port = port;
			this.state = state;
			Task.Run(this.ClientLoop);
		}

		public void Dispose()
		{
			this.cancellationTokenSource.Cancel();
			this.cancellationTokenSource.Dispose();
		}

		private async Task ClientLoop()
		{
			var cancellationToken = this.cancellationTokenSource.Token;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					using var client = new TcpClient();
					SocketUtilities.ConfigureTcpClient(client);

					this.state.UpdateState($"Connecting to port {this.Port}...", State.Disconnected);
					await client.ConnectAsync(IPAddress.Loopback, this.Port);
					this.state.UpdateState($"Connected to port {this.Port}", State.Connected);

					using var stream = client.GetStream();
					using var streamReader = new StreamReader(stream,
						System.Text.Encoding.UTF8,
						detectEncodingFromByteOrderMarks: false,
						bufferSize: 1024,
						leaveOpen: true);

					using var streamWriter = new StreamWriter(stream,
						System.Text.Encoding.UTF8,
						bufferSize: 1024,
						leaveOpen: true);

					streamWriter.AutoFlush = true;

					while (!cancellationToken.IsCancellationRequested)
					{
						this.state.UpdateState("Waiting for command...", State.Connected);
						var jsonContent = await streamReader.ReadLineAsync();
						if (jsonContent == null)
						{
							// Server disconnected
							this.state.UpdateState("Disconnected", State.Disconnected);
							break;
						}

						// Parse JSON and use custom deserialization
						var command = McpCommand.FromJson(jsonContent);

						this.Command = command;

						this.state.UpdateState("Executing command...", State.Connected);
						var response = await command.Result;
						await streamWriter.WriteLineAsync(response);
					}
				}
				catch (OperationCanceledException)
				{
					this.state.UpdateState("Canceled", State.Disconnected);
					break;
				}
				catch (ObjectDisposedException)
				{
					this.state.UpdateState("Disposed", State.Disconnected);
					break;
				}
				catch (IOException e) when (e.InnerException is SocketException)
				{
					this.state.UpdateState(e.Message, State.Disconnected);
				}
				catch (SocketException e)
				{
					if (e.SocketErrorCode == SocketError.AddressAlreadyInUse)
					{
						this.state.UpdateState($"Port {this.Port} already in use", State.Error);
					}
					else if (e.SocketErrorCode == SocketError.ConnectionRefused)
					{
						this.state.UpdateState($"Could not find MCP server at port {this.Port}", State.Disconnected);
					}
					else if (e.SocketErrorCode == SocketError.OperationAborted)
					{
						break;
					}
					else
					{
						this.state.UpdateState($"Socket error: {e.SocketErrorCode}", State.Warning);
					}

					await Task.Delay(1000, cancellationToken);
				}
				catch (Exception e)
				{
					this.state.UpdateState(e.ToString(), State.Error);
				}
			}
		}
	}
}
