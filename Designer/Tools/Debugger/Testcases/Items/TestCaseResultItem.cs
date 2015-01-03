// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCaseResultItem.cs" company="">
//   
// </copyright>
// <summary>
//   Contains the result data for 1 <see cref="TestCaseItem" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     Contains the result data for 1 <see cref="TestCaseItem" />
    /// </summary>
    public class TestCaseResultItem
    {
        #region Result

        /// <summary>
        ///     Gets the actual result that was returned by the network.
        /// </summary>
        public string Result { get; internal set; }

        #endregion

        #region IsPassed

        /// <summary>
        ///     Gets wether the result passed the test.
        /// </summary>
        public bool? IsPassed { get; internal set; }

        #endregion

        #region Duration

        /// <summary>
        ///     Gets the amount of time that the test took to run.
        /// </summary>
        public System.TimeSpan Duration { get; internal set; }

        #endregion

        #region TotalThreads

        /// <summary>
        ///     Gets the nr of threads that were used to calculate the result.
        /// </summary>
        public int TotalThreads { get; internal set; }

        #endregion
    }
}