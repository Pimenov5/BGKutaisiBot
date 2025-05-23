namespace BGKutaisiBot.Attributes
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class ConsoleCommandAttribute(string description) : Attribute
	{
		public string Description = description;
	}
}