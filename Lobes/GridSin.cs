// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GridSin.cs" company="">
//   
// </copyright>
// <summary>
//   Event arguments for the GridOutput event.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{

    #region GridOutput event types

    /// <summary>
    ///     Event arguments for the GridOutput event.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.GridSin, typeof(Neuron))]
    public class GridOutputData
    {
        /// <summary>Gets or sets the x.</summary>
        public int X { get; set; }

        /// <summary>Gets or sets the y.</summary>
        public int Y { get; set; }

        /// <summary>Gets or sets the values.</summary>
        public System.Collections.Generic.List<Neuron> Values { get; set; }
    }

    #endregion

    /// <summary>
    ///     a sensory interface that provides a technique to process items in a grid like fashion. All input and output happens
    ///     in pairs of coordinates + values. So an input can be: pos x, pos y, value1, value2.  Same for output.
    ///     All the data: coordinates and values, should always be supplied with a single 'output' statement so that they all
    ///     arrive
    ///     in the output handler at the same time.  This is required to know which output value is a coordinate and which is a
    ///     value.
    /// </summary>
    public class GridSin : Sin
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.GridSin;
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width of the images to process.
        /// </summary>
        /// <remarks>
        ///     When an image is processed that has a different with value as that of this imagesin, the image is resized.
        ///     when there is no with relationship yet defined on this neuron, it is created
        /// </remarks>
        public int Width
        {
            get
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Width) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(0);
                    Brain.Current.Add(iFound);
                    Link.SetFirstOutTo(this, iFound, (ulong)PredefinedNeurons.Width);
                }

                return iFound.Value;
            }

            set
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Width) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(value);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Width, iFound);
                }
                else
                {
                    iFound.Value = value;
                }
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the Height of the images to process.
        /// </summary>
        /// <remarks>
        ///     When an image is processed that has a different Height value as that of this imagesin, the image is resized.
        ///     when there is no Height relationship yet defined on this neuron, it is created
        /// </remarks>
        public int Height
        {
            get
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Height) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(0);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Height, iFound);
                }

                return iFound.Value;
            }

            set
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Height) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(value);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Height, iFound);
                }
                else
                {
                    iFound.Value = value;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Raised when the TextSin has got some text it wants to output.
        /// </summary>
        public event OutputEventHandler<GridOutputData> GridOutput;

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend"></param>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            if (GridOutput != null)
            {
                var iData = new OutputEventArgs<GridOutputData>();
                iData.Data = toSend;
                var iX = Instruction.GetAsInt(toSend[0]);
                var iY = Instruction.GetAsInt(toSend[1]);
                if (iX.HasValue && iY.HasValue)
                {
                    iData.Value.X = iX.Value;
                    iData.Value.Y = iY.Value;
                    iData.Value.Values = new System.Collections.Generic.List<Neuron>();
                    for (var i = 2; i < toSend.Count; i++)
                    {
                        iData.Value.Values.Add(toSend[i]);
                    }

                    GridOutput(this, iData);
                }
                else
                {
                    LogService.Log.LogError(
                        "GridSin.Output", 
                        "Invalid output format: expected int (x pos), int (y pos), any (0, 1 or more values)");
                }
            }
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            // nothig to flush.
        }

        /// <summary>sends the specified data to the grid sin.</summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="values"></param>
        /// <param name="proc">The proc.</param>
        public void Process(int x, int y, System.Collections.Generic.IEnumerable<Neuron> values, Processor proc)
        {
            var iXList = Factories.Default.NLists.GetBuffer();
            var iYList = Factories.Default.NLists.GetBuffer();
            var iValues = Factories.Default.NLists.GetBuffer();
            var iX = NeuronFactory.GetInt(x);
            var iY = NeuronFactory.GetInt(y);
            Brain.Current.Add(iX);
            Brain.Current.Add(iY);
            iXList.Add(iX);
            iYList.Add(iY);
            iValues.AddRange(values);
            proc.Mem.ParametersStack.Push(iValues);
            proc.Mem.ParametersStack.Push(iYList);
            proc.Mem.ParametersStack.Push(iXList);
            var iToCall = RulesCluster;
            if (iToCall != null)
            {
                ProcessCall(iToCall, proc, Text);
            }
            else
            {
                LogService.Log.LogError(
                    "GridSin.Process", 
                    "no 'rules' cluster attached to the gridsin: don't know what to execute.");
            }
        }

        /// <summary>sends the specified data to the grid sin.</summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="value">The value.</param>
        /// <param name="proc">The proc.</param>
        public void Process(int x, int y, Neuron value, Processor proc)
        {
            var iXList = Factories.Default.NLists.GetBuffer();
            var iYList = Factories.Default.NLists.GetBuffer();
            var iValues = Factories.Default.NLists.GetBuffer();
            var iX = NeuronFactory.GetInt(x);
            var iY = NeuronFactory.GetInt(y);
            Brain.Current.Add(iX);
            Brain.Current.Add(iY);
            iXList.Add(iX);
            iYList.Add(iY);
            iValues.Add(value);
            proc.Mem.ParametersStack.Push(iValues);
            proc.Mem.ParametersStack.Push(iYList);
            proc.Mem.ParametersStack.Push(iXList);
            var iToCall = RulesCluster;
            if (iToCall != null)
            {
                ProcessCall(iToCall, proc, Text);
            }
            else
            {
                LogService.Log.LogError(
                    "GridSin.Process", 
                    "no 'rules' cluster attached to the gridsin: don't know what to execute.");
            }
        }
    }
}