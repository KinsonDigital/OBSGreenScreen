using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OBSGreenScreen.VelaptorStuff
{
    /// <summary>
    /// Loads embedded text file resources.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class EmbeddedResourceLoaderService : IEmbeddedResourceLoaderService
    {
        /// <inheritdoc cref="IEmbeddedResourceLoaderService.LoadResource"/>
        public string LoadResource(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var resources = assembly.GetManifestResourceNames();

            var shaderSrcResource = (from r in resources
                where r.EndsWith(name.ToLower(), StringComparison.Ordinal)
                select r).SingleOrDefault();

            if (resources is null || resources.Length <= 0 || string.IsNullOrEmpty(shaderSrcResource))
            {
                throw new Exception($"The embedded shader source code resource '{name}' does not exist.");
            }

            using (var stream = assembly.GetManifestResourceStream(shaderSrcResource))
            {
                if (stream is not null)
                {
                    using var reader = new StreamReader(stream);

                    return reader.ReadToEnd();
                }
            }

            return string.Empty;
        }
    }
}
