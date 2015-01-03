// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Character.cs" company="">
//   
// </copyright>
// <summary>
//   Determins the style of the background being shown. This is to emphasize certain phonemes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.CharacterEngine
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     Determins the style of the background being shown. This is to emphasize certain phonemes.
    /// </summary>
    public enum CharacterEmphasis
    {
        /// <summary>The emphasized.</summary>
        Emphasized, 

        /// <summary>The stressed.</summary>
        Stressed, 

        /// <summary>The normal.</summary>
        Normal
    }

    /// <summary>
    ///     Represents a visual character. It contains background, lipsync and some other images that
    ///     can be displayed.
    /// </summary>
    public class Character : Characters.CharacterBase
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="Character"/> class.</summary>
        /// <param name="file">The file.</param>
        public Character(string file)
        {
            CurrentIdleLevel = 0;
            if (file != null)
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(file);
            }

            FileName = file;
        }

        #endregion

        /// <summary>
        ///     Occurs when the last idle animation has finished. Can be used to start a new animtimer
        /// </summary>
        public event System.EventHandler IdleAnimationFinished;

        /// <summary>Loads the image for usage a as a viseme. The index specifies which viseme.</summary>
        /// <remarks>we create an image to be displayed on the canvas, rendered in the bottom.</remarks>
        /// <param name="index"></param>
        /// <param name="source">The path to the image, this is absolute.</param>
        public override void LoadViseme(int index, string source)
        {
            while (fVisemes.Count <= index)
            {
                // add null items until the list has enough to do a setter. we do this cause the visemes aren't always provided in the correct order.
                fVisemes.Add(null);
            }

            var iImage = new System.Windows.Controls.Image();
            System.Windows.Media.Imaging.BitmapSource iBitmap = null;
            if (string.IsNullOrEmpty(source) == false)
            {
                if (System.IO.File.Exists(source))
                {
                    iBitmap = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(source));
                    iImage.Source = iBitmap;
                    PrepareImage(iImage, iBitmap);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        string.Format("Invalid image resource: {0}, can't find file", source));
                }
            }

            if (index == 0)
            {
                // it's the first one, which is the background image.
                if (Background == null)
                {
                    // the first image is actually the background. If there already is a background, don't load it, cause we use the prevoiusly used composite background, which offers more options.
                    Background = iImage;
                    if (iBitmap != null)
                    {
                        // if there is no image, don't need to set the size.
                        Width = iImage.Width;
                        Height = iImage.Height;
                    }

                    System.Windows.Controls.Canvas.SetTop(iImage, 0);
                    System.Windows.Controls.Canvas.SetLeft(iImage, 0);
                }
            }
            else
            {
                Position(iImage);
                fVisemes[index] = iImage;
            }
        }

        /// <summary>Loads an image as a background (multiple images can be used to build the background, so that
        ///     parts can be hidden/visualized. If there is no background defined, the first images of the
        ///     viseme list is used.</summary>
        /// <param name="source">The source.</param>
        public override void LoadBackground(string source)
        {
            System.Windows.Controls.Grid iBackground = null;
            if (Background == null)
            {
                iBackground = new System.Windows.Controls.Grid();
                Background = iBackground;
            }
            else
            {
                iBackground = Background as System.Windows.Controls.Grid;
            }

            if (iBackground != null)
            {
                var iImage = new System.Windows.Controls.Image();
                System.Windows.Media.Imaging.BitmapSource iBitmap = null;
                if (string.IsNullOrEmpty(source) == false)
                {
                    if (System.IO.File.Exists(source))
                    {
                        iBitmap = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(source));
                        iImage.Source = iBitmap;
                        PrepareImage(iImage, iBitmap);
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            string.Format("Invalid image resource: {0}, can't find file", source));
                    }
                }

                iImage.Tag = System.IO.Path.GetFileName(source);

                    // so we can easely find the background image for hiding it by animations.
                iBackground.Children.Add(iImage);
                if (iBackground.Children.Count == 1)
                {
                    Width = iImage.Width;
                    iBackground.Width = Width;
                    Height = iImage.Height;
                    iBackground.Height = Height;
                    System.Windows.Controls.Canvas.SetTop(iImage, 0);
                    System.Windows.Controls.Canvas.SetLeft(iImage, 0);
                }
            }
            else
            {
                throw new System.InvalidOperationException(
                    "Failed to find the background container, can't build character.");
            }
        }

        /// <summary>Positions the specified to image on a canvas, in the center, relative to the the background image.</summary>
        /// <param name="toPrepare">To prepare.</param>
        private void Position(System.Windows.Controls.Image toPrepare)
        {
            if (Background != null)
            {
                var iWidthDiv = ((System.Windows.FrameworkElement)Background).Width - toPrepare.Width;
                var iHeightDiv = ((System.Windows.FrameworkElement)Background).Height - toPrepare.Height;

                System.Windows.Controls.Canvas.SetTop(toPrepare, iWidthDiv / 2);
                System.Windows.Controls.Canvas.SetLeft(toPrepare, iHeightDiv / 2);
            }
            else
            {
                System.Windows.Controls.Canvas.SetTop(toPrepare, 0);
                System.Windows.Controls.Canvas.SetLeft(toPrepare, 0);
            }
        }

        /// <summary>The prepare image.</summary>
        /// <Remarks>
        ///     WPF expects everything in 96 dpi. Some images come in another dpi format, so we need to adjust the width and
        ///     height properties of the image, to make certain they are pixel aligned correctly, as they were intended (so smaller
        ///     on
        ///     bigger screens, not always the same size).
        /// </Remarks>
        /// <param name="toPrepare">The to Prepare.</param>
        /// <param name="bitmap">The bitmap.</param>
        private static void PrepareImage(
            System.Windows.Controls.Image toPrepare, 
            System.Windows.Media.Imaging.BitmapSource bitmap)
        {
            if (bitmap != null)
            {
                toPrepare.Stretch = System.Windows.Media.Stretch.Uniform;
                toPrepare.Width = bitmap.Width / (96 / bitmap.DpiX);
                toPrepare.Height = bitmap.Height / (96 / bitmap.DpiX);
            }

            toPrepare.SnapsToDevicePixels = true;
            System.Windows.Media.RenderOptions.SetEdgeMode(toPrepare, System.Windows.Media.EdgeMode.Unspecified);
            System.Windows.Media.RenderOptions.SetBitmapScalingMode(
                toPrepare, 
                System.Windows.Media.BitmapScalingMode.HighQuality);
        }

        /// <summary>Instructs to load all the data for the following animation.</summary>
        /// <param name="toLoad">To load.</param>
        public override void LoadAnimation(Characters.Animation toLoad)
        {
            var iAnim = new Designer.WPF.Controls.AnimatedImage();
            iAnim.ShowBackground = !toLoad.FirstFrameUnderlay;
            iAnim.AllowSpeech = toLoad.EnableFrameSpeaking;
            iAnim.LoopStyle = toLoad.LoopStyle;
            iAnim.UseSoftStop = toLoad.UseSoftStop;

            LoadBackgroundSuppress(toLoad, iAnim);

            if (toLoad.ZIndex.HasValue)
            {
                System.Windows.Controls.Panel.SetZIndex(iAnim, toLoad.ZIndex.Value);
            }

            // iAnim.AnimationFinished += new EventHandler(Animation_AnimationFinished);
            System.Windows.Media.Imaging.BitmapImage iBitmap = null;
            foreach (var i in toLoad.Frames)
            {
                var iFrame = new Designer.WPF.Controls.AnimatedImageFrame();
                if (string.IsNullOrEmpty(i.ImageResource) == false)
                {
                    if (System.IO.File.Exists(i.ImageResource))
                    {
                        iBitmap = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(i.ImageResource));
                        iFrame.Bitmap = iBitmap;
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            string.Format("Invalid image resource: {0}, can't find file", i.ImageResource));
                    }
                }

                iFrame.Duration = i.Duration;
                iAnim.Frames.Add(iFrame);
            }

            PrepareImage(iAnim, iBitmap); // this only works if all the dpi's of the images are the same.
            Position(iAnim); // position in the center, depending on the size of the background image.
            fAnimations.Add(toLoad.Name, iAnim);
        }

        /// <summary>Checks if the image needs to suppress any background parts and stores the indexes, so that
        ///     changes can be done fast.</summary>
        /// <param name="toLoad">To load.</param>
        /// <param name="anim">The anim.</param>
        private void LoadBackgroundSuppress(Characters.Animation toLoad, Designer.WPF.Controls.AnimatedImage anim)
        {
            if (toLoad.BackgroundSuppress != null && toLoad.BackgroundSuppress.Count > 0)
            {
                var iBackground = Background as System.Windows.Controls.Grid;
                if (iBackground != null)
                {
                    anim.BackgroundSuppress = new System.Collections.Generic.List<int>();
                    foreach (var iSuppress in toLoad.BackgroundSuppress)
                    {
                        for (var i = 0; i < iBackground.Children.Count; i++)
                        {
                            var iContent = iBackground.Children[i] as System.Windows.FrameworkElement;
                            if (iContent.Tag is string && ((string)iContent.Tag) == iSuppress)
                            {
                                anim.BackgroundSuppress.Add(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Handles the AnimationFinished event of the Animation control.
        ///     Called when an animation has finished. We remove it from the list + reactivate the background if needed.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Animation_AnimationFinished(object sender, System.EventArgs e)
        {
            var iSender = sender as Designer.WPF.Controls.AnimatedImage;

            System.Diagnostics.Debug.Assert(iSender != null);
            ResetBackground(iSender);
            VisibleItems.Remove(iSender);
            iSender.AnimationFinished -= Animation_AnimationFinished;
            if (fActiveIdleAnim == iSender)
            {
                // Animations idle animation is finished -> indicate that
                fActiveIdleAnim = null;
                OnIdleAnimationFinished();
            }
        }

        /// <summary>Resets the background to it's original state.</summary>
        /// <param name="iSender">The i sender.</param>
        private void ResetBackground(Designer.WPF.Controls.AnimatedImage iSender)
        {
            if (iSender.ShowBackground == false && Background != null)
            {
                // if  the anim first the background to hide, show it again.
                Background.Visibility = System.Windows.Visibility.Visible;
            }

            if (iSender.BackgroundSuppress != null)
            {
                var iBackground = Background as System.Windows.Controls.Grid;
                if (iBackground != null)
                {
                    foreach (var i in iSender.BackgroundSuppress)
                    {
                        iBackground.Children[i].Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        ///     Called when idle animation finished. Raises the event.
        /// </summary>
        private void OnIdleAnimationFinished()
        {
            if (IdleAnimationFinished != null)
            {
                // let any observer restart a new animation when this is done.
                IdleAnimationFinished(this, System.EventArgs.Empty);
            }
        }

        /// <summary>Instructs the char to load a single 'idle' level that can be used by one
        ///     of the previously loaded animations.
        ///     If there is no idle level present in the char, we try to load some default anims to do the job.</summary>
        /// <param name="level">The level.</param>
        public override void LoadIdleLevel(Characters.IdleLevel level)
        {
            if (IdleLevels == null)
            {
                IdleLevels = new System.Collections.Generic.List<Characters.IdleLevel>();
            }

            if (level == null)
            {
                // if leve is null, it is to indicate that there were no idle levels in the file.
                level = new Characters.IdleLevel();
                level.MinStartDelay = MIN_IDLE_DELAY;
                level.MaxStartDelay = MAX_IDLE_DELAY;
                level.MinInterval = MIN_IDLE_DELAY;
                level.MaxInterval = MAX_IDLE_DELAY;

                    // duration is not important  cause it's the  only idle level, so it will play forever.
                level.AnimationNames = new System.Collections.Generic.List<string>();
                if (fAnimations.ContainsKey("blink1"))
                {
                    level.AnimationNames.Add("blink1");
                }

                if (fAnimations.ContainsKey("lookaround"))
                {
                    level.AnimationNames.Add("lookaround");
                }
            }

            IdleLevels.Add(level);
        }

        /// <summary>Sets the index of the Zindex to be used for the viseme images.</summary>
        /// <remarks>When called, it is garanteed that all the visemes have been read in.</remarks>
        /// <param name="zIndex">the z Index .</param>
        public override void SetVisemesZIndex(int zIndex)
        {
            foreach (var i in fVisemes)
            {
                if (i != null)
                {
                    // the first is null cause that's the background.
                    System.Windows.Controls.Panel.SetZIndex(i, zIndex);
                }
            }
        }

        /// <summary>Loads an animation that will be played all the time in the background.</summary>
        /// <param name="toLoad">To load.</param>
        public override void LoadBackgroundAnimation(Characters.Animation toLoad)
        {
            var iAnim = new Designer.WPF.Controls.AnimatedImage();
            iAnim.ShowBackground = true;
            iAnim.AllowSpeech = true;
            iAnim.LoopStyle = toLoad.LoopStyle;
            iAnim.MinStartDelay = toLoad.MinStartDelay;
            iAnim.MaxStartDelay = toLoad.MaxStartDelay;
            if (toLoad.ZIndex.HasValue)
            {
                System.Windows.Controls.Panel.SetZIndex(iAnim, toLoad.ZIndex.Value);
            }

            // iAnim.AnimationFinished += new EventHandler(Animation_AnimationFinished);
            System.Windows.Media.Imaging.BitmapImage iBitmap = null;
            foreach (var i in toLoad.Frames)
            {
                var iFrame = new Designer.WPF.Controls.AnimatedImageFrame();
                if (string.IsNullOrEmpty(i.ImageResource) == false)
                {
                    if (System.IO.File.Exists(i.ImageResource))
                    {
                        iBitmap = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(i.ImageResource));
                        iFrame.Bitmap = iBitmap;
                    }
                    else
                    {
                        throw new System.InvalidOperationException(
                            string.Format("Invalid image resource: {0}, can't find file", i.ImageResource));
                    }
                }

                iFrame.Duration = i.Duration;
                iAnim.Frames.Add(iFrame);
            }

            PrepareImage(iAnim, iBitmap); // this only works if all the dpi's of the images are the same.
            Position(iAnim); // position in the center, depending on the size of the background image.
            VisibleItems.Add(iAnim);

                // background animations aren't added to the list of animations, they are simply added directly to the visible items.
        }

        #region const

        /// <summary>
        ///     Maximum delay in idle animation start, in seconds
        /// </summary>
        private const int MAX_IDLE_DELAY = 5;

        /// <summary>
        ///     Minimum delay in idel time before an animation may start, in seconds.
        /// </summary>
        private const int MIN_IDLE_DELAY = 1;

        #endregion

        #region fields

        /// <summary>The f is loaded.</summary>
        private bool fIsLoaded;

        /// <summary>The f visemes.</summary>
        private readonly System.Collections.Generic.List<System.Windows.UIElement> fVisemes =
            new System.Collections.Generic.List<System.Windows.UIElement>();

        /// <summary>The f background.</summary>
        private System.Windows.UIElement fBackground;

        /// <summary>The f selected viseme.</summary>
        private System.Windows.UIElement fSelectedViseme;

        /// <summary>The f emphasized.</summary>
        private System.Windows.Media.Visual fEmphasized;

        /// <summary>The f stressed.</summary>
        private System.Windows.Media.Visual fStressed;

        /// <summary>The f emphasis style.</summary>
        private CharacterEmphasis fEmphasisStyle;

        /// <summary>The f animations.</summary>
        private System.Collections.Generic.Dictionary<string, System.Windows.UIElement> fAnimations =
            new System.Collections.Generic.Dictionary<string, System.Windows.UIElement>();

        /// <summary>The f visible items.</summary>
        private readonly System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement> fVisibleItems =
            new System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement>();

        /// <summary>The f active idle anim.</summary>
        private System.Windows.UIElement fActiveIdleAnim; // so we can keep track of any idle anim and ask it to stop.

        /// <summary>The f width.</summary>
        private double fWidth;

        /// <summary>The f height.</summary>
        private double fHeight;

        #endregion

        #region const

        /// <summary>The backgroundfile.</summary>
        private const string BACKGROUNDFILE = "background.xaml";

        /// <summary>The emphasizedfile.</summary>
        private const string EMPHASIZEDFILE = "emphasized.xaml";

        /// <summary>The stressedfile.</summary>
        private const string STRESSEDFILE = "stressed.xaml";

        /// <summary>The ccsfileext.</summary>
        public const string CCSFILEEXT = ".ccs";

        #endregion

        #region prop

        #region IsLoaded

        /// <summary>
        ///     Gets/sets the wether the character is loaded or not.
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return fIsLoaded;
            }

            set
            {
                if (value != fIsLoaded)
                {
                    fIsLoaded = value;
                    if (value)
                    {
                        LoadFileContent();
                    }
                    else
                    {
                        ClearContent();
                    }

                    OnPropertyChanged("IsLoaded");
                }
            }
        }

        #endregion

        #region IdleLevels

        /// <summary>
        ///     Gets the idle levels.
        /// </summary>
        public System.Collections.Generic.List<Characters.IdleLevel> IdleLevels { get; private set; }

        #endregion

        #region CurrentIdleLevel

        /// <summary>
        ///     Gets/sets the current idle level.
        /// </summary>
        public int CurrentIdleLevel { get; set; }

        #endregion

        #region Background

        /// <summary>
        ///     Gets/sets the background image that should be displayed for this character.
        /// </summary>
        public System.Windows.UIElement Background
        {
            get
            {
                return fBackground;
            }

            set
            {
                if (fBackground != value)
                {
                    if (fBackground != null)
                    {
                        fVisibleItems.Remove(fBackground);
                    }

                    fBackground = value;
                    if (fBackground != null)
                    {
                        fVisibleItems.Insert(0, fBackground);
                    }

                    OnPropertyChanged("Background");
                }
            }
        }

        #endregion

        #region EmphasisStyle

        /// <summary>
        ///     Gets/sets the style of the background currently being used.
        /// </summary>
        public CharacterEmphasis EmphasisStyle
        {
            get
            {
                return fEmphasisStyle;
            }

            set
            {
                if (value != fEmphasisStyle)
                {
                    fEmphasisStyle = value;
                    OnPropertyChanged("EmphasisStyle");
                    OnPropertyChanged("Emphasis");
                }
            }
        }

        #endregion

        #region Emphasis

        /// <summary>
        ///     Gets/sets the emphasis that should be depicted.
        /// </summary>
        public System.Windows.Media.Visual Emphasis
        {
            get
            {
                switch (EmphasisStyle)
                {
                    case CharacterEmphasis.Emphasized:
                        return fEmphasized;
                    case CharacterEmphasis.Stressed:
                        return fStressed;
                    case CharacterEmphasis.Normal:
                        return null;
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region SelectedViseme

        /// <summary>
        ///     Gets/sets the currently selected phoneme.
        /// </summary>
        public System.Windows.UIElement SelectedViseme
        {
            get
            {
                return fSelectedViseme;
            }

            set
            {
                if (fSelectedViseme != value)
                {
                    if (fSelectedViseme != null)
                    {
                        fVisibleItems.Remove(fSelectedViseme);
                    }

                    fSelectedViseme = value;
                    if (fSelectedViseme != null)
                    {
                        fVisibleItems.Add(fSelectedViseme);
                    }

                    OnPropertyChanged("SelectedViseme");
                }
            }
        }

        #endregion

        #region Animations

        /// <summary>
        ///     Gets the dictionary of animations that are defined for this character.
        /// </summary>
        public System.Collections.Generic.Dictionary<string, System.Windows.UIElement> Animations
        {
            get
            {
                return fAnimations;
            }

            internal set
            {
                fAnimations = value;
            }
        }

        #endregion

        #region VisibleItems

        /// <summary>
        ///     Gets the list of visemes and animations that are currently visible/active.
        /// </summary>
        public System.Collections.ObjectModel.ObservableCollection<System.Windows.UIElement> VisibleItems
        {
            get
            {
                return fVisibleItems;
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width of the character.
        /// </summary>
        public double Width
        {
            get
            {
                return fWidth;
            }

            set
            {
                fWidth = value;
                OnPropertyChanged("Width");
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the height of the char.
        /// </summary>
        public double Height
        {
            get
            {
                return fHeight;
            }

            set
            {
                fHeight = value;
                OnPropertyChanged("Height");
            }
        }

        #endregion

        /// <summary>
        ///     Gets the name of the file that defines the character.
        /// </summary>
        /// <value>
        ///     The name of the file.
        /// </value>
        public string FileName { get; private set; }

        #endregion

        #region Functions

        /// <summary>Activates and displays the specified animation.</summary>
        /// <param name="name">The name.</param>
        public void ActivateAnimation(string name)
        {
            System.Windows.UIElement iItem;
            if (Animations.TryGetValue(name, out iItem))
            {
                if (VisibleItems.Contains(iItem) == false)
                {
                    // don't try to start the animation 2 times.
                    VisibleItems.Add(iItem);
                    var iAnim = iItem as Designer.WPF.Controls.AnimatedImage;
                    iAnim.AnimationFinished += Animation_AnimationFinished;
                    PrepareAnimatedImage(iAnim);
                }
            }
            else
            {
                LogService.Log.LogError(
                    "Character animation", 
                    string.Format("Request for an unknown animatiom: {0}", name));
            }
        }

        /// <summary>Activates an animation idle animation.</summary>
        /// <param name="randomizer">The randomizer.</param>
        public void ActivateIdleAnimation(System.Random randomizer)
        {
            System.Windows.UIElement iFound = null;
            int iVal;
            if (IdleLevels != null && IdleLevels.Count > 0)
            {
                iVal = randomizer.Next(IdleLevels[CurrentIdleLevel].AnimationNames.Count);
                var iName = IdleLevels[CurrentIdleLevel].AnimationNames[iVal].ToLower();
                iFound = null;
                if (Animations.TryGetValue(iName, out iFound) == false)
                {
                    LogService.Log.LogError(
                        "Character.ActivateIdelAnimation", 
                        string.Format(
                            "The idle level {0} contains a reference to an unknown animation {1}", 
                            CurrentIdleLevel, 
                            iName));
                }

                var iTryCount = 0;
                while (VisibleItems.Contains(iFound) && iTryCount < Animations.Count)
                {
                    // we need to build in a stop incase someimthing is wrong: only try to load only as many times as there are anims.
                    iVal = randomizer.Next(IdleLevels[CurrentIdleLevel].AnimationNames.Count);
                    iName = IdleLevels[CurrentIdleLevel].AnimationNames[iVal].ToLower();
                    if (Animations.TryGetValue(iName, out iFound) == false)
                    {
                        LogService.Log.LogError(
                            "Character.ActivateIdelAnimation", 
                            string.Format(
                                "The idle level {0} contains a reference to an unknown animation {1}", 
                                CurrentIdleLevel, 
                                iName));
                    }

                    iTryCount++;
                }
            }

            if (iFound != null && VisibleItems.Contains(iFound) == false)
            {
                // don't try to start the animation 2 times + make certain there is something to start.
                var iAnim = iFound as Designer.WPF.Controls.AnimatedImage;
                fActiveIdleAnim = iFound;
                VisibleItems.Add(iFound);

                iAnim.AnimationFinished += Animation_AnimationFinished;
                PrepareAnimatedImage(iAnim);
            }
        }

        /// <summary>Hides the background if the animation requests it.</summary>
        /// <param name="anim">The anim.</param>
        private void PrepareAnimatedImage(Designer.WPF.Controls.AnimatedImage anim)
        {
            if (anim != null)
            {
                if (anim.ShowBackground == false && Background != null)
                {
                    Background.Visibility = System.Windows.Visibility.Hidden;
                }

                if (anim.BackgroundSuppress != null)
                {
                    var iBackground = Background as System.Windows.Controls.Grid;
                    if (iBackground != null)
                    {
                        foreach (var i in anim.BackgroundSuppress)
                        {
                            iBackground.Children[i].Visibility = System.Windows.Visibility.Hidden;
                        }
                    }
                }

                anim.Visibility = System.Windows.Visibility.Visible;
                anim.StartPlay();
            }
        }

        /// <summary>
        ///     Stops the animation that was started to display an idle state.
        ///     Also stops all the animations that can't be run while speech is happening.
        /// </summary>
        public void StopAnimationsForSpeech()
        {
            if (fActiveIdleAnim != null)
            {
                var iAnim = fActiveIdleAnim as Designer.WPF.Controls.AnimatedImage;
                if (iAnim != null)
                {
                    if (iAnim.IsLoaded)
                    {
                        // if the anim is not loaded, don't wait for it to finish, cause it's not playing anyway.
                        iAnim.Stop();
                    }
                    else
                    {
                        iAnim.HardStop();
                    }
                }
                else
                {
                    VisibleItems.Remove(fActiveIdleAnim);
                    fActiveIdleAnim = null;
                }
            }
            else
            {
                OnIdleAnimationFinished();

                    // if there was no active idle animation, we still raise the event, cause this is used by the channel to actually start the speech.
            }

            foreach (var i in Enumerable.ToArray(VisibleItems))
            {
                // we might modify this list, so remove ourselves from it.
                var iAnim = i as Designer.WPF.Controls.AnimatedImage;
                if (iAnim != null && iAnim.AllowSpeech == false)
                {
                    iAnim.Stop();
                }
            }
        }

        /// <summary>Selects the viseme for display.</summary>
        /// <param name="value">The value.</param>
        public void SelectViseme(int value)
        {
            if (value > -1 && value < fVisemes.Count)
            {
                // Debug.Print("print {0}: {1}", value, DateTime.Now.TimeOfDay);
                SelectedViseme = fVisemes[value];
            }
            else
            {
                SelectedViseme = null;
            }
        }

        /// <summary>
        ///     Clears the content so that it doesn't use resources.
        /// </summary>
        private void ClearContent()
        {
            foreach (var i in Animations)
            {
                var iAnim = i.Value as Designer.WPF.Controls.AnimatedImage;
                if (iAnim != null)
                {
                    iAnim.AnimationFinished -= Animation_AnimationFinished;

                        // need to unregister all event handlers, otherwise we have a mem leak.
                    iAnim.HardStop(); // make certain that the timer is stopped and unregistered.
                }
            }

            Animations.Clear();
            foreach (var i in Enumerable.ToArray(fVisibleItems))
            {
                // need to make local copy, cause stopping the anim will remove items from teh visibleItems list.
                var iAnim = i as Designer.WPF.Controls.AnimatedImage;
                if (iAnim != null)
                {
                    iAnim.HardStop();
                }
            }

            fVisibleItems.Clear();
            foreach (var i in fVisemes)
            {
                var iAnim = i as Designer.WPF.Controls.AnimatedImage;
                if (iAnim != null)
                {
                    iAnim.HardStop();
                }
            }

            fVisemes.Clear();
            IdleLevels.Clear();
            SelectedViseme = null;
            Background = null;
            fStressed = null;
            fEmphasized = null;
            CurrentIdleLevel = 0;
        }

        /// <summary>
        ///     Loads the content of the file. Warning: before the zip is extracted, the dir is cleared.
        /// </summary>
        private void LoadFileContent()
        {
            if (System.IO.Path.GetExtension(FileName).ToLower() == CCSFILEEXT)
            {
                Characters.CCSFile.ImportCCSFile(this, FileName);
            }
            else
            {
                throw new System.NotImplementedException();

                // if (Directory.Exists(Properties.Settings.Default.CharacterExtractionPath) == true)                 //this is to clear out the directory.
                // Directory.Delete(Properties.Settings.Default.CharacterExtractionPath);
                // Directory.CreateDirectory(Properties.Settings.Default.CharacterExtractionPath);
                // using (ZipFile zip = ZipFile.Read(fFileName))
                // {
                // foreach (ZipEntry e in zip)
                // {
                // e.Extract(Properties.Settings.Default.CharacterExtractionPath);
                // LoadFile(e.FileName);
                // }
                // }
            }
        }

        /// <summary>Loads the file.
        ///     WARNING: still in implementation fase. This is not working yet.</summary>
        /// <param name="fileName">Name of the file (without path).</param>
        private void LoadFile(string fileName)
        {
            // string iPath = Path.Combine(CharacterEngine.Properties.Settings.Default.CharacterExtractionPath, fileName);
            // using (XmlTextReader xmlReader = new XmlTextReader(iPath))
            // {
            // if (fileName == BACKGROUNDFILE)
            // Background = System.Windows.Markup.XamlReader.Load(xmlReader) as UIElement;
            // else if(fileName == STRESSEDFILE)
            // fStressed = System.Windows.Markup.XamlReader.Load(xmlReader) as UIElement;
            // else if(fileName == EMPHASIZEDFILE)
            // fEmphasized = System.Windows.Markup.XamlReader.Load(xmlReader) as UIElement;
            // else
            // {
            // UIElement iNew = System.Windows.Markup.XamlReader.Load(xmlReader) as UIElement;
            // fVisemes.Add(iNew);
            // }
            // }
        }

        #endregion
    }
}