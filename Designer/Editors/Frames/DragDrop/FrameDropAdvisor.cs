// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameDropAdvisor.cs" company="">
//   
// </copyright>
// <summary>
//   Provides drop functionality on a frame that is found in an itemcontrol
//   full of frames.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Provides drop functionality on a frame that is found in an itemcontrol
    ///     full of frames.
    /// </summary>
    public class FrameDropAdvisor : DnD.DropTargetBase
    {
        /// <summary>The try create new item.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="dropPoint">The drop point.</param>
        private void TryCreateNewItem(System.Windows.DragEventArgs obj, System.Windows.Point dropPoint)
        {
            var iId = (ulong)obj.Data.GetData(Properties.Resources.NeuronIDFormat);
            var iCluster = Brain.Current[iId] as NeuronCluster;
            var iNew = new Frame(iCluster);
            var iDroppedOn = ((System.Windows.FrameworkElement)TargetUI).DataContext as Frame;
            System.Diagnostics.Debug.Assert(iNew != null && iDroppedOn != null);
            var iList = Items;
            System.Diagnostics.Debug.Assert(iList != null);
            iList.Insert(iList.IndexOf(iDroppedOn), iNew);
        }

        #region prop

        #region UsePreviewEvents

        /// <summary>
        ///     Gets if the preview event versions should be used or not.
        /// </summary>
        /// <remarks>
        ///     don't use preview events cause than the sub lists don't get used but
        ///     only the main list cause this gets the events first, while we usually
        ///     want to drop in a sublist.
        /// </remarks>
        public override bool UsePreviewEvents
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list containing all the code that the UI to which advisor is
        ///     attached too, displays data for.
        /// </summary>
        public System.Collections.Generic.IList<Frame> Items
        {
            get
            {
                var iItemsControl = System.Windows.Controls.ItemsControl.ItemsControlFromItemContainer(TargetUI);
                System.Diagnostics.Debug.Assert(iItemsControl != null);
                return iItemsControl.ItemsSource as System.Collections.Generic.IList<Frame>;
            }
        }

        #endregion

        #endregion

        #region Overrides

        /// <summary>The on drop completed.</summary>
        /// <param name="arg">The arg.</param>
        /// <param name="dropPoint">The drop point.</param>
        public override void OnDropCompleted(System.Windows.DragEventArgs arg, System.Windows.Point dropPoint)
        {
            TryCreateNewItem(arg, dropPoint);
        }

        /// <summary>The is valid data object.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool IsValidDataObject(System.Windows.IDataObject obj)
        {
            if (Items != null && obj.GetDataPresent(Properties.Resources.NeuronIDFormat))
            {
                var iId = (ulong)obj.GetData(Properties.Resources.NeuronIDFormat);
                Neuron iFound;
                if (Brain.Current.TryFindNeuron(iId, out iFound))
                {
                    var iCluster = iFound as NeuronCluster;
                    return iCluster != null && iCluster.Meaning == (ulong)PredefinedNeurons.Frame;
                }

                return false;
            }

            return false;
        }

        #endregion
    }
}