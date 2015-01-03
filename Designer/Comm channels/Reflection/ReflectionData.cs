// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionData.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all the data objects used by the
//   <see cref="ReflectionChannel" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Base class for all the data objects used by the
    ///     <see cref="ReflectionChannel" />
    /// </summary>
    /// <typeparam name="T">The type of the owner.</typeparam>
    public abstract class ReflectionData : Data.OwnedObject
    {
        #region functions

        /// <summary>
        ///     Called by a child to let the owner know that the
        ///     <see cref="IsLoaded" /> might have changed. This is recursive for as
        ///     long as the parent is a <see cref="ReflectionData" /> object.
        /// </summary>
        /// <remarks>
        ///     This is recursive cause, when a leaf has changed, all parents are
        ///     effected by this.
        /// </remarks>
        public void OnLoadedChanged()
        {
            OnPropertyChanged("IsLoaded");
            var iOwner = Owner as ReflectionData;
            if (iOwner != null)
            {
                iOwner.OnLoadedChanged();
            }
        }

        #endregion

        #region fields

        /// <summary>The f is selected.</summary>
        private bool fIsSelected;

        /// <summary>The f name.</summary>
        private string fName;

        /// <summary>The f is expanded.</summary>
        private bool fIsExpanded;

        #endregion

        #region Prop

        #region IsExpanded

        /// <summary>
        ///     Gets/sets the wether the item is expanded or not. This is for wpf.
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
        ///     Gets/sets if the item is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return fIsSelected;
            }

            set
            {
                fIsSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region Name

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        public string Name
        {
            get
            {
                return fName;
            }

            set
            {
                fName = value;
                OnPropertyChanged("Name");
            }
        }

        #endregion

        #region Sin

        /// <summary>
        ///     Gets the Reflection Sin for which this data object was created.
        /// </summary>
        /// <value>
        ///     The sin.
        /// </value>
        public ReflectionSin Sin
        {
            get
            {
                var iOwner = Owner;
                while (iOwner is Data.IOwnedObject && !(iOwner is ReflectionChannel))
                {
                    iOwner = ((Data.IOwnedObject)iOwner).Owner;
                }

                var iChannel = iOwner as ReflectionChannel;
                if (iChannel != null)
                {
                    return iChannel.Sin as ReflectionSin;
                }

                return null;
            }
        }

        #endregion

        #region IsLoaded

        /// <summary>
        ///     Gets or sets the function(s)/children are loaded.
        /// </summary>
        /// <remarks>
        ///     Setting to <see langword="null" /> is not processed.
        /// </remarks>
        /// <value>
        ///     <c>true</c> : all the children are loaded - the function is loaded
        ///     <c>false</c> : none of the children are loaded - the function is not
        ///     loaded. <c>null</c> : some loaded - invalid.
        /// </value>
        public abstract bool? IsLoaded { get; set; }

        #endregion

        /// <summary>
        ///     Gets the assembly that contains this method. This is used to let the
        ///     sin know which sin's need to be loaded for the functions.
        /// </summary>
        /// <value>
        ///     The assembly.
        /// </value>
        public System.Reflection.Assembly OwningAssembly
        {
            get
            {
                var iOwner = Owner as ReflectionData;
                while (iOwner != null && !(iOwner is AssemblyData))
                {
                    iOwner = iOwner.Owner as ReflectionData;
                }

                if (iOwner != null)
                {
                    return iOwner.OwningAssembly;
                }

                return null;
            }
        }

        #endregion
    }
}