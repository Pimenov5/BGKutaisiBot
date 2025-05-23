using BGKutaisiBot.Attributes;
using System.Reflection;

namespace BGKutaisiBot.Commands
{
	internal class Help
	{
		public static void Respond()
		{
			IEnumerable<Type> types = typeof(Help).Assembly.GetTypes().Where((Type type) => type.Namespace == typeof(Help).Namespace);
			foreach (Type type in types)
				if (type.GetCustomAttribute<ConsoleCommandAttribute>() is ConsoleCommandAttribute attribute && attribute.Description is string description)
					Console.WriteLine(type.Name.ToLower() + $" - {description}");
			Console.WriteLine();
		}
	}
}