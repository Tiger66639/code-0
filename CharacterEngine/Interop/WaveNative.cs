// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaveNative.cs" company="">
//   
// </copyright>
// <summary>
//   The wave formats.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WaveLib
{
    /// <summary>The wave formats.</summary>
    public enum WaveFormats
    {
        /// <summary>The pcm.</summary>
        Pcm = 1, 

        /// <summary>The float.</summary>
        Float = 3
    }

    /// <summary>The wave format.</summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public class WaveFormat
    {
        /// <summary>The w format tag.</summary>
        public short wFormatTag;

        /// <summary>The n channels.</summary>
        public short nChannels;

        /// <summary>The n samples per sec.</summary>
        public int nSamplesPerSec;

        /// <summary>The n avg bytes per sec.</summary>
        public int nAvgBytesPerSec;

        /// <summary>The n block align.</summary>
        public short nBlockAlign;

        /// <summary>The w bits per sample.</summary>
        public short wBitsPerSample;

        /// <summary>The cb size.</summary>
        public short cbSize;

        /// <summary>Initializes a new instance of the <see cref="WaveFormat"/> class.</summary>
        /// <param name="rate">The rate.</param>
        /// <param name="bits">The bits.</param>
        /// <param name="channels">The channels.</param>
        public WaveFormat(int rate, int bits, int channels)
        {
            wFormatTag = (short)WaveFormats.Pcm;
            nChannels = (short)channels;
            nSamplesPerSec = rate;
            wBitsPerSample = (short)bits;
            cbSize = 0;

            nBlockAlign = (short)(channels * (bits / 8));
            nAvgBytesPerSec = nSamplesPerSec * nBlockAlign;
        }
    }

    /// <summary>The wave native.</summary>
    internal class WaveNative
    {
        // callbacks
        /// <summary>The wave delegate.</summary>
        /// <param name="hdrvr">The hdrvr.</param>
        /// <param name="uMsg">The u msg.</param>
        /// <param name="dwUser">The dw user.</param>
        /// <param name="wavhdr">The wavhdr.</param>
        /// <param name="dwParam2">The dw param 2.</param>
        public delegate void WaveDelegate(System.IntPtr hdrvr, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);

        // consts
        /// <summary>The mmsyser r_ noerror.</summary>
        public const int MMSYSERR_NOERROR = 0; // no error

        /// <summary>The m m_ wo m_ open.</summary>
        public const int MM_WOM_OPEN = 0x3BB;

        /// <summary>The m m_ wo m_ close.</summary>
        public const int MM_WOM_CLOSE = 0x3BC;

        /// <summary>The m m_ wo m_ done.</summary>
        public const int MM_WOM_DONE = 0x3BD;

        /// <summary>The callbac k_ function.</summary>
        public const int CALLBACK_FUNCTION = 0x00030000; // dwCallback is a FARPROC 

        /// <summary>The tim e_ ms.</summary>
        public const int TIME_MS = 0x0001; // time in milliseconds 

        /// <summary>The tim e_ samples.</summary>
        public const int TIME_SAMPLES = 0x0002; // number of wave samples 

        /// <summary>The tim e_ bytes.</summary>
        public const int TIME_BYTES = 0x0004; // current byte offset 

        /// <summary>The mmdll.</summary>
        private const string mmdll = "winmm.dll";

        // native calls
        /// <summary>The wave out get num devs.</summary>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutGetNumDevs();

        /// <summary>The wave out prepare header.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="lpWaveOutHdr">The lp wave out hdr.</param>
        /// <param name="uSize">The u size.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutPrepareHeader(System.IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        /// <summary>The wave out unprepare header.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="lpWaveOutHdr">The lp wave out hdr.</param>
        /// <param name="uSize">The u size.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutUnprepareHeader(System.IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        /// <summary>The wave out write.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="lpWaveOutHdr">The lp wave out hdr.</param>
        /// <param name="uSize">The u size.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutWrite(System.IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        /// <summary>The wave out open.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="uDeviceID">The u device id.</param>
        /// <param name="lpFormat">The lp format.</param>
        /// <param name="dwCallback">The dw callback.</param>
        /// <param name="dwInstance">The dw instance.</param>
        /// <param name="dwFlags">The dw flags.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutOpen(
            out System.IntPtr hWaveOut, 
            int uDeviceID, 
            WaveFormat lpFormat, 
            WaveDelegate dwCallback, 
            int dwInstance, 
            int dwFlags);

        /// <summary>The wave out reset.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutReset(System.IntPtr hWaveOut);

        /// <summary>The wave out close.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutClose(System.IntPtr hWaveOut);

        /// <summary>The wave out pause.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutPause(System.IntPtr hWaveOut);

        /// <summary>The wave out restart.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutRestart(System.IntPtr hWaveOut);

        /// <summary>The wave out get position.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="lpInfo">The lp info.</param>
        /// <param name="uSize">The u size.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutGetPosition(System.IntPtr hWaveOut, out int lpInfo, int uSize);

        /// <summary>The wave out set volume.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="dwVolume">The dw volume.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutSetVolume(System.IntPtr hWaveOut, int dwVolume);

        /// <summary>The wave out get volume.</summary>
        /// <param name="hWaveOut">The h wave out.</param>
        /// <param name="dwVolume">The dw volume.</param>
        /// <returns>The <see cref="int"/>.</returns>
        [System.Runtime.InteropServices.DllImport(mmdll)]
        public static extern int waveOutGetVolume(System.IntPtr hWaveOut, out int dwVolume);

        // structs 

        /// <summary>The wave hdr.</summary>
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct WaveHdr
        {
            /// <summary>The lp data.</summary>
            public System.IntPtr lpData; // pointer to locked data buffer

            /// <summary>The dw buffer length.</summary>
            public int dwBufferLength; // length of data buffer

            /// <summary>The dw bytes recorded.</summary>
            public int dwBytesRecorded; // used for input only

            /// <summary>The dw user.</summary>
            public System.IntPtr dwUser; // for client's use

            /// <summary>The dw flags.</summary>
            public int dwFlags; // assorted flags (see defines)

            /// <summary>The dw loops.</summary>
            public int dwLoops; // loop control counter

            /// <summary>The lp next.</summary>
            public System.IntPtr lpNext; // PWaveHdr, reserved for driver

            /// <summary>The reserved.</summary>
            public int reserved; // reserved for driver
        }
    }
}