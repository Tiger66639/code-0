// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClearConditionPatterUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   undoes the 'clear' operation on a parse, so it reparses the condition
//   pattern.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     undoes the 'clear' operation on a parse, so it reparses the condition
    ///     pattern.
    /// </summary>
    public class ClearConditionPatterUndoItem : PatternBaseUndoItem
    {
        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            var iUndo = new BuildConditionPatternUndoItem();
            iUndo.Pattern = Pattern;
            WindowMain.UndoStore.AddCustomUndoItem(iUndo);

            try
            {
                var iParser = new Parsers.ConditionParser(Pattern, string.Empty);
                iParser.Parse();
            }
            catch
            {
                // don't do anyithing with the exception, it's a parse error, so the object isn't parsed again, so it will get a reparse when focus is lost
                // and then display the error (without generating extra undo data).
            }
        }
    }
}