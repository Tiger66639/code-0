// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WaveOut.cs" company="">
//   
// </copyright>
// <summary>
//   The wave out helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WaveLib
{
    /// <summary>The wave out helper.</summary>
    internal class WaveOutHelper
    {
        /// <summary>The try.</summary>
        /// <param name="err">The err.</param>
        /// <exception cref="Exception"></exception>
        public static void Try(int err)
        {
            if (err != WaveNative.MMSYSERR_NOERROR)
            {
                throw new System.Exception(err.ToString());
            }
        }
    }

    /// <summary>The buffer fill event handler.</summary>
    /// <param name="data">The data.</param>
    /// <param name="size">The size.</param>
    public delegate void BufferFillEventHandler(System.IntPtr data, int size);

    /// <summary>The wave out buffer.</summary>
    internal class WaveOutBuffer : System.IDisposable
    {
        /// <summary>The m_ header.</summary>
        private WaveNative.WaveHdr m_Header;

        /// <summary>The m_ header data handle.</summary>
        private System.Runtime.InteropServices.GCHandle m_HeaderDataHandle;

        /// <summary>The m_ header handle.</summary>
        private System.Runtime.InteropServices.GCHandle m_HeaderHandle;

        /// <summary>The m_ playing.</summary>
        private bool m_Playing;

        /// <summary>The next buffer.</summary>
        public WaveOutBuffer NextBuffer;

        /// <summary>The m_ header data.</summary>
        private readonly byte[] m_HeaderData;

        /// <summary>The m_ play event.</summary>
        private readonly System.Threading.AutoResetEvent m_PlayEvent = new System.Threading.AutoResetEvent(false);

        /// <summary>The m_ wave out.</summary>
        private readonly System.IntPtr m_WaveOut;

        /// <summary>Initializes a new instance of the <see cref="WaveOutBuffer"/> class.</summary>
        /// <param name="waveOutHandle">The wave out handle.</param>
        /// <param name="size">The size.</param>
        public WaveOutBuffer(System.IntPtr waveOutHandle, int size)
        {
            m_WaveOut = waveOutHandle;

            m_HeaderHandle = System.Runtime.InteropServices.GCHandle.Alloc(
                m_Header, 
                System.Runtime.InteropServices.GCHandleType.Pinned);
            m_Header.dwUser = (System.IntPtr)System.Runtime.InteropServices.GCHandle.Alloc(this);
            m_HeaderData = new byte[size];
            m_HeaderDataHandle = System.Runtime.InteropServices.GCHandle.Alloc(
                m_HeaderData, 
                System.Runtime.InteropServices.GCHandleType.Pinned);
            m_Header.lpData = m_HeaderDataHandle.AddrOfPinnedObject();
            m_Header.dwBufferLength = size;
            WaveOutHelper.Try(
                WaveNative.waveOutPrepareHeader(
                    m_WaveOut, 
                    ref m_Header, 
                    System.Runtime.InteropServices.Marshal.SizeOf(m_Header)));
        }

        /// <summary>Gets the size.</summary>
        public int Size
        {
            get
            {
                return m_Header.dwBufferLength;
            }
        }

        /// <summary>Gets the data.</summary>
        public System.IntPtr Data
        {
            get
            {
                return m_Header.lpData;
            }
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            if (m_Header.lpData != System.IntPtr.Zero)
            {
                WaveNative.waveOutUnprepareHeader(
                    m_WaveOut, 
                    ref m_Header, 
                    System.Runtime.InteropServices.Marshal.SizeOf(m_Header));
                m_HeaderHandle.Free();
                m_Header.lpData = System.IntPtr.Zero;
            }

            m_PlayEvent.Close();
            if (m_HeaderDataHandle.IsAllocated)
            {
                m_HeaderDataHandle.Free();
            }

            System.GC.SuppressFinalize(this);
        }

        /// <summary>The wave out proc.</summary>
        /// <param name="hdrvr">The hdrvr.</param>
        /// <param name="uMsg">The u msg.</param>
        /// <param name="dwUser">The dw user.</param>
        /// <param name="wavhdr">The wavhdr.</param>
        /// <param name="dwParam2">The dw param 2.</param>
        internal static void WaveOutProc(
            System.IntPtr hdrvr, 
            int uMsg, 
            int dwUser, 
            ref WaveNative.WaveHdr wavhdr, 
            int dwParam2)
        {
            if (uMsg == WaveNative.MM_WOM_DONE)
            {
                try
                {
                    var h = (System.Runtime.InteropServices.GCHandle)wavhdr.dwUser;
                    var buf = (WaveOutBuffer)h.Target;
                    buf.OnCompleted();
                }
                catch
                {
                }
            }
        }

        /// <summary>Finalizes an instance of the <see cref="WaveOutBuffer"/> class. </summary>
        ~WaveOutBuffer()
        {
            Dispose();
        }

        /// <summary>The play.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Play()
        {
            lock (this)
            {
                m_PlayEvent.Reset();
                m_Playing = WaveNative.waveOutWrite(
                    m_WaveOut, 
                    ref m_Header, 
                    System.Runtime.InteropServices.Marshal.SizeOf(m_Header)) == WaveNative.MMSYSERR_NOERROR;
                return m_Playing;
            }
        }

        /// <summary>The wait for.</summary>
        public void WaitFor()
        {
            if (m_Playing)
            {
                m_Playing = m_PlayEvent.WaitOne();
            }
            else
            {
                System.Threading.Thread.Sleep(0);
            }
        }

        /// <summary>The on completed.</summary>
        public void OnCompleted()
        {
            m_PlayEvent.Set();
            m_Playing = false;
        }
    }

    /// <summary>The wave out player.</summary>
    public class WaveOutPlayer : System.IDisposable
    {
        /// <summary>The m_ buffers.</summary>
        private WaveOutBuffer m_Buffers; // linked list

        /// <summary>The m_ current buffer.</summary>
        private WaveOutBuffer m_CurrentBuffer;

        /// <summary>The m_ fill proc.</summary>
        private BufferFillEventHandler m_FillProc;

        /// <summary>The m_ finished.</summary>
        private bool m_Finished;

        /// <summary>The m_ thread.</summary>
        private System.Threading.Thread m_Thread;

        /// <summary>The m_ wave out.</summary>
        private System.IntPtr m_WaveOut;

        /// <summary>The m_ buffer proc.</summary>
        private readonly WaveNative.WaveDelegate m_BufferProc = WaveOutBuffer.WaveOutProc;

        /// <summary>The m_zero.</summary>
        private readonly byte m_zero;

        /// <summary>Initializes a new instance of the <see cref="WaveOutPlayer"/> class.</summary>
        /// <param name="device">The device.</param>
        /// <param name="format">The format.</param>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="bufferCount">The buffer count.</param>
        /// <param name="fillProc">The fill proc.</param>
        public WaveOutPlayer(
            int device, 
            WaveFormat format, 
            int bufferSize, 
            int bufferCount, 
            BufferFillEventHandler fillProc)
        {
            m_zero = format.wBitsPerSample == 8 ? (byte)128 : (byte)0;
            m_FillProc = fillProc;
            WaveOutHelper.Try(
                WaveNative.waveOutOpen(out m_WaveOut, device, format, m_BufferProc, 0, WaveNative.CALLBACK_FUNCTION));
            AllocateBuffers(bufferSize, bufferCount);
            m_Thread = new System.Threading.Thread(ThreadProc);
            m_Thread.Start();
        }

        /// <summary>Gets the device count.</summary>
        public static int DeviceCount
        {
            get
            {
                return WaveNative.waveOutGetNumDevs();
            }
        }

        /// <summary>The dispose.</summary>
        public void Dispose()
        {
            if (m_Thread != null)
            {
                try
                {
                    m_Finished = true;
                    if (m_WaveOut != System.IntPtr.Zero)
                    {
                        WaveNative.waveOutReset(m_WaveOut);
                    }

                    m_Thread.Join();
                    m_FillProc = null;
                    FreeBuffers();
                    if (m_WaveOut != System.IntPtr.Zero)
                    {
                        WaveNative.waveOutClose(m_WaveOut);
                    }
                }
                finally
                {
                    m_Thread = null;
                    m_WaveOut = System.IntPtr.Zero;
                }
            }

            System.GC.SuppressFinalize(this);
        }

        /// <summary>Finalizes an instance of the <see cref="WaveOutPlayer"/> class. </summary>
        ~WaveOutPlayer()
        {
            Dispose();
        }

        /// <summary>The thread proc.</summary>
        private void ThreadProc()
        {
            while (!m_Finished)
            {
                Advance();
                if (m_FillProc != null && !m_Finished)
                {
                    m_FillProc(m_CurrentBuffer.Data, m_CurrentBuffer.Size);
                }
                else
                {
                    // zero out buffer
                    var v = m_zero;
                    var b = new byte[m_CurrentBuffer.Size];
                    for (var i = 0; i < b.Length; i++)
                    {
                        b[i] = v;
                    }

                    System.Runtime.InteropServices.Marshal.Copy(b, 0, m_CurrentBuffer.Data, b.Length);
                }

                m_CurrentBuffer.Play();
            }

            WaitForAllBuffers();
        }

        /// <summary>The allocate buffers.</summary>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="bufferCount">The buffer count.</param>
        private void AllocateBuffers(int bufferSize, int bufferCount)
        {
            FreeBuffers();
            if (bufferCount > 0)
            {
                m_Buffers = new WaveOutBuffer(m_WaveOut, bufferSize);
                var Prev = m_Buffers;
                try
                {
                    for (var i = 1; i < bufferCount; i++)
                    {
                        var Buf = new WaveOutBuffer(m_WaveOut, bufferSize);
                        Prev.NextBuffer = Buf;
                        Prev = Buf;
                    }
                }
                finally
                {
                    Prev.NextBuffer = m_Buffers;
                }
            }
        }

        /// <summary>The free buffers.</summary>
        private void FreeBuffers()
        {
            m_CurrentBuffer = null;
            if (m_Buffers != null)
            {
                var First = m_Buffers;
                m_Buffers = null;

                var Current = First;
                do
                {
                    var Next = Current.NextBuffer;
                    Current.Dispose();
                    Current = Next;
                }
                while (Current != First);
            }
        }

        /// <summary>The advance.</summary>
        private void Advance()
        {
            m_CurrentBuffer = m_CurrentBuffer == null ? m_Buffers : m_CurrentBuffer.NextBuffer;
            m_CurrentBuffer.WaitFor();
        }

        /// <summary>The wait for all buffers.</summary>
        private void WaitForAllBuffers()
        {
            var Buf = m_Buffers;
            while (Buf.NextBuffer != m_Buffers)
            {
                Buf.WaitFor();
                Buf = Buf.NextBuffer;
            }
        }
    }
}