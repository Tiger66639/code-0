// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebugChild.cs" company="">
//   
// </copyright>
// <summary>
//   A <see cref="DebugRef" /> that points to the child of a cluster.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A <see cref="DebugRef" /> that points to the child of a cluster.
    /// </summary>
    public class DebugChild : DebugRef
    {
        /// <summary>Initializes a new instance of the <see cref="DebugChild"/> class.</summary>
        /// <param name="child">The id of the child to reference.</param>
        /// <param name="owner">The owner of this reference.</param>
        public DebugChild(ulong child, DebugNeuron owner)
        {
            Owner = owner;
            PointsTo = new DebugNeuron(Brain.Current[child]);
        }

        /// <summary>Initializes a new instance of the <see cref="DebugChild"/> class.</summary>
        /// <param name="child">The child.</param>
        /// <param name="owner">The owner.</param>
        public DebugChild(Neuron child, DebugNeuron owner)
        {
            Owner = owner;
            PointsTo = new DebugNeuron(child);
        }
    }
}