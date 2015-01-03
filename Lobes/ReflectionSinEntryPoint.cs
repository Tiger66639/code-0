// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionSinEntryPoint.cs" company="">
//   
// </copyright>
// <summary>
//   Stores all the data for a single entry point used by the
//   <see cref="ReflectionSin" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Stores all the data for a single entry point used by the
    ///     <see cref="ReflectionSin" /> .
    /// </summary>
    public class ReflectionSinEntryPoint
    {
        /// <summary>
        ///     Gets or sets the name of the method that is mapped.
        /// </summary>
        /// <value>
        ///     The name of the method.
        /// </value>
        public string MethodName { get; set; }

        /// <summary>
        ///     Gets or sets the names of the parameter types.
        /// </summary>
        /// <value>
        ///     The parameter types.
        /// </value>
        public System.Collections.Generic.List<string> ParameterTypes { get; set; }

        /// <summary>
        ///     Gets or sets the name of the type that contains the mapped method.
        /// </summary>
        /// <value>
        ///     The name of the type.
        /// </value>
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="ID" /> of the neuron that the method maps
        ///     to.
        /// </summary>
        /// <value>
        ///     The ID.
        /// </value>
        public ulong ID { get; set; }
    }

    /// <summary>The exportable reflection sin entry point.</summary>
    [System.Serializable]
    [System.Xml.Serialization.XmlRoot(ElementName = "LibRef")]
    public class ExportableReflectionSinEntryPoint
    {
        /// <summary>
        ///     Gets or sets the name of the method that is mapped.
        /// </summary>
        /// <value>
        ///     The name of the method.
        /// </value>
        public string AssemblyName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the type that contains the mapped method.
        /// </summary>
        /// <value>
        ///     The name of the type.
        /// </value>
        public string TypeName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the method that is mapped.
        /// </summary>
        /// <value>
        ///     The name of the method.
        /// </value>
        public string MethodName { get; set; }

        /// <summary>
        ///     Gets or sets the name of the method as it should be used in patterns
        ///     and such (it's DisplayTitle prop)
        /// </summary>
        /// <value>
        ///     The name of the mapped.
        /// </value>
        public string MappedName { get; set; }

        /// <summary>
        ///     Gets or sets the names of the parameter types.
        /// </summary>
        /// <value>
        ///     The parameter types.
        /// </value>
        public System.Collections.Generic.List<string> ParameterTypes { get; set; }
    }
}