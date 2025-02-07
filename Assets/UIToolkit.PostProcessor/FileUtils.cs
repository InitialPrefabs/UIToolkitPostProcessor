using System.IO;
using UnityEngine;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    internal static class FileUtils {
        /// <summary>
        /// Turns a relative path from <see cref="AssetDatabase"/> as a full path.
        /// </summary>
        /// <param name="assetPath">The relative path of the asset in the project.</param>
        /// <returns>A full path to the asset.</returns>
        public static string AbsolutePath(string assetPath) {
            return Path.Combine(
                Application.dataPath.Replace("Assets", string.Empty), assetPath)
                .Replace(Path.PathSeparator, '/');
        }

        /// <summary>
        /// Fast and simple way to check if <see cref="string"/>s ends with a custom pattern.
        /// </summary>
        /// <param name="a">The string to compare to</param>
        /// <param name="b">The pattern a string ends with</param>
        /// <returns>True, if there is a match.</returns>
        public static bool SimpleEndsWith(this string a, string b) {
            var ap = a.Length - 1;
            var bp = b.Length - 1;

            while (ap >= 0 && bp >= 0 && a[ap] == b[bp]) {
                ap--;
                bp--;
            }

            return bp < 0;
        }
    }
}
