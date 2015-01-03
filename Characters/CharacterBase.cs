// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterBase.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for objects that represent a character. This is a base class,
//   so we can share among silverlight and WPF the same import code for CCS
//   files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     Base class for objects that represent a character. This is a base class,
    ///     so we can share among silverlight and WPF the same import code for CCS
    ///     files.
    /// </summary>
    public abstract class CharacterBase : Data.NamedObject
    {
        #region AuthorWebsite

        /// <summary>
        ///     Gets/sets the char-creator's website address
        /// </summary>
        public virtual string AuthorWebsite
        {
            get
            {
                return fAuthorWebsite;
            }

            set
            {
                fAuthorWebsite = value;
                OnPropertyChanged("AuthorWebsite");
                OnPropertyChanged("HasAuthorWebsite");
            }
        }

        #endregion

        #region HasAuthorWebsite

        /// <summary>
        ///     Gets the wether there is a website available for the
        ///     <see langword="char" /> author. This is for the UI to bind agains.
        /// </summary>
        public bool HasAuthorWebsite
        {
            get
            {
                return string.IsNullOrEmpty(fAuthorWebsite) == false;
            }
        }

        #endregion

        #region Author

        /// <summary>
        ///     Gets/sets the name of the author
        /// </summary>
        public virtual string Author
        {
            get
            {
                return fAuthor;
            }

            set
            {
                fAuthor = value;
                OnPropertyChanged("Author");
            }
        }

        #endregion

        #region Copyright

        /// <summary>
        ///     Gets or sets the copyright text.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        public virtual string Copyright
        {
            get
            {
                return fCopyright;
            }

            set
            {
                fCopyright = value;
                OnPropertyChanged("Copyright");
            }
        }

        #endregion

        #region License

        /// <summary>
        ///     Gets or sets the license for the character.
        /// </summary>
        /// <value>
        ///     The license.
        /// </value>
        public virtual string License
        {
            get
            {
                return fLicense;
            }

            set
            {
                fLicense = value;
                OnPropertyChanged("License");
            }
        }

        #endregion

        #region CreationDate

        /// <summary>
        ///     Gets or sets the creation date.
        /// </summary>
        /// <value>
        ///     The creation date.
        /// </value>
        public virtual System.DateTime CreationDate
        {
            get
            {
                return fCreationDate;
            }

            set
            {
                fCreationDate = value;
                OnPropertyChanged("CreationDate");
            }
        }

        #endregion

        #region LastUpdateDate

        /// <summary>
        ///     Gets or sets the date of last modification.
        /// </summary>
        /// <value>
        ///     The last update date.
        /// </value>
        public virtual System.DateTime LastUpdateDate
        {
            get
            {
                return fLastUpdateDate;
            }

            set
            {
                fLastUpdateDate = value;
                OnPropertyChanged("LastUpdateDate");
            }
        }

        #endregion

        #region Rating

        /// <summary>
        ///     Gets or sets the rating.
        /// </summary>
        /// <value>
        ///     The rating.
        /// </value>
        public virtual string Rating
        {
            get
            {
                return fRating;
            }

            set
            {
                fRating = value;
                OnPropertyChanged("Rating");
            }
        }

        #endregion

        #region Sexual

        /// <summary>
        ///     Gets or sets the sexual.
        /// </summary>
        /// <value>
        ///     The sexual.
        /// </value>
        public virtual string Sexual
        {
            get
            {
                return fSexual;
            }

            set
            {
                fSexual = value;
                OnPropertyChanged("Sexual");
            }
        }

        #endregion

        #region Violence

        /// <summary>
        ///     Gets or sets the violence.
        /// </summary>
        /// <value>
        ///     The violence.
        /// </value>
        public virtual string Violence
        {
            get
            {
                return fViolence;
            }

            set
            {
                fViolence = value;
                OnPropertyChanged("Violence");
            }
        }

        #endregion

        #region Other

        /// <summary>
        ///     Gets or sets the other.
        /// </summary>
        /// <value>
        ///     The other.
        /// </value>
        public virtual string Other
        {
            get
            {
                return fOther;
            }

            set
            {
                fOther = value;
                OnPropertyChanged("Other");
            }
        }

        #endregion

        #region Description

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public virtual string Description
        {
            get
            {
                return fDescription;
            }

            set
            {
                fDescription = value;
                OnPropertyChanged("Description");
            }
        }

        #endregion

        #region fields

        /// <summary>The f author.</summary>
        private string fAuthor;

        /// <summary>The f copyright.</summary>
        private string fCopyright;

        /// <summary>The f author website.</summary>
        private string fAuthorWebsite;

        /// <summary>The f other.</summary>
        private string fOther;

        /// <summary>The f violence.</summary>
        private string fViolence;

        /// <summary>The f sexual.</summary>
        private string fSexual;

        /// <summary>The f rating.</summary>
        private string fRating;

        /// <summary>The f license.</summary>
        private string fLicense;

        /// <summary>The f description.</summary>
        private string fDescription;

        /// <summary>The f last update date.</summary>
        private System.DateTime fLastUpdateDate;

        /// <summary>The f creation date.</summary>
        private System.DateTime fCreationDate;

        #endregion

        #region Functions

        /// <summary>Loads the image for usage a as a viseme. The <paramref name="index"/>
        ///     specifies which viseme.</summary>
        /// <param name="index">The index.</param>
        /// <param name="source">The path to the image, this is absolute.</param>
        public abstract void LoadViseme(int index, string source);

        /// <summary>Instructs to load all the data for the following animation.</summary>
        /// <param name="toLoad">To load.</param>
        public abstract void LoadAnimation(Animation toLoad);

        /// <summary>Loads an animation that will be played all the time in the background.</summary>
        /// <param name="toLoad">To load.</param>
        public abstract void LoadBackgroundAnimation(Animation toLoad);

        /// <summary>Loads an image as a background (multiple images can be used to build
        ///     the background, so that parts can be hidden/visualized. If there is no
        ///     background defined, the first images of the viseme list is used.</summary>
        /// <param name="iSource">The i source.</param>
        public abstract void LoadBackground(string iSource);

        /// <summary>Instructs the <see langword="char"/> to load a single 'idle'<paramref name="level"/> that can be used by one of the previously
        ///     loaded animations.</summary>
        /// <param name="level">The level. When null, there were no idle levels defined in the file</param>
        public abstract void LoadIdleLevel(IdleLevel level);

        /// <summary>Sets the index of the Zindex to be used for the viseme images.</summary>
        /// <remarks>When called, it is garanteed that all the visemes have been read in.</remarks>
        /// <param name="iZIndex">Index of the i Z.</param>
        public abstract void SetVisemesZIndex(int iZIndex);

        #endregion
    }
}