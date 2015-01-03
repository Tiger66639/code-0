// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompilerRenderDict.cs" company="">
//   
// </copyright>
// <summary>
//   implements the <see cref="IRenderDict" /> <see langword="interface" /> for
//   the <see cref="Parsers.NNLModuleCompiler" /> . We put this in a single class so
//   that it can be used accros the application (should remain the same for
//   all)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     implements the <see cref="IRenderDict" /> <see langword="interface" /> for
    ///     the <see cref="Parsers.NNLModuleCompiler" /> . We put this in a single class so
    ///     that it can be used accros the application (should remain the same for
    ///     all)
    /// </summary>
    internal class CompilerRenderDict : Parsers.IRendererDict
    {
        /// <summary>The f default.</summary>
        private static readonly CompilerRenderDict fDefault = new CompilerRenderDict();

        /// <summary>
        ///     gets the default object
        /// </summary>
        public static CompilerRenderDict Default
        {
            get
            {
                return fDefault;
            }
        }

        #region IRendererDict Members

        /// <summary>Declares the specified neuron as a 'module property' that the user
        ///     should be able to edit in an editor.</summary>
        /// <param name="item">The item to add as module property</param>
        /// <param name="moduleName">The module Name.</param>
        /// <param name="settings">The settings for the propery. Currently include -The title of the
        ///     property -The tooltip for the property. -The type of the property
        ///     (current value are 'boolprop', 'valueprop' and 'listprop'</param>
        public void SetModuleProp(Neuron item, string moduleName, string settings)
        {
            BrainData.Current.DesignerData.ModulePropIds.Add(item.ID);
            var iInfo = BrainData.Current.NeuronInfo[item];
            iInfo.Category = moduleName;
            var iCustomData = new System.IO.MemoryStream();

                // the settings is stored in the custom data field, which is a memory stream.
            var iWriter = new System.IO.BinaryWriter(iCustomData);
            iWriter.Write(settings);
            iInfo.CustomData = iCustomData;
        }

        #endregion

        #region IRendererDict Members

        /// <summary>assignes the <paramref name="name"/> to the specified neuron. the
        ///     string can be null.</summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        public void SetName(Neuron item, string name)
        {
            var iData = BrainData.Current.NeuronInfo[item];
            if (string.IsNullOrEmpty(iData.Title) && !(item is ValueNeuron))
            {
                // don't store a name to a value neuron, they always display the value. Doing this would raise an error if it were a text. The value itself is usually assigned duirng construction.
                iData.SetTitleNoUndo(name);

                    // don't generate any undo data. This creates to much data and is a problem for the topic patterns.
            }
        }

        /// <summary>Gets the name for a neuron.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public string GetName(ulong item)
        {
            return BrainData.Current.NeuronInfo.GetDisplayTitleFor(item);
        }

        #endregion
    }
}