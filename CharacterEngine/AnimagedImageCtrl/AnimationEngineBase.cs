// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimationEngineBase.cs" company="">
//   
// </copyright>
// <summary>
//   base class for the object that provides timing information for the
//   <see cref="AnimatedImage" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     base class for the object that provides timing information for the
    ///     <see cref="AnimatedImage" /> .
    /// </summary>
    internal abstract class AnimationEngineBase
    {
        /// <summary>Inits the <paramref name="image"/> for playing the animation.</summary>
        /// <param name="image">The image.</param>
        public abstract void Init(AnimatedImage image);

        /// <summary>called when the animation's timer has passed and a new frame needs to
        ///     be drawn + the new time needs to be calclated.</summary>
        /// <param name="image">The image.</param>
        public abstract void DoTimerTick(AnimatedImage image);
    }
}