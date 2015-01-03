// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolViewItem.cs" company="">
//   
// </copyright>
// <summary>
//   represents a single tool view
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     represents a single tool view
    /// </summary>
    public class ToolViewItem : Data.ObservableObject
    {
        /// <summary>The f data template.</summary>
        private System.Windows.DataTemplate fDataTemplate;

        /// <summary>The f icon source.</summary>
        private string fIconSource;

        /// <summary>The f is active.</summary>
        private bool fIsActive;

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is visible.</summary>
        private bool fIsVisible = true;

        /// <summary>The f title.</summary>
        private string fTitle;

        #region Title

        /// <summary>
        ///     Gets/sets the title of the tool
        /// </summary>
        public string Title
        {
            get
            {
                return fTitle;
            }

            set
            {
                fTitle = value;
                OnPropertyChanged("Title");
            }
        }

        #endregion

        #region IsVisible

        /// <summary>
        ///     Gets/sets the value that indicates if the tool is visible or not.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return fIsVisible;
            }

            set
            {
                if (fIsVisible != value)
                {
                    fIsVisible = value;
                    OnPropertyChanged("IsVisible");
                }
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the value that indicates if this tool is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                if (value != fIsSelected)
                {
                    fIsSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region IconSource

        /// <summary>
        ///     Gets/sets the location of the icon that should be used for this tool.
        /// </summary>
        public string IconSource
        {
            get
            {
                return fIconSource;
            }

            set
            {
                fIconSource = value;
                OnPropertyChanged("IconSource");
            }
        }

        #endregion

        #region IsActive

        /// <summary>
        ///     Gets/sets fi this tool is currently active or not.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return fIsActive;
            }

            set
            {
                if (value != fIsActive)
                {
                    fIsActive = value;
                    OnPropertyChanged("IsActive");
                }
            }
        }

        #endregion

        #region DataTemplate

        /// <summary>
        ///     Gets/sets the dataTemplate to use for this tool.
        /// </summary>
        public System.Windows.DataTemplate DataTemplate
        {
            get
            {
                return fDataTemplate;
            }

            set
            {
                fDataTemplate = value;
                OnPropertyChanged("DataTemplate");
            }
        }

        #endregion

        /// <summary>
        ///     gets the id of the toolwindow, in case there was a translation. never
        ///     translate this prop, it is used internally to find toolwindows.
        /// </summary>
        public string ID { get; set; }
    }
}