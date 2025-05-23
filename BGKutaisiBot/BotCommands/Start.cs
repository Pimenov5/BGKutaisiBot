using BGKutaisiBot.Types;
using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.BotCommands
{
	internal class Start : BotCommand
	{
		public override TextMessage Respond(string[] args) => new("Здравствуйте, вас приветствует бот\\-помощник для канала [BGK](t.me/bg\\_kutaisi)", true) { ParseMode = ParseMode.MarkdownV2};
	}
}