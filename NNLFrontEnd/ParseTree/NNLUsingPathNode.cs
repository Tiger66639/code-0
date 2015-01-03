// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLUsingPathNode.cs" company="">
//   
// </copyright>
// <summary>
//   a path node specific for using clauses, so that the pathnode knows it is
//   a using clause (which is required cause using clauses can't themselves
//   look into the using clauses to find it's ref, cause then we get a
//   possible endless loop (when the item isn't defined).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a path node specific for using clauses, so that the pathnode knows it is
    ///     a using clause (which is required cause using clauses can't themselves
    ///     look into the using clauses to find it's ref, cause then we get a
    ///     possible endless loop (when the item isn't defined).
    /// </summary>
    internal class NNLUsingPathNode : NNLPathNode
    {
    }
}