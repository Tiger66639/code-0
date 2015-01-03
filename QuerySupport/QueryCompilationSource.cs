// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryCompilationSource.cs" company="">
//   
// </copyright>
// <summary>
//   provides the data for the compiler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Queries
{
    /// <summary>
    ///     provides the data for the compiler.
    /// </summary>
    internal class QueryCompilationSource : Parsers.ICompilationSource
    {
        /// <summary>The f source.</summary>
        private readonly Query fSource;

        /// <summary>Initializes a new instance of the <see cref="QueryCompilationSource"/> class. Initializes a new instance of the<see cref="QueryCompilationSource"/> class.</summary>
        /// <param name="source">The source.</param>
        /// <param name="toCompile">The to Compile.</param>
        /// <param name="extraFiles">The extra Files.</param>
        /// <param name="result">The result.</param>
        public QueryCompilationSource(
            Query source, 
            string toCompile, System.Collections.Generic.IList<string> extraFiles, 
            NeuronCluster result)
        {
            fSource = source;
            SourceString = toCompile;
            SourceFiles = extraFiles;
            Result = result;
        }

        #region SourceString

        /// <summary>
        ///     Gets the list of strings that need to be compiled as source files.
        /// </summary>
        public string SourceString { get; private set; }

        #endregion

        #region SourceFiles

        /// <summary>
        ///     Gets the list of strings that need to be compiled as source files.
        /// </summary>
        public System.Collections.Generic.IList<string> SourceFiles { get; private set; }

        #endregion

        #region ICompilationSource Members

        /// <summary>
        ///     gets the module to compile to.
        /// </summary>
        public Module Module
        {
            get
            {
                return fSource.Module;
            }
        }

        #endregion

        /// <summary>
        ///     gets the query for which we are compiling.
        /// </summary>
        public Neuron Source
        {
            get
            {
                return fSource;
            }
        }

        #region Result

        /// <summary>
        ///     Gets the cluster that will contain the result code defined in the
        ///     source file.
        /// </summary>
        public NeuronCluster Result { get; private set; }

        #endregion
    }
}