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
				.AddProperty("x", v => v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => v.magnitude)
				.AddProperty("sqrMagnitude", v => v.sqrMagnitude)
				.AddProperty("normalized", v => v.normalized) // returns Vector2 — not safe
				.AddMethod("toString", v => v.ToString())
				.SetConstructor((_, args) => new Vector2(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f))
				.AddStaticValue("zero", () => UnityEngine.Vector2.zero)
				.AddStaticValue("one", () => UnityEngine.Vector2.one)
				.AddStaticValue("up", () => UnityEngine.Vector2.up)
				.AddStaticValue("down", () => UnityEngine.Vector2.down)
				.AddStaticValue("left", () => UnityEngine.Vector2.left)
				.AddStaticValue("right", () => UnityEngine.Vector2.right)
				.AddStaticMethod("distance", (ctx, args) =>
                    UnityEngine.Vector2.Distance(
                        args.Length > 0 ? (Vector2)ctx.FromScript(args[0], typeof(Vector2)) : UnityEngine.Vector2.zero,
                        args.Length > 1 ? (Vector2)ctx.FromScript(args[1], typeof(Vector2)) : UnityEngine.Vector2.zero))
				.AddStaticMethod("dot", (ctx, args) =>
                    UnityEngine.Vector2.Dot(
                        args.Length > 0 ? (Vector2)ctx.FromScript(args[0], typeof(Vector2)) : UnityEngine.Vector2.zero,
                        args.Length > 1 ? (Vector2)ctx.FromScript(args[1], typeof(Vector2)) : UnityEngine.Vector2.zero))
				.SetDefault(UnityEngine.Vector2.zero)
				.Build();

		// ── Vector3 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector3 =
			ScriptingTypeConverterBuilder<Vector3>.Create()
				.AddProperty("x", v => v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => v.magnitude)
				.AddProperty("sqrMagnitude", v => v.sqrMagnitude)
				.AddProperty("normalized", v => v.normalized) // returns Vector3 — not safe
				.AddMethod("toString", v => v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Vector3(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f))
				.AddStaticValue("zero", () => UnityEngine.Vector3.zero)
				.AddStaticValue("one", () => UnityEngine.Vector3.one)
				.AddStaticValue("up", () => UnityEngine.Vector3.up)
				.AddStaticValue("down", () => UnityEngine.Vector3.down)
				.AddStaticValue("left", () => UnityEngine.Vector3.left)
				.AddStaticValue("right", () => UnityEngine.Vector3.right)
				.AddStaticValue("forward", () => UnityEngine.Vector3.forward)
				.AddStaticValue("back", () => UnityEngine.Vector3.back)
				.AddStaticMethod("distance", (ctx, args) =>
                    UnityEngine.Vector3.Distance(
                        args.Length > 0 ? (Vector3)ctx.FromScript(args[0], typeof(Vector3)) : UnityEngine.Vector3.zero,
                        args.Length > 1 ? (Vector3)ctx.FromScript(args[1], typeof(Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("dot", (ctx, args) =>
                    UnityEngine.Vector3.Dot(
                        args.Length > 0 ? (Vector3)ctx.FromScript(args[0], typeof(Vector3)) : UnityEngine.Vector3.zero,
                        args.Length > 1 ? (Vector3)ctx.FromScript(args[1], typeof(Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("cross", (ctx, args) =>
                    UnityEngine.Vector3.Cross(
                        args.Length > 0 ? (Vector3)ctx.FromScript(args[0], typeof(Vector3)) : UnityEngine.Vector3.zero,
                        args.Length > 1 ? (Vector3)ctx.FromScript(args[1], typeof(Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("lerp", (ctx, args) =>
                    UnityEngine.Vector3.Lerp(
                        args.Length > 0 ? (Vector3)ctx.FromScript(args[0], typeof(Vector3)) : UnityEngine.Vector3.zero,
                        args.Length > 1 ? (Vector3)ctx.FromScript(args[1], typeof(Vector3)) : UnityEngine.Vector3.zero,
                        args.Length > 2 ? args[2].ToFloat() : 0f))
				.SetDefault(UnityEngine.Vector3.zero)
				.Build();

		// ── Vector4 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector4 =
			ScriptingTypeConverterBuilder<Vector4>.Create()
				.AddProperty("x", v => v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("w", v => v.w, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => v.magnitude)
				.AddProperty("sqrMagnitude", v => v.sqrMagnitude)
				.AddProperty("normalized", v => v.normalized) // returns Vector4 — not safe
				.AddMethod("toString", v => v.ToString())
				.SetConstructor((_, args) => new Vector4(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 0f))
				.SetDefault(UnityEngine.Vector4.zero)
				.Build();

		// ── Quaternion ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Quaternion =
			ScriptingTypeConverterBuilder<Quaternion>.Create()
				.AddProperty("x", v => v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("w", v => v.w, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("eulerAngles", v => v.eulerAngles) // returns Vector3 — not safe
				.AddProperty("normalized", v => v.normalized) // returns Quaternion — not safe
				.AddMethod("toString", v => v.ToString())
				.SetConstructor((_, args) => new Quaternion(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 1f))
				.AddStaticValue("identity", () => UnityEngine.Quaternion.identity)
				.AddStaticMethod("euler", (_, args) =>
                    UnityEngine.Quaternion.Euler(
                        args.Length > 0 ? args[0].ToFloat() : 0f,
                        args.Length > 1 ? args[1].ToFloat() : 0f,
                        args.Length > 2 ? args[2].ToFloat() : 0f))
				.AddStaticMethod("lerp", (ctx, args) =>
                    UnityEngine.Quaternion.Lerp(
                        args.Length > 0 ? (Quaternion)ctx.FromScript(args[0], typeof(Quaternion)) : UnityEngine.Quaternion.identity,
                        args.Length > 1 ? (Quaternion)ctx.FromScript(args[1], typeof(Quaternion)) : UnityEngine.Quaternion.identity,
                        args.Length > 2 ? args[2].ToFloat() : 0f))
				.AddStaticMethod("angleAxis", (ctx, args) =>
                    UnityEngine.Quaternion.AngleAxis(
                        args.Length > 0 ? args[0].ToFloat() : 0f,
                        args.Length > 1 ? (Vector3)ctx.FromScript(args[1], typeof(Vector3)) : UnityEngine.Vector3.up))
				.SetDefault(UnityEngine.Quaternion.identity)
				.Build();

		// ── Color ─────────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Color =
			ScriptingTypeConverterBuilder<Color>.Create()
				.AddProperty("r", v => v.r, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("g", v => v.g, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("b", v => v.b, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("a", v => v.a, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("grayscale", v => v.grayscale)
				.AddProperty("linear", v => v.linear) // returns Color — not safe
				.AddProperty("gamma", v => v.gamma) // returns Color — not safe
				.AddMethod("toString", v => v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Color(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 1f))
				.AddStaticValue("red", () => UnityEngine.Color.red)
				.AddStaticValue("green", () => UnityEngine.Color.green)
				.AddStaticValue("blue", () => UnityEngine.Color.blue)
				.AddStaticValue("white", () => UnityEngine.Color.white)
				.AddStaticValue("black", () => UnityEngine.Color.black)
				.AddStaticValue("yellow", () => UnityEngine.Color.yellow)
				.AddStaticValue("cyan", () => UnityEngine.Color.cyan)
				.AddStaticValue("magenta", () => UnityEngine.Color.magenta)
				.AddStaticValue("clear", () => UnityEngine.Color.clear)
				.AddStaticMethod("lerp", (ctx, args) =>
                    UnityEngine.Color.Lerp(
                        args.Length > 0 ? (Color)ctx.FromScript(args[0], typeof(Color)) : UnityEngine.Color.black,
                        args.Length > 1 ? (Color)ctx.FromScript(args[1], typeof(Color)) : UnityEngine.Color.black,
                        args.Length > 2 ? args[2].ToFloat() : 0f))
				.SetDefault(UnityEngine.Color.white)
				.Build();

		// ── GameObject ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter GameObject =
			ScriptingTypeConverterBuilder<GameObject>.Create()
				.AddProperty("name",
					getter: v => v.name,
					setter: (v, val) => v.name = val?.ToString() ?? "",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("tag",
					getter: v => v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("layer",
					getter: v => v.layer,
					setter: (v, val) => v.layer = val.ToInt(),
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("activeSelf", v => v.activeSelf, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("activeInHierarchy", v => v.activeInHierarchy)
				.AddMethod("setActive", (v, args) => {
					if (args.Length > 0)
						v.SetActive(args[0].ToBool());
					return null;
				})
				.AddMethod("toString", v => v.ToString())
				// GameObject cannot be constructed from scripts; it must come from C#
				.SetDefault((GameObject)null)
				.Build();

		// ── Transform ─────────────────────────────────────────────────────
		// Declared in <see cref="TransformConverter"/>: the Transform converter enforces the
		// session sandbox through Anchor, so it does not live with the value types.

		// ── Rigidbody ─────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Rigidbody =
			ScriptingTypeConverterBuilder<Rigidbody>.Create()
				.AddProperty("linearVelocity",
					getter: (ctx, rb) => rb.linearVelocity,
					setter: (ctx, rb, val) => rb.linearVelocity = (Vector3)ctx.FromScript(val, typeof(Vector3)))
				.AddProperty("angularVelocity",
					getter: (ctx, rb) => rb.angularVelocity,
					setter: (ctx, rb, val) => rb.angularVelocity = (Vector3)ctx.FromScript(val, typeof(Vector3)))
				.AddProperty("mass",
					getter: rb => rb.mass,
					setter: (rb, val) => rb.mass = val.ToFloat())
				.AddProperty("linearDamping",
					getter: rb => rb.linearDamping,
					setter: (rb, val) => rb.linearDamping = val.ToFloat())
				.AddProperty("angularDamping",
					getter: rb => rb.angularDamping,
					setter: (rb, val) => rb.angularDamping = val.ToFloat())
				.AddProperty("isKinematic",
					getter: rb => rb.isKinematic,
					setter: (rb, val) => rb.isKinematic = val.ToBool())
				.AddProperty("useGravity",
					getter: rb => rb.useGravity,
					setter: (rb, val) => rb.useGravity = val.ToBool())
				.AddMethod("addForce", (ctx, rb, args) => {
					if (args.Length >= 3)
						rb.AddForce(args[0].ToFloat(), args[1].ToFloat(), args[2].ToFloat());
					else if (args.Length > 0)
						rb.AddForce((Vector3)ctx.FromScript(args[0], typeof(Vector3)));
					return null;
				})
				.AddMethod("addTorque", (ctx, rb, args) => {
					if (args.Length >= 3)
						rb.AddTorque(args[0].ToFloat(), args[1].ToFloat(), args[2].ToFloat());
					else if (args.Length > 0)
						rb.AddTorque((Vector3)ctx.FromScript(args[0], typeof(Vector3)));
					return null;
				})
				// Rigidbody cannot be constructed from scripts; it must come from C#
				.SetDefault((Rigidbody)null)
				.Build();

		/// <summary>All Unity type converters as a flat array for registration.</summary>
		public static readonly IScriptingTypeConverter[] All = {
			Vector2,
			Vector3,
			Vector4,
			Quaternion,
			Color,
			GameObject,
			TransformConverter.Transform,
			Rigidbody,
		};
	}
}