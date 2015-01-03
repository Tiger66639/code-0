// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectFramesEditor.cs" company="">
//   
// </copyright>
// <summary>
//   An editor for frames that are atached to an object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     An editor for frames that are atached to an object.
    /// </summary>
    public class ObjectFramesEditor : NeuronEditor, IFrameOwner
    {
        #region Frames

        /// <summary>
        ///     Gets the list of frames
        /// </summary>
        public FramesCollection Frames
        {
            get
            {
                return fFrames;
            }

            private set
            {
                fFrames = value;
                OnPropertyChanged("Frames");
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
                return Name + " - Object-frames Editor";
            }
        }

        #endregion

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific. start with /
        /// </summary>
        /// <value>
        /// </value>
        public override string Icon
        {
            get
            {
                return "/Images/Frame/Object_Frames_Enabled.png";
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
                return "Object-frames editor for: " + Name;
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
                return "Object-frames editor";
            }
        }

        #endregion

        #region IFrameOwner.Frames

        /// <summary>Gets the frames.</summary>
        Data.ObservedCollection<Frame> IFrameOwner.Frames
        {
            get
            {
                return fFrames;
            }
        }

        #endregion

        #region SelectedFrame

        /// <summary>
        ///     Gets/sets the currently selected frame.
        /// </summary>
        public Frame SelectedFrame
        {
            get
            {
                return fSelectedFrame;
            }

            set
            {
                if (fSelectedFrame != value)
                {
                    if (fSelectedFrame != null)
                    {
                        fSelectedFrame.SetIsSelected(false);
                    }

                    fSelectedFrame = value;
                    if (fSelectedFrame != null)
                    {
                        fSelectedFrame.SetIsSelected(true);
                    }

                    OnPropertyChanged("SelectedFrame");
                }
            }
        }

        #endregion

        #region SelectedTabIndex

        /// <summary>
        ///     Gets/sets the index of the currently selected tab, which determins the
        ///     visible part af the frame editor: the elements or the sequences. The
        ///     ui links to this, so that we can control the selected item from here.
        /// </summary>
        public int SelectedTabIndex
        {
            get
            {
                return fSelectedTabIndex;
            }

            set
            {
                fSelectedTabIndex = value;
                OnPropertyChanged("SelectedTabIndex");
            }
        }

        #endregion

        #region Fields

        /// <summary>The f frames.</summary>
        private FramesCollection fFrames;

        /// <summary>The f selected frame.</summary>
        private Frame fSelectedFrame;

        /// <summary>The f selected tab index.</summary>
        private int fSelectedTabIndex;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ObjectFramesEditor"/> class. 
        ///     Initializes a new instance of the <see cref="ObjectFramesEditor"/>
        ///     class.</summary>
        public ObjectFramesEditor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ObjectFramesEditor"/> class. Initializes a new instance of the <see cref="ObjectFramesEditor"/>
        ///     class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public ObjectFramesEditor(Neuron toWrap)
            : base(toWrap)
        {
        }

        #endregion

        #region functions

        #region clipboard

        /// <summary>The copy to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            data.SetData(Properties.Resources.NeuronIDFormat, SelectedFrame.Item.ID);
            data.SetData(Properties.Resources.FRAMEFORMAT, SelectedFrame.Item.ID);
        }

        /// <summary>
        ///     Determines whether this instance can copy the selected data to the
        ///     clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can copy to the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanCopyToClipboard()
        {
            return SelectedFrame != null;
        }

        /// <summary>
        ///     Determines whether this instance can paste special from the clipboard.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste special from the clipboard;
        ///     otherwise, <c>false</c> .
        /// </returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return false;
        }

        /// <summary>
        ///     Pastes the data in a special way from the clipboard.
        /// </summary>
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
                        return iFrame != null && iFrame.Meaning == (ulong)PredefinedNeurons.Frame;
                    }
                }
                else if (System.Windows.Clipboard.ContainsData(Properties.Resources.FRAMEFORMAT))
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
                    var iFrameId = (ulong)System.Windows.Clipboard.GetData(Properties.Resources.FRAMEFORMAT);
                    var iFrame = new Frame((NeuronCluster)Brain.Current[iFrameId]);
                    if (iFrame != null)
                    {
                        if (System.Windows.Clipboard.ContainsData(Properties.Resources.CUTOPERATION) == false)
                        {
                            // if it was a cut operation, we don't need to make a duplicate of the data.
                            iFrame = EditorsHelper.DuplicateFrame(iFrame);
                        }

                        Frames.Add(iFrame);
                    }
                    else
                    {
                        var iToPaste =
                            Brain.Current[(ulong)System.Windows.Clipboard.GetData(Properties.Resources.NeuronIDFormat)]
                            as NeuronCluster;
                        System.Diagnostics.Debug.Assert(iToPaste != null);
                        Frames.Add(new Frame(iToPaste));
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

        #endregion

        #region delete

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            var iIndex = Frames.IndexOf(SelectedFrame);
            Frames.Remove(SelectedFrame);
            if (Frames.Count > iIndex)
            {
                SelectedFrame = Frames[iIndex];
            }
            else if (Frames.Count > 0)
            {
                SelectedFrame = Frames[Frames.Count - 1];
            }
            else
            {
                DeleteCollection();
            }
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iSelected = SelectedFrame;
                    var iIndex = Frames.IndexOf(SelectedFrame);
                    Frames.Remove(iSelected);

                        // important to remove the frame from the list  before it gets deleted for the undo data, otherwise we try to add the frame to the list when the backing neuron is still deleted.
                    EditorsHelper.DeleteFrame(iSelected);
                    if (Frames.Count > iIndex)
                    {
                        SelectedFrame = Frames[iIndex];
                    }
                    else if (Frames.Count > 0)
                    {
                        SelectedFrame = Frames[Frames.Count - 1];
                    }
                    else
                    {
                        DeleteCollection();
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

        /// <summary>
        ///     Deletes the cluster used as collection. This is usually done because
        ///     it became empty, so we need to remove the reference + the cluster.
        ///     This will automatically make a temp collection again.
        /// </summary>
        private void DeleteCollection()
        {
            WindowMain.DeleteItemFromBrain(fFrames.Cluster);
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return SelectedFrame != null;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
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
                            Frames.Remove(SelectedFrame);
                            SelectedFrame = null;
                            break;
                        case DeletionMethod.DeleteIfNoRef:
                            Frames.Remove(SelectedFrame);
                            iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, iDlg.BranchHandling);
                            iDeleter.Start(SelectedFrame.Item);
                            SelectedFrame = null;
                            break;
                        case DeletionMethod.Delete:
                            Frames.Remove(SelectedFrame);
                            iDeleter = new NeuronDeleter(DeletionMethod.Delete, iDlg.BranchHandling);
                            iDeleter.Start(SelectedFrame.Item);
                            SelectedFrame = null;
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

        /// <summary>
        ///     Deletes all the neurons on the editor that aren't referenced anywhere
        ///     else, if appropriate for the editor. This is called when the editor is
        ///     removed from the project. Usually, the user will expect unused data to
        ///     get removed as well.
        /// </summary>
        /// <remarks>
        ///     Doesn't need to be undo save, this is done by the caller.
        /// </remarks>
        public override void DeleteEditor()
        {
            foreach (var i in Enumerable.ToArray(Frames))
            {
                // we make a local copy cause the loop will probably alter the list.
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(DeletionMethod.DeleteIfNoRef, DeletionMethod.DeleteIfNoRef);
            iDeleter.Start((from i in Frames select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is removed from the project. Usually, the user will expect unused data
        ///     to get removed as well.</summary>
        /// <remarks>Doesn't need to be undo save, this is done by the caller.</remarks>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            // no need for try catch, the caller has done this.
            foreach (var i in Frames.ToArray())
            {
                i.NeuronInfo.IsLocked = false;

                    // make certain it is no longer lockec, cause the editor is removed from the project.
            }

            var iDeleter = new NeuronDeleter(deletionMethod, branchHandling);
            iDeleter.Start((from i in Frames select i.Item).ToArray());

                // make a local copy, cause the flows-list will change.
        }

        #endregion

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            var iFound = Item.FindFirstOut((ulong)PredefinedNeurons.Frames) as NeuronCluster;
            if (iFound != null)
            {
                Frames = new FramesCollection(this, iFound);
            }
            else
            {
                Frames = new FramesCollection(this, (ulong)PredefinedNeurons.Frames);
            }

            foreach (var i in Frames)
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
            foreach (var i in Frames)
            {
                i.IsLoaded = false;
            }

            Frames = null;
        }

        #endregion
    }
}