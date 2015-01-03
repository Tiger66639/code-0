// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgSelectMeanings.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Provides a geneneral purpose selection method for multiple meanings.
//   Provides an 'update' list functionality.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Dialogs
{
    using System.Linq;

    /// <summary>
    ///     Provides a geneneral purpose selection method for multiple meanings.
    ///     Provides an 'update' list functionality.
    /// </summary>
    public partial class DlgSelectMeanings : System.Windows.Window
    {
        #region fields

        /// <summary>The f meanings.</summary>
        private readonly System.Collections.Generic.List<AvailableMeaning> fMeanings =
            new System.Collections.Generic.List<AvailableMeaning>();

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DlgSelectMeanings"/> class. 
        ///     Initializes a new instance of the <see cref="DlgSelectMeanings"/>
        ///     class.</summary>
        public DlgSelectMeanings()
        {
            foreach (var i in BrainData.Current.DefaultMeaningIds)
            {
                var iNew = new AvailableMeaning();
                iNew.Item = Brain.Current[i];
                fMeanings.Add(iNew);
            }

            InitializeComponent();
        }

        #endregion

        #region Meanings

        /// <summary>
        ///     Gets the list of meanings that can be selected.
        /// </summary>
        public System.Collections.Generic.List<AvailableMeaning> Meanings
        {
            get
            {
                return fMeanings;
            }
        }

        #endregion

        /// <summary>Handles the Click event of the Ok control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void Ok_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #region internal types

        /// <summary>
        ///     <list type="number">
        ///         <item>
        ///             <description>meaning that can be selected.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public class AvailableMeaning
        {
            /// <summary>Gets or sets the item.</summary>
            public Neuron Item { get; set; }

            /// <summary>Gets or sets a value indicating whether is selected.</summary>
            public bool IsSelected { get; set; }

            /// <summary>Gets or sets a value indicating whether or is selected.</summary>
            public bool OrIsSelected { get; set; }
        }

        #endregion

        #region SelectedItems

        /// <summary>
        ///     Gets/sets the list of selected items. When assigned, the list of
        ///     selected items are updated. Get this list to retrieve all the items
        ///     that are selected after the operation.
        /// </summary>
        public System.Collections.Generic.IList<Neuron> SelectedItems
        {
            get
            {
                return (from i in Meanings where i.IsSelected select i.Item).ToList();
            }

            set
            {
                if (value == null)
                {
                    foreach (var i in Meanings)
                    {
                        i.IsSelected = false;
                        i.OrIsSelected = false;
                    }
                }
                else
                {
                    foreach (var i in value)
                    {
                        var iFound = (from u in Meanings where u.Item == i select u).FirstOrDefault();
                        if (iFound != null)
                        {
                            iFound.IsSelected = true;
                            iFound.OrIsSelected = true;
                        }
                        else
                        {
                            var iNew = new AvailableMeaning();
                            iNew.Item = i;
                            iNew.IsSelected = true;
                            iNew.OrIsSelected = true;
                        }
                    }
                }
            }
        }

        #region UnselectedItems

        /// <summary>
        ///     Gets the list of items that were selected before the operation, but
        ///     were unselected.
        /// </summary>
        public System.Collections.Generic.IList<Neuron> UnselectedItems
        {
            get
            {
                return (from i in Meanings where i.IsSelected == false && i.OrIsSelected select i.Item).ToList();
            }
        }

        #endregion

        #region NewSelectedItems

        /// <summary>
        ///     Gets the list of items that weren't selected when started, but were at
        ///     the end.
        /// </summary>
        public System.Collections.Generic.IList<Neuron> NewSelectedItems
        {
            get
            {
                return (from i in Meanings where i.IsSelected && i.OrIsSelected == false select i.Item).ToList();
            }
        }

        #endregion

        #endregion
    }
}