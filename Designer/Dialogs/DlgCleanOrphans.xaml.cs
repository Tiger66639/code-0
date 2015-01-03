// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgCleanOrphans.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   A type that determins what should be cleaned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     A type that determins what should be cleaned.
    /// </summary>
    public enum CleanType
    {
        /// <summary>
        ///     al neurons that have no related items.
        /// </summary>
        Orphans, 

        /// <summary>
        ///     All clusters that have no children.
        /// </summary>
        EmptyClusters, 

        /// <summary>
        ///     All flow items that don't have parent flows (conditionals and parts)
        /// </summary>
        FlowItems
    }

    /// <summary>
    ///     Interaction logic for DlgCleanOrphans.xaml
    /// </summary>
    public partial class DlgCleanOrphans : System.Windows.Window
    {
        /// <summary>The f orphans.</summary>
        private System.Collections.ObjectModel.ObservableCollection<Orphan> fOrphans =
            new System.Collections.ObjectModel.ObservableCollection<Orphan>();

        /// <summary>The f selected.</summary>
        private System.Collections.Generic.List<Orphan> fSelected;

                                                        // required to reset previous  selection when select all checkbox is indeterminate. 

        /// <summary>The f stop processing.</summary>
        private bool fStopProcessing; // switch used to stop the processing.

        /// <summary>Initializes a new instance of the <see cref="DlgCleanOrphans"/> class.</summary>
        public DlgCleanOrphans()
        {
            InitializeComponent();
            CurrentPos = 0;
            Maximum = Brain.Current.NextID;
        }

        /// <summary>The on delete_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var i = 0;
            while (i < Orphans.Count)
            {
                var iOrphan = Orphans[i];
                if (iOrphan.IsSelected)
                {
                    Brain.Current.Delete(iOrphan.Item.Item);
                    Orphans.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        #region internal types

        /// <summary>The orphan.</summary>
        public class Orphan : Data.ObservableObject
        {
            /// <summary>The f is selected.</summary>
            private bool fIsSelected;

            /// <summary>
            ///     Gets or sets the item, as a debug neuron.
            /// </summary>
            /// <value>The item.</value>
            public DebugNeuron Item { get; set; }

            #region IsSelected

            /// <summary>
            ///     Gets or sets a value indicating whether this instance is selected for deletion or not.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is selected; otherwise, <c>false</c>.
            /// </value>
            public bool IsSelected
            {
                get
                {
                    return fIsSelected;
                }

                set
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }

            #endregion
        }

        #endregion

        #region Prop

        #region CleanType

        /// <summary>
        ///     Gets/sets the which neurons should be presented for cleaning.
        /// </summary>
        public CleanType CleanType { get; set; }

        #endregion

        #region Orhphans

        /// <summary>
        ///     Gets the list of orphans, as debug neurons.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<Orphan> Orphans
        {
            get
            {
                return fOrphans;
            }

            internal set
            {
                fOrphans = value;
            }
        }

        #endregion

        #region CurrentPos

        /// <summary>
        ///     CurrentPos Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CurrentPosProperty =
            System.Windows.DependencyProperty.Register(
                "CurrentPos", 
                typeof(ulong), 
                typeof(DlgCleanOrphans), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the CurrentPos property.  This dependency property
        ///     indicates the id of the current neuron being processed.
        /// </summary>
        public ulong CurrentPos
        {
            get
            {
                return (ulong)GetValue(CurrentPosProperty);
            }

            set
            {
                SetValue(CurrentPosProperty, value);
            }
        }

        #endregion

        #region Maximum

        /// <summary>
        ///     Maximum Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty MaximumProperty =
            System.Windows.DependencyProperty.Register(
                "Maximum", 
                typeof(ulong), 
                typeof(DlgCleanOrphans), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the Maximum property.  This dependency property
        ///     indicates how many neurons need to be processed in total.
        /// </summary>
        public ulong Maximum
        {
            get
            {
                return (ulong)GetValue(MaximumProperty);
            }

            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        #endregion

        #endregion

        #region Process

        /// <summary>The on click start.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void OnClickStart(object sender, System.Windows.RoutedEventArgs e)
        {
            if (BtnStartStop.Tag == null)
            {
                Orphans.Clear();
                CurrentPos = 0;
                BtnStartStop.Content = "Stop";
                BtnStartStop.Tag = this; // we use a pointer, this is faster to check instead of boxing the val
                BtnStartStop.ToolTip = "Stop the process.";
                BtnClose.IsEnabled = false;
                BtnDelete.IsEnabled = false;
                System.Action iThread = ProcessItems;
                System.AsyncCallback iDoWhenDone = ProcessFinished;
                var iAsync = iThread.BeginInvoke(iDoWhenDone, null);
            }
            else
            {
                fStopProcessing = true;
            }
        }

        /// <summary>The process items.</summary>
        private void ProcessItems()
        {
            for (var i = (ulong)PredefinedNeurons.Dynamic; i < Brain.Current.NextID; i++)
            {
                try
                {
                    if (fStopProcessing)
                    {
                        // check if a stop was requested.
                        break;
                    }

                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<ulong>(SetCurPos), 
                        i); // we use a high priority so that it will always get updated.
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        // the neuron exists, so check it is ok.
                        CheckNeuron(iFound);
                    }
                }
                catch (System.Exception e)
                {
                    LogService.Log.LogError(
                        "DlgCleanOrphans.ProcessItems", 
                        string.Format("failed to clean '{0}', error: {1}.", i, e));
                }
            }
        }

        /// <summary>The check neuron.</summary>
        /// <param name="toCheck">The to check.</param>
        private void CheckNeuron(Neuron toCheck)
        {
            if (toCheck.InfoUsageCount == 0)
            {
                switch (CleanType)
                {
                    case CleanType.Orphans:
                        using (ListAccessor<Link> iLinks = toCheck.LinksIn)
                            if (iLinks.Count > 0)
                            {
                                return; // the neuron has links, so not empty
                            }

                        using (ListAccessor<Link> iLinks = toCheck.LinksOut)
                            if (iLinks.Count > 0)
                            {
                                return; // the neuron has links, so not empty
                            }

                        using (ListAccessor<ulong> iClusters = toCheck.ClusteredBy)
                            if (iClusters.Count > 0)
                            {
                                return;
                            }

                        var iCluster = toCheck as NeuronCluster;
                        if (iCluster != null)
                        {
                            using (ListAccessor<ulong> iList = iCluster.Children)
                                if (iList.Count > 0)
                                {
                                    return;
                                }
                        }

                        break;
                    case CleanType.EmptyClusters:
                        iCluster = toCheck as NeuronCluster;
                        if (iCluster != null)
                        {
                            using (ListAccessor<ulong> iList = iCluster.Children)
                                if (iList.Count > 0)
                                {
                                    return;
                                }
                        }
                        else
                        {
                            return;
                        }

                        break;
                    case CleanType.FlowItems:
                        iCluster = toCheck as NeuronCluster;
                        if (iCluster != null && toCheck.ClusteredByIdentifier != null
                            && (iCluster.Meaning == (ulong)PredefinedNeurons.Flow
                                || iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditional
                                || iCluster.Meaning == (ulong)PredefinedNeurons.FlowItemConditionalPart))
                        {
                            using (var iList = toCheck.ClusteredBy)
                            {
                                foreach (var i in iList)
                                {
                                    var iParent = Brain.Current[i] as NeuronCluster;
                                    if (iParent != null
                                        && (iParent.Meaning == (ulong)PredefinedNeurons.Flow
                                            || iParent.Meaning == (ulong)PredefinedNeurons.FlowItemConditional
                                            || iParent.Meaning == (ulong)PredefinedNeurons.FlowItemConditionalPart))
                                    {
                                        return; // the part/cond has a valid flow parent, so don't show as orphan.
                                    }
                                }
                            }
                        }
                        else
                        {
                            return; // not a flow item, so don't show as orphan.
                        }

                        break;
                    default:
                        break;
                }

                var iToAdd = new Orphan();
                iToAdd.Item = new DebugNeuron(toCheck);
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<Orphan>(fOrphans.Add), 
                    iToAdd);
            }
        }

        /// <summary>Sets the current pos (for async calls).</summary>
        /// <param name="id">The id.</param>
        private void SetCurPos(ulong id)
        {
            CurrentPos = id;
        }

        /// <summary>Called when the async <see cref="DlgFixBrokenRefs.ProcessItems"/> is finished.  Updates the buttons and stuff
        ///     to indicate that we are done.</summary>
        /// <param name="result">The result.</param>
        private void ProcessFinished(System.IAsyncResult result)
        {
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action(InternalProcessFinished));
        }

        /// <summary>The internal process finished.</summary>
        private void InternalProcessFinished()
        {
            BtnStartStop.Content = "Start";
            BtnStartStop.ToolTip = "Start the process.";
            BtnClose.IsEnabled = true;
            BtnStartStop.Tag = null; // always let the system know we canallow a new start.
            if (fOrphans.Count > 0)
            {
                BtnDelete.IsEnabled = true;

                // MessageBox.Show("All items processed.");
            }
            else
            {
                System.Windows.MessageBox.Show("All items processed, no orphans found.");
            }
        }

        #endregion

        #region Select

        /// <summary>Handles the Click event of the ChkIsSelected control.</summary>
        /// <remarks>Must make certain that the ChkSelectAll is kept in sync: whenever an item is changed in the selection: we
        ///     set the item to indetermined.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ChkIsSelected_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = null; // whenever we click on an item, the previous selection is undone
            ChkSelectAll.IsChecked = null;
        }

        /// <summary>The chk select al_ unchecked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            fSelected = (from i in fOrphans where i.IsSelected select i).ToList();
            foreach (var i in fOrphans)
            {
                i.IsSelected = false;
            }
        }

        /// <summary>The chk select al_ indeterminate.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Indeterminate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (fSelected != null)
            {
                // this event is triggered whenever an 'IsSelected' checkbox is clicked because the ChkSelectAll is reset.  Only need to reset when the sender is Chk
                foreach (var i in fOrphans)
                {
                    i.IsSelected = false;
                }

                foreach (var i in fSelected)
                {
                    i.IsSelected = true;
                }

                fSelected = null;
            }
        }

        /// <summary>The chk select al_ checked.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void ChkSelectAl_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var i in fOrphans)
            {
                i.IsSelected = true;
            }
        }

        #endregion
    }
}