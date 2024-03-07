using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Admin : OwnerBotCommand
	{
		bool _isFirst = true;
		public static Func<string, Task>? CommandCallback { get; set; }
		public override TextMessage? Respond(string? messageText, out bool finished)
		{
			if (CommandCallback is null)
				throw new CancelException(CancelException.Cancel.Current, "не инициализировано свойство CommandCallback");

			finished = _isFirst && !string.IsNullOrEmpty(messageText);
			_isFirst = false;
			if (string.IsNullOrEmpty(messageText))
				return new TextMessage("Режим администратора включён");

			CommandCallback(messageText);
			return null;
		}
	}
}