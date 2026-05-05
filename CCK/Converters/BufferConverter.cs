using System;
using System.Linq;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Converters {
	/// <summary>
	/// Pre-built <see cref="IScriptingTypeConverter"/> for <c>byte[]</c>.
	/// Exposes <c>length</c>, <c>toString(encoding?)</c> and a constructor that
	/// accepts either raw byte arguments or <c>(string, encoding?)</c>.
	/// </summary>
	public static class BufferConverter {
		public static readonly IScriptingTypeConverter Buffer =
			ScriptingTypeConverterBuilder<byte[]>.Create()
				.AddProperty("length", v => (object)v.Length)
				.AddMethod("toString", (v, args) => {
					var encoding = args.Length > 0 && args[0] is string enc ? enc : "utf8";
					return (object)Decode(v, encoding);
				})
				.SetConstructor((_, args) => {
					// Buffer.from(string) / Buffer.from(string, encoding)
					if (args.Length >= 1 && args[0] is string str) {
						var enc = args.Length >= 2 && args[1] is string e ? e : "utf8";
						return Encode(str, enc);
					}
					// Buffer.from(byte, byte, ...)
					var result = new byte[ args.Length ];
					for (var i = 0; i < args.Length; i++)
						result[i] = args[i] != null ? Convert.ToByte(args[i]) : (byte)0;
					return result;
				})
				.SetDefault(Array.Empty<byte>())
				.Build();

		internal static byte[] Encode(string data, string encoding)
			=> encoding.ToLower() switch {
				"utf8"    => System.Text.Encoding.UTF8.GetBytes(data),
				"ascii"   => System.Text.Encoding.ASCII.GetBytes(data),
				"unicode" => System.Text.Encoding.Unicode.GetBytes(data),
				"base64"  => Convert.FromBase64String(data),
				"hex" => Enumerable.Range(0, data.Length / 2)
					.Select(x => Convert.ToByte(data.Substring(x * 2, 2), 16))
					.ToArray(),
				_ => throw new NotSupportedException($"Encoding '{encoding}' is not supported"),
			};

		internal static string Decode(byte[] buffer, string encoding)
			=> encoding.ToLower() switch {
				"utf8"    => System.Text.Encoding.UTF8.GetString(buffer),
				"ascii"   => System.Text.Encoding.ASCII.GetString(buffer),
				"unicode" => System.Text.Encoding.Unicode.GetString(buffer),
				"base64"  => Convert.ToBase64String(buffer),
				"hex"     => BitConverter.ToString(buffer).Replace("-", "").ToLower(),
				_         => throw new NotSupportedException($"Encoding '{encoding}' is not supported"),
			};

		public static readonly IScriptingTypeConverter[] All = {
			Buffer
		};
	}
}