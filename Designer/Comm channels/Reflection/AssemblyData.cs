// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Contains all the
    /// </summary>
    public class AssemblyData : ReflectionData
    {
        #region Fields

        /// <summary>The f assembly.</summary>
        private System.Reflection.Assembly fAssembly;

        /// <summary>The f is assembly loaded.</summary>
        private bool fIsAssemblyLoaded;

        /// <summary>The f path.</summary>
        private string fPath;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="AssemblyData"/> class.</summary>
        /// <param name="path">The path.</param>
        /// <param name="owner">The owner.</param>
        public AssemblyData(string path, ReflectionChannel owner)
        {
            Children = new Data.ObservedCollection<NamespaceData>(this);
            Owner = owner;

                // we set the owner already so that 'functionData' can find the sin it belongs to, for resolving 'isloaded'.
            if (path.StartsWith("file:/"))
            {
                path = path.Substring(8);
            }

            SetPath(path);
        }

        #region Path

        /// <summary>
        ///     Gets/sets the path to the library. Also allow an update to make the
        ///     path relative.
        /// </summary>
        public string Path
        {
            get
            {
                return fPath;
            }

            internal set
            {
                fPath = value;
                OnPropertyChanged("Path");
            }
        }

        /// <summary>The set path.</summary>
        /// <param name="path">The path.</param>
        private void SetPath(string path)
        {
            fPath = path;
            System.Reflection.Assembly iAsm = null;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    iAsm = ReflectionSin.GetAssembly(path);
                }
                catch
                {
                    LogService.Log.LogError(
                        "AssemblyData.Create", 
                        string.Format("Failed to find assembly with name: '{0}'.", path));
                }
            }
            else
            {
                Name = path;
            }

            if (iAsm != null)
            {
                LoadAssembly(iAsm);
            }
            else
            {
                LogService.Log.LogError(
                    "AssemblyData.Create", 
                    string.Format("Failed to find assembly with name: '{0}'.", path));
            }
        }

        #endregion

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyData" /> class.
        /// </summary>
        public AssemblyData()
        {
            Children = new Data.ObservedCollection<NamespaceData>(this);
        }

        #endregion

        #region prop

        #region Children

        /// <summary>
        ///     Gets the list of all the types found in the
        ///     <see langword="namespace" />
        /// </summary>
        public Data.ObservedCollection<NamespaceData> Children { get; private set; }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets or sets the function(s)/children are loaded.
        /// </summary>
        /// <remarks>
        ///     Setting to <see langword="null" /> is not processed.
        /// </remarks>
        /// <value>
        ///     <c>true</c> : all the children are loaded - the function is loaded
        ///     <c>false</c> : none of the children are loaded - the function is not
        ///     loaded. <c>null</c> : some loaded - invalid.
        /// </value>
        public override bool? IsLoaded
        {
            get
            {
                if (Children.Count > 0)
                {
                    var iRes = Children[0].IsLoaded;
                    for (var i = 1; i < Children.Count; i++)
                    {
                        if (Children[i].IsLoaded != iRes)
                        {
                            return null;
                        }
                    }

                    return iRes;
                }

                return null;
            }

            set
            {
                if (value.HasValue)
                {
                    foreach (var i in Children)
                    {
                        i.IsLoaded = value;
                    }

                    OnLoadedChanged();
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets/sets wether the assembly is loaded or not.
        /// </summary>
        public bool IsAssemblyLoaded
        {
            get
            {
                return fIsAssemblyLoaded;
            }

            set
            {
                if (value != fIsAssemblyLoaded)
                {
                    if (value)
                    {
                        LoadAssemblyData();
                    }
                    else
                    {
                        Children.Clear();
                    }

                    fIsAssemblyLoaded = value;
                }
            }
        }

        #region Assembly

        /// <summary>
        ///     Gets/sets the assembly that this object wraps.
        /// </summary>
        public System.Reflection.Assembly Assembly
        {
            get
            {
                return fAssembly;
            }

            set
            {
                LoadAssembly(value);
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>Loads all the data for the assembly.</summary>
        /// <param name="asm">The asm.</param>
        private void LoadAssembly(System.Reflection.Assembly asm)
        {
            Name = asm.GetName().Name + " (" + asm.Location + ")";

            fAssembly = asm;
            if (IsAssemblyLoaded)
            {
                LoadAssemblyData();
            }
        }

        /// <summary>The load assembly data.</summary>
        private void LoadAssemblyData()
        {
            var iTypes = from i in fAssembly.GetTypes() where i.IsPublic orderby i.Namespace select i;
            NamespaceData iNsData = null;
            var iNsAdded = false;
            foreach (var i in iTypes)
            {
                if (iNsData == null || iNsData.Name != i.Namespace)
                {
                    // need to create a new namespace item
                    iNsData = new NamespaceData();
                    iNsData.Name = i.Namespace;
                    iNsData.Owner = this;
                    iNsAdded = false;
                }

                var iType = new TypeData(i, iNsData);
                if (iType.Children.Count > 0)
                {
                    iNsData.Children.Add(iType);
                    if (iNsAdded == false)
                    {
                        // we only add if there are actually items in the ns -> there were static public functions in the ns.
                        Children.Add(iNsData);
                        iNsAdded = true;
                    }
                }
            }
        }

        #endregion
    }
}