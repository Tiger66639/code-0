// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapDropTargetAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Drop advisor for the mindmap panel.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Drop advisor for the mindmap panel.
    /// </summary>
    public class MindMapDropTargetAdvisor : DnD.DropTargetBase
    {
        /// <summary>
        ///     when entering this border, we start scrolling.
        /// </summary>
        private const double SCROLLBORDERSIZE = 10.0;

        /// <summary>The f focused neuron.</summary>
        private MindMapNeuron fFocusedNeuron;

                              // stores a reference to the item that we want to drop on for changing a link.

        /// <summary>The f prev pos.</summary>
        private System.Windows.Point? fPrevPos; // so we know how much we need to move the panel, when on the edge.

        /// <summary>The on drop completed.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
            System.Diagnostics.Debug.Assert(iPanel != null);
            try
            {
                dropPoint.Offset(iPanel.ItemsSource.HorScrollPos, iPanel.ItemsSource.VerScrollPos);

                    // we need to adjust for the scrollpos.
                var iItem = obj.Data.GetData(Properties.Resources.MindMapItemFormat) as MindMapItem;
                if (iItem == null)
                {
                    // we are not moving around an item, so check if there is a neuron for which we can creat a new mindmapneuron.
                    var iList =
                        obj.Data.GetData(Properties.Resources.MultiMindMapItemFormat) as
                        System.Collections.Generic.List<PositionedMindMapItem>;
                    if (iList == null)
                    {
                        TryCreateNewNeuron(obj.Data, dropPoint);
                    }
                    else if ((obj.Effects & System.Windows.DragDropEffects.Copy) == System.Windows.DragDropEffects.Copy)
                    {
                        TryDuplicateItems(obj.Data, dropPoint, iList);
                    }
                    else
                    {
                        TryMoveItems(obj.Data, dropPoint, iList);
                    }
                }
                else if ((obj.Effects & System.Windows.DragDropEffects.Copy) == System.Windows.DragDropEffects.Copy)
                {
                    TryDuplicateItem(obj.Data, dropPoint, iItem);
                }
                else
                {
                    TryMoveItem(obj.Data, dropPoint, iItem, null);
                }
            }
            finally
            {
                iPanel.ActivateAdorners();
            }
        }

        /// <summary>Tries to duplicate the items.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="list">The list.</param>
        private void TryDuplicateItems(
            System.Windows.IDataObject obj, 
            System.Windows.Point dropPoint, System.Collections.Generic.List<PositionedMindMapItem> list)
        {
            System.Windows.Point iNewPoint;
            var iFirst = new System.Windows.Point(list[0].X, list[0].Y);

                // we have to store the pos before the item is moved, otherwise we work with wrong values.
            foreach (var i in list)
            {
                var iNeuron = i as MindMapNeuron;
                if (iNeuron != null)
                {
                    var iNew = iNeuron.Item.Duplicate();
                    iNewPoint = new System.Windows.Point(dropPoint.X + (i.X - iFirst.X), dropPoint.Y + (i.Y - iFirst.Y));
                    CreateNew(iNew.ID, iNewPoint);
                }
                else if (i is MindMapNote)
                {
                    var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
                    var iNew = ((MindMapNote)i).Duplicate() as PositionedMindMapItem;
                    iNewPoint = new System.Windows.Point(dropPoint.X + (i.X - iFirst.X), dropPoint.Y + (i.Y - iFirst.Y));
                    iNew.SetPositionFromDrag(
                        iNewPoint.X, 
                        iNewPoint.Y, 
                        new System.Collections.Generic.List<MindMapNeuron>()); // a note can't be or have children.
                    iPanel.ItemsSource.Items.Add(iNew);
                }
                else
                {
                    throw new System.InvalidOperationException("Invalid mindmap item: can't duplicate.");
                }
            }
        }

        /// <summary>Tries to duplicate the item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="item">The item.</param>
        private void TryDuplicateItem(System.Windows.IDataObject obj, System.Windows.Point dropPoint, MindMapItem item)
        {
            var iNeuron = item as MindMapNeuron;
            if (iNeuron != null)
            {
                var iNew = iNeuron.Item.Duplicate();
                CreateNew(iNew.ID, dropPoint);
            }
            else if (item is MindMapNote)
            {
                var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
                var iNew = ((MindMapNote)item).Duplicate();
                iPanel.ItemsSource.Items.Add(iNew);
            }
            else
            {
                throw new System.InvalidOperationException("Can't duplicate links");
            }
        }

        /// <summary>Tries the move all the selected items. If the items
        ///     are not part of the mind map (from another one), they are first duplicated.
        ///     Please note: you can't add duplicate neurons.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        /// <param name="list">The list.</param>
        private void TryMoveItems(
            System.Windows.IDataObject obj, 
            System.Windows.Point dropPoint, System.Collections.Generic.List<PositionedMindMapItem> list)
        {
            try
            {
                System.Windows.Point iNewPoint;
                var iFirst = new System.Windows.Point(list[0].X, list[0].Y);

                    // we have to store the pos before the item is moved, otherwise we work with wrong values.
                var iAlreadyMoved = new System.Collections.Generic.List<MindMapNeuron>();

                    // we need to keep track of the items that have already been moved, so that we don't try 2 times.
                foreach (var i in list)
                {
                    var iNeuron = i as MindMapNeuron;
                    var iNeedMove = true;
                    if (iNeuron != null)
                    {
                        iNeedMove = !iAlreadyMoved.Contains(iNeuron);
                    }

                    if (iNeedMove)
                    {
                        iAlreadyMoved.Add(iNeuron);
                        iNewPoint = new System.Windows.Point(
                            dropPoint.X + (i.X - iFirst.X), 
                            dropPoint.Y + (i.Y - iFirst.Y));
                        TryMoveItem(obj, iNewPoint, i, iAlreadyMoved);
                    }
                }
            }
            catch (System.Exception e)
            {
                var iMsg = e.ToString();
                LogService.Log.LogError("MindMapDropTargetAdvisor.TryMoveItems", iMsg);
                System.Windows.MessageBox.Show(
                    "drag drop failed", 
                    string.Format("drag drop operation can't be completed because: {0}.", iMsg), 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Tries to move the specified item around on the mind map. If the item
        ///     is not part of the mind map (from another one), it is first duplicated.
        ///     Please note: you can't add duplicate neurons.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint"></param>
        /// <param name="item">The item.</param>
        /// <param name="alreadyMoved">The already Moved.</param>
        private void TryMoveItem(
            System.Windows.IDataObject obj, 
            System.Windows.Point dropPoint, 
            MindMapItem item, System.Collections.Generic.List<MindMapNeuron> alreadyMoved)
        {
            try
            {
                var iMap = ((System.Windows.FrameworkElement)TargetUI).DataContext as MindMap;
                System.Diagnostics.Debug.Assert(iMap != null);
                if (iMap.Items.Contains(item) == false)
                {
                    // we are not moving an item on the same canvas, so make a duplicate.
                    item = item.Duplicate();
                    iMap.Items.Add(item);
                }

                var iPosItem = (PositionedMindMapItem)item;
                if (iPosItem != null)
                {
                    // its a note or neuron, so adjust it's pos.
                    if (alreadyMoved == null)
                    {
                        alreadyMoved = new System.Collections.Generic.List<MindMapNeuron>();
                        if (iPosItem is MindMapNeuron)
                        {
                            alreadyMoved.Add((MindMapNeuron)iPosItem);
                        }
                    }

                    iPosItem.SetPositionFromDrag(dropPoint.X, dropPoint.Y, alreadyMoved);
                }
                else
                {
                    var iLink = (MindMapLink)item;
                    if (iLink != null)
                    {
                        if (fFocusedNeuron != null)
                        {
                            var iSide = obj.GetData(Properties.Resources.MindMapLinkSide) as string;
                            if (iSide != null)
                            {
                                if (iSide == "start")
                                {
                                    iLink.FromMindMapItem = fFocusedNeuron;
                                }
                                else
                                {
                                    iLink.ToMindMapItem = fFocusedNeuron;
                                }
                            }
                            else
                            {
                                System.Windows.MessageBox.Show("Invalid side of link stored in drag operation!");
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("No item focused to connect the link with!");
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Unrecognized mindmap item!");
                    }
                }
            }
            catch (System.Exception e)
            {
                var iMsg = e.ToString();
                LogService.Log.LogError("MindMapDropTargetAdvisor.TryMoveItem", iMsg);
                System.Windows.MessageBox.Show(
                    "drag drop failed", 
                    string.Format("drag drop operation can't be completed because: {0}.", iMsg), 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>Tries to create a new mindmap neuron based on the neuron found in the data.</summary>
        /// <param name="obj"></param>
        /// <param name="dropPoint"></param>
        private void TryCreateNewNeuron(System.Windows.IDataObject obj, System.Windows.Point dropPoint)
        {
            if (obj.GetDataPresent(Properties.Resources.MultiNeuronIDFormat))
            {
                var iIds = (System.Collections.Generic.List<ulong>)obj.GetData(Properties.Resources.MultiNeuronIDFormat);
                foreach (var i in iIds)
                {
                    CreateNew(i, dropPoint);
                    dropPoint.Offset(5, 5); // move a bit down, right, so that it's clear multiple items were dropped.
                }
            }
            else if (obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                CreateNew(iId, dropPoint);
            }
        }

        /// <summary>The create new.</summary>
        /// <param name="iId">The i id.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void CreateNew(ulong iId, System.Windows.Point dropPoint)
        {
            var iMap = ((System.Windows.FrameworkElement)TargetUI).DataContext as MindMap;
            System.Diagnostics.Debug.Assert(iMap != null);
            var iNew = MindMapNeuron.CreateFor(Brain.Current[iId]);
            iNew.X = dropPoint.X;
            iNew.Y = dropPoint.Y;
            iNew.Width = 100;
            iNew.Height = 50;
            iMap.Items.Add(iNew);
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            return obj.GetDataPresent(Properties.Resources.MindMapItemFormat)
                   || obj.GetDataPresent(Properties.Resources.MultiMindMapItemFormat)
                   || obj.GetDataPresent(Properties.Resources.NeuronIDFormat)
                   || obj.GetDataPresent(Properties.Resources.MultiNeuronIDFormat);
        }

        /// <summary>Called whenever an item is dragged into the drop target. By default, doesn't do anything. Allows
        ///     descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public override void OnDragEnter(System.Windows.DragEventArgs e)
        {
            var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
            System.Diagnostics.Debug.Assert(iPanel != null);
            iPanel.DeactivateAdorners();
        }

        /// <summary>Called whenever an item is dragged out of the drop target. By default, doesn't do anything. Allows
        ///     descendents to do some custom actions.</summary>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        public override void OnDragLeave(System.Windows.DragEventArgs e)
        {
            var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
            System.Diagnostics.Debug.Assert(iPanel != null);
            iPanel.DeactivateAdorners();
            fPrevPos = null;
        }

        /// <summary>Called whenever an item is being moved over the drop target.  By default doesn't do anything.</summary>
        /// <param name="position"></param>
        /// <param name="data">The data being dragged.</param>
        /// <remarks>Can be used to adjust the position (for snapping) or draw extra placement information.</remarks>
        public override void OnDragOver(ref System.Windows.Point position, System.Windows.IDataObject data)
        {
            var iPanel = (WPF.Controls.MindMapPanel)TargetUI;
            System.Diagnostics.Debug.Assert(iPanel != null);
            if (fPrevPos.HasValue)
            {
                var iVal = iPanel.ActualWidth;
                if (position.X + SCROLLBORDERSIZE > iVal)
                {
                    iPanel.ItemsSource.HorScrollPos += (position.X - fPrevPos.Value.X)
                                                       + (position.X + SCROLLBORDERSIZE - iVal);
                }
                else if (position.X <= SCROLLBORDERSIZE)
                {
                    iPanel.ItemsSource.HorScrollPos -= SCROLLBORDERSIZE - position.X;
                }

                iVal = iPanel.ActualHeight;
                if (position.Y + SCROLLBORDERSIZE > iVal)
                {
                    iPanel.ItemsSource.VerScrollPos += (position.Y - fPrevPos.Value.Y)
                                                       + (position.Y + SCROLLBORDERSIZE - iVal);
                }
                else if (position.Y <= SCROLLBORDERSIZE)
                {
                    iPanel.ItemsSource.VerScrollPos -= SCROLLBORDERSIZE - position.Y;
                }
            }

            fPrevPos = position;
        }
    }
}