using System.Collections.Generic;

namespace Nox.Scripting {
	/// <summary>
	/// Interface that scripting engine backends (Jint/JavaScript, MoonSharp/Lua, etc.)
	/// implement to receive live updates from the scripting registry.
	///
	/// Register via <see cref="IScriptingAPI.RegisterBackend"/>.
	/// </summary>
	public interface IScriptingBackend {
		/// <summary>
		/// Language identifier for this backend, e.g. <c>"javascript"</c>, <c>"lua"</c>.
		/// </summary>
		string Language { get; }

		/// <summary>
		/// Context tags for this backend instance, e.g. <c>"session"</c>, <c>"avatar"</c>.
		/// Used to filter which modules are bound: a module is bound if its Tags list is empty
		/// OR if it shares at least one tag with this backend.
		/// An empty list here means the backend accepts all modules.
		/// </summary>
		IReadOnlyList<string> Tags { get; }

		/// <summary>Called when a module is registered or replaced in the registry.</summary>
		void OnModuleRegistered(IScriptingModuleDefinition definition);

		/// <summary>Called when a module is removed from the registry.</summary>
		void OnModuleUnregistered(INameResolver id);

		/// <summary>Called when a type converter is registered or replaced.</summary>
		void OnConverterRegistered(IScriptingTypeConverter converter);
	}
}
