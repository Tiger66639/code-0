// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DesignerDataFile.cs" company="">
//   
// </copyright>
// <summary>
//   local data, so we can undo.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     local data, so we can undo.
    /// </summary>
    public class OnlineData
    {
        /// <summary>Initializes a new instance of the <see cref="OnlineData"/> class.</summary>
        public OnlineData()
        {
            TimeOut = 10000;
            DefaultController = "Html";
            CSSFile = System.IO.Path.Combine(NeuronDataDictionary.DefaultDataPath, "site.css");
        }

        /// <summary>
        ///     The username (for ftp and path)
        /// </summary>
        public string User { get; set; }

        /// <summary>
        ///     the pwd to the user account.
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        ///     any possible ip filter (regex) that needs to be used as a filter for
        ///     allowed incomming connections for the web API
        /// </summary>
        public string IPFilter { get; set; }

        /// <summary>
        ///     The max time that the system should wait for blocking calls.
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        ///     A possible css file to customize the webpage with. When null, the
        ///     default is used.
        /// </summary>
        public string CSSFile { get; set; }

        /// <summary>
        ///     Gets/sets a possible template that can be used as root html page. When
        ///     null, the default is used.
        /// </summary>
        public string HtmlTemplate { get; set; }

        /// <summary>
        ///     the default controller to use for the online version: api or html.
        /// </summary>
        public string DefaultController { get; set; }

        /// <summary>
        ///     gets/sets the location where the chatbot has been installed (so that
        ///     this can be changed + so that each bot knows this).
        /// </summary>
        public string FTPLocation { get; set; }

        /// <summary>
        ///     Gets/sets the path on the server where the site is installed. This is
        ///     required for properly preparing the web.config file.
        /// </summary>
        public string ServerPath { get; set; }

        /// <summary>
        ///     Lets the user determin if the html should only render a partial (for
        ///     interjection), or a complete html page.
        /// </summary>
        public bool HtmlAsPartial { get; set; }

        /// <summary>
        ///     the location of the crazy talk file.
        /// </summary>
        public string CTLocation { get; set; }

        /// <summary>
        ///     Gets/sets if this site has been installed or not.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>The assign to.</summary>
        /// <param name="dest">The dest.</param>
        internal void AssignTo(OnlineData dest)
        {
            dest.IPFilter = IPFilter;
            dest.Pwd = Pwd;
            dest.TimeOut = TimeOut;
            dest.User = User;
            dest.CSSFile = CSSFile;
            dest.DefaultController = DefaultController;
            dest.HtmlTemplate = HtmlTemplate;
            dest.HtmlAsPartial = HtmlAsPartial;
            dest.FTPLocation = FTPLocation;
            dest.ServerPath = ServerPath;
            dest.CTLocation = CTLocation;
        }

        /// <summary>for export</summary>
        /// <param name="writer"></param>
        internal void WriteToXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("Online");
            XmlStore.WriteElement(writer, "User", User);
            XmlStore.WriteElement(writer, "Pwd", Pwd);
            XmlStore.WriteElement(writer, "TimeOut", TimeOut);
            XmlStore.WriteElement(writer, "Location", FTPLocation);
            XmlStore.WriteElement(writer, "ServerPath", ServerPath);
            XmlStore.WriteElement(writer, "IPFilter", IPFilter);
            XmlStore.WriteElement(writer, "HTMLTemplate", HtmlTemplate);
            XmlStore.WriteElement(writer, "HMTLAsPartial", HtmlAsPartial);
            XmlStore.WriteElement(writer, "DefaultController", DefaultController);
            XmlStore.WriteElement(writer, "CSSFile", CSSFile);
            XmlStore.WriteElement(writer, "Installed", IsInstalled);
            XmlStore.WriteElement(writer, "CTLocation", CTLocation);
            writer.WriteEndElement();
        }

        /// <summary>reads from xml.</summary>
        /// <param name="reader"></param>
        /// <returns>The <see cref="OnlineData"/>.</returns>
        internal static OnlineData ReadFromXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
            {
                return null;
            }

            var iOnline = new OnlineData();
            iOnline.User = XmlStore.ReadElement<string>(reader, "User");
            iOnline.Pwd = XmlStore.ReadElement<string>(reader, "Pwd");
            iOnline.TimeOut = XmlStore.ReadElement<int>(reader, "TimeOut");
            iOnline.FTPLocation = XmlStore.ReadElement<string>(reader, "Location");
            string iPath = null;
            if (XmlStore.TryReadElement(reader, "ServerPath", ref iPath))
            {
                iOnline.ServerPath = iPath;
            }

            iOnline.IPFilter = XmlStore.ReadElement<string>(reader, "IPFilter");
            iOnline.HtmlTemplate = XmlStore.ReadElement<string>(reader, "HTMLTemplate");
            iOnline.HtmlAsPartial = XmlStore.ReadElement<bool>(reader, "HMTLAsPartial");
            iOnline.DefaultController = XmlStore.ReadElement<string>(reader, "DefaultController");
            iOnline.CSSFile = XmlStore.ReadElement<string>(reader, "CSSFile");
            var iInstalled = false;
            if (XmlStore.TryReadElement(reader, "Installed", ref iInstalled))
            {
                iOnline.IsInstalled = iInstalled;
            }

            if (XmlStore.TryReadElement(reader, "CTLocation", ref iPath))
            {
                iOnline.CTLocation = iPath;
            }

            reader.ReadEndElement();
            return iOnline;
        }
    }

    /// <summary>
    ///     Contains all the data for the designer for easy streaming.
    /// </summary>
    /// <remarks>
    ///     All the streamable data for the <see cref="BrainData" /> object is stored
    ///     in a different class so that the braindata can easely load different data
    ///     by replacing a field.
    /// </remarks>
    public class DesignerDataFile
    {
        /// <summary>
        ///     Gets or sets the version of the template that this project was based
        ///     on, so that we can check if an upgrade is required or not.
        /// </summary>
        /// <value>
        ///     The template version.
        /// </value>
        public string TemplateVersion { get; set; }

        /// <summary>
        ///     Gets or sets the name of the project
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the chatbotdata.
        /// </summary>
        /// <value>
        ///     The chatbotdata.
        /// </value>
        public ChatbotData Chatbotdata { get; set; }

        /// <summary>Gets or sets the debugmode.</summary>
        public DebugMode Debugmode { get; set; }

        /// <summary>Gets or sets a value indicating whether break on exception.</summary>
        public bool BreakOnException { get; set; }

        /// <summary>Gets or sets the break points.</summary>
        public BreakPointCollection BreakPoints { get; set; }

        /// <summary>Gets or sets the play speed.</summary>
        public System.TimeSpan PlaySpeed { get; set; }

        /// <summary>Gets or sets the tool box items.</summary>
        public ToolBoxItemCollection ToolBoxItems { get; set; }

        /// <summary>Gets or sets the default meaning ids.</summary>
        public SmallIDCollection DefaultMeaningIds { get; set; }

        /// <summary>
        ///     Gets or sets the list of neurons (as ids) that define extra properties
        ///     which were declared in modules.
        /// </summary>
        /// <value>
        ///     The module prop ids.
        /// </value>
        public SmallIDCollection ModulePropIds { get; set; }

        /// <summary>Gets or sets the asset pronoun ids.</summary>
        public AssetPronounsMap AssetPronounIds { get; set; }

        /// <summary>
        ///     Gets or sets the map that defines neurons like I, you, he, she, it,
        ///     we, they, mine, myself,...
        /// </summary>
        /// <value>
        ///     The person map ids.
        /// </value>
        public PersonsMap PersonMapIds { get; set; }

        /// <summary>Gets or sets the neuron info.</summary>
        public NeuronDataDictionary NeuronInfo { get; set; }

        /// <summary>Gets or sets the instructions.</summary>
        public NeuronCollection<Instruction> Instructions { get; set; }

        /// <summary>Gets or sets the operators.</summary>
        public NeuronCollection<Neuron> Operators { get; set; }

        /// <summary>
        ///     Gets or sets the variables that need to be monitored for this project.
        /// </summary>
        /// <value>
        ///     The watches.
        /// </value>
        public WatchCollection Watches { get; set; }

        /// <summary>Gets or sets the comm channels.</summary>
        public CommChannelCollection CommChannels { get; set; }

        /// <summary>
        ///     This is stored seperatly cause it isn't really used as a channel but
        ///     as a resource, so it isn't listed as a channel, but we need to store
        ///     some data for this when it is used, so keep a ref.
        /// </summary>
        public WordNetChannel WordnetChannel { get; set; }

        /// <summary>
        ///     <para>Gets or sets the list of frame editors.</para>
        ///     <para>Gets or sets the list of flows declared in the project.</para>
        ///     <para>
        ///         Gets or sets all the editors stored in the project. This is a tree
        ///         like structure.
        ///     </para>
        /// </summary>
        /// <value>
        ///     The frame editors. The editors.
        /// </value>
        public EditorCollection Editors { get; set; }

        /// <summary>
        ///     Gets or sets the thesaurus data (relationships between objects) stored
        ///     for this project.
        /// </summary>
        /// <value>
        ///     The thesaurus.
        /// </value>
        public Thesaurus Thesaurus { get; set; }

        /// <summary>
        ///     Gets or sets the list of overlays that have been defined in the
        ///     project.
        /// </summary>
        /// <value>
        ///     The overlays.
        /// </value>
        public System.Collections.Generic.List<OverlayText> Overlays { get; set; }

        /// <summary>
        ///     Gets or sets the list of paths that have been stored to easely find
        ///     the processors again.
        /// </summary>
        /// <value>
        ///     The split paths.
        /// </value>
        public Data.ObservedCollection<SplitPath> SplitPaths { get; set; }

        /// <summary>
        ///     Gets or sets all the documents that were open.
        /// </summary>
        /// <value>
        ///     The open documents.
        /// </value>
        public OpenDocsCollection OpenDocuments { get; set; }

        /// <summary>
        ///     Gets/sets any online data that's available for installing the network
        ///     online.
        /// </summary>
        public OnlineData OnlineInfo { get; set; }
    }
}