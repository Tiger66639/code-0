// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRendererDict.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that should be implemented by objects able
//   to compile nnl modules. This functions as a callback to find external
//   neurons and assign names and so.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     an <see langword="interface" /> that should be implemented by objects able
    ///     to compile nnl modules. This functions as a callback to find external
    ///     neurons and assign names and so.
    /// </summary>
    public interface IRendererDict
    {
        /// <summary>assignes the <paramref name="name"/> to the specified neuron. the
        ///     string can be null.</summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        void SetName(Neuron item, string name);

        /// <summary>Gets the name for a neuron.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        string GetName(ulong item);

        /// <summary>Declares the specified neuron as a 'module property' that the user
        ///     should be able to edit in an editor.</summary>
        /// <param name="item">The item to add as module property</param>
        /// <param name="moduleName">Name of the module.</param>
        /// <param name="settings">The settings for the propery. Currently include -The title of the
        ///     property -The tooltip for the property. -The type of the property
        ///     (current value are 'boolprop', 'valueprop' and 'listprop'</param>
        void SetModuleProp(Neuron item, string moduleName, string settings);
    }
}