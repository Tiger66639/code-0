// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatBotDialogCtrl.cs" company="">
//   
// </copyright>
// <summary>
//   A richtextbox that exposes a property for monitoring a list and
//   displaying the content of that list formatted in such a way that it's
//   easy to make a difference between 'bot' sentences and 'user' sentences.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     A richtextbox that exposes a property for monitoring a list and
    ///     displaying the content of that list formatted in such a way that it's
    ///     easy to make a difference between 'bot' sentences and 'user' sentences.
    /// </summary>
    public class ChatBotDialogCtrl : System.Windows.Controls.RichTextBox
    {
        #region ItemsSource

        /// <summary>
        ///     <see cref="ItemsSource" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty ItemsSourceProperty =
            System.Windows.DependencyProperty.Register(
                "ItemsSource", 
                typeof(System.Collections.ObjectModel.ObservableCollection<string>), 
                typeof(ChatBotDialogCtrl), 
                new System.Windows.FrameworkPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        ///     Gets or sets the <see cref="ItemsSource" /> property. This dependency
        ///     property indicates which list to use as datasource.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> ItemsSource
        {
            get
            {
                return (System.Collections.ObjectModel.ObservableCollection<string>)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        /// <summary>Handles changes to the <see cref="ItemsSource"/> property.</summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The e.</param>
        private static void OnItemsSourceChanged(
            System.Windows.DependencyObject d, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            ((ChatBotDialogCtrl)d).OnItemsSourceChanged(e);
        }

        /// <summary>Provides derived classes an opportunity to handle changes to the<see cref="ItemsSource"/> property.</summary>
        /// <param name="e">The e.</param>
        protected virtual void OnItemsSourceChanged(System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var iVal = e.OldValue as System.Collections.ObjectModel.ObservableCollection<string>;
            if (iVal != null)
            {
                iVal.CollectionChanged -= List_CollectionChanged;
            }

            Document.Blocks.Clear(); // make certain that there is no previous data buffered.
            iVal = e.NewValue as System.Collections.ObjectModel.ObservableCollection<string>;
            if (iVal != null)
            {
                iVal.CollectionChanged += List_CollectionChanged;
                BuildList(iVal);
            }
        }

        /// <summary>The build list.</summary>
        /// <param name="list">The list.</param>
        private void BuildList(System.Collections.ObjectModel.ObservableCollection<string> list)
        {
            System.Windows.Documents.Paragraph iLast = null;
            foreach (var i in list)
            {
                iLast = AddPar(i);
            }

            if (iLast != null)
            {
                iLast.Loaded += iPar_Loaded;

                    // we can only try to bring it into view after it is loaded, hence the event handler.
            }
        }

        /// <summary>The list_ collection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void List_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    var iVal = (string)e.NewItems[0];
                    var iPar = AddPar(iVal);
                    iPar.Loaded += iPar_Loaded;

                        // we can only try to bring it into view after it is loaded, hence the event handler.
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:

                    // not implemented yet.
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                    // not implemented yet.
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:

                    // not implemented yet.
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    Document.Blocks.Clear(); // clear the entire content.
                    break;
                default:
                    break;
            }
        }

        /// <summary>The add par.</summary>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="Paragraph"/>.</returns>
        private System.Windows.Documents.Paragraph AddPar(string val)
        {
            System.Windows.Documents.Inline iLine = new System.Windows.Documents.Run(val);
            if (val.StartsWith(ChatBotChannel.BOT))
            {
                iLine.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                iLine.Foreground = System.Windows.Media.Brushes.Blue;
            }

            var iPar = new System.Windows.Documents.Paragraph(iLine);
            Document.Blocks.Add(iPar);
            return iPar;
        }

        /// <summary>The i par_ loaded.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iPar_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var iSender = (System.Windows.Documents.Paragraph)sender;
            System.Diagnostics.Debug.Assert(iSender != null);
            iSender.BringIntoView();
            iSender.Loaded -= iPar_Loaded;
        }

        #endregion
    }
}