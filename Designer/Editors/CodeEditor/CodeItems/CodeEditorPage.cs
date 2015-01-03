// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeEditorPage.cs" company="">
//   
// </copyright>
// <summary>
//   A single page for a code editor. A page contains the code listing of 1
//   cluster (rules, actions, children,..)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A single page for a code editor. A page contains the code listing of 1
    ///     cluster (rules, actions, children,..)
    /// </summary>
    public class CodeEditorPage : Data.OwnedObject, INeuronWrapper, IEditorSelection, ICodeItemsOwner
    {
        /// <summary>Makes all the items that represent the specified<paramref name="neuron"/> on this page selected</summary>
        /// <param name="neuron">The neuron.</param>
        public void Select(Neuron neuron)
        {
            SelectedItems.Clear();
            foreach (var i in Items)
            {
                i.Select(neuron);
            }
        }

        /// <summary>The reset all break points.</summary>
        internal void ResetAllBreakPoints()
        {
            foreach (var i in Items)
            {
                i.ResetBreakPoint();
            }
        }

        /// <summary>
        ///     Unloads the UI data associated with this item..
        /// </summary>
        internal void UnloadUIData()
        {
            SelectedItems.Clear();
            foreach (var i in Items)
            {
                i.UnloadUIData();
            }
        }

        #region fields

        /// <summary>The f selected items.</summary>
        private readonly EditorItemSelectionList<CodeItem> fSelectedItems = new EditorItemSelectionList<CodeItem>();

        /// <summary>The f registered variables.</summary>
        private System.Collections.ObjectModel.ObservableCollection<Variable> fRegisteredVariables =
            new System.Collections.ObjectModel.ObservableCollection<Variable>();

        // List<Variable> fParkedVariables;                                                                   //required for backing up the registeredVariables during a save procedure.
        /// <summary>The f zoom.</summary>
        private double fZoom = 1.0;

        /// <summary>The f hor scroll pos.</summary>
        private double fHorScrollPos;

        /// <summary>The f ver scroll pos.</summary>
        private double fVerScrollPos;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeEditorPage"/> class. Constructor for when only the id is known of the cluster containing
        ///     all the code.</summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="root">The neuron that owns the cluster with the code (required for possibly
        ///     creating the link between the neuron and the code.</param>
        /// <param name="linkId">The link Id.</param>
        public CodeEditorPage(string title, Neuron root, ulong linkId)
        {
            InternalCreate(title, root, linkId);
            Items = new CodeItemCollection(this, linkId);
        }

        /// <summary>Initializes a new instance of the <see cref="CodeEditorPage"/> class. Constructor for when the <paramref name="cluster"/> is already
        ///     created.</summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="root">The neuron that owns the <paramref name="cluster"/> with the code
        ///     (required for possibly creating the link between the neuron and the
        ///     code.</param>
        /// <param name="linkId">The link Id.</param>
        /// <param name="cluster">The cluster.</param>
        public CodeEditorPage(string title, Neuron root, ulong linkId, NeuronCluster cluster)
        {
            InternalCreate(title, root, linkId);
            Items = new CodeItemCollection(this, cluster);
        }

        /// <summary>Internally creates the instance</summary>
        /// <param name="title">The title.</param>
        /// <param name="root">The root.</param>
        /// <param name="linkId">The link Id.</param>
        private void InternalCreate(string title, Neuron root, ulong linkId)
        {
            Title = title;
            Item = root;
            LinkMeaning = linkId;
        }

        #endregion

        #region Prop

        #region Title

        /// <summary>
        ///     Gets the title of this page.
        /// </summary>
        public string Title { get; internal set; }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of code items for this page.
        /// </summary>
        public CodeItemCollection Items { get; internal set; }

        #endregion

        #region LinkMeaning

        /// <summary>
        ///     Gets the id of the neuron assigned to the meaning of the link between
        ///     the code cluster,wrapped by this page, and the neuron that links to
        ///     it.
        /// </summary>
        public ulong LinkMeaning { get; internal set; }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     gets the list with all the selected items.
        /// </summary>
        public System.Collections.Generic.IList<CodeItem> SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        #endregion

        #region SelectedItem

        /// <summary>
        ///     gets the first selected item.
        /// </summary>
        /// <remarks>
        ///     Used by the ContextMenu of the CodeEditorPage to find the codeItem's
        ///     IsBreakPoint property (and possibly others).
        /// </remarks>
        public CodeItem SelectedItem
        {
            get
            {
                if (fSelectedItems.Count > 0)
                {
                    return fSelectedItems[0];
                }

                return null;
            }
        }

        #endregion

        #region Zoom

        /// <summary>
        ///     Gets/sets the zoom that should be applied to the visual.
        /// </summary>
        public double Zoom
        {
            get
            {
                return fZoom;
            }

            set
            {
                if (value < 0.001)
                {
                    // need to make certain we don't make it to small.
                    value = 0.001;
                }

                if (fZoom != value)
                {
                    fZoom = value;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                }
            }
        }

        /// <summary>
        ///     Gets/sets the inverse value of the zoom factor that should be applied.
        ///     This is used to re-adjust zoom values for overlays (bummer, need to
        ///     work this way for wpf).
        /// </summary>
        public double ZoomInverse
        {
            get
            {
                return 1 / fZoom;
            }
        }

        /// <summary>
        ///     Gets/sets the zoom factor that should be applied, expressed in procent
        ///     values.
        /// </summary>
        public double ZoomProcent
        {
            get
            {
                return fZoom * 100;
            }

            set
            {
                var iVal = value / 100;
                if (fZoom != iVal)
                {
                    fZoom = iVal;
                    OnPropertyChanged("Zoom");
                    OnPropertyChanged("ZoomInverse");
                    OnPropertyChanged("ZoomProcent");
                }
            }
        }

        #endregion

        #region HorScrollPos

        /// <summary>
        ///     Gets/sets the horizontal scroll position
        /// </summary>
        public double HorScrollPos
        {
            get
            {
                return fHorScrollPos;
            }

            set
            {
                if (fHorScrollPos != value)
                {
                    fHorScrollPos = value;
                    OnPropertyChanged("HorScrollPos");
                }
            }
        }

        #endregion

        #region VerScrollPos

        /// <summary>
        ///     Gets/sets the vertical scroll position
        /// </summary>
        public double VerScrollPos
        {
            get
            {
                return fVerScrollPos;
            }

            set
            {
                if (value < 0)
                {
                    // can't have values smaller than 0.
                    value = 0;
                }

                if (value != fVerScrollPos)
                {
                    fVerScrollPos = value;
                    OnPropertyChanged("VerScrollPos");
                }
            }
        }

        #endregion

        // #region RegisteredVariables

        ///// <summary>
        ///// Gets the list of all the variables that have been used in this neural function.
        ///// </summary>
        ///// <remarks>
        ///// this list gets populated through the <see cref="CodeItemVariable"/>'s which add/remove their neuron
        ///// during construction/destruction.  This list is than monitored for changes so we can update the 
        ///// brainData's list of toolbox items.
        ///// </remarks>
        // public ObservableCollection<Variable> RegisteredVariables
        // {
        // get { return fRegisteredVariables; }
        // }

        // #endregion
        #region INeuronWrapper Members

        /// <summary>
        ///     This property actually returns the owner of this cluster (the neuron
        ///     for which this list is a rules or actions list).
        /// </summary>
        public Neuron Item { get; private set; }

        #endregion

        #region IEditorSelection Members

        /// <summary>
        ///     Gets the list of selected items.
        /// </summary>
        /// <value>
        ///     The selected items.
        /// </value>
        System.Collections.IList IEditorSelection.SelectedItems
        {
            get
            {
                return fSelectedItems;
            }
        }

        /// <summary>
        ///     Gets/sets the currently selected item. If there are multiple
        ///     selections, the first is returned.
        /// </summary>
        /// <value>
        /// </value>
        object IEditorSelection.SelectedItem
        {
            get
            {
                return SelectedItem;
            }
        }

        #endregion

        #endregion

        #region Functions

        ///// <summary>
        ///// Temporarely removes all the registered variables so that the corresponding toolbox items are also removed from the BrainData.
        ///// To put the variables back, use <see cref="CodeEditorPAge.UnParkVariables"/>.
        ///// </summary>
        // public void ParkVariables()
        // {
        // fParkedVariables = new List<Variable>(fRegisteredVariables);
        // fRegisteredVariables.Clear();
        // }

        // public void UnParkVariables()
        // {
        // foreach (Variable i in fParkedVariables)
        // fRegisteredVariables.Add(i);
        // fParkedVariables = null;
        // }
        #region Order

        /// <summary>Checks if the MoveDown command can be executed on the currently
        ///     selected items if all items are supposed to be in the specified list.</summary>
        /// <param name="list">The list from which all the selected items should come from if a move
        ///     is allowed.</param>
        /// <returns>True if there are selected items and all come from the specified list.</returns>
        internal bool CanMoveDownFor(CodeItemCollection list)
        {
            if (SelectedItems.Count > 0)
            {
                foreach (var i in SelectedItems)
                {
                    var iIndex = list != null ? list.IndexOf(i) : -1;
                    if (iIndex == -1 || iIndex == list.Count)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>Checks if the MoveUp command can be executed on the currently
        ///     selected items if all items are supposed to be in the specified list.</summary>
        /// <param name="list">The list from which all the selected items should come from if a move
        ///     is allowed.</param>
        /// <returns>True if there are selected items and all come from the specified
        ///     list.</returns>
        internal bool CanMoveUpFor(CodeItemCollection list)
        {
            if (SelectedItems.Count > 0)
            {
                foreach (var i in SelectedItems)
                {
                    var iIndex = list != null ? list.IndexOf(i) : -1;
                    if (iIndex == -1 || iIndex == 0)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>Moves the currently selected items 1 up in the specified list.</summary>
        /// <param name="list">The list that should own all the currently sected items (not checked,
        ///     use <see cref="CodeEditorPage.CanMoveUpFor"/> for that</param>
        internal void MoveUpFor(CodeItemCollection list)
        {
            var iToMove = (from i in SelectedItems select i).ToList();

                // need to make a copy of the list so we can modify it.
            foreach (var i in iToMove)
            {
                var iIndex = list.IndexOf(i);
                list.Move(iIndex, iIndex - 1); // move up in the designer is move to the front of the list.
            }
        }

        /// <summary>The move down for.</summary>
        /// <param name="list">The list.</param>
        internal void MoveDownFor(CodeItemCollection list)
        {
            var iToMove = (from i in SelectedItems select i).ToList();

                // need to make a copy of the list so we can modify it.
            foreach (var i in iToMove)
            {
                var iIndex = list.IndexOf(i);
                list.Move(iIndex, iIndex + 1); // move up in the designer is move to the front of the list.
            }
        }

        #endregion

        /// <summary>The move to end for.</summary>
        /// <param name="list">The list.</param>
        internal void MoveToEndFor(CodeItemCollection list)
        {
            var iToMove = (from i in SelectedItems select i).ToList();

                // need to make a copy of the list so we can modify it.
            var iCount = list.Count - 1;
            foreach (var i in iToMove)
            {
                var iIndex = list.IndexOf(i);
                list.Move(iIndex, iCount);
                iCount--;
            }
        }

        /// <summary>The move to start for.</summary>
        /// <param name="list">The list.</param>
        internal void MoveToStartFor(CodeItemCollection list)
        {
            var iToMove = (from i in SelectedItems select i).ToList();

                // need to make a copy of the list so we can modify it.
            var iCount = 0;
            foreach (var i in iToMove)
            {
                var iIndex = list.IndexOf(i);
                list.Move(iIndex, iCount);
                iCount++;
            }
        }

        #endregion
    }
}