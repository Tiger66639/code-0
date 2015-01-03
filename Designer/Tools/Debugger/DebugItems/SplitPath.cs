// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitPath.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="SplitPath" /> is used to store and retrieve a path to one or
//   more processors by means of the split instruction.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A <see cref="SplitPath" /> is used to store and retrieve a path to one or
    ///     more processors by means of the split instruction.
    /// </summary>
    /// <remarks>
    ///     Each time a split is performed, new processors are created. Each
    ///     processor gets a unique neuron assigned. This object records each neuron
    ///     that leads to a specific processor.
    /// </remarks>
    public class SplitPath : Data.ObservableObject
    {
        /// <summary>Finalizes an instance of the <see cref="SplitPath"/> class. </summary>
        ~SplitPath()
        {
            if (ProcessorManager.Current.SelectedPath == this)
            {
                ProcessorManager.Current.SelectedPath = null;
            }
        }

        #region fields

        /// <summary>The f name.</summary>
        private string fName;

        /// <summary>The f items.</summary>
        private readonly System.Collections.Generic.List<SplitPathItem> fItems =
            new System.Collections.Generic.List<SplitPathItem>();

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        /// <summary>The f path traversers.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<DebugProcessor> fPathTraversers =
            new System.Collections.ObjectModel.ObservableCollection<DebugProcessor>();

        /// <summary>The f color.</summary>
        private System.Windows.Media.Color fColor;

        #endregion

        #region prop

        #region Name

        /// <summary>
        ///     Gets/sets the name of the path.
        /// </summary>
        public string Name
        {
            get
            {
                return fName;
            }

            set
            {
                OnPropertyChanging("Name", fName, value);
                fName = value;
                OnPropertyChanged("Name");
            }
        }

        #endregion

        #region Items

        /// <summary>
        ///     Gets the list of neuron points that define this path.
        /// </summary>
        /// <remarks>
        ///     This is a simpel list, not an ObservableCollection because this info
        ///     can not change.
        /// </remarks>
        public System.Collections.Generic.List<SplitPathItem> Items
        {
            get
            {
                return fItems;
            }
        }

        #endregion

        #region IsExpanded

        /// <summary>
        ///     Gets/sets wether the view for this item is expanded or not. This needs
        ///     to be stored here if we want to use UI virtualization.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return fIsExpanded;
            }

            set
            {
                fIsExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }

        #endregion

        #region IsSelected

        /// <summary>
        ///     Gets/sets the wether this path is the selected path to follow during
        ///     debugging.
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
                    if (fIsSelected)
                    {
                        ProcessorManager.Current.SelectedPath = null;
                        foreach (var i in ProcessorManager.Current.Processors)
                        {
                            i.OnSplitPathUnSelected(this);
                        }
                    }

                    fIsSelected = value;
                    if (fIsSelected)
                    {
                        ProcessorManager.Current.SelectedPath = this;
                        foreach (var i in ProcessorManager.Current.Processors)
                        {
                            i.OnSplitPathSelected(this);
                        }
                    }

                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion

        #region Color

        /// <summary>
        ///     Gets/sets the color to use for this splitpath. This is rendered by
        ///     default.
        /// </summary>
        public System.Windows.Media.Color Color
        {
            get
            {
                return fColor;
            }

            set
            {
                fColor = value;
                OnPropertyChanged("Color");
            }
        }

        #endregion

        #endregion
    }
}