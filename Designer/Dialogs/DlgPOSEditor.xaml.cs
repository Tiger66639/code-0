// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgPOSEditor.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Displays a dialog that allows the user to edit the part of speech value
//   assigned to all the object clusters in the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Displays a dialog that allows the user to edit the part of speech value
    ///     assigned to all the object clusters in the brain.
    /// </summary>
    /// <remarks>
    ///     When this dialog is opened, a snap shot of the brain is taken.
    /// </remarks>
    public partial class DlgPOSEditor : System.Windows.Window
    {
        #region Fields

        /// <summary>The f prev cursor.</summary>
        private readonly System.Windows.Input.Cursor fPrevCursor;

        #endregion

        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="DlgPOSEditor" /> class.
        /// </summary>
        public DlgPOSEditor()
        {
            InitializeComponent();
            fPrevCursor = System.Windows.Input.Mouse.OverrideCursor;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            System.Action iLoadData = LoadData;
            iLoadData.BeginInvoke(null, null);
            GlobalCommands.Register(this);
        }

        #endregion

        /// <summary>Handles the Sorting event of the Data grid control. This provides
        ///     faster sorting.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Windows.Controls.DataGridSortingEventArgs"/>
        ///     instance containing the event data.</param>
        private void DG_Sorting(object sender, System.Windows.Controls.DataGridSortingEventArgs e)
        {
            var iDir = (e.Column.SortDirection != System.ComponentModel.ListSortDirection.Ascending)
                           ? System.ComponentModel.ListSortDirection.Ascending
                           : System.ComponentModel.ListSortDirection.Descending;
            e.Column.SortDirection = iDir; // this is a nullable prop, that's why we make certain it is assigned.
            var iView =
                (System.Windows.Data.ListCollectionView)
                System.Windows.Data.CollectionViewSource.GetDefaultView(DataContext);

                // datacontext contains the data list.
            var mySort = new CustomSort(iDir, e.Column);
            iView.CustomSort = mySort;
            e.Handled = true; // this prevents the build in sort.
        }

        /// <summary>The this_ closing.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void this_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DataContext = null; // we reset the datacontext to null, so that the NeuronData can be unloaded quickly.
        }

        #region inner types

        /// <summary>
        ///     A wrapper for a single object/posgroup that is displayed in the
        ///     editor.
        /// </summary>
        private class ObjectItem : Data.ObservableObject, INeuronWrapper
        {
            /// <summary>Initializes a new instance of the <see cref="ObjectItem"/> class. Initializes a new instance of the<see cref="DlgPOSEditor.ObjectItem"/> class.</summary>
            /// <param name="pos">The initial pos.</param>
            public ObjectItem(Neuron pos)
            {
                fPOS = pos;
            }

            /// <summary>
            ///     Gets or sets the cluster being wrapped.
            /// </summary>
            /// <value>
            ///     The cluster.
            /// </value>
            public NeuronCluster Cluster { get; set; }

            /// <summary>
            ///     Gets or sets a value indicating whether this instance is changed.
            /// </summary>
            /// <value>
            ///     <c>true</c> if this instance is changed; otherwise, <c>false</c> .
            /// </value>
            public bool IsChanged { get; set; }

            #region DisplayTitle

            /// <summary>
            ///     Gets/sets the display title of the item.
            /// </summary>
            /// <remarks>
            ///     Since this is a snapshot, we don't need direct linke to
            ///     NeuronInfo.displayTitle, instead, we need to update. IsCHanged.
            /// </remarks>
            public string DisplayTitle
            {
                get
                {
                    if (fData == null)
                    {
                        fData = BrainData.Current.NeuronInfo[Cluster.ID];
                    }

                    return fData.DisplayTitle;
                }
            }

            #endregion

            #region POS

            /// <summary>
            ///     Gets/sets the type of grammar assigned to the neuron wrapped by
            ///     this object.
            /// </summary>
            public Neuron POS
            {
                get
                {
                    return fPOS;
                }

                set
                {
                    fPOS = value;
                    OnPropertyChanged("POS");
                    IsChanged = true;
                }
            }

            #endregion

            #region ObjectType

            /// <summary>
            ///     Gets the type of neuron: posgroup (P), object (O) or compound
            ///     word(C). For info purpose.
            /// </summary>
            public string ObjectType { get; internal set; }

            #endregion

            #region INeuronWrapper Members

            /// <summary>
            ///     Gets the item. This is so we can browse the selected neuron (so the
            ///     user can further inspect items.
            /// </summary>
            /// <value>
            ///     The item.
            /// </value>
            public Neuron Item
            {
                get
                {
                    return Cluster;
                }
            }

            #endregion

            /// <summary>
            ///     Returns a <see cref="string" /> that represents the current
            ///     <see cref="object" /> .
            /// </summary>
            /// <returns>
            ///     A <see cref="string" /> that represents the current
            ///     <see cref="object" /> .
            /// </returns>
            public override string ToString()
            {
                return DisplayTitle;
            }

            #region Fields

            /// <summary>The f pos.</summary>
            private Neuron fPOS;

            /// <summary>The f data.</summary>
            private NeuronData fData;

            #endregion
        }

        /// <summary>
        ///     A custom sorter for the object item.
        /// </summary>
        private class CustomSort : System.Collections.IComparer
        {
            /// <summary>Initializes a new instance of the <see cref="CustomSort"/> class.</summary>
            /// <param name="direction">The direction.</param>
            /// <param name="column">The column.</param>
            public CustomSort(
                System.ComponentModel.ListSortDirection direction, 
                System.Windows.Controls.DataGridColumn column)
            {
                Direction = direction;
                Column = column;
            }

            /// <summary>
            ///     Gets or sets the direction to sort.
            /// </summary>
            /// <value>
            ///     The direction.
            /// </value>
            public System.ComponentModel.ListSortDirection Direction { get; private set; }

            /// <summary>
            ///     Gets or sets the column to sort.
            /// </summary>
            /// <value>
            ///     The column.
            /// </value>
            public System.Windows.Controls.DataGridColumn Column { get; private set; }

            /// <summary>The compare.</summary>
            /// <param name="X">The x.</param>
            /// <param name="Y">The y.</param>
            /// <returns>The <see cref="int"/>.</returns>
            int System.Collections.IComparer.Compare(object X, object Y)
            {
                Neuron iPos1, iPos2;
                string iStr1, iStr2;
                switch ((string)Column.Header)
                {
                    case "POS":
                        iPos1 = ((ObjectItem)X).POS;
                        iPos2 = ((ObjectItem)Y).POS;
                        iStr1 = iPos1 != null ? BrainData.Current.NeuronInfo[iPos1.ID].DisplayTitle : string.Empty;
                        iStr2 = iPos2 != null ? BrainData.Current.NeuronInfo[iPos2.ID].DisplayTitle : string.Empty;
                        return StringCompare(iStr1, iStr2);
                    case "Type":
                        iStr1 = ((ObjectItem)X).ObjectType ?? string.Empty;
                        iStr2 = ((ObjectItem)Y).ObjectType ?? string.Empty;
                        return StringCompare(iStr1, iStr2);
                    case "Name":
                        iStr1 = ((ObjectItem)X).DisplayTitle ?? string.Empty;
                        iStr2 = ((ObjectItem)Y).DisplayTitle ?? string.Empty;
                        return StringCompare(iStr1, iStr2);
                }

                return 0;
            }

            /// <summary>compares 2 strings.</summary>
            /// <param name="s1">The s1.</param>
            /// <param name="s2">The s2.</param>
            /// <returns>The <see cref="int"/>.</returns>
            private int StringCompare(string s1, string s2)
            {
                if (Direction == System.ComponentModel.ListSortDirection.Ascending)
                {
                    return s1.CompareTo(s2);
                }

                return s2.CompareTo(s1);
            }
        }

        #endregion

        #region function

        /// <summary>Called when the user clicked ok.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void OnClickOk(object sender, System.Windows.RoutedEventArgs e)
        {
            var iList = (System.Collections.Generic.List<ObjectItem>)DataContext;
            System.Diagnostics.Debug.Assert(iList != null);
            foreach (var i in iList)
            {
                if (i.IsChanged)
                {
                    i.Cluster.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.POS, i.POS);
                }

                var iData = BrainData.Current.NeuronInfo[i.Cluster.ID];
            }

            DialogResult = true;
        }

        /// <summary>
        ///     Loads all the object clusters into a temp structure that can easely be
        ///     edited.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         All posgroups are added, but only objects and compound words that
        ///         don't belong to a posgroup.
        ///     </para>
        ///     <para>
        ///         We deliberatly store a <see langword="ref" /> to the neurons, but don't
        ///         provide updates to this temp list since we want to make a snapsshot of
        ///         the state, as it was when the dialog was opened.
        ///     </para>
        /// </remarks>
        private void LoadData()
        {
            var iCount = 0;
            var iObjectCount = 0;
            var iRes = new System.Collections.Generic.List<ObjectItem>();
            try
            {
                for (var i = Neuron.StartId; i < Brain.Current.NextID; i++)
                {
                    NeuronCluster iCluster;
                    Neuron iFound;
                    if (Brain.Current.TryFindNeuron(i, out iFound))
                    {
                        iCluster = iFound as NeuronCluster;
                        if (iCluster != null)
                        {
                            if (iCluster.Meaning == (ulong)PredefinedNeurons.Object)
                            {
                                if (IsInPosGroup(iCluster) == false)
                                {
                                    var iNew = new ObjectItem(iCluster.FindFirstOut((ulong)PredefinedNeurons.POS));
                                    iNew.Cluster = iCluster;
                                    iNew.ObjectType = "O";
                                    iRes.Add(iNew);
                                    iCount++;
                                }

                                iObjectCount++;
                            }
                            else if (iCluster.Meaning == (ulong)PredefinedNeurons.POSGroup)
                            {
                                var iNew = new ObjectItem(iCluster.FindFirstOut((ulong)PredefinedNeurons.POS));
                                iNew.Cluster = iCluster;
                                iNew.ObjectType = "P";
                                iRes.Add(iNew);
                                iCount++;
                            }
                        }
                    }
                }
            }
            finally
            {
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, 
                    new System.Action<object, int, int>(SetDataContext), 
                    iRes, 
                    iCount, 
                    iObjectCount);

                    // need to pass the result back to the main thread. Always needs to be done, to make certain that the cursor is reset.
            }
        }

        /// <summary>Determines whether the <paramref name="item"/> is owned by a posgroup
        ///     or not.</summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if the specified <paramref name="item"/> is in a pos
        ///     group; otherwise, <c>false</c> .</returns>
        private bool IsInPosGroup(Neuron item)
        {
            if (item.ClusteredByIdentifier != null)
            {
                using (var iList = item.ClusteredBy)
                {
                    var iFound = (from u in iList
                                  let iPosGroup = Brain.Current[u] as NeuronCluster
                                  where iPosGroup != null && iPosGroup.Meaning == (ulong)PredefinedNeurons.POSGroup
                                  select iPosGroup).FirstOrDefault();
                    return iFound != null;
                }
            }

            return false;
        }

        /// <summary>Sets the data context.</summary>
        /// <param name="values">The values.</param>
        /// <param name="count">The count.</param>
        /// <param name="objectCount">The object Count.</param>
        private void SetDataContext(object values, int count, int objectCount)
        {
            DataContext = values;
            System.Windows.Input.Mouse.OverrideCursor = fPrevCursor;
            TxtCount.Text = string.Format("In list: {0}, Total objects: {1}", count, objectCount);
        }

        #endregion
    }
}