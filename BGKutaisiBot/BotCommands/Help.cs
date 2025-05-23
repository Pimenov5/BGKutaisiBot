using BGKutaisiBot.Types;
using System.Text;
using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Attributes;
using System.Reflection;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Подробные инструкции для каждой команды",
		"присылает список команд и их подробные инструкции. Можно указать имена конкретных команд без / через пробел, например: /help collection the7wonders")]
	internal class Help : BotCommand
	{
		public static TextMessage Respond(string[] args)
		{
			Type helpType = typeof(Help);
			IEnumerable<Type> types = helpType.Assembly.GetTypes().Where((Type type) => type.IsSubclassOf(typeof(BotCommand)) && (args.Length == 0 || args.Contains(type.Name.ToLower())));

			StringBuilder stringBuilder = new();
			foreach (Type type in types)
				if (type.GetCustomAttribute<BotCommandAttribute>() is BotCommandAttribute attribute && attribute.Instruction is string help && !string.IsNullOrEmpty(help))
					stringBuilder.AppendLine($"/{type.Name.ToLower()} {help}\n");

			if (stringBuilder.Length == 0)
				throw new CancelException(CancelException.Cancel.Current, "не удалось найти команды или инструкции к ним" + args.Length switch
				{
					0 => string.Empty,
					_ => " из списка: " + new StringBuilder(args.Length).AppendJoin(' ', args)
				});

			return new TextMessage(stringBuilder.ToString().TrimEnd(), true);
		}
	}
}