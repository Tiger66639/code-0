// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BrainEventArgs.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for all event arguments for events comming from the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Base class for all event arguments for events comming from the brain.
    /// </summary>
    public class BrainEventArgs : System.EventArgs
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="BrainEventArgs" /> class.
        /// </summary>
        public BrainEventArgs()
        {
            Processor = Processor.CurrentProcessor;
        }

        #endregion

        #region Processor

        /// <summary>
        ///     Gets the processor in which the event was triggered.
        /// </summary>
        /// <remarks>
        ///     This allows you to check on this value reliably accros threads.
        ///     (important cause the Current processor is specific to a thread).
        /// </remarks>
        public Processor Processor { get; private set; }

        #endregion
    }
}