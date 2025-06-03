using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace UniMcp.Client.Unity
{
	internal sealed class UnityTestRunner : ICallbacks
	{
		private static readonly MethodInfo? IsRunActiveMethod = typeof(TestRunnerApi).GetMethod(
			"IsRunActive",
			BindingFlags.NonPublic | BindingFlags.Static);
		private readonly TestRunnerApi testRunner;

		private TestSummary? result;

		public UnityTestRunner(TestMode mode, string testFilter)
		{
			this.testRunner = ScriptableObject.CreateInstance<TestRunnerApi>();
			this.testRunner.RegisterCallbacks(this);

			var filter = new Filter
			{
				testMode = mode,
				groupNames = new[] { testFilter },
			};

			this.testRunner.Execute(new ExecutionSettings(filter));
		}

		public TestSummary? Update()
		{
			// Normally RunFinished will be called before the test run finishes.
			// However, RunFinished will not be called if the test run is canceled by the user.
			// To work around this case, we need to check if the tests have finished and if there is no result report
			// back that they were canceled.
			if (this.result == null && IsTestRunCanceled())
			{
				this.testRunner.UnregisterCallbacks(this);

				this.result = new TestSummary(0, 0, new[] { new TestDetails("TestRun", "Canceled", "Test run was canceled") });
			}

			return this.result;
		}

		private static bool IsTestRunCanceled()
		{
			// Unfortunately this is an internal method that was added in UTF 1.3.5. For earlier versions we assume
			// that the tests are running. This can result in the command hanging when cancelling the tests until
			// the scripts are recompiled.
			if (IsRunActiveMethod == null)
			{
				return false;
			}

			return IsRunActiveMethod.Invoke(null, Array.Empty<object>()) is false;
		}

		public void RunStarted(ITestAdaptor testsToRun)
		{
		}

		public void RunFinished(ITestResultAdaptor result)
		{
			this.testRunner.UnregisterCallbacks(this);

			var failedTests = result.Children
				.SelectMany(GetLeafChildrenWithFailures)
				.Select(r =>
				{
					var message = r.Message;

					if (!string.IsNullOrEmpty(r.StackTrace))
					{
						message += $"\n{r.StackTrace}";
					}

					if (!string.IsNullOrEmpty(r.Output))
					{
						message += $"\n{r.Output}";
					}
					return new TestDetails(r.FullName, r.ResultState, message);
				})
				.ToArray();

			this.result = new TestSummary(result.PassCount, result.FailCount, failedTests);
		}

		public void TestStarted(ITestAdaptor test)
		{
		}

		public void TestFinished(ITestResultAdaptor result)
		{
		}

		private static IEnumerable<ITestResultAdaptor> GetLeafChildrenWithFailures(ITestResultAdaptor testResult)
		{
			if (testResult.Children.Any())
			{
				foreach (var child in testResult.Children.Where(c => c.FailCount > 0))
				{
					foreach (var leafChild in GetLeafChildrenWithFailures(child))
					{
						yield return leafChild;
					}
				}
			}
			else
			{
				yield return testResult;
			}
		}

		public record TestSummary(int PassedTestCount, int FailedTestCount, TestDetails[] FailedTests);

		public record TestDetails(string Name, string Result, string Message);
	}
}
