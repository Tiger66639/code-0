// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleResultExpression.cs" company="">
//   
// </copyright>
// <summary>
//   A base class for Expressions that, when executed, simply put there result
//   on the stack.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     A base class for Expressions that, when executed, simply put there result
    ///     on the stack.
    /// </summary>
    public abstract class SimpleResultExpression : ResultExpression
    {
        /// <summary>Executes this expression. The result is simply discarded.</summary>
        /// <param name="handler">The processor to execute the statements on.</param>
        protected internal override void Execute(Processor handler)
        {
            var iRes = handler.Mem.ArgumentStack.Push();
            try
            {
                GetValue(handler);
            }
            finally
            {
                handler.Mem.ArgumentStack.Pop();
            }
        }
    }
}