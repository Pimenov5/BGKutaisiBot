using BGKutaisiBot.UI.Commands;
using Telegram.Bot;

namespace BGKutaisiBot.UI
{
	internal abstract class Command
	{
		readonly Dictionary<uint, Func<string[], Task>> _delegates = [];

		private protected void Add(uint argsCount, Func<string[], Task> func) => _delegates.Add(argsCount, func);

		public readonly string Description;
		public Command(string description) => this.Description = description;

		public async Task<bool> TryExecuteAsync(string[] args)
		{
			_delegates.TryGetValue((uint)args.Length, out Func<string[], Task>? func);
			if (func is null)
				return false;

			await func.Invoke(args);
			return true;
		}
	}

	internal class AllCommands : Command
	{
		readonly Dictionary<string, Command> _commands = [];
		void Add(Command command) => _commands.Add(command.GetType().Name.ToLower(), command);

		public AllCommands(Func<ITelegramBotClient?> getBotClient, Action<ITelegramBotClient> onBotStarted) : base("список команд")
		{
			this.Add(0, (args) =>
			{
				foreach (var item in _commands)
					Console.WriteLine($"{item.Key} - {item.Value.Description}");
				Console.WriteLine();
				return Task.CompletedTask;
			});

			_commands.Add("help", this);
			this.Add(new Exit());
			CancellationTokenSource? cancellationTokenSource = null;
			this.Add(new StartBot(getBotClient, (botClient, newCancellationTokenSource) =>
			{
				cancellationTokenSource = newCancellationTokenSource;
				onBotStarted.Invoke(botClient);
			}));
			this.Add(new CancelBot(() => cancellationTokenSource));
			this.Add(new SendPoll(getBotClient));
			this.Add(new SendNotice(getBotClient));
			this.Add(new DeleteMessage(getBotClient));
			this.Add(new ReadConfig());
		}
		public bool ContainsCommand(string commandName) => _commands.ContainsKey(commandName.ToLower());
		public async Task<bool> TryExecuteAsync(string commandName, string[] args) => _commands.TryGetValue(commandName.ToLower(), out Command? command) && await command.TryExecuteAsync(args);
	}
}