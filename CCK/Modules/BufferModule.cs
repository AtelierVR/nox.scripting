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
					var data     = args.Length > 0 ? args[0]?.ToString() ?? "" : "";
					var encoding = args.Length > 1 ? args[1]?.ToString() ?? "utf8" : "utf8";
					return (object)BufferConverter.Encode(data, encoding);
				})
				.AddMethod("toString", (_, args) => {
					if (args.Length == 0) return null;
					byte[] buf;
					switch (args[0]) {
						case byte[] b:
							buf = b;
							break;
						case object[] arr:
							buf = arr.Select(x => x != null ? Convert.ToByte(x) : (byte)0).ToArray();
							break;
						default:
							return null;
					}
					var encoding = args.Length > 1 ? args[1]?.ToString() ?? "utf8" : "utf8";
					return (object)BufferConverter.Decode(buf, encoding);
				})
				.Build();
	}
}
