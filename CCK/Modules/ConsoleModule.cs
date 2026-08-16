using System.Linq;
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
						string.Join(" ", args.Select(Format)),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.AddMethod("warn", (ctx, args) => {
					NoxLogger.LogWarning(
						string.Join(" ", args.Select(Format)),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.AddMethod("error", (ctx, args) => {
					NoxLogger.LogError(
						string.Join(" ", args.Select(Format)),
						ctx.ScriptObject,
						Tag(ctx.ScriptObject));
					return null;
				})
				.Build();

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
