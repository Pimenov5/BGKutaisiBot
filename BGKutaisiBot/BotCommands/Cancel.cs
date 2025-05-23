using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Отменить текущую команду", "отменяет выполнение текущей команды (ожидающей ответное сообщение от пользователя)")]
	internal class Cancel : BotCommand
	{
		public static TextMessage Respond() => throw new CancelException(CancelException.Cancel.Previous);
	}
}