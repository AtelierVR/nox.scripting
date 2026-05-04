using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Nox.CCK.Scripting.Converters;
using Nox.CCK.Utils;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"crypto"</c> — RSA asymmetric crypto, HMAC and random utilities.
	/// <para>
	/// The app's RSA private key lives on disk (managed by <see cref="Crypto"/>).
	/// Scripts can sign and decrypt with the local key, but <b>cannot read, set or delete the private key</b>.
	/// They can verify and encrypt using any <see cref="RsaPublicKey"/> or raw DER bytes.
	/// </para>
	/// <code>
	/// import { publicKey, sign, verify, encrypt, decrypt,
	///          randomBytes, uuid, hmacSha256 } from "crypto";
	///
	/// const myKey = publicKey;                         // RsaPublicKey
	/// const sig   = sign("hello");                     // base64 RSA-SHA256
	/// const ok    = verify(myKey, "hello", sig);       // true
	/// </code>
	/// </summary>
	public static class CryptoModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("crypto")
				// ── Random ───────────────────────────────────────────────────
				.AddMethod("randomBytes", args => {
					var n      = args.Length > 0 && args[0] != null ? Convert.ToInt32(args[0]) : 16;
					var buffer = new byte[Math.Max(0, n)];
					RandomNumberGenerator.Fill(buffer);
					return (object)buffer;
				})
				.AddMethod("uuid", _ => (object)Guid.NewGuid().ToString())

				// ── HMAC (symmetric) ─────────────────────────────────────────
				.AddMethod("hmacSha256", args => {
					var key  = ToBytes(args, 0);
					var data = ToBytes(args, 1);
					return (object)ToHex(HmacSha256(key, data));
				})

                // ── RSA (asymmetric) ─────────────────────────────────────────
                .AddMethod("createKeyPair", args => {
                    var keySize = args.Length > 0 && args[0] != null ? Convert.ToInt32(args[0]) : 4096;
                    var rsa     = Crypto.CreateKeyPair(keySize);
                    return (object)new Dictionary<string, object> {
                        ["publicKey"]  = new RsaPublicKey(Crypto.ExportPublicKeyToDer(rsa)),
                        ["privateKey"] = new RsaPrivateKey(Crypto.ExportPrivateKeyToDer(rsa))
                    };
                })

				.Build();

		// ── Helpers ──────────────────────────────────────────────────────────

		private static byte[] ToBytes(object[] args, int index) {
			if (index >= args.Length || args[index] == null) return Array.Empty<byte>();
			if (args[index] is byte[] b) return b;
			return Encoding.UTF8.GetBytes(args[index].ToString());
		}

		private static string ToHex(byte[] bytes)
			=> BitConverter.ToString(bytes).Replace("-", "").ToLower();

		private static byte[] HmacSha256(byte[] key, byte[] data) {
			using var hmac = new HMACSHA256(key);
			return hmac.ComputeHash(data);
		}
	}
}

