using Nox.Scripting;

namespace Nox.CCK.Scripting.Modules {

	public class UnityModule {
		public static readonly IScriptingModuleDefinition Module = ScriptingModuleBuilder
			.Create("unity")
			.AddType("Vector2", Converters.UnityConverters.Vector2)
			.AddType("Vector3", Converters.UnityConverters.Vector3)
			.AddType("Vector4", Converters.UnityConverters.Vector4)
			.AddType("Quaternion", Converters.UnityConverters.Quaternion)
			.AddType("Color", Converters.UnityConverters.Color)
			.Build();

	}
}