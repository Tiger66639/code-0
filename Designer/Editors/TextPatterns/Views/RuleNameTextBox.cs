// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleNameTextBox.cs" company="">
//   
// </copyright>
// <summary>
//   the textbox for rule name, provides an extra prop to bind the selection
//   range against. (for resetting from the undo system).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     the textbox for rule name, provides an extra prop to bind the selection
    ///     range against. (for resetting from the undo system).
    /// </summary>
    public class RuleNameTextBox : PatternTextBox
    {
        #region SelRange

        /// <summary>
        ///     <see cref="SelRange" /> Dependency Property
        /// </summary>
        public static readonly System.Windows.DependencyProperty SelRangeProperty =
            System.Windows.DependencyProperty.Register(
                "SelRange", 
                typeof(object), 
                typeof(RuleNameTextBox), 
                new System.Windows.FrameworkPropertyMetadata((object)null));

        /// <summary>
        ///     Gets or sets the <see cref="SelRange" /> property. This dependency
        ///     property indicates an extra prop to bind the selection range against,
        ///     so we can update it when undoing.
        /// </summary>
        public object SelRange
        {
            get
            {
                return GetValue(SelRangeProperty);
            }

            set
            {
                SetValue(SelRangeProperty, value);
            }
        }

        #endregion
    }
}