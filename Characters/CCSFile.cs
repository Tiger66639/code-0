// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CCSFile.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the content of a conversave character studio file, so we can
//   import it.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     Defines the content of a conversave character studio file, so we can
    ///     import it.
    /// </summary>
    public class CCSFile : System.Xml.Serialization.IXmlSerializable
    {
        #region fields

        /// <summary>The f import to.</summary>
        private readonly CharacterBase fImportTo;

        /// <summary>The f file name.</summary>
        private readonly string fFileName;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CCSFile"/> class.</summary>
        /// <param name="importTo">The import to.</param>
        /// <param name="fileName">The file Name.</param>
        public CCSFile(CharacterBase importTo, string fileName)
        {
            fImportTo = importTo;
            fFileName = fileName;
        }

        /// <summary>Imports the CCS file into the character.</summary>
        /// <param name="importTo">The import to.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void ImportCCSFile(CharacterBase importTo, string fileName)
        {
            var iParser = new CCSFile(importTo, fileName);

            using (var iFile = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                var iSettings = new System.Xml.XmlReaderSettings();
                iSettings.IgnoreComments = true;
                iSettings.IgnoreProcessingInstructions = true;
                iSettings.IgnoreWhitespace = true;

                using (var iReader = System.Xml.XmlReader.Create(iFile, iSettings))
                {
                    if (iReader.IsStartElement())
                    {
                        iParser.ReadXml(iReader);
                    }
                }
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>Can't yet export to this file format.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the
        ///     IXmlSerializable interface, you should return <see langword="null" />
        ///     (Nothing in Visual Basic) from this method, and instead, if specifying
        ///     a custom schema is required, apply the
        ///     <see cref="System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="System.Xml.Schema.XmlSchema" /> that describes the XML representation of
        ///     the object that is produced by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the
        ///     <see cref="System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
        /// </returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var iName = reader.Name;
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            ReadCharInfo(reader);
            ReadBackgrounds(reader);
            ReadAnimations(reader);
            ReadVisemeGroups(reader);
            ReadIdleLevels(reader);
            if (reader.EOF == false)
            {
                ReadBackgroundAnimations(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read backgrounds.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadBackgrounds(System.Xml.XmlReader reader)
        {
            if (reader.Name == "Background")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "Background")
                {
                    var iSource = XmlStore.ReadElement<string>(reader, "ImageResource");
                    fImportTo.LoadBackground(MakeAbsolute(iSource));
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read background animations.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadBackgroundAnimations(System.Xml.XmlReader reader)
        {
            if (reader.Name == "BackgroundAnimations")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                while (reader.Name != "BackgroundAnimations")
                {
                    ReadBackgroundAnim(reader);
                }

                reader.ReadEndElement();
            }
        }

        /// <summary>The read background anim.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadBackgroundAnim(System.Xml.XmlReader reader)
        {
            var iReturnAnimName = string.Empty;
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iRes = new Animation();
            IdleLevel iIdleTimeInfo = null;

                // in case the background animation needs to use timer info similar to the idle handler.
            ReadAnimationFrames(reader, iRes);
            iRes.Name = XmlStore.ReadElement<string>(reader, "Name");

            iRes.LoopStyle =
                (AnimationLoopStyle)
                System.Enum.Parse(typeof(AnimationLoopStyle), XmlStore.ReadElement<string>(reader, "LoopStyle"));
            if (iRes.LoopStyle != AnimationLoopStyle.VarTimer)
            {
                iRes.VariableSpeed = XmlStore.ReadElement<bool>(reader, "VariableSpeed");
            }
            else
            {
                iRes.MinStartDelay = XmlStore.ReadElement<int>(reader, "MinStartDelay");
                iRes.MaxStartDelay = XmlStore.ReadElement<int>(reader, "MaxStartDelay");
            }

            int? iZIndex = null;
            if (reader.Name == "ZIndex")
            {
                iZIndex = XmlStore.ReadElement<int>(reader, "ZIndex");
            }

            iRes.ZIndex = iZIndex;
            reader.ReadEndElement();
            if (iIdleTimeInfo == null)
            {
                fImportTo.LoadBackgroundAnimation(iRes);
            }
        }

        /// <summary>The read idle levels.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadIdleLevels(System.Xml.XmlReader reader)
        {
            if (reader.Name == "IdleLevels")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    fImportTo.LoadIdleLevel(null);

                        // to indicate that there are no idle levels specified, so default ones can be made
                    return;
                }

                while (reader.Name != "IdleLevels")
                {
                    ReadIdleLevel(reader);
                }

                reader.ReadEndElement();
            }
            else
            {
                fImportTo.LoadIdleLevel(null);

                    // to indicate that there are no idle levels specified, so default ones can be made
            }
        }

        /// <summary>The read idle level.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadIdleLevel(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iNew = new IdleLevel();
            ReadIdleValues(reader, iNew);
            ReadIdleNames(reader, iNew);
            reader.ReadEndElement();
            fImportTo.LoadIdleLevel(iNew);
        }

        /// <summary>The read idle values.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="iNew">The i new.</param>
        private void ReadIdleValues(System.Xml.XmlReader reader, IdleLevel iNew)
        {
            iNew.MinStartDelay = XmlStore.ReadElement<int>(reader, "MinStartDelay");
            iNew.MaxStartDelay = XmlStore.ReadElement<int>(reader, "MaxStartDelay");
            iNew.MinDuration = XmlStore.ReadElement<int>(reader, "MinDuration");
            iNew.MaxDuration = XmlStore.ReadElement<int>(reader, "MaxDuration");
            iNew.MinInterval = XmlStore.ReadElement<int>(reader, "MinInterval");
            iNew.MaxInterval = XmlStore.ReadElement<int>(reader, "MaxInterval");
        }

        /// <summary>The read idle names.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="level">The level.</param>
        private void ReadIdleNames(System.Xml.XmlReader reader, IdleLevel level)
        {
            level.AnimationNames = new System.Collections.Generic.List<string>();
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "AnimationNames")
            {
                var iName = XmlStore.ReadElement<string>(reader, "AnimationName");
                level.AnimationNames.Add(iName);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read char info.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadCharInfo(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fImportTo.Name = XmlStore.ReadElement<string>(reader, "Name");
            fImportTo.Author = XmlStore.ReadElement<string>(reader, "Author");
            fImportTo.Copyright = XmlStore.ReadElement<string>(reader, "Copyright");
            fImportTo.License = XmlStore.ReadElement<string>(reader, "License");
            fImportTo.AuthorWebsite = XmlStore.ReadElement<string>(reader, "AuthorWebsite");

            fImportTo.CreationDate = XmlStore.ReadElement<System.DateTime>(reader, "CreationDate");
            fImportTo.LastUpdateDate = XmlStore.ReadElement<System.DateTime>(reader, "LastUpdateDate");
            ScanRating(reader);
            ReadOnlineOptions(reader);
            var iDummy = string.Empty;
            XmlStore.TryReadElement(reader, "RatingLevel", ref iDummy); // can be missing
            XmlStore.TryReadElement(reader, "RatingLevel", ref iDummy);
            XmlStore.TryReadElement(reader, "Sexual", ref iDummy); // some doubles are possible in the file, so skip.
            XmlStore.TryReadElement(reader, "Violence", ref iDummy);
            XmlStore.TryReadElement(reader, "Other", ref iDummy);
            XmlStore.TryReadElement(reader, "Description", ref iDummy);

            reader.ReadEndElement();
        }

        /// <summary>The read online options.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadOnlineOptions(System.Xml.XmlReader reader)
        {
            if (reader.Name == "OnlineOptions")
            {
                var wasEmpty = reader.IsEmptyElement;
                reader.Read();
                if (wasEmpty)
                {
                    return;
                }

                XmlStore.ReadElement<string>(reader, "OnlineCharacterBaseUrl");
                XmlStore.ReadElement<string>(reader, "PreferredWidth");
                XmlStore.ReadElement<string>(reader, "PreferredHeight");
                reader.ReadEndElement();
            }
        }

        /// <summary>Scans through the 'rating' folder</summary>
        /// <param name="reader">The reader.</param>
        private void ScanRating(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            fImportTo.Rating = XmlStore.ReadElement<string>(reader, "Rating");
            fImportTo.Sexual = XmlStore.ReadElement<string>(reader, "Sexual");
            fImportTo.Violence = XmlStore.ReadElement<string>(reader, "Violence");
            fImportTo.Other = XmlStore.ReadElement<string>(reader, "Other");
            fImportTo.Description = XmlStore.ReadElement<string>(reader, "Description");
            reader.ReadEndElement();
        }

        /// <summary>The read viseme groups.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadVisemeGroups(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iCount = 0;
            while (reader.Name != "VisemeGroups")
            {
                ReadVisemeGroup(reader, iCount++);
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads a single viseme group. When <paramref name="count"/> is bigger
        ///     then 0, we don't ask the engine to store the data cause we can only
        ///     handle 1 phiseme set: the first.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="count">The count.</param>
        private void ReadVisemeGroup(System.Xml.XmlReader reader, int count)
        {
            int? iZIndex = null;
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            XmlStore.ReadElement<string>(reader, "Name");
            if (reader.Name == "ZIndex")
            {
                iZIndex = XmlStore.ReadElement<int>(reader, "ZIndex");
            }

            ReadVisemeImages(reader, count);
            if (iZIndex.HasValue)
            {
                fImportTo.SetVisemesZIndex(iZIndex.Value);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read viseme images.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="count">The count.</param>
        private void ReadVisemeImages(System.Xml.XmlReader reader, int count)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "VisemeImages")
            {
                ReadViseme(reader, count);
            }

            reader.ReadEndElement();
        }

        /// <summary>Reads a single viseme image ans instructs the engine to store this if<paramref name="count"/> =0.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="count">The count.</param>
        private void ReadViseme(System.Xml.XmlReader reader, int count)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iIndex = XmlStore.ReadElement<int>(reader, "VisemeIndex");
            var iSource = XmlStore.ReadElement<string>(reader, "ImageResource");
            iSource = MakeAbsolute(iSource);

            fImportTo.LoadViseme(iIndex, iSource);
            reader.ReadEndElement();
        }

        /// <summary>The read animations.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadAnimations(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "Animations")
            {
                ReadAnimation(reader);
            }

            reader.ReadEndElement();
        }

        /// <summary>The read animation.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadAnimation(System.Xml.XmlReader reader)
        {
            var iTemp = false;
            var iReturnAnimName = string.Empty;
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            var iRes = new Animation();
            iRes.UseSoftStop = true;

                // for the time being, use a soft stop for all animations (fast forward to closest end point).
            ReadAnimationFrames(reader, iRes);
            iRes.EnableFrameSpeaking = XmlStore.ReadElement<bool>(reader, "EnableFrameSpeaking");
            XmlStore.TryReadElement(reader, "HoldLastFrameForSpeak", ref iTemp);
            iRes.HoldLastFrameForSpeak = iTemp;
            iRes.FirstFrameUnderlay = XmlStore.ReadElement<bool>(reader, "FirstFrameUnderlay");
            ReadBackgroundSuppress(reader, iRes);
            iRes.Name = XmlStore.ReadElement<string>(reader, "Name");
            XmlStore.TryReadElement(reader, "ReturnAnimationName", ref iReturnAnimName);
            iRes.ReturnAnimationName = iReturnAnimName;
            int? iZIndex = null;
            if (reader.Name == "ZIndex")
            {
                iZIndex = XmlStore.ReadElement<int>(reader, "ZIndex");
            }

            iRes.ZIndex = iZIndex;
            reader.ReadEndElement();
            fImportTo.LoadAnimation(iRes);
        }

        /// <summary>The read background suppress.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="res">The res.</param>
        private void ReadBackgroundSuppress(System.Xml.XmlReader reader, Animation res)
        {
            var iBackgrounds = new System.Collections.Generic.List<string>();
            while (reader.Name == "BackgroundSuppress")
            {
                iBackgrounds.Add(XmlStore.ReadElement<string>(reader, "BackgroundSuppress"));
            }

            if (iBackgrounds.Count > 0)
            {
                res.BackgroundSuppress = iBackgrounds;
            }
        }

        /// <summary>The read animation frames.</summary>
        /// <param name="reader">The reader.</param>
        /// <param name="res">The res.</param>
        private void ReadAnimationFrames(System.Xml.XmlReader reader, Animation res)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            while (reader.Name != "AnimationFrames")
            {
                res.Frames.Add(ReadAnimationFrame(reader));
            }

            reader.ReadEndElement();
        }

        /// <summary>The read animation frame.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="AnimationFrame"/>.</returns>
        private AnimationFrame ReadAnimationFrame(System.Xml.XmlReader reader)
        {
            var iFrame = new AnimationFrame();
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return iFrame;
            }

            iFrame.Duration = XmlStore.ReadElement<int>(reader, "Duration") * 10;

                // we need it in milliseconds for the timer.
            iFrame.ImageResource = XmlStore.ReadElement<string>(reader, "ImageResource");
            iFrame.ImageResource = MakeAbsolute(iFrame.ImageResource);
            XmlStore.ReadElement<string>(reader, "VisemeGroupName");
            reader.ReadEndElement();
            return iFrame;
        }

        /// <summary>The make absolute.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private string MakeAbsolute(string value)
        {
            if (System.IO.Path.IsPathRooted(value) == false && string.IsNullOrEmpty(value) == false)
            {
                var iPath = new System.Uri(value, System.UriKind.Relative); // need to make the path absolute.
                var iTemp = new System.Uri(new System.Uri(System.IO.Path.GetFullPath(fFileName)), iPath);
                return iTemp.LocalPath;
            }

            return null;
        }

        #endregion
    }
}