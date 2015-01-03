// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SelectedPatternItemsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Maintains all the selected items in a TextPatternEditor.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Maintains all the selected items in a TextPatternEditor.
    /// </summary>
    internal class SelectedPatternItemsCollection : EditorItemSelectionList<PatternEditorItem>
    {
        /// <summary>Gets the clipboard ID that's best suied, based on the content of the
        ///     selection list.</summary>
        /// <returns>The <see cref="string"/>.</returns>
        internal string GetClipboardID()
        {
            var iType = this[0].GetType();
            for (var i = 1; i < Count; i++)
            {
                if (iType != this[i].GetType())
                {
                    if (iType != typeof(InputPattern) || iType != typeof(OutputPattern))
                    {
                        // textpattern and output can be intermingled, cause they are simple text.
                        iType = null;
                        break;
                    }
                }
            }

            if (iType == null)
            {
                return Properties.Resources.MIXEDTEXTPATTERNFORMAT;
            }

            if (iType == typeof(InputPattern))
            {
                return Properties.Resources.TEXTPATTERNFORMAT;
            }

            if (iType == typeof(OutputPattern))
            {
                return Properties.Resources.OUTPUTPATTERNFORMAT;
            }

            if (iType == typeof(ConditionPattern))
            {
                return Properties.Resources.CONDITIONPATTERNFORMAT;
            }

            if (iType == typeof(DoPattern))
            {
                return Properties.Resources.DOPATTERNFORMAT;
            }

            if (iType == typeof(InvalidPatternResponse))
            {
                return Properties.Resources.INVALIDPATTERNFORMAT;
            }

            if (iType == typeof(PatternRule))
            {
                return Properties.Resources.TEXTPATTERNDEFFORMAT;
            }

            return Properties.Resources.MIXEDTEXTPATTERNFORMAT; // this should normally not happen, cause
        }
    }
}