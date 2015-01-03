// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPILinkIn.cs" company="">
//   
// </copyright>
// <summary>
//   Allows to go from the current path item to the neuron found on an
//   incomming link, for a speicific meaning
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Allows to go from the current path item to the neuron found on an
    ///     incomming link, for a speicific meaning
    /// </summary>
    public class DPILinkIn : DPILink
    {
        /// <summary>Initializes a new instance of the <see cref="DPILinkIn"/> class.</summary>
        public DPILinkIn()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DPILinkIn"/> class.</summary>
        /// <param name="meaning">The meaning.</param>
        public DPILinkIn(ulong meaning)
            : base(meaning)
        {
        }

        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get from.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }
    }
}