using System.Text;
using Karambolo.Extensions.Logging.File;
using Microsoft.Extensions.Logging;

namespace UniMcp.Server;

internal class ServerIdLogTextBuilder : FileLogEntryTextBuilder
{
	private readonly string serverIdPrefix = $"[{ServerId.Get()}] ";

	protected override void AppendLogLevel(StringBuilder sb, LogLevel logLevel)
	{
		sb.Append(this.serverIdPrefix);
		base.AppendLogLevel(sb, logLevel);
	}
}
