// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextChannelView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   This class displays the info of a <see cref="TextSin" /> and provides
//   interaction with it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This class displays the info of a <see cref="TextSin" /> and provides
    ///     interaction with it.
    /// </summary>
    public partial class TextChannelView : System.Windows.Controls.UserControl
    {
        #region ctor-dtor

        /// <summary>Initializes a new instance of the <see cref="TextChannelView"/> class.</summary>
        public TextChannelView()
        {
            InitializeComponent();
        }

        #endregion

        #region Prop

        /// <summary>
        ///     Gets the <see cref="TextSin" /> that we provide a view for.
        /// </summary>
        /// <remarks>
        ///     This is retrieved when the <see cref="TextChannel.Channel" /> property
        ///     is assigned.
        /// </remarks>
        public TextSin Sin { get; private set; }

        #endregion

        /// <summary>Handles the Click event of the SaveConv control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void SaveConv_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iDlg = new Microsoft.Win32.SaveFileDialog();
            iDlg.AddExtension = true;
            iDlg.DefaultExt = "txt";
            iDlg.Filter = "Text files (*.txt)|*.txt";
            var iDlgRes = iDlg.ShowDialog(WindowMain.Current);
            if (iDlgRes.HasValue && iDlgRes.Value)
            {
                var iChannel = (TextChannel)DataContext;
                System.Diagnostics.Debug.Assert(iChannel != null);
                using (var iWriter = System.IO.File.CreateText(iDlg.FileName))
                {
                    foreach (var i in iChannel.DialogData)
                    {
                        iWriter.WriteLine("{0}: {1}", i.Originator, i.Text);
                    }
                }
            }
        }

        /// <summary>Handles the Click event of the SaveConv control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BtnCopy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (TextChannel)DataContext;
            System.Diagnostics.Debug.Assert(iChannel != null);
            var iRes = new System.Text.StringBuilder();
            foreach (var i in iChannel.DialogData)
            {
                iRes.AppendFormat("{0}: {1}", i.Originator, i.Text);
                iRes.AppendLine();
            }

            System.Windows.Clipboard.SetText(iRes.ToString(), System.Windows.TextDataFormat.Text);
        }

        /// <summary>Handles the DataContextChanged event of the Ctrl control. When new
        ///     textchannel -&gt; monitor text collection, so that when an item is added,
        ///     we can scroll it into view.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void Ctrl_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iChannel = e.OldValue as TextChannel;
            if (iChannel != null)
            {
                iChannel.DialogData.CollectionChanged -= DialogData_CollectionChanged;
            }

            iChannel = e.NewValue as TextChannel;
            if (iChannel != null)
            {
                iChannel.DialogData.CollectionChanged += DialogData_CollectionChanged;
            }
        }

        /// <summary>Handles the CollectionChanged event of the DialogData control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void DialogData_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                LstDialog.ScrollIntoView(e.NewItems[0]);
                LstDialog.SelectedItem = e.NewItems[0];
            }
        }

        #region functions

        /// <summary>The btn send_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnSend_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SendTextToSin();
        }

        /// <summary>The txt send_ prv key down.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void TxtSend_PrvKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                SendTextToSin();
            }
        }

        /// <summary>
        ///     Tries to send the text in the send text box to the Sin (if there is
        ///     anything to send).
        /// </summary>
        private void SendTextToSin()
        {
            var iChannel = (TextChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.SendTextToSin(TxtToSend.Text);
                iChannel.InputText = null; // clear the selected text so that the use can enter new data.
            }
        }

        /// <summary>The btn clear_ click.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void BtnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var iChannel = (TextChannel)DataContext;
            if (iChannel != null)
            {
                iChannel.ClearData();
            }
        }

        #endregion
    }
}