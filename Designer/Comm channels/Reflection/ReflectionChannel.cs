// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionChannel.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data for managing a <see cref="ReflectionSin" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    using System.Linq;

    /// <summary>
    ///     Contains all the data for managing a <see cref="ReflectionSin" />
    /// </summary>
    public class ReflectionChannel : CommChannel
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="ReflectionChannel"/> class. 
        ///     Initializes a new instance of the <see cref="ReflectionChannel"/>
        ///     class.</summary>
        public ReflectionChannel()
        {
            Assemblies = new Data.ObservedCollection<AssemblyData>(this);
            OpCodes = new Data.ObservedCollection<DebugNeuron>(this);
        }

        #endregion

        #region prop

        #region Assemblies

        /// <summary>
        ///     Gets the list of available assemblies
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Data.ObservedCollection<AssemblyData> Assemblies { get; private set; }

        #endregion

        #region AssemblyNames

        /// <summary>
        ///     Gets/sets the list of assemblies that is loaded.
        /// </summary>
        /// <remarks>
        ///     This is primarely used for xml streaming.
        /// </remarks>
        public System.Collections.Generic.List<string> AssemblyFiles
        {
            get
            {
                var iSin = (ReflectionSin)Sin;
                return (from i in Assemblies select i.Path).ToList();
            }

            set
            {
                Assemblies.Clear();
                foreach (var i in value)
                {
                    Assemblies.Add(new AssemblyData(i, this));
                }
            }
        }

        #endregion

        #region OpCodes

        /// <summary>
        ///     Gets the list of neurons used as op codes in the form of debug
        ///     neurons.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public Data.ObservedCollection<DebugNeuron> OpCodes { get; private set; }

        #endregion

        #region OpCodeIds

        /// <summary>
        ///     Gets/sets the list of neuron id's used as opcodes by the current
        ///     channel
        /// </summary>
        /// <remarks>
        ///     Primarely for streaming.
        /// </remarks>
        public System.Collections.Generic.List<ulong> OpCodeIds
        {
            get
            {
                return (from i in OpCodes select i.Item.ID).ToList();
            }

            set
            {
                OpCodes.Clear();
                if (value != null)
                {
                    foreach (var i in value)
                    {
                        Neuron iFound;
                        if (Brain.Current.TryFindNeuron(i, out iFound))
                        {
                            OpCodes.Add(new DebugNeuron(iFound));
                        }
                        else
                        {
                            LogService.Log.LogError(
                                "ReflectionChannel.OpCodeId's", 
                                string.Format("removed opcode from list: id '{0}' not found in brain.", i));
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        ///     Loads all the op codes.
        /// </summary>
        internal void LoadOpCodes()
        {
            var iSin = (ReflectionSin)Sin;
            OpCodes.Clear();
            foreach (var i in iSin.CreateOpcodes())
            {
                TextNeuron iText;
                using (var iList = i.Children)
                    iText =
                        (from ii in iList.ConvertTo<Neuron>() where ii is TextNeuron select (TextNeuron)ii)
                            .FirstOrDefault(); // we get the text neuron so we can build a display tag.
                var iWrap = new DebugNeuron(i);
                OpCodes.Add(iWrap);
                iWrap.NeuronInfo.DisplayTitle = iText.Text;
            }
        }

        /// <summary>
        ///     Unloads all the op codes.
        /// </summary>
        internal void UnloadOpCodes()
        {
            LogService.Log.LogError(
                "ReflectionChannel.UnloadOpCodes", 
                "There's still a bug here: the opcodes are created as objects, but deleted as neurons: the clusters and textneurons still remain in the brain!!");
            foreach (var i in OpCodes)
            {
                BrainHelper.DeleteObject((NeuronCluster)i.Item);
            }

            OpCodes.Clear();
        }

        #endregion

        #region overides

        /// <summary>
        ///     Called when the visibility of the commchannel is changed.
        /// </summary>
        protected internal override void UpdateOpenDocuments()
        {
            var iSin = (ReflectionSin)Sin;
            base.UpdateOpenDocuments();
            if (BrainData.Current != null && BrainData.Current.DesignerData != null)
            {
                // when designerData is not set, not all the data is loaded yet. + this is also called when the project is cleared, so that comm channels get a change of clearing any 'global stuff', like the character windows of the chatbots.
                if (IsVisible)
                {
                    // register the eventhandler + check if there were any new libs loaded.
                    iSin.LibraryAdded += iSin_LibraryAdded;
                    iSin.LibraryRemoved += iSin_LibraryRemoved;
                    var iCount = 0;
                    while (iCount < Assemblies.Count)
                    {
                        var i = Assemblies[iCount];
                        if (
                            iSin.FunctionAssemblies.ContainsKey(
                                ReflectionSin.GetRelativePath(i.Assembly.Location).ToLower()) == false)
                        {
                            // the assembly is no longer in the list.
                            Assemblies.RemoveAt(iCount);
                        }
                        else
                        {
                            i.IsAssemblyLoaded = true; // make certain that all assemblies are unloaded.
                            iCount++;
                        }
                    }

                    if (Assemblies.Count != iSin.FunctionAssemblies.Count)
                    {
                        // don't need to do this if the nr of items is the same (if all in assemblyList are in the sin and sin has no more items, than both lists are the same.
                        foreach (var i in iSin.FunctionAssemblies)
                        {
                            var iFound =
                                (from u in Assemblies
                                 where ReflectionSin.GetRelativePath(u.Assembly.Location).ToLower() == i.Key
                                 select u).FirstOrDefault();
                            if (iFound == null)
                            {
                                var iData = new AssemblyData(i.Key, this);
                                Assemblies.Add(iData);
                                iData.IsAssemblyLoaded = true;
                            }
                        }
                    }
                }
                else
                {
                    iSin.LibraryAdded -= iSin_LibraryAdded; // make certain that there are no mem wholes.
                    iSin.LibraryRemoved -= iSin_LibraryRemoved;
                    foreach (var i in Assemblies)
                    {
                        i.IsAssemblyLoaded = false; // make certain that all assemblies are unloaded.
                    }
                }
            }
            else if (iSin != null)
            {
                iSin.LibraryAdded -= iSin_LibraryAdded; // make certain that there are no mem wholes.
                iSin.LibraryRemoved -= iSin_LibraryRemoved;
            }
        }

        /// <summary>Called when the lib is removed from the sin.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iSin_LibraryRemoved(object sender, LibEventargs e)
        {
            var iCount = 0;
            while (iCount < Assemblies.Count)
            {
                if (Assemblies[iCount].Assembly == e.Value)
                {
                    if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                    {
                        // can only add from ui thread.
                        Assemblies.RemoveAt(iCount);
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(
                            new System.Func<AssemblyData, bool>(Assemblies.Remove), 
                            Assemblies[iCount]); // do async
                    }

                    break;
                }

                iCount++;
            }
        }

        /// <summary>Checks if the specified library is loaded in the channel, if not, it
        ///     is added.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void iSin_LibraryAdded(object sender, LibEventargs e)
        {
            var iFound = (from u in Assemblies where u.Assembly == e.Value select u).FirstOrDefault();
            if (iFound == null)
            {
                if (System.Threading.Thread.CurrentThread == System.Windows.Application.Current.Dispatcher.Thread)
                {
                    // can only add from ui thread.
                    AddAssembly(e.Location);
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                        new System.Action<string>(AddAssembly), 
                        e.Value); // do async
                }
            }
            else
            {
                iFound.Path = e.Location; // update location, so it's relative.
            }
        }

        /// <summary>The add assembly.</summary>
        /// <param name="name">The name.</param>
        private void AddAssembly(string name)
        {
            var iData = new AssemblyData(name, this);
            Assemblies.Add(iData);
            iData.IsAssemblyLoaded = IsVisible;
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <remarks>When streaming to a module (for export), we do a mapping, to the index
        ///     of the neuron in the module that is currently being exported, and off
        ///     course visa versa, when reading from a module.</remarks>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteStartElement("Assemblies");
            foreach (var i in AssemblyFiles)
            {
                XmlStore.WriteElement(writer, "Name", i);
            }

            writer.WriteEndElement();
            XmlStore.WriteIDList(writer, "OpCodeIds", OpCodeIds);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <remarks>Descendents need to perform mapping between module index and neurons
        ///     when importing from modules.</remarks>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object is
        ///     deserialized.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            XmlStore.ReadList(reader, "Assemblies", ReadListEntry);
            XmlStore.ReadIDList(reader, "OpCodeIds", OpCodeIds);
        }

        /// <summary>Reads the list entry.</summary>
        /// <param name="reader">The reader.</param>
        private void ReadListEntry(System.Xml.XmlReader reader)
        {
            var iName = XmlStore.ReadElement<string>(reader, "Name");
            var iSin = (ReflectionSin)Sin;
            var iNew = new AssemblyData(iSin.GetAbsPath(iName), this);
            Assemblies.Add(iNew);
        }

        #endregion
    }
}