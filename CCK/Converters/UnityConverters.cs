using System;
using Nox.Scripting;
using UnityEngine;

namespace Nox.CCK.Scripting.Converters {
	/// <summary>
	/// Pre-built <see cref="IScriptingTypeConverter"/> instances for common Unity types.
	/// Register via <see cref="IScriptingAPI.RegisterConverter"/> or use <see cref="All"/>
	/// to register all at once.
	///
	/// Each converter exposes per-instance bindings (properties + methods) via
	/// <see cref="IScriptingTypeConverter.Bindings"/> and supports construction from
	/// ordered script arguments via <see cref="IScriptingTypeConverter.Constructor"/>
	/// (e.g. <c>new Vector3(x, y, z)</c>) with a sensible
	/// <see cref="IScriptingTypeConverter.Default"/> fallback.
	/// </summary>
	public static class UnityConverters {

		// ── Vector2 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector2 =
			ScriptingTypeConverterBuilder<Vector2>.Create()
				.AddProperty("x", v => (object)v.x)
				.AddProperty("y", v => (object)v.y)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized)
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new Vector2(
					args.Length > 0 && args[0] != null ? Convert.ToSingle(args[0]) : 0f,
					args.Length > 1 && args[1] != null ? Convert.ToSingle(args[1]) : 0f))
				.SetDefault(UnityEngine.Vector2.zero)
				.Build();

		// ── Vector3 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector3 =
			ScriptingTypeConverterBuilder<Vector3>.Create()
				.AddProperty("x", v => (object)v.x)
				.AddProperty("y", v => (object)v.y)
				.AddProperty("z", v => (object)v.z)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized)
				.AddMethod("toString", v => (object)v.ToString())
				.AddMethod("cross", (ctx, v, args) =>
					(object)UnityEngine.Vector3.Cross(v,
						args.Length > 0
							? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3))
							: UnityEngine.Vector3.zero))
				.AddMethod("dot", (ctx, v, args) =>
					(object)UnityEngine.Vector3.Dot(v,
						args.Length > 0
							? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3))
							: UnityEngine.Vector3.zero))
				.AddMethod("distance", (ctx, v, args) =>
					(object)UnityEngine.Vector3.Distance(v,
						args.Length > 0
							? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3))
							: UnityEngine.Vector3.zero))
				.SetConstructor((_, args) => new UnityEngine.Vector3(
					args.Length > 0 && args[0] != null ? Convert.ToSingle(args[0]) : 0f,
					args.Length > 1 && args[1] != null ? Convert.ToSingle(args[1]) : 0f,
					args.Length > 2 && args[2] != null ? Convert.ToSingle(args[2]) : 0f))
				.SetDefault(UnityEngine.Vector3.zero)
				.Build();

		// ── Vector4 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector4 =
			ScriptingTypeConverterBuilder<Vector4>.Create()
				.AddProperty("x", v => (object)v.x)
				.AddProperty("y", v => (object)v.y)
				.AddProperty("z", v => (object)v.z)
				.AddProperty("w", v => (object)v.w)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized)
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Vector4(
					args.Length > 0 && args[0] != null ? Convert.ToSingle(args[0]) : 0f,
					args.Length > 1 && args[1] != null ? Convert.ToSingle(args[1]) : 0f,
					args.Length > 2 && args[2] != null ? Convert.ToSingle(args[2]) : 0f,
					args.Length > 3 && args[3] != null ? Convert.ToSingle(args[3]) : 0f))
				.SetDefault(UnityEngine.Vector4.zero)
				.Build();

		// ── Quaternion ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Quaternion =
			ScriptingTypeConverterBuilder<Quaternion>.Create()
				.AddProperty("x", v => (object)v.x)
				.AddProperty("y", v => (object)v.y)
				.AddProperty("z", v => (object)v.z)
				.AddProperty("w", v => (object)v.w)
				.AddProperty("eulerAngles", v => (object)v.eulerAngles)
				.AddProperty("normalized", v => (object)v.normalized)
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Quaternion(
					args.Length > 0 && args[0] != null ? Convert.ToSingle(args[0]) : 0f,
					args.Length > 1 && args[1] != null ? Convert.ToSingle(args[1]) : 0f,
					args.Length > 2 && args[2] != null ? Convert.ToSingle(args[2]) : 0f,
					args.Length > 3 && args[3] != null ? Convert.ToSingle(args[3]) : 1f))
				.SetDefault(UnityEngine.Quaternion.identity)
				.Build();

		// ── Color ─────────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Color =
			ScriptingTypeConverterBuilder<Color>.Create()
				.AddProperty("r", v => (object)v.r)
				.AddProperty("g", v => (object)v.g)
				.AddProperty("b", v => (object)v.b)
				.AddProperty("a", v => (object)v.a)
				.AddProperty("grayscale", v => (object)v.grayscale)
				.AddProperty("linear", v => (object)v.linear)
				.AddProperty("gamma", v => (object)v.gamma)
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Color(
					args.Length > 0 && args[0] != null ? Convert.ToSingle(args[0]) : 0f,
					args.Length > 1 && args[1] != null ? Convert.ToSingle(args[1]) : 0f,
					args.Length > 2 && args[2] != null ? Convert.ToSingle(args[2]) : 0f,
					args.Length > 3 && args[3] != null ? Convert.ToSingle(args[3]) : 1f))
				.SetDefault(UnityEngine.Color.white)
				.Build();

		// ── GameObject ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter GameObject =
			ScriptingTypeConverterBuilder<GameObject>.Create()
				.AddProperty("name",
					getter: v => (object)v.name,
					setter: (v, val) => v.name = val?.ToString() ?? "")
				.AddProperty("tag",
					getter: v => (object)v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged")
				.AddProperty("layer",
					getter: v => (object)v.layer,
					setter: (v, val) => v.layer = Convert.ToInt32(val))
				.AddProperty("activeSelf", v => (object)v.activeSelf)
				.AddProperty("activeInHierarchy", v => (object)v.activeInHierarchy)
				.AddMethod("setActive", (v, args) => {
					if (args.Length > 0)
						v.SetActive(Convert.ToBoolean(args[0]));
					return null;
				})
				.AddMethod("toString", v => (object)v.ToString())
				// GameObject cannot be constructed from scripts; it must come from C#
				.SetDefault((GameObject)null)
				.Build();

		// ── Transform ─────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Transform =
			ScriptingTypeConverterBuilder<Transform>.Create()
				.AddProperty("name",
					getter: v => (object)v.name,
					setter: (v, val) => v.name = val?.ToString() ?? "")
				.AddProperty("tag",
					getter: v => (object)v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged")
				.AddProperty("position",
					getter: (ctx, t) => (object)t.position,
					setter: (ctx, t, val) => t.position = (UnityEngine.Vector3)ctx.FromScript(val, typeof(UnityEngine.Vector3)))
				.AddProperty("localPosition",
					getter: (ctx, t) => (object)t.localPosition,
					setter: (ctx, t, val) => t.localPosition = (UnityEngine.Vector3)ctx.FromScript(val, typeof(UnityEngine.Vector3)))
				.AddProperty("localScale",
					getter: (ctx, t) => (object)t.localScale,
					setter: (ctx, t, val) => t.localScale = (UnityEngine.Vector3)ctx.FromScript(val, typeof(UnityEngine.Vector3)))
				.AddProperty("rotation",
					getter: (ctx, t) => (object)t.rotation,
					setter: (ctx, t, val) => t.rotation = (UnityEngine.Quaternion)ctx.FromScript(val, typeof(UnityEngine.Quaternion)))
				.AddProperty("localRotation",
					getter: (ctx, t) => (object)t.localRotation,
					setter: (ctx, t, val) => t.localRotation = (UnityEngine.Quaternion)ctx.FromScript(val, typeof(UnityEngine.Quaternion)))
				.AddProperty("childCount", t => (object)t.childCount)
				.AddMethod("toString", t => (object)t.ToString())
				.AddMethod("rotate", (ctx, t, args) => {
					if (args.Length >= 3) {
						// rotate(x, y, z [, space])
						var euler = new UnityEngine.Vector3(
							Convert.ToSingle(args[0]),
							Convert.ToSingle(args[1]),
							Convert.ToSingle(args[2]));
						var space = args.Length > 3 ? (UnityEngine.Space)Convert.ToInt32(args[3]) : UnityEngine.Space.Self;
						t.Rotate(euler, space);
					} else if (args.Length > 0) {
						// rotate(vector3 [, space])
						t.Rotate((UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)),
							args.Length > 1 ? (UnityEngine.Space)Convert.ToInt32(args[1]) : UnityEngine.Space.Self);
					}
					return null;
				})
				// Transform cannot be constructed from scripts; it must come from C#
				.SetDefault((Transform)null)
				.Build();

		/// <summary>All Unity type converters as a flat array for registration.</summary>
		public static readonly IScriptingTypeConverter[] All = {
			Vector2,
			Vector3,
			Vector4,
			Quaternion,
			Color,
			GameObject,
			Transform,
		};
	}
}