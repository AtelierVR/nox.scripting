using Nox.Scripting;
using UnityEngine;
using Nox.CCK.Utils;

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
				.AddVariable("gameObject", ctx => ctx.ScriptObject)
				.AddVariable("transform",  ctx => ctx.ScriptObject?.transform)
				.AddVariable("rigidbody",  ctx => ctx.ScriptObject?.GetComponent<Rigidbody>())
				.AddVariable("id",         ctx => ctx.ScriptObject?.GetId() ?? 0)
				.Build();
	}
}
