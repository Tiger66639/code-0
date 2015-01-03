// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SplitData.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the data specific for a processor that is taking part in a
//   split.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Contains all the data specific for a processor that is taking part in a
    ///     split.
    /// </summary>
    internal class SplitData
    {
        /// <summary>Initializes a new instance of the <see cref="SplitData"/> class.</summary>
        public SplitData()
        {
            IsAccum = false;
        }

        /// <summary>
        ///     Gets or sets the object that manages the split.
        /// </summary>
        /// <value>
        ///     The head.
        /// </value>
        public HeadData Head { get; set; }

        /// <summary>
        ///     Gets or sets the processor to which this data item belongs.
        /// </summary>
        /// <value>
        ///     The processor.
        /// </value>
        public Processor Processor { get; set; }

        /// <summary>
        ///     Keeps track of the nr of splits that were called after a 'LastOfSplit'
        ///     was performed. This is used by the 'lastOfsplit' to determin if there
        ///     were new splits called during the 'lastOfsplit' callback routine that
        ///     require a new 'finishSplit'.
        /// </summary>
        public int NrSplitsAfterLastOfSplit { get; set; }

        /// <summary>
        ///     when true, weights of the same results in multiple processoers, will
        ///     be added, instead of taking hte max.
        /// </summary>
        public bool IsAccum { get; set; }
    }
}