using Telegram.Bot.Types.Enums;

namespace BGKutaisiBot.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class BotCommandAttribute(string? description, string? instruction, int chatAction = default, BotCommandScopeType scopeType = BotCommandScopeType.AllPrivateChats) : Attribute
	{
		public string? Description = description;
		public string? Instruction = instruction;
		public int ChatAction = chatAction;
		public BotCommandScopeType ScopeType = scopeType;
	}
}