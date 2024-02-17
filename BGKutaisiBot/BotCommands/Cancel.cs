using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Cancel : BotCommand
	{
		public static string Description { get => "Отменить текущую команду"; }
		public static string Instruction { get => "отменяет выполнение текущей команды (ожидающей ответное сообщение от пользователя)"; }
		public override TextMessage Respond(string? messageText, out bool finished) => throw new CancelException(CancelException.Cancel.Previous);
	}
}