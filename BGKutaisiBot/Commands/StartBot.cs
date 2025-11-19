using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Запустить бота")]
	internal class StartBot
	{
		public delegate void OnBotStartedHandler(Type type, ITelegramBotClient botClient);
		public static event OnBotStartedHandler? OnBotStartedEvent;

		public static async Task RespondAsync(ITelegramBotClient? telegramBotClient, string botToken, CancellationToken cancellationToken)
		{
			if (telegramBotClient is not null)
				throw new InvalidOperationException("Бот уже был запущен");

			TelegramBotClient botClient = new(botToken);
			if (!await botClient.TestApiAsync(cancellationToken))
				throw new ArgumentException($"Токен {botToken} бота не прошёл проверку API");

			Dictionary<BotCommandScopeType, List<Telegram.Bot.Types.BotCommand>> botCommands = [];
			IEnumerable<Type> types = typeof(StartBot).Assembly.GetTypes().Where((Type type) => type.IsSubclassOf(typeof(Types.BotCommand)));
			foreach (Type type in types)
				if (type.GetCustomAttribute<BotCommandAttribute>() is BotCommandAttribute attribute && !string.IsNullOrEmpty(attribute.Description))
				{
					if (!botCommands.ContainsKey(attribute.ScopeType))
						botCommands.Add(attribute.ScopeType, []);

					botCommands[attribute.ScopeType].Add(new Telegram.Bot.Types.BotCommand() { Command = type.Name.ToLower(), Description = attribute.Description });
				}

			foreach (BotCommandScopeType key in  botCommands.Keys)
				await botClient.SetMyCommandsAsync(botCommands[key], 
					key switch
					{
						BotCommandScopeType.Default => BotCommandScope.Default(),
						BotCommandScopeType.AllPrivateChats => BotCommandScope.AllPrivateChats(),
						BotCommandScopeType.AllGroupChats => BotCommandScope.AllGroupChats(),
						BotCommandScopeType.AllChatAdministrators => BotCommandScope.AllChatAdministrators(),
						_ => throw new InvalidCastException("Не поддерживаемый тип области команды бота: " + key.ToString())
					}, cancellationToken: cancellationToken);

			botClient.StartReceiving(new TelegramUpdateHandler(), new ReceiverOptions { AllowedUpdates = [] }, cancellationToken);

			User user = await botClient.GetMeAsync(cancellationToken);
			Logs.Instance.Add($"@{user.Username} запущен", true);
			OnBotStartedEvent?.Invoke(typeof(StartBot), botClient);
		}
		public static async Task RespondAsync(ITelegramBotClient? botClient, CancellationToken cancellationToken)
		{
			string botToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? throw new NullReferenceException("В переменных окружения отсутствует значение токена бота");
			await RespondAsync(botClient, botToken, cancellationToken);
		}
	}
}