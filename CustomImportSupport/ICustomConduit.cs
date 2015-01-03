// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICustomConduit.cs" company="">
//   
// </copyright>
// <summary>
//   so that results can be passed along to the UI for dislay.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.CustomConduitSupport
{
    /// <summary>
    ///     so that results can be passed along to the UI for dislay.
    /// </summary>
    public class StringEventArgs : System.EventArgs
    {
        /// <summary>Initializes a new instance of the <see cref="StringEventArgs"/> class.</summary>
        /// <param name="value">The value.</param>
        public StringEventArgs(string value)
        {
            Value = value;
        }

        /// <summary>Gets or sets the value.</summary>
        public string Value { get; set; }
    }

    /// <summary>
    ///     this interface should be implemented by a class in the custom dll for providing support to loading custom data.
    ///     This interface can be used in 2 distinct ways: through one of the <see cref="ICustomConduit.Process" /> functions,
    ///     which are used in a push scenario (from the editor: file/custom conduit)
    ///     Or the interface can be used for supplying data to a query through the <see cref="ICustomConduit.Open" />,
    ///     <see cref="ICustomConduit.ReadLine" />
    ///     and <see cref="ICustomConduit.Close" /> functions.
    /// </summary>
    public interface ICustomConduit
    {
        /// <summary>
        ///     Gets the index of the file we are currently importing (can be used to keep track  of a large scale position
        ///     indicator).
        /// </summary>
        double FilesPosition { get; set; }

        /// <summary>
        ///     Gets/sets the relative file position (pos/length)
        /// </summary>
        double FilePosition { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the conduit is open or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        bool IsOpen { get; }

        /// <summary>
        ///     this event is raised whenever the system should be saved to relieve memory usage.
        /// </summary>
        /// <remarks>
        ///     This is done through an event instead of letting the custom conduit save the networks itself, cause the network
        ///     might be
        ///     part of a bigger project, like with the designer.
        /// </remarks>
        event System.EventHandler SaveRequested;

        /// <summary>
        ///     called when the process is done.
        /// </summary>
        event System.EventHandler Finished;

        /// <summary>
        ///     called when a result has been found.
        /// </summary>
        event System.EventHandler<StringEventArgs> ResultFound;

        /// <summary>gets a value to indicate that the system should be prepared to receive 'SaveRequests' or not. Allows the system to
        ///     warn the user in
        ///     case of large data processing.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        bool NeedsSaving();

        /// <summary>
        ///     Gets a value to indicate if the process needs to be started with a location and destination or only a location
        ///     argument.
        /// </summary>
        /// <returns>true or false</returns>
        bool NeedsDestination();

        /// <summary>reads in the data and/or starts producing output.
        ///     This is for a push scenario.</summary>
        /// <remarks>Depending on the value of <see cref="ICustomConduit.NeedsDestination"/>, either this function needs to be
        ///     implemented,
        ///     or process(location, destination) has to be supplied, but you don't need to do both.</remarks>
        /// <param name="location">The location of the datasource</param>
        void Process(string location);

        /// <summary>reads in the data and/or starts producing output, rendering everthing to the specified output.
        ///     This is for a push scenario.</summary>
        /// <param name="location">The location of the datasource</param>
        /// <param name="destination">The destination.</param>
        void Process(string location, string destination);

        /// <summary>Opens the file at the specified location. So that data can be read by the <see cref="ICustomConduit.ReadLine"/>
        ///     function.
        ///     This is for a pull scenario.</summary>
        /// <param name="location">The location.</param>
        void Open(string location);

        /// <summary>reads a single line of data from the file.
        ///     This is used together with <see cref="ICustomConduit.Opern"/> and  <see cref="ICustomConduit.Close"/></summary>
        /// <param name="result"></param>
        /// <returns>false if no more line could be read, otherwise true.</returns>
        bool ReadLine(System.Collections.Generic.List<Neuron> result);

        /// <summary>The close.</summary>
        void Close();

        /// <summary>
        ///     stops the operation.
        /// </summary>
        void Cancel();

        /// <summary>provides a way to save any conduit specific data to the db (the neural network), so the conduit can correcly be
        ///     loaded from disk again.</summary>
        /// <param name="writer"></param>
        void WriteV1(System.IO.BinaryWriter writer);

        /// <summary>provides a way to read the settings for the conduit from disk (the neural network database), so that it can be
        ///     reused again with the previous settings.</summary>
        /// <param name="reader"></param>
        void ReadV1(System.IO.BinaryReader reader);

        /// <summary>asks the conduit to write it's settings to an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="writer"></param>
        void WriteXml(System.Xml.XmlWriter writer);

        /// <summary>asks the conduit to read it's settings from an xml file. This is used for exporting/importing queries that use a
        ///     conduit as datasource.</summary>
        /// <param name="reader"></param>
        void ReadXml(System.Xml.XmlReader reader);
    }
}