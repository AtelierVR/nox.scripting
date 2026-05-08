using Nox.CCK;
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
				.AddProperty("x", v => (object)v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => (object)v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized) // returns Vector2 — not safe
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new Vector2(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f))
				.AddStaticValue("zero", () => (object)UnityEngine.Vector2.zero)
				.AddStaticValue("one", () => (object)UnityEngine.Vector2.one)
				.AddStaticValue("up", () => (object)UnityEngine.Vector2.up)
				.AddStaticValue("down", () => (object)UnityEngine.Vector2.down)
				.AddStaticValue("left", () => (object)UnityEngine.Vector2.left)
				.AddStaticValue("right", () => (object)UnityEngine.Vector2.right)
				.AddStaticMethod("distance", (ctx, args) =>
					(object)UnityEngine.Vector2.Distance(
						args.Length > 0 ? (UnityEngine.Vector2)ctx.FromScript(args[0], typeof(UnityEngine.Vector2)) : UnityEngine.Vector2.zero,
						args.Length > 1 ? (UnityEngine.Vector2)ctx.FromScript(args[1], typeof(UnityEngine.Vector2)) : UnityEngine.Vector2.zero))
				.AddStaticMethod("dot", (ctx, args) =>
					(object)UnityEngine.Vector2.Dot(
						args.Length > 0 ? (UnityEngine.Vector2)ctx.FromScript(args[0], typeof(UnityEngine.Vector2)) : UnityEngine.Vector2.zero,
						args.Length > 1 ? (UnityEngine.Vector2)ctx.FromScript(args[1], typeof(UnityEngine.Vector2)) : UnityEngine.Vector2.zero))
				.SetDefault(UnityEngine.Vector2.zero)
				.Build();

		// ── Vector3 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector3 =
			ScriptingTypeConverterBuilder<Vector3>.Create()
				.AddProperty("x", v => (object)v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => (object)v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => (object)v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized) // returns Vector3 — not safe
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Vector3(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f))
				.AddStaticValue("zero", () => (object)UnityEngine.Vector3.zero)
				.AddStaticValue("one", () => (object)UnityEngine.Vector3.one)
				.AddStaticValue("up", () => (object)UnityEngine.Vector3.up)
				.AddStaticValue("down", () => (object)UnityEngine.Vector3.down)
				.AddStaticValue("left", () => (object)UnityEngine.Vector3.left)
				.AddStaticValue("right", () => (object)UnityEngine.Vector3.right)
				.AddStaticValue("forward", () => (object)UnityEngine.Vector3.forward)
				.AddStaticValue("back", () => (object)UnityEngine.Vector3.back)
				.AddStaticMethod("distance", (ctx, args) =>
					(object)UnityEngine.Vector3.Distance(
						args.Length > 0 ? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero,
						args.Length > 1 ? (UnityEngine.Vector3)ctx.FromScript(args[1], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("dot", (ctx, args) =>
					(object)UnityEngine.Vector3.Dot(
						args.Length > 0 ? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero,
						args.Length > 1 ? (UnityEngine.Vector3)ctx.FromScript(args[1], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("cross", (ctx, args) =>
					(object)UnityEngine.Vector3.Cross(
						args.Length > 0 ? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero,
						args.Length > 1 ? (UnityEngine.Vector3)ctx.FromScript(args[1], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero))
				.AddStaticMethod("lerp", (ctx, args) =>
					(object)UnityEngine.Vector3.Lerp(
						args.Length > 0 ? (UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero,
						args.Length > 1 ? (UnityEngine.Vector3)ctx.FromScript(args[1], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.zero,
						args.Length > 2 ? args[2].ToFloat() : 0f))
				.SetDefault(UnityEngine.Vector3.zero)
				.Build();

		// ── Vector4 ───────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Vector4 =
			ScriptingTypeConverterBuilder<Vector4>.Create()
				.AddProperty("x", v => (object)v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => (object)v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => (object)v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("w", v => (object)v.w, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("magnitude", v => (object)v.magnitude)
				.AddProperty("sqrMagnitude", v => (object)v.sqrMagnitude)
				.AddProperty("normalized", v => (object)v.normalized) // returns Vector4 — not safe
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Vector4(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 0f))
				.SetDefault(UnityEngine.Vector4.zero)
				.Build();

		// ── Quaternion ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Quaternion =
			ScriptingTypeConverterBuilder<Quaternion>.Create()
				.AddProperty("x", v => (object)v.x, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("y", v => (object)v.y, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("z", v => (object)v.z, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("w", v => (object)v.w, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("eulerAngles", v => (object)v.eulerAngles) // returns Vector3 — not safe
				.AddProperty("normalized", v => (object)v.normalized) // returns Quaternion — not safe
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Quaternion(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 1f))
				.AddStaticValue("identity", () => (object)UnityEngine.Quaternion.identity)
				.AddStaticMethod("euler", (_, args) =>
					(object)UnityEngine.Quaternion.Euler(
						args.Length > 0 ? args[0].ToFloat() : 0f,
						args.Length > 1 ? args[1].ToFloat() : 0f,
						args.Length > 2 ? args[2].ToFloat() : 0f))
				.AddStaticMethod("lerp", (ctx, args) =>
					(object)UnityEngine.Quaternion.Lerp(
						args.Length > 0 ? (UnityEngine.Quaternion)ctx.FromScript(args[0], typeof(UnityEngine.Quaternion)) : UnityEngine.Quaternion.identity,
						args.Length > 1 ? (UnityEngine.Quaternion)ctx.FromScript(args[1], typeof(UnityEngine.Quaternion)) : UnityEngine.Quaternion.identity,
						args.Length > 2 ? args[2].ToFloat() : 0f))
				.AddStaticMethod("angleAxis", (ctx, args) =>
					(object)UnityEngine.Quaternion.AngleAxis(
						args.Length > 0 ? args[0].ToFloat() : 0f,
						args.Length > 1 ? (UnityEngine.Vector3)ctx.FromScript(args[1], typeof(UnityEngine.Vector3)) : UnityEngine.Vector3.up))
				.SetDefault(UnityEngine.Quaternion.identity)
				.Build();

		// ── Color ─────────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Color =
			ScriptingTypeConverterBuilder<Color>.Create()
				.AddProperty("r", v => (object)v.r, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("g", v => (object)v.g, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("b", v => (object)v.b, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("a", v => (object)v.a, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("grayscale", v => (object)v.grayscale)
				.AddProperty("linear", v => (object)v.linear) // returns Color — not safe
				.AddProperty("gamma", v => (object)v.gamma) // returns Color — not safe
				.AddMethod("toString", v => (object)v.ToString())
				.SetConstructor((_, args) => new UnityEngine.Color(
					args.Length > 0 && args[0] != null ? args[0].ToFloat() : 0f,
					args.Length > 1 && args[1] != null ? args[1].ToFloat() : 0f,
					args.Length > 2 && args[2] != null ? args[2].ToFloat() : 0f,
					args.Length > 3 && args[3] != null ? args[3].ToFloat() : 1f))
				.AddStaticValue("red", () => (object)UnityEngine.Color.red)
				.AddStaticValue("green", () => (object)UnityEngine.Color.green)
				.AddStaticValue("blue", () => (object)UnityEngine.Color.blue)
				.AddStaticValue("white", () => (object)UnityEngine.Color.white)
				.AddStaticValue("black", () => (object)UnityEngine.Color.black)
				.AddStaticValue("yellow", () => (object)UnityEngine.Color.yellow)
				.AddStaticValue("cyan", () => (object)UnityEngine.Color.cyan)
				.AddStaticValue("magenta", () => (object)UnityEngine.Color.magenta)
				.AddStaticValue("clear", () => (object)UnityEngine.Color.clear)
				.AddStaticMethod("lerp", (ctx, args) =>
					(object)UnityEngine.Color.Lerp(
						args.Length > 0 ? (UnityEngine.Color)ctx.FromScript(args[0], typeof(UnityEngine.Color)) : UnityEngine.Color.black,
						args.Length > 1 ? (UnityEngine.Color)ctx.FromScript(args[1], typeof(UnityEngine.Color)) : UnityEngine.Color.black,
						args.Length > 2 ? args[2].ToFloat() : 0f))
				.SetDefault(UnityEngine.Color.white)
				.Build();

		// ── GameObject ────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter GameObject =
			ScriptingTypeConverterBuilder<GameObject>.Create()
				.AddProperty("name",
					getter: v => (object)v.name,
					setter: (v, val) => v.name = val?.ToString() ?? "",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("tag",
					getter: v => (object)v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("layer",
					getter: v => (object)v.layer,
					setter: (v, val) => v.layer = val.ToInt(),
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("activeSelf", v => (object)v.activeSelf, flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("activeInHierarchy", v => (object)v.activeInHierarchy)
				.AddMethod("setActive", (v, args) => {
					if (args.Length > 0)
						v.SetActive(args[0].ToBool());
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
					setter: (v, val) => v.name = val?.ToString() ?? "",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("tag",
					getter: v => (object)v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged",
					flags: ScriptingTypePropertyFlags.InspectGetter)
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
							args[0].ToFloat(),
							args[1].ToFloat(),
							args[2].ToFloat());
						var space = args.Length > 3 ? (UnityEngine.Space)args[3].ToInt() : UnityEngine.Space.Self;
						t.Rotate(euler, space);
					} else if (args.Length > 0) {
						// rotate(vector3 [, space])
						t.Rotate((UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)),
							args.Length > 1 ? (UnityEngine.Space)args[1].ToInt() : UnityEngine.Space.Self);
					}
					return null;
				})
				// Transform cannot be constructed from scripts; it must come from C#
				.SetDefault((Transform)null)
				.Build();

		// ── Rigidbody ─────────────────────────────────────────────────────

		public static readonly IScriptingTypeConverter Rigidbody =
			ScriptingTypeConverterBuilder<Rigidbody>.Create()
				.AddProperty("linearVelocity",
					getter: (ctx, rb) => (object)rb.linearVelocity,
					setter: (ctx, rb, val) => rb.linearVelocity = (UnityEngine.Vector3)ctx.FromScript(val, typeof(UnityEngine.Vector3)))
				.AddProperty("angularVelocity",
					getter: (ctx, rb) => (object)rb.angularVelocity,
					setter: (ctx, rb, val) => rb.angularVelocity = (UnityEngine.Vector3)ctx.FromScript(val, typeof(UnityEngine.Vector3)))
				.AddProperty("mass",
					getter: rb => (object)rb.mass,
					setter: (rb, val) => rb.mass = val.ToFloat())
				.AddProperty("linearDamping",
					getter: rb => (object)rb.linearDamping,
					setter: (rb, val) => rb.linearDamping = val.ToFloat())
				.AddProperty("angularDamping",
					getter: rb => (object)rb.angularDamping,
					setter: (rb, val) => rb.angularDamping = val.ToFloat())
				.AddProperty("isKinematic",
					getter: rb => (object)rb.isKinematic,
					setter: (rb, val) => rb.isKinematic = val.ToBool())
				.AddProperty("useGravity",
					getter: rb => (object)rb.useGravity,
					setter: (rb, val) => rb.useGravity = val.ToBool())
				.AddMethod("addForce", (ctx, rb, args) => {
					if (args.Length >= 3)
						rb.AddForce(args[0].ToFloat(), args[1].ToFloat(), args[2].ToFloat());
					else if (args.Length > 0)
						rb.AddForce((UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)));
					return null;
				})
				.AddMethod("addTorque", (ctx, rb, args) => {
					if (args.Length >= 3)
						rb.AddTorque(args[0].ToFloat(), args[1].ToFloat(), args[2].ToFloat());
					else if (args.Length > 0)
						rb.AddTorque((UnityEngine.Vector3)ctx.FromScript(args[0], typeof(UnityEngine.Vector3)));
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
			Transform,
			Rigidbody,
		};
	}
}