using System;
using System.Collections.Generic;
using System.Linq;
using Nox.CCK.Events;
using Nox.CCK.Mods.Cores;
using Nox.CCK.Mods.Initializers;
using Nox.CCK.Scripting;
using Nox.CCK.Scripting.Converters;
using Nox.CCK.Scripting.Modules;
using Nox.Scripting;

namespace Nox.Scripting.Runtime {
	public class Main : IMainModInitializer, IScriptingAPI {
		public static Main Instance { get; private set; }

		private readonly List<IScriptingModuleDefinition> _modules = new();
		private readonly List<IScriptingTypeConverter> _converters = new();
		private readonly List<IScriptingBackend> _backends = new();

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

		public void UnregisterModule(INameResolver moduleId) {
			if (_modules.RemoveAll(m => m.Id == moduleId) <= 0)
				return;
			foreach (var backend in _backends)
				backend.OnModuleUnregistered(moduleId);
			OnModuleUnregistered.Invoke(moduleId);
		}

		// ── Converters ───────────────────────────────────────────────────────

		public void RegisterConverter(IScriptingTypeConverter converter) {
			if (converter == null)
				throw new ArgumentNullException(nameof(converter));
			_converters.RemoveAll(c => c.HandledType == converter.HandledType);
			_converters.Add(converter);
			foreach (var backend in _backends)
				backend.OnConverterRegistered(converter);
			OnConverterRegistered.Invoke(converter);
		}

		public void UnregisterConverter(IScriptingTypeConverter converter) {
			_converters.Remove(converter);
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
		}

		public void OnDisposeMain() {
			_modules.Clear();
			_converters.Clear();
			_backends.Clear();
			Instance = null;
		}
	}
}