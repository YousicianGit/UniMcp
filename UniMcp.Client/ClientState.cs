using System;
using System.Runtime.CompilerServices;

namespace UniMcp.Client
{
	[Serializable]
	internal class ClientState
	{
		public string Name { get; set; } = string.Empty;
		public int Port { get; set; }
		public string Message { get; private set; } = "Not running";
		public State CurrentState { get; private set; }

		public virtual void UpdateState(string message, State state, [CallerFilePath] string? callerFilePath = null)
		{
			this.Message = message;
			this.CurrentState = state;

			if (state == State.Error)
			{
				var callerClassName = string.IsNullOrEmpty(callerFilePath)
					? "Unknown"
					: System.IO.Path.GetFileNameWithoutExtension(callerFilePath);

				var messageWithCaller = $"[{callerClassName}] {message}";

#if UNITY_EDITOR
				UnityEngine.Debug.LogError(messageWithCaller);
#else
				Console.Error.WriteLine(messageWithCaller);
#endif
			}
		}
	}
}
