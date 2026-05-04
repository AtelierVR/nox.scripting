using System;
using System.Security.Cryptography;
using Nox.CCK.Utils;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"hashing"</c> — one-way hash functions.
	/// <para>All methods accept either a <c>string</c> (UTF-8 encoded) or a <c>byte[]</c>.</para>
	/// <code>
	/// import { sha256, sha512, md5, crc32, crc64 } from "hashing";
	/// const digest = sha256("hello");   // "2cf24db..."
	/// const n      = crc32("hello");    // integer
	/// </code>
	/// </summary>
	public static class HashingModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("hashing")
				.AddMethod("sha256", args => (object)Sha256(ToBytes(args, 0)))
				.AddMethod("sha512", args => (object)Sha512(ToBytes(args, 0)))
				.AddMethod("md5", args => (object)Md5(ToBytes(args, 0)))
				.AddMethod("crc32", args => (object)Hash.CRC32(ToBytes(args, 0)))
				.AddMethod("crc64", args => (object)Hash.CRC64(ToBytes(args, 0)))
				.Build();

		// ── Helpers ──────────────────────────────────────────────────────────

		private static byte[] ToBytes(object[] args, int index) {
			if (index >= args.Length || args[index] == null)
				return Array.Empty<byte>();
			if (args[index] is byte[] b)
				return b;
			return System.Text.Encoding.UTF8.GetBytes(args[index].ToString());
		}

		private static string ToHex(byte[] bytes)
			=> BitConverter.ToString(bytes).Replace("-", "").ToLower();

		private static string Sha256(byte[] data) {
			using var sha = SHA256.Create();
			return ToHex(sha.ComputeHash(data));
		}

		private static string Sha512(byte[] data) {
			using var sha = SHA512.Create();
			return ToHex(sha.ComputeHash(data));
		}

		private static string Md5(byte[] data) {
			using var md5 = MD5.Create();
			return ToHex(md5.ComputeHash(data));
		}
	}
}