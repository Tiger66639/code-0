// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageChannel.cs" company="">
//   
// </copyright>
// <summary>
//   A wrapper class for the <see cref="ImageSin" /> .
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     A wrapper class for the <see cref="ImageSin" /> .
    /// </summary>
    public class ImageChannel : CommChannel
    {
        #region ctor

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImageChannel" /> class.
        /// </summary>
        public ImageChannel()
        {
            GridImage = new GridImage(this);
        }

        #endregion

        #region IsProcessing

        /// <summary>
        ///     Gets the if the sin is currently still processing input data or if it
        ///     is ready to receive new data.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the sin is processing data; otherwise, <c>false</c> ,
        ///     meaning it is ready to receive new data.
        /// </value>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsProcessing
        {
            get
            {
                return fIsProcessing;
            }

            private set
            {
                fIsProcessing = value;
                OnPropertyChanged("IsProcessing");
            }
        }

        #endregion

        #region AsVideo

        /// <summary>
        ///     Gets/sets the wether input should be processed continuasly like a
        ///     video (or the eye),
        /// </summary>
        public bool AsVideo
        {
            get
            {
                return fAsVideo;
            }

            set
            {
                if (fAsVideo != value)
                {
                    fAsVideo = value;
                    if (value == false)
                    {
                        // need to reset  asap, to prevent memory leaks.
                        VideoSource = null;
                    }

                    OnPropertyChanged("AsVideo");
                }
            }
        }

        #endregion

        #region VideoSource

        /// <summary>
        ///     Gets/sets the visual that serves as image source for video imput. As
        ///     soon as video input is turned of, this is reset. The visual is only
        ///     stored if video is requested.
        /// </summary>
        public System.Windows.Media.Visual VideoSource { get; set; }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width in pixels of the image that is supplied to the
        ///     brain.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int Width
        {
            get
            {
                return ((ImageSin)Sin).Width;
            }

            set
            {
                var iSin = (ImageSin)Sin;
                var iPrev = iSin.Width;
                if (iPrev != value && value > 0)
                {
                    // need at least 1 pixel.
                    OnPropertyChanging("Width", iPrev, value);
                    iSin.Width = value;
                    GridImage.UpdateWidth(iPrev, value);
                    OnPropertyChanged("Width");
                }
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the height, expressed in pixels of the image supplied to the
        ///     brain
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public int Height
        {
            get
            {
                return ((ImageSin)Sin).Height;
            }

            set
            {
                var iSin = (ImageSin)Sin;
                var iPrev = iSin.Height;
                if (value != iPrev && value > 0)
                {
                    // need at least 1 pixel
                    OnPropertyChanging("Height", iPrev, value);
                    iSin.Height = value;
                    GridImage.UpdateHeight(iPrev, value);
                    OnPropertyChanged("Height");
                }
            }
        }

        #endregion

        #region Image

        /// <summary>
        ///     Gets the image object used to transform the input visual (usually an
        ///     inkcanvas), into a bitmap.
        /// </summary>
        /// <remarks>
        ///     We provide access to visualize it. Everytime an image gets send, this
        ///     shows what has exactly been sent.
        /// </remarks>
        [System.Xml.Serialization.XmlIgnore]
        public System.Windows.Media.Imaging.BitmapSource Image
        {
            get
            {
                return fImage;
            }

            internal set
            {
                fImage = value;
                OnPropertyChanged("Image");
            }
        }

        #endregion

        #region GridImage

        /// <summary>
        ///     Gets the image that can be edited as a grid.
        /// </summary>
        public GridImage GridImage { get; private set; }

        #endregion

        #region Output

        /// <summary>
        ///     Gets the output image of the image sin.
        /// </summary>
        public System.Windows.Media.Imaging.RenderTargetBitmap Output
        {
            get
            {
                return fOutput;
            }

            internal set
            {
                fOutput = value;
                OnPropertyChanged("Output");
            }
        }

        #endregion

        #region SelectedInputView

        /// <summary>
        ///     Gets/sets the index of the selected input view: a grid, a stylus,....
        ///     This is used so we can recover to the correct pos.
        /// </summary>
        public int SelectedInputView
        {
            get
            {
                return fSelectedInputView;
            }

            set
            {
                fSelectedInputView = value;
                OnPropertyChanged("SelectedInputView");
            }
        }

        #endregion

        #region fields

        /// <summary>The f is processing.</summary>
        private bool fIsProcessing;

        /// <summary>The f as video.</summary>
        private bool fAsVideo;

        /// <summary>The f image.</summary>
        private System.Windows.Media.Imaging.BitmapSource fImage;

        /// <summary>The f output.</summary>
        private System.Windows.Media.Imaging.RenderTargetBitmap fOutput;

        /// <summary>The f selected input view.</summary>
        private int fSelectedInputView;

        #endregion

        #region functions

        /// <summary>Sets the Sensory <see langword="interface"/> that this object is a
        ///     wrapper of.</summary>
        /// <param name="sin">The sin.</param>
        protected internal override void SetSin(Sin sin)
        {
            var iPrev = Sin as ImageSin; // remove prev handler
            if (iPrev != null)
            {
                iPrev.IsReady -= sin_IsReady;
            }

            var iSin = (ImageSin)sin;
            base.SetSin(sin);
            if (iSin != null)
            {
                iSin.IsReady += sin_IsReady;
                if (BrainData.Current.DesignerData != null)
                {
                    // if the designer dat is null, we are laoding a new project (<> creating new channel). When loading project, this gets called from a non UI thread. the init can't handle that. This is solved with overriding AfterLoaded
                    GridImage.Init();
                }
            }
            else
            {
                Image = null;
                Output = null;
            }
        }

        /// <summary>
        ///     Called from the UI thread just after the project has been loaded. This
        ///     allows communication channels to perform load tasks that can only be
        ///     done from the UI.
        /// </summary>
        internal override void AfterLoaded()
        {
            base.AfterLoaded();
            GridImage.Init();
        }

        /// <summary>
        ///     Creates the render target image.
        /// </summary>
        /// <remarks>
        ///     Both Widht and <see cref="Height" /> must be bigger than 0, otherwise
        ///     ther is no bitmap created.
        /// </remarks>
        private void CreateImage()
        {
            var iSin = (ImageSin)Sin;
            if (iSin.Width > 0 && iSin.Height > 0)
            {
                // Output = new RenderTargetBitmap(iSin.Width, iSin.Height, 96.0, 96.0, PixelFormats.Default);
            }
            else
            {
                Image = null;
                Output = null;
                GridImage.ReleaseData();
            }
        }

        /// <summary>Handles the IsReady event of the sin control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void sin_IsReady(object sender, System.EventArgs e)
        {
            IsProcessing = false;
            if (AsVideo)
            {
                SendImageInternal(VideoSource);
            }
        }

        /// <summary>The send image internal.</summary>
        /// <param name="renderSource">The render source.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void SendImageInternal(System.Windows.Media.Visual renderSource)
        {
            if (IsProcessing == false)
            {
                if (AsVideo)
                {
                    // only track the processing state for video imput, in which case there is a sort of 'pull' going on. Otherwise, the user cand do whatever.
                    IsProcessing = true;
                }

                var iSin = (ImageSin)Sin;
                var iImage = new System.Windows.Media.Imaging.RenderTargetBitmap(
                    iSin.Width, 
                    iSin.Height, 
                    96.0, 
                    96.0, 
                    System.Windows.Media.PixelFormats.Default);
                iImage.Render(renderSource);
                Image = iImage;
                iSin.Process(Image, ProcessorFactory.GetProcessor());
            }
            else
            {
                throw new System.InvalidOperationException("Previous image hasn't been processed yet.");
            }
        }

        /// <summary>Sends the <see cref="ImageChannel.RenderSource"/> to the brain.</summary>
        /// <param name="renderSource">The render Source.</param>
        public void SendImage(System.Windows.Media.Visual renderSource)
        {
            if (AsVideo)
            {
                // if we start a video imput, we need to store the input source.
                VideoSource = renderSource;
            }

            SendImageInternal(renderSource);
        }

        /// <summary>Sends the Bitmap directly to the sin for processing.</summary>
        /// <param name="renderSource">The render source.</param>
        public void SendImage(System.Windows.Media.Imaging.BitmapSource renderSource)
        {
            if (IsProcessing == false)
            {
                if (AsVideo)
                {
                    // only track the processing state for video imput, in which case there is a sort of 'pull' going on. Otherwise, the user cand do whatever.
                    IsProcessing = true;
                }

                Image = renderSource;
                var iSin = (ImageSin)Sin;
                iSin.Process(renderSource, ProcessorFactory.GetProcessor());
            }
            else
            {
                throw new System.InvalidOperationException("Previous image hasn't been processed yet.");
            }
        }

        /// <summary>Sends the resource to the sensory interface, in case it is supported.</summary>
        /// <param name="fileName">Name of the file.</param>
        internal override void SendResource(string fileName)
        {
            if (IsProcessing == false)
            {
                if (AsVideo)
                {
                    // only track the processing state for video imput, in which case there is a sort of 'pull' going on. Otherwise, the user cand do whatever.
                    IsProcessing = true;
                }

                Image = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(fileName));
                var iSin = (ImageSin)Sin;
                iSin.Process(fileName, ProcessorFactory.GetProcessor());
            }
            else
            {
                throw new System.InvalidOperationException("Previous image hasn't been processed yet.");
            }
        }

        /// <summary>
        ///     Sends the grid that is currently stored for editing.
        /// </summary>
        internal void SendGrid()
        {
            if (IsProcessing == false)
            {
                if (AsVideo)
                {
                    // only track the processing state for video imput, in which case there is a sort of 'pull' going on. Otherwise, the user cand do whatever.
                    IsProcessing = true;
                }

                var iSin = (ImageSin)Sin;

                var iGridImage = new System.Windows.Media.Imaging.WriteableBitmap(
                    Width, 
                    Height, 
                    96.0, 
                    96.0, 
                    System.Windows.Media.PixelFormats.Bgr24, 
                    null);
                iGridImage.WritePixels(
                    new System.Windows.Int32Rect(0, 0, Width, Height), 
                    GridImage.Pixels, 
                    Width * System.Windows.Media.PixelFormats.Bgr24.BitsPerPixel / 8, 
                    0);
                Image = iGridImage;
                iSin.Process(GridImage.Pixels, ProcessorFactory.GetProcessor());
            }
            else
            {
                throw new System.InvalidOperationException("Previous image hasn't been processed yet.");
            }
        }

        #endregion

        #region xml

        /// <summary>Converts an object into its XML representation.</summary>
        /// <remarks>When streaming to a module (for export), we do a mapping, to the index
        ///     of the neuron in the module that is currently being exported, and off
        ///     course visa versa, when reading from a module.</remarks>
        /// <param name="writer">The <see cref="System.Xml.XmlWriter"/> stream to which the object is
        ///     serialized.</param>
        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            base.WriteXml(writer);
            XmlStore.WriteElement(writer, "AsVideo", AsVideo);
        }

        /// <summary>Generates an object from its XML representation.</summary>
        /// <remarks>Descendents need to perform mapping between module index and neurons
        ///     when importing from modules.</remarks>
        /// <param name="reader">The <see cref="System.Xml.XmlReader"/> stream from which the object
        ///     is deserialized.</param>
        protected override void ReadXmlContent(System.Xml.XmlReader reader)
        {
            base.ReadXmlContent(reader);
            AsVideo = XmlStore.ReadElement<bool>(reader, "AsVideo");
        }

        #endregion
    }
}