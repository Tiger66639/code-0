// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DescriptionView.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   Interaction logic for DescriptionView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Interaction logic for DescriptionView.xaml
    /// </summary>
    public partial class DescriptionView : System.Windows.Controls.UserControl, System.Windows.IWeakEventListener
    {
        /// <summary>Handles the SelectionChanged event of the DescriptionEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void DescriptionEditor_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            fIsUpdating = true;
            try
            {
                var selectionRange = new System.Windows.Documents.TextRange(
                    DescriptionEditor.Selection.Start, 
                    DescriptionEditor.Selection.End);
                BtnBold.IsChecked = selectionRange.GetPropertyValue(FontWeightProperty).ToString() == "Bold";
                BtnItalic.IsChecked = selectionRange.GetPropertyValue(FontWeightProperty).ToString() == "Italic";
                BtnUnderline.IsChecked =
                    selectionRange.GetPropertyValue(System.Windows.Documents.Inline.TextDecorationsProperty)
                    == System.Windows.TextDecorations.Underline;

                BtnBullets.IsChecked = false;
                BtnNumbers.IsChecked = false;
                var iFound = selectionRange.Start.Parent;
                var iList = ControlFramework.Utility.TreeHelper.FindInLTree<System.Windows.Documents.List>(iFound);
                if (iList != null)
                {
                    var iStyle = iList.MarkerStyle;
                    if (iStyle == System.Windows.TextMarkerStyle.Decimal)
                    {
                        BtnNumbers.IsChecked = true;
                    }
                    else if (iStyle != System.Windows.TextMarkerStyle.None)
                    {
                        BtnBullets.IsChecked = true;
                    }
                }

                CmbFontSize.SelectedValue =
                    selectionRange.GetPropertyValue(System.Windows.Documents.FlowDocument.FontSizeProperty);
                CmbFont.SelectedItem =
                    selectionRange.GetPropertyValue(System.Windows.Documents.FlowDocument.FontFamilyProperty);

                if (
                    selectionRange.GetPropertyValue(System.Windows.Documents.FlowDocument.TextAlignmentProperty)
                        .Equals(System.Windows.TextAlignment.Left))
                {
                    BtnAlignLeft.IsChecked = true;
                }
                else if (
                    selectionRange.GetPropertyValue(System.Windows.Documents.FlowDocument.TextAlignmentProperty)
                        .Equals(System.Windows.TextAlignment.Right))
                {
                    BtnAlignRight.IsChecked = true;
                }
                else if (
                    selectionRange.GetPropertyValue(System.Windows.Documents.FlowDocument.TextAlignmentProperty)
                        .Equals(System.Windows.TextAlignment.Justify))
                {
                    BtnAlignJustify.IsChecked = true;
                }
                else if (
                    selectionRange.GetPropertyValue(
                        System.Windows.Documents.FlowDocument.TextAlignmentProperty)
                        .Equals(System.Windows.TextAlignment.Center))
                {
                    BtnAlignCenter.IsChecked = true;
                }
            }
            finally
            {
                fIsUpdating = false;
            }
        }

        /// <summary>Handles the TextInput event of the CmbFontSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.TextCompositionEventArgs"/> instance containing the
        ///     event data.</param>
        private void CmbFontSize_TextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (fIsUpdating == false)
            {
                double ival;
                if (double.TryParse(e.Text, out ival))
                {
                    DescriptionEditor.Selection.ApplyPropertyValue(FontSizeProperty, ival);
                }
                else
                {
                    LogService.Log.LogError(
                        "DescriptionView.CmbFontSize_TextInput", 
                        "Fontsize requires an integer value.");
                }
            }
        }

        /// <summary>Handles the SelectionChanged event of the CmbFontSize control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CmbFontSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (fIsUpdating == false && DescriptionEditor != null)
            {
                DescriptionEditor.Selection.ApplyPropertyValue(FontSizeProperty, e.AddedItems[0]);
            }
        }

        /// <summary>Handles the SelectionChanged event of the CmbFont control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.SelectionChangedEventArgs"/> instance containing the
        ///     event data.</param>
        private void CmbFont_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (fIsUpdating == false && DescriptionEditor != null)
            {
                DescriptionEditor.Selection.ApplyPropertyValue(FontFamilyProperty, e.AddedItems[0]);
            }
        }

        /// <summary>Handles the TextChanged event of the DescriptionEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event
        ///     data.</param>
        private void DescriptionEditor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            IsChanged = true;
        }

        /// <summary>The description editor_ preview lost keyboard focus.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void DescriptionEditor_PreviewLostKeyboardFocus(
            object sender, 
            System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            WindowMain.Current.FlushDescriptionData(DescriptionEditor);
        }

        /// <summary>The set document.</summary>
        /// <param name="value">The value.</param>
        internal static void SetDocument(System.Windows.Documents.FlowDocument value)
        {
            if (Current != null)
            {
                Current.Document = value;
            }
            else
            {
                fValueToUse = value;
            }
        }

        #region Fields

        /// <summary>The f is updating.</summary>
        private bool fIsUpdating;

                     // set to true when we are updating the toolbar buttons based on the current state of the newly selected items, so that we don't re-apply the values (response of the buttons).

        /// <summary>The f available fonts.</summary>
        private static System.Collections.Generic.List<System.Windows.Media.FontFamily> fAvailableFonts;

        /// <summary>The f value to use.</summary>
        private static System.Windows.Documents.FlowDocument fValueToUse;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="DescriptionView"/> class. 
        ///     Initializes a new instance of the <see cref="DescriptionView"/>
        ///     class.</summary>
        public DescriptionView()
        {
            InitializeComponent();
            AfterLoadEventManager.AddListener(BrainData.Current, this);
            if (fValueToUse != null)
            {
                // assigned from previous round, when we were not loaded
                Document = fValueToUse;
            }

            Current = this;
        }

        /// <summary>Finalizes an instance of the <see cref="DescriptionView"/> class. 
        ///     Releases unmanaged resources and performs other cleanup operations
        ///     before the <see cref="DescriptionView"/> is reclaimed by garbage
        ///     collection.</summary>
        ~DescriptionView()
        {
            AfterLoadEventManager.RemoveListener(BrainData.Current, this);
        }

        #endregion

        #region prop

        /// <summary>
        ///     provides access to the last loaded description view. This allows us
        ///     acces to this object from the main window and other places.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public static DescriptionView Current { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the current document was changed or
        ///     not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is changed; otherwise, <c>false</c> .
        /// </value>
        public bool IsChanged { get; private set; }

        /// <summary>
        ///     Gets or sets the document that is currently depicted.
        /// </summary>
        /// <remarks>
        ///     The get simply returns a shallow copy of the current flowdocument.
        ///     The setter uses the reference supplied, doesn't copy the contents.
        ///     When <see langword="null" /> is assigned, the editor is disabled, but
        ///     the previous data is still visible (although not returned). This is
        ///     done so that a text can still be read after something else was
        ///     selected that doesn't have a description.
        /// </remarks>
        /// <value>
        ///     The document.
        /// </value>
        public System.Windows.Documents.FlowDocument Document
        {
            get
            {
                if (DescriptionEditor.IsEnabled == false)
                {
                    return null;
                }

                return DescriptionEditor.Document;
            }

            set
            {
                if (value != null)
                {
                    DescriptionEditor.Document = value;

                        // we can assign directly cause all flowDocuments given to this editor should be detached.
                    DescriptionEditor.IsEnabled = true;
                }
                else
                {
                    DescriptionEditor.IsEnabled = false;
                }

                IsChanged = false;

                    // important: needs to be at the end, cause changing anything to the document, triggers 'TextChanged' which sets changed to true.
            }
        }

        /// <summary>
        ///     Gets the list of available fonts, filtered on only the visible ones.
        /// </summary>
        /// <value>
        ///     The available fonts.
        /// </value>
        public System.Collections.Generic.List<System.Windows.Media.FontFamily> AvailableFonts
        {
            get
            {
                return AvailableFontsStatic;
            }
        }

        /// <summary>Gets the available fonts static.</summary>
        public static System.Collections.Generic.List<System.Windows.Media.FontFamily> AvailableFontsStatic
        {
            get
            {
                if (fAvailableFonts == null)
                {
                    fAvailableFonts = new System.Collections.Generic.List<System.Windows.Media.FontFamily>();
                    foreach (var i in System.Windows.Media.Fonts.SystemFontFamilies)
                    {
                        if (IsSymbolFont(i) == false)
                        {
                            fAvailableFonts.Add(i);
                        }
                    }
                }

                return fAvailableFonts;
            }
        }

        /// <summary>Determines whether the font is a symbol font.</summary>
        /// <param name="fontFamily">The font family.</param>
        /// <returns><c>true</c> if [is symbol font] [the specified font family];
        ///     otherwise, <c>false</c> .</returns>
        internal static bool IsSymbolFont(System.Windows.Media.FontFamily fontFamily)
        {
            try
            {
                foreach (var typeface in fontFamily.GetTypefaces())
                {
                    System.Windows.Media.GlyphTypeface face;
                    if (typeface.TryGetGlyphTypeface(out face))
                    {
                        return face.Symbol;
                    }
                }

                return false;
            }
            catch
            {
                return true;
            }
        }

        #endregion

        #region IWeakEventListener Members

        /// <summary>The receive weak event.</summary>
        /// <param name="managerType">The manager type.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool ReceiveWeakEvent(System.Type managerType, object sender, System.EventArgs e)
        {
            if (managerType == typeof(AfterLoadEventManager))
            {
                BrainData_AfterLoaded(sender, e);
                return true;
            }

            return false;
        }

        /// <summary>Handles the AfterLoaded event of the <see cref="BrainData"/> control.</summary>
        /// <remarks>When a new one is loaded, need to make certain that the text of the
        ///     previous one is gone.</remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BrainData_AfterLoaded(object sender, System.EventArgs e)
        {
            Document = new System.Windows.Documents.FlowDocument(); // first empty it
            Document = null; // setting it to null wont empty it, but will disable it.
            IsChanged = false;
        }

        #endregion
    }
}