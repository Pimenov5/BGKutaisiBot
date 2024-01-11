using System.Text.Json;
using Telegram.Bot.Types;

namespace BGKutaisiBot.Types
{
	internal class Configuration
	{
		static JsonSerializerOptions _serializerOptions = new() { IncludeFields = true };
		static Configuration? _instance;

		public readonly string BotToken;
		public readonly long? TestChatId;
		public readonly string? TestChatIdAlias;
		public readonly Polling Poll;
		public readonly Notification Notice;

		internal class Polling
		{
			public readonly char IgnoreChar;
			public readonly int CollectionId;

			public Polling(char ignoreChar, int collectionId)
			{
				this.IgnoreChar = ignoreChar;
				this.CollectionId = collectionId;
			}
		}

		internal class Notification
		{
			public readonly int FaqMessageId;
			public Notification(int faqMessageId) => this.FaqMessageId = faqMessageId;
		}

		public Configuration(string botToken, long? testChatId, string? testChatIdAlias, Polling poll, Notification notice)
		{
			this.BotToken = botToken;
			this.TestChatId = testChatId;
			this.TestChatIdAlias = testChatIdAlias;
			this.Poll = poll;
			this.Notice = notice;
		}

		public static Configuration Instance { get { return _instance ??  throw new NullReferenceException("Конфигурационный файл ещё не был загружен"); } } 
		public static void FromStream(Stream utf8Json)
		{
			_instance = JsonSerializer.Deserialize<Configuration>(utf8Json, _serializerOptions)
				?? throw new NullReferenceException($"Не удалось десериализовать файл");
		}
	}
}