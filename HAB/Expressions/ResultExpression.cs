// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResultExpression.cs" company="">
//   
// </copyright>
// <summary>
//   An expression that returns a result.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     An expression that returns a result.
    /// </summary>
    /// <remarks>
    ///     A result expression is used by other expressions to retrieve argument
    ///     values. Results can be from a search or a Predefined node.
    /// </remarks>
    public abstract class ResultExpression : Expression
    {
        /// <summary>Calcules the result of this expression and returns this as a list. The
        ///     result should be stored on the<see cref="processor.Mem.ArgumentStack"/> to prevent too many
        ///     duplicate lists.</summary>
        /// <param name="processor">The processor.</param>
        internal abstract void GetValue(Processor processor);

        /// <summary>Returns a list with all the neurons of the input list, except the<see cref="ResultExpressions"/> , which are replaced with their
        ///     results.</summary>
        /// <param name="toConvert">The list to convert all the result expressions from.</param>
        /// <param name="processor">The processor.</param>
        /// <returns>A list of neurons.</returns>
        public static System.Collections.Generic.List<Neuron> Solve(System.Collections.Generic.IList<Neuron> toConvert, 
            Processor processor)
        {
            var iRes = processor.Mem.ArgumentStack.Push();
            if (iRes.Capacity < toConvert.Count)
            {
                iRes.Capacity = toConvert.Count;

                    // make certain that we can at least the arguments without resizing the list.
            }

            try
            {
                foreach (var i in toConvert)
                {
                    var iExp = i as ResultExpression;
                    if (iExp != null)
                    {
                        iExp.GetValue(processor); // getValue returns the value on the stack of the processor.
                    }
                    else
                    {
                        iRes.Add(i);
                    }
                }
            }
            finally
            {
                processor.Mem.ArgumentStack.Pop();
            }

            return iRes;
        }
    }
}