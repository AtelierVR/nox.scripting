using System;

namespace Nox.Scripting.Extensions {
	/// <summary>
	/// Extension methods for <see cref="IScriptingContext"/> to simplify
	/// type conversion from script values to C# objects.
	/// </summary>
	public static class ScriptingContextExtensions {
		/// <summary>
		/// Converts a script value to a C# object of the requested type.
		/// This is a convenience overload that avoids the need to specify the type as
		/// <see cref="Type"/> when the desired type is known at compile time.
		/// </summary>
		/// <typeparam name="T">The target C# type.</typeparam>
		/// <param name="context">The scripting context.</param>
		/// <param name="scriptValue">The script value to convert.</param>
		/// <returns>The converted object of type <typeparamref name="T"/>.</returns>
		public static T FromScript<T>(this IScriptingContext context, object scriptValue)
			=> (T)context.FromScript(scriptValue, typeof(T));
	}
}
