using System;
using System.Security.Cryptography;
using System.Text;
using Nox.CCK.Utils;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Converters {
	/// <summary>
	/// A script-safe wrapper around an RSA private key (stored as PKCS#8 DER).
	/// The raw key material is accessible only via the explicit <c>exportDer()</c>
	/// method — there is no passive property that leaks the bytes.
	/// </summary>
	public sealed class RsaPrivateKey {
		private readonly byte[] _der;

		public RsaPrivateKey(byte[] der)
			=> _der = der ?? throw new ArgumentNullException(nameof(der));
		
		internal RSA ToRSA()
			=> Crypto.ImportPrivateKeyFromDer(_der);

		internal byte[] ExportDer()
			=> _der;
	}

	/// <summary>
	/// A script-safe wrapper around an RSA public key.
	/// The private key is never stored here — all signing/decryption
	/// remains in C# (see the <c>"crypto"</c> scripting module).
	/// </summary>
	public sealed class RsaPublicKey {
		private readonly byte[] _der;

		public RsaPublicKey(byte[] der)
			=> _der = der ?? throw new ArgumentNullException(nameof(der));
		
		/// <summary>Reconstruct an RSA instance from the stored DER.</summary>
		internal RSA ToRSA()
			=> Crypto.ImportPublicKeyFromDer(_der);

		internal byte[] ExportDer()
			=> _der;
	}

	/// <summary>
	/// <see cref="IScriptingTypeConverter"/> for <see cref="RsaPrivateKey"/>.
	/// Exposes signing and decryption only.
	/// Raw key material is accessible through the explicit <c>exportDer()</c> method.
	/// </summary>
	public static class RsaConverter {
		public static readonly IScriptingTypeConverter PrivateKey =
			ScriptingTypeConverterBuilder<RsaPrivateKey>.Create()
				// ── Properties ───────────────────────────────────────────────
				.AddProperty("publicKey", k => {
					using var rsa = k.ToRSA();
					return (object)new RsaPublicKey(Crypto.ExportPublicKeyToDer(rsa));
				})

				// ── Methods ──────────────────────────────────────────────────
				.AddMethod("sign", (k, args) => {
					var data = ToBytes(args, 0);
					try {
						using var rsa = k.ToRSA();
						return (object)Convert.ToBase64String(Crypto.Sign(data, rsa));
					} catch { return null; }
				})
				.AddMethod("decrypt", (k, args) => {
					if (args.Length == 0 || args[0] == null)
						return null;
					byte[] ciphertext;
					try { ciphertext = Convert.FromBase64String(args[0].ToString()); } catch { return null; }
					try {
						using var rsa = k.ToRSA();
						return (object)Crypto.Decrypt(ciphertext, rsa);
					} catch { return null; }
				})
				// Explicit export — caller must opt-in to extract key material.
				.AddMethod("der", (k, _) => (object)k.ExportDer())

				// ── Constructor ──────────────────────────────────────────────
				// new RsaPrivateKey(derBytes or base64String)
				.SetConstructor((_, args) => {
					if (args.Length == 0 || args[0] == null)
						return null;
					byte[] der;
					if (args[0] is byte[] b)
						der = b;
					else {
						try { der = Convert.FromBase64String(args[0].ToString()); } catch { return null; }
					}
					try { return new RsaPrivateKey(der); } catch { return null; }
				})
				.SetDefault((RsaPrivateKey)null)
				.Build();

		public static readonly IScriptingTypeConverter PublicKey =
			ScriptingTypeConverterBuilder<RsaPublicKey>.Create()
				// ── Properties ───────────────────────────────────────────────
				.AddProperty("der", k => (object)k.ExportDer())
				// NOTE: no "privateKey" property – private key access is refused.

				// ── Methods ──────────────────────────────────────────────────
				.AddMethod("verify", (k, args) => {
					var data = ToBytes(args, 0);
					if (args.Length < 2 || args[1] == null)
						return (object)false;
					byte[] sig;
					try { sig = Convert.FromBase64String(args[1].ToString()); } catch { return (object)false; }
					try { return (object)Crypto.Verify(data, sig, k.ToRSA()); } catch { return (object)false; }
				})
				.AddMethod("encrypt", (k, args) => {
					var plaintext = DataToString(args, 0);
					try { return (object)Convert.ToBase64String(Crypto.Encrypt(plaintext, k.ToRSA())); } catch { return null; }
				})

				// ── Constructor ──────────────────────────────────────────────
				// new RsaPublicKey(derBytesOrBase64String)
				.SetConstructor((_, args) => {
					if (args.Length == 0 || args[0] == null)
						return null;
					byte[] der = null;
					if (args[0] is byte[] b)
						der = b;
					else {
						try { der = Convert.FromBase64String(args[0].ToString()); } catch { return null; }
					}
					try { return new RsaPublicKey(der); } catch { return null; }
				})
				.SetDefault((RsaPublicKey)null)
				.Build();

		public static readonly IScriptingTypeConverter[] All = {
			PublicKey,
			PrivateKey
		};

		// ── Helpers ──────────────────────────────────────────────────────────

		private static string DataToString(object[] args, int index) {
			if (index >= args.Length || args[index] == null)
				return "";
			if (args[index] is byte[] b)
				return Encoding.UTF8.GetString(b);
			return args[index].ToString();
		}

		private static byte[] ToBytes(object[] args, int index) {
			if (index >= args.Length || args[index] == null)
				return Array.Empty<byte>();
			if (args[index] is byte[] b)
				return b;
			return Encoding.UTF8.GetBytes(args[index].ToString());
		}
	}
}