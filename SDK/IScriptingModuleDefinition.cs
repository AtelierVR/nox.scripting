using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nox.Scripting {
	/// <summary>
	/// A named group of scripting bindings (methods and variables) that can be
	/// imported by scripts as a module, e.g. <c>import { log } from "console"</c>.
	/// </summary>
	public interface IScriptingModuleDefinition {
		/// <summary>The module identifier, e.g. <c>"console"</c>, <c>"players"</c>.</summary>
		INameResolver Id { get; }

		/// <summary>All bindings exported by this module.</summary>
		IReadOnlyList<IScriptingBindingDefinition> Bindings { get; }

		/// <summary>
		/// Optional tags restricting which backends receive this module.
		/// An empty list means the module is available to all backends.
		/// A non-empty list means the backend must share at least one tag.
		/// </summary>
		IReadOnlyList<string> Tags { get; }
	}

	/// <summary>Base descriptor for a single exported binding inside a module.</summary>
	public interface IScriptingBindingDefinition {
		/// <summary>Exported name of the binding.</summary>
		INameResolver Name { get; }
	}

	/// <summary>
	/// A synchronous method binding. The <see cref="Handler"/> is invoked each time the
	/// script calls this export.
	/// </summary>
	public interface IScriptingSyncMethodDefinition : IScriptingBindingDefinition {
		/// <summary>
		/// Handler called on each invocation.
		/// <para>Parameters: the current <see cref="IScriptingContext"/> and the caller's arguments.</para>
		/// <para>Return <c>null</c> for void methods.</para>
		/// </summary>
		Func<IScriptingContext, object[], object> Handler { get; }
	}

	/// <summary>
	/// An asynchronous method binding. Backends typically wrap the returned
	/// <see cref="Task{T}"/> in a language-native Promise.
	/// </summary>
	public interface IScriptingAsyncMethodDefinition : IScriptingBindingDefinition {
		/// <summary>
		/// Async handler called on each invocation.
		/// <para>Return <c>null</c> to resolve the promise with <c>null</c>.</para>
		/// </summary>
		Func<IScriptingContext, object[], Task<object>> Handler { get; }
	}

	/// <summary>
	/// A variable (or constant) binding whose value is resolved once at engine-creation
	/// time via <see cref="Getter"/>.
	/// </summary>
	public interface IScriptingPropertyDefinition : IScriptingBindingDefinition {
		/// <summary>Returns the variable's current value for the given context.</summary>
		Func<IScriptingContext, object> Getter { get; }

		/// <summary>Setter invoked when scripts assign to this variable, or <c>null</c> if read-only.</summary>
		Action<IScriptingContext, object> Setter { get; }
	}

	/// <summary>
	/// A type-converter binding that exports a named class-like object built from an
	/// <see cref="IScriptingTypeConverter"/> (static methods, static values, constructor via
	/// <c>TypeName.from(…)</c>). Scripts import it as <c>import { Vector3 } from 'unity'</c>.
	/// </summary>
	public interface IScriptingTypeConverterDefinition : IScriptingBindingDefinition {
		/// <summary>The converter whose static bindings and constructor are exported.</summary>
		IScriptingTypeConverter Converter { get; }
	}

	/// <summary>
	/// The module's default export, importable with <c>import X from "module"</c>.
	/// Exactly one of <see cref="Getter"/>, <see cref="Handler"/>, or
	/// <see cref="AsyncHandler"/> will be non-null, determining whether the default
	/// export is a plain value, a synchronous callable, or an async callable.
	/// </summary>
	public interface IScriptingDefaultDefinition : IScriptingBindingDefinition {
		/// <summary>Non-null when the default export is a plain value (not callable).</summary>
		Func<IScriptingContext, object> Getter { get; }

		/// <summary>Non-null when the default export is a synchronous function.</summary>
		Func<IScriptingContext, object[], object> Handler { get; }

		/// <summary>Non-null when the default export is an async function.</summary>
		Func<IScriptingContext, object[], Task<object>> AsyncHandler { get; }
	}
}