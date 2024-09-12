using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Exceptions;
using BGKutaisiBot.Types.Logging;

namespace BGKutaisiBot.BotCommands
{
	internal class Admin : BotCommand, IConsoleCommand
	{
		const byte LOGIN_TRIES_MAX_COUNT = 3;
		bool _isFirst = true;
		static readonly List<long> _admins = [];
		static readonly Dictionary<long, byte> _users = [];

		static bool AddAdmin(long userId) {
			if (Contains(userId))
			{
				Logs.Instance.Add($"Идентификатор \"{userId}\" уже существует в списке администраторов");
				return false;
			}
			else
			{
				_admins.Add(userId);
				Logs.Instance.Add($"Идентификатор \"{userId}\" добавлен в список администраторов");
				return true;
			}
		}
		static bool RemoveAdmin(long userId)
		{
			_users.TryAdd(userId, LOGIN_TRIES_MAX_COUNT); // запрет пользователю авторизации как администратор
			if (Contains(userId))
			{
				_admins.Remove(userId);
				Logs.Instance.Add($"Идентификатор \"{userId}\" удалён из списка администраторов");
				return true;
			}
			else
			{
				Logs.Instance.Add($"Идентификатор \"{userId}\" не существует в списке администраторов");
				return false;
			}
		}

		public static Func<string[], Task>? CommandCallback { get; set; }
		public static bool Contains(long id) => _admins.Contains(id);
		public override TextMessage? Respond(string[] args, out bool finished) => throw new NotImplementedException();
		public override TextMessage? Respond(long chatId, string[] args, out bool finished)
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
					if (args.Length == 0)
						return new TextMessage("Введите пароль для авторизации");
					else if (args.Length == 1 && args[0] == password)
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

			finished = _isFirst && args.Length != 0;
			_isFirst = false;
			if (args.Length == 0)
				return new TextMessage("Режим администратора включён, введите команду");

			if (Environment.GetEnvironmentVariable("ADMIN_CHAT_ID_ALIAS") is string alias && Array.IndexOf(args, alias) is int index && index >= 0)
				args[index] = chatId.ToString();

			CommandCallback(args);
			return null;
		}
		public static void Respond(string action, string strUserId)
		{
			if (action.Length > 1)
				throw new ArgumentException($"\"{action}\" не является символом");
			if (!long.TryParse(strUserId, out long userId))
				throw new ArgumentException($"\"{strUserId}\" не является идентификатором пользователя");

			switch (action[0])
			{
				case '+':
					AddAdmin(userId);
					break;
				case '-':
					RemoveAdmin(userId);
					break;
				default:
					throw new ArgumentException($"\"{action}\" не является символом операции");
			};			
		}
	}
}