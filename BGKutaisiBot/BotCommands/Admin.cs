using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Admin : OwnerBotCommand
	{
		public static Action<string>? CommandCallback { get; set; }
		public override TextMessage? Respond(string? messageText, out bool finished)
		{
			finished = false;
			if (CommandCallback is null)
				throw new CancelException(CancelException.Cancel.Current, "не инициализировано свойство CommandCallback");

			if (string.IsNullOrEmpty(messageText))
				return new TextMessage("Режим администратора включён");

			CommandCallback(messageText);
			return null;
		}
	}
}