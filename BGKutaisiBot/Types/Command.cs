using System.Reflection;

namespace BGKutaisiBot.Types
{
	internal abstract class Command
	{
		const char CALLBACK_DATA_DELIMITER = '-';
		private protected static string GetCallbackData(Type type, string methodName)
		{
			MethodInfo methodInfo = type.GetMethod(methodName) ?? throw new NullReferenceException($"У типа \"{type.Name} отсутствует метод \"{methodName}\"");
			return $"{type.Name}{CALLBACK_DATA_DELIMITER}{methodInfo.Name}";
		}

		public abstract TextMessage Respond(string? messageText, out bool finished);
		public static bool TryParseCallbackData(string callbackData, out KeyValuePair<string, string>? result)
		{
			string[] stringArray = callbackData.Split(CALLBACK_DATA_DELIMITER);
			result = stringArray.Length == 2 && !string.IsNullOrEmpty(stringArray[0]) && !string.IsNullOrEmpty(stringArray[1]) ? new KeyValuePair<string, string>(stringArray[0], stringArray[1])
				: null;
			return result is not null;
		}
	}
}