using System.Linq;
using UniMcp.Client.Unity;
using UnityEditor.TestTools.TestRunner.Api;

namespace UniMcp.Client.Commands
{
	/// <summary>
	/// Unity Editor specific functionality for RunTestsCommand.
	/// </summary>
	internal sealed partial class RunTestsCommand
	{
		private UnityTestRunner? editModeRunner;
		private UnityTestRunner? playModeRunner;

		public override void Update()
		{
			var compilerResult = UnityCompiler.instance.Update();
			switch (compilerResult)
			{
				case null:
					// Wait for the code to compile
					return;
				case { Result: UnityCompiler.Outcome.Fail }:
					this.Complete(compilerResult);
					break;
				default:
					this.RunTests();
					break;
			}
		}

		private void RunTests()
		{
			this.editModeRunner ??= new UnityTestRunner(TestMode.EditMode, this.TestFilter);

			var editModeResult = this.editModeRunner.Update();

			if (editModeResult != null)
			{
				this.playModeRunner ??= new UnityTestRunner(TestMode.PlayMode, this.TestFilter);
				var playModeResult = this.playModeRunner.Update();

				if (playModeResult != null)
				{
					var summary = new UnityTestRunner.TestSummary(
						editModeResult.PassedTestCount + playModeResult.PassedTestCount,
						editModeResult.FailedTestCount + playModeResult.FailedTestCount,
						editModeResult.FailedTests.Concat(playModeResult.FailedTests).ToArray()
					);

					if (summary is { PassedTestCount: 0, FailedTestCount: 0 })
					{
						this.Complete("Error: No tests matched the filter. Provide a namespace or class name.");
					}
					else
					{
						this.Complete(summary);
					}
				}
			}
		}
	}
}
