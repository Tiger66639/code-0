// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowEditor.cs" company="">
//   
// </copyright>
// <summary>
//   The flow editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>The flow editor.</summary>
    public class FlowEditor : EditorBase
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="FlowEditor" /> class.
        /// </summary>
        public FlowEditor()
        {
            Flows = new FlowCollection(this);
        }

        #endregion

        /// <summary>The unload ui data.</summary>
        protected override void UnloadUIData()
        {
            foreach (var i in Flows)
            {
                i.IsOpen = false;
            }
        }

        /// <summary>The load ui data.</summary>
        protected override void LoadUIData()
        {
            foreach (var i in Flows)
            {
                i.IsOpen = true;
            }
        }

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <remarks>This is used to determin which neurons need to be exported when an
        ///     editor is selected for export.</remarks>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            foreach (var i in Flows)
            {
                yield return i.Item;
            }
        }

        #region Fields

        /// <summary>The f selected flow.</summary>
        private Flow fSelectedFlow;

        /// <summary>The f display as list.</summary>
        private bool fDisplayAsList;

        /// <summary>The f overlay visibility.</summary>
        private System.Windows.Visibility fOverlayVisibility;

        #endregion

        #region Prop

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific.
        /// </summary>
        /// <value>
        /// </value>
        public override string Icon
        {
            get
            {
                return "/Images/Flow/flow.png";
            }
        }

        #endregion

        #region DescriptionTitle

        /// <summary>
        ///     Gets a title that the description editor can use to display in the
        ///     header.
        /// </summary>
        /// <value>
        /// </value>
        public override string DescriptionTitle
        {
            get
            {
                return Name + " - Flow Editor";
            }
        }

        #endregion

        #region Flows

        /// <summary>
        ///     Gets the list of frames
        /// </summary>
        public FlowCollection Flows { get; private set; }

        #endregion

        #region SelectedFlow

        /// <summary>
        ///     Gets/sets the currently selected flow.
        /// </summary>
        /// <remarks>
        ///     This is provided so that we can change the active flow easely from
        ///     code.
        /// </remarks>
        public Flow SelectedFlow
        {
            get
            {
                return fSelectedFlow;
            }

            set
            {
                if (value != fSelectedFlow)
                {
                    if (fSelectedFlow != null)
                    {
                        fSelectedFlow.SetSelected(false);
                    }

                    fSelectedFlow = value;
                    if (fSelectedFlow != null)
                    {
                        fSelectedFlow.SetSelected(true);
                        Flows.SelectionHistory.Add(fSelectedFlow);
                    }

                    OnPropertyChanged("SelectedFlow");
                }
            }
        }

        /// <summary>Updates the specified <paramref name="flow"/> as selected or not
        ///     selected without asking the <paramref name="flow"/> to update it's
        ///     selected value.</summary>
        /// <param name="flow">The flow.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        internal void SetSelectedFlow(Flow flow, bool value)
        {
            if (value)
            {
                if (flow != fSelectedFlow)
                {
                    if (fSelectedFlow != null)
                    {
                        fSelectedFlow.SetSelected(false);
                    }

                    fSelectedFlow = flow;
                    Flows.SelectionHistory.Add(fSelectedFlow);
                }
            }
            else
            {
                fSelectedFlow = null;
            }

            OnPropertyChanged("SelectedFlow");
        }

        /// <summary>Sets the selected <paramref name="flow"/> without adding navigation
        ///     information, so that it can be used by the navigation commands.</summary>
        /// <param name="flow">The flow.</param>
        internal void SetSelectedFlowFromNavigation(Flow flow)
        {
            if (flow != fSelectedFlow)
            {
                if (fSelectedFlow != null)
                {
                    fSelectedFlow.SetSelected(false);
                }

                fSelectedFlow = flow;
                if (fSelectedFlow != null)
                {
                    fSelectedFlow.SetSelected(true);
                }

                OnPropertyChanged("SelectedFlow");
            }
        }

        #endregion

        #region DisplayAsList

        /// <summary>
        ///     Gets/sets wether the flows are displayed in a list or in a single
        ///     detail view.
        /// </summary>
        public bool DisplayAsList
        {
            get
            {
                return fDisplayAsList;
            }

            set
            {
                if (value != fDisplayAsList)
                {
                    fDisplayAsList = value;

                    OnPropertyChanged("DisplayAsList");
                }
            }
        }

        #endregion

        #region OverlayVisibility

        /// <summary>
        ///     Gets/sets the overlay visibility.
        /// </summary>
        public System.Windows.Visibility OverlayVisibility
        {
            get
            {
                return fOverlayVisibility;
            }

            set
            {
                fOverlayVisibility = value;
                OnPropertyChanged("OverlayVisibility");
            }
        }

        #endregion

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public override string DocumentInfo
        {
            get
            {
                return "Flow editor: " + Name;
            }
        }

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public override string DocumentType
        {
            get
            {
                return "Flow editor";
            }
        }

        #endregion

        #region functions

        /// <summary>
        ///     Called when the editor is loaded from stream. Allows The editor to
        ///     register things, like neurons it monitors. Let all the flows register
        ///     as well.
        /// </summary>
        public override void Register()
        {
            foreach (var i in Flows)
            {
                i.Register();
            }
        }

        /// <summary>Creates the correct <see cref="FlowItem"/> for the specified neuron.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <returns>The <see cref="FlowItem"/>.</returns>
        public static FlowItem CreateFlowItemFor(Neuron toWrap)
        {
            var iToWrap = toWrap as NeuronCluster;
            if (iToWrap != null)
            {
                if (iToWrap.Meaning == (ulong)PredefinedNeurons.FlowItemConditional)
                {
                    return new FlowItemConditional(iToWrap);
                }

                if (iToWrap.Meaning == (ulong)PredefinedNeurons.FlowItemConditionalPart)
                {
                    return new FlowItemConditionalPart(iToWrap);
                }

                return new FlowItemStatic(toWrap);
            }

            return new FlowItemStatic(toWrap);
        }

        /// <summary>
        ///     need to reasign the list and the selected item so that they are
        ///     change, this will force a complete redraw.
        /// </summary>
        internal void ReDrawFlows()
        {
            var iSelected = SelectedFlow;
            var iList = Flows;
            Flows = null;
            OnPropertyChanged("Flows");
            SelectedFlow = null;
            Flows = iList;
            OnPropertyChanged("Flows");
            SelectedFlow = iSelected;
        }

        #endregion

        #region Xml

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <remarks>This function is called for each element that is found, so this
        ///     function should check which element it is and only read that element
        ///     accordingly.</remarks>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Flow")
            {
                var iNeuronSer = new System.Xml.Serialization.XmlSerializer(typeof(Flow));
                var iNode = (Flow)iNeuronSer.Deserialize(reader);
                Flows.Add(iNode);
                return true;
            }

            return base.ReadXmlInternal(reader);
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            var iFlowSer = new System.Xml.Serialization.XmlSerializer(typeof(Flow));
            foreach (var i in Flows)
            {
                iFlowSer.Serialize(writer, i);
            }
        }

        #endregion

        #region Clipboard

        /// <summary>Copies the selected <paramref name="data"/> to the clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            data.SetData(Properties.Resources.NeuronIDFormat, SelectedFlow.ItemID);
        }

        /// <summary>The can copy to clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanCopyToClipboard()
        {
            return SelectedFlow != null;
        }

        /// <summary>The can paste special from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return CanPasteFromClipboard();
        }

        /// <summary>The paste special from clipboard.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void PasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            RemoveSelectedFlow();
        }

        /// <summary>The can paste from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanPasteFromClipboard()
        {
            if (base.CanPasteFromClipboard())
            {
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
                {
                    var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat);
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(iId, out iFound))
                    {
                        var iCluster = iFound as NeuronCluster;
                        if (iCluster != null)
                        {
                            return iCluster.Meaning == (ulong)PredefinedNeurons.Flow;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
                {
                    var iList =
                        (System.Collections.Generic.List<ulong>)
                        System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat);
                    var iNeurons = from i in iList where Brain.Current.IsValidID(i) select Brain.Current[i];

                        // we need to filter out the invalid id's (because they got deleted while beig on the clipboard for instance).
                    var iCount =
                        (from i in iNeurons
                         where i is NeuronCluster && ((NeuronCluster)i).Meaning == (ulong)PredefinedNeurons.Flow
                         select i).Count();
                    return iCount == iList.Count;
                }
            }

            return false;
        }

        /// <summary>The paste from clipboard.</summary>
        public override void PasteFromClipboard()
        {
            if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat);
                var iFlow = new Flow();
                iFlow.ItemID = iId;
                Flows.Add(iFlow);
            }
            else if (System.Windows.Clipboard.ContainsData(Properties.Resources.MultiNeuronIDFormat))
            {
                var iList =
                    (System.Collections.Generic.List<ulong>)
                    System.Windows.Clipboard.GetData(Properties.Resources.MultiNeuronIDFormat);
                foreach (var i in iList)
                {
                    var iFlow = new Flow();
                    iFlow.ItemID = i;
                    Flows.Add(iFlow);
                }
            }
        }

        #endregion

        #region delete

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        public override void DeleteEditor()
        {
            foreach (var i in Flows)
            {
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
            iDeleter.Start((from i in Flows select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            foreach (var i in Flows)
            {
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start((from i in Flows select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        /// <summary>The delete.</summary>
        public override void Delete()
        {
            var iToDelete = SelectedFlow;
            System.Diagnostics.Debug.Assert(iToDelete != null);
            var iRes =
                System.Windows.MessageBox.Show(
                    string.Format(
                        "Remove flow: {0} from the designer and from the network when no longer referenced?", 
                        iToDelete.Name), 
                    "Delete", 
                    System.Windows.MessageBoxButton.YesNo, 
                    System.Windows.MessageBoxImage.Question);
            if (iRes == System.Windows.MessageBoxResult.Yes)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we group all the data together so a single undo command cand restore the previous state.
                try
                {
                    var iToRemove = iToDelete.Item;

                        // make a local copy, cause RemoveSelectedRow disconnects the item from the flow.
                    RemoveSelectedFlow();
                    var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
                    iDeleter.Start(iToRemove);
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>
        ///     Removes the selected flow from the editor. Note: side effect: the
        ///     selected flow's Item is reset.
        /// </summary>
        private void RemoveSelectedFlow()
        {
            var iToRemove = SelectedFlow;
            var iIndex = Flows.IndexOf(iToRemove);
            iToRemove.NeuronInfo.IsLocked = false;

                // we unlock the neuron since the editor is also removed. This allows the editor the be deleted.
            Flows.RemoveAt(iIndex);
            if (iIndex < Flows.Count)
            {
                SelectedFlow = Flows[iIndex];
            }
            else if (Flows.Count > 0)
            {
                SelectedFlow = Flows[Flows.Count - 1];
            }
            else
            {
                SelectedFlow = null;
            }
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return SelectedFlow != null;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            var iToDelete = SelectedFlow;
            System.Diagnostics.Debug.Assert(iToDelete != null);
            var iToRemove = iToDelete.Item;
            if (iToRemove.ID == Neuron.EmptyId)
            {
                // this should not happen, but it is, so check.
                System.Windows.MessageBox.Show(
                    string.Format(
                        "Neuron '{0}' can't be deleted because it has already been removed from the network.", 
                        iToDelete), 
                    "Delete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
                RemoveSelectedFlow();
                return;
            }

            var iDlg = new DlgSelectDeletionMethod();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we group all the data together so a single undo command cand restore the previous state.
                try
                {
                    NeuronDeleter iDeleter;
                    switch (iDlg.NeuronDeletionMethod)
                    {
                        case DeletionMethod.Remove:
                            RemoveSelectedFlow();
                            break;
                        case DeletionMethod.DeleteIfNoRef:
                            RemoveSelectedFlow();

                                // make a local copy, cause RemoveSelectedRow disconnects the item from the flow.
                            iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                            iDeleter.Start(iToRemove);
                            break;
                        case DeletionMethod.Delete:
                            if (iToDelete.Item.CanBeDeleted)
                            {
                                RemoveSelectedFlow();
                                iDeleter = new NeuronDeleter(DeletionMethod.Delete, iDlg.BranchHandling);
                                iDeleter.Start(iToRemove);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show(
                                    string.Format(
                                        "Neuron '{0}' can't be deleted because it is used as a meaning or info.", 
                                        iToDelete), 
                                    "Delete", 
                                    System.Windows.MessageBoxButton.OK, 
                                    System.Windows.MessageBoxImage.Warning);
                            }

                            break;
                        default:
                            throw new System.InvalidOperationException();
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanDeleteSpecial()
        {
            return CanDelete();
        }

        #endregion
    }
}