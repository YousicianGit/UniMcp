using System.Net.Sockets;

namespace UniMcp.Client
{
	internal static class SocketUtilities
	{
		public static void ConfigureTcpClient(TcpClient client)
		{
			client.Client.SetSocketOption(
				SocketOptionLevel.Socket,
				SocketOptionName.KeepAlive,
				optionValue: true);
			client.ReceiveTimeout = 60000;
		}

		public static UdpClient CreateUdpBroadcastClient() => new()
		{
			EnableBroadcast = true,
			ExclusiveAddressUse = false,
		};
	}
}
