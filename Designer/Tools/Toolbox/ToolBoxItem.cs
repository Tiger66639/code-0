// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToolBoxItem.cs" company="">
//   
// </copyright>
// <summary>
//   A class that stores all the info for toolbox items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A class that stores all the info for toolbox items.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(NeuronToolBoxItem))]
    [System.Xml.Serialization.XmlInclude(typeof(TypeToolBoxItem))]
    [System.Xml.Serialization.XmlInclude(typeof(InstructionToolBoxItem))]
    public abstract class ToolBoxItem : Data.OwnedObject
    {
        /// <summary>Retrieves the <see cref="Neuron"/> for this toolbox item.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public abstract Neuron GetData();

        /// <summary>Retrieves the type</summary>
        /// <returns>The <see cref="Type"/>.</returns>
        public abstract System.Type GetResultType();

        #region Prop

        /// <summary>
        ///     Gets/sets the category value of the <see cref="ToolBoxITem.Neuron" />
        /// </summary>
        public abstract string Category { get; set; }

        /// <summary>
        ///     Gets the title of the toolboxitem.
        /// </summary>
        public abstract string Title { get; }

        #endregion
    }
}