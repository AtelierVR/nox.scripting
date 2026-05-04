using Nox.Scripting;

namespace Nox.CCK.Scripting {
	/// <summary>
	/// Resolves variable names to their corresponding values within the context of a scripting module.
	/// This interface is used by the scripting backend to look up variables when executing scripts.
	/// </summary>
	public class NameResolver : INameResolver {
		public const string DefaultStyle = "snake_case";

		public const string camelCaseStyle = "camelCase";
		public const string PascalCaseStyle = "PascalCase";
		public const string snake_case_style = "snake_case";
		public const string UPPER_SNAKE_CASE = "UPPER_SNAKE_CASE";


		public NameResolver(string name)
			=> Name = name;

		public string Name { get; }

		public string Resolve(string style)
			=> style switch {
				camelCaseStyle   => ToCamelCase(Name),
				PascalCaseStyle  => ToPascalCase(Name),
				snake_case_style => ToSnakeCase(Name),
				UPPER_SNAKE_CASE => ToUpperSnakeCase(Name),
				_                => Resolve(DefaultStyle)
			};

		private static string ToCamelCase(string name)
			=> char.ToLowerInvariant(name[0]) + name[1..];

		private static string ToPascalCase(string name)
			=> char.ToUpperInvariant(name[0]) + name[1..];

		private static string ToSnakeCase(string name)
			=> name.ToLowerInvariant().Replace(" ", "_");

		private static string ToUpperSnakeCase(string name)
			=> ToSnakeCase(name).ToUpperInvariant();

		public static implicit operator string(NameResolver resolver)
			=> resolver.Name;

		public static implicit operator NameResolver(string name)
			=> new(name);

		public bool Equals(INameResolver other)
			=> other != null && Resolve(DefaultStyle) == other.Resolve(DefaultStyle);

		public override string ToString()
			=> Name;
	}
}