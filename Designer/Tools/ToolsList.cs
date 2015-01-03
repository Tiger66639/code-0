// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolsList.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the list of all the tools.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Defines the list of all the tools.
    /// </summary>
    public class ToolsList : System.Collections.Generic.List<ToolViewItem>
    {
        /// <summary>The f default.</summary>
        private static ToolsList fDefault;

        /// <summary>The f description tool.</summary>
        private ToolViewItem fDescriptionTool;

        /// <summary>The f explorer tool.</summary>
        private ToolViewItem fExplorerTool;

        /// <summary>The f log tool.</summary>
        private ToolViewItem fLogTool;

        /// <summary>The f search results tool.</summary>
        private ToolViewItem fSearchResultsTool;

        /// <summary>Gets the default.</summary>
        public static ToolsList Default
        {
            get
            {
                if (fDefault == null)
                {
                    fDefault = WindowMain.Current.FindResource("Tools") as ToolsList;
                }

                return fDefault;
            }
        }

        /// <summary>
        ///     Provides quick access to the description tool.
        /// </summary>
        /// <value>
        ///     The description tool.
        /// </value>
        public ToolViewItem DescriptionTool
        {
            get
            {
                if (fDescriptionTool == null)
                {
                    fDescriptionTool = (from i in this where i.ID == "Description" select i).FirstOrDefault();
                }

                return fDescriptionTool;
            }
        }

        /// <summary>
        ///     Provides quick access to the description tool.
        /// </summary>
        /// <value>
        ///     The description tool.
        /// </value>
        public ToolViewItem LogTool
        {
            get
            {
                if (fLogTool == null)
                {
                    fLogTool = (from i in this where i.ID == "Log" select i).FirstOrDefault();
                }

                return fLogTool;
            }
        }

        /// <summary>
        ///     Provides quick access to the description tool.
        /// </summary>
        /// <value>
        ///     The description tool.
        /// </value>
        public ToolViewItem SearchResultsTool
        {
            get
            {
                if (fSearchResultsTool == null)
                {
                    fSearchResultsTool = (from i in this where i.ID == "SearchResults" select i).FirstOrDefault();
                }

                return fSearchResultsTool;
            }
        }

        /// <summary>
        ///     Provides quick access to the description tool.
        /// </summary>
        /// <value>
        ///     The description tool.
        /// </value>
        public ToolViewItem ExplorerTool
        {
            get
            {
                if (fExplorerTool == null)
                {
                    fExplorerTool = (from i in this where i.ID == "Explorer" select i).FirstOrDefault();
                }

                return fExplorerTool;
            }
        }

        /// <summary>
        ///     Sets up the tools for a viewer.Visibility="{Binding ElementName=This,
        ///     Path=DesignerVisibility, Mode=OneTime}"
        /// </summary>
        internal void SetViewer()
        {
            foreach (var i in this)
            {
                i.IsVisible = false;
            }
        }

        /// <summary>The set pro.</summary>
        internal void SetPro()
        {
            foreach (var i in this)
            {
                if (i.ID == "Toolbox" || i.ID == "Explorer" || i.ID == "MemProfiler" || i.ID == "Timers"
                    || i.ID == "Debugger")
                {
                    i.IsVisible = false;
                }
            }
        }
    }
}