using System;
using System.Threading.Tasks;

namespace Nox.Scripting {
	/// <summary>
	/// Base descriptor for a single binding exposed on instances of a converted type.
	/// Unlike module bindings (<see cref="IScriptingBindingDefinition"/>), these handlers
	/// receive the underlying C# instance as a second parameter.
	/// </summary>
	public interface IScriptingTypeBindingDefinition {
		/// <summary>
		/// Binding name, supporting all <see cref="INameResolver"/> naming styles.
		/// The backend chooses which style to use when exposing the binding to scripts.
		/// </summary>
		INameResolver Name { get; }
	}

	/// <summary>
	/// A synchronous method binding on a converted type instance.
	/// <para>Handler signature: <c>(context, instance, args) → result</c></para>
	/// Return <c>null</c> for void methods.
	/// </summary>
	public interface IScriptingTypeDefaultDefinition {
	}

	/// <summary>
	/// A synchronous method binding on a converted type instance.
	/// <para>Handler signature: <c>(context, instance, args) → result</c></para>
	/// Return <c>null</c> for void methods.
	/// </summary>
	public interface IScriptingTypeSyncMethod {
		/// <summary>Invoked each time the script calls this method.</summary>
		Func<IScriptingContext, object, object[], object> Handler { get; }
	}

	/// <summary>
	/// An asynchronous method binding on a converted type instance.
	/// Backends typically wrap the result in a language-native Promise.
	/// </summary>
	public interface IScriptingTypeAsyncMethod {
		/// <summary>Async handler: (context, instance, args) → Task&lt;object&gt;.</summary>
		Func<IScriptingContext, object, object[], Task<object>> Handler { get; }
	}

	/// <summary>
	/// A property binding on a converted type instance.
	/// <see cref="IScriptingTypeProperty.Getter"/> is required; <see cref="IScriptingTypeProperty.Setter"/> may be <c>null</c>
	/// for read-only properties.
	/// </summary>
	public interface IScriptingTypeProperty {
		/// <summary>Returns the property value given the context and the C# instance.</summary>
		Func<IScriptingContext, object, object> Getter { get; }

		/// <summary>
		/// Whether the property is read-only (i.e. has no setter).
		/// Backends may use this to determine whether to allow assignment in scripts.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// Sets the property; <c>null</c> if the property is read-only.
		/// Receives: context, the C# instance, and the new script value.
		/// </summary>
		Action<IScriptingContext, object, object> Setter { get; }
	}

	/// <summary>
	/// A synchronous method binding on a converted type instance.
	/// <para>Handler signature: <c>(context, instance, args) → result</c></para>
	/// Return <c>null</c> for void methods.
	/// </summary>
	public interface IScriptingTypeBindingSyncMethodDefinition : IScriptingTypeSyncMethod, IScriptingTypeBindingDefinition {
	}

	/// <summary>
	/// An asynchronous method binding on a converted type instance.
	/// Backends typically wrap the result in a language-native Promise.
	/// </summary>
	public interface IScriptingTypeBindingAsyncMethodDefinition : IScriptingTypeAsyncMethod, IScriptingTypeBindingDefinition {
	}

	/// <summary>
	/// A property binding on a converted type instance.
	/// <see cref="Getter"/> is required; <see cref="Setter"/> may be <c>null</c>
	/// for read-only properties.
	/// </summary>
	public interface IScriptingTypeBindingPropertyDefinition : IScriptingTypeProperty, IScriptingTypeBindingDefinition {
	}

	/// <summary>
	/// The default export of a converted type, used when scripts call the instance directly
	/// (e.g. <c>myVector()</c> instead of <c>myVector.magnitude()</c>). Backends typically
	/// treat this as a special case and allow it to be either a plain value or a callable.
	/// </summary>
	public interface IScriptingTypeDefaultSyncMethodDefinition : IScriptingTypeDefaultDefinition, IScriptingTypeSyncMethod {
	}

	/// <summary>
	/// The default export of a converted type, used when scripts call the instance directly
	/// (e.g. <c>myVector()</c> instead of <c>myVector.magnitude()</c>). Backends typically
	/// treat this as a special case and allow it to be either a plain value or a callable.
	/// </summary>
	public interface IScriptingTypeDefaultAsyncMethodDefinition : IScriptingTypeDefaultDefinition, IScriptingTypeAsyncMethod {
	}

	/// <summary>
	/// The default export of a converted type, used when scripts call the instance directly
	/// (e.g. <c>myVector()</c> instead of <c>myVector.magnitude()</c>). Backends typically
	/// treat this as a special case and allow it to be either a plain value or a callable.
	/// </summary>
	public interface IScriptingTypeDefaultPropertyDefinition : IScriptingTypeDefaultDefinition, IScriptingTypeProperty {
	}
}