// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualsEditor.cs" company="">
//   
// </copyright>
// <summary>
//   The visual editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>The visual editor.</summary>
    public class VisualEditor : EditorBase
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="VisualEditor" /> class.
        /// </summary>
        public VisualEditor()
        {
            Visuals = new VisualFrameCollection(this);
        }

        #endregion

        #region Fields

        /// <summary>The f selected visual.</summary>
        private VisualFrame fSelectedVisual;

        /// <summary>The f nr hor items.</summary>
        private int fNrHorItems = 2;

        /// <summary>The f nr ver items.</summary>
        private int fNrVerItems = 2;

        /// <summary>The f low value.</summary>
        private int fLowValue;

        /// <summary>The f high value.</summary>
        private int fHighValue = 64;

        /// <summary>The f low val operator.</summary>
        private Neuron fLowValOperator;

        /// <summary>The f high val operator.</summary>
        private Neuron fHighValOperator;

        #endregion

        #region Prop

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific. start with /
        /// </summary>
        public override string Icon
        {
            get
            {
                return "/Images/VisualF/VisualFrame_Enabled.png";
            }
        }

        #endregion

        #region DocumentInfo

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
                return Name;
            }
        }

        #endregion

        #region DocumentType

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
                return "Visual editor";
            }
        }

        #endregion

        #region Visuals

        /// <summary>
        ///     Gets the list of visual frame that belong to this editor.
        /// </summary>
        public VisualFrameCollection Visuals { get; private set; }

        #endregion

        #region SelectedVisual

        /// <summary>
        ///     Gets/sets the currently selected frame.
        /// </summary>
        public VisualFrame SelectedVisual
        {
            get
            {
                return fSelectedVisual;
            }

            set
            {
                if (fSelectedVisual != value)
                {
                    if (fSelectedVisual != null)
                    {
                        fSelectedVisual.SetIsSelected(false);
                    }

                    fSelectedVisual = value;
                    if (fSelectedVisual != null)
                    {
                        fSelectedVisual.SetIsSelected(true);
                    }

                    OnPropertyChanged("SelectedVisual");
                }
            }
        }

        #endregion

        #region NrHorItems

        /// <summary>
        ///     Gets/sets the nr of items there are horizontally in each frame
        ///     contained by this editor.
        /// </summary>
        public int NrHorItems
        {
            get
            {
                return fNrHorItems;
            }

            set
            {
                if (fNrHorItems != value)
                {
                    OnPropertyChanging("NrHorItems", fNrHorItems, value);

                        // we generate undo data this way, which means we can't generate undo data for the newly created/removed objects, but we can undo at the correct level so that it is correctly shown to the user..
                    if (value > fNrHorItems)
                    {
                        AddHorItems(value - fNrHorItems);
                    }
                    else
                    {
                        RemoveHorItems(fNrHorItems - value);
                    }

                    fNrHorItems = value;
                    OnPropertyChanged("NrHorItems");
                }
            }
        }

        #endregion

        #region NrVerItems

        /// <summary>
        ///     Gets/sets the nr of items that are displayed vertically in each frame
        ///     displayed by this editor.
        /// </summary>
        public int NrVerItems
        {
            get
            {
                return fNrVerItems;
            }

            set
            {
                if (value != fNrVerItems)
                {
                    OnPropertyChanging("NrVerItems", fNrVerItems, value);

                        // we generate undo data this way, which means we can't generate undo data for the newly created/removed objects, but we can undo at the correct level so that it is correctly shown to the user..
                    if (value > fNrVerItems)
                    {
                        AddVerItems(value - fNrVerItems);
                    }
                    else
                    {
                        RemoveVerItems(fNrVerItems - value);
                    }

                    fNrVerItems = value;
                    OnPropertyChanged("NrVerItems");
                }
            }
        }

        #endregion

        #region LowValue

        /// <summary>
        ///     Gets/sets the value that should be assigned to white spaces.
        /// </summary>
        public int LowValue
        {
            get
            {
                return fLowValue;
            }

            set
            {
                if (value != fLowValue)
                {
                    if (value == HighValue)
                    {
                        throw new System.InvalidOperationException("High and low range must be different!");
                    }

                    OnPropertyChanging("LowValue", fLowValue, value);
                    AdjustValues(fLowValue, value);
                    fLowValue = value;
                    OnPropertyChanged("LowValue");
                }
            }
        }

        #endregion

        #region LowValOperator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> that should be used to
        ///     compare the low values against the input.
        /// </summary>
        public Neuron LowValOperator
        {
            get
            {
                return fLowValOperator;
            }

            set
            {
                if (value != fLowValOperator)
                {
                    AdjustOperator(fLowValue, value);
                    fLowValOperator = value;
                    OnPropertyChanged("LowValOperator");
                }
            }
        }

        #endregion

        #region HighValue

        /// <summary>
        ///     Gets/sets the value to assign for selected items.
        /// </summary>
        public int HighValue
        {
            get
            {
                return fHighValue;
            }

            set
            {
                if (value != fHighValue)
                {
                    if (value == LowValue)
                    {
                        throw new System.InvalidOperationException("High and low range must be different!");
                    }

                    OnPropertyChanging("HighValue", fHighValue, value);
                    AdjustValues(fHighValue, value);
                    fHighValue = value;
                    OnPropertyChanged("HighValue");
                }
            }
        }

        #endregion

        #region HighValOperator

        /// <summary>
        ///     Gets/sets the <see langword="operator" /> to use for comparing the high
        ///     values against the input.
        /// </summary>
        public Neuron HighValOperator
        {
            get
            {
                return fHighValOperator;
            }

            set
            {
                if (fHighValOperator != value)
                {
                    AdjustOperator(fHighValue, value);
                    fHighValOperator = value;
                    OnPropertyChanged("HighValOperator");
                }
            }
        }

        #endregion

        #endregion

        #region functions

        #region clipboard

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            data.SetData(Properties.Resources.NeuronIDFormat, SelectedVisual.Item.ID);
            data.SetData(Properties.Resources.VISUALFRAMEFORMAT, SelectedVisual.Item.ID);
        }

        /// <summary>The can copy to clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanCopyToClipboard()
        {
            return SelectedVisual != null;
        }

        /// <summary>The can paste special from clipboard.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return false;
        }

        /// <summary>The paste special from clipboard.</summary>
        /// <exception cref="NotImplementedException"></exception>
        public override void PasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Determines whether this instance can paste from the clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste from the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanPasteFromClipboard()
        {
            if (base.CanPasteFromClipboard())
            {
                if (System.Windows.Clipboard.ContainsData(Properties.Resources.NeuronIDFormat))
                {
                    Neuron iFound;
                    if (
                        Brain.Current.TryFindNeuron(
                            (ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat), 
                            out iFound))
                    {
                        var iFrame = iFound as NeuronCluster;
                        return iFrame != null && iFrame.Meaning == (ulong)PredefinedNeurons.VisualFrame;
                    }
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.VISUALFRAMEFORMAT))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The paste from clipboard.</summary>
        public override void PasteFromClipboard()
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iFrameId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.VISUALFRAMEFORMAT);
                    var iFrame = new VisualFrame((NeuronCluster)Brain.Current[iFrameId]);
                    if (iFrame != null)
                    {
                        if (System.Windows.Clipboard.ContainsData(Properties.Resources.CUTOPERATION) == false)
                        {
                            // if it was a cut operation, we don't need to make a duplicate of the data.
                            iFrame = EditorsHelper.DuplicateVisualF(iFrame);
                        }

                        Visuals.Add(iFrame);
                    }
                    else
                    {
                        var iToPaste =
                            Brain.Current[(ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat)]
                            as NeuronCluster;
                        System.Diagnostics.Debug.Assert(iToPaste != null);
                        Visuals.Add(new VisualFrame(iToPaste));
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("Paste to Visual editor", iMsg);
            }
        }

        #endregion

        #region delete

        /// <summary>The remove.</summary>
        public override void Remove()
        {
            var iIndex = Visuals.IndexOf(SelectedVisual);
            Visuals.Remove(SelectedVisual);
            if (Visuals.Count > iIndex)
            {
                SelectedVisual = Visuals[iIndex];
            }
            else if (Visuals.Count > 0)
            {
                SelectedVisual = Visuals[Visuals.Count - 1];
            }
        }

        /// <summary>The delete.</summary>
        public override void Delete()
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iSelected = SelectedVisual;
                    var iIndex = Visuals.IndexOf(SelectedVisual);
                    Visuals.Remove(iSelected);

                        // important to remove the frame from the list  before it gets deleted for the undo data, otherwise we try to add the frame to the list when the backing neuron is still deleted.
                    EditorsHelper.DeleteVFrame(iSelected);
                    if (Visuals.Count > iIndex)
                    {
                        SelectedVisual = Visuals[iIndex];
                    }
                    else if (Visuals.Count > 0)
                    {
                        SelectedVisual = Visuals[Visuals.Count - 1];
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("Paste to Frame editor", iMsg);
            }
        }

        /// <summary>The can delete.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return SelectedVisual != null;
        }

        /// <summary>The delete special.</summary>
        public override void DeleteSpecial()
        {
            var iDlg = new DlgSelectDeletionMethod();
            iDlg.Owner = System.Windows.Application.Current.MainWindow;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);
                try
                {
                    NeuronDeleter iDeleter;
                    switch (iDlg.NeuronDeletionMethod)
                    {
                        case DeletionMethod.Nothing:
                            break;
                        case DeletionMethod.Remove:
                            Visuals.Remove(SelectedVisual);
                            SelectedVisual = null;
                            break;
                        case DeletionMethod.DeleteIfNoRef:
                            Visuals.Remove(SelectedVisual);
                            iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                            iDeleter.Start(SelectedVisual.Item);
                            SelectedVisual = null;
                            break;
                        case DeletionMethod.Delete:
                            Visuals.Remove(SelectedVisual);
                            iDeleter = new NeuronDeleter(DeletionMethod.Delete, iDlg.BranchHandling);
                            iDeleter.Start(SelectedVisual.Item);
                            SelectedVisual = null;
                            break;
                        default:
                            break;
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The can delete special.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDeleteSpecial()
        {
            return CanDelete();
        }

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        public override void DeleteEditor()
        {
            foreach (var i in Enumerable.ToArray(Visuals))
            {
                // we make a local copy cause the loop will probably alter the list.
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
            iDeleter.Start((from i in Visuals select i.Item).ToArray());

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
            // no need for try catch, the caller has done this.
            foreach (var i in Visuals.ToArray())
            {
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start((from i in Visuals select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        #endregion

        #region resizing

        /// <summary>Removes x nr ver items. Doesn't remove this value to 'NrVerItems'.
        ///     Should be called before this value gets updated.</summary>
        /// <param name="nrItems">The nr items.</param>
        private void RemoveVerItems(int nrItems)
        {
            foreach (var iFrame in Visuals)
            {
                for (var i = 0; i < nrItems * NrHorItems; i++)
                {
                    Brain.Current.Delete(iFrame.Items[iFrame.Items.Count - 1 - i].Item);

                        // the ClusterCollection is updated async, so we can't simpy take the last item, we need to go back as we delete, otherwise we try to delete the null item.
                }
            }
        }

        /// <summary>Removes x nr hor items. Doesn't remove this value to 'NrHorItems'.
        ///     Should be called before this value gets updated.</summary>
        /// <param name="nrItems">The nr items.</param>
        private void RemoveHorItems(int nrItems)
        {
            foreach (var iFrame in Visuals)
            {
                var iPos = 0;
                for (var iRow = 0; iRow < NrVerItems; iRow++)
                {
                    for (var iNrItems = nrItems; iNrItems > 0; iNrItems--)
                    {
                        Brain.Current.Delete(iFrame.Items[iPos].Item);
                    }

                    iPos += NrHorItems;
                }
            }
        }

        /// <summary>Adds x nr of hor items. Doesn't add this value to 'NrHorItems'. Should
        ///     be called before this value gets updated.</summary>
        /// <param name="nrItems">The nr items.</param>
        private void AddHorItems(int nrItems)
        {
            foreach (var iFrame in Visuals)
            {
                var iPos = 0;
                for (var iRow = 0; iRow < NrVerItems; iRow++)
                {
                    for (var iNrItems = nrItems; iNrItems > 0; iNrItems--)
                    {
                        var iNew = NeuronFactory.GetInt();
                        Brain.Current.Add(iNew); // don't geneate undo data, this is done by the property setter.
                        using (var iList = iFrame.Items.Cluster.ChildrenW) iList.Insert(iPos, iNew);
                    }

                    iPos += NrHorItems + nrItems;
                }
            }
        }

        /// <summary>Adds x nr of ver items. Doesn't add this value to 'NrVerItems'. Should
        ///     be called before this value gets updated.</summary>
        /// <param name="nrItems">The nr items.</param>
        private void AddVerItems(int nrItems)
        {
            foreach (var iFrame in Visuals)
            {
                var iToAdd = nrItems * NrHorItems;
                while (iToAdd > 0)
                {
                    var iNew = NeuronFactory.GetInt();
                    Brain.Current.Add(iNew); // don't geneate undo data, this is done by the property setter.
                    using (var iList = iFrame.Items.Cluster.ChildrenW) iList.Add(iNew);
                    iToAdd--;
                }
            }
        }

        #endregion

        #region high/low

        /// <summary>Adjusts all the <see langword="int"/> values that match the<paramref name="prev"/> <paramref name="value"/> in each visual frame.</summary>
        /// <param name="prev">The prev.</param>
        /// <param name="value">The value.</param>
        private void AdjustValues(int prev, int value)
        {
            foreach (var i in Visuals)
            {
                foreach (var u in i.Items)
                {
                    if (u.Value == prev)
                    {
                        ((IntNeuron)u.Item).Value = value;

                            // we do a direct change and don't use the prop setter cause this generates undo data, which we don't want, cause we store a single undo item for the entire operation.
                    }
                }
            }
        }

        /// <summary>The adjust operator.</summary>
        /// <param name="level">The level.</param>
        /// <param name="value">The value.</param>
        private void AdjustOperator(int level, Neuron value)
        {
            foreach (var i in Visuals)
            {
                foreach (var u in i.Items)
                {
                    if (u.Value == level)
                    {
                        if (value != null)
                        {
                            u.Item.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Operator, value);
                        }
                        else
                        {
                            var iFound = u.Item.FindFirstOut((ulong)PredefinedNeurons.Operator);
                            if (iFound != null)
                            {
                                var iLink = Link.Find(u.Item, iFound, Brain.Current[(ulong)PredefinedNeurons.Operator]);
                                iLink.Destroy();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>Gets all the neurons that this editor contains directly.</summary>
        /// <returns>The <see cref="IEnumerable"/>.</returns>
        public override System.Collections.Generic.IEnumerable<Neuron> GetRootNeurons()
        {
            foreach (var i in Visuals)
            {
                yield return i.Item;
            }
        }

        #region xml

        /// <summary>Reads the fields/properties of the class.</summary>
        /// <remarks>This function is called for each element that is found, so this
        ///     function should check which element it is and only read that element
        ///     accordingly.</remarks>
        /// <param name="reader">The reader.</param>
        /// <returns>True if the item was properly read, otherwise false.</returns>
        protected override bool ReadXmlInternal(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Frame")
            {
                NeuronCluster iCluster;
                var id = XmlStore.ReadElement<ulong>(reader, "Frame");
                iCluster = Brain.Current[id] as NeuronCluster;
                if (iCluster == null)
                {
                    throw new System.InvalidOperationException(
                        "Failed to find a neuron, used by the designer, in the network.");
                }

                var iFrame = new VisualFrame(iCluster);
                Visuals.AddFromStream(iFrame);
                return true;
            }

            if (reader.Name == "NrHorItems")
            {
                fNrHorItems = XmlStore.ReadElement<int>(reader, "NrHorItems");
                return true;
            }

            if (reader.Name == "NrVerItems")
            {
                fNrVerItems = XmlStore.ReadElement<int>(reader, "NrVerItems");
                return true;
            }

            if (reader.Name == "LowValue")
            {
                fLowValue = XmlStore.ReadElement<int>(reader, "LowValue");
                return true;
            }

            if (reader.Name == "LowValOperator")
            {
                fLowValOperator = XmlStore.ReadNeuron(reader, "LowValOperator");
                return true;
            }

            if (reader.Name == "HighValue")
            {
                fHighValue = XmlStore.ReadElement<int>(reader, "HighValue");
                return true;
            }

            if (reader.Name == "HighValOperator")
            {
                fHighValOperator = XmlStore.ReadNeuron(reader, "HighValOperator");
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
            XmlStore.WriteElement(writer, "NrHorItems", NrHorItems);
            XmlStore.WriteElement(writer, "NrVerItems", NrVerItems);

            XmlStore.WriteElement(writer, "HighValue", HighValue);
            XmlStore.WriteElement(writer, "LowValue", LowValue);
            XmlStore.WriteNeuron(writer, "LowValOperator", fLowValOperator);
            XmlStore.WriteNeuron(writer, "HighValOperator", fHighValOperator);
            foreach (var i in Visuals)
            {
                XmlStore.WriteElement(writer, "Frame", i.Item.ID);
            }
        }

        #endregion

        #region overides

        /// <summary>
        ///     Called when the editor is loaded from stream. Allows The editor to
        ///     register things, like neurons it monitors. Every frame needs to be
        ///     registered.
        /// </summary>
        public override void Register()
        {
            foreach (var i in Visuals)
            {
                i.NeuronInfo.IsLocked = true;
            }
        }

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            foreach (var i in Visuals)
            {
                i.IsLoaded = true;
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        protected override void UnloadUIData()
        {
            foreach (var i in Visuals)
            {
                i.IsLoaded = false;
            }
        }

        #endregion

        #endregion
    }
}