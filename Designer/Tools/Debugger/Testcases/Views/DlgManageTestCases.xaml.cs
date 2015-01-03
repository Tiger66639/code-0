// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DlgManageTestCases.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DlgManageTestCases.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Interaction logic for DlgManageTestCases.xaml
    /// </summary>
    public partial class DlgManageTestCases : System.Windows.Window
    {
        #region Ctor

        /// <summary>Initializes a new instance of the <see cref="DlgManageTestCases"/> class. 
        ///     Initializes a new instance of the <see cref="DlgManageTestCases"/>
        ///     class.</summary>
        public DlgManageTestCases()
        {
            InitializeComponent();
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of items that should be displayed. We make a copy of the
        ///     original, so we can apply the changes when the user presses ok.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<TestCaseItem> Items
        {
            get
            {
                if (fItems == null)
                {
                    fItems = new System.Collections.ObjectModel.ObservableCollection<TestCaseItem>();
                    foreach (var i in BrainData.Current.TestCases)
                    {
                        var iNew = new TestCaseItem();
                        iNew.TestCase = i;
                        iNew.Name = iNew.OriginalName = i.Name;
                        fItems.Add(iNew);
                    }
                }

                return fItems;
            }
        }

        #endregion

        /// <summary>Handles the SelectionChanged event of the ListBox control. Make
        ///     certain that the correct buttons are enabled.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void ListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            BtnCopy.IsEnabled = BtnDelete.IsEnabled = BtnName.IsEnabled = LstItems.SelectedItem != null;
        }

        /// <summary>Handles the Click event of the BtnOk control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnOk_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (var i in fDeleted)
            {
                if (string.IsNullOrEmpty(BrainData.Current.NeuronInfo.StoragePath) == false
                    && string.IsNullOrEmpty(i.OriginalName) == false)
                {
                    var iOrName = System.IO.Path.Combine(
                        BrainData.Current.NeuronInfo.StoragePath, 
                        i.OriginalName + "." + BrainData.TESTCASEEXT);
                    BrainData.Current.OpenDocuments.Remove(i.TestCase); // in case the testcase was open in the editor.
                    System.IO.File.Delete(iOrName);
                }

                BrainData.Current.TestCases.Remove(i.TestCase);
            }

            foreach (var i in Items)
            {
                if (string.IsNullOrEmpty(BrainData.Current.NeuronInfo.StoragePath) == false)
                {
                    if (i.CopyOf != null)
                    {
                        var iOrName = System.IO.Path.Combine(
                            BrainData.Current.NeuronInfo.StoragePath, 
                            i.CopyOf.OriginalName + "." + BrainData.TESTCASEEXT);
                        if (System.IO.File.Exists(iOrName) == false)
                        {
                            // could be that the original file was already renamed.
                            iOrName = System.IO.Path.Combine(
                                BrainData.Current.NeuronInfo.StoragePath, 
                                i.CopyOf.Name + "." + BrainData.TESTCASEEXT);
                        }

                        var iNewName = System.IO.Path.Combine(
                            BrainData.Current.NeuronInfo.StoragePath, 
                            i.Name + "." + BrainData.TESTCASEEXT);
                        System.IO.File.Copy(iOrName, iNewName);

                            // don't need to check if the new name already exists, this is done during the rename operation.
                    }
                    else if (i.Name != i.OriginalName)
                    {
                        var iOrName = System.IO.Path.Combine(
                            BrainData.Current.NeuronInfo.StoragePath, 
                            i.OriginalName + "." + BrainData.TESTCASEEXT);
                        var iNewName = System.IO.Path.Combine(
                            BrainData.Current.NeuronInfo.StoragePath, 
                            i.Name + "." + BrainData.TESTCASEEXT);
                        System.IO.File.Move(iOrName, iNewName);
                    }
                }

                if (i.TestCase == null)
                {
                    i.TestCase = new Test.TestCase();
                    i.TestCase.IsChanged = false; // is already copied from file.
                    BrainData.Current.TestCases.Add(i.TestCase);
                }

                i.TestCase.Name = i.Name; // make certain that the testcase knows it's name. Important if not yet saved.
            }

            DialogResult = true;
        }

        /// <summary>Handles the Click event of the BtnName control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnName_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSelected = LstItems.SelectedItem as TestCaseItem;
            System.Diagnostics.Debug.Assert(iSelected != null);

            var iDlg = new DlgStringQuestion();
            iDlg.Title = "Rename";
            iDlg.Question = "Name:";
            iDlg.Answer = iSelected.Name;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes == true)
            {
                var iOk = true;
                if (string.IsNullOrEmpty(BrainData.Current.NeuronInfo.StoragePath) == false)
                {
                    var iNewName = System.IO.Path.Combine(
                        BrainData.Current.NeuronInfo.StoragePath, 
                        iDlg.Answer + "." + BrainData.TESTCASEEXT);
                    iOk = !System.IO.File.Exists(iNewName);
                }
                else
                {
                    var iFound = (from i in fItems where i.Name == iDlg.Answer select i).FirstOrDefault();
                    iOk = iFound == null;
                }

                if (iOk == false)
                {
                    System.Windows.MessageBox.Show(
                        string.Format("Testcase '{0}' already exists, can't use this name!", iDlg.Answer), 
                        "Invalid name", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    iSelected.Name = iDlg.Answer;
                }
            }
        }

        /// <summary>Handles the Click event of the BtnDelete control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnDelete_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSelected = LstItems.SelectedItem as TestCaseItem;
            System.Diagnostics.Debug.Assert(iSelected != null);
            fDeleted.Add(iSelected);
            fItems.Remove(iSelected);
        }

        /// <summary>The btn copy_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnCopy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSelected = LstItems.SelectedItem as TestCaseItem;
            System.Diagnostics.Debug.Assert(iSelected != null);

            var iDlg = new DlgStringQuestion();
            iDlg.Title = "Rename";
            iDlg.Question = "Name:";
            iDlg.Answer = iSelected.Name;
            var iDlgRes = iDlg.ShowDialog();
            if (iDlgRes.HasValue && iDlgRes == true)
            {
                var iOk = true;
                if (string.IsNullOrEmpty(BrainData.Current.NeuronInfo.StoragePath) == false)
                {
                    var iNewName = System.IO.Path.Combine(
                        BrainData.Current.NeuronInfo.StoragePath, 
                        iDlg.Answer + "." + BrainData.TESTCASEEXT);
                    iOk = !System.IO.File.Exists(iNewName);
                }
                else
                {
                    var iFound = (from i in fItems where i.Name == iDlg.Answer select i).FirstOrDefault();
                    iOk = iFound == null;
                }

                if (iOk == false)
                {
                    System.Windows.MessageBox.Show(
                        string.Format("Testcase '{0}' already exists, can't use this name!", iDlg.Answer), 
                        "Invalid name", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Error);
                }
                else
                {
                    var iNew = new TestCaseItem();
                    iNew.CopyOf = iSelected;
                    iNew.Name = iDlg.Answer;
                    fItems.Add(iNew);
                }
            }
        }

        #region internal types

        /// <summary>
        ///     Represents a testcase, so that we can do the changes when the user
        ///     presses 'ok'.
        /// </summary>
        public class TestCaseItem : Data.ObservableObject
        {
            /// <summary>The f name.</summary>
            private string fName;

            /// <summary>Gets or sets the test case.</summary>
            public Test.TestCase TestCase { get; set; }

            /// <summary>
            ///     Gets or sets the name of the original testcase, so we can find it
            ///     again (if it is not a copy of another one).
            /// </summary>
            /// <value>
            ///     The name of the original.
            /// </value>
            public string OriginalName { get; set; }

            #region Name

            /// <summary>
            ///     Gets or sets the (new) name.
            /// </summary>
            /// <value>
            ///     The name.
            /// </value>
            public string Name
            {
                get
                {
                    return fName;
                }

                set
                {
                    fName = value;
                    OnPropertyChanged("Name");
                }
            }

            #endregion

            /// <summary>
            ///     Gets or sets the testCase item from which this is supposed to be a
            ///     copy. We refer to the object and not the name, cause this might
            ///     have changed.
            /// </summary>
            /// <value>
            ///     The copy of.
            /// </value>
            public TestCaseItem CopyOf { get; set; }

            /// <summary>
            ///     Gets the test case of which this is a copy.
            /// </summary>
            public Test.TestCase CopyOfTestCase
            {
                get
                {
                    var iItem = CopyOf;
                    if (iItem.TestCase != null)
                    {
                        return iItem.TestCase;
                    }

                    return iItem.CopyOfTestCase;
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>The f items.</summary>
        private System.Collections.ObjectModel.ObservableCollection<TestCaseItem> fItems;

        /// <summary>The f deleted.</summary>
        private readonly System.Collections.Generic.List<TestCaseItem> fDeleted =
            new System.Collections.Generic.List<TestCaseItem>();

        #endregion
    }
}