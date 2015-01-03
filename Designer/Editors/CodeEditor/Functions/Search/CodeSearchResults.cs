// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeSearchResults.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a class to manage a list of display paths that were build for a
//   code-editor search.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Provides a class to manage a list of display paths that were build for a
    ///     code-editor search.
    /// </summary>
    public class CodeSearchResult : Data.ObservableObject
    {
        /// <summary>The f root.</summary>
        private readonly DPICodeEditorRoot fRoot; // so we can easely add items.

        #region Item

        /// <summary>
        ///     Gets the list of display paths.
        /// </summary>
        public DisplayPath Item { get; private set; }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether this instance has more then 1 item in
        ///     the path already or not. Used to determin if it's a valid path.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has path; otherwise, <c>false</c> .
        /// </value>
        public bool HasPath
        {
            get
            {
                return fRoot.Items.Count > 0;
            }
        }

        #region IsClosed

        /// <summary>
        ///     Gets/sets the wether the path found a terminus. When no terminus
        ///     found, don't add to to result list.
        /// </summary>
        public bool IsClosed { get; private set; }

        #endregion

        /// <summary>The insert outgoing link.</summary>
        /// <param name="meaning">The meaning.</param>
        internal void InsertOutgoingLink(ulong meaning)
        {
            var iNew = new DPILinkOut(meaning);
            fRoot.Items.Insert(0, iNew);
        }

        /// <summary>The insert child.</summary>
        /// <param name="index">The index.</param>
        internal void InsertChild(int index)
        {
            var iNew = new DPIChild(index);
            fRoot.Items.Insert(0, iNew);
        }

        /// <summary>Inserts the code root.</summary>
        /// <param name="neuron">The neuron.</param>
        internal void InsertCodeRoot(Neuron neuron)
        {
            // if(fRoot.Items.Count == 0)
            fRoot.Item = neuron;
            Item.Title = BrainData.Current.NeuronInfo[neuron].DisplayTitle;
            IsClosed = true;
        }

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeSearchResult"/> class. 
        ///     Initializes a new instance of the <see cref="SearchResults"/> class.</summary>
        /// <remarks>a new, empty code-editor root is created for each item.</remarks>
        /// <param name="initCount">The initial nr of empty display paths to create.</param>
        public CodeSearchResult()
        {
            IsClosed = false;
            fRoot = new DPICodeEditorRoot();
            Item = new DisplayPath(fRoot);
        }

        /// <summary>Initializes a new instance of the <see cref="CodeSearchResult"/> class. Initializes a new instance of the <see cref="SearchResults"/> class.
        ///     Duplicates the display paths of the <paramref name="source"/> into new
        ///     displaypaths.</summary>
        /// <param name="source">The source.</param>
        public CodeSearchResult(CodeSearchResult source)
        {
            IsClosed = false;
            fRoot = new DPICodeEditorRoot();
            Item = new DisplayPath(fRoot);

            foreach (var i in source.fRoot.Items)
            {
                fRoot.Items.Add(i);
            }
        }

        #endregion
    }
}