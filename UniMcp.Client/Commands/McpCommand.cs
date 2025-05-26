using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniMcp.Client.Commands
{
	/// <summary>
	/// Base class that represents a command sent to the Model Context Protocol server.
	/// </summary>
	internal abstract partial class McpCommand
	{
		private readonly TaskCompletionSource<string> taskCompletionSource = new();

		[JsonIgnore]
		public Task<string> Result => this.taskCompletionSource.Task;

		[JsonIgnore]
		public bool Completed => this.taskCompletionSource.Task.IsCompleted;

		public abstract string Type { get; }

		/// <summary>
		/// Creates a specific command subclass based on the command type.
		/// </summary>
		/// <param name="jsonString">The JSON string to deserialize from.</param>
		/// <returns>The deserialized command object.</returns>
		public static McpCommand FromJson(string jsonString)
		{
			var json = JObject.Parse(jsonString);
			if (!json.TryGetValue(nameof(Type), out JToken? typeToken))
			{
				throw new InvalidOperationException("Command type is required");
			}

			var type = typeToken.Value<string>();

			return type switch
			{
				RunTestsCommand.TypeConstant => json.ToObject<RunTestsCommand>()!,
				CompileCommand.TypeConstant => json.ToObject<CompileCommand>()!,
				_ => throw new InvalidOperationException($"Unknown command type: {type}"),
			};
		}

		// ReSharper disable once MemberCanBePrivate.Global used by UniMcp.Server
		public void Complete(string result)
		{
#if UNITY_EDITOR
			Unity.UnityCompiler.instance.Reset();
#endif
			this.taskCompletionSource.SetResult(result);
		}

		protected void Complete(object result) => this.Complete(JsonConvert.SerializeObject(result));
	}
}
