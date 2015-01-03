// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompilationSource.cs" company="">
//   
// </copyright>
// <summary>
//   an <see langword="interface" /> that can be implemented by objects that
//   want to provide a customised compilation of a string and possibly extra
//   files, which are stored in a module, but not registered as regular
//   modules (so for <see langword="internal" /> use). This is used by
//   <see cref="Query" /> s to compile the query itself. Note: the module
//   itself gets stored like any other regular module on disk, but it is not
//   registered in the list of modules. The module is used so we can delete
//   all the code again.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     an <see langword="interface" /> that can be implemented by objects that
    ///     want to provide a customised compilation of a string and possibly extra
    ///     files, which are stored in a module, but not registered as regular
    ///     modules (so for <see langword="internal" /> use). This is used by
    ///     <see cref="Query" /> s to compile the query itself. Note: the module
    ///     itself gets stored like any other regular module on disk, but it is not
    ///     registered in the list of modules. The module is used so we can delete
    ///     all the code again.
    /// </summary>
    public interface ICompilationSource
    {
        #region SourceStrings

        /// <summary>
        ///     Gets the list of strings that need to be compiled as source files.
        /// </summary>
        string SourceString { get; }

        /// <summary>
        ///     Gets the list of files that need to be compiled.
        /// </summary>
        System.Collections.Generic.IList<string> SourceFiles { get; }

        /// <summary>
        ///     gets the module to compile to.
        /// </summary>
        Module Module { get; }

        /// <summary>
        ///     Gets the cluster that will contain the result code defined in the
        ///     source file.
        /// </summary>
        NeuronCluster Result { get; }

        /// <summary>
        ///     gets the neuron which we are compiling (it references the result
        ///     cluster). This is provided so that it can be added tot he compilation
        ///     source so that it can be reached from within the code without having
        ///     to do anythign special.
        /// </summary>
        Neuron Source { get; }

        #endregion
    }
}