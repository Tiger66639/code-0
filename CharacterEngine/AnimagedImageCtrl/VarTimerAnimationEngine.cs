// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VarTimerAnimationEngine.cs" company="">
//   
// </copyright>
// <summary>
//   provides simple, forward only animation, repeating the animation in loop,
//   with a variable time in between each call, using
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>
    ///     provides simple, forward only animation, repeating the animation in loop,
    ///     with a variable time in between each call, using
    /// </summary>
    internal class VarTimerAnimationEngine : AnimationEngineBase
    {
        /// <summary>The f cur anim pos.</summary>
        private int fCurAnimPos = 1;

        /// <summary>The f randomizer.</summary>
        private readonly System.Random fRandomizer = new System.Random();

        /// <summary>Inits the <paramref name="image"/> for playing the animation.</summary>
        /// <param name="image">The image.</param>
        public override void Init(AnimatedImage image)
        {
            image.Source = null;
            image.Timer.Interval = new System.TimeSpan(
                0, 
                0, 
                0, 
                fRandomizer.Next(image.MinStartDelay, image.MaxStartDelay), 
                0);
            fCurAnimPos = 0;

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
                image.Source = null; // remove the image for the time being until the timer has passed
                fCurAnimPos = 0;
                image.Timer.Interval = new System.TimeSpan(
                    0, 
                    0, 
                    0, 
                    fRandomizer.Next(image.MinStartDelay, image.MaxStartDelay), 
                    0);
            }
        }
    }
}