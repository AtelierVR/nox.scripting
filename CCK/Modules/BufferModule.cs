using System;
using System.Linq;
using Nox.CCK.Scripting.Converters;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"buffer"</c> — Node.js-compatible Buffer helpers.
	/// <code>
	/// import { from, toString } from 'buffer';
	/// const bytes = from("hello", "utf8");
	/// const text  = toString(bytes, "utf8");
	/// </code>
	/// </summary>
	public static class BufferModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("buffer")
				.WithTags("session")
				.AddMethod("from", (_, args) => {
					if (args.Length == 0) return Array.Empty<byte>();
					var encoding = args.Length > 1 ? args[1]?.ToString() ?? "utf8" : "utf8";
					switch (args[0]) {
						case byte[] b:
							return b;
						case object[] arr:
							return arr.Select(x => x.ToByte()).ToArray();
						default: {
							var data = args[0]?.ToString() ?? "";
							return BufferConverter.Encode(data, encoding);
						}
					}
				})
				.AddMethod("toString", (_, args) => {
					if (args.Length == 0) return "";
					byte[] buf;
					switch (args[0]) {
						case byte[] b:
							buf = b;
							break;
						case object[] arr:
							buf = arr.Select(x => x.ToByte()).ToArray();
							break;
						default:
							return "";
					}
					var encoding = args.Length > 1 ? args[1]?.ToString() ?? "utf8" : "utf8";
					return BufferConverter.Decode(buf, encoding);
				})
				.Build();
	}
}
