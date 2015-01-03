// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrowsableOutputsEnumerator.cs" company="">
//   
// </copyright>
// <summary>
//   This enumerator allows a <see cref="DropDownNSSelector" /> to have a
//   <see cref="BrowserDataSource" /> object that only loads it's data when
//   needed, not when the BrowserDataSource object is loaded.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This enumerator allows a <see cref="DropDownNSSelector" /> to have a
    ///     <see cref="BrowserDataSource" /> object that only loads it's data when
    ///     needed, not when the BrowserDataSource object is loaded.
    /// </summary>
    public class BrowsableOutputsEnumerator : System.Collections.Generic.IEnumerator<INeuronInfo>, 
                                              System.Collections.IEnumerable
    {
        /// <summary>The f counter.</summary>
        private int fCounter;

        /// <summary>The f list.</summary>
        private System.Collections.Generic.List<INeuronInfo> fList;

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="BrowsableOutputsEnumerator"/> class. Initializes a new instance of the<see cref="BrowsableOutputsEnumerator"/> class.</summary>
        /// <param name="editor">The editor.</param>
        public BrowsableOutputsEnumerator(TextPatternEditor editor)
        {
            Editor = editor;
        }

        #endregion

        #region Editor

        /// <summary>
        ///     Gets the editor.
        /// </summary>
        public TextPatternEditor Editor { get; private set; }

        #endregion

        /// <summary>
        ///     Gets a value indicating whether can be selected. This is a group, so
        ///     it can't be selected, the children can.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [allow selection]; otherwise, <c>false</c> .
        /// </value>
        public bool AllowSelection
        {
            get
            {
                return false;
            }
        }

        #region HasChildren

        /// <summary>
        ///     Gets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has children; otherwise, <c>false</c> .
        /// </value>
        public bool HasChildren
        {
            get
            {
                if (fList == null)
                {
                    LoadData();
                }

                return fList.Count > 0;
            }
        }

        #endregion

        /// <summary>Gets or sets a value indicating whether is expanded.</summary>
        public bool IsExpanded { get; set; }

        #region IEnumerable Members

        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Collections.IEnumerator" /> object that can be used to iterate
        ///     through the collection.
        /// </returns>
        public System.Collections.IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator<INeuronInfo> Members

        /// <summary>
        ///     Gets the current.
        /// </summary>
        public INeuronInfo Current
        {
            get
            {
                return fList[fCounter];
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            fList = null;
        }

        #endregion

        #region IEnumerator Members

        /// <summary>Gets the current.</summary>
        object System.Collections.IEnumerator.Current
        {
            get
            {
                return fList[fCounter];
            }
        }

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to
        ///     the next element; <see langword="false" /> if the enumerator has passed
        ///     the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            if (fList == null)
            {
                LoadData();
            }

            if (fCounter < fList.Count - 1)
            {
                fCounter++;
                return true;
            }

            return false;
        }

        /// <summary>The load data.</summary>
        private void LoadData()
        {
            var iWasLoaded = Editor.IsOpen;
            Editor.IsOpen = true;
            try
            {
                fList = new System.Collections.Generic.List<INeuronInfo>();
                foreach (var iRule in Editor.Items)
                {
                    var iLoaded = iRule.IsLoaded;
                    iRule.IsLoaded = true; // make certain that the rule data is also loaded
                    try
                    {
                        foreach (var iGrp in iRule.ResponsesFor)
                        {
                            foreach (var iCond in iGrp.Conditionals)
                            {
                                fList.AddRange(iCond.Outputs);
                            }
                        }

                        foreach (var iCond in iRule.Conditionals)
                        {
                            fList.AddRange(iCond.Outputs);
                        }

                        fList.AddRange(iRule.Outputs);
                    }
                    finally
                    {
                        iRule.IsLoaded = iLoaded;
                    }
                }

                foreach (var iQuestion in Editor.Questions)
                {
                    fList.AddRange(iQuestion.Outputs);
                }
            }
            finally
            {
                Editor.IsOpen = iWasLoaded;
            }

            fList.Sort(
                delegate(INeuronInfo p1, INeuronInfo p2)
                    {
                        return p1.NeuronInfo.DisplayTitle.CompareTo(p2.NeuronInfo.DisplayTitle);
                    });

                // sort the list so that things are easier found.
            fCounter = -1;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first
        ///     element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     The collection was modified after the enumerator was created.
        /// </exception>
        public void Reset()
        {
            fList = null;
            fCounter = -1;
        }

        #endregion
    }
}