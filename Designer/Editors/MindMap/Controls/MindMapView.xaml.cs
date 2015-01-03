// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   A view for a <see cref="MindMap" /> object. The object is expected to be
//   passed as data context.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     A view for a <see cref="MindMap" /> object. The object is expected to be
    ///     passed as data context.
    /// </summary>
    /// <remarks>
    ///     Only supports multi selection for neurons not links.
    /// </remarks>
    public partial class MindMapView : System.Windows.Controls.UserControl
    {
        #region Fields

        /// <summary>The f mouse pos.</summary>
        private System.Windows.Point fMousePos;

        // SelectionAdorner fSelectionBox;
        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="MindMapView"/> class.</summary>
        public MindMapView()
        {
            InitializeComponent();
        }

        #endregion

        /// <summary>Handles the MouseLeave event of the Slider control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Slider_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ToggleZoom.IsChecked = false;
        }

        #region Commands

        /// <summary>
        ///     Command to add the selected item(s) to a cluster
        /// </summary>
        public static System.Windows.Input.RoutedCommand AddToClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to remove the selected item(s) from a cluster it belongs to.
        /// </summary>
        public static System.Windows.Input.RoutedCommand RemoveFromClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command creates a new cluster and adds all the selected items to it.
        /// </summary>
        public static System.Windows.Input.RoutedCommand MakeClusterCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to select all the visible links for a neuron on a mindmap
        ///     view.
        /// </summary>
        public static System.Windows.Input.RoutedCommand SelectLinksCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to show all the incomming links
        /// </summary>
        public static System.Windows.Input.RoutedCommand ShowIncommingLinksCmd =
            new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to show all the outgoing links.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ShowOutgoingLinksCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to show all the children of a cluster.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ShowChildrenCmd = new System.Windows.Input.RoutedCommand();

        /// <summary>
        ///     Command to show all the clusters a neuron belongs to.
        /// </summary>
        public static System.Windows.Input.RoutedCommand ShowClustersCmd = new System.Windows.Input.RoutedCommand();

        #endregion

        #region CanvasHeight

        /// <summary>
        ///     <see cref="CanvasHeight" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CanvasHeightProperty =
            System.Windows.DependencyProperty.Register(
                "CanvasHeight", 
                typeof(double), 
                typeof(MindMapView), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the <see cref="CanvasHeight" /> property. This dependency
        ///     property indicates the height that should be used by the canvas.
        /// </summary>
        public double CanvasHeight
        {
            get
            {
                return (double)GetValue(CanvasHeightProperty);
            }

            set
            {
                SetValue(CanvasHeightProperty, value);
            }
        }

        #endregion

        #region Event handlers

        #region Context menu

        /// <summary>Used by all objects that have a context menu, to store the last mouse
        ///     position.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AllContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            fMousePos = System.Windows.Input.Mouse.GetPosition(this);
        }

        /// <summary>Handles the Click event of the MnuNewNote control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewNote_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iMap = (MindMap)DataContext;

                var iNote = new MindMapNote();
                AddNewItem(iNote, 120, 120);
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New note", iMsg);
            }
        }

        /// <summary>Handles the Click event of the MnuNewObject menu item.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewObject_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    var iObject = EditorsHelper.MakeObject();
                    if (iObject != null)
                    {
                        AddNewObject(iObject);
                    }
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New Object", iMsg);
            }
        }

        /// <summary>Adds a new <paramref name="cluster"/> as an object (showing all it's
        ///     children).</summary>
        /// <param name="cluster">The cluster to show.</param>
        private void AddNewObject(MindMapCluster cluster)
        {
            var fPrevAutoAddVal = MindMapCluster.AutoAddItemsToMindMapCluter;

                // we use a small trick to make certain that all items are shown when the MMcluster is created.
            MindMapCluster.AutoAddItemsToMindMapCluter = true;
            try
            {
                var iMap = (MindMap)DataContext;
                AddNewItem(cluster, 120, 120);
            }
            finally
            {
                MindMapCluster.AutoAddItemsToMindMapCluter = fPrevAutoAddVal;
            }
        }

        /// <summary>Handles the Click event of the MnuNewNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewNeuron_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iNeuron = NeuronFactory.GetNeuron();
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    var iNew = MindMapNeuron.CreateFor(iNeuron);
                    AddNewItem(iNew, 90, 50);
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New neuron", iMsg);
            }
        }

        /// <summary>Handles the Click event of the MnuNewTextNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewTextNeuron_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iNeuron = NeuronFactory.Get<TextNeuron>();
                iNeuron.Text = "new neuron";
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    var iNew = MindMapNeuron.CreateFor(iNeuron);
                    AddNewItem(iNew, 90, 50);
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled.
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New text neuron", iMsg);
            }
        }

        /// <summary>Handles the Click event of the MnuNewCluster control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewCluster_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iNeuron = NeuronFactory.GetCluster();
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    var iNew = MindMapNeuron.CreateFor(iNeuron);
                    AddNewItem(iNew, 90, 50);
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled. -> 
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New text neuron", iMsg);
            }
        }

        /// <summary>Handles the Click event of the MnuNewIntNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewIntNeuron_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iNeuron = NeuronFactory.GetInt();
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    var iNew = MindMapNeuron.CreateFor(iNeuron);
                    AddNewItem(iNew, 90, 50);
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled. -> 
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New IntNeuron", iMsg);
            }
        }

        /// <summary>Handles the Click event of the MnuNewDoubleNeuron control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MnuNewDoubleNeuron_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iNeuron = NeuronFactory.GetDouble();
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we begin a group because this action will also remove code items, mindmapitems, ....  to create them correctly (at the correct pos,.., we need to store them as well.
                try
                {
                    WindowMain.AddItemToBrain(iNeuron); // we use this function cause it takes care of the undo data.
                    var iNew = MindMapNeuron.CreateFor(iNeuron);
                    AddNewItem(iNew, 90, 50);
                }
                finally
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Normal, 
                        new System.Action(WindowMain.UndoStore.EndUndoGroup));

                        // we call async cause this action triggers some events in the brain which are handled async with the dispatcher, we need to close the undo group after these have been handled. -> 
                }
            }
            catch (System.Exception ex)
            {
                var iMsg = ex.ToString();
                System.Windows.MessageBox.Show(iMsg);
                LogService.Log.LogError("New DoubleNeuron", iMsg);
            }
        }

        /// <summary>Adds a new item.</summary>
        /// <param name="toAdd">To add.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        private void AddNewItem(PositionedMindMapItem toAdd, double width, double height)
        {
            var iMap = (MindMap)DataContext;

            toAdd.X = fMousePos.X + iMap.HorScrollPos;
            toAdd.Y = fMousePos.Y + iMap.VerScrollPos;
            toAdd.Width = width;
            toAdd.Height = height;
            iMap.Items.Add(toAdd);
        }

        /// <summary>The zoom clicked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ZoomClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = e.OriginalSource as System.Windows.Controls.MenuItem;
            if (iSender != null && iSender.Tag is double)
            {
                var iMap = (MindMap)DataContext;
                iMap.ZoomProcent = (double)iSender.Tag;
            }
        }

        #region Commands

        #region Delete

        /// <summary>Handles the CanExecute event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            e.CanExecute = iMap.SelectedItems.Count > 0;
        }

        /// <summary>Handles the Executed event of the Delete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void Delete_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false, true);
            try
            {
                var iMap = (MindMap)DataContext;
                var iToDelete = Enumerable.ToList(iMap.SelectedItems);

                    // need to make a copy of the list, otherwise we get an error of trying to modify a list in a loop.
                iMap.SelectedItems.Clear();

                foreach (var i in iToDelete)
                {
                    if (iMap.Items.Contains(i))
                    {
                        // it could already have been removed by a previous remove.
                        if (i is MindMapLink)
                        {
                            // if link, ask to remove the link from the ui or delete it from the brain.
                            DeleteLink((MindMapLink)i);
                        }
                        else if (i is MindMapNeuron)
                        {
                            DeleteNeuron(iMap, (MindMapNeuron)i);
                        }
                        else if (i is MindMapNote)
                        {
                            iMap.Items.Remove(i);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(string.Format("Unknown mindmap item: {0}, can't delete!", i));
                        }
                    }
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        /// <summary>Deletes the neuron.</summary>
        /// <param name="map">The map.</param>
        /// <param name="neuron">The neuron.</param>
        private void DeleteNeuron(MindMap map, MindMapNeuron neuron)
        {
            var iId = neuron.ItemID;
            var iPermDelete =
                System.Windows.MessageBox.Show(
                    string.Format(
                        "Permenantly delete the neuron '{0}' from the brain or remove it from the UI (yes to permanently delete it)?", 
                        BrainData.Current.NeuronInfo[iId]), 
                    "Delete neuron", 
                    System.Windows.MessageBoxButton.YesNoCancel, 
                    System.Windows.MessageBoxImage.Asterisk, 
                    System.Windows.MessageBoxResult.Yes);
            if (iPermDelete != System.Windows.MessageBoxResult.Cancel)
            {
                WindowMain.UndoStore.BeginUndoGroup(false);

                    // we group all the data together so a single undo command cand restore the previous state. Also need to reverse order due to remove/delete - create/add operation
                try
                {
                    switch (iPermDelete)
                    {
                        case System.Windows.MessageBoxResult.No:

                            // it's a neuron, all the visible links that reference it, must also be removed from the ui.
                            var iAlsoToRemove = (from i in map.Items
                                                 where
                                                     i is MindMapLink
                                                     && (((MindMapLink)i).From == iId || ((MindMapLink)i).To == iId)
                                                 select (MindMapLink)i).ToList();
                            foreach (var i in iAlsoToRemove)
                            {
                                map.Items.Remove(i);
                            }

                            map.Items.Remove(neuron);
                            break;
                        case System.Windows.MessageBoxResult.Yes:
                            if (iId >= (ulong)PredefinedNeurons.Dynamic)
                            {
                                // we can only delete dynmic items, others can't.
                                map.Items.Remove(neuron);

                                    // need to do a remove before the delete, so that the action can be reversed correctly.
                                WindowMain.DeleteItemFromBrain(neuron.Item);

                                    // don't need to remove any links seperatly, links are destroyed by the AI engine, which triggers the deletion of the mindmap links.
                            }
                            else
                            {
                                System.Windows.MessageBox.Show(
                                    string.Format(
                                        "'{0}' is a static neuron which can't be deleted", 
                                        neuron.NeuronInfo.DisplayTitle), 
                                    "Delete neuron", 
                                    System.Windows.MessageBoxButton.OK);
                            }

                            break;
                        default:
                            LogService.Log.LogError("MindMapView.DeleteNeuron", "internal error: invalid mbox result");
                            break;
                    }
                }
                finally
                {
                    WindowMain.UndoStore.EndUndoGroup();
                }
            }
        }

        /// <summary>The delete link.</summary>
        /// <param name="link">The link.</param>
        private void DeleteLink(MindMapLink link)
        {
            var iPermDelete =
                System.Windows.MessageBox.Show(
                    "Permenantly delete the link from the brain or remove it from the UI (yes to permanently delete it)?", 
                    "Delete link", 
                    System.Windows.MessageBoxButton.YesNoCancel, 
                    System.Windows.MessageBoxImage.Asterisk, 
                    System.Windows.MessageBoxResult.Yes);
            switch (iPermDelete)
            {
                case System.Windows.MessageBoxResult.Cancel:
                    return;
                case System.Windows.MessageBoxResult.No:
                    link.Remove();
                    break;
                case System.Windows.MessageBoxResult.Yes:
                    link.Destroy();
                    break;
                default:
                    LogService.Log.LogError("MindMapView.DeleteLink", "internal error: invalid mbox result");
                    break;
            }
        }

        #endregion

        #region Order

        /// <summary>The move up_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;

            if (iMap.SelectedItems.Count > 0)
            {
                var iFound =
                    (from MindMapItem i in iMap.SelectedItems where i.ZIndex == iMap.Items.Count - 1 select i)
                        .FirstOrDefault();
                e.CanExecute = iFound == null; // we can alsways move up if none of the selected items is on the top.
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The move down_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;

            if (iMap.SelectedItems.Count > 0)
            {
                var iFound = (from MindMapItem i in iMap.SelectedItems where i.ZIndex == 0 select i).FirstOrDefault();
                e.CanExecute = iFound == null; // we can alsways move up if none of the selected items is on the top.
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>The move up_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveUp_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            var iToMove = (from object i in iMap.SelectedItems select (MindMapItem)i).ToList();

                // need to make a copy of the list so we can modify it.
            iMap.MoveUp(iToMove);
        }

        /// <summary>The move down_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveDown_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            var iToMove = (from object i in iMap.SelectedItems select (MindMapItem)i).ToList();

                // need to make a copy of the list so we can modify it.
            iMap.MoveDown(iToMove);
        }

        /// <summary>The move to end_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveToEnd_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            iMap.MoveToBack((from MindMapItem i in iMap.SelectedItems select i).ToList());

                // need to convert to MindMapItems
        }

        /// <summary>The move to home_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MoveToHome_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            iMap.MoveToFront((from MindMapItem i in iMap.SelectedItems select i).ToList());
        }

        #endregion

        #region Links

        /// <summary>The show outgoing links_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ShowOutgoingLinks_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = DataContext as MindMap;
            if (iMap != null && iMap.SelectedItems.Count > 0)
            {
                iMap.ShowLinksOut(iMap.SelectedItems[0] as MindMapNeuron);
            }
        }

        /// <summary>The show incomming links_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ShowIncommingLinks_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = DataContext as MindMap;
            if (iMap != null && iMap.SelectedItems.Count > 0)
            {
                iMap.ShowLinksIn(iMap.SelectedItems[0] as MindMapNeuron);
            }
        }

        #region Select links

        /// <summary>The select links_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SelectLinks_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = DataContext as MindMap;
            var iNew = new DlgSelectLinks(iMap.SelectedItems[0] as MindMapNeuron);
            iNew.Owner = System.Windows.Window.GetWindow(this);
            iNew.ShowDialog();
        }

        /// <summary>The select links_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SelectLinks_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = DataContext as MindMap;
            e.CanExecute = iMap.SelectedItems.Count == 1 && iMap.SelectedItems[0] is MindMapNeuron;
        }

        #endregion

        /// <summary>Shows the link dialog box, while trying to fill in the default values
        ///     using the following scheme: selection[0] -&gt; from selection[1] -&gt; to
        ///     selection[2] -&gt; meaning.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void MnuLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var iMap = (MindMap)DataContext;
                var iDlg = new DlgLink();
                iDlg.Owner = System.Windows.Window.GetWindow(this);
                iDlg.ToList =
                    (from i in iMap.Items
                     where i is MindMapNeuron
                     orderby ((MindMapNeuron)i).NeuronInfo.DisplayTitle
                     select (MindMapNeuron)i).ToList();

                    // extract all the neurons because we can make connections between those.
                iDlg.FromList = iDlg.ToList; // from list and to list is the same.
                if (iMap.SelectedItems.Count == 1)
                {
                    iDlg.SelectedFrom = iMap.SelectedItems[0] as MindMapNeuron;
                }
                else if (iMap.SelectedItems.Count >= 2)
                {
                    iDlg.SelectedFrom = iMap.SelectedItems[1] as MindMapNeuron;
                    iDlg.SelectedTo = iMap.SelectedItems[0] as MindMapNeuron;
                }

                var iRes = iDlg.ShowDialog();
                if (iRes.HasValue && iRes.Value)
                {
                    var iTo = iDlg.SelectedTo; // guaranteed to be valid if dialog closed with ok.
                    var iFrom = iDlg.SelectedFrom;
                    var iMeaning = iDlg.SelectedMeaning;
                    WindowMain.UndoStore.BeginUndoGroup(false);

                        // we reverse order cause this action generates 2 undo data items: a collection add / a link create.  When reversed we get collection remove / remove link.
                    try
                    {
                        var iLink = new Link(iTo.Item, iFrom.Item, iMeaning);
                        var iNew = new MindMapLink();
                        iNew.CreateLink(iLink, iFrom, iTo);
                        var iUndoData = new LinkUndoItem(iLink, BrainAction.Created);
                        WindowMain.UndoStore.AddCustomUndoItem(iUndoData);
                        iMap.Items.Add(iNew);

                            // very important that we do an add at the end, cause we request the list to be performed in reverse order.
                    }
                    finally
                    {
                        WindowMain.UndoStore.EndUndoGroup();
                    }
                }
            }
            catch (System.Exception iEx)
            {
                var iMsg = iEx.ToString();
                System.Windows.MessageBox.Show(
                    "failed to create the link:\n" + iMsg, 
                    "Link", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        #endregion

        #region ShowChildren

        /// <summary>Handles the CanExecute event of the ShowChildren command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ShowChildren_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            e.CanExecute = iMap.SelectedItems.Count > 0 && iMap.SelectedItems[0] is MindMapCluster;
        }

        /// <summary>Handles the Executed event of the ShowChildren command.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ShowChildren_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                var iMap = DataContext as MindMap;
                if (iMap != null && iMap.SelectedItems.Count > 0)
                {
                    iMap.ShowChildren(iMap.SelectedItems[0] as MindMapCluster);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #region ShowClusters

        /// <summary>Handles the Executed event of the ShowClusters control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ShowClusters_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            WindowMain.UndoStore.BeginUndoGroup(false);
            try
            {
                var iMap = DataContext as MindMap;
                if (iMap != null && iMap.SelectedItems.Count > 0)
                {
                    iMap.ShowOwners(iMap.SelectedItems[0] as MindMapNeuron);
                }
            }
            finally
            {
                WindowMain.UndoStore.EndUndoGroup();
            }
        }

        #endregion

        #endregion

        #endregion

        #region Add To cluster

        /// <summary>The add to cluster_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AddToCluster_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = DataContext as MindMap;
            e.CanExecute = iMap.SelectedItems.Count >= 1 && iMap.SelectedItems[0] is MindMapNeuron;
        }

        /// <summary>Handles the Executed event of the AddToCluster control.</summary>
        /// <remarks><para>Shows a dialog box from which the user can select all the clusters to
        ///         which the first selected item should be added. If there are more than
        ///         1 items selected, all the others are used as default values for the
        ///         clusters (if possible). The List of clusters displayed is limited to
        ///         the clusters that are visible on the mindmap.</para>
        /// <para>When a circular reference is detected, a warning is displayed.</para>
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void AddToCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;

            var iClusters = from i in iMap.Items
                            where i is MindMapCluster

                            // build the data for the dialog
                            select ((MindMapCluster)i).Item;

            var iCluster = iMap.SelectedItem as MindMapCluster;
            var iNew = new DlgSelectNeurons(iClusters, iCluster != null ? iCluster.Item : null);
            iNew.ItemTemplate = FindResource("SelectableClusterForMindMaps") as System.Windows.DataTemplate;
            iNew.Owner = System.Windows.Window.GetWindow(this);

            var iRes = iNew.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                // process all the selected data from the dialog if the user selected 'ok'.
                var iItemsToAdd =
                    (from i in iMap.SelectedItems where i is MindMapNeuron select ((MindMapNeuron)i).Item).Except(
                        iNew.SelectedValues);
                foreach (var i in iItemsToAdd)
                {
                    foreach (NeuronCluster iSelected in iNew.SelectedValues)
                    {
                        using (var iList = iSelected.ChildrenW)
                            iList.Add(i);

                                // when we get here, we want to add the item as a child. This call will also take care of deticting circular references.
                    }
                }
            }
        }

        #endregion

        #region RemoveFrom cluster

        /// <summary>The remove from cluster_ can execute.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RemoveFromCluster_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            e.CanExecute = iMap.SelectedItems.Count >= 1 && iMap.SelectedItems[0] is MindMapNeuron;
        }

        /// <summary>The remove from cluster_ executed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void RemoveFromCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            var iFirstSel = (MindMapNeuron)iMap.SelectedItems[0];

                // selectedItem is garanteed MindMapNeuron because of CanExecute check.
            var iClusters = from i in iMap.Items

                            // build the data for the dialog
                            where i is MindMapCluster && ((MindMapCluster)i).Children.Contains(iFirstSel)
                            select ((MindMapCluster)i).Item;
            var iDefault = from object i in iMap.SelectedItems
                           where i != iMap.SelectedItems[0] && i is MindMapCluster
                           select ((MindMapCluster)i).Item;
            var iNew = new DlgSelectNeurons(iClusters, iDefault);
            iNew.ItemTemplate = FindResource("SelectableClusterForMindMaps") as System.Windows.DataTemplate;
            iNew.Owner = System.Windows.Window.GetWindow(this);

            var iRes = iNew.ShowDialog();
            if (iRes.HasValue && iRes.Value)
            {
                // process all the selected data from the dialog if the user selected 'ok'.
                var iToRemove = iFirstSel.Item;
                foreach (NeuronCluster i in iNew.SelectedValues)
                {
                    using (var iList = i.ChildrenW) iList.Remove(iToRemove); // This will also remove any circular ref indicator
                }
            }
        }

        #endregion

        #region Make cluster

        /// <summary>Handles the CanExecute event of the MakeCluster control.</summary>
        /// <remarks>needs to make certain that each selected item is a Neuron.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.CanExecuteRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void MakeCluster_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            if (iMap.SelectedItems.Count >= 1)
            {
                var iFound = (from object i in iMap.SelectedItems where !(i is MindMapNeuron) select i).FirstOrDefault();
                e.CanExecute = iFound == null;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        /// <summary>Handles the Executed event of the MakeCluster control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.ExecutedRoutedEventArgs"/> instance containing the
        ///     event data.</param>
        private void MakeCluster_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            var iMap = (MindMap)DataContext;
            var iCluster = DlgCreateCluster.CreateCluster("New cluster", System.Windows.Window.GetWindow(this));
            if (iCluster != null)
            {
                using (var iList = iCluster.ChildrenW)
                {
                    foreach (MindMapNeuron i in iMap.SelectedItems)
                    {
                        iList.Add(i.ItemID);
                    }
                }

                var iNew = new MindMapCluster(iCluster);

                var iQuery = from MindMapNeuron i in iMap.SelectedItems select i;
                iNew.X = System.Math.Max(iQuery.Min(w => w.X) - 4, 0);

                    // we need to set the size of the cluster to the outer limits of the neurons we encapsulate.
                iNew.Width = iQuery.Max(w => w.X + w.Width) - iNew.X + 4;
                iNew.Y = System.Math.Max(iQuery.Min(w => w.Y) - 4, 0);
                iNew.Height = iQuery.Max(w => w.Y + w.Height) - iNew.Y + 4;
                iMap.Items.Add(iNew);

                // iNew.LoadChildren();
            }
        }

        #endregion

        /// <summary>Handles the TiltWheel event on the MMPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void MMPanel_TiltWheel(object sender, WPF.Controls.MouseTiltEventArgs e)
        {
            ScrollHor.Value += e.Tilt;
        }

        #endregion
    }
}