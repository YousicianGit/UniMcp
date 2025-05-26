namespace UniMcp.Client.Commands
{
	internal abstract partial class McpCommand
	{
		/// <summary>
		/// Execute the command.
		/// </summary>
		/// <remarks>
		/// To enable multi-frame commands, Unity will repeatedly call this function from
		/// <see cref="UnityEditor.EditorApplication.update"/> until <see cref="Complete"/> is called.
		/// </remarks>
		public abstract void Update();
	}
}
