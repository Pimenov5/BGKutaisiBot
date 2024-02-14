using System.Text;
using BGKutaisiBot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace BGKutaisiBot.Commands
{
	internal class The7Wonders : Command
	{
		const string NAMES_DELIMITER = " - ";
		readonly static string[] _wondersNames = ["Alexandria", "Babylon", "Ephesos", "Gizah", "Halikarnassos", "Olympia", "Rhodos"];

		static TextMessage GetTextMessage(string[] names)
		{
			Random random = new();
			random.Shuffle<string>(names);
			random.Shuffle<string>(_wondersNames);

			StringBuilder stringBuilder = new();
			for (int i = 0; i < names.Length; i++)
				stringBuilder.AppendLine($"{names[i]}{NAMES_DELIMITER}{_wondersNames[i]}");

			const string KEYBOARD_BUTTON_TEXT = "🔀";
			return new TextMessage(stringBuilder.ToString()) { ReplyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton(KEYBOARD_BUTTON_TEXT)
				{ CallbackData = GetCallbackData(typeof(The7Wonders), "Shuffle") })
			};
		}

		public static string Description { get => "Рандомайзер планшетов для \"7 Чудес\""; }
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

			return GetTextMessage(names.ToArray());
		}

		public override TextMessage Respond(string? input, out bool finished)
		{
			finished = false;
			if (string.IsNullOrEmpty(input))
				return new TextMessage("Введите от 3 до 7 имён игроков, разделённых пробелами, например:\nДима Дмитрий Димон");

			string[] names = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (names.Length < 3 || names.Length > 7)
				return new TextMessage($"\"7 Чудес\" поддерживает 3-7 игроков, введённое вами количество: {names.Length}");

			finished = true;
			return GetTextMessage(names);
		}
	}
}