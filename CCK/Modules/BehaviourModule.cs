using Nox.Scripting;
using UnityEngine;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"behaviour"</c> — access to the script's host GameObject and components.
	/// <code>
	/// import { gameObject, transform, rigidbody, id } from 'behaviour';
	/// transform.rotate(0, 1, 0);
	/// </code>
	/// </summary>
	public static class BehaviourModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("behaviour")
				.WithTags("session")
				.AddVariable("gameObject", ctx => (object)ctx.ScriptObject)
				.AddVariable("transform",  ctx => (object)ctx.ScriptObject?.transform)
				.AddVariable("rigidbody",  ctx => (object)ctx.ScriptObject?.GetComponent<Rigidbody>())
				.AddVariable("id",         ctx => (object)(ctx.ScriptObject?.GetInstanceID() ?? 0))
				.Build();
	}
}
