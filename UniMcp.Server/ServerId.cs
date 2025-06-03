using System.Text.RegularExpressions;

namespace UniMcp.Server;

public static class ServerId
{
	public static string Get()
	{
		var codeiumEnvVar = Environment.GetEnvironmentVariable("CODEIUM_EDITOR_APP_ROOT");
		if (codeiumEnvVar != null)
		{
			codeiumEnvVar = codeiumEnvVar.Replace("\\", "/");
			var match = Regex.Match(codeiumEnvVar, "([^/]+)/plugins/codeium");
			if (match is { Success: true, Groups.Count: > 1 })
			{
				return $"Windsurf {match.Groups[1].Value}";
			}

			return "Windsurf";
		}

		var cursorEnvVar = Environment.GetEnvironmentVariable("CURSOR_TRACE_ID");
		if (cursorEnvVar != null)
		{
			return "Cursor";
		}

		var copilotEnvVar = Environment.GetEnvironmentVariable("PKG_EXECPATH");
		if (copilotEnvVar != null)
		{
			copilotEnvVar = copilotEnvVar.Replace("\\", "/");
			var match = Regex.Match(copilotEnvVar, "([^/]+)/plugins/github-copilot-intellij");
			if (match is { Success: true, Groups.Count: > 1 })
			{
				return $"Copilot {match.Groups[1].Value}";
			}
		}

		var vsCodeEnvVar = Environment.GetEnvironmentVariable("VSCODE_CWD");
		if (vsCodeEnvVar != null)
		{
			return "Visual Studio Code";
		}

		return Path.GetFileName(Directory.GetCurrentDirectory());
	}
}
