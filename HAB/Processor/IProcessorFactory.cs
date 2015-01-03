// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IProcessorFactory.cs" company="">
//   
// </copyright>
// <summary>
//   An interface that can be implemented when custom processors need to be made by the system.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An interface that can be implemented when custom processors need to be made by the system.
    /// </summary>
    public interface IProcessorFactory
    {
        /// <summary>Called when a new processor needs to be created.</summary>
        /// <returns>The <see cref="Processor"/>.</returns>
        Processor CreateProcessor();

        /// <summary>Called when a processor is about to be started. This is always called after a<see cref="IProcessorFactory.CreateProcessor"/>
        ///     was called, but can also be called at other times, when a processor gets reused.</summary>
        /// <param name="proc"></param>
        void ActivateProc(Processor proc);
    }
}