using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;

namespace BGKutaisiBot.BotCommands
{
	internal class Admin : BotCommand
	{
		const byte LOGIN_TRIES_MAX_COUNT = 3;
		bool _isFirst = true;
		static readonly List<long> _admins = [];
		static readonly Dictionary<long, byte> _users = [];

		public static Func<string, Task>? CommandCallback { get; set; }
		public static bool Contains(long id) => _admins.Contains(id);
		public override TextMessage? Respond(string? messageText, out bool finished) => throw new NotImplementedException();
		public override TextMessage? Respond(long chatId, string? messageText, out bool finished)
		{
			if (CommandCallback is null)
				throw new CancelException(CancelException.Cancel.Current, "не инициализировано свойство CommandCallback");
			if (Environment.GetEnvironmentVariable("BOT_ADMIN_PASSWORD") is not string password)
				throw new CancelException(CancelException.Cancel.Current, "в переменных окружения отсутствует пароль администратора");

			if (!Contains(chatId))
			{
				if (!_users.ContainsKey(chatId))
					_users.Add(chatId, 0);

				if (_users[chatId] >= LOGIN_TRIES_MAX_COUNT)
				{
					finished = true;
					return new TextMessage("Вы превысили количество попыток авторизации");
				}
				else
				{
					finished = false;
					if (string.IsNullOrEmpty(messageText))
						return new TextMessage("Введите пароль для авторизации");
					else if (messageText == password)
					{
						_isFirst = false;
						_admins.Add(chatId);
						_users.Remove(chatId);
						return new TextMessage("Вы успешно авторизовались, введите команду");
					}
					else
					{
						++_users[chatId];
						if (_users[chatId] >= LOGIN_TRIES_MAX_COUNT)
							finished = true;
						return new TextMessage($"Неверный пароль, осталось попыток: {LOGIN_TRIES_MAX_COUNT - _users[chatId]}"); 
					}
				}
			}

			finished = _isFirst && !string.IsNullOrEmpty(messageText);
			_isFirst = false;
			if (string.IsNullOrEmpty(messageText))
				return new TextMessage("Режим администратора включён, введите команду");

			if (Environment.GetEnvironmentVariable("ADMIN_CHAT_ID_ALIAS") is string alias && messageText.Contains(alias))
				messageText = messageText.Replace(alias, chatId.ToString());

			CommandCallback(messageText);
			return null;
		}
	}
}