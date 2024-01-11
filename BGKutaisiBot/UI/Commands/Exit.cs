namespace BGKutaisiBot.UI.Commands
{
	internal class Exit : Command
	{
		public Exit() : base("закрыть программу") => this.Add(0, (string[] args) => throw new Types.ExitException());
	}
}