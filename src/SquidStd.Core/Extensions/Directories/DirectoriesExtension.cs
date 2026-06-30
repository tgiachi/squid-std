using SquidStd.Core.Extensions.Env;

namespace SquidStd.Core.Extensions.Directories;

/// <summary>
/// Provides extension methods for directory path resolution and environment variable expansion
/// </summary>
public static class DirectoriesExtension
{
    /// <param name="path">The path to resolve</param>
    extension(string path)
    {
        /// <summary>
        /// Resolves path by expanding tilde (~) to user home directory and expanding environment variables
        /// </summary>
        /// <returns>The fully resolved path with expanded environment variables</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
        public string ResolvePathAndEnvs()
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            path = path.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            path = Environment.ExpandEnvironmentVariables(path).ExpandEnvironmentVariables();

            return path;
        }
    }
}
