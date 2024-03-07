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

				using MemoryStream memoryStream = new();
				using StreamWriter streamWriter = new(memoryStream);
				{
					foreach (var item in Logs.Instance.ToEnumerable())
						streamWriter.WriteLine(item.ToString());
				}
				streamWriter.Flush();
				memoryStream.Position = 0;

				string fileName = $"{Logs.Instance.First.DateTime.ToString("dd MMMM yyyy HH-mm")} — {(Logs.Instance.First.DateTime.Date == Logs.Instance.Last.DateTime.Date
					? $"{Logs.Instance.Last.DateTime.ToString("HH-mm")}" : $"{Logs.Instance.Last.DateTime.ToString("dd MMMM yyyy HH-mm")}")}.txt";
				Message message = await this.BotClient.SendDocumentAsync(args[0], InputFile.FromStream(memoryStream, fileName));
				Logs.Instance.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}) с документом:  {fileName}");
			}

			this.Add(1, Function);
		}
	}
}