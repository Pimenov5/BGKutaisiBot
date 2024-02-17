using BGKutaisiBot.Types;
using System.Text;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Help : BotCommand
	{
		public override TextMessage Respond(string? messageText, out bool finished)
		{
			finished = true;
			string[] commands = messageText?.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

			Type helpType = this.GetType();
			IEnumerable<Type> types = helpType.Assembly.GetTypes().Where((Type type) => type.IsSubclassOf(typeof(BotCommand)) && (commands.Length == 0 || commands.Contains(type.Name.ToLower())));

			StringBuilder stringBuilder = new();
			foreach (Type type in types)
				if (type.GetProperty("Instruction", typeof(string)) is { } propertyInfo && propertyInfo.GetValue(null) is string help)
					stringBuilder.AppendLine($"/{type.Name.ToLower()} {help}\n");

			if (stringBuilder.Length == 0)
				throw new CancelException(CancelException.Cancel.Current, "не удалось найти команды или инструкции к ним" + commands.Length switch
				{
					0 => string.Empty,
					_ => " из списка: " + messageText
				});

			return new TextMessage(stringBuilder.ToString().TrimEnd());
		}

		public static string Description { get => "Подробные инструкции для каждой команды"; }
		public static string Instruction { get => "присылает список команд и их подробные инструкции."
			+ $" Можно указать имена конкретных команд без / через пробел, например:\n/{typeof(Help).Name.ToLower()} collection the7wonders"; }
	}
}