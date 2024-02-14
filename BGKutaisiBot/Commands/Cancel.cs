using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.Commands
{
	internal class Cancel : Command
	{
		public static string Description { get => "Отменить текущую команду"; }
		public override TextMessage Respond(string? messageText, out bool finished) => throw new CancelException(CancelException.Cancel.Previous);
	}
}