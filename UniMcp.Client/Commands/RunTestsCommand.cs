namespace UniMcp.Client.Commands
{
	internal sealed partial class RunTestsCommand : McpCommand
	{
		public const string TypeConstant = "run_tests";

		public string TestFilter { get; }

		public override string Type => TypeConstant;

		public RunTestsCommand(string testFilter)
		{
			this.TestFilter = testFilter;
		}
	}
}
