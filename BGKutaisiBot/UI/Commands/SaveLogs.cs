using BGKutaisiBot.Types.Logging;

namespace BGKutaisiBot.UI.Commands
{
	internal class SaveLogs : Command
	{
		public SaveLogs() : base("сохранить лог в файл")
		{
			static Task Function(string[] args)
			{
				if (Logs.Instance.Count == 0)
					throw new InvalidOperationException("Отсутствуют записи в логе");

				if (args.Length == 0)
					args = [($"Logs\\{Logs.Instance.First.DateTime.ToString("dd MMMM yyyy HH-mm")} — {(Logs.Instance.First.DateTime.Date == Logs.Instance.Last.DateTime.Date
						? $"{Logs.Instance.Last.DateTime.ToString("HH-mm")}" : $"{Logs.Instance.Last.DateTime.ToString("dd MMMM yyyy HH-mm")}")}.txt")];

				if (!Directory.Exists(args[0]))
					Directory.CreateDirectory(Path.GetDirectoryName(args[0]) ?? throw new NullReferenceException($"Не удалось выделить имя директории из \"{args[0]}\""));

				using FileStream fileStream = new(args[0], FileMode.CreateNew, FileAccess.Write);
				using StreamWriter streamWriter = new(fileStream);
				foreach (var item in Logs.Instance.ToEnumerable())
					streamWriter.WriteLine(item.ToString());

				Logs.Instance.Add($"Лог сохранён в файл {args[0]}", true);
				return Task.CompletedTask;
			}

			this.Add(0, Function);
			this.Add(1, Function);
		}
	}
}