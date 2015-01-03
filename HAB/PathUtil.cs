// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PathUtil.cs" company="">
//   
// </copyright>
// <summary>
//   The path util.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>The path util.</summary>
    public class PathUtil
    {
        /// <summary>Creates a relative path from one fileor folder to another.</summary>
        /// <param name="fromDirectory">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns>The relative path from the start directory to the end path.</returns>
        public static string RelativePathTo(string fromDirectory, string toPath)
        {
            if (fromDirectory == null)
            {
                throw new System.ArgumentNullException("fromDirectory");
            }

            if (toPath == null)
            {
                throw new System.ArgumentNullException("toPath");
            }

            var isRooted = System.IO.Path.IsPathRooted(fromDirectory) && System.IO.Path.IsPathRooted(toPath);
            if (isRooted)
            {
                var isDifferentRoot = string.Compare(
                    System.IO.Path.GetPathRoot(fromDirectory), 
                    System.IO.Path.GetPathRoot(toPath), 
                    true) != 0;
                if (isDifferentRoot)
                {
                    return toPath;
                }
            }

            var relativePath = new System.Collections.Specialized.StringCollection();
            var fromDirectories = fromDirectory.Split(System.IO.Path.DirectorySeparatorChar);
            var toDirectories = toPath.Split(System.IO.Path.DirectorySeparatorChar);

            var length = System.Math.Min(fromDirectories.Length, toDirectories.Length);
            var lastCommonRoot = -1;

            // find common root
            for (var x = 0; x < length; x++)
            {
                if (string.Compare(fromDirectories[x], toDirectories[x], true) != 0)
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return toPath;
            }

            // add relative folders in from path
            for (var x = lastCommonRoot + 1; x < fromDirectories.Length; x++)
            {
                if (fromDirectories[x].Length > 0)
                {
                    relativePath.Add("..");
                }
            }

            // add to folders to path
            for (var x = lastCommonRoot + 1; x < toDirectories.Length; x++)
            {
                relativePath.Add(toDirectories[x]);
            }

            // create relative path
            var relativeParts = new string[relativePath.Count];
            relativePath.CopyTo(relativeParts, 0);
            var newPath = string.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativeParts);
            return newPath;
        }

        /// <summary>Checks if the specified string ends with a <paramref name="path"/>
        ///     delimiter <see langword="char"/> and if not, makes certin it does.</summary>
        /// <param name="path">The string.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string VerifyPathEnd(string path)
        {
            if (string.IsNullOrEmpty(path) == false && path[path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                return path + System.IO.Path.DirectorySeparatorChar;
            }

            return path;
        }

        /// <summary>Checks if the specified string ends with the specifiec delimiter<see langword="char"/> and if not, makes certin it does.</summary>
        /// <param name="path">The string.</param>
        /// <param name="endChar">The end Char.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string VerifyPathEnd(string path, char endChar)
        {
            if (string.IsNullOrEmpty(path) == false && path[path.Length - 1] != endChar)
            {
                return path + endChar;
            }

            return path;
        }
    }
}