// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildConditionPatternUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   provides undo features for the conditionPattern parsing.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     provides undo features for the conditionPattern parsing.
    /// </summary>
    public class BuildConditionPatternUndoItem : PatternBaseUndoItem
    {
        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            try
            {
                var iUndo = new ClearConditionPatterUndoItem();
                iUndo.Pattern = Pattern;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                Parsers.ConditionParser.RemoveCondPattern(Pattern);
            }
            catch
            {
                // don't do anyithing with the exception, it's a parse error, so the object isn't parsed again, so it will get a reparse when focus is lost
                // and then display the error (without generating extra undo data).
            }
        }
    }
}