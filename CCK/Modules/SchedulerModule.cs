using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Nox.CCK.Utils;
using Nox.Scripting;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"scheduler"</c> — Node/browser-style timing helpers.
	///
	/// <para>
	/// <c>sleep</c>/<c>sleep</c> are async and simply resolve after the given time.
	/// <c>setTimeout</c>/<c>setInterval</c> return a numeric handle that can be passed
	/// to <c>clearTimeout</c>/<c>clearInterval</c> (both accept either handle type,
	/// like in JS). All timers are automatically cancelled when the owning script
	/// context ends, so scripts never leak timers past their lifetime.
	/// </para>
	///
	/// <code>
	/// import { sleep, setTimeout, setInterval, clearTimeout, clearInterval, clearAll } from 'scheduler';
	///
	/// // Async wait
	/// await sleep(1000);
	///
	/// // One-shot callback, optional extra args forwarded to the callback
	/// const handle = setTimeout((msg) => console.log(msg), 500, "hello");
	/// clearTimeout(handle);
	///
	/// // Repeating callback
	/// const tick = setInterval(() => console.log("tick"), 1000);
	/// clearInterval(tick);
	///
	/// // Clear every timer owned by this script
	/// clearAll();
	/// </code>
	/// </summary>
	public static class SchedulerModule {
		private static int _nextId;

		// Per-script-context timer registries. Keyed off the context itself so
		// concurrent scripts never see or clear each other's timers, and entries
		// are collected automatically once a context is no longer referenced.
		private static readonly ConditionalWeakTable<IScriptingContext, ConcurrentDictionary<int, CancellationTokenSource>>
			_timersByContext = new();

		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("scheduler")
				.WithTags("session")

				// ── Async wait ───────────────────────────────────────────────
				.AddAsyncMethod("sleep", async (ctx, args) => {
					await UniTask.Delay(ToMillis(args, 0), cancellationToken: ctx.CancellationToken);
					return null;
				})

				// ── One-shot / repeating callbacks ──────────────────────────
				.AddMethod("setTimeout", (ctx, args) => {
					var callback = ExtractCallback(args);
					if (callback == null) {
						Logger.LogWarning($"setTimeout requires a function as its first argument ({string.Join(", ", args.Select(a => a.GetType().FullName))}).", tag: nameof(SchedulerModule));
						return -1;
					}
					var ms    = ToMillis(args, 1);
					var extra = ExtraArgs(args, 2);
					var (id, token) = CreateTimer(ctx);
					RunTimeout(ctx, id, callback, ms, extra, token).Forget();
					return id;
				})
				.AddMethod("setInterval", (ctx, args) => {
					var callback = ExtractCallback(args);
					if (callback == null) {
						Logger.LogWarning($"setInterval requires a function as its first argument ({string.Join(", ", args.Select(a => a.GetType().FullName))}).", tag: nameof(SchedulerModule));
						return -1;
					}
					var ms    = Math.Max(1, ToMillis(args, 1));
					var extra = ExtraArgs(args, 2);
					var (id, token) = CreateTimer(ctx);
					RunInterval(ctx, id, callback, ms, extra, token).Forget();
					return id;
				})

				// ── Cancellation ─────────────────────────────────────────────
				// clearTimeout/clearInterval are interchangeable, exactly like in JS.
				.AddMethod("clearTimeout",  (ctx, args) => {
                    ClearTimer(ctx, args); 
                    return null;
                })
				.AddMethod("clearInterval", (ctx, args) => { 
                    ClearTimer(ctx, args); 
                    return null;
                })
				.AddMethod("clearAll", (ctx, _) => {
					ClearAll(ctx);
					return null;
				})
				.Build();

		// ── Callback Extraction ──────────────────────────────────────────────

        private static Action<object[]> ExtractCallback(object[] args) {
            if (args.Length == 0 || args[0] == null)
                return null;
        
            if (args[0] is Action<object[]> action)
                return action;
        
            // Intercepte directement le type renvoyé par FromValue
            if (args[0] is Func<object[], object> func)
                return extraArgs => func(extraArgs ?? Array.Empty<object>());
        
            // Fallback pour les autres types de délégués
            if (args[0] is Delegate del)
                return extraArgs => del.DynamicInvoke(extraArgs ?? Array.Empty<object>());
        
            return null;
        }

		// ── Timer bookkeeping ────────────────────────────────────────────────

		private static ConcurrentDictionary<int, CancellationTokenSource> Timers(IScriptingContext ctx)
			=> _timersByContext.GetOrCreateValue(ctx);

		/// <summary>Allocate a new handle plus a cancellation token linked to the script's own lifetime.</summary>
		private static (int id, CancellationToken token) CreateTimer(IScriptingContext ctx) {
			var id  = Interlocked.Increment(ref _nextId);
			var cts = CancellationTokenSource.CreateLinkedTokenSource(ctx.CancellationToken);
			Timers(ctx)[id] = cts;
			return (id, cts.Token);
		}

		private static void RemoveTimer(IScriptingContext ctx, int id) {
			if (Timers(ctx).TryRemove(id, out var cts))
				cts.Dispose();
		}

		private static void ClearTimer(IScriptingContext ctx, object[] args) {
			if (args.Length == 0 || args[0] == null) {
				Logger.LogWarning("clearTimeout/clearInterval requires a handle argument.", tag: nameof(SchedulerModule));
				return;
			}
			int id;
			try { id = Convert.ToInt32(args[0]); }
			catch {
				Logger.LogWarning($"clearTimeout/clearInterval received an invalid handle: {args[0]}.", tag: nameof(SchedulerModule));
				return;
			}

			if (Timers(ctx).TryRemove(id, out var cts)) {
				try { cts.Cancel(); } catch { /* already disposed/cancelled */ }
				cts.Dispose();
			}
		}

		private static void ClearAll(IScriptingContext ctx) {
			var timers = Timers(ctx);
			foreach (var id in timers.Keys)
				if (timers.TryRemove(id, out var cts)) {
					try { cts.Cancel(); } 
                    catch { /* already disposed/cancelled */ }
					cts.Dispose();
				}
		}

		// ── Timer loops ──────────────────────────────────────────────────────

		private static async UniTaskVoid RunTimeout(
			IScriptingContext ctx, int id, Action<object[]> callback, int ms, object[] args, CancellationToken token
		) {
			try {
				await UniTask.Delay(Math.Max(0, ms), cancellationToken: token);
				if (!token.IsCancellationRequested)
					Invoke(callback, args);
			} catch (OperationCanceledException) {
				// Cleared via clearTimeout/clearAll, or the script context ended.
			} finally {
				RemoveTimer(ctx, id);
			}
		}

		private static async UniTaskVoid RunInterval(
			IScriptingContext ctx, int id, Action<object[]> callback, int ms, object[] args, CancellationToken token
		) {
			try {
				while (!token.IsCancellationRequested) {
					await UniTask.Delay(ms, cancellationToken: token);
					if (token.IsCancellationRequested) break;
					Invoke(callback, args);
				}
			} catch (OperationCanceledException) {
				// Cleared via clearInterval/clearAll, or the script context ended.
			} finally {
				RemoveTimer(ctx, id);
			}
		}

		private static void Invoke(Action<object[]> callback, object[] args) {
			try { callback(args); }
			catch (Exception ex) { 
                Logger.LogWarning($"scheduler callback failed: {ex.Message}", tag: nameof(SchedulerModule)); 
            }
		}

		// ── Argument helpers ─────────────────────────────────────────────────

		private static int ToMillis(object[] args, int index)
			=> args.Length > index && args[index] != null 
                ? Convert.ToInt32(args[index]) 
                : 0;

		private static object[] ExtraArgs(object[] args, int fromIndex)
			=> args.Length > fromIndex 
                ? args[fromIndex..] 
                : Array.Empty<object>();
	}
}