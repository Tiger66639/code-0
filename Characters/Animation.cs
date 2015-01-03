// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Animation.cs" company="">
//   
// </copyright>
// <summary>
//   Determins the loop style used by animations.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Characters
{
    /// <summary>
    ///     Determins the loop style used by animations.
    /// </summary>
    public enum AnimationLoopStyle
    {
        /// <summary>The none.</summary>
        None, 

        /// <summary>
        ///     the nr of frames played is variable, so lower range is always 0, but
        ///     upper range varies. When end is reached, reverse.
        /// </summary>
        Jojo, 

        /// <summary>
        ///     timing is variable as well as length (nr of frames).
        /// </summary>
        VariableJojo, 

        /// <summary>
        ///     when end is reached, begin back at start.
        /// </summary>
        FrontToBack, 

        /// <summary>
        ///     looping is provided by a timer, like the idle levels
        /// </summary>
        VarTimer
    }

    /// <summary>
    ///     contains all the info required to build an animation. This class is
    ///     provided so we can read it from a ccs file.
    /// </summary>
    public class Animation
    {
        /// <summary>
        ///     The individual frames in the animation.
        /// </summary>
        public System.Collections.Generic.List<AnimationFrame> Frames =
            new System.Collections.Generic.List<AnimationFrame>();

        /// <summary>
        ///     Gets or sets a value indicating whether it is allowed to speak when
        ///     this animation is playing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [enable frame speaking]; otherwise, <c>false</c> .
        /// </value>
        public bool EnableFrameSpeaking { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to hold the last frame while
        ///     speaking.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [hold last frame for speak]; otherwise, <c>false</c> .
        /// </value>
        public bool HoldLastFrameForSpeak { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the first frame should be used
        ///     as an underlay (always visible).
        /// </summary>
        /// <value>
        ///     <c>true</c> if [first frame underlay]; otherwise, <c>false</c> .
        /// </value>
        public bool FirstFrameUnderlay { get; set; }

        /// <summary>
        ///     Gets or sets the name of the anim.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        ///     When the animation needs to be stopeed cause speech is about to begin,
        ///     use this animation.
        /// </summary>
        /// <value>
        ///     The name of the return animation.
        /// </value>
        public string ReturnAnimationName { get; set; }

        /// <summary>Gets or sets the z index.</summary>
        public int? ZIndex { get; set; }

        /// <summary>
        ///     When set, determins the loopstyle that should be used.
        /// </summary>
        /// <value>
        ///     variable jojo: start at image 0, go up till variable amount, go back
        ///     down again to null, repeat.
        /// </value>
        public AnimationLoopStyle LoopStyle { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the anim should be played with
        ///     a variable speed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [variable speed]; otherwise, <c>false</c> .
        /// </value>
        public bool VariableSpeed { get; set; }

        /// <summary>
        ///     Gets or sets the min start delay that should be used in case the
        ///     loopstyle is
        ///     <see cref="JaStDev.HAB.Characters.AnimationLoopStyle.VarTimer" />
        /// </summary>
        /// <value>
        ///     The min delay to use for starting the animation again.
        /// </value>
        public int MinStartDelay { get; set; }

        /// <summary>
        ///     Gets or sets the max start delay that should be used in case the
        ///     loopstyle is
        ///     <see cref="JaStDev.HAB.Characters.AnimationLoopStyle.VarTimer" />
        /// </summary>
        /// <value>
        ///     The max delay to use for starting the animation again.
        /// </value>
        public int MaxStartDelay { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether to stop the animation by going
        ///     to the end (or start, if it is looping) as fast as possible or by
        ///     simply hardstopping it.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use soft stop]; otherwise, <c>false</c> .
        /// </value>
        public bool UseSoftStop { get; set; }

        /// <summary>
        ///     Gets or sets the filter that needs to be applied to the background,
        ///     for removing parts. All transparent sections of the background filter
        ///     will let the background visible, any parts that aren't transparent
        ///     will result in new transparent parts in the background. This way, we
        ///     can remove sections of the background.
        /// </summary>
        /// <value>
        ///     The background filter.
        /// </value>
        public System.Collections.Generic.List<string> BackgroundSuppress { get; set; }
    }
}