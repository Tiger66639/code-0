// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MMAudio.cs" company="">
//   
// </copyright>
// <summary>
//   The mm audio.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Multimedia
{
    /// <summary>The mm audio.</summary>
    public class MMAudio
    {
        /// <summary>The sn d_ async.</summary>
        public const uint SND_ASYNC = 1;

        /// <summary>The sn d_ memory.</summary>
        public const uint SND_MEMORY = 4;

        // these 2 overloads we dont need ...
        // [DllImport("Winmm.dll")]
        // public static extern bool PlaySound(IntPtr rsc, IntPtr hMod, UInt32 dwFlags);

        // [DllImport("Winmm.dll")]
        // public static extern bool PlaySound(string Sound, IntPtr hMod, UInt32 dwFlags);

        // this is the overload we want to play embedded resource...
        /// <summary>The play sound.</summary>
        /// <param name="data">The data.</param>
        /// <param name="hMod">The h mod.</param>
        /// <param name="dwFlags">The dw flags.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [System.Runtime.InteropServices.DllImport("Winmm.dll")]
        public static extern bool PlaySound(byte[] data, System.IntPtr hMod, uint dwFlags);

        /// <summary>Plays the <see langword="byte"/> array as a wave stream.</summary>
        /// <param name="toPlay">To play.</param>
        public static void PlayWav(byte[] toPlay)
        {
            PlaySound(toPlay, System.IntPtr.Zero, SND_ASYNC | SND_MEMORY);
        }
    }
}