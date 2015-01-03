// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleAnimationEngine.cs" company="">
//   
// </copyright>
// <summary>
//   provides simple, forward only animation, 1 time (no repeats possible).
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     provides simple, forward only animation, 1 time (no repeats possible).
    /// </summary>
    internal class SimpleAnimationEngine : AnimationEngineBase
    {
        /// <summary>The f cur anim pos.</summary>
        private int fCurAnimPos = 1;

        /// <summary>Inits the <paramref name="image"/> for playing the animation.</summary>
        /// <param name="image">The image.</param>
        public override void Init(AnimatedImage image)
        {
            var iFrame = image.Frames[0];
            image.Source = iFrame.Bitmap;
            image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, iFrame.Duration);
            fCurAnimPos = 1;

                // always reset to init value during init cause this can  be called multiple times in the life of engine.
        }

        /// <summary>called when the animation's timer has passed and a new frame needs to
        ///     be drawn + the new time needs to be calclated.</summary>
        /// <param name="image">The image.</param>
        public override void DoTimerTick(AnimatedImage image)
        {
            if (fCurAnimPos < image.Frames.Count)
            {
                var iFrame = image.Frames[fCurAnimPos];
                image.Source = iFrame.Bitmap;
                image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, iFrame.Duration);
                fCurAnimPos++;
            }
            else
            {
                image.HardStop();
            }
        }
    }
}