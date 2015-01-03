// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AudioSin.cs" company="">
//   
// </copyright>
// <summary>
//   A Sensory interface for audio.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A Sensory interface for audio.
    /// </summary>
    /// <remarks>
    ///     Audio input is handled by the <see cref="AudioSin.Process" /> function.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.AudioSin, typeof(Neuron))]
    public class AudioSin : Sin
    {
        #region Events

        /// <summary>
        ///     Raised when the <see cref="AudioSin" /> has finished processing a data
        ///     block that was provided through <see cref="AudioSin.Process" />.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Don't try to add data again before this event was raised.
        ///     </para>
        ///     <para>
        ///         The <see cref="short" /> array that is returned contains the frequency
        ///         set that can be released again.
        ///     </para>
        /// </remarks>
        public event OutputEventHandler<short[]> IsReady;

        #endregion

        /// <summary>Processes the specified data by transforming the list of audio samples into it's representive list
        ///     of frequency values which are turned into neurons.</summary>
        /// <remarks>To get the frequency values, an FFt function is used.  See: http://www.codeproject.com/KB/recipes/howtofft.aspx
        ///     for more info on how to use this.</remarks>
        /// <param name="data">The data.</param>
        public void Process(short[] data)
        {
            if (fIsProcessing)
            {
                throw new System.InvalidOperationException("Previous audio sample has not yet been processed.");
            }

            fIsProcessing = true;
            fData = data;

            var iEntry = FindFirstOut((ulong)PredefinedNeurons.EntryPoints) as NeuronCluster;
            if (iEntry == null)
            {
                // if for some reason, the entrypoints have not yet been created, do it now.  Should not be required though.
                ReBuildStartPoints(data.Length);
            }

            // using (ChildrenAccessor iList = iEntry.Children)
            // {
            // for (int i = 0; i < data.Length; i++)
            // {
            // ulong iId = iList[i];
            // DoubleNeuron iVal = (DoubleNeuron)Brain.Current[iId];
            // iVal.Value = data[i].Re;
            // }
            // }
            System.Diagnostics.Debug.Assert(iEntry != null);
        }

        /// <summary>rebuild the starting points of this sin so that they have the specified lenght.</summary>
        /// <remarks>Only the required neurons are removed or created.</remarks>
        /// <param name="length">The new nr of sequence slots that this audio sin can handle.</param>
        private void ReBuildStartPoints(int length)
        {
            var iEntry = FindFirstOut((ulong)PredefinedNeurons.EntryPoints) as NeuronCluster;
            if (iEntry == null)
            {
                iEntry = NeuronFactory.GetCluster();
                Brain.Current.Add(iEntry);
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.EntryPoints, iEntry);
            }

            using (var iList = iEntry.ChildrenW)
            {
                while (iList.Count > length)
                {
                    // do alock for each statement cause we are going to do deletes and adds, which are cache write operations.
                    Brain.Current.Delete(iList[iList.Count - 1]);
                    iList.RemoveAt(iList.Count);
                }

                while (iList.Count < length)
                {
                    var iNew = NeuronFactory.GetDouble();
                    Brain.Current.Add(iNew);
                    iList.Add(iNew);
                }
            }

            var iToCall = FindFirstOut((ulong)PredefinedNeurons.EntryPointsCreated) as NeuronCluster;
            if (iToCall != null)
            {
                var iProc = ProcessorFactory.GetProcessor();
                iProc.CallSingle(iToCall);
            }
        }

        #region Fields

        /// <summary>The f is processing.</summary>
        private bool fIsProcessing; // when true, the brain is processing

        /// <summary>The f data.</summary>
        private short[] fData; // current data being processed.

        #endregion

        #region Overrides

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.AudioSin;
            }
        }

        #endregion

        /// <summary>The output.</summary>
        /// <param name="toSend">The to send.</param>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                if (i == this)
                {
                    fIsProcessing = false;

                    // if (IsReady != null)
                    // IsReady(this, new OutputEventArgs<ComplexF[]>() { Data = toSend, Value = fData });
                    fData = null; // not really required, just clean.
                }
            }
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        /// <remarks>
        ///     nothing to do
        /// </remarks>
        public override void Flush()
        {
            // nothing to do.
        }

        #endregion
    }
}