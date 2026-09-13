using System;
using Cysharp.Threading.Tasks;

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
		/// <summary>Async handler: (context, instance, args) → UniTask&lt;object&gt;.</summary>
		Func<IScriptingContext, object, object[], UniTask<object>> Handler { get; }
	}

	/// <summary>
	/// Flags that control the behaviour of a <see cref="IScriptingTypeProperty"/> in script
	/// backends and tooling. Additional flags may be added in future versions.
	/// </summary>
	[Flags]
	public enum ScriptingTypePropertyFlags {
		/// <summary>No special behaviour.</summary>
		None = 0,

		/// <summary>
		/// The getter may be evaluated safely during inspection (e.g. <c>console.log</c>).
		/// Leave unset for expensive or self-typed properties to avoid infinite recursion.
		/// </summary>
		InspectGetter = 1 << 0,

		/// <summary>
		/// The property is read-only at the scripting level (no assignment allowed).
		/// When using <see cref="ScriptingTypeConverterBuilder{T}"/>, a non-null setter always
		/// overrides this flag — the flag is stripped automatically.
		/// </summary>
		IsReadOnly = 1 << 1,
	}

	/// <summary>
	/// A property binding on a converted type instance.
	/// <see cref="Getter"/> is required; <see cref="Setter"/> may be <c>null</c>
	/// for read-only properties.
	/// </summary>
	public interface IScriptingTypeProperty {
		/// <summary>Returns the property value given the context and the C# instance.</summary>
		Func<IScriptingContext, object, object> Getter { get; }

		/// <summary>
		/// Sets the property; <c>null</c> if the property is read-only.
		/// Receives: context, the C# instance, and the new script value.
		/// </summary>
		Action<IScriptingContext, object, object> Setter { get; }

		/// <summary>Flags controlling backend and tooling behaviour for this property.</summary>
		ScriptingTypePropertyFlags Flags { get; }
	}

	/// <summary>
	/// An event binding on a converted type instance. Exposes <c>on</c>/<c>off</c>/<c>once</c>/<c>emit</c>
	/// (or language-specific equivalents) to scripts. The C# side provides
	/// <see cref="AddHandler"/>/<see cref="RemoveHandler"/>/<see cref="Emit"/> callbacks; the backend
	/// wraps them into the appropriate language API.
	/// </summary>
	public interface IScriptingTypeEventDefinition : IScriptingTypeBindingDefinition {
		/// <summary>
		/// Called by the backend when a script subscribes to this event.
		/// <para>Parameters: context, the C# instance, and a handler delegate that the</para>
		/// <para>backend has wrapped from the script function. The C# code must store</para>
		/// <para>this delegate and invoke it when the event fires.</para></summary>
		Action<IScriptingContext, object, Action<object[]>> AddHandler { get; }

		/// <summary>
		/// Called by the backend when a script subscribes with <c>once</c> flag.
		/// <para>Parameters: context, the C# instance, and a handler delegate.</para>
		/// <para>The C# code must store this delegate and invoke it exactly once,</para>
		/// <para>then automatically remove it.</para></summary>
		Action<IScriptingContext, object, Action<object[]>> AddOnceHandler { get; }

		/// <summary>
		/// Called by the backend when a script unsubscribes from this event.
		/// <para>Parameters: context, the C# instance, and the same handler delegate</para>
		/// <para>that was passed to <see cref="AddHandler"/> or <see cref="AddOnceHandler"/>.</para>
		/// <para>The C# code must remove it.</para></summary>
		Action<IScriptingContext, object, Action<object[]>> RemoveHandler { get; }

		/// <summary>
		/// Called by the backend when a script emits this event programmatically
		/// (e.g. <c>socket.emit('data', ...)</c>). Only invoked if <see cref="ScriptingTypeEventFlags.AllowEmit"/> is set.
		/// <para>Parameters: context, the C# instance, and the event arguments array.</para></summary>
		Action<IScriptingContext, object, object[]> Emit { get; }
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

	/// <summary>Base for all static bindings on a converted type.</summary>
	public interface IScriptingStaticBindingDefinition {
		/// <summary>Binding name.</summary>
		INameResolver Name { get; }
	}

	/// <summary>
	/// A static read-only value on a converted type (e.g. <c>Vector3.zero</c>).
	/// The getter receives the context and returns the value.
	/// </summary>
	public interface IScriptingStaticValueDefinition : IScriptingStaticBindingDefinition, IScriptingTypeProperty {
	}

	/// <summary>
	/// A static synchronous method on a converted type (e.g. <c>Vector3.Distance(a, b)</c>).
	/// </summary>
	public interface IScriptingStaticSyncMethodDefinition : IScriptingStaticBindingDefinition, IScriptingTypeSyncMethod {
	}

	/// <summary>
	/// A static asynchronous method on a converted type.
	/// Backends typically wrap the result in a language-native Promise.
	/// </summary>
	public interface IScriptingStaticAsyncMethodDefinition : IScriptingStaticBindingDefinition, IScriptingTypeAsyncMethod {
	}
}