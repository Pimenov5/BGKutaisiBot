using BGKutaisiBot.Types;
using BGKutaisiBot.Types.Logging;

namespace BGKutaisiBot.UI.Commands
{
	internal class ReadConfig : Command
	{
		public const string FILE_NAME = "config.json";
		public ReadConfig() : base("прочитать настройки из файла")
		{
			static Task Function(string[] args) 
			{
				using FileStream fileStream = new(args[0], FileMode.Open);
				Configuration.FromStream(fileStream);
				Logs.Instance.Add($"Конфигурационный файл {args[0]} прочитан", true);
				return Task.CompletedTask;
			}

			this.Add(0, (string[] args) => Function([ReadConfig.FILE_NAME]));
			this.Add(1, Function);
		}
	}
}