// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteSequence.cs" company="">
//   
// </copyright>
// <summary>
//   Gets a sequence of numbers and completes them.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Gets a sequence of numbers and completes them.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         1: first number, can be <see langword="double" /> or <see langword="int" />
    ///         neuron, determins which type the 2 other numbers must be. 2: second
    ///         number, is made as same type as 1 3: third number, is made as same type
    ///         as 1 4: nr of numbers that need to be generated.
    ///     </para>
    ///     <para>
    ///         check http://www.purplemath.com/modules/nextnumb.htm for more info
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.CompleteSequenceInstruction)]
    public class CompleteSequence : MultiResultInstruction
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CompleteSequenceInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.CompleteSequenceInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without any
        ///     specific number of values.
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return 4;
            }
        }

        /// <summary>performs the task and returns it's result.</summary>
        /// <remarks>Note: when When a result instruction is executed, ( so<see cref="Instruction.Execute"/> is called instead of<see cref="ResultInstruction.GetValues"/> , the result value(s) are
        ///     pushed on the execution stack.</remarks>
        /// <param name="processor"></param>
        /// <param name="list">The list of arguments</param>
        public override void GetValues(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list != null && list.Count >= 4)
            {
                int iGenCount;
                if (list[3] is IntNeuron)
                {
                    iGenCount = ((IntNeuron)list[3]).Value;
                }
                else
                {
                    LogService.Log.LogError(
                        "CompleteSequence.GetValues", 
                        "IntNeuron expected to indicate the nr of values to generate(fourth arg)");
                    return;
                }

                if (list[0] is IntNeuron)
                {
                    CompleteInts(processor, list, iGenCount);
                }
                else if (list[0] is DoubleNeuron)
                {
                    CompleteDoubles(processor, list, iGenCount);
                }
                else
                {
                    LogService.Log.LogError(
                        "CompleteSequence.GetValues", 
                        string.Format("Invalid first argument, IntNeuron or DoubleNeuron expected, found {0}.", list[0]));
                }
            }
            else
            {
                LogService.Log.LogError("CompleteSequence.GetValues", "No arguments specified");
            }
        }

        /// <summary>The complete doubles.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <param name="genCount">The gen count.</param>
        private void CompleteDoubles(Processor processor, System.Collections.Generic.IList<Neuron> list, int genCount)
        {
            var iBase = new double[3];

            for (var i = 0; i < 3; i++)
            {
                if (list[i] is IntNeuron)
                {
                    iBase[i] = ((IntNeuron)list[i]).Value;
                }
                else if (list[i] is DoubleNeuron)
                {
                    iBase[i] = ((DoubleNeuron)list[i]).Value;
                }
                else
                {
                    LogService.Log.LogError("CompleteSequence.CompleteInts", "No arguments specified");
                    return;
                }
            }

            /*
          * check http://www.purplemath.com/modules/nextnumb.htm for more info
          * here's an example calculation:
          * x        |   p(x) = 2x2 − 3x + 2 |	diff1(x) = ( p(x-1) - p(x) ) 	|  diff2(x) = ( diff1(x-1) - diff1(x) )
          * 0.00     | 	         2.00 		
          * 0.10     | 	1.72                 |	0.28 	
          * 0.20 	   |  1.48 	               |  0.24 	                        |  0.04
          * 0.30 	   |  1.28 	               |  0.20 	                        |  0.04
          * 0.40 	   |  1.12 	               |  0.16 	                        |  0.04
          */
            var iDiffs = new double[2];
            iDiffs[0] = iBase[1] - iBase[0];
            iDiffs[1] = iBase[2] - iBase[1];
            var iDifDif = iDiffs[1] - iDiffs[0];
            var iRes = processor.Mem.ArgumentStack.Peek();
            var iDiffCum = iDiffs[1];
            var iBaseCum = iBase[2];
            while (genCount > 0)
            {
                iDiffCum += iDifDif;
                iBaseCum += iDiffCum;
                var iNew = NeuronFactory.GetDouble(iBaseCum);
                Brain.Current.MakeTemp(iNew);
                iRes.Add(iNew);
                genCount--;
            }
        }

        /// <summary>Completes the sequence using int.</summary>
        /// <param name="processor">The processor.</param>
        /// <param name="list">The list.</param>
        /// <param name="genCount">The nr of items to generate.</param>
        private void CompleteInts(Processor processor, System.Collections.Generic.IList<Neuron> list, int genCount)
        {
            var iBase = new int[3];

            for (var i = 0; i < 3; i++)
            {
                if (list[i] is IntNeuron)
                {
                    iBase[i] = ((IntNeuron)list[i]).Value;
                }
                else if (list[i] is DoubleNeuron)
                {
                    iBase[i] = (int)((DoubleNeuron)list[i]).Value;
                }
                else
                {
                    LogService.Log.LogError("CompleteSequence.CompleteInts", "No arguments specified");
                    return;
                }
            }

            /*
          * check http://www.purplemath.com/modules/nextnumb.htm for more info
          * here's an example calculation:
          * x        |   p(x) = 2x2 − 3x + 2 |	diff1(x) = ( p(x-1) - p(x) ) 	|  diff2(x) = ( diff1(x-1) - diff1(x) )
          * 0.00     | 	         2.00 		
          * 0.10     | 	1.72                 |	0.28 	
          * 0.20 	   |  1.48 	               |  0.24 	                        |  0.04
          * 0.30 	   |  1.28 	               |  0.20 	                        |  0.04
          * 0.40 	   |  1.12 	               |  0.16 	                        |  0.04
          */
            var iDiffs = new int[2];
            iDiffs[0] = iBase[1] - iBase[0];
            iDiffs[1] = iBase[2] - iBase[1];
            var iDifDif = iDiffs[1] - iDiffs[0];
            var iRes = processor.Mem.ArgumentStack.Peek();
            var iDiffCum = iDiffs[1];
            var iBaseCum = iBase[2];
            while (genCount > 0)
            {
                iDiffCum += iDifDif;
                iBaseCum += iDiffCum;
                var iNew = NeuronFactory.GetInt(iBaseCum);
                Brain.Current.MakeTemp(iNew);
                iRes.Add(iNew);
                genCount--;
            }
        }
    }
}