// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JojoAnimationEngine.cs" company="">
//   
// </copyright>
// <summary>
//   The jojo animation engine.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.WPF.Controls
{
    /// <summary>The jojo animation engine.</summary>
    internal class JojoAnimationEngine : AnimationEngineBase
    {
        /// <summary>The f cur anim pos.</summary>
        private int fCurAnimPos = 1;

        /// <summary>The f last frame.</summary>
        private int fLastFrame;

                    // stores the index of the last frame that needs to be played. This is used for the variable jojo effect, so we can change the last frame upon each run.

        /// <summary>The f playing reverse.</summary>
        private bool fPlayingReverse;

        /// <summary>
        ///     Gets or sets a value indicating whether to use a variable speed or
        ///     not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [variable speed]; otherwise, <c>false</c> .
        /// </value>
        public bool VariableSpeed { get; set; }

        /// <summary>Inits the <paramref name="image"/> for playing the animation.</summary>
        /// <param name="image">The image.</param>
        public override void Init(AnimatedImage image)
        {
            var iFrame = image.Frames[0];
            image.Source = iFrame.Bitmap;
            image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
            fLastFrame = image.Randomizer.Next(image.Frames.Count);
            fCurAnimPos = 1;

                // always reset to init value during init cause this can  be called multiple times in the life of engine.
            fPlayingReverse = false;
        }

        /// <summary>called when the animation's timer has passed and a new frame needs to
        ///     be drawn + the new time needs to be calclated.</summary>
        /// <param name="image">The image.</param>
        public override void DoTimerTick(AnimatedImage image)
        {
            if (fPlayingReverse == false)
            {
                if (fCurAnimPos <= fLastFrame)
                {
                    var iFrame = image.Frames[fCurAnimPos];
                    image.Source = iFrame.Bitmap;
                    image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
                    fCurAnimPos++;
                }
                else
                {
                    if (fCurAnimPos > 1)
                    {
                        // if fLastFrame is 1, we don't get a big fCurAnim, which would make the -2 fail. We check and make certain taht this special case is handled correctly.
                        fPlayingReverse = true;
                        fCurAnimPos -= 2;

                            // remove 2 cause the last image is still visible and we already advanced outside of the scope of the list, so go back 2 to get the prev image
                        var iFrame = image.Frames[fCurAnimPos];
                        image.Source = iFrame.Bitmap;
                        image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
                        fCurAnimPos--;
                    }
                    else
                    {
                        var iFrame = image.Frames[0];
                        image.Source = iFrame.Bitmap;
                        image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
                        fCurAnimPos = 1;
                    }
                }
            }
            else
            {
                if (fCurAnimPos >= 0)
                {
                    var iFrame = image.Frames[fCurAnimPos];
                    image.Source = iFrame.Bitmap;
                    image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
                    fCurAnimPos--;
                }
                else
                {
                    fPlayingReverse = false;
                    var iFrame = image.Frames[1];
                    image.Source = iFrame.Bitmap;
                    image.Timer.Interval = new System.TimeSpan(0, 0, 0, 0, GetDuration(iFrame.Duration, image));
                    fCurAnimPos = 2;
                    fLastFrame = image.Randomizer.Next(image.Frames.Count);
                }
            }
        }

        /// <summary>Gets the duration adjusted with a variable speed.</summary>
        /// <param name="value">The value.</param>
        /// <param name="image">The image.</param>
        /// <returns>The <see cref="int"/>.</returns>
        private int GetDuration(int value, AnimatedImage image)
        {
            if (VariableSpeed == false)
            {
                return value;
            }

            return value + image.Randomizer.Next(image.MaxLowerDeviation, image.MaxUpperDeviation);
        }
    }
}