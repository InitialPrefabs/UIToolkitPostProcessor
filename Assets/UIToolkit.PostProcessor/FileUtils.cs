using System.IO;
using UnityEngine;

namespace InitialPrefabs.UIToolkit.PostProcessor {
    internal static class FileUtils {
        public static string AbsolutePath(string assetPath) {
            return Path.Combine(
                Application.dataPath.Replace("Assets", string.Empty), assetPath)
                .Replace(Path.PathSeparator, '/');
        }

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
