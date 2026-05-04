using System;
using System.Collections.Generic;

namespace Nox.Scripting {
	/// <summary>
	/// Converts values of a specific C# type to and from the backend-native
	/// script representation.
	///
	/// Register via <see cref="IScriptingAPI.RegisterConverter"/>.
	/// </summary>
	public interface IScriptingTypeConverter {
		/// <summary>The C# type this converter handles.</summary>
		Type HandledType { get; }

		/// <summary>
		/// Optional per-instance bindings (methods and properties) that backends
		/// should expose on each converted value.
		/// An empty list means the backend falls back to <see cref="ToScript"/> /
		/// <see cref="Constructor"/> for raw conversion.
		/// </summary>
		IReadOnlyList<IScriptingTypeBindingDefinition> Bindings { get; }

		/// <summary>
		/// Constructs a C# value of <see cref="HandledType"/> from an ordered array of
		/// script-provided arguments (e.g. <c>new Vector3(1, 2, 3)</c> → <c>args = [1, 2, 3]</c>).
		/// When the script passes a structured object (<c>{x, y, z}</c>), the backend extracts
		/// the property values in <see cref="Bindings"/> order before calling this constructor.
		/// May be <c>null</c> if the type is not constructable from scripts.
		/// </summary>
		Func<IScriptingContext, object[], object> Constructor { get; }

		/// <summary>
		/// The default value or factory returned when a script passes <c>null</c> or when
		/// <see cref="Constructor"/> is <c>null</c>. May be <c>null</c> if no default is set.
		/// Use <see cref="IScriptingTypeDefaultDefinition.Getter"/>,
		/// <see cref="IScriptingTypeDefaultDefinition.Handler"/>, or
		/// <see cref="IScriptingTypeDefaultDefinition.AsyncHandler"/> to resolve the actual value.
		/// </summary>
		IScriptingTypeDefaultDefinition Default { get; }

		/// <summary>
		/// Convert a C# value of <see cref="HandledType"/> to a script-native value
		/// suitable for the backend that owns the supplied <paramref name="context"/>.
		/// Only called when <see cref="Bindings"/> is empty.
		/// </summary>
		object ToScript(IScriptingContext context, object value);
	}
}
