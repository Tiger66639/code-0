// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuildTextPatternUndoItem.cs" company="">
//   
// </copyright>
// <summary>
//   Undoes the build of a text pattern by clearing the build.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Undoes the build of a text pattern by clearing the build.
    /// </summary>
    public class BuildTextPatternUndoItem : TextPatternBaseUndoItem
    {
        /// <summary>Performs all the actions stored in the undo item, thereby undoing the
        ///     action.</summary>
        /// <param name="caller">The undo managaer that is calling this method.</param>
        public override void Execute(UndoSystem.UndoStore caller)
        {
            try
            {
                var iUndo = new ClearTextPatternBuildUndoItem();
                iUndo.Pattern = Pattern;
                WindowMain.UndoStore.AddCustomUndoItem(iUndo);

                Parsers.InputParser.RemoveInputPattern(Pattern);
            }
            catch
            {
                // don't do anyithing with the exception, it's a parse error, so the object isn't parsed again, so it will get a reparse when focus is lost
                // and then display the error (without generating extra undo data).
            }
        }
    }
}