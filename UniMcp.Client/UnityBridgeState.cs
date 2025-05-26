using System.Collections.Generic;
using System.Linq;

namespace UniMcp.Client
{
	internal class UnityBridgeState
#if UNITY_EDITOR
		: UnityEditor.ScriptableSingleton<UnityBridgeState>
#endif
	{
		public List<ClientState> Clients { get; } = new();
		public ClientState Discovery { get; private set; } = new();

		public ClientState GetClient(int port)
		{
			var client = this.Clients.FirstOrDefault(c => c.Port == port);

			if (client == null)
			{
				client = new ClientState
				{
					Port = port,
				};

				this.Clients.Add(client);
			}

			return client;
		}
	}
}
