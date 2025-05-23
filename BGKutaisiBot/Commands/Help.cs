namespace BGKutaisiBot.Commands
{
	internal class Help
	{
		public static void Respond()
		{
			IEnumerable<Type> types = typeof(Help).Assembly.GetTypes().Where((Type type) => type.Namespace == typeof(Help).Namespace);
			foreach (Type type in types)
				if (type.GetProperty("Description", typeof(string))?.GetValue(null) is string description)
					Console.WriteLine(type.Name.ToLower() + $" - {description}");
			Console.WriteLine();
		}
	}
}