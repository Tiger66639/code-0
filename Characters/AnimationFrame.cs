// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimationFrame.cs" company="">
//   
// </copyright>
// <summary>
//   respresents a single frame in an animation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     respresents a single frame in an animation.
    /// </summary>
    public class AnimationFrame
    {
        /// <summary>Gets or sets the duration.</summary>
        public int Duration { get; set; }

        /// <summary>Gets or sets the image resource.</summary>
        public string ImageResource { get; set; }
    }
}