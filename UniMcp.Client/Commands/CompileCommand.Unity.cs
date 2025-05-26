namespace UniMcp.Client.Commands
{
	internal sealed partial class CompileCommand
	{
		public override void Update()
		{
			var result = Unity.UnityCompiler.instance.Update();

			if (result != null)
			{
				this.Complete(result);
			}
		}
	}
}
