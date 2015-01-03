// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CsvRenderPipe.cs" company="">
//   
// </copyright>
// <summary>
//   provides a way to save the output of a query to a csv file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>
    ///     provides a way to save the output of a query to a csv file.
    /// </summary>
    public class CsvRenderPipe : Queries.IRenderPipe
    {
        /// <summary>The f field separator.</summary>
        private char fFieldSeparator = ',';

        /// <summary>The f writer.</summary>
        private CsvWriter fWriter;

        /// <summary>Initializes a new instance of the <see cref="CsvRenderPipe"/> class.</summary>
        public CsvRenderPipe()
        {
            Append = false;
        }

        #region FileName

        /// <summary>
        ///     Gets/sets the name of the file to render to.
        /// </summary>
        public string FileName { get; set; }

        #endregion

        #region Append

        /// <summary>
        ///     Gets/sets the value that indicates if the data will be appended to the
        ///     file or a new file will be created. Default = new file.
        /// </summary>
        public bool Append { get; set; }

        #endregion

        /// <summary>
        ///     Gets a list of all the available cultures in the system. This is a
        ///     helper class, so that wpf can easily bind to it. So that xaml can bind
        ///     to it.
        /// </summary>
        public System.Globalization.CultureInfo[] AvailableCultures
        {
            get
            {
                return System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.AllCultures);
            }
        }

        #region Separator

        /// <summary>
        ///     Gets/sets the character to use for seperating the fields.
        /// </summary>
        public char FieldSeparator
        {
            get
            {
                return fFieldSeparator;
            }

            set
            {
                fFieldSeparator = value;
                if (fWriter != null)
                {
                    fWriter.Separator = value;
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if tab is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsTab
        {
            get
            {
                return fFieldSeparator == '\t';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = '\t';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if SemiColon is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsSemiColon
        {
            get
            {
                return fFieldSeparator == ';';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ';';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Comma is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsComma
        {
            get
            {
                return fFieldSeparator == ',';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ',';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Space is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsSpace
        {
            get
            {
                return fFieldSeparator == ' ';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = ' ';
                }
            }
        }

        /// <summary>
        ///     for binding, gets/sets if Pipe is in the field separators list.
        /// </summary>
        public bool FieldSeparatorsIsPipe
        {
            get
            {
                return fFieldSeparator == '|';
            }

            set
            {
                if (value)
                {
                    fFieldSeparator = '|';
                }
            }
        }

        #endregion

        #region CultureInfo

        /// <summary>
        ///     Gets/sets the name culture/IFormatter to use for converting numbers.
        ///     This determins if a . is interpreted as a thousand mark or a
        ///     integer/floating part separator.
        /// </summary>
        public string CultureName
        {
            get
            {
                if (Culture != null)
                {
                    return Culture.Name;
                }

                return null;
            }

            set
            {
                if (value != CultureName)
                {
                    if (value == null)
                    {
                        Culture = null;
                    }
                    else
                    {
                        Culture = System.Globalization.CultureInfo.GetCultureInfo(value);
                    }

                    // OnPropertyChanged("Culture");
                    // OnPropertyChanged("CultureName");
                }
            }
        }

        /// <summary>
        ///     gets the object itself for converting the values.
        /// </summary>
        public System.Globalization.CultureInfo Culture { get; set; }

        #endregion

        #region IRenderPipe Members

        /// <summary>
        ///     called when extra data needs to be saved to disk in seperate files.
        /// </summary>
        public void Flush()
        {
            if (fWriter != null)
            {
                fWriter.Flush();
            }
        }

        /// <summary>write the settings to a stream.</summary>
        /// <param name="writer"></param>
        public void WriteV1(System.IO.BinaryWriter writer)
        {
            writer.Write((System.Int16)2); // version nr.
            if (FileName != null)
            {
                writer.Write(FileName);
            }
            else
            {
                writer.Write(string.Empty);
            }

            writer.Write(Append);
            if (CultureName != null)
            {
                writer.Write(CultureName);
            }
            else
            {
                writer.Write(string.Empty);
            }

            writer.Write(FieldSeparator);
        }

        /// <summary>read the settings from a stream.</summary>
        /// <param name="reader"></param>
        public void ReadV1(System.IO.BinaryReader reader)
        {
            var iVer = reader.ReadInt16();
            if (iVer >= 1)
            {
                FileName = reader.ReadString();
                Append = reader.ReadBoolean();
                CultureName = reader.ReadString();
            }

            if (iVer == 2)
            {
                // separator was new from ver 2.
                FieldSeparator = reader.ReadChar();
            }
        }

        /// <summary>
        ///     called when the rendering is about to begin.
        /// </summary>
        public void Open()
        {
            if (fWriter != null)
            {
                fWriter.Close();
            }

            System.IO.FileStream iFile;
            if (Append == false)
            {
                iFile = new System.IO.FileStream(FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            }
            else
            {
                iFile = new System.IO.FileStream(FileName, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            }

            fWriter = new CsvWriter(iFile, FieldSeparator);
        }

        /// <summary>
        ///     called when rendering is done and resources can be released.
        /// </summary>
        public void Close()
        {
            if (fWriter != null)
            {
                fWriter.Close();
                fWriter = null;
            }
        }

        /// <summary>called when output was found</summary>
        /// <param name="values"></param>
        public void Output(System.Collections.Generic.IList<Neuron> values)
        {
            fWriter.WriteLine(values);
        }

        #endregion
    }
}