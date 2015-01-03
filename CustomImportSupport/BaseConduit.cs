// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseConduit.cs" company="">
//   
// </copyright>
// <summary>
//   The base conduit.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>The base conduit.</summary>
    public abstract class BaseConduit : Data.ObservableObject, ICustomConduit
    {
        /// <summary>asks the conduit to write it's settings to an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="writer"></param>
        public virtual void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlStore.WriteElement(writer, "CultureName", CultureName);
        }

        /// <summary>asks the conduit to read it's settings from an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="reader"></param>
        public virtual void ReadXml(System.Xml.XmlReader reader)
        {
            var iFound = string.Empty;
            if (XmlStore.TryReadElement(reader, "CultureName", ref iFound))
            {
                CultureName = iFound;
            }
        }

        #region fields

        /// <summary>The f files position.</summary>
        private double fFilesPosition;

        /// <summary>The f file position.</summary>
        private double fFilePosition;

        /// <summary>The f culture info.</summary>
        private System.Globalization.CultureInfo fCultureInfo; // for quick access to the formatter object.

        #endregion

        #region events

        /// <summary>
        ///     triggered when the system should be saved to disk.
        /// </summary>
        public event System.EventHandler SaveRequested;

        /// <summary>
        ///     Occurs when the operation is finished.
        /// </summary>
        public event System.EventHandler Finished;

        /// <summary>
        ///     called when a result has been found.
        /// </summary>
        public event System.EventHandler<StringEventArgs> ResultFound;

        #endregion

        #region prop

        #region FilesPosition

        /// <summary>
        ///     Gets the index of the file we are currently importing (can be used to keep track  of a large scale position
        ///     indicator).
        /// </summary>
        public double FilesPosition
        {
            get
            {
                return fFilesPosition;
            }

            set
            {
                fFilesPosition = value;
                OnPropertyChanged("FilesPosition");
            }
        }

        #endregion

        #region FilePosition

        /// <summary>
        ///     Gets/sets the relative file position (pos/length)
        /// </summary>
        public double FilePosition
        {
            get
            {
                return fFilePosition;
            }

            set
            {
                fFilePosition = value;
                OnPropertyChanged("FilePosition");
            }
        }

        #endregion

        #region CancelRequested

        /// <summary>
        ///     Gets the value that indicates if a cancel has been requested or not.
        ///     This allows us to cancel async operations.
        /// </summary>
        public bool CancelRequested { get; set; }

        #endregion

        #region CultureInfo

        /// <summary>
        ///     Gets/sets the name culture/IFormatter to use for converting numbers. This determins if a . is interpreted as a
        ///     thousand mark or a integer/floating part separator.
        /// </summary>
        public string CultureName
        {
            get
            {
                if (fCultureInfo != null)
                {
                    return fCultureInfo.Name;
                }

                return null;
            }

            set
            {
                if (value != CultureName)
                {
                    if (value == null)
                    {
                        fCultureInfo = null;
                    }
                    else
                    {
                        fCultureInfo = System.Globalization.CultureInfo.GetCultureInfo(value);
                    }

                    OnPropertyChanged("Culture");
                    OnPropertyChanged("CultureName");
                }
            }
        }

        /// <summary>
        ///     gets the object itself for converting the values.
        /// </summary>
        public System.Globalization.CultureInfo Culture
        {
            get
            {
                return fCultureInfo;
            }

            set
            {
                fCultureInfo = value;
                OnPropertyChanged("Culture");
                OnPropertyChanged("CultureName");
            }
        }

        #endregion

        /// <summary>
        ///     Gets a list of all the available cultures in the system. This is a helper class, so that wpf can easily bind to it.
        /// </summary>
        public System.Globalization.CultureInfo[] AvailableCultures
        {
            get
            {
                return System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the conduit is open or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public abstract bool IsOpen { get; }

        #endregion

        #region functions

        /// <summary>gets a value to indicate that the system should be prepared to receive 'SaveRequests' or not. Allows the system to
        ///     warn the user in
        ///     case of large data processing.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public virtual bool NeedsSaving()
        {
            return false;
        }

        /// <summary>
        ///     Gets a value to indicate if the process needs to be started with a location and destination or only a location
        ///     argument.
        /// </summary>
        /// <returns>
        ///     true or false
        /// </returns>
        public abstract bool NeedsDestination();

        /// <summary>reads in the data and/or starts producing output.
        ///     throws an exception by default.</summary>
        /// <param name="location">The location of the datasource</param>
        public virtual void Process(string location)
        {
            throw new System.InvalidOperationException();
        }

        /// <summary>reads in the data and/or starts producing output, rendering everthing to the specified output.
        ///     throws an exception by default.</summary>
        /// <param name="location">The location of the datasource</param>
        /// <param name="destination"></param>
        public virtual void Process(string location, string destination)
        {
            throw new System.InvalidOperationException();
        }

        /// <summary>
        ///     stops the operation.
        /// </summary>
        public void Cancel()
        {
            CancelRequested = true;
        }

        /// <summary>
        ///     raises the <see cref="DataConduit.SaveRequested" /> event
        /// </summary>
        protected void OnSaveRequested()
        {
            if (SaveRequested != null)
            {
                SaveRequested(this, System.EventArgs.Empty);
            }
        }

        /// <summary>
        ///     raises the <see cref="BaseConduit.Finished" /> event
        /// </summary>
        protected void OnFinished()
        {
            CancelRequested = false; // make certain taht the cancel request is reset, then we can start again properly.
            if (Finished != null)
            {
                Finished(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Called when a new result is found. Raises the event.</summary>
        /// <param name="value">The value.</param>
        protected void OnResultFound(string value)
        {
            if (ResultFound != null)
            {
                ResultFound(this, new StringEventArgs(value));
            }
        }

        #endregion

        #region ICustomConduit Members

        /// <summary>Opens the file at the specified location. So that data can be read by the <see cref="ICustomConduit.ReadLine"/>
        ///     function.
        ///     This is for a pull scenario.</summary>
        /// <param name="location">The location.</param>
        public virtual void Open(string location)
        {
            throw new System.InvalidOperationException();
        }

        /// <summary>reads a single line of data from the file.
        ///     This is used together with <see cref="ICustomConduit.Opern"/> and  <see cref="ICustomConduit.Close"/></summary>
        /// <param name="result"></param>
        /// <returns>false if no more line could be read, otherwise true.</returns>
        public virtual bool ReadLine(System.Collections.Generic.List<Neuron> result)
        {
            throw new System.InvalidOperationException();
        }

        /// <summary>
        ///     Closes this instance.
        /// </summary>
        public virtual void Close()
        {
            CancelRequested = false; // make certain that cancel is reset.
        }

        #endregion

        #region ICustomConduit Members

        /// <summary>provides a way to save any conduit specific data to the db, so the conduit can correcly be loaded from disk again.</summary>
        /// <param name="writer"></param>
        public virtual void WriteV1(System.IO.BinaryWriter writer)
        {
            if (CultureName != null)
            {
                writer.Write(CultureName);
            }
            else
            {
                writer.Write(string.Empty);
            }
        }

        /// <summary>provides a way to read the settings for the conduit from disk, so that it can be reused again with the previous
        ///     settings.</summary>
        /// <param name="reader"></param>
        public virtual void ReadV1(System.IO.BinaryReader reader)
        {
            CultureName = reader.ReadString();
        }

        #endregion
    }
}