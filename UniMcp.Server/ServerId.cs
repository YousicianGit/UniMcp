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
				return match.Groups[1].Value;
			}

			return "Windsurf";
		}

		var cursorEnvVar = Environment.GetEnvironmentVariable("CURSOR_TRACE_ID");
		if (cursorEnvVar != null)
		{
			return "Cursor";
		}

		return Path.GetFileName(Directory.GetCurrentDirectory());
	}
}
