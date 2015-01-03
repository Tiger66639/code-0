// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NeuronIDAttribute.cs" company="">
//   
// </copyright>
// <summary>
//   <para>
//   An attribute that can be applied to classes to indicate that the
//   <see cref="Neuron" />
//   </para>
//   <para>
//   should be automatically created for an empty brain, using the specified
//   <see cref="JaStDev.HAB.Neuron.ID" />
//   </para>
//   <para>
//   value(s). You can optionally also define which type to use for creating
//   the neuron, if no type is specified, the type to which the attribute is
//   supplied is used. Multiple attributes for the same type are allowed.
//   </para>
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     <para>
    ///         An attribute that can be applied to classes to indicate that the
    ///         <see cref="Neuron" />
    ///     </para>
    ///     <para>
    ///         should be automatically created for an empty brain, using the specified
    ///         <see cref="JaStDev.HAB.Neuron.ID" />
    ///     </para>
    ///     <para>
    ///         value(s). You can optionally also define which type to use for creating
    ///         the neuron, if no type is specified, the type to which the attribute is
    ///         supplied is used. Multiple attributes for the same type are allowed.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This attribute is used to declare 'known' neurons such as
    ///     <see cref="PopInstruction" /> or some <see cref="TextNeuron" /> s.
    /// </remarks>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class NeuronIDAttribute : System.Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="NeuronIDAttribute"/> class.</summary>
        /// <param name="id">The id.</param>
        public NeuronIDAttribute(ulong id)
        {
            ID = id;
        }

        /// <summary>Initializes a new instance of the <see cref="NeuronIDAttribute"/> class.</summary>
        /// <param name="id">The id.</param>
        /// <param name="type">The type.</param>
        public NeuronIDAttribute(ulong id, System.Type type)
        {
            ID = id;
            Type = type;
        }

        /// <summary>
        ///     Gets/sets the idea to use
        /// </summary>
        public ulong ID { get; set; }

        // There should come a check for the correct type here
        /// <summary>
        ///     Gets/sets the type of neuron to create.
        /// </summary>
        public System.Type Type { get; set; }
    }
}