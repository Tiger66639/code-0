// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DPITopicQuestionsSelect.cs" company="">
//   
// </copyright>
// <summary>
//   Used to indicate that the 'questions' part should be selected on a topic
//   editor. doesn't do anything, this is done by the
//   DPITextPatternEditorRoot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Search
{
    /// <summary>
    ///     Used to indicate that the 'questions' part should be selected on a topic
    ///     editor. doesn't do anything, this is done by the
    ///     DPITextPatternEditorRoot.
    /// </summary>
    internal class DPITopicQuestionsSelect : DisplayPathItem
    {
        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(ICodeItemsOwner owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get from.</summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(PatternEditorItem owner)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>The get from.</summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="object"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object GetFrom(System.Collections.IList list)
        {
            throw new System.NotImplementedException();
        }
    }
}