// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Randomizer.cs" company="">
//   
// </copyright>
// <summary>
//   provides a thread save global random number generator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides a thread save global random number generator.
    /// </summary>
    internal class Randomizer
    {
        /// <summary>The f randomizer.</summary>
        private static System.Random fRandomizer;

        /// <summary>gets a random number thread save.</summary>
        /// <param name="maxValue">the maximum value.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int Next(int maxValue)
        {
            if (fRandomizer == null)
            {
                // only create one, when actually used.
                fRandomizer = new System.Random();
            }

            lock (fRandomizer) return fRandomizer.Next(maxValue);
        }
    }
}