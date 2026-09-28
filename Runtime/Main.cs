using System;
using System.Collections.Generic;
using System.Linq;
using Nox.CCK.Events;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.CCK.Scripting;
using Nox.CCK.Scripting.Converters;
using Nox.CCK.Scripting.Modules;

namespace Nox.Scripting.Runtime {
	public class Main : IMainModInitializer, IScriptingAPI {
		public static Main Instance { get; private set; }

		private readonly List<IScriptingModuleDefinition> _modules = new();
		private readonly List<IScriptingTypeConverter> _converters = new();
		private readonly List<IScriptingBackend> _backends = new();

		/// <summary>Cache of <see cref="ResolveConverter"/> results, cleared on every registration change.</summary>
		private readonly Dictionary<Type, IScriptingTypeConverter> _resolved = new();

		/// <summary>Guards <see cref="_resolved"/> (resolution runs on the scripting hot path).</summary>
		private readonly object _lock = new();

		public IReadOnlyList<IScriptingModuleDefinition> Modules
			=> _modules;

		public IReadOnlyList<IScriptingTypeConverter> Converters
			=> _converters;

		public NoxEvent<IScriptingModuleDefinition> OnModuleRegistered { get; } = new();
		public NoxEvent<INameResolver> OnModuleUnregistered { get; } = new();
		public NoxEvent<IScriptingTypeConverter> OnConverterRegistered { get; } = new();

		// ── Modules ──────────────────────────────────────────────────────────

		public void RegisterModule(IScriptingModuleDefinition definition) {
			if (definition == null)
				throw new ArgumentNullException(nameof(definition));

			_modules.RemoveAll(m => m.Id == definition.Id);
			_modules.Add(definition);

			foreach (var backend in _backends) {
				if (!ModuleMatchesBackend(definition, backend))
					continue;
				backend.OnModuleRegistered(definition);
			}
			OnModuleRegistered.Invoke(definition);
		}

		public void UnregisterModule(IScriptingModuleDefinition definition) {
			if (_modules.RemoveAll(m => m.Id == definition.Id) <= 0)
				return;
			foreach (var backend in _backends)
				backend.OnModuleUnregistered(definition.Id);
			OnModuleUnregistered.Invoke(definition.Id);
		}

		// ── Converters ───────────────────────────────────────────────────────

		public void RegisterConverter(IScriptingTypeConverter converter) {
			if (converter == null)
				throw new ArgumentNullException(nameof(converter));
			_converters.RemoveAll(c => c.HandledType == converter.HandledType);
			_converters.Add(converter);
			lock (_lock)
				_resolved.Clear();
			foreach (var backend in _backends)
				backend.OnConverterRegistered(converter);
			OnConverterRegistered.Invoke(converter);
		}

		public void UnregisterConverter(IScriptingTypeConverter converter) {
			if (!_converters.Remove(converter))
				return;
			lock (_lock)
				_resolved.Clear();
		}

		/// <summary>
		/// Merges every converter whose <see cref="IScriptingTypeConverter.HandledType"/> is assignable
		/// from <paramref name="type"/>: a value is then exposed with ALL the compatible bindings at once
		/// (e.g. an <c>IEntity</c> converter and an <c>IPlayer</c> converter both apply to a player).
		/// The most specific handled type wins for a given binding name; its converter also supplies
		/// the constructor, the default and the raw conversion.
		/// </summary>
		public IScriptingTypeConverter ResolveConverter(Type type) {
			if (type == null)
				return null;

			lock (_lock) {
				if (_resolved.TryGetValue(type, out var cached))
					return cached;
			}

			var matches = _converters
				.Where(c => c.HandledType.IsAssignableFrom(type))
				.OrderByDescending(c => Specificity(c.HandledType))
				.ToArray();

			if (matches.Length == 0)
				return null;

			var resolved = matches.Length == 1
				? matches[0]
				: new MergedConverter(matches);

			lock (_lock)
				_resolved[type] = resolved;

			return resolved;
		}

		/// <summary>
		/// Rough specificity score of a handled type: the deeper in the hierarchy (base classes +
		/// interfaces), the more specific. Used to order the compatible converters.
		/// </summary>
		private static int Specificity(Type type) {
			var score = 0;
			for (var current = type; current != null; current = current.BaseType)
				score++;
			return score + type.GetInterfaces().Length;
		}

		/// <summary>
		/// Converter exposing the bindings of several compatible converters at once. Bindings are
		/// deduplicated by their resolved (camelCase) name, keeping the most specific first.
		/// </summary>
		private sealed class MergedConverter : IScriptingTypeConverter {
			private readonly IScriptingTypeConverter _primary;

			public MergedConverter(IReadOnlyList<IScriptingTypeConverter> converters) {
				_primary        = converters[0];
				Bindings        = Merge(converters.SelectMany(c => c.Bindings), b => b.Name);
				StaticBindings  = Merge(converters.SelectMany(c => c.StaticBindings), b => b.Name);
			}

			public Type HandledType
				=> _primary.HandledType;

			public IReadOnlyList<IScriptingTypeBindingDefinition> Bindings { get; }

			public IReadOnlyList<IScriptingStaticBindingDefinition> StaticBindings { get; }

			public Func<IScriptingContext, object[], object> Constructor
				=> _primary.Constructor;

			public IScriptingTypeDefaultDefinition Default
				=> _primary.Default;

			public object ToScript(IScriptingContext context, object value)
				=> _primary.ToScript(context, value);

			private static IReadOnlyList<T> Merge<T>(IEnumerable<T> bindings, Func<T, INameResolver> name) where T : class {
				var merged = new List<T>();
				var names  = new HashSet<string>();
				foreach (var binding in bindings) {
					var resolved = name(binding).Resolve(NameResolver.camelCaseStyle);
					if (names.Add(resolved))
						merged.Add(binding);
				}
				return merged;
			}
		}

		// ── Backends ─────────────────────────────────────────────────────────

		public void RegisterBackend(IScriptingBackend backend) {
			if (backend == null)
				throw new ArgumentNullException(nameof(backend));
			if (_backends.Contains(backend))
				return;
			_backends.Add(backend);

			// Catch the backend up with already-registered modules and converters.
			foreach (var module in _modules) {
				if (!ModuleMatchesBackend(module, backend))
					continue;
				backend.OnModuleRegistered(module);
			}

			foreach (var converter in _converters)
				backend.OnConverterRegistered(converter);
		}

		public void UnregisterBackend(IScriptingBackend backend) {
			_backends.Remove(backend);
		}

		// ── Tag helpers ──────────────────────────────────────────────────────

		/// <summary>
		/// Returns true if <paramref name="module"/> should be sent to <paramref name="backend"/>.
		/// A module with no tags targets all backends; otherwise at least one tag must match.
		/// A backend with no tags accepts all modules.
		/// </summary>
		private static bool ModuleMatchesBackend(IScriptingModuleDefinition module, IScriptingBackend backend) {
			if (module.Tags.Count == 0 || backend.Tags.Count == 0)
				return true;
			return module.Tags.Any(t => backend.Tags.Contains(t));
		}

		// ── Mod lifecycle ────────────────────────────────────────────────────

		public void OnInitializeMain(IMainModCoreAPI api) {
			Instance = this;

			foreach (var converter in UnityConverters.All)
				RegisterConverter(converter);

			foreach (var converter in BufferConverter.All)
				RegisterConverter(converter);

			foreach (var converter in RsaConverter.All)
				RegisterConverter(converter);

			RegisterModule(HashingModule.Module);
			RegisterModule(CryptoModule.Module);
			RegisterModule(UnityModule.Module);
			RegisterModule(ConsoleModule.Module);
			RegisterModule(BehaviourModule.Module);
			RegisterModule(BufferModule.Module);
			RegisterModule(TimeModule.Module);
			RegisterModule(SchedulerModule.Module);
		}

		public void OnDisposeMain() {
			foreach(var backend in _backends.ToArray())
				UnregisterBackend(backend);
			_backends.Clear();
			foreach(var module in _modules.ToArray())
				UnregisterModule(module);
			_modules.Clear();
			foreach(var converter in _converters.ToArray())
				UnregisterConverter(converter);
			_converters.Clear();
			lock (_lock)
				_resolved.Clear();
			Instance = null;
		}
	}
}