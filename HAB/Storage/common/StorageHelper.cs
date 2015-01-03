// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StorageHelper.cs" company="">
//   
// </copyright>
// <summary>
//   A helper class for the storage system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A helper class for the storage system.
    /// </summary>
    public class StorageHelper
    {
        /// <summary>Clears the specified root.</summary>
        /// <remarks><para>All sub dirs are deleted seperatly, including all the data files. This
        ///         is done to prevent accidental deletion of sub dirs used by subversion
        ///         or another version control system (all files are deleted, but only the
        ///         known sub folders.</para>
        /// <para>This is a major shortcomming of the xmlstorage, it's far to slow since
        ///         it has to many files, to deep a directory structure. The NDBStore has
        ///         far fewer files, but can use the same algorithm for cleaning, without
        ///         to much of an overhead.</para>
        /// </remarks>
        /// <param name="root">The root.</param>
        public static void ClearDir(string root)
        {
            foreach (var i in System.IO.Directory.GetFiles(root))
            {
                // this takes care of the property files
                System.IO.File.Delete(i);
            }

            var iLev1 = 0;
            var iLev1Path = System.IO.Path.Combine(root, iLev1.ToString());
            while (System.IO.Directory.Exists(iLev1Path))
            {
                var iLev2 = 0;
                var iLev2Path = System.IO.Path.Combine(iLev1Path, iLev2.ToString());
                while (System.IO.Directory.Exists(iLev2Path))
                {
                    var iLev3 = 0;
                    var iLev3Path = System.IO.Path.Combine(iLev2Path, iLev3.ToString());
                    while (System.IO.Directory.Exists(iLev3Path))
                    {
                        var iLev4 = 0;
                        var iLev4Path = System.IO.Path.Combine(iLev3Path, iLev4.ToString());
                        while (System.IO.Directory.Exists(iLev4Path))
                        {
                            foreach (var iFile in System.IO.Directory.GetFiles(iLev4Path))
                            {
                                System.IO.File.Delete(iFile);
                            }

                            if (System.IO.Directory.GetDirectories(iLev4Path).Length == 0)
                            {
                                System.IO.Directory.Delete(iLev4Path);

                                    // if not empty, this fails, which is ok for subversion.
                            }

                            iLev4++;
                            iLev4Path = System.IO.Path.Combine(iLev3Path, iLev4.ToString());
                        }

                        if (System.IO.Directory.GetDirectories(iLev3Path).Length == 0)
                        {
                            System.IO.Directory.Delete(iLev3Path);

                                // if not empty, this fails, which is ok for subversion.
                        }

                        iLev3++;
                        iLev3Path = System.IO.Path.Combine(iLev2Path, iLev3.ToString());
                    }

                    if (System.IO.Directory.GetDirectories(iLev2Path).Length == 0)
                    {
                        System.IO.Directory.Delete(iLev2Path); // if not empty, this fails, which is ok for subversion.
                    }

                    iLev2++;
                    iLev2Path = System.IO.Path.Combine(iLev1Path, iLev2.ToString());
                }

                if (System.IO.Directory.GetDirectories(iLev1Path).Length == 0)
                {
                    System.IO.Directory.Delete(iLev1Path); // if not empty, this fails, which is ok for subversion.
                }

                iLev1++;
                iLev1Path = System.IO.Path.Combine(root, iLev1.ToString());
            }
        }

        /// <summary>Gets the default path to the modules directory.</summary>
        /// <param name="loc">The loc.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetModulesPath(string loc)
        {
            return System.IO.Path.Combine(loc, "Modules");
        }

        /// <summary>Gets the default path to the data directory.</summary>
        /// <param name="loc">The loc.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetDataPath(string loc)
        {
            return System.IO.Path.Combine(loc, "Data");
        }

        /// <summary>checks if the current storage has a datapath assigned. If not, the
        ///     specified <paramref name="path"/> is used, appended with<see cref="StorageHelper.GetDataPath"/> .</summary>
        /// <param name="path"></param>
        internal static void VerifyDataPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException("path");
            }

            if (Brain.Current != null && Brain.Current.Storage != null)
            {
                if (string.IsNullOrEmpty(Brain.Current.Storage.DataPath))
                {
                    var iDataPath = GetDataPath(path);
                    if (System.IO.Directory.Exists(iDataPath) == false)
                    {
                        System.IO.Directory.CreateDirectory(iDataPath);
                    }

                    Brain.Current.Storage.DataPath = iDataPath;

                    var iModulesPath = GetModulesPath(path);
                    if (System.IO.Directory.Exists(iModulesPath) == false)
                    {
                        System.IO.Directory.CreateDirectory(iModulesPath);
                    }

                    Brain.Current.Modules.Path = iModulesPath;
                }
            }
            else
            {
                throw new System.InvalidOperationException("No DB object created.");
            }
        }
    }
}