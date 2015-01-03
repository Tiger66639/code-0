// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MindMapNoteView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for MindMapNoteView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for MindMapNoteView.xaml
    /// </summary>
    public partial class MindMapNoteView : System.Windows.Controls.UserControl, System.Windows.IWeakEventListener
    {
        /// <summary>The f editor.</summary>
        private System.Windows.Controls.RichTextBox fEditor;

                                                    // we need a ref to the texteditor, so we can set the document content. Since we use a datatemplate, can't reference it directly.

        /// <summary>The f internal change.</summary>
        private bool fInternalChange;

        /// <summary>Initializes a new instance of the <see cref="MindMapNoteView"/> class.</summary>
        public MindMapNoteView()
        {
            InitializeComponent();
        }

        #region IWeakEventListener

        /// <summary>Receives events from the centralized event manager.</summary>
        /// <param name="managerType">The type of the <see cref="System.Windows.WeakEventManager"/> calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns><see langword="true"/> if the listener handled the event. It is
        ///     considered an error by the <see cref="System.Windows.WeakEventManager"/> handling in
        ///     WPF to register a listener for an event that the listener does not
        ///     handle. Regardless, the method should return <see langword="false"/>
        ///     if it receives an event that it does not recognize or handle.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(System.ComponentModel.PropertyChangedEventManager))
            {
                // only happens when the descriptio has changed.
                if (fInternalChange == false && fEditor != null)
                {
                    fEditor.Document = ((MindMapNote)DataContext).Description;
                }

                return true;
            }

            return false;
        }

        #endregion

        /// <summary>Used to assign the flowdocument in the data context to the richtextbox
        ///     editor, it doesn't allow binding.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void NoteEditorLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            fEditor = (System.Windows.Controls.RichTextBox)sender;
            fEditor.Document = ((MindMapNote)Content).Description;
        }

        /// <summary>Handles the LostFocus event of the NoteEditor control.</summary>
        /// <remarks>need to save the value back to the data object. That is because it
        ///     always returns a new item and keeps no <see langword="ref"/> to the
        ///     flowdocs it returns.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void NoteEditor_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            fInternalChange = true;

                // need to prevent that MindMapNote_PropertyChanged gets called when we change the value, this causes a loop, which we don't want.
            try
            {
                var iSender = (System.Windows.Controls.RichTextBox)sender;
                ((MindMapNote)Content).Description = iSender.Document;
            }
            finally
            {
                fInternalChange = false;
            }
        }

        /// <summary>Called when the<see cref="System.Windows.Controls.ContentControl.Content"/> property
        ///     changes.</summary>
        /// <param name="oldContent">The old value of the<see cref="System.Windows.Controls.ContentControl.Content"/>
        ///     property.</param>
        /// <param name="newContent">The new value of the<see cref="System.Windows.Controls.ContentControl.Content"/>
        ///     property.</param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent != null)
            {
                System.ComponentModel.PropertyChangedEventManager.RemoveListener(
                    (MindMapNote)oldContent, 
                    this, 
                    "Description");
            }

            if (newContent != null)
            {
                System.ComponentModel.PropertyChangedEventManager.AddListener(
                    (MindMapNote)newContent, 
                    this, 
                    "Description");
            }

            base.OnContentChanged(oldContent, newContent);
        }
    }
}