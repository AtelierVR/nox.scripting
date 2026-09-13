using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Nox.Scripting;

namespace Nox.CCK.Scripting {
	/// <summary>
	/// Fluent builder for <see cref="IScriptingTypeConverter"/> instances with optional
	/// per-instance bindings (methods and properties callable from scripts).
	///
	/// <example><code>
	/// IScriptingTypeConverter conv = ScriptingTypeConverterBuilder&lt;Vector3&gt;.Create()
	///     .AddProperty("x",         v => (object)v.x)
	///     .AddProperty("y",         v => (object)v.y)
	///     .AddProperty("z",         v => (object)v.z)
	///     .AddProperty("magnitude", v => (object)v.magnitude)
	///     .AddMethod("toString",    v => (object)v.ToString())
	///     .SetConstructor((_, args) => new Vector3(
	///         args.Length > 0 ? Convert.ToSingle(args[0]) : 0f,
	///         args.Length > 1 ? Convert.ToSingle(args[1]) : 0f,
	///         args.Length > 2 ? Convert.ToSingle(args[2]) : 0f))
	///     .SetDefault(Vector3.zero)
	///     .Build();
	/// </code></example>
	/// </summary>
	public sealed class ScriptingTypeConverterBuilder<T> {
		private readonly List<IScriptingTypeBindingDefinition> _bindings = new();
		private readonly List<IScriptingStaticBindingDefinition> _staticBindings = new();
		private Func<IScriptingContext, T, object> _toScript;
		private Func<IScriptingContext, object[], T> _constructor;
		private IScriptingTypeDefaultDefinition _default;

		private ScriptingTypeConverterBuilder() { }

		/// <summary>Begin building a converter for <typeparamref name="T"/>.</summary>
		public static ScriptingTypeConverterBuilder<T> Create()
			=> new();

		// ── ToScript ──────────────────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> ToScript(Func<IScriptingContext, T, object> fn) {
			_toScript = fn;
			return this;
		}
		public ScriptingTypeConverterBuilder<T> ToScript(Func<T, object> fn) {
			_toScript = (_, v) => fn(v);
			return this;
		}

		// ── Constructor / Default ─────────────────────────────────────────

		/// <summary>
		/// Declare how to construct a <typeparamref name="T"/> from an ordered array of
		/// script arguments. The backend passes property values in <see cref="IScriptingTypeConverter.Bindings"/>
		/// order when the script provides a structured object.
		/// </summary>
		public ScriptingTypeConverterBuilder<T> SetConstructor(Func<IScriptingContext, object[], T> fn) {
			_constructor = fn;
			return this;
		}

		/// <summary>Declare a context-free constructor.</summary>
		public ScriptingTypeConverterBuilder<T> SetConstructor(Func<object[], T> fn) {
			_constructor = (_, args) => fn(args);
			return this;
		}

		/// <summary>Set a direct default value (returned as-is when needed).</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(T value) {
			_default = new DefaultPropertyDef(_ => (object)value);
			return this;
		}

		/// <summary>Set a context-free computed default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(Func<T> fn) {
			_default = new DefaultPropertyDef(_ => fn());
			return this;
		}

		/// <summary>Set a context-aware computed default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(Func<IScriptingContext, T> fn) {
			_default = new DefaultPropertyDef(ctx => fn(ctx));
			return this;
		}

		/// <summary>Set a context-free async default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefaultAsync(Func<UniTask<T>> fn) {
			_default = new DefaultAsyncDef((_, _2) => fn().ContinueWith(t => (object)t));
			return this;
		}

		/// <summary>Set a context-aware async default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefaultAsync(Func<IScriptingContext, UniTask<T>> fn) {
			_default = new DefaultAsyncDef((ctx, _) => fn(ctx).ContinueWith(t => (object)t));
			return this;
		}

		// ── Synchronous methods ───────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<IScriptingContext, T, object[], object> h) {
			_bindings.Add(new SyncMethodDef(name, (ctx, inst, args) => h(ctx, (T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<IScriptingContext, T, object[]> h) {
			_bindings.Add(new SyncMethodDef(name, (ctx, inst, args) => {
				h(ctx, (T)inst, args);
				return null;
			}));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<T, object[], object> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, args) => h((T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<T, object[]> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, args) => {
				h((T)inst, args);
				return null;
			}));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<T, object> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, _2) => h((T)inst)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<T> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, _2) => {
				h((T)inst);
				return null;
			}));
			return this;
		}

		// ── Asynchronous methods ──────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> AddAsyncMethod(NameResolver name, Func<IScriptingContext, T, object[], UniTask<object>> h) {
			_bindings.Add(new AsyncMethodDef(name, (ctx, inst, args) => h(ctx, (T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddAsyncMethod(NameResolver name, Func<T, object[], UniTask<object>> h) {
			_bindings.Add(new AsyncMethodDef(name, (_, inst, args) => h((T)inst, args)));
			return this;
		}

		// ── Properties ────────────────────────────────────────────────────

		/// <summary>Add a read-only property (context-aware).</summary>
		/// <param name="flags">Optional <see cref="PropertyFlags"/> (e.g. <see cref="PropertyFlags.InspectGetter"/>).
		/// <see cref="PropertyFlags.IsReadOnly"/> is always set automatically for read-only overloads.</param>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name, Func<IScriptingContext, T, object> getter, ScriptingTypePropertyFlags flags = ScriptingTypePropertyFlags.None) {
			_bindings.Add(new PropertyDef(name, (ctx, inst) => getter(ctx, (T)inst), null, flags));
			return this;
		}

		/// <summary>Add a read-only property (context-free).</summary>
		/// <param name="flags">Optional <see cref="PropertyFlags"/>.</param>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name, Func<T, object> getter, ScriptingTypePropertyFlags flags = ScriptingTypePropertyFlags.None) {
			_bindings.Add(new PropertyDef(name, (_, inst) => getter((T)inst), null, flags));
			return this;
		}

		/// <summary>Add a read-write property (context-aware).</summary>
		/// <param name="flags">Optional <see cref="PropertyFlags"/>. <see cref="PropertyFlags.IsReadOnly"/> is stripped automatically when a setter is provided.</param>
		public ScriptingTypeConverterBuilder<T> AddProperty(
			NameResolver                         name,
			Func<IScriptingContext, T, object>   getter,
			Action<IScriptingContext, T, object> setter,
			ScriptingTypePropertyFlags           flags = ScriptingTypePropertyFlags.None
		) {
			_bindings.Add(new PropertyDef(name,
				(ctx, inst) => getter(ctx, (T)inst),
				(ctx, inst, val) => setter(ctx, (T)inst, val),
				flags));
			return this;
		}

		/// <summary>Add a read-write property (context-free).</summary>
		/// <param name="flags">Optional <see cref="PropertyFlags"/>. <see cref="PropertyFlags.IsReadOnly"/> is stripped automatically when a setter is provided.</param>
		public ScriptingTypeConverterBuilder<T> AddProperty(
			NameResolver               name,
			Func<T, object>            getter,
			Action<T, object>          setter,
			ScriptingTypePropertyFlags flags = ScriptingTypePropertyFlags.None
		) {
			_bindings.Add(new PropertyDef(name,
				(_, inst) => getter((T)inst),
				(_, inst, val) => setter((T)inst, val),
				flags));
			return this;
		}

		// ── Static methods / values ──────────────────────────────────────────

		/// <summary>Add an event binding. The C# side provides AddHandler/RemoveHandler/AddOnceHandler/Emit callbacks
		/// that the backend wraps into the appropriate language API (e.g. on/off/once/emit for JS,
		/// +=/-= for C#, etc.). The handler receives an array of arguments (object[]).
		/// <para>When a script subscribes via <c>on</c>, <paramref name="addHandler"/> is called with the
		/// script-wrapped handler. The C# code must store and invoke this delegate when the
		/// event fires.</para>
		/// <para>When a script subscribes via <c>once</c>, <paramref name="addOnceHandler"/> is called.
		/// The C# code must invoke this delegate exactly once and then auto-remove it.</para>
		/// <para>When a script unsubscribes via <c>off</c>, <paramref name="removeHandler"/> is called with
		/// the same delegate for removal.</para>
		/// <para>When a script emits via <c>emit</c> (if <see cref="ScriptingTypeEventFlags.AllowEmit"/>),
		/// <paramref name="emit"/> is called with the event arguments.</para></summary>
		/// <param name="name">Event name (e.g. "data", "error", "connected").</param>
		/// <param name="addHandler">Called when a script subscribes via <c>on</c>. Receives context, instance, and the handler to store.</param>
		/// <param name="addOnceHandler">Called when a script subscribes via <c>once</c>. Receives context, instance, and the handler to store and invoke once.</param>
		/// <param name="removeHandler">Called when a script unsubscribes via <c>off</c>. Receives context, instance, and the handler to remove.</param>
		/// <param name="emit">Called when a script emits via <c>emit</c> (if AllowEmit flag). Receives context, instance, and event arguments.</param>
		/// <param name="flags">Event behaviour flags (see <see cref="ScriptingTypeEventFlags"/>).</param>
		public ScriptingTypeConverterBuilder<T> AddEvent(
			NameResolver                                    name,
			Action<IScriptingContext, T, Action<object[]>>  addHandler,
			Action<IScriptingContext, T, Action<object[]>>  addOnceHandler,
			Action<IScriptingContext, T, Action<object[]>>  removeHandler,
			Action<IScriptingContext, T, object[]>          emit
		) {
			_bindings.Add(new EventDef(name, 
				(ctx, inst, h) => addHandler(ctx, (T)inst, h),
				(ctx, inst, h) => addOnceHandler(ctx, (T)inst, h),
				(ctx, inst, h) => removeHandler(ctx, (T)inst, h),
				(ctx, inst, args) => emit(ctx, (T)inst, args)
			));
			return this;
		}

		/// <summary>Context-free overload for <see cref="AddEvent"/>.</summary>
		public ScriptingTypeConverterBuilder<T> AddEvent(
			NameResolver                     name,
			Action<T, Action<object[]>>      addHandler,
			Action<T, Action<object[]>>      addOnceHandler,
			Action<T, Action<object[]>>      removeHandler,
			Action<T, object[]>              emit
		) {
			_bindings.Add(new EventDef(name,
				(_, inst, h) => addHandler((T)inst, h),
				(_, inst, h) => addOnceHandler((T)inst, h),
				(_, inst, h) => removeHandler((T)inst, h),
				(_, inst, args) => emit((T)inst, args)
			));
			return this;
		}

		// ── Static methods / values ──────────────────────────────────────────

		/// <summary>Add a static synchronous method (e.g. <c>Vector3.Distance(a, b)</c>).</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticMethod(NameResolver name, Func<IScriptingContext, object[], object> h) {
			_staticBindings.Add(new StaticSyncMethodDef(name, h));
			return this;
		}

		/// <summary>Add a context-free static synchronous method.</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticMethod(NameResolver name, Func<object[], object> h) {
			_staticBindings.Add(new StaticSyncMethodDef(name, (_, args) => h(args)));
			return this;
		}

		/// <summary>Add a static asynchronous method.</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticAsyncMethod(NameResolver name, Func<IScriptingContext, object[], UniTask<object>> h) {
			_staticBindings.Add(new StaticAsyncMethodDef(name, h));
			return this;
		}

		/// <summary>Add a context-free static asynchronous method.</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticAsyncMethod(NameResolver name, Func<object[], UniTask<object>> h) {
			_staticBindings.Add(new StaticAsyncMethodDef(name, (_, args) => h(args)));
			return this;
		}

		/// <summary>Add a static read-only constant value (e.g. <c>Vector3.zero</c>).</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticValue(NameResolver name, Func<object> getter) {
			_staticBindings.Add(new StaticValueDef(name, _ => getter()));
			return this;
		}

		/// <summary>Add a context-aware static read-only value.</summary>
		public ScriptingTypeConverterBuilder<T> AddStaticValue(NameResolver name, Func<IScriptingContext, object> getter) {
			_staticBindings.Add(new StaticValueDef(name, getter));
			return this;
		}

		/// <summary>Build the converter.</summary>
		public IScriptingTypeConverter Build()
			=> new ConverterDef(_toScript, _constructor, _default, _bindings.ToArray(), _staticBindings.ToArray());

		// ── Private implementations ───────────────────────────────────────

		private sealed class SyncMethodDef : IScriptingTypeBindingSyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object[], object> Handler { get; }
			public SyncMethodDef(INameResolver name, Func<IScriptingContext, object, object[], object> h) {
				Name    = name;
				Handler = h;
			}
		}

		private sealed class AsyncMethodDef : IScriptingTypeBindingAsyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object[], UniTask<object>> Handler { get; }
			public AsyncMethodDef(INameResolver name, Func<IScriptingContext, object, object[], UniTask<object>> h) {
				Name    = name;
				Handler = h;
			}
		}

		private sealed class PropertyDef : IScriptingTypeBindingPropertyDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object> Getter { get; }
			public Action<IScriptingContext, object, object> Setter { get; }
			public ScriptingTypePropertyFlags Flags { get; }
			public PropertyDef(INameResolver name, Func<IScriptingContext, object, object> getter, Action<IScriptingContext, object, object> setter, ScriptingTypePropertyFlags flags = ScriptingTypePropertyFlags.None) {
				Name   = name;
				Getter = getter;
				Setter = setter;
				// Setter presence overrides IsReadOnly: strip the flag when a setter is provided,
				// force it when there is none.
				Flags = setter == null
					? flags | ScriptingTypePropertyFlags.IsReadOnly
					: flags & ~ScriptingTypePropertyFlags.IsReadOnly;
			}
		}

		private sealed class DefaultPropertyDef : IScriptingTypeDefaultPropertyDefinition {
			public INameResolver Name
				=> new NameResolver("default");
			public Func<IScriptingContext, object, object> Getter { get; }
			public Action<IScriptingContext, object, object> Setter
				=> null;
			public ScriptingTypePropertyFlags Flags
				=> ScriptingTypePropertyFlags.IsReadOnly;
			public DefaultPropertyDef(Func<IScriptingContext, object> getter) {
				Getter = (ctx, _) => getter(ctx);
			}
		}

		private sealed class DefaultAsyncDef : IScriptingTypeDefaultAsyncMethodDefinition {
			public INameResolver Name
				=> new NameResolver("default");
			public Func<IScriptingContext, object, object[], UniTask<object>> Handler { get; }
			public DefaultAsyncDef(Func<IScriptingContext, object[], UniTask<object>> asyncHandler) {
				Handler = (ctx, _, args) => asyncHandler(ctx, args);
			}
		}

		private sealed class StaticValueDef : IScriptingStaticValueDefinition {
			public INameResolver Name { get; }
			// instance is always null for static values
			public Func<IScriptingContext, object, object> Getter { get; }
			public Action<IScriptingContext, object, object> Setter
				=> null;
			public ScriptingTypePropertyFlags Flags
				=> ScriptingTypePropertyFlags.IsReadOnly;
			public StaticValueDef(INameResolver name, Func<IScriptingContext, object> getter) {
				Name   = name;
				Getter = (ctx, _) => getter(ctx);
			}
		}

		private sealed class StaticSyncMethodDef : IScriptingStaticSyncMethodDefinition {
			public INameResolver Name { get; }
			// instance is always null for static methods
			public Func<IScriptingContext, object, object[], object> Handler { get; }
			public StaticSyncMethodDef(INameResolver name, Func<IScriptingContext, object[], object> h) {
				Name    = name;
				Handler = (ctx, _, args) => h(ctx, args);
			}
		}

		private sealed class StaticAsyncMethodDef : IScriptingStaticAsyncMethodDefinition {
			public INameResolver Name { get; }
			// instance is always null for static methods
			public Func<IScriptingContext, object, object[], UniTask<object>> Handler { get; }
			public StaticAsyncMethodDef(INameResolver name, Func<IScriptingContext, object[], UniTask<object>> h) {
				Name    = name;
				Handler = (ctx, _, args) => h(ctx, args);
			}
		}

		private sealed class EventDef : IScriptingTypeEventDefinition {
			public INameResolver Name { get; }
			public Action<IScriptingContext, object, Action<object[]>> AddHandler { get; }
			public Action<IScriptingContext, object, Action<object[]>> AddOnceHandler { get; }
			public Action<IScriptingContext, object, Action<object[]>> RemoveHandler { get; }
			public Action<IScriptingContext, object, object[]> Emit { get; }
			public EventDef(INameResolver name,
				Action<IScriptingContext, object, Action<object[]>> addHandler,
				Action<IScriptingContext, object, Action<object[]>> addOnceHandler,
				Action<IScriptingContext, object, Action<object[]>> removeHandler,
				Action<IScriptingContext, object, object[]> emit) {
				Name          = name;
				AddHandler    = addHandler;
				AddOnceHandler = addOnceHandler;
				RemoveHandler = removeHandler;
				Emit          = emit;
			}
		}

		private sealed class ConverterDef : IScriptingTypeConverter {
			private readonly Func<IScriptingContext, T, object> _toScript;
			private readonly Func<IScriptingContext, object[], T> _constructor;

			public Type HandledType
				=> typeof(T);
			public IReadOnlyList<IScriptingTypeBindingDefinition> Bindings { get; }
			public IReadOnlyList<IScriptingStaticBindingDefinition> StaticBindings { get; }
			public Func<IScriptingContext, object[], object> Constructor
				=> _constructor != null ? (ctx, args) => (object)_constructor(ctx, args) : null;
			public IScriptingTypeDefaultDefinition Default { get; }

			public ConverterDef(
				Func<IScriptingContext, T, object>   toScript,
				Func<IScriptingContext, object[], T> constructor,
				IScriptingTypeDefaultDefinition      defaultDef,
				IScriptingTypeBindingDefinition[]    bindings,
				IScriptingStaticBindingDefinition[]  staticBindings
			) {
				_toScript      = toScript;
				_constructor   = constructor;
				Default        = defaultDef;
				Bindings       = Array.AsReadOnly(bindings);
				StaticBindings = Array.AsReadOnly(staticBindings);
			}

			public object ToScript(IScriptingContext context, object value)
				=> _toScript != null ? _toScript(context, (T)value) : value;
		}
	}
}