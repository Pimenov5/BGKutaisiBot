using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BGKutaisiBot.UI.Commands
{
	internal class SendLogs : BotCommand
	{
		public SendLogs(Func<ITelegramBotClient?> getBotClient) : base("сохранить лог в файл", getBotClient)
		{
			async Task Function(string[] args)
			{
				if (Logs.Instance.Count == 0)
					throw new InvalidOperationException("Отсутствуют записи в логе");

				if (args.Length == 1) {
					Array.Resize(ref args, 2);
					args[1] = $"Logs\\{Logs.Instance.First.DateTime.ToString("dd MMMM yyyy HH-mm")} — {(Logs.Instance.First.DateTime.Date == Logs.Instance.Last.DateTime.Date
						? $"{Logs.Instance.Last.DateTime.ToString("HH-mm")}" : $"{Logs.Instance.Last.DateTime.ToString("dd MMMM yyyy HH-mm")}")}.txt";
				}

				string path = args[1];
				if (!Directory.Exists(path))
					Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new NullReferenceException($"Не удалось выделить имя директории из \"{path}\""));

				using (StreamWriter streamWriter = new(path))
				{
					foreach (var item in Logs.Instance.ToEnumerable())
						streamWriter.WriteLine(item.ToString());
				}

				Logs.Instance.Add($"Лог сохранён в файл {path}", true);

				FileStream fileStream = new(path, FileMode.Open);
				Message message = await this.BotClient.SendDocumentAsync(args[0], InputFile.FromStream(fileStream, path));
				Logs.Instance.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}) с документом:  {path}");
			}

			this.Add(1, Function);
			this.Add(2, Function);
		}
	}
}