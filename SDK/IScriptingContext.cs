using System;
using Nox.Sessions;
using UnityEngine;

namespace Nox.Scripting {
	/// <summary>
	/// Per-instance context passed to method and variable handlers during script execution.
	/// Provides access to the active session, the script's host GameObject, and type
	/// conversion helpers supplied by the backend.
	/// </summary>
	public interface IScriptingContext {
		/// <summary>The backend that is executing this script instance.</summary>
		IScriptingBackend Backend { get; }

		/// <summary>
		/// The active session this script is running inside, or <c>null</c> for
		/// scripts that are not session-scoped.
		/// </summary>
		ISession Session { get; }

		/// <summary>
		/// The <see cref="GameObject"/> the script component is attached to,
		/// or <c>null</c> if there is no associated GameObject.
		/// </summary>
		GameObject ScriptObject { get; }

		/// <summary>Convert a C# value to a backend-native script value.</summary>
		object ToScript(object value);

		/// <summary>Convert a backend-native script value to a C# object of the requested type.</summary>
		object FromScript(object scriptValue, Type targetType);
	}
}