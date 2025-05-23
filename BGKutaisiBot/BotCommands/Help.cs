using BGKutaisiBot.Types;
using System.Text;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Help : BotCommand
	{
		public static TextMessage Respond(string[] args)
		{
			Type helpType = typeof(Help);
			IEnumerable<Type> types = helpType.Assembly.GetTypes().Where((Type type) => type.IsSubclassOf(typeof(BotCommand)) && (args.Length == 0 || args.Contains(type.Name.ToLower())));

			StringBuilder stringBuilder = new();
			foreach (Type type in types)
				if (type.GetProperty("Instruction", typeof(string)) is { } propertyInfo && propertyInfo.GetValue(null) is string help)
					stringBuilder.AppendLine($"/{type.Name.ToLower()} {help}\n");

			if (stringBuilder.Length == 0)
				throw new CancelException(CancelException.Cancel.Current, "не удалось найти команды или инструкции к ним" + args.Length switch
				{
					0 => string.Empty,
					_ => " из списка: " + new StringBuilder(args.Length).AppendJoin(' ', args)
				});

			return new TextMessage(stringBuilder.ToString().TrimEnd(), true);
		}

		public static string Description { get => "Подробные инструкции для каждой команды"; }
		public static string Instruction { get => "присылает список команд и их подробные инструкции."
			+ $" Можно указать имена конкретных команд без / через пробел, например:\n/{typeof(Help).Name.ToLower()} collection the7wonders"; }
	}
}