using System;
using System.Collections.Generic;
using Nox.CCK.Events;

namespace Nox.Scripting {
	/// <summary>
	/// Central registry interface exposed by the <c>nox.scripting</c> mod.
	/// Retrieve it via <c>CoreAPI.ModAPI.GetMod("scripting").GetInstance&lt;IScriptingAPI&gt;()</c>.
	/// </summary>
	public interface IScriptingAPI {
		/// <summary>All currently registered module definitions.</summary>
		IReadOnlyList<IScriptingModuleDefinition> Modules { get; }

		/// <summary>All currently registered type converters.</summary>
		IReadOnlyList<IScriptingTypeConverter> Converters { get; }

		/// <summary>
		/// Register a module definition. If a module with the same Id already exists, it is replaced.
		/// All registered backends are notified immediately.
		/// </summary>
		void RegisterModule(IScriptingModuleDefinition definition);

		/// <summary>Unregister a module by its <see cref="IScriptingModuleDefinition.Id"/>. All backends are notified.</summary>
		void UnregisterModule(INameResolver id);

		/// <summary>
		/// Register a type converter. Replaces any existing converter for the same <see cref="IScriptingTypeConverter.HandledType"/>.
		/// All registered backends are notified immediately.
		/// </summary>
		void RegisterConverter(IScriptingTypeConverter converter);

		/// <summary>Unregister a type converter.</summary>
		void UnregisterConverter(IScriptingTypeConverter converter);

		/// <summary>
		/// Register a scripting backend. The backend is immediately notified of all already-registered modules and converters.
		/// </summary>
		void RegisterBackend(IScriptingBackend backend);

		/// <summary>Unregister a scripting backend.</summary>
		void UnregisterBackend(IScriptingBackend backend);

		/// <summary>Fired when a new module definition is registered (or an existing one replaced).</summary>
		NoxEvent<IScriptingModuleDefinition> OnModuleRegistered { get; }

		/// <summary>Fired when a module definition is unregistered.</summary>
		NoxEvent<INameResolver> OnModuleUnregistered { get; }

		/// <summary>Fired when a type converter is registered (or replaced).</summary>
		NoxEvent<IScriptingTypeConverter> OnConverterRegistered { get; }
	}
}