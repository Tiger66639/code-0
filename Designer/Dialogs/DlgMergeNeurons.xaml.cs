// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgMergeNeurons.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   The dlg merge neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>The dlg merge neurons.</summary>
    public partial class DlgMergeNeurons : System.Windows.Window
    {
        /// <summary>Called when the user clicked start.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickStart(object sender, System.Windows.RoutedEventArgs e)
        {
            var iMBRes =
                System.Windows.MessageBox.Show(
                    "Warning: merging a neuron into another one will destroy the original! This is not an undoable operation! Do you want to continue?", 
                    "Merge neurons", 
                    System.Windows.MessageBoxButton.OKCancel, 
                    System.Windows.MessageBoxImage.Warning, 
                    System.Windows.MessageBoxResult.OK);
            if (iMBRes == System.Windows.MessageBoxResult.OK && GetArguments())
            {
                BtnStart.IsEnabled = false;
                System.Action iBuild = InternalRun;
                iBuild.BeginInvoke(null, null);
            }
        }

        /// <summary>Gets the arguments from the UI and checks them.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool GetArguments()
        {
            ulong iFound;
            if (ulong.TryParse(TxtFrom.Text, out iFound))
            {
                if (Brain.Current.TryFindNeuron(iFound, out fFrom) == false)
                {
                    System.Windows.MessageBox.Show(
                        string.Format("Invalid neuron id: {0}, please provide a new 'from' id.", TxtFrom.Text), 
                        "Invalid input", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    if (fFrom.ID < (ulong)PredefinedNeurons.EndOfStatic)
                    {
                        System.Windows.MessageBox.Show(
                            string.Format("Invalid neuron id: {0}, static neurons can't be merged.", TxtFrom.Text), 
                            "Invalid input", 
                            System.Windows.MessageBoxButton.OK, 
                            System.Windows.MessageBoxImage.Error);
                    }
                    else
                    {
                        if (ulong.TryParse(TxtTo.Text, out iFound))
                        {
                            if (Brain.Current.TryFindNeuron(iFound, out fTo) == false)
                            {
                                System.Windows.MessageBox.Show(
                                    string.Format("Invalid neuron id: {0}, please provide a new 'to' id.", TxtFrom.Text), 
                                    "Invalid input", 
                                    System.Windows.MessageBoxButton.OK, 
                                    System.Windows.MessageBoxImage.Error);
                            }
                            else
                            {
                                return true;
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show(
                                "Please provide a valid neuron id in 'to'.", 
                                "Invalid input", 
                                System.Windows.MessageBoxButton.OK, 
                                System.Windows.MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "Please provide a valid neuron id in 'from'.", 
                    "Invalid input", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }

            return false;
        }

        /// <summary>
        ///     The root of the sub thread.
        /// </summary>
        private void InternalRun()
        {
            ChangeFromData();
            if (fFrom.MeaningUsageCount > 0)
            {
                UpdateMeaningRef();
            }

            Brain.Current.Delete(fFrom);
            Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal, 
                new System.Action(ProcessFinished));
        }

        /// <summary>The set cur pos.</summary>
        /// <param name="i">The i.</param>
        private void SetCurPos(ulong i)
        {
            CurrentPos = i;
        }

        /// <summary>
        ///     Called when the sub thread is done.
        /// </summary>
        /// <param name="result">The result.</param>
        private void ProcessFinished()
        {
            BtnStart.IsEnabled = true;
        }

        /// <summary>The change from data.</summary>
        private void ChangeFromData()
        {
            UpdateLinksOut();
            UpdateLinksIn();
            UpdateClusters();
            UpdateChildren();
        }

        /// <summary>The update children.</summary>
        private void UpdateChildren()
        {
            var iFrom = fFrom as NeuronCluster;
            var iTo = fTo as NeuronCluster;
            if (iFrom != null && iTo != null)
            {
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = iFrom.Children) iChildren = iList.ConvertTo<Neuron>();
                try
                {
                    using (var iToList = iTo.ChildrenW) iToList.AddRange(iChildren);
                }
                catch (System.Exception e)
                {
                    Factories.Default.NLists.Recycle(iChildren);
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(AddError), 
                        e.ToString());
                }
            }
        }

        /// <summary>The update clusters.</summary>
        private void UpdateClusters()
        {
            if (fFrom.ClusteredByIdentifier != null)
            {
                System.Collections.Generic.List<NeuronCluster> iClusters;
                using (var iList = fFrom.ClusteredBy) iClusters = iList.ConvertTo<NeuronCluster>();
                foreach (var i in iClusters)
                {
                    var iChildLock = i.ChildrenW;
                    iChildLock.Lock();
                    try
                    {
                        for (var count = 0; count < iChildLock.CountUnsafe; count++)
                        {
                            if (iChildLock.GetUnsafe(count) == fFrom.ID)
                            {
                                try
                                {
                                    iChildLock.SetUnsafe(count, fTo.ID);
                                }
                                catch (System.Exception e)
                                {
                                    Dispatcher.BeginInvoke(
                                        System.Windows.Threading.DispatcherPriority.Send, 
                                        new System.Action<string>(AddError), 
                                        e.ToString());
                                }
                            }
                        }
                    }
                    finally
                    {
                        iChildLock.Dispose(); // also unlocks
                    }
                }

                Factories.Default.CLists.Recycle(iClusters);
            }
        }

        /// <summary>The update links out.</summary>
        private void UpdateLinksOut()
        {
            if (fFrom.LinksOutIdentifier != null)
            {
                Link[] iToModify;
                using (var iLinks = fFrom.LinksOut) iToModify = Enumerable.ToArray(iLinks);
                foreach (var i in iToModify)
                {
                    try
                    {
                        i.From = fTo; // it could be that the link already exists, try to be prepared for that.
                    }
                    catch (System.Exception e)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(AddError), 
                            e.ToString());
                    }
                }
            }
        }

        /// <summary>The update links in.</summary>
        private void UpdateLinksIn()
        {
            if (fFrom.LinksInIdentifier != null)
            {
                Link[] iToModify;
                using (var iLinks = fFrom.LinksIn) iToModify = Enumerable.ToArray(fFrom.LinksIn);

                foreach (var i in iToModify)
                {
                    try
                    {
                        i.To = fTo; // it could be that the link already exists, try to be prepared for that.
                    }
                    catch (System.Exception e)
                    {
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Send, 
                            new System.Action<string>(AddError), 
                            e.ToString());
                    }
                }
            }
        }

        /// <summary>Adds an error to the log (for the async guys).</summary>
        /// <param name="p">The p.</param>
        private void AddError(string p)
        {
            Errors.Add(p);
        }

        /// <summary>The update meaning ref.</summary>
        private void UpdateMeaningRef()
        {
            for (var i = (ulong)PredefinedNeurons.Dynamic; i < Brain.Current.NextID; i++)
            {
                try
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<ulong>(SetCurPos), 
                        i); // we use a high priority so that it will always get updated.
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        // the neuron exists, so check it is ok.
                        UpdateMeaning(iFound);
                    }
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(AddError), 
                        e.ToString());
                }
            }
        }

        /// <summary>The update meaning.</summary>
        /// <param name="toChange">The to change.</param>
        private void UpdateMeaning(Neuron toChange)
        {
            var iCluster = toChange as NeuronCluster;
            if (iCluster != null && iCluster.Meaning == fFrom.ID)
            {
                iCluster.Meaning = fTo.ID;
            }

            Link[] iLinks;
            using (var iList = toChange.LinksOut)
                iLinks = Enumerable.ToArray(iList);

                    // we only check the linksOut cause we are checking every neuron, and all links depart from somewhere, so don't need to do double work.
            foreach (var i in iLinks)
            {
                try
                {
                    if (i.MeaningID == fFrom.ID)
                    {
                        i.Meaning = fTo;
                    }
                }
                catch (System.Exception e)
                {
                    Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.Send, 
                        new System.Action<string>(AddError), 
                        e.ToString());
                }
            }
        }

        #region fields

        /// <summary>The f from.</summary>
        private Neuron fFrom;

        /// <summary>The f to.</summary>
        private Neuron fTo;

        /// <summary>The f errors.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<string> fErrors =
            new System.Collections.ObjectModel.ObservableCollection<string>();

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DlgMergeNeurons"/> class. 
        ///     Initializes a new instance of the <see cref="DlgMergeNeurons"/>
        ///     class.</summary>
        public DlgMergeNeurons()
        {
            InitializeComponent();
            Maximum = Brain.Current.NextID;
        }

        /// <summary>Initializes a new instance of the <see cref="DlgMergeNeurons"/> class. Initializes a new instance of the <see cref="DlgMergeNeurons"/>
        ///     class.</summary>
        /// <param name="from">From.</param>
        public DlgMergeNeurons(Neuron from)
        {
            InitializeComponent();
            fFrom = from;
            TxtFrom.Text = from.ID.ToString();
            Maximum = Brain.Current.NextID;
        }

        #endregion

        #region Prop

        #region CurrentPos

        /// <summary>
        ///     <see cref="CurrentPos" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty CurrentPosProperty =
            System.Windows.DependencyProperty.Register(
                "CurrentPos", 
                typeof(ulong), 
                typeof(DlgMergeNeurons), 
                new System.Windows.FrameworkPropertyMetadata((ulong)0));

        /// <summary>
        ///     Gets or sets the <see cref="CurrentPos" /> property. This dependency
        ///     property indicates the current neuron that is being processed.
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
        ///     <see cref="Maximum" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty MaximumProperty =
            System.Windows.DependencyProperty.Register(
                "Maximum", 
                typeof(double), 
                typeof(DlgMergeNeurons), 
                new System.Windows.FrameworkPropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the <see cref="Maximum" /> property. This dependency
        ///     property indicates the maximum of items that needs to be processed.
        /// </summary>
        public double Maximum
        {
            get
            {
                return (double)GetValue(MaximumProperty);
            }

            set
            {
                SetValue(MaximumProperty, value);
            }
        }

        #endregion

        #region Errors

        /// <summary>
        ///     Gets the list of errors that occured during the merge process.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> Errors
        {
            get
            {
                return fErrors;
            }
        }

        #endregion

        #endregion
    }
}