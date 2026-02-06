using System.Threading.Tasks;
using UnityEditor;

namespace UniMcp.Client.Unity
{
	[InitializeOnLoad]
	internal static class UnityEditorBridge
	{
		private static ServerDiscovery Discovery;

		static UnityEditorBridge()
		{
			Discovery = new ServerDiscovery(UnityBridgeState.instance);

			EditorApplication.quitting += () =>
			{
				EditorApplication.update -= Update;
				Discovery.Dispose();
			};
			EditorApplication.update += Update;
		}

		public static async Task Restart()
		{
			Discovery.Dispose();
			await Task.Delay(1000); // Wait a bit for the previous server to dispose
			Discovery = new ServerDiscovery(UnityBridgeState.instance);
		}

		private static void Update()
		{
			foreach (var client in Discovery.Clients)
			{
				if (client.Command is { Completed: false } command)
				{
					command.Update();
				}
			}
		}
	}
}
