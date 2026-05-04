using System;
namespace Nox.Scripting {
    public interface INameResolver: IEquatable<INameResolver> {
        /// <summary>
        /// Resolve a name to a value based on the provided style. 
        /// The style parameter can be used to indicate the type of name being resolved 
        /// (e.g., "camelCase", "PascalCase", "snake_case", etc.) to allow for different resolution strategies.
        /// </summary>
        string Resolve(string style);
    }
}