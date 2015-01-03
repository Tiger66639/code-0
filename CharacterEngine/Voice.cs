// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Voice.cs" company="">
//   
// </copyright>
// <summary>
//   Defines extra information for a TTs voice, like the TTS send type to use,
//   the prefered character,...
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    /// <summary>
    ///     Defines extra information for a TTs voice, like the TTS send type to use,
    ///     the prefered character,...
    /// </summary>
    public class Voice : Data.NamedObject
    {
        /// <summary>The f is enabled.</summary>
        private bool fIsEnabled;

        #region IsEnabled

        /// <summary>
        ///     Gets/sets the value that indicates if this voice is available or not.
        ///     In case there is a bad installation in the registry for the SAPI
        ///     voices, there might be an unwanted voice, this way, you can remove it
        ///     from the drop-down list.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return fIsEnabled;
            }

            set
            {
                fIsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        #endregion

        #region SSMLEnabled

        /// <summary>
        ///     Gets/sets wether SSML can be used for the voice.
        /// </summary>
        public bool SSMLEnabled
        {
            get
            {
                return fSSMLEnabled;
            }

            set
            {
                if (value != fSSMLEnabled)
                {
                    fSSMLEnabled = value;
                    if (value == false)
                    {
                        SendFormatString = AvailableFormatStrings[AvailableFormatStrings.Count - 1];

                            // select last format by default when there is no ssml available.
                    }
                    else
                    {
                        SendFormatString = AvailableFormatStrings[0];

                            // when enabling the ssml, the default format string is selected again.
                    }

                    OnPropertyChanged("SSMLEnabled");
                }
            }
        }

        #endregion

        #region SendFormatString

        /// <summary>
        ///     Gets/sets the formatting string that should be used for sending the
        ///     text to the TTS engine.
        /// </summary>
        public string SendFormatString
        {
            get
            {
                return fSendFormatString;
            }

            set
            {
                fSendFormatString = value;
                OnPropertyChanged("SendFormatString");
            }
        }

        #endregion

        #region AvailableFormatStrings

        /// <summary>
        ///     Gets a list of predefined format strings.
        /// </summary>
        public static System.Collections.Generic.List<string> AvailableFormatStrings
        {
            get
            {
                return fFormats;
            }
        }

        #endregion

        #region PreferedCharacter

        /// <summary>
        ///     Gets/sets the name of the character to use preferably when the voice
        ///     is selected (if no character selection was made).
        /// </summary>
        public string PreferedCharacter
        {
            get
            {
                return fPreferedCharacter;
            }

            set
            {
                fPreferedCharacter = value;
                OnPropertyChanged("PreferedCharacter");
            }
        }

        #endregion

        #region DisplayName

        /// <summary>
        ///     Gets/sets the name to display to the user (some of the tts voices have
        ///     long names)
        /// </summary>
        public string DisplayName
        {
            get
            {
                return fDisplayName;
            }

            set
            {
                fDisplayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        #endregion

        #region const

        /// <summary>The ssmlstring.</summary>
        private const string SSMLSTRING =
            "<?xml version='1.0'?><speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemalocation='http://www.w3.org/2001/10/synthesis http://www.w3.org/TR/speech-synthesis/synthesis.xsd' xml:lang='en-US'>{0}</speak>";

        /// <summary>The simple ssmlstring.</summary>
        private const string SimpleSSMLSTRING =
            "<?xml version='1.0'?><speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0'>{0}</speak>";

        /// <summary>The simple ssmlstrin g 2.</summary>
        private const string SimpleSSMLSTRING2 = "<?xml version='1.0'?><speak>{0}</speak>";

        /// <summary>The simple ssmlstrin g 3.</summary>
        private const string SimpleSSMLSTRING3 =
            "<speak xmlns='http://www.w3.org/2001/10/synthesis' version='1.0'>{0}</speak>";

        /// <summary>The simple ssmlstrin g 4.</summary>
        private const string SimpleSSMLSTRING4 = "<speak>{0}</speak>";

        /// <summary>The ssmlve r 2 long.</summary>
        private const string SSMLVER2Long =
            "<?xml version='1.0' encoding='ISO-8859-1'?><ssml:speak version='1.0' xmlns:ssml='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>{0}</ssml:speak>";

        /// <summary>The ssmlve r 2 long 2.</summary>
        private const string SSMLVER2Long2 =
            "<?xml version='1.0'?><ssml:speak version='1.0' xmlns:ssml='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>{0}</ssml:speak>";

        /// <summary>The ssmlve r 23.</summary>
        private const string SSMLVER23 =
            "<ssml:speak version='1.0' xmlns:ssml='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>{0}</ssml:speak>";

        /// <summary>The ssmlve r 24.</summary>
        private const string SSMLVER24 = "<ssml:speak>{0}</ssml:speak>";

        /// <summary>The ssmlshort.</summary>
        private const string SSMLSHORT = "{0}";

        #endregion

        #region fields

        /// <summary>The f ssml enabled.</summary>
        private bool fSSMLEnabled;

        /// <summary>The f send format string.</summary>
        private string fSendFormatString;

        /// <summary>The f prefered character.</summary>
        private string fPreferedCharacter;

        /// <summary>The f display name.</summary>
        private string fDisplayName;

        /// <summary>The f formats.</summary>
        private static readonly System.Collections.Generic.List<string> fFormats =
            new System.Collections.Generic.List<string>
                {
                    SSMLSTRING, 
                    SimpleSSMLSTRING, 
                    SimpleSSMLSTRING2, 
                    SimpleSSMLSTRING3, 
                    SimpleSSMLSTRING4, 
                    SSMLVER2Long, 
                    SSMLVER2Long2, 
                    SSMLVER23, 
                    SSMLVER24, 
                    SSMLSHORT
                };

        #endregion
    }
}