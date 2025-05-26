namespace UniMcp.Client.Commands
{
	internal sealed partial class CompileCommand : McpCommand
	{
		public const string TypeConstant = "compile";

		public override string Type => TypeConstant;
	}
}
