// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CaretMonitor.cs" company="">
//   
// </copyright>
// <summary>
//   a dependency object that provides an attachable property for txtboxes, to
//   monitor the caret position. provides a way to track and display the
//   current line and column position for different text objects. When focus
//   is changed, the current position is automatically updated to the newly
//   focused item, if it has this info, otherwise the value of the last item
//   remains visilbe.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a dependency object that provides an attachable property for txtboxes, to
    ///     monitor the caret position. provides a way to track and display the
    ///     current line and column position for different text objects. When focus
    ///     is changed, the current position is automatically updated to the newly
    ///     focused item, if it has this info, otherwise the value of the last item
    ///     remains visilbe.
    /// </summary>
    public class CaretBehavior : System.Windows.DependencyObject
    {
        /// <summary>The get observe caret.</summary>
        /// <param name="obj">The obj.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool GetObserveCaret(System.Windows.DependencyObject obj)
        {
            return (bool)obj.GetValue(ObserveCaretProperty);
        }

        /// <summary>The set observe caret.</summary>
        /// <param name="obj">The obj.</param>
        /// <param name="value">The value.</param>
        public static void SetObserveCaret(System.Windows.DependencyObject obj, bool value)
        {
            obj.SetValue(ObserveCaretProperty, value);
        }

        /// <summary>The on observe caret property changed.</summary>
        /// <param name="dpo">The dpo.</param>
        /// <param name="e">The e.</param>
        private static void OnObserveCaretPropertyChanged(
            System.Windows.DependencyObject dpo, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var textBox = dpo as System.Windows.Controls.TextBox;
            if (textBox != null)
            {
                if ((bool)e.NewValue)
                {
                    textBox.SelectionChanged += textBox_SelectionChanged;
                }
                else
                {
                    textBox.SelectionChanged -= textBox_SelectionChanged;
                }
            }
        }

        /// <summary>The text box_ selection changed.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void textBox_SelectionChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            var caretIndex = textBox.CaretIndex;
            if (caretIndex > 0)
            {
                TextPosTracker.Default.Line = textBox.GetLineIndexFromCharacterIndex(caretIndex) + 1;
            }
            else
            {
                TextPosTracker.Default.Line = 0;
            }

            if (TextPosTracker.Default.Line > 0)
            {
                // becomes null if Query editor looses focus
                TextPosTracker.Default.Col = caretIndex
                                             - textBox.GetCharacterIndexFromLineIndex(TextPosTracker.Default.Line - 1)
                                             + 1;
            }
            else
            {
                TextPosTracker.Default.Col = 0;
            }
        }

        /// <summary>The observe caret property.</summary>
        public static readonly System.Windows.DependencyProperty ObserveCaretProperty =
            System.Windows.DependencyProperty.RegisterAttached(
                "ObserveCaret", 
                typeof(bool), 
                typeof(CaretBehavior), 
                new System.Windows.UIPropertyMetadata(false, OnObserveCaretPropertyChanged));
    }

    /// <summary>
    /// </summary>
    public class TextPosTracker : Data.ObservableObject
    {
        /// <summary>The f default.</summary>
        private static readonly TextPosTracker fDefault = new TextPosTracker();

        /// <summary>The f col.</summary>
        private int fCol;

        /// <summary>The f line.</summary>
        private int fLine;

        /// <summary>Prevents a default instance of the <see cref="TextPosTracker"/> class from being created.</summary>
        private TextPosTracker()
        {
        }

        /// <summary>
        ///     Gets the entry point.
        /// </summary>
        public static TextPosTracker Default
        {
            get
            {
                return fDefault;
            }
        }

        #region Line

        /// <summary>
        ///     Gets/sets the current line position.
        /// </summary>
        public int Line
        {
            get
            {
                return fLine;
            }

            set
            {
                fLine = value;
                OnPropertyChanged("Line");
            }
        }

        #endregion

        #region Col

        /// <summary>
        ///     Gets/sets the current column position.
        /// </summary>
        public int Col
        {
            get
            {
                return fCol;
            }

            set
            {
                fCol = value;
                OnPropertyChanged("Col");
            }
        }

        #endregion
    }
}