// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdleLevel.cs" company="">
//   
// </copyright>
// <summary>
//   Contains all the info about a single Idle level that is defined in a CCS
//   file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     Contains all the info about a single Idle level that is defined in a CCS
    ///     file.
    /// </summary>
    public class IdleLevel
    {
        /// <summary>Gets or sets the min start delay.</summary>
        public int MinStartDelay { get; set; }

        /// <summary>Gets or sets the max start delay.</summary>
        public int MaxStartDelay { get; set; }

        /// <summary>Gets or sets the min duration.</summary>
        public int MinDuration { get; set; }

        /// <summary>Gets or sets the max duration.</summary>
        public int MaxDuration { get; set; }

        /// <summary>Gets or sets the min interval.</summary>
        public int MinInterval { get; set; }

        /// <summary>Gets or sets the max interval.</summary>
        public int MaxInterval { get; set; }

        /// <summary>
        ///     Stores the amount of time that has elapsed for this idle level.
        /// </summary>
        /// <value>
        ///     The elapsed time.
        /// </value>
        public int ElapsedTime { get; set; }

        /// <summary>
        ///     Gets or sets the names of all the animations that should be managed by
        ///     this idle level.
        /// </summary>
        /// <value>
        ///     The animation names.
        /// </value>
        public System.Collections.Generic.List<string> AnimationNames { get; set; }
    }
}