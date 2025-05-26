using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace UniMcp.Client.Unity
{
	internal sealed class UnityCompiler : ScriptableSingleton<UnityCompiler>
	{
		public record Summary(Outcome Result, string[] Errors, string[] Warnings);

		[JsonConverter(typeof(StringEnumConverter))]
		public enum Outcome
		{
			Success,
			Fail,
		};

		[SerializeField] private bool compileRequested;
		[SerializeField] private List<string> errors = new();
		[SerializeField] private List<string> warnings = new();

		public Summary? Update()
		{
			if (this.compileRequested)
			{
				if (!EditorApplication.isCompiling)
				{
					CompilationPipeline.assemblyCompilationFinished -= CompilationFinished;
					return new Summary(this.errors.Count > 0 ? Outcome.Fail : Outcome.Success, this.errors.ToArray(), this.warnings.ToArray());
				}
			}
			else
			{
				CompilationPipeline.assemblyCompilationFinished += CompilationFinished;
				this.compileRequested = true;
				CompilationPipeline.RequestScriptCompilation();
			}

			return null;
		}

		/// <summary>
		/// Reset the compiler. A new compile will not be started until this is called.
		/// </summary>
		public void Reset()
		{
			this.compileRequested = false;
			this.errors.Clear();
			this.warnings.Clear();
		}

		private void CompilationFinished(string assembly, CompilerMessage[] messages)
		{
			foreach (var message in messages)
			{
				switch (message.type)
				{
					case CompilerMessageType.Error:
						this.errors.Add(message.message);
						break;
					case CompilerMessageType.Warning:
						this.warnings.Add(message.message);
						break;
				}
			}
		}
	}
}
