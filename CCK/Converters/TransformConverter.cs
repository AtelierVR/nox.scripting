using Nox.CCK.Utils;
using Nox.Scripting;
using Nox.Scripting.Extensions;
using UnityEngine;
using Logger = Nox.CCK.Utils.Logger;

namespace Nox.CCK.Scripting.Converters {
	/// <summary>
	/// <see cref="IScriptingTypeConverter"/> for <see cref="Transform"/>, sandboxed by the
	/// <see cref="Anchor"/> components of the hierarchy.
	/// <para>
	/// A script must not escape the session it runs in: the anchor tagged with the session id
	/// (<see cref="IScriptingContext.Session"/>.<c>Id</c>) is the boundary. <c>parent</c> stops at
	/// it, and reparenting outside of that boundary is refused.
	/// </para>
	/// <code>
	/// transform.position.set(...)      // still blocked: Vector3 is a value type
	/// transform.parent                 // null once the sandbox boundary is reached
	/// transform.parent = other         // refused outside the session sandbox
	/// </code>
	/// </summary>
	public static class TransformConverter {
		public static readonly IScriptingTypeConverter Transform =
			ScriptingTypeConverterBuilder<Transform>.Create()
				.AddProperty("name",
					getter: v => v.name,
					setter: (v, val) => v.name = val?.ToString() ?? "",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("tag",
					getter: v => v.tag,
					setter: (v, val) => v.tag = val?.ToString() ?? "Untagged",
					flags: ScriptingTypePropertyFlags.InspectGetter)
				.AddProperty("position",
					getter: (ctx, t) => t.position,
					setter: (ctx, t, val) => t.position = ctx.FromScript<Vector3>(val))
				.AddProperty("localPosition",
					getter: (ctx, t) => t.localPosition,
					setter: (ctx, t, val) => t.localPosition = ctx.FromScript<Vector3>(val))
				.AddProperty("localScale",
					getter: (ctx, t) => t.localScale,
					setter: (ctx, t, val) => t.localScale = ctx.FromScript<Vector3>(val))
				.AddProperty("rotation",
					getter: (ctx, t) => t.rotation,
					setter: (ctx, t, val) => t.rotation = ctx.FromScript<Quaternion>(val))
				.AddProperty("localRotation",
					getter: (ctx, t) => t.localRotation,
					setter: (ctx, t, val) => t.localRotation = ctx.FromScript<Quaternion>(val))
				.AddProperty("childCount", t => t.childCount)
				// Sandboxed: the parent is hidden once it leaves the session boundary.
				.AddProperty("parent",
					getter: (ctx, t) => CanAccess(ctx, t.parent) ? t.parent : null,
					setter: (ctx, t, val) => Reparent(ctx, t, val))
				.AddMethod("toString", t => t.ToString())
				.AddMethod("rotate", (ctx, t, args) => {
					if (args.Length >= 3) {
						// rotate(x, y, z [, space])
						var euler = new Vector3(
							args[0].ToFloat(),
							args[1].ToFloat(),
							args[2].ToFloat());
						t.Rotate(euler, args.Length > 3 ? (Space)args[3].ToInt() : Space.Self);
					} else if (args.Length > 0) {
						// rotate(vector3 [, space])
						t.Rotate(ctx.FromScript<Vector3>(args[0]),
							args.Length > 1 ? (Space)args[1].ToInt() : Space.Self);
					}
					return null;
				})
				// Transform cannot be constructed from scripts; it must come from C#
				.SetDefault((Transform)null)
				.Build();

		/// <summary>Session id used as the sandbox tag, or <c>null</c> when not session-scoped.</summary>
		private static string SessionTag(IScriptingContext ctx)
			=> ctx?.Session?.Id;

		/// <summary>
		/// Nearest ancestor (inclusive) carrying an <see cref="Anchor"/> tagged with the session id:
		/// the node the script must not leave. <c>null</c> when the hierarchy has no such boundary.
		/// </summary>
		public static Transform Boundary(IScriptingContext ctx, Transform from) {
			var tag = SessionTag(ctx);
			if (string.IsNullOrEmpty(tag) || !from)
				return null;

			for (var current = from; current; current = current.parent)
				if (current.TryGetComponent<Anchor>(out var anchor) && anchor.HasTag(tag))
					return current;

			return null;
		}

		/// <summary>
		/// Whether a script may reach <paramref name="candidate"/>: never the sandbox boundary
		/// itself, nor anything above it.
		/// </summary>
		public static bool CanAccess(IScriptingContext ctx, Transform candidate) {
			if (!candidate)
				return false;

			// Not sandboxed (no session, or no anchor tagged with it): nothing to enforce.
			var boundary = Boundary(ctx, candidate);
			return boundary == null || boundary != candidate;
		}

		private static void Reparent(IScriptingContext ctx, Transform target, object value) {
			var parent = ctx.FromScript<Transform>(value);

			// An object may only move to another node of its own sandbox: no parent (scene root) and
			// no node at/above the boundary are accepted.
			if (!parent || !CanAccess(ctx, parent) || Boundary(ctx, target) == target) {
				Logger.LogWarning(
					$"Refused to reparent '{target.name}' to '{(parent ? parent.name : "null")}': "
					+ "it would leave the session sandbox."
				);
				return;
			}

			target.SetParent(parent, worldPositionStays: true);
		}
	}
}
