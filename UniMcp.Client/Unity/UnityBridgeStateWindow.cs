using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UniMcp.Client.Unity
{
	public class UnityBridgeStateWindow : EditorWindow
	{
		private static readonly Color DisabledColor = new(0.5f, 0.5f, 0.5f);
		private static readonly Color GoodColor = new(0.4f, 0.8f, 0.4f);
		private static readonly Color WarningColor = new(0.9f, 0.9f, 0.3f);
		private static readonly Color ErrorColor = new(0.9f, 0.3f, 0.3f);

		private Vector2 scrollPosition;

		[MenuItem("Tools/UniMcp")]
		public static void ShowWindow()
		{
			GetWindow<UnityBridgeStateWindow>("UniMcp");
		}

		private void OnGUI()
		{
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);

			EditorGUILayout.LabelField("UniMcp Unity Bridge", EditorStyles.boldLabel);

			// Display discovery state
			var discovery = UnityBridgeState.instance.Discovery;
			this.DrawClientSection(null, discovery);

			EditorGUILayout.Space();

			// Display clients
			var clients = UnityBridgeState.instance.Clients;
			if (clients.Any())
			{
				EditorGUILayout.LabelField("Connected MCP Servers", EditorStyles.boldLabel);

				foreach (var client in clients.OrderBy(c => c.Port))
				{
					var name = string.IsNullOrEmpty(client.Name) ? "MCP Server" : client.Name;
					this.DrawClientSection($"{name} ({client.Port})", client);
					EditorGUILayout.Space();
				}
			}
			else
			{
				EditorGUILayout.LabelField("No clients connected", EditorStyles.boldLabel);
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox("The MCP server will appear in this list after running a command", MessageType.Info);
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(500));

			foreach (var configAction in ConfigurationHelper.ConfigActions)
			{
				if (GUILayout.Button(configAction.Key, GUILayout.Height(30)))
				{
					var message = configAction.Value();
					this.ShowNotification(new GUIContent(message), 2);
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndScrollView();
		}

		private void DrawClientSection(string? title, ClientState clientState)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			if (title != null)
			{
				EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
			}

			EditorGUILayout.LabelField("State:", EditorStyles.boldLabel);

			var stateColor = this.GetStateColor(clientState.CurrentState);
			var originalColor = GUI.color;
			GUI.color = stateColor;

			// Draw the state with a colored background
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.LabelField(clientState.CurrentState.ToString(), EditorStyles.boldLabel);
			EditorGUILayout.EndVertical();

			// Restore the original color
			GUI.color = originalColor;

			EditorGUILayout.LabelField("Message:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(clientState.Message, EditorStyles.wordWrappedLabel);

			EditorGUILayout.EndVertical();
		}

		private Color GetStateColor(State state)
		{
			switch (state)
			{
				case State.Connected:
					return GoodColor;
				case State.Warning:
					return WarningColor;
				case State.Error:
					return ErrorColor;
				case State.Disconnected:
					return DisabledColor;
				default:
					throw new NotImplementedException($"State color {state} is not implemented");
			}
		}
	}
}
