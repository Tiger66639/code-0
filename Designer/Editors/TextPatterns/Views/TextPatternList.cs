// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextPatternList.cs" company="">
//   
// </copyright>
// <summary>
//   The text pattern list.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>The text pattern list.</summary>
    public class TextPatternList : System.Windows.Controls.ItemsControl
    {
        /// <summary>Initializes static members of the <see cref="TextPatternList"/> class.</summary>
        static TextPatternList()
        {
            FocusableProperty.OverrideMetadata(
                typeof(TextPatternList), 
                new System.Windows.FrameworkPropertyMetadata(false));

            // DefaultStyleKeyProperty.OverrideMetadata(typeof(TextPatternList), new FrameworkPropertyMetadata(typeof(TextPatternList)));
        }

        /// <summary>Determines if the specified <paramref name="item"/> is (or is
        ///     eligible to be) its own container.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns><see langword="true"/> if the <paramref name="item"/> is (or is
        ///     eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is System.Windows.Controls.ContentControl;
        }

        /// <summary>
        ///     Creates or identifies the element that is used to display the given
        ///     item.
        /// </summary>
        /// <returns>
        ///     The element that is used to display the given item.
        /// </returns>
        protected override System.Windows.DependencyObject GetContainerForItemOverride()
        {
            return new System.Windows.Controls.ContentControl();
        }
    }
}