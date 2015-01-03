// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITextPatternEditorView.cs" company="">
//   
// </copyright>
// <summary>
//   Used to identify the current action that is being undertaken. So we can
//   correctly move keyboard focus when a new item is added.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Used to identify the current action that is being undertaken. So we can
    ///     correctly move keyboard focus when a new item is added.
    /// </summary>
    public enum EditMode
    {
        /// <summary>The normal.</summary>
        Normal, 

        /// <summary>The add pattern.</summary>
        AddPattern, 

        /// <summary>The add output.</summary>
        AddOutput, 

        /// <summary>The add invalid.</summary>
        AddInvalid, 

        /// <summary>The add do.</summary>
        AddDo, 

        /// <summary>The add conditional.</summary>
        AddConditional, 

        /// <summary>The add response for.</summary>
        AddResponseFor, 

        /// <summary>The add topic filter.</summary>
        AddTopicFilter
    }

    /// <summary>
    ///     an <see langword="interface" /> that should be implemnented by all viewss
    ///     that provide text pattern editing features. This allows other objects to
    ///     embed texpatterns, while providing a uniform editing.
    /// </summary>
    public interface ITextPatternEditorView
    {
        /// <summary>
        ///     Gets/sets the editing mode currently active. So we know when to move
        ///     keyboard focus or not.
        /// </summary>
        EditMode CurrentEditMode { get; set; }
    }
}