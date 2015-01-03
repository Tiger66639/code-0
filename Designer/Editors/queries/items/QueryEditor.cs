// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryEditor.cs" company="">
//   
// </copyright>
// <summary>
//   an editor for queries. provides a way to store the text, compile the
//   source, run the query and make the result visible.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     an editor for queries. provides a way to store the text, compile the
    ///     source, run the query and make the result visible.
    /// </summary>
    public class QueryEditor : NeuronEditor
    {
        /// <summary>
        ///     refreshes the list so that it gets rerendered? could also be done by
        ///     walking through each item and refreshing the
        /// </summary>
        internal void RefreshOutput()
        {
            var iOutput = (System.Collections.ObjectModel.ObservableCollection<QueryResultLine>)Output;
            fOutput = null;
            OnPropertyChanged("Output");
            fOutput = iOutput;
            OnPropertyChanged("Output");
        }

        /// <summary>Gets the size used by the columns except the last one and returns the
        ///     last col.</summary>
        /// <param name="lastCol">The last col.</param>
        /// <returns>The <see cref="double"/>.</returns>
        public double GetUsedSizeAndLastCol(out QueryColumn lastCol)
        {
            lastCol = null;
            double iRes = 0;
            foreach (var i in Columns)
            {
                if (i.Index == Columns.Count - 1)
                {
                    lastCol = i;
                }
                else
                {
                    iRes += i.Width;
                }
            }

            return iRes;
        }

        /// <summary>
        ///     runs the query.
        /// </summary>
        public void Run()
        {
            lock (fTempList)

                // when clearing hte output list, lock the templist cause there could still be another query running?
                Output.Clear();
            var iQuery = (Queries.Query)Item;
            Compile();
            WindowMain.TryStartQuery(iQuery);
        }

        /// <summary>
        ///     compiles the source and makes certain that the source is stored in the
        ///     neurondata object.
        /// </summary>
        public void Compile()
        {
            Flush();
            var iQuery = (Queries.Query)Item;
            if (NeedsRecompile)
            {
                NeedsRecompile = iQuery.Compile(Source, AdditionalFiles);
            }
        }

        /// <summary>
        ///     <para>
        ///         Deletes all the neurons on the editor that aren't referenced anywhere
        ///         else, if appropriate for the editor. This is called when the editor is
        ///         deleted from the project. Usually, the user will expect unused data to
        ///         get removed as well. The neuron that the editor wraps, is always
        ///         deleted.
        ///     </para>
        ///     <para>
        ///         before deleting the query, make certain that it is unloaded.
        ///     </para>
        /// </summary>
        public override void DeleteEditor()
        {
            var iQuery = (Queries.Query)Item;
            if (iQuery != null)
            {
                iQuery.Unload(); // unload the module, so we can delete the neuron
            }

            base.DeleteEditor();
        }

        #region fields

        /// <summary>The uiupdatedelay.</summary>
        private const int UIUPDATEDELAY = 1000; // nr of milliseconds that the ui will be updated from teh query.

        /// <summary>The f source editor height.</summary>
        private System.Windows.GridLength fSourceEditorHeight = new System.Windows.GridLength(
            1, 
            System.Windows.GridUnitType.Star);

        /// <summary>The f output.</summary>
        private System.Collections.ObjectModel.ObservableCollection<QueryResultLine> fOutput =
            new System.Collections.ObjectModel.ObservableCollection<QueryResultLine>();

        /// <summary>The f source.</summary>
        private string fSource;

        /// <summary>The f is changed.</summary>
        private bool fIsChanged; // so we can keep track of when we need to flush data to the db.

        /// <summary>The f is running result.</summary>
        private bool fIsRunningResult;

        /// <summary>The f needs recompile.</summary>
        private bool fNeedsRecompile;

        /// <summary>The f temp list.</summary>
        private readonly System.Collections.Generic.List<QueryResultLine> fTempList =
            new System.Collections.Generic.List<QueryResultLine>();

                                                                          // while the query runs, we first collect the data here, until enough items have been found to update the ui. This is to make certain taht the UI isn't over-taxed while the query runs.

        /// <summary>The f columns.</summary>
        private System.Collections.ObjectModel.ObservableCollection<QueryColumn> fColumns;

        /// <summary>The f timer.</summary>
        private System.Windows.Threading.DispatcherTimer fTimer;

                                                         // makes certain that the templist gets copied to the endresult list.

        /// <summary>The f data sources.</summary>
        private static System.Collections.Generic.List<QueryDataSource> fDataSources;

        /// <summary>The f render targets.</summary>
        private static System.Collections.Generic.List<RenderDataSource> fRenderTargets;

        /// <summary>The f selected data source.</summary>
        private QueryDataSource fSelectedDataSource;

        /// <summary>The f selected render target.</summary>
        private RenderDataSource fSelectedRenderTarget;

        /// <summary>The f additional files.</summary>
        private System.Collections.ObjectModel.ObservableCollection<string> fAdditionalFiles;

        #endregion

        #region ctor

        /// <summary>Initializes a new instance of the <see cref="QueryEditor"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        public QueryEditor(Queries.Query toWrap)
            : base(toWrap)
        {
            IsLoading = false;
        }

        /// <summary>Handles the CollectionChanged event of the<see cref="fAdditionalFiles"/> control. Need to make certain that the
        ///     query recompiles when the files are changed.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void fAdditionalFiles_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (IsLoading == false)
            {
                // while loading, we are also changing the list.
                NeedsRecompile = true;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryEditor" /> class.
        /// </summary>
        public QueryEditor()
        {
            IsLoading = false;
        }

        /// <summary>Handles the CollectionChanged event of the <see cref="Columns"/>
        ///     control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance
        ///     containing the event data.</param>
        private void Columns_CollectionChanged(
            object sender, 
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // we set the owner here so that we don't continuously have to think about doing this whenever adding columns.
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                // we set the owner when adding, not for removing, cause we can't do anythign about a reset anyway. 
                foreach (QueryColumn i in e.NewItems)
                {
                    i.Owner = this;
                }
            }

            OnPropertyChanged("ColumnCount");
        }

        #endregion

        #region prop

        #region Icon

        /// <summary>
        ///     Gets the resource path to the icon that should be used for this
        ///     editor. This is usually class specific. start with /
        /// </summary>
        public override string Icon
        {
            get
            {
                return "/Images/queries/Query_enabled.png";
            }
        }

        #endregion

        #region DocumentInfo

        /// <summary>
        ///     Gets or sets the document info.
        /// </summary>
        /// <value>
        ///     The document info.
        /// </value>
        public override string DocumentInfo
        {
            get
            {
                return "query: " + Name;
            }
        }

        #endregion

        #region DocumentType

        /// <summary>
        ///     Gets or sets the type of the document.
        /// </summary>
        /// <value>
        ///     The type of the document.
        /// </value>
        public override string DocumentType
        {
            get
            {
                return "queries";
            }
        }

        #endregion

        #region NeedsRecompile

        /// <summary>
        ///     Gets a value indicating whether a recompile is required or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [needs recompile]; otherwise, <c>false</c> .
        /// </value>
        public bool NeedsRecompile
        {
            get
            {
                return fNeedsRecompile;
            }

            set
            {
                fNeedsRecompile = value;
                OnPropertyChanged("NeedsRecompile");
                fIsChanged = true;
            }
        }

        #endregion

        #region Source

        /// <summary>
        ///     Gets/sets the source text of the query.
        /// </summary>
        public string Source
        {
            get
            {
                return fSource;
            }

            set
            {
                if (fSource != value)
                {
                    fSource = value;
                    OnPropertyChanged("Source");
                    ProjectManager.Default.ProjectChanged = true;

                        // need to indicate that the project has changed. need to do it like this cause we aren't using the global undo system but the local textbox undo for the source, so the change doesn't register otherwise..
                    fIsChanged = true;
                    NeedsRecompile = true;
                }
            }
        }

        #endregion

        #region AdditionalFiles

        /// <summary>
        ///     Gets the list of additional source files that the query should use
        ///     during hte compilation stage.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<string> AdditionalFiles
        {
            get
            {
                if (fAdditionalFiles == null)
                {
                    fAdditionalFiles = new System.Collections.ObjectModel.ObservableCollection<string>();
                    fAdditionalFiles.CollectionChanged += fAdditionalFiles_CollectionChanged;
                }

                return fAdditionalFiles;
            }
        }

        #endregion

        #region Output

        /// <summary>
        ///     Gets/sets the name of the object
        /// </summary>
        /// <remarks>
        ///     Internally, a stringbuilder is used to efficiently construct the
        ///     output string.
        /// </remarks>
        public System.Collections.Generic.IList<QueryResultLine> Output
        {
            get
            {
                return fOutput;
            }
        }

        #endregion

        #region Columns

        /// <summary>
        ///     Gets the list of columns that was generated from the result. Note:
        ///     these are stored in the brainData, so that the user can rename the
        ///     columns but doesn't have to redo this each time.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<QueryColumn> Columns
        {
            get
            {
                if (fColumns == null)
                {
                    fColumns = new System.Collections.ObjectModel.ObservableCollection<QueryColumn>();
                    Columns.CollectionChanged += Columns_CollectionChanged;
                }

                return fColumns;
            }
        }

        #endregion

        #region ColumnCount

        /// <summary>
        ///     gets the nr of colummns. So xaml can bind to the prop.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                if (fColumns != null)
                {
                    return fColumns.Count;
                }

                return 0;
            }
        }

        #endregion

        #region IsLoading

        /// <summary>
        ///     Gets the vlaue that indicates if the editor is currently loading or
        ///     not.
        /// </summary>
        public bool IsLoading { get; internal set; }

        #endregion

        #region DataSources

        /// <summary>
        ///     Gets the list of available datasources.
        /// </summary>
        public static System.Collections.Generic.List<QueryDataSource> DataSources
        {
            get
            {
                if (fDataSources == null)
                {
                    fDataSources = new System.Collections.Generic.List<QueryDataSource>();
                    fDataSources.Add(new NoQueryDataSource());
                    fDataSources.Add(new ExplorerQueryDataSource());
                    fDataSources.Add(new WordsQueryDataSource());
                    fDataSources.Add(new WordNetQueryDataSource());
                    fDataSources.Add(new CustomConduitQueryDataSource());
                }

                return fDataSources;
            }
        }

        #endregion

        #region rendertargets

        /// <summary>
        ///     Gets the list of render targets.
        /// </summary>
        public static System.Collections.Generic.List<RenderDataSource> RenderTargets
        {
            get
            {
                if (fRenderTargets == null)
                {
                    fRenderTargets = new System.Collections.Generic.List<RenderDataSource>();
                    fRenderTargets.Add(new NoRenderDataSource());
                    fRenderTargets.Add(new CsvRenderDataSource());
                }

                return fRenderTargets;
            }
        }

        #endregion

        #region IsRunningResult

        /// <summary>
        ///     Gets/sets the value that indicates if the result panel should only
        ///     show 1 line or all the results.
        /// </summary>
        public bool IsRunningResult
        {
            get
            {
                return fIsRunningResult;
            }

            set
            {
                fIsRunningResult = value;
                OnPropertyChanged("IsRunningResult");
            }
        }

        #endregion

        #region SelectedDataSource

        /// <summary>
        ///     Gets/sets the currently selected datasource
        /// </summary>
        public QueryDataSource SelectedDataSource
        {
            get
            {
                return fSelectedDataSource;
            }

            set
            {
                if (fSelectedDataSource != value)
                {
                    var iItem = (Queries.Query)Item;
                    if (iItem != null)
                    {
                        if (value != null)
                        {
                            iItem.DataSource = value.GetPipe();
                        }
                        else
                        {
                            iItem.DataSource = null;
                        }
                    }

                    SetSelectedDataSource(value);
                }
            }
        }

        /// <summary>The set selected data source.</summary>
        /// <param name="value">The value.</param>
        private void SetSelectedDataSource(QueryDataSource value)
        {
            fSelectedDataSource = value;
            OnPropertyChanged("SelectedDataSource");
            OnPropertyChanged("HasDataSourceExtra");
            OnPropertyChanged("Pipe");
        }

        /// <summary>
        ///     this is provides so that we can bind to it from xaml. It is possible
        ///     to provide custom ui editors through this property.
        /// </summary>
        public object Pipe
        {
            get
            {
                if (SelectedDataSource != null)
                {
                    var iRes = SelectedDataSource.GetEditor(this);
                    if (iRes != null)
                    {
                        return iRes;
                    }
                }

                var iItem = (Queries.Query)Item;
                if (iItem != null)
                {
                    // can be null ite item is null -> no exception thrown
                    return iItem.DataSource;
                }

                return null;
            }
        }

        #endregion

        #region SelectedRenderTarget

        /// <summary>
        ///     Gets/sets the currently selected datasource
        /// </summary>
        public RenderDataSource SelectedRenderTarget
        {
            get
            {
                return fSelectedRenderTarget;
            }

            set
            {
                if (fSelectedRenderTarget != value)
                {
                    var iItem = (Queries.Query)Item;
                    if (iItem != null)
                    {
                        if (value != null)
                        {
                            iItem.RenderTarget = value.GetPipe();
                        }
                        else
                        {
                            iItem.RenderTarget = null;
                        }
                    }

                    SetSelectedRenderTarget(value);
                }
            }
        }

        /// <summary>The set selected render target.</summary>
        /// <param name="value">The value.</param>
        private void SetSelectedRenderTarget(RenderDataSource value)
        {
            fSelectedRenderTarget = value;
            OnPropertyChanged("SelectedRenderTarget");
            OnPropertyChanged("HasRenderTargetExtra");
            OnPropertyChanged("RenderTarget");
        }

        /// <summary>
        ///     this is provides so that we can bind to it from xaml. It is possible
        ///     to provide custom ui editors through this property.
        /// </summary>
        public object RenderTarget
        {
            get
            {
                if (SelectedRenderTarget != null)
                {
                    var iRes = SelectedRenderTarget.GetEditor(this);
                    if (iRes != null)
                    {
                        return iRes;
                    }
                }

                var iItem = (Queries.Query)Item;
                if (iItem != null)
                {
                    // can be null ite item is null -> no exception thrown
                    return iItem.RenderTarget;
                }

                return null;
            }
        }

        #endregion

        #region HasDataSourceExtra

        /// <summary>
        ///     Gets/sets the value that indicates if there is a datasource selected
        ///     or not and if it requires extra data.
        /// </summary>
        public bool HasDataSourceExtra
        {
            get
            {
                return SelectedDataSource != null && SelectedDataSource.HasExtraData;
            }
        }

        #endregion

        #region HasRenderTargetExtra

        /// <summary>
        ///     Gets/sets the value that indicates if there is a render target
        ///     selected or not and if it requires extra data.
        /// </summary>
        public bool HasRenderTargetExtra
        {
            get
            {
                return SelectedRenderTarget != null && SelectedRenderTarget.HasExtraData;
            }
        }

        #endregion

        #region SourceEditorHeight

        /// <summary>
        ///     Gets/sets the height of the source editor panel.
        /// </summary>
        public System.Windows.GridLength SourceEditorHeight
        {
            get
            {
                return fSourceEditorHeight;
            }

            set
            {
                if (value != fSourceEditorHeight)
                {
                    if (value.IsStar == false)
                    {
                        // in the beginning, it is a star, but the splitter only changes the 'value' as if it is in pixels, but keeps it as a start, which screws up the rendering next time, so change the value.
                        fSourceEditorHeight = value;
                    }
                    else
                    {
                        fSourceEditorHeight = new System.Windows.GridLength(
                            value.Value, 
                            System.Windows.GridUnitType.Pixel);
                    }

                    OnPropertyChanged("SourceEditorHeight");
                }
            }
        }

        #endregion

        #endregion

        #region functions

        /// <summary>
        ///     Called when all the data UI data should be loaded.
        /// </summary>
        protected override void LoadUIData()
        {
            IsLoading = true;
            try
            {
                var iInfo = NeuronInfo;
                if (iInfo != null && iInfo.CustomData != null)
                {
                    iInfo.CustomData.Position = 0;
                    var iBin = new System.IO.BinaryReader(iInfo.CustomData);
                    var iVer = iBin.ReadInt16();
                    if (iVer == 1)
                    {
                        fIsRunningResult = iBin.ReadBoolean();
                        fNeedsRecompile = iBin.ReadBoolean();
                        fSource = iBin.ReadString();

                            // don't assign to the property, cause then we set the fIsChanged value, which we don't want, otherwise there would be to many saving back to disk
                        var iNrExtraFiles = iBin.ReadInt32();
                        while (iNrExtraFiles > 0)
                        {
                            AdditionalFiles.Add(iBin.ReadString());
                            iNrExtraFiles--;
                        }

                        var iNrCol = iBin.ReadInt32();
                        while (iNrCol > 0)
                        {
                            var iCol = new QueryColumn();
                            iCol.Read(iBin);
                            Columns.Add(iCol);
                            iNrCol--;
                        }
                    }
                }

                var iItem = Item as Queries.Query;
                if (iItem != null)
                {
                    iItem.NeuronsOut += Query_NeuronsOut;
                    foreach (var i in DataSources)
                    {
                        if (i.IsPipe(iItem.DataSource))
                        {
                            SetSelectedDataSource(i);

                                // don't assign to DataSource, but set it like this, if we don't, we will recreate a new datasource for the query, which we don't want, it already has one.
                            break;
                        }
                    }

                    foreach (var i in RenderTargets)
                    {
                        if (i.IsPipe(iItem.RenderTarget))
                        {
                            SetSelectedRenderTarget(i);

                                // don't assign to DataSource, but set it like this, if we don't, we will recreate a new datasource for the query, which we don't want, it already has one.
                            break;
                        }
                    }

                    if (iItem.ActionsForInput == null)
                    {
                        // in case the query got deleted and then the delete got undone, 'NeedsCompile' might be 'false', but the undo doesn't automatically do a recompile, so this makes certain taht the correct state is shown.
                        NeedsRecompile = true;
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        ///     Called when all the data that is kept in memory for the UI part can be
        ///     unloaded.
        /// </summary>
        protected override void UnloadUIData()
        {
            Flush();
            var iItem = Item as Queries.Query;
            if (iItem != null)
            {
                iItem.NeuronsOut -= Query_NeuronsOut;
            }

            Output.Clear();
            if (fColumns != null)
            {
                fColumns.CollectionChanged -= Columns_CollectionChanged;

                    // saver for mem leaks to always unregister event hanlders.
                fColumns = null;
            }

            fDataSources = null; // no need to keep this in mem when not open.
            if (fAdditionalFiles != null)
            {
                fAdditionalFiles.CollectionChanged -= fAdditionalFiles_CollectionChanged;
                fAdditionalFiles = null;

                    // unload when possible. It is stored in the braindata anyway and reloaded when ui is loaded.
            }

            fSourceEditorHeight = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);

                // reset the height of the editor, so that it doesn't disapear when the app gets resized.
        }

        /// <summary>
        ///     writes the extra data to the <see cref="NeuronData" /> object's
        ///     CustomData stream.
        /// </summary>
        private void Flush()
        {
            if (fIsChanged)
            {
                var iStream = new System.IO.MemoryStream();
                var iWrite = new System.IO.BinaryWriter(iStream);
                iWrite.Write((System.Int16)1); // the version
                iWrite.Write(IsRunningResult);
                iWrite.Write(NeedsRecompile);

                    // so we can keep track the next time that we open the query, if it needs to be recompiled first.
                if (fSource != null)
                {
                    // if there source == null -> write throws an error.
                    iWrite.Write(fSource);
                }
                else
                {
                    iWrite.Write(string.Empty);
                }

                if (fAdditionalFiles != null)
                {
                    iWrite.Write(AdditionalFiles.Count);
                    foreach (var i in AdditionalFiles)
                    {
                        iWrite.Write(i);
                    }
                }
                else
                {
                    iWrite.Write(0);
                }

                if (fColumns != null)
                {
                    iWrite.Write(fColumns.Count);
                    foreach (var i in fColumns)
                    {
                        i.Write(iWrite);
                    }
                }
                else
                {
                    iWrite.Write(0);
                }

                NeuronInfo.CustomData = iStream;
                fIsChanged = false; // reset this, otherwise we can get bogus saves.
            }
        }

        /// <summary>Converts an object into its XML representation.</summary>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            Flush();

                // make certain taht the extra data is also saved. the flush will only write it to the neuronData, but this gets saved after all the edtiors were written to xml.
        }

        /// <summary>handles the incomming events from the query for displaying results.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Query_NeuronsOut(object sender, OutputEventArgs e)
        {
            var iRes = new QueryResultLine();
            iRes.Owner = this;
            iRes.Add(e.Data);
            lock (fTempList)
            {
                fTempList.Add(iRes);
                if (fTimer == null)
                {
                    fTimer =
                        new System.Windows.Threading.DispatcherTimer(
                            System.Windows.Threading.DispatcherPriority.Normal, 
                            System.Windows.Application.Current.Dispatcher);
                    fTimer.Tick += OnTimeSlot;
                    fTimer.Interval = new System.TimeSpan(0, 0, 0, 0, UIUPDATEDELAY);
                    fTimer.Start();
                }
                else if (fTimer.IsEnabled == false)
                {
                    fTimer.Start();
                }
            }
        }

        /// <summary>called when the timer has expired and the ui needs to be updated.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnTimeSlot(object sender, System.EventArgs e)
        {
            lock (fTempList)
            {
                fTimer.IsEnabled = false; // this is not a repeating timer, but a single shot.
                if (IsRunningResult)
                {
                    Output.Clear();
                }

                foreach (var i in fTempList)
                {
                    Output.Add(i);
                    while (fColumns != null && i.DataFormat.Count > fColumns.Count)
                    {
                        // fColumns can become null when we close the app during processing.
                        var iCol = new QueryColumn(fColumns.Count);
                        fColumns.Add(iCol);
                    }
                }

                fTempList.Clear();
            }
        }

        #region copy/paste/delete: not handled by this object, but through wpf.

        /// <summary>Copies to clipboard.</summary>
        /// <param name="data">The data.</param>
        protected override void CopyToClipboard(System.Windows.DataObject data)
        {
            // performed by the wpf object.
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Determines whether this instance can copy the selected data to the
        ///     clipboard].
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can copy to the clipboard; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanCopyToClipboard()
        {
            return false; // performed by the wpf object.
        }

        /// <summary>
        ///     Determines whether this instance can paste special from the clipboard.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can paste special from the clipboard;
        ///     otherwise, <c>false</c> .
        /// </returns>
        public override bool CanPasteSpecialFromClipboard()
        {
            return false; // performed by the wpf object.
        }

        /// <summary>
        ///     Pastes the data in a special way from the clipboard.
        /// </summary>
        public override void PasteSpecialFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Pastes the data from the clipboard.
        /// </summary>
        public override void PasteFromClipboard()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Removes all the selected items on this editor but doesn't delete them.
        ///     This is used by the cut command.
        /// </summary>
        public override void Remove()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Deletes all the selected items on this editor.
        /// </summary>
        public override void Delete()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>Checks if a delete can be performed on this editor.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public override bool CanDelete()
        {
            return false;
        }

        /// <summary>
        ///     Deletes all the selected items on this editor after the user has
        ///     selected extra deletion options.
        /// </summary>
        public override void DeleteSpecial()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        ///     Determines whether a delete special can be performed
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this instance can do a special delete; otherwise,
        ///     <c>false</c> .
        /// </returns>
        public override bool CanDeleteSpecial()
        {
            return false;
        }

        /// <summary>Deletes all the neurons on the editor according to the specified
        ///     deletion and branch-handling methods. This is called when the editor
        ///     is deleted from the project using the special delete.</summary>
        /// <param name="deletionMethod">The deletion method.</param>
        /// <param name="branchHandling">The branch handling.</param>
        public override void DeleteAll(DeletionMethod deletionMethod, DeletionMethod branchHandling)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #endregion
    }
}