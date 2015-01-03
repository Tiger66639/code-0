// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorStyleSelector.cs" company="">
//   
// </copyright>
// <summary>
//   A style selector to help the treeview select a style for a folder or an
//   editor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A style selector to help the treeview select a style for a folder or an
    ///     editor.
    /// </summary>
    public class EditorStyleSelector : System.Windows.Controls.StyleSelector
    {
        #region FolderStyle

        /// <summary>
        ///     Gets/sets the style to be used for folders
        /// </summary>
        public System.Windows.Style FolderStyle { get; set; }

        #endregion

        #region EditorStyle

        /// <summary>
        ///     Gets/sets the style for editors
        /// </summary>
        public System.Windows.Style EditorStyle { get; set; }

        #endregion

        #region TopicStyle

        /// <summary>
        ///     Gets/sets the the style for topics.
        /// </summary>
        public System.Windows.Style TopicStyle { get; set; }

        #endregion

        /// <summary>When overridden in a derived class, returns a <see cref="System.Windows.Style"/>
        ///     based on custom logic.</summary>
        /// <param name="item">The content.</param>
        /// <param name="container">The element to which the style will be applied.</param>
        /// <returns>Returns an application-specific style to apply; otherwise, null.</returns>
        public override System.Windows.Style SelectStyle(object item, System.Windows.DependencyObject container)
        {
            if (item is EditorFolder)
            {
                return FolderStyle;
            }

            if (item is TextPatternEditor)
            {
                return TopicStyle;
            }

            return EditorStyle;
        }
    }
}