using BGKutaisiBot.Types.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BGKutaisiBot.Commands
{
	internal class SendLogs
	{
		public static async Task RespondAsync(ITelegramBotClient botClient, string chatId, CancellationToken cancellationToken)
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
			Message message = await botClient.SendDocumentAsync(chatId, InputFile.FromStream(memoryStream, fileName), cancellationToken: cancellationToken);
			Logs.Instance.Add($"@{message.Chat.Username} получил сообщение (ID {message.MessageId}) с документом:  {fileName}");
		}
	}
}