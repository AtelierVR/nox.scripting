using System.Linq;
using System.Text;
using Nox.CCK.Utils;
using Nox.Scripting;
using UnityEngine;
using NoxLogger = Nox.CCK.Utils.Logger;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"console"</c> — debug logging.
	/// <code>
	/// import { log, warn, error } from 'console';
	/// log("hello", someValue);
	/// </code>
	/// </summary>
	public static class ConsoleModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("console")
				.WithTags("session")
				.AddMethod("log", (ctx, args) => {
					NoxLogger.Log(
						BuildMessage(args),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.AddMethod("warn", (ctx, args) => {
					NoxLogger.LogWarning(
						BuildMessage(args),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.AddMethod("error", (ctx, args) => {
					NoxLogger.LogError(
						BuildMessage(args),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.Build();

		/// <summary>
		/// Builds the log line without the LINQ pipeline (<c>Select</c> + <c>string.Join</c>
		/// allocate an iterator and two collections per call). Scripts that log every frame sit on
		/// this path, and the single-argument case — by far the most common — allocates nothing extra.
		/// </summary>
		private static string BuildMessage(object[] args) {
			if (args == null || args.Length == 0)
				return string.Empty;
			if (args.Length == 1)
				return Format(args[0]);

			var builder = new StringBuilder();
			for (var i = 0; i < args.Length; i++) {
				if (i > 0)
					builder.Append(' ');
				builder.Append(Format(args[i]));
			}
			return builder.ToString();
		}

		private static string Tag(GameObject obj)
			=> obj != null ? $"Script_{obj.GetId()}" : "Script";

		private static string Format(object arg, int depth = 0) {
			if (arg == null) return "null";
			if (depth > 3)   return arg.ToString();
			if (arg is object[] arr)
				return "[" + string.Join(", ", arr.Select(a => Format(a, depth + 1))) + "]";
			return arg.ToString();
		}
	}
}
