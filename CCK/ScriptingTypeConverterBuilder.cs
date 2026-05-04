using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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
		private Func<IScriptingContext, T, object>           _toScript;
		private Func<IScriptingContext, object[], T>         _constructor;
		private IScriptingTypeDefaultDefinition              _default;

		private ScriptingTypeConverterBuilder() { }

		/// <summary>Begin building a converter for <typeparamref name="T"/>.</summary>
		public static ScriptingTypeConverterBuilder<T> Create() => new();

		// ── ToScript ──────────────────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> ToScript(Func<IScriptingContext, T, object> fn) { _toScript = fn; return this; }
		public ScriptingTypeConverterBuilder<T> ToScript(Func<T, object> fn)                    { _toScript = (_, v) => fn(v); return this; }

		// ── Constructor / Default ─────────────────────────────────────────

		/// <summary>
		/// Declare how to construct a <typeparamref name="T"/> from an ordered array of
		/// script arguments. The backend passes property values in <see cref="IScriptingTypeConverter.Bindings"/>
		/// order when the script provides a structured object.
		/// </summary>
		public ScriptingTypeConverterBuilder<T> SetConstructor(Func<IScriptingContext, object[], T> fn) { _constructor = fn; return this; }

		/// <summary>Declare a context-free constructor.</summary>
		public ScriptingTypeConverterBuilder<T> SetConstructor(Func<object[], T> fn) { _constructor = (_, args) => fn(args); return this; }

		/// <summary>Set a direct default value (returned as-is when needed).</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(T value) { _default = new DefaultPropertyDef(_ => (object)value); return this; }

		/// <summary>Set a context-free computed default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(Func<T> fn) { _default = new DefaultPropertyDef(_ => (object)fn()); return this; }

		/// <summary>Set a context-aware computed default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefault(Func<IScriptingContext, T> fn) { _default = new DefaultPropertyDef(ctx => (object)fn(ctx)); return this; }

		/// <summary>Set a context-free async default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefaultAsync(Func<Task<T>> fn) { _default = new DefaultAsyncDef((_, _2) => fn().ContinueWith(t => (object)t.Result)); return this; }

		/// <summary>Set a context-aware async default value.</summary>
		public ScriptingTypeConverterBuilder<T> SetDefaultAsync(Func<IScriptingContext, Task<T>> fn) { _default = new DefaultAsyncDef((ctx, _) => fn(ctx).ContinueWith(t => (object)t.Result)); return this; }

		// ── Synchronous methods ───────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<IScriptingContext, T, object[], object> h) {
			_bindings.Add(new SyncMethodDef(name, (ctx, inst, args) => h(ctx, (T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<IScriptingContext, T, object[]> h) {
			_bindings.Add(new SyncMethodDef(name, (ctx, inst, args) => { h(ctx, (T)inst, args); return null; }));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<T, object[], object> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, args) => h((T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<T, object[]> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, args) => { h((T)inst, args); return null; }));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Func<T, object> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, _2) => h((T)inst)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddMethod(NameResolver name, Action<T> h) {
			_bindings.Add(new SyncMethodDef(name, (_, inst, _2) => { h((T)inst); return null; }));
			return this;
		}

		// ── Asynchronous methods ──────────────────────────────────────────

		public ScriptingTypeConverterBuilder<T> AddAsyncMethod(NameResolver name, Func<IScriptingContext, T, object[], Task<object>> h) {
			_bindings.Add(new AsyncMethodDef(name, (ctx, inst, args) => h(ctx, (T)inst, args)));
			return this;
		}

		public ScriptingTypeConverterBuilder<T> AddAsyncMethod(NameResolver name, Func<T, object[], Task<object>> h) {
			_bindings.Add(new AsyncMethodDef(name, (_, inst, args) => h((T)inst, args)));
			return this;
		}

		// ── Properties ────────────────────────────────────────────────────

		/// <summary>Add a read-only property (value is snapshotted at conversion time).</summary>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name, Func<IScriptingContext, T, object> getter) {
			_bindings.Add(new PropertyDef(name, (ctx, inst) => getter(ctx, (T)inst), null));
			return this;
		}

		/// <summary>Add a read-only property (context-free).</summary>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name, Func<T, object> getter) {
			_bindings.Add(new PropertyDef(name, (_, inst) => getter((T)inst), null));
			return this;
		}

		/// <summary>Add a live read-write property with getter and setter.</summary>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name,
			Func<IScriptingContext, T, object>     getter,
			Action<IScriptingContext, T, object>   setter) {
			_bindings.Add(new PropertyDef(name,
				(ctx, inst) => getter(ctx, (T)inst),
				(ctx, inst, val) => setter(ctx, (T)inst, val)));
			return this;
		}

		/// <summary>Add a live read-write property (context-free).</summary>
		public ScriptingTypeConverterBuilder<T> AddProperty(NameResolver name,
			Func<T, object>    getter,
			Action<T, object>  setter) {
			_bindings.Add(new PropertyDef(name,
				(_, inst) => getter((T)inst),
				(_, inst, val) => setter((T)inst, val)));
			return this;
		}

		/// <summary>Build the converter.</summary>
		public IScriptingTypeConverter Build()
			=> new ConverterDef(_toScript, _constructor, _default, _bindings.ToArray());

		// ── Private implementations ───────────────────────────────────────

		private sealed class SyncMethodDef : IScriptingTypeBindingSyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object[], object> Handler { get; }
			public SyncMethodDef(INameResolver name, Func<IScriptingContext, object, object[], object> h) { Name = name; Handler = h; }
		}

		private sealed class AsyncMethodDef : IScriptingTypeBindingAsyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object[], Task<object>> Handler { get; }
			public AsyncMethodDef(INameResolver name, Func<IScriptingContext, object, object[], Task<object>> h) { Name = name; Handler = h; }
		}

		private sealed class PropertyDef : IScriptingTypeBindingPropertyDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object, object> Getter { get; }
			public bool IsReadOnly => Setter == null;
			public Action<IScriptingContext, object, object> Setter { get; }
			public PropertyDef(INameResolver name, Func<IScriptingContext, object, object> getter, Action<IScriptingContext, object, object> setter) {
				Name = name; Getter = getter; Setter = setter;
			}
		}

		private sealed class DefaultPropertyDef : IScriptingTypeDefaultPropertyDefinition {
			public INameResolver Name => new NameResolver("default");
			public Func<IScriptingContext, object, object> Getter { get; }
			public bool IsReadOnly => true;
			public Action<IScriptingContext, object, object> Setter => null;
			public DefaultPropertyDef(Func<IScriptingContext, object> getter) {
				Getter = (ctx, _) => getter(ctx);
			}
		}

		private sealed class DefaultAsyncDef : IScriptingTypeDefaultAsyncMethodDefinition {
			public INameResolver Name => new NameResolver("default");
			public Func<IScriptingContext, object, object[], Task<object>> Handler { get; }
			public DefaultAsyncDef(Func<IScriptingContext, object[], Task<object>> asyncHandler) {
				Handler = (ctx, _, args) => asyncHandler(ctx, args);
			}
		}

		private sealed class ConverterDef : IScriptingTypeConverter {
			private readonly Func<IScriptingContext, T, object>   _toScript;
			private readonly Func<IScriptingContext, object[], T> _constructor;

			public Type HandledType => typeof(T);
			public IReadOnlyList<IScriptingTypeBindingDefinition> Bindings { get; }
			public Func<IScriptingContext, object[], object> Constructor
				=> _constructor != null ? (ctx, args) => (object)_constructor(ctx, args) : null;
			public IScriptingTypeDefaultDefinition Default { get; }

			public ConverterDef(
				Func<IScriptingContext, T, object>            toScript,
				Func<IScriptingContext, object[], T>          constructor,
				IScriptingTypeDefaultDefinition               defaultDef,
				IScriptingTypeBindingDefinition[]             bindings) {
				_toScript    = toScript;
				_constructor = constructor;
				Default      = defaultDef;
				Bindings     = Array.AsReadOnly(bindings);
			}

			public object ToScript(IScriptingContext context, object value)
				=> _toScript != null ? _toScript(context, (T)value) : value;
		}
	}
}
