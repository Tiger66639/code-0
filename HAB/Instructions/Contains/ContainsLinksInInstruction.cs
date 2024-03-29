﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContainsLinksInInstruction.cs" company="">
//   
// </copyright>
// <summary>
//   Returns <see langword="true" /> if the first argument, contains any
//   incomming links with the specfiied neurons as meaning, otherwise,
//   <see langword="false" /> is returned.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     Returns <see langword="true" /> if the first argument, contains any
    ///     incomming links with the specfiied neurons as meaning, otherwise,
    ///     <see langword="false" /> is returned.
    /// </summary>
    /// <remarks>
    ///     Arg: 1: The neuron to check the list of incomming links ...meaning id's
    ///     that need to be found.
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.ContainsLinksInInstruction)]
    public class ContainsLinksInInstruction : SingleResultInstruction, ICalculateBool
    {
        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsLinksInInstruction" />
        ///     .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ContainsLinksInInstruction;
            }
        }

        #endregion

        /// <summary>
        ///     Returns the number of arguments that are required by this
        ///     instruction.
        /// </summary>
        /// <remarks>
        ///     A value of -1 indicates that a list of neurons is allowed, without
        ///     any specific number of values.
        /// </remarks>
        /// <value>
        /// </value>
        public override int ArgCount
        {
            get
            {
                return -1;
            }
        }

        #region ICalculateBool Members

        /// <summary>Calculate the <see langword="int"/> value and return it.</summary>
        /// <param name="processor"></param>
        /// <param name="list"></param>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool CalculateBool(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (list.Count >= 1)
            {
                var iToCheck = list[0];
                if (iToCheck != null)
                {
                    if (iToCheck.LinksInIdentifier != null)
                    {
                        // if there are no links-in, can't contain the link.
                        if (iToCheck.fSortedLinksIn != null)
                        {
                            for (var i = 1; i < list.Count; i++)
                            {
                                if (iToCheck.fSortedLinksIn.ContainsKey(list[i].ID))
                                {
                                    return true;
                                }
                            }

                            return false;
                        }

                        var iItemsToCheck = new System.Collections.Generic.HashSet<ulong>();

                            // we use the id's cause the links have the id's stored but need to get the neurons, so it's faster to simly check the ids.
                        for (var i = 1; i < list.Count; i++)
                        {
                            iItemsToCheck.Add(list[i].ID);
                        }

                        using (var iLinks = iToCheck.LinksIn)
                        {
                            var iFound =
                                (from i in iLinks where iItemsToCheck.Contains(i.MeaningID) select i).FirstOrDefault();
                            return iFound != null;
                        }
                    }

                    return false;
                }

                LogService.Log.LogError(
                    "ContainsLinksInInstruction.InternalGetValue", 
                    "Invalid first argument, neuron expected.");
            }
            else
            {
                LogService.Log.LogError(
                    "ContainsLinksInInstruction.InternalGetValue", 
                    "Invalid nr of arguments specified");
            }

            return false;
        }

        #endregion

        /// <summary>Gets the actual value.</summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="list">the list to get the nr of items from.</param>
        /// <returns>The result of the instruction.</returns>
        protected override Neuron InternalGetValue(Processor processor, System.Collections.Generic.IList<Neuron> list)
        {
            if (CalculateBool(processor, list))
            {
                return Brain.Current.TrueNeuron;
            }

            return Brain.Current.FalseNeuron;
        }
    }
}