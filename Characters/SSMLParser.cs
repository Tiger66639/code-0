// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SSMLParser.cs" company="">
//   
// </copyright>
// <summary>
//   Provides parsing functionality to convert an ssml xml string into a flat
//   text string with all the xml removed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     Provides parsing functionality to convert an ssml xml string into a flat
    ///     text string with all the xml removed.
    /// </summary>
    public class SSMLParser
    {
        /// <summary>The f reader.</summary>
        private System.Xml.XmlReader fReader;

        /// <summary>The f result.</summary>
        private readonly System.Text.StringBuilder fResult = new System.Text.StringBuilder();

        /// <summary>Converts the SSML string to text with all the xml formatting removed.</summary>
        /// <param name="iXmlText">The i XML text.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string ConvertSSMLToText(string iXmlText)
        {
            var iParse = new SSMLParser();
            try
            {
                using (var iXml = System.Xml.XmlReader.Create(new System.IO.StringReader(iXmlText)))
                {
                    iParse.fReader = iXml;
                    iParse.ConvertSSMLToText();
                    return iParse.fResult.ToString();
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Convert SSML to text", e.Message);
                return iParse.fResult + "(" + iXmlText + ")";
            }
        }

        /// <summary>
        ///     Converts the SSML to text.
        /// </summary>
        private void ConvertSSMLToText()
        {
            if (fReader.IsStartElement())
            {
                fReader.Read();
                CollectValue();
                while (fReader.EOF == false && fReader.Name != "speak")
                {
                    if (fReader.Name == "paragraph" || fReader.Name == "p")
                    {
                        ReadParagraph();
                    }
                    else
                    {
                        ReadParagraphContent();
                    }

                    if (fReader.EOF == false)
                    {
                        CollectValue();
                    }
                }
            }
        }

        /// <summary>
        ///     Reads the content of the paragraph.
        /// </summary>
        private void ReadParagraphContent()
        {
            if (fReader.Name == "sentence" || fReader.Name == "s")
            {
                ReadSentence();
            }
            else
            {
                ReadSentenceContent();
            }
        }

        /// <summary>
        ///     Reads the content of the sentence.
        /// </summary>
        private void ReadSentenceContent()
        {
            if (fReader.Name == "say-as" || fReader.Name == "phoneme" || fReader.Name == "sub"
                || fReader.Name == "emphasis")
            {
                ReadContentOfElement();
            }
            else if (fReader.Name == "break" || fReader.Name == "prosody" || fReader.Name == "mark")
            {
                ReadEmpty();
            }
            else
            {
                fResult.Append(fReader.ReadOuterXml());

                    // if something else is included that's not recognized, copy it over. If we don't do this, we get in an ethernal loop.
            }
        }

        /// <summary>
        ///     Reads the empty.
        /// </summary>
        private void ReadEmpty()
        {
            var wasEmpty = fReader.IsEmptyElement;
            var iName = fReader.Name;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (fReader.Name != iName && fReader.EOF == false)
            {
                fReader.Read();
            }

            fReader.ReadEndElement();
        }

        /// <summary>
        ///     Reads the sentence.
        /// </summary>
        private void ReadSentence()
        {
            var wasEmpty = fReader.IsEmptyElement;
            var iName = fReader.Name;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            CollectValue();
            while (fReader.Name != iName && fReader.EOF == false)
            {
                ReadSentenceContent();
                if (fReader.EOF == false)
                {
                    CollectValue();
                }
            }

            fReader.ReadEndElement();
        }

        /// <summary>
        ///     Collects the value.
        /// </summary>
        private void CollectValue()
        {
            if (fReader.HasValue)
            {
                fResult.Append(fReader.Value);
                if (fReader.EOF == false)
                {
                    fReader.Read();
                }
                else
                {
                    throw new System.InvalidOperationException("The tag was not closed correctly!");
                }
            }
        }

        /// <summary>
        ///     Reads the paragraph.
        /// </summary>
        private void ReadParagraph()
        {
            var wasEmpty = fReader.IsEmptyElement;
            var iName = fReader.Name;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            CollectValue();
            while (fReader.Name != iName && fReader.EOF == false)
            {
                ReadParagraphContent();
                if (fReader.EOF == false)
                {
                    CollectValue();
                }
            }

            fReader.ReadEndElement();
        }

        /// <summary>
        ///     Reads the content of element.
        /// </summary>
        private void ReadContentOfElement()
        {
            var wasEmpty = fReader.IsEmptyElement;
            var iName = fReader.Name;
            fReader.Read();
            if (wasEmpty)
            {
                return;
            }

            CollectValue();
            fReader.ReadEndElement();
        }
    }
}