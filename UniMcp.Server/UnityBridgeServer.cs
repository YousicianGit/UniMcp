using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UniMcp.Client;
using UniMcp.Client.Commands;

namespace UniMcp.Server
{
	internal class UnityBridgeServer : IDisposable
	{
		private readonly CancellationTokenSource cancellationTokenSource = new();
		private readonly ILogger logger;
		private readonly TcpListener listener;
		private McpCommand? command;

		public IPEndPoint EndPoint { get; }
		public string Id { get; }

		public UnityBridgeServer(ILogger<UnityBridgeServer> logger)
		{
			this.logger = logger;

			this.listener = new TcpListener(IPAddress.Loopback, 0);
			this.listener.Start();

			this.EndPoint = (IPEndPoint)this.listener.LocalEndpoint;
			this.Id = ServerId.Get();
			this.logger.LogInformation($"Server started on port {this.EndPoint.Port} with ID {this.Id}");

			Task.Run(this.ListenerLoop);
		}

		public void RunCommand(McpCommand command) => this.command = command;

		public void Dispose()
		{
			this.cancellationTokenSource.Cancel();
			this.listener.Stop();
			this.cancellationTokenSource.Dispose();
		}

		private async Task ListenerLoop()
		{
			var cancellationToken = this.cancellationTokenSource.Token;
			while (!cancellationToken.IsCancellationRequested)
			{
				try
				{
					var client = await listener.AcceptTcpClientAsync(cancellationToken);
					SocketUtilities.ConfigureTcpClient(client);

					_ = HandleClientAsync(client, cancellationToken);
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
					this.logger.LogError(e, "Listener error");
				}
			}
		}

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
	        await using var stream = client.GetStream();
	        using var streamReader = new StreamReader(stream,
		        System.Text.Encoding.UTF8,
		        detectEncodingFromByteOrderMarks: false,
		        bufferSize: 1024,
		        leaveOpen: true);

	        await using var streamWriter = new StreamWriter(stream,
				System.Text.Encoding.UTF8,
				bufferSize: 1024,
				leaveOpen: true);

			streamWriter.AutoFlush = true;

			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					if (this.command is { Completed: false })
					{
						var json = JsonConvert.SerializeObject(this.command);
						this.logger.LogInformation($"Sending command:\n{json}");
						await streamWriter.WriteLineAsync(json);

						var response = await streamReader.ReadLineAsync(cancellationToken);
						if (response == null)
						{
							this.logger.LogInformation("Client disconnected");
							break;
						}

						this.logger.LogInformation($"Command completed:\n{response}");
						this.command.Complete(response);
					}
					else
					{
						await Task.Delay(500, cancellationToken);
					}
				}
			}
			catch (OperationCanceledException)
			{
				this.logger.LogInformation("Canceled");
			}
			catch (IOException e) when (e.InnerException is SocketException)
			{
				this.logger.LogInformation($"Disconnected:\n{e.Message}");
			}
			catch (Exception e)
			{
				this.logger.LogError(e, "Client error");
			}
			finally
			{
				client.Dispose();
			}
        }
	}
}
