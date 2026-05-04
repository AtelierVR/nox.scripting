using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Nox.Scripting;

namespace Nox.CCK.Scripting {
	/// <summary>
	/// Fluent builder for creating <see cref="IScriptingModuleDefinition"/> instances.
	///
	/// <example><code>
	/// var module = ScriptingModuleBuilder.Create("console")
	///     .AddMethod("log",  (ctx, args) => Logger.Log(string.Join(" ", args)))
	///     .AddMethod("warn", (ctx, args) => Logger.LogWarning(string.Join(" ", args)))
	///     .Build();
	/// scriptingAPI.RegisterModule(module);
	/// </code></example>
	/// </summary>
	public sealed class ScriptingModuleBuilder {
		private readonly NameResolver _id;
		private readonly List<IScriptingBindingDefinition> _bindings = new();
		private string[] _tags = Array.Empty<string>();

		private ScriptingModuleBuilder(NameResolver id)
			=> _id = id;

		/// <summary>Begin building a module with the given identifier.</summary>
		public static ScriptingModuleBuilder Create(NameResolver id)
			=> new(id);

		/// <summary>
		/// Restrict this module to backends that share at least one of the given tags.
		/// Leave empty (default) to make the module available to all backends.
		/// </summary>
		public ScriptingModuleBuilder WithTags(params string[] tags) {
			_tags = tags ?? Array.Empty<string>();
			return this;
		}

		// ── Synchronous methods ──────────────────────────────────────────────

		/// <summary>
		/// Add a synchronous method that receives the active <see cref="IScriptingContext"/>
		/// and the caller's arguments. Return <c>null</c> for void methods.
		/// </summary>
		public ScriptingModuleBuilder AddMethod(NameResolver name, Func<IScriptingContext, object[], object> handler) {
			_bindings.Add(new SyncMethodDef(name, handler));
			return this;
		}

		/// <summary>Add a synchronous void method that receives the active context and arguments.</summary>
		public ScriptingModuleBuilder AddMethod(NameResolver name, Action<IScriptingContext, object[]> handler) {
			_bindings.Add(new SyncMethodDef(name, (ctx, args) => {
				handler(ctx, args);
				return null;
			}));
			return this;
		}

		/// <summary>Add a context-free synchronous method (context is ignored).</summary>
		public ScriptingModuleBuilder AddMethod(NameResolver name, Func<object[], object> handler) {
			_bindings.Add(new SyncMethodDef(name, (_, args) => handler(args)));
			return this;
		}

		/// <summary>Add a context-free synchronous void method.</summary>
		public ScriptingModuleBuilder AddMethod(NameResolver name, Action<object[]> handler) {
			_bindings.Add(new SyncMethodDef(name, (_, args) => {
				handler(args);
				return null;
			}));
			return this;
		}

		// ── Asynchronous methods ─────────────────────────────────────────────

		/// <summary>
		/// Add an asynchronous method. Backends typically wrap the result in a
		/// language-native Promise.
		/// </summary>
		public ScriptingModuleBuilder AddAsyncMethod(NameResolver name, Func<IScriptingContext, object[], Task<object>> handler) {
			_bindings.Add(new AsyncMethodDef(name, handler));
			return this;
		}

		/// <summary>Add a context-free asynchronous method.</summary>
		public ScriptingModuleBuilder AddAsyncMethod(NameResolver name, Func<object[], Task<object>> handler) {
			_bindings.Add(new AsyncMethodDef(name, (_, args) => handler(args)));
			return this;
		}

		// ── Variables ────────────────────────────────────────────────────────

		/// <summary>
		/// Add a variable whose value is resolved once per engine instance via
		/// <paramref name="getter"/>. Optionally provide a <paramref name="setter"/>
		/// for writable variables.
		/// </summary>
		public ScriptingModuleBuilder AddVariable(
			NameResolver                      name,
			Func<IScriptingContext, object>   getter,
			Action<IScriptingContext, object> setter = null
		) {
			_bindings.Add(new VariableDef(name, getter, setter));
			return this;
		}

		/// <summary>Add a context-free variable (context is ignored during get/set).</summary>
		public ScriptingModuleBuilder AddVariable(
			NameResolver   name,
			Func<object>   getter,
			Action<object> setter = null
		) {
			_bindings.Add(new VariableDef(
				name,
				_ => getter(),
				setter != null ? (_, v) => setter(v) : (Action<IScriptingContext, object>)null
			));
			return this;
		}

		/// <summary>Add a static (constant) variable.</summary>
		public ScriptingModuleBuilder AddVariable(NameResolver name, object value) {
			_bindings.Add(new VariableDef(name, _ => value, null));
			return this;
		}

		// ── Default export ───────────────────────────────────────────────────

		/// <summary>
		/// Set the module's default export to a plain value, importable with
		/// <c>import X from "module"</c>. Only one default export per module is allowed.
		/// </summary>
		public ScriptingModuleBuilder SetDefault(Func<IScriptingContext, object> getter) {
			_bindings.Add(new DefaultDef(getter, null, null));
			return this;
		}

		/// <summary>Set the module's default export to a context-free value.</summary>
		public ScriptingModuleBuilder SetDefault(Func<object> getter) {
			_bindings.Add(new DefaultDef(_ => getter(), null, null));
			return this;
		}

		/// <summary>
		/// Set the module's default export to a synchronous callable, importable with
		/// <c>import X from "module"</c> and invocable as <c>X(...)</c> or <c>new X(...)</c>.
		/// </summary>
		public ScriptingModuleBuilder SetDefault(Func<IScriptingContext, object[], object> handler) {
			_bindings.Add(new DefaultDef(null, handler, null));
			return this;
		}

		/// <summary>Set the module's default export to a context-free synchronous callable.</summary>
		public ScriptingModuleBuilder SetDefault(Func<object[], object> handler) {
			_bindings.Add(new DefaultDef(null, (_, args) => handler(args), null));
			return this;
		}

		/// <summary>
		/// Set the module's default export to an async callable. Backends wrap the result
		/// in a Promise.
		/// </summary>
		public ScriptingModuleBuilder SetDefaultAsync(Func<IScriptingContext, object[], Task<object>> handler) {
			_bindings.Add(new DefaultDef(null, null, handler));
			return this;
		}

		/// <summary>Set the module's default export to a context-free async callable.</summary>
		public ScriptingModuleBuilder SetDefaultAsync(Func<object[], Task<object>> handler) {
			_bindings.Add(new DefaultDef(null, null, (_, args) => handler(args)));
			return this;
		}

		// ── Build ────────────────────────────────────────────────────────────

		/// <summary>Build and return the immutable <see cref="IScriptingModuleDefinition"/>.</summary>
		public IScriptingModuleDefinition Build()
			=> new ModuleDef(_id, _bindings, _tags);

		// ── Private implementation types ─────────────────────────────────────

		private sealed class ModuleDef : IScriptingModuleDefinition {
			public INameResolver Id { get; }

			public IReadOnlyList<IScriptingBindingDefinition> Bindings { get; }

			public IReadOnlyList<string> Tags { get; }

			public ModuleDef(NameResolver id, List<IScriptingBindingDefinition> bindings, string[] tags) {
				Id       = id;
				Bindings = new ReadOnlyCollection<IScriptingBindingDefinition>(bindings);
				Tags     = Array.AsReadOnly(tags);
			}
		}

		private sealed class SyncMethodDef : IScriptingSyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object[], object> Handler { get; }

			public SyncMethodDef(NameResolver name, Func<IScriptingContext, object[], object> handler) {
				Name    = name;
				Handler = handler;
			}
		}

		private sealed class AsyncMethodDef : IScriptingAsyncMethodDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object[], Task<object>> Handler { get; }

			public AsyncMethodDef(NameResolver name, Func<IScriptingContext, object[], Task<object>> handler) {
				Name    = name;
				Handler = handler;
			}
		}

		private sealed class VariableDef : IScriptingVariableDefinition {
			public INameResolver Name { get; }
			public Func<IScriptingContext, object> Getter { get; }
			public Action<IScriptingContext, object> Setter { get; }

			public VariableDef(
				NameResolver                      name,
				Func<IScriptingContext, object>   getter,
				Action<IScriptingContext, object> setter
			) {
				Name   = name;
				Getter = getter;
				Setter = setter;
			}
		}

		private sealed class DefaultDef : IScriptingDefaultDefinition {
			public INameResolver Name { get; } = new NameResolver("default");
			public Func<IScriptingContext, object> Getter { get; }
			public Func<IScriptingContext, object[], object> Handler { get; }
			public Func<IScriptingContext, object[], Task<object>> AsyncHandler { get; }

			public DefaultDef(
				Func<IScriptingContext, object>                 getter,
				Func<IScriptingContext, object[], object>       handler,
				Func<IScriptingContext, object[], Task<object>> asyncHandler
			) {
				Getter       = getter;
				Handler      = handler;
				AsyncHandler = asyncHandler;
			}
		}
	}
}