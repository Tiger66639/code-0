// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WordNetEventItem.cs" company="">
//   
// </copyright>
// <summary>
//   Stores the information that was gathered when an event came in from the
//   <see cref="WordNetSin" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Stores the information that was gathered when an event came in from the
    ///     <see cref="WordNetSin" /> .
    /// </summary>
    public class WordNetEventItem
    {
        /// <summary>Initializes a new instance of the <see cref="WordNetEventItem"/> class.</summary>
        public WordNetEventItem()
        {
            Time = System.DateTime.Now;
        }

        /// <summary>
        ///     Gets or sets the text that should be displayed to disgribe the item.
        /// </summary>
        /// <value>
        ///     The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the time at which the event was registered.
        /// </summary>
        /// <value>
        ///     The time.
        /// </value>
        public System.DateTime Time { get; set; }
    }
}