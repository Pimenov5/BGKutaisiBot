using BGKutaisiBot.Attributes;
using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BGKutaisiBot.Commands
{
	[ConsoleCommand("Отправить файл лога")]
	internal class SendLogs
	{
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
		{
			if (Logs.Count == 0)
				throw new InvalidOperationException("Отсутствуют записи в логе");

			using MemoryStream memoryStream = new();
			using StreamWriter streamWriter = new(memoryStream);
			{
				foreach (var item in Logs.ToEnumerable())
					streamWriter.WriteLine(item.ToString());
			}
			streamWriter.Flush();
			memoryStream.Position = 0;

			string fileName = $"{Logs.First.DateTime.ToString("dd MMMM yyyy HH-mm")} — {(Logs.First.DateTime.Date == Logs.Last.DateTime.Date
				? $"{Logs.Last.DateTime.ToString("HH-mm")}" : $"{Logs.Last.DateTime.ToString("dd MMMM yyyy HH-mm")}")}.txt";
			Message message = await botClient.SendDocument(chatId, InputFile.FromStream(memoryStream, fileName), cancellationToken: cancellationToken);
			Logs.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}) с документом:  {fileName}");
		}
	}
}