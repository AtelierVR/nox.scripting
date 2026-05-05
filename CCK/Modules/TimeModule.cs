using Nox.Scripting;
using UnityEngine;

namespace Nox.CCK.Scripting.Modules {
	/// <summary>
	/// Scripting module <c>"time"</c> — Unity Time helpers.
	/// <code>
	/// import { time, deltaTime, realtime, frameCount, timeScale,
	///          unscaledTime, unscaledDeltaTime, fixedDeltaTime } from 'time';
	/// </code>
	/// </summary>
	public static class TimeModule {
		public static readonly IScriptingModuleDefinition Module =
			ScriptingModuleBuilder.Create("time")
				.WithTags("session")
				/// <summary>Seconds since the scene started (affected by timeScale).</summary>
				.AddVariable("time",              () => (object)Time.time)
				/// <summary>Seconds elapsed since last frame (affected by timeScale).</summary>
				.AddVariable("deltaTime",         () => (object)Time.deltaTime)
				/// <summary>Seconds elapsed since last frame, ignoring timeScale.</summary>
				.AddVariable("unscaledDeltaTime", () => (object)Time.unscaledDeltaTime)
				/// <summary>Seconds since the scene started, ignoring timeScale.</summary>
				.AddVariable("unscaledTime",      () => (object)Time.unscaledTime)
				/// <summary>Fixed physics timestep interval in seconds.</summary>
				.AddVariable("fixedDeltaTime",    () => (object)Time.fixedDeltaTime)
				/// <summary>Seconds since the application started (wall-clock, unaffected by timeScale).</summary>
				.AddVariable("realtime",          () => (object)Time.realtimeSinceStartup)
				/// <summary>Total frames rendered since the application started.</summary>
				.AddVariable("frameCount",        () => (object)Time.frameCount)
				/// <summary>Current time scale multiplier (read/write).</summary>
				.AddVariable("timeScale",
					getter: () => (object)Time.timeScale,
					setter: v  => { if (v != null) Time.timeScale = System.Convert.ToSingle(v); })
				.Build();
	}
}
