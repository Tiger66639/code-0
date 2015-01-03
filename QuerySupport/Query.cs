// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Query.cs" company="">
//   
// </copyright>
// <summary>
//   A sin that provides query features. It provides a way to compile source code that contains the query definition
//   and, if needed, a way to feed data into the query through an interface.
//   Note: source code is not stored by the query.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     A sin that provides query features. It provides a way to compile source code that contains the query definition
    ///     and, if needed, a way to feed data into the query through an interface.
    ///     Note: source code is not stored by the query.
    /// </summary>
    /// <remarks>
    ///     The code for a query consists out of the enirior of a function. There is no need to write the beginning and end
    ///     brackets or name.
    ///     This means that the source can only be 1 function. To get more functions, you can include other files (and or
    ///     modules).
    ///     The neurons for the entry point are stored in <see cref="Sin.ActionsForInput" />.
    /// </remarks>
    public class Query : Sin, IForEachSource
    {
        /// <summary>The f module.</summary>
        private Module fModule;

        /// <summary>The f module uid.</summary>
        private string fModuleUID = string.Empty; // stores the name of the module associated with this query.

        #region DataSource

        /// <summary>
        ///     Gets/sets the datasource to use.
        /// </summary>
        public IQueryPipe DataSource { get; set; }

        #endregion

        #region RenderTarget

        /// <summary>
        ///     Gets/sets the object that will save the data that was send to the output file. This is not required.
        /// </summary>
        public IRenderPipe RenderTarget { get; set; }

        #endregion

        #region Module

        /// <summary>
        ///     Gets the module associated with this query. It is used to store the compiled data. but no reference to the source
        ///     files.
        /// </summary>
        public Module Module
        {
            get
            {
                if (fModule == null)
                {
                    fModule = new Module(true);
                    if (string.IsNullOrEmpty(fModuleUID))
                    {
                        // check if the module has already been created, if so, reload the same module.
                        fModuleUID = System.Guid.NewGuid().ToString();

                            // create a unique name for the query, so it can be saved. This allows the user to create queries with the same name (no unique names required)
                    }

                    fModule.ID.Name = Text;
                    fModule.UID = fModuleUID;
                }

                return fModule;
            }
        }

        #endregion

        /// <summary>
        ///     Occurs when there is output sent to the query. This can be used for displaying or storing values.
        /// </summary>
        public event OutputEventHandler NeuronsOut;

        /// <summary>compiles the specified source and stores the compiled data in the database.</summary>
        /// <param name="source"></param>
        /// <param name="extraFiles">The extra Files.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Compile(string source, System.Collections.Generic.IList<string> extraFiles)
        {
            var iToCall = ActionsForInput;
            if (iToCall == null)
            {
                // if there is no entry point yet, create it now.
                iToCall = NeuronFactory.GetCluster();
                Brain.Current.Add(iToCall);
                ActionsForInput = iToCall;
            }
            else
            {
                using (IDListAccessor iChildren = iToCall.ChildrenW)
                    iChildren.Clear();

                        // always make certain that the cluster is empty. When the code items are used by other modules, they aren't deleted.
            }

            var iCompiler = new Parsers.NNLModuleCompiler();

            var iExtraFiles = new System.Collections.Generic.List<string>();

                // resolve any relative files (compared to the project).
            foreach (var i in extraFiles)
            {
                if (System.IO.Path.IsPathRooted(i))
                {
                    iExtraFiles.Add(i);
                }
                else if (string.IsNullOrEmpty(Brain.Current.Storage.DataPath) == false)
                {
                    var iPath = new System.Uri(i, System.UriKind.Relative);
                    var iTemp = new System.Uri(new System.Uri(Brain.Current.Storage.DataPath), iPath);
                    iExtraFiles.Add(iTemp.LocalPath);
                }
                else
                {
                    LogService.Log.LogWarning(
                        "Query", 
                        "Can't resolve relative path for additional files, the project has not yet been saved so the path is not yet known.");
                }
            }

            var iSource = new QueryCompilationSource(this, source, iExtraFiles, ActionsForInput);
            iCompiler.Compile(Text, iSource);
            return iCompiler.HasErrors;
        }

        /// <summary>unloads the compiled data.</summary>
        public void Unload()
        {
            var iCluster = ActionsForInput;
            if (iCluster != null)
            {
                if (Module != null)
                {
                    Parsers.NNLModuleCompiler.RemovePreviousDef(Module);
                    Brain.Current.Modules.Release(Module);
                    fModule = null;
                }

                ActionsForInput = null; // break the link, so we can check if we can delete the cluster as well
                if (BrainHelper.HasIncommingReferences(iCluster) == false)
                {
                    Brain.Current.Delete(iCluster);
                }
            }
        }

        /// <summary>
        ///     runs the query. Warning: this is done assynchronously.
        ///     Although this is a sin, no 'SinActivity is called, since the sin is already running a bunch of custom code.
        /// </summary>
        public void Run()
        {
            try
            {
                var iToCall = ActionsForInput;
                if (iToCall != null)
                {
                    var iProc = ProcessorFactory.GetProcessor();
                    iProc.CurrentSin = this; // we are calling it so, let the proc know this.
                    if (RenderTarget != null)
                    {
                        iProc.Finished += Proc_Finished;
                        RenderTarget.Open();
                    }

                    iProc.CallSingle(iToCall);
                }
                else
                {
                    LogService.Log.LogError(
                        "Query", 
                        "The query has no entry point. Please provide some code to be executed.");
                }
            }
            catch (System.Exception e)
            {
                LogService.Log.LogError("Query", e.ToString());
            }
        }

        /// <summary>Handles the Finished event of the processor.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Proc_Finished(object sender, System.EventArgs e)
        {
            if (RenderTarget != null)
            {
                RenderTarget.Close();
            }

            var iProc = (Processor)sender;
            iProc.Finished -= Proc_Finished; // release the event to make certain that there are no mem leaks.
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            if (fModule != null)
            {
                fModule.Flush();
            }

            if (DataSource != null)
            {
                DataSource.Flush();
            }

            if (RenderTarget != null)
            {
                RenderTarget.Flush();
            }
        }

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend"></param>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            if (NeuronsOut != null)
            {
                var iArgs = new OutputEventArgs();
                iArgs.Data = toSend;
                NeuronsOut(this, iArgs);
            }

            if (RenderTarget != null)
            {
                RenderTarget.Output(toSend);
            }
        }

        /// <summary>
        ///     Called when all the data of the sensory interface needs to be loaded into memory.
        /// </summary>
        public override void TouchMem()
        {
            base.TouchMem();
            Module.TouchMem();
            if (DataSource != null)
            {
                DataSource.TouchMem();
            }
        }

        /// <summary>Writes the neuron in version 1 format.</summary>
        /// <param name="writer">The writer.</param>
        protected override void WriteV1(System.IO.BinaryWriter writer)
        {
            base.WriteV1(writer);
            writer.Write(fModuleUID);
            if (DataSource != null)
            {
                writer.Write(DataSource.GetType().AssemblyQualifiedName);

                    // so we can recreate the datasource object. Make certain it includes the assemblyname as well, otherwise pipes in other assemblies don't load very well.
                var iTemp = new System.IO.MemoryStream();
                var iTempWriter = new System.IO.BinaryWriter(iTemp);

                    // write to a temp memory stream, so we can first determin the size of the datasource's data and store this, so in case we can't recreate the datasource, we don't get stuck with a corrupt db.
                DataSource.WriteV1(iTempWriter);
                writer.Write((System.Int32)iTemp.Length);
                writer.Write(iTemp.GetBuffer(), 0, (System.Int32)iTemp.Length); // write the data to the stream.
            }
            else
            {
                writer.Write(string.Empty); // write an empty string as the type name.
            }

            if (RenderTarget != null)
            {
                writer.Write(RenderTarget.GetType().AssemblyQualifiedName);

                    // so we can recreate the datasource object. Make certain it includes the assemblyname as well, otherwise pipes in other assemblies don't load very well.
                var iTemp = new System.IO.MemoryStream();
                var iTempWriter = new System.IO.BinaryWriter(iTemp);

                    // write to a temp memory stream, so we can first determin the size of the datasource's data and store this, so in case we can't recreate the datasource, we don't get stuck with a corrupt db.
                RenderTarget.WriteV1(iTempWriter);
                writer.Write((System.Int32)iTemp.Length);
                writer.Write(iTemp.GetBuffer(), 0, (System.Int32)iTemp.Length); // write the data to the stream.
            }
            else
            {
                writer.Write(string.Empty); // write an empty string as the type name.
            }
        }

        /// <summary>Reads the neuron in file version 1 format.</summary>
        /// <param name="reader">The reader.</param>
        /// <returns>The <see cref="LinkResolverData"/>.</returns>
        protected override LinkResolverData ReadV1(System.IO.BinaryReader reader)
        {
            var iRes = base.ReadV1(reader);
            fModuleUID = reader.ReadString(); // the name of the module
            var iTypeName = reader.ReadString();
            if (string.IsNullOrEmpty(iTypeName) == false)
            {
                var iNrBytes = reader.ReadInt32();
                var iData = new byte[iNrBytes];
                reader.Read(iData, 0, iNrBytes);
                var iStream = new System.IO.MemoryStream(iData);

                    // create a temp stream to make certain that the datasource can't screw up the database pointer (this would otherwise be a potential way to hack into the sytem).
                var iTempReader = new System.IO.BinaryReader(iStream);
                try
                {
                    DataSource = System.Activator.CreateInstance(System.Type.GetType(iTypeName)) as IQueryPipe;
                    DataSource.ReadV1(iTempReader);
                }
                catch
                {
                    LogService.Log.LogError("Query", "Failed to load data source: " + iTypeName);
                }
            }

            return iRes;
        }

        /// <summary>
        ///     Calls the session stop event. Happens when the sin gets deleted from the network. This is for dynamic sins like
        ///     text channels.
        ///     Need to make certain that the
        /// </summary>
        public override void CallSinDestroyEvent()
        {
            base.CallSinDestroyEvent();
            Unload();
        }

        #region IForEachSource Members

        /// <summary>Gets the enumerator that can be used to get the data source items.</summary>
        /// <returns>The <see cref="IEnumerator"/>.</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> GetEnumerator()
        {
            if (DataSource != null)
            {
                return DataSource.GetEnumerator();
            }

            return null;
        }

        /// <summary>tries to duplicate the enumerator. When it is impossible to return a duplicate, its'
        ///     ok to return a new enum that goes back to the start (a warning should be rendered that splits are
        ///     not supported in this case).</summary>
        /// <param name="Enum">the enumerator to duplicate.</param>
        /// <returns>a new enumerator</returns>
        public System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Duplicate(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            return DataSource.Duplicate(Enum);
        }

        /// <summary>moves the enumerator till the end, possibly closing the datasource.
        ///     This is used for a 'break' statement.</summary>
        /// <param name="Enum">The enum to move passed the end.</param>
        public void GotoEnd(System.Collections.Generic.IEnumerator<System.Collections.Generic.IEnumerable<Neuron>> Enum)
        {
            DataSource.GotoEnd(Enum);
        }

        #endregion
    }
}