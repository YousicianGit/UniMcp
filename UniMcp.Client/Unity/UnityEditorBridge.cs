using UnityEditor;

namespace UniMcp.Client.Unity
{
	[InitializeOnLoad]
	internal static class UnityEditorBridge
	{
		private static readonly ServerDiscovery Discovery;

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
