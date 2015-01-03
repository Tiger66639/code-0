// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageSin.cs" company="">
//   
// </copyright>
// <summary>
//   Sensory interface able to convert WPF BitmaSource objects into  neurons and vice versa.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Sensory interface able to convert WPF BitmaSource objects into  neurons and vice versa.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An image sin converts an image into neuron impulses by dividing each pixel into 4 colors:
    ///         red, green, blue, gray value.  Each color becomes a neuron value, 4 colors in a neuron make
    ///         a pixel.  These pixel neurons form the entrypoints that are saved in a cluster grouped per
    ///         row.  So there is the image neuron (a cluster) containing lines (also cluster), each line contains
    ///         pixels as children (neurons), each pixel has 4 references to other int neurons which have the actual value.
    ///     </para>
    ///     <para>
    ///         A neuron is created per pixel.  This neuron contains 4 links to int neurons which contain the 3
    ///         base colors + the gray scaled value of the 3 colors (just like the eye), so no transparancy, which
    ///         is not wanted for a system that needs to mimic the eye.
    ///     </para>
    ///     <para>
    ///         The neurons that represent the initial value of the image are not recreated each time a new image is
    ///         provided. Instead, they are reused for each image. This is done because we would otherwise have to
    ///         create far to many neurons + we would not be able to program each imput item seperatly + this technique
    ///         is similar to our eyes (they don't create new neurons for each image, values are simply adjusted.
    ///         This does have some implications:
    ///         - an image can only be processed if the previous image has been handled (loaded as neurons).
    ///         - The size of the image that can be processed, is predifened. Whenever this changes, the entry points will
    ///         change and
    ///         so will the brain, cause old entry points might get deleted, new ones added.
    ///         -For video, use the <see cref="ImageSin.IsReady" /> event. This triggered by sending the image sin into it's
    ///         own output
    ///     </para>
    ///     <para>
    ///         Because the entrypoint neurons are reused each time.  a special EntryPointsCreated function is called (if it
    ///         assigned to
    ///         this sin) after the entrypoints have been recreated.
    ///     </para>
    /// </remarks>
    [NeuronID((ulong)PredefinedNeurons.Red, typeof(Neuron))]

    // create the default neuron we use as initial link types for pixels we receive as input.
    [NeuronID((ulong)PredefinedNeurons.Green, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Blue, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.Gray, typeof(Neuron))]
    [NeuronID((ulong)PredefinedNeurons.ImageSin, typeof(Neuron))]
    public class ImageSin : Sin
    {
        #region Fields

        /// <summary>Initializes a new instance of the <see cref="ImageSin"/> class.</summary>
        public ImageSin()
        {
            IsProcessing = false;
        }

        #endregion

        #region IsProcessing

        /// <summary>
        ///     Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is processing; otherwise, <c>false</c>.
        /// </value>
        public bool IsProcessing { get; private set; }

        #endregion

        #region TypeOfNeuron

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value><see cref="PredefinedNeurons.TextNeuron" />.</value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.ImageSin;
            }
        }

        #endregion

        #region Width

        /// <summary>
        ///     Gets/sets the width of the images to process.
        /// </summary>
        /// <remarks>
        ///     When an image is processed that has a different with value as that of this imagesin, the image is resized.
        ///     when there is no with relationship yet defined on this neuron, it is created
        /// </remarks>
        public int Width
        {
            get
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Width) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(0);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Width, iFound);
                }

                return iFound.Value;
            }

            set
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Width) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(value);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Width, iFound);
                }
                else
                {
                    iFound.Value = value;
                }

                ReBuildStartPoints(Height, value);
            }
        }

        #endregion

        #region Height

        /// <summary>
        ///     Gets/sets the Height of the images to process.
        /// </summary>
        /// <remarks>
        ///     When an image is processed that has a different Height value as that of this imagesin, the image is resized.
        ///     when there is no Height relationship yet defined on this neuron, it is created
        /// </remarks>
        public int Height
        {
            get
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Height) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(0);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Height, iFound);
                }

                return iFound.Value;
            }

            set
            {
                var iFound = FindFirstOut((ulong)PredefinedNeurons.Height) as IntNeuron;
                if (iFound == null)
                {
                    iFound = NeuronFactory.GetInt(value);
                    Brain.Current.Add(iFound);
                    SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Height, iFound);
                }
                else
                {
                    iFound.Value = value;
                }

                ReBuildStartPoints(value, Width);
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs when the sin is ready proccesing a bitmapsource.
        /// </summary>
        /// <remarks>
        ///     Monitor this event to check when a new bitmapsource can be send to this sin.
        /// </remarks>
        public event System.EventHandler IsReady;

        #endregion

        /// <summary>Sets the colors of a single pixel neuron.</summary>
        /// <param name="pixel">The pixel.</param>
        /// <param name="blue">The blue value.</param>
        /// <param name="green">The green value.</param>
        /// <param name="red">The red value.</param>
        private void SetPixel(Neuron pixel, byte blue, byte green, byte red)
        {
            var iLinksOut = Factories.Default.LinkLists.GetBuffer();
            try
            {
                using (var iList = pixel.LinksOut) iLinksOut.AddRange(iList); // make local copy, so we don't have deadlocks.
                ((IntNeuron)iLinksOut[0].To).Value = blue;
                ((IntNeuron)iLinksOut[1].To).Value = green;
                ((IntNeuron)iLinksOut[2].To).Value = red;
                ((IntNeuron)iLinksOut[3].To).Value = blue + green + red; // final link is gray.
                iLinksOut[3].InfoW.Clear();

                    // the info of the gray value is used to indicate if the neuron was processed  or not, always reset this.
            }
            finally
            {
                Factories.Default.LinkLists.Recycle(iLinksOut);
            }
        }

        /// <summary>Builds the neurons that function as the starting points for images.</summary>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <remarks>Rows are added/removed as needed.</remarks>
        private void ReBuildStartPoints(int height, int width)
        {
            var iEntry = FindFirstOut((ulong)PredefinedNeurons.EntryPoints) as NeuronCluster;
            if (iEntry == null)
            {
                iEntry = NeuronFactory.GetCluster();
                Brain.Current.Add(iEntry);
                SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.EntryPoints, iEntry);
            }

            UpdateHeight(height, width, iEntry);
            UpdateWidth(width, iEntry);
            var iToCall = FindFirstOut((ulong)PredefinedNeurons.EntryPointsCreated) as NeuronCluster;
            if (iToCall != null)
            {
                var iProc = ProcessorFactory.GetProcessor();
                iProc.CallSingle(iToCall);
            }
        }

        /// <summary>Updates the widths of all the lines in the entrypoints cluster.</summary>
        /// <param name="width">The width.</param>
        /// <param name="lines">The lines.</param>
        private void UpdateWidth(int width, NeuronCluster lines)
        {
            System.Collections.Generic.List<NeuronCluster> iList;
            using (var iLines = lines.Children) iList = iLines.ConvertTo<NeuronCluster>();
            foreach (var iCluster in iList)
            {
                // after the lines are updated, we can update all the lines who haven't yet have the proper width.  The first time we meet one that does, we can quit cause it means that the widh hasnt' changed, or we have reached the new lines.
                System.Diagnostics.Debug.Assert(iCluster != null);
                var iClusterList = iCluster.ChildrenW;
                try
                {
                    var iCount = iClusterList.Count;
                    if (iCount == width)
                    {
                        break;
                    }
                    else if (iCount > width)
                    {
                        while (iClusterList.Count > width)
                        {
                            RemoveLastPixel(iClusterList);
                        }
                    }
                    else
                    {
                        while (iClusterList.Count < width)
                        {
                            AddPixel(iClusterList);
                        }
                    }
                }
                finally
                {
                    iClusterList.Dispose();
                }
            }
        }

        /// <summary>Updates the nr of lines in the entry point cluster..</summary>
        /// <param name="height">The height: nr of lines.</param>
        /// <param name="width">The width: nr of pixels per line</param>
        /// <param name="lines">the list of lines</param>
        private void UpdateHeight(int height, int width, NeuronCluster lines)
        {
            int iCount;
            using (var iLines = lines.Children) iCount = iLines.Count;
            if (iCount > height)
            {
                // first handle the lines.
                while (iCount > height)
                {
                    NeuronCluster iLast;
                    using (var iLines = lines.Children) iLast = Brain.Current[iLines[iCount - 1]] as NeuronCluster; // this is the last line
                    System.Diagnostics.Debug.Assert(iLast != null);
                    while (iLast.Children.Count > 0)
                    {
                        using (var iLastChildren = iLast.ChildrenW) RemoveLastPixel(iLastChildren);
                    }

                    using (var iLines = lines.ChildrenW)
                    {
                        iLines.RemoveAt(iLines.Count - 1);
                        Brain.Current.Delete(iLast);
                        iCount = iLines.Count;
                    }
                }
            }
            else
            {
                while (iCount < height)
                {
                    var iNew = NeuronFactory.GetCluster();
                    Brain.Current.Add(iNew);
                    using (var iLines = lines.ChildrenW)
                    {
                        iLines.Add(iNew);
                        for (var i = 0; i < width; i++)
                        {
                            using (var iNewChildren = iNew.ChildrenW) AddPixel(iNewChildren);
                        }

                        iCount = iLines.Count;
                    }
                }
            }
        }

        /// <summary>Removes the last pixel.</summary>
        /// <param name="list">The list. Must be writeable</param>
        private void RemoveLastPixel(System.Collections.Generic.IList<ulong> list)
        {
            var iPixel = Brain.Current[list[list.Count - 1]];
            var iColor = iPixel.FindFirstOut((ulong)PredefinedNeurons.Red);
            Brain.Current.Delete(iColor); // this will also remove the links.
            iColor = iPixel.FindFirstOut((ulong)PredefinedNeurons.Green);
            Brain.Current.Delete(iColor);
            iColor = iPixel.FindFirstOut((ulong)PredefinedNeurons.Blue);
            Brain.Current.Delete(iColor);
            iColor = iPixel.FindFirstOut((ulong)PredefinedNeurons.Gray);
            Brain.Current.Delete(iColor);
            list.RemoveAt(list.Count - 1);
            Brain.Current.Delete(iPixel);
        }

        /// <summary>Adds an empty pixel to the cluster.</summary>
        /// <param name="list">The list to add the pixel to (usually the list of the cluster).  This is provided so that the accessor
        ///     can be shared across calls.  Must be writable.</param>
        private void AddPixel(ChildrenAccessor list)
        {
            var iPixel = NeuronFactory.GetNeuron();
            Brain.Current.Add(iPixel);
            list.Add(iPixel);

            var iColor = NeuronFactory.GetInt();
            Brain.Current.Add(iColor);
            iPixel.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Blue, iColor);

            iColor = NeuronFactory.GetInt();
            Brain.Current.Add(iColor);
            iPixel.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Green, iColor);

            iColor = NeuronFactory.GetInt();
            Brain.Current.Add(iColor);
            iPixel.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Red, iColor);

            iColor = NeuronFactory.GetInt();
            Brain.Current.Add(iColor);
            iPixel.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.Gray, iColor);
        }

        #region Overrides

        /// <summary>Tries to translate the specified neuron to the output type of the Sin and send it to the outside world.</summary>
        /// <param name="toSend"></param>
        /// <remarks><para>When this imageSin is being send back as output, we raise the event that the image has been processed.</para>
        /// </remarks>
        public override void Output(System.Collections.Generic.IList<Neuron> toSend)
        {
            foreach (var i in toSend)
            {
                if (i == this && IsReady != null)
                {
                    IsReady(this, new System.EventArgs());
                }
            }
        }

        /// <summary>
        ///     Called when the data needs to be saved.
        /// </summary>
        public override void Flush()
        {
            // don't need to do anything?
        }

        #endregion

        #region Process

        /// <summary>Tries to load an image from file and send this to the brain.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="proc">The processor to use for handling the file</param>
        public void Process(string filename, Processor proc)
        {
            if (System.IO.File.Exists(filename))
            {
                var iImage = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(filename));
                if (iImage != null)
                {
                    Process(iImage, proc);
                }
            }
            else
            {
                throw new System.InvalidOperationException("File not found!");
            }
        }

        /// <summary>Loads the specified source image into the brain after it is scaled to fit this sin.</summary>
        /// <param name="source">The source.</param>
        /// <param name="proc">The processor to use for handling the image.</param>
        /// <remarks>This function checks if the image needs to be resized and does this if needed.  It also makes
        ///     certain that the image is of correct pixelformat which is important for loading the image.</remarks>
        public void Process(System.Windows.Media.Imaging.BitmapSource source, Processor proc)
        {
            if (source.PixelWidth != Width || source.PixelHeight != Height)
            {
                // need to resize
                var iScale = new System.Windows.Media.Imaging.TransformedBitmap();

                    // Create the new BitmapSource that will be used to scale the size of the source.
                iScale.BeginInit();

                    // BitmapSource objects like TransformedBitmap can only have their properties changed within a BeginInit/EndInit block.
                iScale.Source = source;
                iScale.Transform = new System.Windows.Media.ScaleTransform(
                    Width / source.PixelWidth, 
                    Height / source.PixelHeight);
                iScale.EndInit();
                source = iScale;
            }

            if (source.Format != System.Windows.Media.PixelFormats.Bgra32
                && source.Format != System.Windows.Media.PixelFormats.Bgr24)
            {
                // need to change the pixel format.
                var iConv = new System.Windows.Media.Imaging.FormatConvertedBitmap();
                iConv.BeginInit();
                iConv.Source = source;
                iConv.DestinationFormat = System.Windows.Media.PixelFormats.Bgr24;
                iConv.EndInit();
            }

            InternalProcess(source, proc);
        }

        /// <summary>Processes the specified source as a byte stream, so no convertion needs to be made from bytestream to image and
        ///     back again if a bytestream
        ///     is what the caller has. If you have an image, best use this directly since extra checking for convertion
        ///     requirements are made.</summary>
        /// <remarks>Required pixel format: PixelFormats.Bgr24. So each pixel takes up 3 bytes in the input stream. Width and height
        ///     must match that
        ///     of the sin. No checking is done for this.</remarks>
        /// <param name="source">The source.</param>
        /// <param name="proc">The proc.</param>
        public void Process(byte[] source, Processor proc)
        {
            if (IsProcessing)
            {
                throw new System.InvalidOperationException("Previous image has not yet been processed.");
            }

            IsProcessing = true;
            try
            {
                var iEntry = FindFirstOut((ulong)PredefinedNeurons.EntryPoints) as NeuronCluster;
                if (iEntry == null)
                {
                    // if for some reason, the entrypoints have not yet been created, do it now.  Should not be required though.
                    ReBuildStartPoints(Height, Width);
                }

                System.Diagnostics.Debug.Assert(iEntry != null);
                ProcessPixels(iEntry, source, 3);
                var iExec = NeuronFactory.GetNeuron(); // the  neuron that we use to start the exeuction process.
                Brain.Current.Add(iExec);
                iExec.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.EntryPoints, this);

                    // we link the executablt to the sin and not the entrypoints, this should be better for processing (less code?).
                Process(iExec, proc, source.ToString());
            }
            finally
            {
                IsProcessing = false;

                    // as soon as we set the data, we allow a new one. The system may still be processing the current image, that's up to the caller.
            }
        }

        /// <summary>Internal load: translates the image into neurons.</summary>
        /// <param name="source">The source.</param>
        /// <param name="proc">The processor that will evaulate the image neurons.</param>
        /// <remarks><para>Doesn't check the size or the pixelformat of the image.  This must already have been adjusted.</para>
        /// <para>The pixelformat expected is either Pbgra32 or Bgr24 (prefered, smaller data).</para>
        /// </remarks>
        private void InternalProcess(System.Windows.Media.Imaging.BitmapSource source, Processor proc)
        {
            if (IsProcessing)
            {
                throw new System.InvalidOperationException("Previous image has not yet been processed.");
            }

            IsProcessing = true;
            try
            {
                var iEntry = FindFirstOut((ulong)PredefinedNeurons.EntryPoints) as NeuronCluster;
                if (iEntry == null)
                {
                    // if for some reason, the entrypoints have not yet been created, do it now.  Should not be required though.
                    ReBuildStartPoints(Height, Width);
                }

                System.Diagnostics.Debug.Assert(iEntry != null);

                var iWidth = source.PixelWidth;
                var iStride = iWidth * (source.Format.BitsPerPixel / 8);
                var iPixels = new byte[source.PixelHeight * iStride];

                    // need a byte for each pixel in each collumn and row + for each color.
                source.CopyPixels(iPixels, iStride, 0);

                if (source.Format == System.Windows.Media.PixelFormats.Bgr24)
                {
                    ProcessPixels(iEntry, iPixels, 3);
                }
                else if (source.Format == System.Windows.Media.PixelFormats.Bgra32)
                {
                    ProcessPixels(iEntry, iPixels, 4);
                }
                else
                {
                    throw new System.InvalidOperationException(
                        "Image format not supported by ImageSin, only Bgra32 or Bgr24 are supported.");
                }

                Process(this, proc, source.ToString());

                    // an imagesin solves itself. This is because all the data is in a cluster as a relationship.
            }
            finally
            {
                IsProcessing = false;

                    // as soon as we set the data, we allow a new one. The system may still be processing the current image, that's up to the caller.
            }
        }

        /// <summary>The process pixels.</summary>
        /// <param name="entry">The entry.</param>
        /// <param name="pixels">The pixels.</param>
        /// <param name="byteCount">The byte count.</param>
        private void ProcessPixels(NeuronCluster entry, byte[] pixels, int byteCount)
        {
            int iIndex;
            System.Collections.Generic.List<NeuronCluster> iList;
            using (var iChildren = entry.Children) iList = iChildren.ConvertTo<NeuronCluster>();
            var i = 0;
            foreach (var iLine in iList)
            {
                var iLineList = iLine.Children;
                iLineList.Lock();
                try
                {
                    for (var u = 0; u < iLineList.CountUnsafe; u++)
                    {
                        iIndex = i * iLineList.CountUnsafe * byteCount + (u * byteCount);
                        var iPixel = Brain.Current[iLineList.GetUnsafe(u)];
                        SetPixel(iPixel, pixels[iIndex], pixels[iIndex + 1], pixels[iIndex + 2]);

                            // first link must be blue, second link must be green, third red.
                    }
                }
                finally
                {
                    iLineList.Dispose();
                }

                i++;
            }
        }

        #endregion
    }
}