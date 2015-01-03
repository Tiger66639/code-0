// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstructionToolBoxItem.cs" company="">
//   
// </copyright>
// <summary>
//   This is a special type of <see cref="NeuronToolBoxItem" /> that is
//   generated automatically for each <see cref="Instruction" /> in the brain.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     This is a special type of <see cref="NeuronToolBoxItem" /> that is
    ///     generated automatically for each <see cref="Instruction" /> in the brain.
    /// </summary>
    public class InstructionToolBoxItem : NeuronToolBoxItem
    {
        /// <summary>The get data.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public override Neuron GetData()
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.ArgumentsList;
            Expression iStatement;
            if (Item is ResultInstruction)
            {
                iStatement = new ResultStatement((Instruction)Item, iCluster);
            }
            else
            {
                iStatement = new Statement((Instruction)Item, iCluster);

                    // this constructor also registers with the brain, otherwise can't create links.
            }

            return iStatement;
        }
    }
}