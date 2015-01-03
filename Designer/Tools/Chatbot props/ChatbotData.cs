// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChatbotData.cs" company="">
//   
// </copyright>
// <summary>
//   Defines all the data that needs to be streamed for the chatbot
//   properties.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Defines all the data that needs to be streamed for the chatbot
    ///     properties.
    /// </summary>
    public class ChatbotData
    {
        /// <summary>
        ///     Gets or sets the ID of the neuron that respresents the bot. This is
        ///     for easy access to the properties of the bot from within the designer.
        /// </summary>
        /// <value>
        ///     The bot ID.
        /// </value>
        public ulong BotID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the neuron that respresents the creator. This
        ///     is for easy access to the properties of the creator from within the
        ///     designer.
        /// </summary>
        /// <value>
        ///     The creator ID.
        /// </value>
        public ulong CreatorID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the neuron used to link the current user to .
        /// </summary>
        /// <value>
        ///     The <see langword="ref" /> to user ID.
        /// </value>
        public ulong RefToUserID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the neuron that represents the name attribute.
        /// </summary>
        /// <value>
        ///     The name ID.
        /// </value>
        public ulong NameID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of the neuron that represents the birthday
        ///     attribute.
        /// </summary>
        /// <value>
        ///     The birthday ID.
        /// </value>
        public ulong BirthdayID { get; set; }

        /// <summary>
        ///     Gets or sets the type of the bot.
        /// </summary>
        /// <value>
        ///     The type of the bot: 0=Stand alone (default) 1=Online bot.
        /// </value>
        public int BotType { get; set; }

        /// <summary>
        ///     Gets or sets the ID of object used to represent gender attribute.
        /// </summary>
        /// <value>
        ///     The gender ID.
        /// </value>
        public ulong GenderID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of object used to represent male value.
        /// </summary>
        /// <value>
        ///     The male ID.
        /// </value>
        public ulong MaleID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of object used to represent female value.
        /// </summary>
        /// <value>
        ///     The female ID.
        /// </value>
        public ulong FemaleID { get; set; }

        /// <summary>
        ///     Gets or sets the ID of object used to represent Hermaphrodite value.
        /// </summary>
        /// <value>
        ///     The herma ID.
        /// </value>
        public ulong HermaID { get; set; }

        /// <summary>
        ///     Gets or sets the id of the <see cref="IntNeuron" /> that's used to let
        ///     the network know if synonyms need to be undoubled or if thesaurus refs
        ///     are used in the pattterns.
        /// </summary>
        /// <value>
        ///     The synonym resolve <see langword="switch" /> ID.
        /// </value>
        public ulong SynonymResolveSwitchID { get; set; }

        /// <summary>
        ///     Gets or sets the id of the <see langword="switch" /> that lets the
        ///     network know that it needs to use the 'output' var (declared in output
        ///     patterns) for collecting previous output data, or if the prev data is
        ///     collected automatically.
        /// </summary>
        /// <value>
        ///     The use output var <see langword="switch" /> ID.
        /// </value>
        public ulong UseOutputVarSwitchID { get; set; }

        /// <summary>
        ///     Gets or sets the id of the <see langword="switch" /> that lets the
        ///     network know that it needs to use the weight given by the STT or not.
        /// </summary>
        public ulong UseSTTWeightID { get; set; }

        /// <summary>
        ///     Gets or sets the id of the <see langword="switch" /> that lets the
        ///     network know that it needs to search for a single top pattern or if
        ///     multiple top patterns are allowed.
        /// </summary>
        public ulong SingleTopPatternResultID { get; set; }

        /// <summary>
        ///     Gets or sets the mapping to use between identifiers and neurons during
        ///     parsing. This list simply contains the id's of the neurons that should
        ///     be included in the map, the displayTitle is used as text part of the
        ///     mapping.
        /// </summary>
        /// <value>
        ///     The parser map.
        /// </value>
        public ParserMap ParserMap { get; set; }

        /// <summary>
        ///     Gets or sets the mapping to use between identifiers and neurons during
        ///     parsing of do patterns. This list simply contains the id's of the
        ///     neurons that should be included in the map, the displayTitle is used
        ///     as text part of the mapping. This stores references to neurons that
        ///     represent dll entry points.
        /// </summary>
        /// <value>
        ///     The parser map.
        /// </value>
        public FunctionMap DoFunctionMap { get; set; }
    }
}