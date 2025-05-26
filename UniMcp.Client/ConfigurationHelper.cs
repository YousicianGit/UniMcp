using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMcp.Client
{
	internal static class ConfigurationHelper
	{
		private record ServerConfig(string ConfigDir, string ConfigFileName);

		private static readonly ServerConfig JetBrainsWindsurfConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codeium"),
			"mcp_config.json"
		);

		private static readonly ServerConfig WindsurfConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codeium", "windsurf"),
			"mcp_config.json"
		);

		private static readonly ServerConfig CursorConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cursor"),
			"mcp.json"
		);

		public static readonly Dictionary<string, Func<string>> ConfigActions = new()
		{
			{ "Configure JetBrains Windsurf", () => Configure(JetBrainsWindsurfConfig) },
			{ "Configure Windsurf", () => Configure(WindsurfConfig) },
			{ "Configure Cursor", () => Configure(CursorConfig) },
			{ "Copy Config to Clipboard", CopyConfigToClipboard },
		};

		private static string Configure(ServerConfig config)
		{
			var configPath = Path.Combine(config.ConfigDir, config.ConfigFileName);

			if (!Directory.Exists(config.ConfigDir))
			{
				Directory.CreateDirectory(config.ConfigDir);
			}

			var mcpConfig = File.Exists(configPath)
				? JObject.Parse(File.ReadAllText(configPath))
				: new JObject();

			CreateUniMcpServerConfig(mcpConfig);

			var updatedJson = mcpConfig.ToString(Formatting.Indented);
			File.WriteAllText(configPath, updatedJson);

			return $"✓ Configuration file updated: {configPath}";
		}

		private static void CreateUniMcpServerConfig(JObject config)
		{
			config["mcpServers"] ??= new JObject();

			var mcpServers = (JObject)config["mcpServers"]!;
			mcpServers["UniMcp"] = new JObject
			{
				["command"] = "dotnet",
				["args"] = new JArray { "run", "--project", GetServerPath() },
			};
		}

		private static string GetServerPath([System.Runtime.CompilerServices.CallerFilePath] string fileName = null!)
		{
			var relativePath = Path.Combine(Path.GetDirectoryName(fileName)!, "..", "UniMcp.Server");
			return Path.GetFullPath(relativePath).Replace("\\", "/");
		}

		private static string CopyConfigToClipboard()
		{
			var config = new JObject();
			CreateUniMcpServerConfig(config);
#if UNITY_EDITOR
			UnityEditor.EditorGUIUtility.systemCopyBuffer = config.ToString(Formatting.Indented);
#endif
			return "✓ Config copied to clipboard!";
		}
	}
}
