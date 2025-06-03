using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMcp.Client
{
	internal static class ConfigurationHelper
	{
		private const string DefaultConfigRoot = "mcpServers";

		private record ServerConfig(string ConfigDir, string ConfigFileName, string JsonRoot = DefaultConfigRoot);

		private static readonly ServerConfig CopilotConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "github-copilot", "intellij"),
			"mcp.json",
			"servers"
		);

		private static readonly ServerConfig CursorConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cursor"),
			"mcp.json"
		);

		private static readonly ServerConfig JetBrainsWindsurfConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codeium"),
			"mcp_config.json"
		);

		private static readonly ServerConfig WindsurfConfig = new(
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".codeium", "windsurf"),
			"mcp_config.json"
		);

		public static readonly Dictionary<string, Func<string>> ConfigActions = new()
		{
			{ "Configure Copilot (JetBrains)", () => Configure(CopilotConfig) },
			{ "Configure Cursor", () => Configure(CursorConfig) },
			{ "Configure Windsurf", () => Configure(WindsurfConfig) },
			{ "Configure Windsurf (JetBrains)", () => Configure(JetBrainsWindsurfConfig) },
			{ "Copy Config to Clipboard", CopyConfigToClipboard },
		};

		private static string Configure(ServerConfig config)
		{
			var configPath = Path.Combine(config.ConfigDir, config.ConfigFileName);

			if (!Directory.Exists(config.ConfigDir))
			{
				Directory.CreateDirectory(config.ConfigDir);
			}

			JObject mcpConfig;
			if (File.Exists(configPath))
			{
				try
				{
					mcpConfig = JObject.Parse(File.ReadAllText(configPath));
				}
				catch (JsonException)
				{
					mcpConfig = new JObject();
				}
			}
			else
			{
				mcpConfig = new JObject();
			}

			CreateUniMcpServerConfig(mcpConfig, config.JsonRoot);

			var updatedJson = mcpConfig.ToString(Formatting.Indented);
			File.WriteAllText(configPath, updatedJson);

			return $"✓ Configuration file updated: {configPath}";
		}

		private static void CreateUniMcpServerConfig(JObject config, string root)
		{
			config[root] ??= new JObject();

			var mcpServers = (JObject)config[root]!;
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
			CreateUniMcpServerConfig(config, DefaultConfigRoot);
#if UNITY_EDITOR
			UnityEditor.EditorGUIUtility.systemCopyBuffer = config.ToString(Formatting.Indented);
#endif
			return "✓ Config copied to clipboard!";
		}
	}
}
