// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DisplayPath.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data to describe the path to get to a neuron from a
//   project root item like an code/frame/flow editor or the thesaurus. Also
//   used to store the location that an error occured in a processor during
//   exeuction (for the log).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Contains all the data to describe the path to get to a neuron from a
    ///     project root item like an code/frame/flow editor or the thesaurus. Also
    ///     used to store the location that an error occured in a processor during
    ///     exeuction (for the log).
    /// </summary>
    public class DisplayPath
    {
        #region Title

        /// <summary>
        ///     Gets/sets the title of the display path.
        /// </summary>
        /// <remarks>
        ///     This is not required but can be used when showing a lis of display
        ///     paths for selection.
        /// </remarks>
        public string Title { get; set; }

        #endregion

        /// <summary>
        ///     Selects the neuron at the end of the path. The root item determins
        ///     where to start the display from: a code editor, frame,...
        /// </summary>
        public void SelectPathResult()
        {
            try
            {
                if (Root != null)
                {
                    Root.SelectPathResult();
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError(
                    "DisplayPath", 
                    "Failed to move focus to the selected UI item with message: " + e.Message);
            }
        }

        /// <summary>Creates a display path, based on the execution stack of the processor,
        ///     up untill it's currently selected frame and code item.</summary>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        internal static DisplayPath CreateFromSelectedCode(DebugProcessor proc)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Creates a duplicate (deep copy) of this displayPath.</summary>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        internal DisplayPath Duplicate()
        {
            var iNew = Root.Duplicate();
            var iItem = new DisplayPath(iNew);
            iItem.Title = Title;
            return iItem;
        }

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisplayPath" /> class.
        /// </summary>
        public DisplayPath()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DisplayPath"/> class.
        ///     Allows manual building of a path.</summary>
        /// <param name="root">The root object to use for the path.</param>
        public DisplayPath(DPIRoot root)
        {
            Root = root;
        }

        /// <summary>
        ///     gets the root starting point for this displaypath.
        /// </summary>
        public DPIRoot Root { get; private set; }

        /// <summary>Creates a searchpath, based on the current call stack of the
        ///     specified processor.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="DisplayPath"/>.</returns>
        public static DisplayPath CreateFromProcessor(DebugProcessor source)
        {
            var iRes = new DisplayPath();
            var iRoot = new DPICodeEditorRoot();
            iRes.Root = iRoot.BuildFrom(source);
            return iRes;
        }

        #endregion
    }
}