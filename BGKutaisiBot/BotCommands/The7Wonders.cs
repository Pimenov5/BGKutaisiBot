using System.Text;
using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BGKutaisiBot.BotCommands
{
	[BotCommand("Рандомайзер планшетов для \"7 Чудес\"", "присылает перемешанный список игроков (от 3 до 7) и их планшетов для игры \"7 чудес\"."
			+ $" Нажатие {KEYBOARD_BUTTON_TEXT} ещё раз перемешивает игроков и их планшеты из общего списка."
			+ $" Имена игроков можно отправлять как в отдельном сообщении, так и в одном с командой, например: /the7wonders {EXAMPLE_NAMES}", (int)ChatAction.Typing)]
	internal class The7Wonders : BotForm
	{
		const string NAMES_DELIMITER = " - ";
		const string KEYBOARD_BUTTON_TEXT = "🔀";
		const string EXAMPLE_NAMES = "Дима Дмитрий Димон";
		readonly static string[] _wondersNames = ["Alexandria", "Babylon", "Ephesos", "Gizah", "Halikarnassos", "Olympia", "Rhodos"];

		static TextMessage GetTextMessage(string[] names)
		{
			Random random = new();
			random.Shuffle<string>(names);
			random.Shuffle<string>(_wondersNames);

			StringBuilder stringBuilder = new();
			for (int i = 0; i < names.Length; i++)
				stringBuilder.AppendLine($"{names[i]}{NAMES_DELIMITER}{_wondersNames[i]}");

			return new TextMessage(stringBuilder.ToString()) { ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton(KEYBOARD_BUTTON_TEXT)
				{ CallbackData = GetCallbackData(typeof(The7Wonders), nameof(The7Wonders.Shuffle)) })
			};
		}

		public static TextMessage Shuffle(string messageText)
		{
			using StringReader stringReader = new(messageText);
			List<string> names = [];
			while (true)
			{
				string? name = stringReader.ReadLine();
				if (string.IsNullOrEmpty(name))
					break;
				names.Add(name.Remove(name.IndexOf(NAMES_DELIMITER)));
			}

			if (names.Count == 0)
				throw new ArgumentException("Не удалось выделить имя игроков", nameof(messageText));

			return GetTextMessage(names.ToArray());
		}

		public TextMessage Respond(string[] args)
		{
			this.IsCompleted = false;
			if (args.Length == 0)
				return new TextMessage("Введите от 3 до 7 имён игроков, разделённых пробелами или переносом строки, например:\n" + EXAMPLE_NAMES);

			if (args.Length < 3 || args.Length > 7)
				return new TextMessage($"\"7 Чудес\" поддерживает 3-7 игроков, введённое вами количество: {args.Length}");

			this.IsCompleted = true;
			return GetTextMessage(args);
		}
	}
}