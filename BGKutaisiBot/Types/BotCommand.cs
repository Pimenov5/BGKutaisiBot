using System.Reflection;
using System.Text.RegularExpressions;

namespace BGKutaisiBot.Types
{
	internal abstract class BotCommand : Command
	{
		const char CLASS_METHOD_DELIMITER = '.';
		const char METHOD_ARGS_DELIMITER = '(';

		private protected static string GetCallbackData(Type type, string methodName, string[]? args = null)
		{
			MethodInfo methodInfo = type.GetMethod(methodName) ?? throw new NullReferenceException($"У типа \"{type.Name} отсутствует метод \"{methodName}\"");
			return type.Name + CLASS_METHOD_DELIMITER
				+ $"{methodInfo.Name}{(args is null ? string.Empty : $"({string.Concat(args.ToList().ConvertAll<string>((string arg) => arg + (args.Last() == arg ? string.Empty : ",")))})")}";
		}

		public virtual bool IsLong { get => false; }
		public abstract TextMessage? Respond(string[] args);
		public virtual TextMessage? Respond(long chatId, string[] args) => Respond(args);
		public static bool TryParseCallbackData(string callbackData, out string? typeName, out string? methodName, out string[]? args)
		{
			int index = callbackData.IndexOf(CLASS_METHOD_DELIMITER);
			typeName = index < 0 ? null : callbackData.Remove(index);
			methodName = typeName is null ? null : callbackData.Remove(0, index + 1);
			index = methodName is null ? -1 : methodName.IndexOf(METHOD_ARGS_DELIMITER);
			if (index > 0 && !string.IsNullOrEmpty(methodName))
			{
				string argsString = methodName[index..];
				methodName = methodName.Remove(index);

				Regex regex = new("\\w+");
				MatchCollection matches = regex.Matches(argsString);
				args = matches.Count == 0 ? null : new string[matches.Count];
				for (int i = 0; i < args?.Length; i++)
					args[i] = matches[i].Value;
			}
			else
				args = null;

			return typeName is not null && methodName is not null;
		}
	}
}