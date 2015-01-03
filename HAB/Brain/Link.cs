// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Link.cs" company="">
//   
// </copyright>
// <summary>
//   A link between 2 <see cref="Neuron" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using System.Linq;

    /// <summary>
    ///     A link between 2 <see cref="Neuron" />s.
    /// </summary>
    /// <remarks>
    ///     A link can't be a neuron itself cause temporary links get created and destroyed very often during processing of
    ///     neurons,
    ///     this would tax the system to much.
    /// </remarks>
    public class Link : System.Xml.Serialization.IXmlSerializable
    {
        /// <summary>Finds the first link between to and from with the specified meaning and returns it.</summary>
        /// <param name="from">the from part of the link.</param>
        /// <param name="to">the To part of the link.</param>
        /// <param name="meaning">The meaning of the link.</param>
        /// <returns>The linkk that was found or null</returns>
        public static Link Find(Neuron from, Neuron to, Neuron meaning)
        {
            var iLock = LockRequestList.Create();
            var iReq = LockRequestInfo.Create();
            iReq.Neuron = from;
            iReq.Level = LockLevel.LinksOut;
            iReq.Writeable = false;
            iLock.Add(iReq);

            iReq = LockRequestInfo.Create();
            iReq.Neuron = to;
            iReq.Level = LockLevel.LinksIn;
            iReq.Writeable = false;
            iLock.Add(iReq);

            Link iRes = null;
            LockManager.Current.RequestLocks(iLock);
            try
            {
                iRes = Neuron.FindLinkUnsafe(from, to, meaning);
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iLock);
            }

            return iRes;
        }

        /// <summary>Finds the first link between to and from with the specified meaning and returns it.
        ///     This is done without locking anything, so it should only be used from within an already
        ///     existing lock.</summary>
        /// <param name="from">the from part of the link.</param>
        /// <param name="to">the To part of the link.</param>
        /// <param name="meaning">The meaning of the link.</param>
        /// <returns>The linkk that was found or null</returns>
        internal static Link FindUnsafe(Neuron from, Neuron to, Neuron meaning)
        {
            return Neuron.FindLinkUnsafe(from, to, meaning);
        }

        /// <summary>Finds the index of the link in the indexer list.</summary>
        /// <param name="from">From part of the link</param>
        /// <param name="to">To part of the link</param>
        /// <param name="meaning">The meaning part of the link</param>
        /// <param name="indexer">The indexer is either 'from' or 'to', it indicates relative to which neuron the index should be
        ///     returned.</param>
        /// <returns>The <see cref="int"/>.</returns>
        public static int FindIndex(Neuron from, Neuron to, Neuron meaning, Neuron indexer)
        {
            var iCounter = 0;
            if (indexer == from)
            {
                if (from.LinksOutIdentifier != null)
                {
                    using (var iList = from.LinksOut)
                        foreach (var i in iList)
                        {
                            if (i.ToID == to.ID && i.MeaningID == meaning.ID)
                            {
                                return iCounter;
                            }

                            iCounter++;
                        }
                }
            }
            else if (indexer == to)
            {
                if (to.LinksInIdentifier != null)
                {
                    using (var iLinks = to.LinksIn)
                    {
                        foreach (var i in iLinks)
                        {
                            if (i.FromID == from.ID && i.MeaningID == meaning.ID)
                            {
                                return iCounter;
                            }

                            iCounter++;
                        }
                    }
                }
            }
            else
            {
                throw new BrainException("Invalid indexer specified, must be equal to either 'to' or 'from'");
            }

            return -1; // -1 indicates not found.
        }

        /// <summary>Searches the first link with the specified meaning in the<see cref="Neuron.LinksOut"/> list of the from arg  and changes it's <see cref="Link.To"/>
        ///     reference to the new value. If this value is null, the link will be removed.</summary>
        /// <remarks><para>this is thread safe.</para>
        /// Used by property setters.</remarks>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <param name="meaning">The meaning to search for.</param>
        public static void SetFirstOutTo(Neuron from, Neuron to, ulong meaning)
        {
            SetFirstOutTo(from, to, Brain.Current[meaning]);
        }

        /// <summary>Searches the first link with the specified meaning in the<see cref="Neuron.LinksOut"/> list of the from arg  and changes it's <see cref="Link.To"/>
        ///     reference to the new value. If this value is null, the link will be removed.</summary>
        /// <remarks><para>this is thread safe.</para>
        /// Used by property setters.</remarks>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <param name="meaning">The meaning to search for.</param>
        public static void SetFirstOutTo(Neuron from, Neuron to, Neuron meaning)
        {
            Link iLink;
            var iArgs = new CreateArgs { To = to, From = from, Meaning = meaning };
            if (to != null && to.ID == Neuron.TempId)
            {
                Brain.Current.Add(to);
            }

            if (from.ID == Neuron.TempId)
            {
                Brain.Current.Add(from);
                iLink = new Link();
                iLink.InternalCreate(iArgs);
                return;
            }

            iLink = GetLinkForSetFirstOut(iArgs);
            if (iLink != null)
            {
                if (to == null)
                {
                    iLink.Destroy(); // we do this outside of the lock, cause we also need to lock any info on the link.
                }
                else
                {
                    iLink.To = to;

                        // also needs to be outside of the lock cause we need to lock the old to as well in the same lock, otherwise we can have deadlock situations.
                }
            }
            else if (to != null)
            {
                iLink = new Link(iArgs);
            }
        }

        /// <summary>The get link for set first out.</summary>
        /// <param name="iArgs">The i args.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        private static Link GetLinkForSetFirstOut(CreateArgs iArgs)
        {
            Link iLink = null;
            if (iArgs.From.LinksOutIdentifier != null)
            {
                var iReq = LockRequestList.Create();
                BuildLock(iReq, iArgs);
                LockManager.Current.RequestLocks(iReq);
                try
                {
                    iLink =
                        (from i in iArgs.From.LinksOutIdentifier where i.MeaningID == iArgs.Meaning.ID select i)
                            .FirstOrDefault();
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iReq);
                }
            }

            return iLink;
        }

        /// <summary>Searches the first link with the specified meaning in the<see cref="Neuron.LinksIn"/> list of the from arg  and changes it's <see cref="Link.To"/>
        ///     reference to the new value. If this value is null, the link will be removed.</summary>
        /// <remarks><para>this is thread safe.</para>
        /// Used by property setters.</remarks>
        /// <param name="from">The from.</param>
        /// <param name="value">The value to assign to the 'From' property.</param>
        /// <param name="meaning">The meaning to search for.</param>
        public static void SetFirstInTo(Neuron from, Neuron value, ulong meaning)
        {
            Link iLink;

            System.Diagnostics.Debug.Assert(meaning != Neuron.TempId); // something strange going on here.
            using (var iLinks = from.LinksIn) iLink = (from i in iLinks where i.MeaningID == meaning select i).FirstOrDefault();
            if (value != null)
            {
                if (iLink != null)
                {
                    iLink.From = value;
                }
                else
                {
                    iLink = new Link(from, value, meaning);
                }
            }
            else if (iLink != null)
            {
                iLink.Destroy();
            }
        }

        /// <summary>
        ///     resets all the data of the  link to 0, so that everyone knows this links is no longer valid.
        /// </summary>
        internal void Reset()
        {
            FromID = Neuron.EmptyId;
            ToID = Neuron.EmptyId;
            MeaningID = Neuron.EmptyId;
        }

        #region internal types

        /// <summary>
        ///     Arguments used for the <see cref="Link.InternalCreate" /> function.  Have been put in class because they are so
        ///     many.
        /// </summary>
        public class CreateArgs
        {
            /// <summary>Initializes a new instance of the <see cref="CreateArgs"/> class.</summary>
            public CreateArgs()
            {
                ToIndex = -1;
                FromIndex = -1;
            }

            /// <summary>Gets or sets the to.</summary>
            public Neuron To { get; set; }

            /// <summary>Gets or sets the to index.</summary>
            public int ToIndex { get; set; }

            /// <summary>Gets or sets the from.</summary>
            public Neuron From { get; set; }

            /// <summary>Gets or sets the from index.</summary>
            public int FromIndex { get; set; }

            /// <summary>Gets or sets the meaning.</summary>
            public Neuron Meaning { get; set; }
        }

        /// <summary>
        ///     for the multi duplicator, so we can craete multiple copies of the same link in 1 go.
        /// </summary>
        internal class MultiCreateArgs
        {
            /// <summary>The from.</summary>
            public System.Collections.Generic.IList<Neuron> From;

            /// <summary>The meaning.</summary>
            public Neuron Meaning;

            /// <summary>The to.</summary>
            public System.Collections.Generic.IList<Neuron> To;
        }

        #endregion

        #region fields

        /// <summary>The f info acc.</summary>
        private LinkInfoAccessor fInfoAcc;

        /// <summary>The f info.</summary>
        private LinkInfoList fInfo;

        /// <summary>Gets the f info.</summary>
        private LinkInfoList FInfo
        {
            get
            {
                if (fInfo == null)
                {
                    fInfo = new LinkInfoList(this);
                }

                return fInfo;
            }
        }

        #endregion

        #region ctor

        /// <summary>Creates a link from the specified neuron to the specified link, using the specified meaning.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        public static Link Create(Neuron from, Neuron to, ulong meaning)
        {
            var iNew = new Link(to, from, meaning);
            return iNew;
        }

        /// <summary>The create.</summary>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        public static Link Create(Neuron from, ulong to, ulong meaning)
        {
            var iNew = new Link(Brain.Current[to], from, meaning);
            return iNew;
        }

        /// <summary>The create.</summary>
        /// <param name="from">The from.</param>
        /// <param name="to">The to.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link"/>.</returns>
        public static Link Create(Neuron from, Neuron to, Neuron meaning)
        {
            var iNew = new Link(to, from, meaning);
            return iNew;
        }

        #region For duplicators

        /// <summary>Creates a multi link in an unsafe manner. This is used for the duplication process.
        ///     Doesn't effect any changes in the Unfreeze state.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link[]"/>.</returns>
        internal static Link[] CreateUnsafe(System.Collections.Generic.IList<Neuron> from, Neuron to, Neuron meaning)
        {
            var iArgs = new MultiCreateArgs();
            iArgs.From = from;
            var iMemFac = Factories.Default;
            iArgs.To = iMemFac.NLists.GetBuffer();
            iArgs.To.Add(to);
            iArgs.Meaning = meaning;
            var iRes = InternalCreateUnsafe(iArgs);
            iMemFac.NLists.Recycle((System.Collections.Generic.List<Neuron>)iArgs.To);
            return iRes;
        }

        /// <summary>Creates a multi link in an unsafe manner. This is used for the duplication process.
        ///     Doesn't effect any changes in the Unfreeze state.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaning">The meaning.</param>
        /// <returns>The <see cref="Link[]"/>.</returns>
        internal static Link[] CreateUnsafe(Neuron from, System.Collections.Generic.IList<Neuron> to, Neuron meaning)
        {
            var iArgs = new MultiCreateArgs();
            var iMemFac = Factories.Default;
            iArgs.From = iMemFac.NLists.GetBuffer();
            iArgs.From.Add(from);
            iArgs.To = to;
            iArgs.Meaning = meaning;
            var iRes = InternalCreateUnsafe(iArgs);
            iMemFac.NLists.Recycle((System.Collections.Generic.List<Neuron>)iArgs.From);
            return iRes;
        }

        /// <summary>creates a link without locking anything. this is for the instructions.</summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="meaning"></param>
        /// <returns>The <see cref="Link"/>.</returns>
        internal static Link CreateLinkUnsafe(Neuron from, Neuron to, Neuron meaning)
        {
            var iArgs = new CreateArgs();
            iArgs.From = from;
            iArgs.To = to;
            iArgs.Meaning = meaning;
            var iRes = new Link();
            iRes.CreateLinkUnsafe(iArgs);
            return iRes;
        }

        #endregion

        /// <summary>Prevents a default instance of the <see cref="Link"/> class from being created. 
        ///     Default constructor.</summary>
        private Link()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class. Constructor that specifies the 2 neurons that need to be connected, using references to the objects and a meaning.</summary>
        /// <remarks>if to, from or meaning have a <see cref="Neuron.TempId"/> identifier, they are first registered.</remarks>
        /// <param name="to">The source of the link (stored in <see cref="Link.To"/>)</param>
        /// <param name="from">The destination of the link (stored in <see cref="Link.From"/>)</param>
        /// <param name="meaning">the meaning of the link.</param>
        public Link(Neuron to, Neuron from, Neuron meaning)
        {
            System.Diagnostics.Debug.Assert(to != null);
            System.Diagnostics.Debug.Assert(from != null);
            System.Diagnostics.Debug.Assert(meaning != null);
            var iArgs = new CreateArgs { To = to, From = from, Meaning = meaning };
            InternalCreate(iArgs);
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class. Constructor that specifies the 2 neurons that need to be connected, using references to the objects and a meaning
        ///     as ulong.</summary>
        /// <remarks>if to or from have a <see cref="Neuron.TempId"/> identifier, they are first registered. Meaning must be a valid
        ///     registered
        ///     id.</remarks>
        /// <param name="to">The source of the link (stored in <see cref="Link.To"/>)</param>
        /// <param name="from">The destination of the link (stored in <see cref="Link.From"/>)</param>
        /// <param name="meaning">the meaning of the link.</param>
        public Link(Neuron to, Neuron from, ulong meaning)
        {
            System.Diagnostics.Debug.Assert(to != null);
            System.Diagnostics.Debug.Assert(from != null);
            if (meaning == Neuron.EmptyId)
            {
                throw new System.ArgumentException(
                    "The neuron has an invalid Id, has it been added to the Brain already?", 
                    "from");
            }

            var iArgs = new CreateArgs { To = to, From = from, Meaning = Brain.Current[meaning] };
            InternalCreate(iArgs);
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class.</summary>
        /// <param name="to">The to.</param>
        /// <param name="toIndex">The to index.</param>
        /// <param name="from">The from.</param>
        /// <param name="meaning">The meaning.</param>
        public Link(Neuron to, int toIndex, Neuron from, Neuron meaning)
        {
            var iArgs = new CreateArgs { To = to, From = from, Meaning = meaning, ToIndex = toIndex };
            InternalCreate(iArgs);
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class.</summary>
        /// <param name="to">The to.</param>
        /// <param name="from">The from.</param>
        /// <param name="fromIndex">The from index.</param>
        /// <param name="meaning">The meaning.</param>
        public Link(Neuron to, Neuron from, int fromIndex, Neuron meaning)
        {
            var iArgs = new CreateArgs { To = to, From = from, Meaning = meaning, FromIndex = fromIndex };
            InternalCreate(iArgs);
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class.</summary>
        /// <param name="to">The to.</param>
        /// <param name="toIndex">The to index.</param>
        /// <param name="from">The from.</param>
        /// <param name="fromIndex">The from index.</param>
        /// <param name="meaning">The meaning.</param>
        public Link(Neuron to, int toIndex, Neuron from, int fromIndex, Neuron meaning)
        {
            var iArgs = new CreateArgs
                            {
                                To = to, 
                                From = from, 
                                Meaning = meaning, 
                                FromIndex = fromIndex, 
                                ToIndex = toIndex
                            };
            InternalCreate(iArgs);
        }

        /// <summary>Checks the args for the constructors and throws the correct exceptions when needed.</summary>
        /// <param name="args">The args.</param>
        private void CheckArgs(CreateArgs args)
        {
            // Neuron to, Neuron from, Neuron meaning)
            if (Neuron.LinkExistsUnsafe(args))
            {
                throw new BrainException(
                    string.Format(
                        "Can't create link from: {0}, to: {1}, meaning:{2}.  Link already exists.", 
                        args.To, 
                        args.From, 
                        args.Meaning));
            }

            // check isDeleted after exists, maybe something went wrong while releasing the locks and they got deletd in the mean time.
            if (args.To.IsDeleted || args.To.ID == Neuron.TempId)
            {
                // this also checks if the neuron is in the process if being deleted, but not all lists have been emptied yet.
                throw new System.ArgumentException(
                    "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                    "to");
            }

            if (args.From.IsDeleted || args.From.ID == Neuron.TempId)
            {
                throw new System.ArgumentException(
                    "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                    "from");
            }

            if (args.Meaning.IsDeleted || args.Meaning.ID == Neuron.TempId)
            {
                throw new System.ArgumentException(
                    "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                    "meaning");
            }
        }

        /// <summary>Checks the args for a multiCreate situation (multiduplicator).</summary>
        /// <param name="args">The args.</param>
        private static void CheckArgs(MultiCreateArgs args)
        {
            foreach (var i in args.From)
            {
                if (i.IsDeleted)
                {
                    throw new System.ArgumentException(
                        "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                        i.ID.ToString());
                }
            }

            foreach (var i in args.To)
            {
                if (i.IsDeleted)
                {
                    throw new System.ArgumentException(
                        "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                        i.ID.ToString());
                }
            }

            if (args.Meaning.IsDeleted || args.Meaning.ID == Neuron.TempId)
            {
                throw new System.ArgumentException(
                    "The neuron has an invalid Id, has it been deleted from or added to the network already?", 
                    "meaning");
            }
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class. An internal constructor used for readin from xml file, only saves the data.</summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="meaning"></param>
        internal Link(ulong to, ulong from, ulong meaning)
        {
            ToID = to;
            FromID = from;
            MeaningID = meaning;
        }

        /// <summary>Initializes a new instance of the <see cref="Link"/> class. Internal constructor used by the processor when the action was cashed.</summary>
        /// <param name="args"></param>
        internal Link(CreateArgs args)
        {
            InternalCreate(args);
        }

        /// <summary>Performs the actual init of the object, creates the links, but it only adds the link to the lists if it's valid
        ///     for the current processor state.</summary>
        /// <param name="args">The args.</param>
        /// <remarks>if to, from have a <see cref="Neuron.TempId"/> identifier, they are first registered.</remarks>
        private void InternalCreate(CreateArgs args)
        {
            if (args.To.ID == Neuron.TempId)
            {
                Brain.Current.Add(args.To);
            }

            if (args.From.ID == Neuron.TempId)
            {
                Brain.Current.Add(args.From);
            }

            if (args.Meaning.ID == Neuron.TempId)
            {
                Brain.Current.Add(args.Meaning);
            }

            var iReq = LockRequestList.Create();
            BuildLock(iReq, args);
            LockManager.Current.RequestLocks(iReq);
            try
            {
                CreateLinkUnsafe(args);
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }

            args.To.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            args.From.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            args.Meaning.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
        }

        /// <summary>sets up all the data for a link in an unsafe manner. This should never be called directly, but always wrapped in a
        ///     lock somehow.</summary>
        /// <param name="args">The args.</param>
        private void CreateLinkUnsafe(CreateArgs args)
        {
            CheckArgs(args);

                // this must be done from within the lock, otherwise the state could have changed between this call and after the lock was acquired.
            ToID = args.To.ID;
            FromID = args.From.ID;
            MeaningID = args.Meaning.ID;
            args.Meaning.IncMeaningUnsafe();

            if (args.ToIndex == -1)
            {
                args.To.AddInboundLink(this);
            }
            else
            {
                args.To.InsertInboundLink(this, args.ToIndex);
            }

            if (args.FromIndex == -1)
            {
                args.From.AddOutgoingLink(this);
            }
            else
            {
                args.From.InsertOutgoingLink(this, args.FromIndex);
            }

            if (Brain.Current.HasLinkChangedEvents)
            {
                // this needs to happen within the lock, if we don't do this, we can not monitor attached nuerons correctly for instance, cause 1 of the neurons might already have been deleted when we get to processing the event handler.
                var iArgs = new LinkChangedEventArgs
                                {
                                    Action = BrainAction.Created, 
                                    NewTo = args.To.ID, 
                                    NewFrom = args.From.ID, 
                                    NewMeaning = args.Meaning.ID, 
                                    OriginalSource = this
                                };
                Brain.Current.OnLinkChanged(iArgs);
            }
        }

        ///// <summary>
        ///// sets up all the data for a link in an unsafe manner without changing any freeze state. This should never be called directly, 
        ///// but always wrapped in a lock somehow and only during a duplication process. 
        ///// </summary>
        ///// <param name="args">The args.</param>
        // private void CreateLinkUnsafeNoUnfreeze(CreateArgs args)
        // {
        // CheckArgs(args);                            //this must be done from within the lock, otherwise the state could have changed between this call and after the lock was acquired.
        // fTo = args.To.ID;
        // fFrom = args.From.ID;
        // fMeaning = args.Meaning.ID;
        // args.Meaning.IncMeaningUnsafeNoUnfreeze();

        // if (args.ToIndex == -1)
        // args.To.AddInboundLinkNoUnfreeze(this);
        // else
        // args.To.InsertInboundLinkNoUnfreeze(this, args.ToIndex);
        // if (args.FromIndex == -1)
        // args.From.AddOutgoingLinkNoUnfreeze(this);
        // else
        // args.From.InsertOutgoingLinkNoUnfreeze(this, args.FromIndex);
        // if (Brain.Current.HasLinkChangedEvents == true)                         //this needs to happen within the lock, if we don't do this, we can not monitor attached nuerons correctly for instance, cause 1 of the neurons might already have been deleted when we get to processing the event handler.
        // {
        // LinkChangedEventArgs iArgs = new LinkChangedEventArgs()
        // {
        // Action = BrainAction.Created,
        // NewTo = args.To.ID,
        // NewFrom = args.From.ID,
        // NewMeaning = args.Meaning.ID,
        // OriginalSource = this
        // };
        // Brain.Current.OnLinkChanged(iArgs);
        // }
        // }

        /// <summary>for creating multiple links at the same time.
        ///     Warning: less checks are done + no locking whatsoever + no changes in the freeze state.</summary>
        /// <param name="args">The args.</param>
        /// <returns>The <see cref="Link[]"/>.</returns>
        private static Link[] InternalCreateUnsafe(MultiCreateArgs args)
        {
            var iRes = Factories.Default.LinkLists.GetBuffer();

            // some debug code
            CheckArgs(args);

                // this must be done from within the lock, otherwise the state could have changed between this call and after the lock was acquired.
            foreach (var iFrom in args.From)
            {
                foreach (var iTo in args.To)
                {
                    var iNew = new Link();
                    iRes.Add(iNew);
                    iNew.ToID = iTo.ID;
                    iNew.FromID = iFrom.ID;
                    iNew.MeaningID = args.Meaning.ID;
                    args.Meaning.IncMeaningUnsafe();
                    iTo.AddInboundLink(iNew);
                    iFrom.AddOutgoingLink(iNew);
                    if (Brain.Current.HasLinkChangedEvents)
                    {
                        // this needs to happen within the lock, if we don't do this, we can not monitor attached nuerons correctly for instance, cause 1 of the neurons might already have been deleted when we get to processing the event handler.
                        var iArgs = new LinkChangedEventArgs
                                        {
                                            Action = BrainAction.Created, 
                                            NewTo = iTo.ID, 
                                            NewFrom = iFrom.ID, 
                                            NewMeaning = args.Meaning.ID, 
                                            OriginalSource = iNew
                                        };
                        Brain.Current.OnLinkChanged(iArgs);
                    }
                }
            }

            return iRes.ToArray();
        }

        #endregion

        #region Prop

        #region To

        /// <summary>
        ///     Gets/sets the object that this link points to.
        /// </summary>
        /// <value>To.</value>
        /// <remarks>
        ///     The getter throws an exception if not found.
        /// </remarks>
        public Neuron To
        {
            get
            {
                if (ToID != Neuron.EmptyId)
                {
                    return Brain.Current[ToID];
                }

                return null;
            }

            set
            {
                if ((value == null && ToID != Neuron.EmptyId) || (value != null && ToID != value.ID))
                {
                    if (value.ID == Neuron.TempId)
                    {
                        Brain.Current.Add(value);
                    }

                    var iReq = LockRequestList.Create();
                    var iFrom = From;
                    var iTo = To;
                    BuildLockForTo(iReq, iFrom, null, iTo, value);
                    LockManager.Current.RequestLocks(iReq);
                    try
                    {
                        if (Neuron.LinkExistsUnsafe(value, iTo, MeaningID))
                        {
                            // this statement  is important, it needs to be done before fto is changed.  This way, both neurons are loaded from disk and the link is this object, so we everything stays ok.
                            throw new BrainException(
                                string.Format(
                                    "Link between from:{0} to: {1}, meaning: {2} already exists.  old To ({3} remains)", 
                                    FromID, 
                                    value.ID, 
                                    MeaningID, 
                                    ToID));
                        }

                        iTo.RemoveInboundLink(this);
                        var iPrev = ToID;
                        if (value != null)
                        {
                            ToID = value.ID;
                        }
                        else
                        {
                            ToID = Neuron.EmptyId;
                        }

                        if (ToID != Neuron.EmptyId)
                        {
                            value.AddInboundLink(this);
                        }

                        ResetRulesBuffer();
                        if (Brain.Current.HasLinkChangedEvents)
                        {
                            var iArgs = new LinkChangedEventArgs
                                            {
                                                Action = BrainAction.Changed, 
                                                NewFrom = FromID, 
                                                OldFrom = FromID, 
                                                OldMeaning = MeaningID, 
                                                NewMeaning = MeaningID, 
                                                NewTo = value.ID, 
                                                OldTo = iPrev, 
                                                OriginalSource = this
                                            };
                            Brain.Current.OnLinkChanged(iArgs);
                        }
                    }
                    finally
                    {
                        LockManager.Current.ReleaseLocks(iReq);
                    }

                    value.SetIsChangedNoClearBuffers(true);

                        // don't clear the buffers, this is already done during the action itself. = true;
                    iTo.SetIsChangedNoClearBuffers(true);

                        // don't clear the buffers, this is already done during the action itself. = true;
                    iFrom.SetIsChangedNoClearBuffers(true);

                        // don't clear the buffers, this is already done during the action itself. = true;
                }
            }
        }

        /// <summary>
        ///     When this link is changed and the meaning is 'Actions', we let the 'from' part
        ///     know that it needs to reset it's buffers.
        /// </summary>
        private void ResetRulesBuffer()
        {
            if (MeaningID == (ulong)PredefinedNeurons.Rules)
            {
                var iFrom = From;
                if (iFrom != null)
                {
                    iFrom.fRulesList = null; // all cached code needs to be cleared when links out has changed.
                }
            }
        }

        /// <summary>The reset rules buffer.</summary>
        /// <param name="from">The from.</param>
        private void ResetRulesBuffer(Neuron from)
        {
            if (MeaningID == (ulong)PredefinedNeurons.Rules)
            {
                if (from != null)
                {
                    from.fRulesList = null; // all cached code needs to be cleared when links out has changed.
                }
            }
        }

        /// <summary>
        ///     Touches the IsChanged prop of the specified neuron.
        /// </summary>
        /// <remarks>
        ///     When a link is changed, we need to make certain that both neurons get signalled as changed,
        ///     cause both neurons store the link info.
        /// </remarks>
        /// <param name="item">The item.</param>
        /// <summary>
        ///     Gets the id of the To Neuron
        /// </summary>
        /// <remarks>
        ///     Convenience prop for quick access to the id.
        /// </remarks>
        public ulong ToID { get; private set; }

        #endregion

        #region From

        /// <summary>
        ///     Gets/sets the id of the object that this link originates from.
        /// </summary>
        /// <remarks>
        ///     The getter throws an exception if not found.
        /// </remarks>
        public Neuron From
        {
            get
            {
                if (FromID != Neuron.EmptyId)
                {
                    return Brain.Current[FromID];
                }

                return null;
            }

            set
            {
                if ((value == null && FromID != Neuron.EmptyId) || (value != null && FromID != value.ID))
                {
                    if (value.ID == Neuron.TempId)
                    {
                        Brain.Current.Add(value);
                    }

                    SetFromValue(value);
                }
            }
        }

        /// <summary>The set from value.</summary>
        /// <param name="value">The value.</param>
        /// <exception cref="BrainException"></exception>
        private void SetFromValue(Neuron value)
        {
            var iTo = To;
            var iFrom = From;
            var iReq = LockRequestList.Create();
            BuildLockForFrom(iReq, iFrom, value, iTo, null);
            LockManager.Current.RequestLocks(iReq);
            try
            {
                if (Neuron.LinkExistsUnsafe(value, iTo, MeaningID))
                {
                    // this statement  is important, it needs to be done before fto is changed.  This way, both neurons are loaded from disk and the link is this object, so we everything stays ok.
                    throw new BrainException(
                        string.Format(
                            "Link between from:{0} to: {1}, meaning: {2} already exists.  old from ({3} remains)", 
                            value.ID, 
                            ToID, 
                            MeaningID, 
                            FromID));
                }

                if (FromID != Neuron.EmptyId)
                {
                    ResetRulesBuffer(iFrom); // make certain that the 'from' refreshed it's cached action/rules code
                    iFrom.RemoveOutgoingLink(this);
                }

                var iPrev = FromID;
                if (value != null)
                {
                    FromID = value.ID;
                }
                else
                {
                    FromID = Neuron.EmptyId;
                }

                if (FromID != Neuron.EmptyId)
                {
                    value.AddOutgoingLink(this);
                }

                if (Brain.Current.HasLinkChangedEvents)
                {
                    var iArgs = new LinkChangedEventArgs
                                    {
                                        Action = BrainAction.Changed, 
                                        OldMeaning = MeaningID, 
                                        NewMeaning = MeaningID, 
                                        NewFrom = value.ID, 
                                        OldFrom = iPrev, 
                                        NewTo = ToID, 
                                        OldTo = ToID, 
                                        OriginalSource = this
                                    };
                    Brain.Current.OnLinkChanged(iArgs);
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }

            value.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iTo.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iFrom.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
        }

        #endregion

        #region FromID

        /// <summary>
        ///     Gets the id of the FromNeuron
        /// </summary>
        /// <remarks>
        ///     Convenience prop for quick access to the id.
        /// </remarks>
        public ulong FromID { get; private set; }

        #endregion

        #region Meaning

        /// <summary>
        ///     Gets/sets the id of the object that defines the meaning of this link.
        /// </summary>
        /// <remarks>
        ///     The getter throws an exception if not found.
        /// </remarks>
        public Neuron Meaning
        {
            get
            {
                return Brain.Current[MeaningID];
            }

            set
            {
                if ((value == null && MeaningID != Neuron.EmptyId) || (value != null && MeaningID != value.ID))
                {
                    if (value.ID == Neuron.TempId)
                    {
                        Brain.Current.Add(value);
                    }

                    SetMeaning(value);
                }
            }
        }

        /// <summary>The set meaning.</summary>
        /// <param name="value">The value.</param>
        /// <exception cref="BrainException"></exception>
        private void SetMeaning(Neuron value)
        {
            var iReq = LockRequestList.Create();
            BuildMeaningLock(iReq, value);
            var iFrom = iReq[0].Neuron;
            var iTo = iReq[1].Neuron;
            Neuron iMeaning;
            if (MeaningID != Neuron.EmptyId)
            {
                iMeaning = iReq[2].Neuron;
            }
            else
            {
                iMeaning = null;
            }

            LockManager.Current.RequestLocks(iReq);
            try
            {
                if (Neuron.LinkExistsUnsafe(iFrom, iTo, value.ID))
                {
                    // this statmenet is important: it makes certain both neurons are loaded before meaning is changed, so they are both loaded correctly.
                    throw new BrainException(
                        string.Format(
                            "Link between from:{0} to: {1}, meaning: {2} already exists.  old meaning ({3} remains)", 
                            FromID, 
                            ToID, 
                            value.ID, 
                            MeaningID));
                }

                if (iMeaning != null)
                {
                    iMeaning.DecMeaningUnsafe();
                }

                ResetRulesBuffer(); // make certain that the 'from' refreshed it's cached action/rules code
                var iPrev = MeaningID;
                if (value != null)
                {
                    MeaningID = value.ID;
                    value.IncMeaningUnsafe();
                }
                else
                {
                    MeaningID = Neuron.EmptyId;
                }

                if (Brain.Current.HasLinkChangedEvents)
                {
                    var iArgs = new LinkChangedEventArgs
                                    {
                                        Action = BrainAction.Changed, 
                                        OldMeaning = iPrev, 
                                        NewMeaning = MeaningID, 
                                        NewFrom = FromID, 
                                        OldFrom = FromID, 
                                        NewTo = ToID, 
                                        OldTo = ToID, 
                                        OriginalSource = this
                                    };
                    Brain.Current.OnLinkChanged(iArgs);
                }
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }

            value.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iTo.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iFrom.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            if (iMeaning != null)
            {
                // for a create, there is no previous?
                iMeaning.SetIsChangedNoClearBuffers(true);

                    // don't clear the buffers, this is already done during the action itself. = true;
            }
        }

        #endregion

        #region MeaningID

        /// <summary>
        ///     Gets the id of the Meaning Neuron
        /// </summary>
        /// <remarks>
        ///     Convenience prop for quick access to the id.
        /// </remarks>
        public ulong MeaningID { get; private set; }

        #endregion

        #region Info

        /// <summary>
        ///     Gets the thread safe read accessor for the info list of the link.
        /// </summary>
        /// <value>The info.</value>
        public LinkInfoAccessor Info
        {
            get
            {
                if (fInfoAcc == null)
                {
                    fInfoAcc = new LinkInfoAccessor(this, false);
                }

                fInfoAcc.IsWriteable = false;
                return fInfoAcc;
            }
        }

        /// <summary>
        ///     Gets the thread safe write accessor for the info list of the link.
        /// </summary>
        /// <value>The info.</value>
        public LinkInfoAccessor InfoW
        {
            get
            {
                if (fInfoAcc == null)
                {
                    fInfoAcc = new LinkInfoAccessor(this, false);
                }

                fInfoAcc.IsWriteable = true;
                return fInfoAcc;
            }
        }

        /// <summary>
        ///     Gets an object that uniquely identifies the list that was changed.  This can be used in dicationaries for instance,
        ///     like the designer's eventManager.
        /// </summary>
        /// <value>The identifier.</value>
        public object InfoIdentifier
        {
            get
            {
                return fInfo;
            }
        }

        /// <summary>
        ///     Provides direct access to the link info list. This is used for reading the list from storage. (also in
        ///     ClearInfoInstruciton)
        /// </summary>
        /// <value>The info direct.</value>
        public LinkInfoList InfoDirect
        {
            get
            {
                return FInfo;
            }
        }

        #endregion

        /// <summary>
        ///     checks if the link is still valid or not.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return Neuron.IsEmpty(ToID) == false && Neuron.IsEmpty(FromID) == false
                       && Neuron.IsEmpty(MeaningID) == false;
            }
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Destroys the link between the 2 objects. If not all the neurons in the link, are found, an exception is thrown.
        /// </summary>
        /// <remarks>
        ///     Is able to handle invalid links.
        /// </remarks>
        public void Destroy()
        {
            if (ToID == Neuron.EmptyId && FromID == Neuron.EmptyId && MeaningID == Neuron.EmptyId)
            {
                // check if the link is actually still existing.
                return;
            }

            System.Collections.Generic.List<Neuron> iInfoList;
            var iReq = BuildCompleteLock(true, out iInfoList);

                // we are destroying the link, so better get a writeable one (this was a big oops). + also need to get a complete lock, including the info cause this also gets changed. If we wouldn't do this, we can have deadlocks.
            var iFrom = iReq[0].Neuron;
            var iTo = iReq[1].Neuron;
            var iMeaning = iReq[2].Neuron;
            if (iReq[0].Neuron.IsDeleted == false && iReq[1].Neuron.IsDeleted == false)
            {
                // if to or from are being deleted, don't try to remove the link as well, but simply unregister the data used by the lock.
                LockManager.Current.RequestLocks(iReq);
                try
                {
                    iFrom.RemoveOutgoingLink(this);

                        // we know the first neuron in the lock is the to part, so lets simply reuse this, so that we safe a duplicate neuron search.
                    iTo.RemoveInboundLink(this); // we know the second in the lock is the to part, so reuse.
                    iMeaning.DecMeaningUnsafe();
                    if (fInfo != null)
                    {
                        for (var i = 3; i < iReq.Count - 1; i++)
                        {
                            // the last item in iReq is the lock on the cache (auto created by the LockManager), this doens't have a neuron, so skip this.
                            iReq[i].Neuron.DecInfoUnsafe();
                        }
                    }

                    RaiseDestroyEvent();
                    ToID = Neuron.EmptyId; // also need to clear the values from the link object
                    FromID = Neuron.EmptyId;
                    MeaningID = Neuron.EmptyId;
                    if (fInfo != null)
                    {
                        fInfo.Clear(iInfoList);

                            // we can do this directly, cause everything is locked down and cleaned up anyway.
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLocks(iReq);
                }

                if (fInfo != null)
                {
                    iInfoList.ForEach(i => i.SetIsChangedNoClearBuffers(true));
                    Factories.Default.NLists.Recycle(iInfoList);
                }

                iFrom.SetIsChangedNoClearBuffers(true);

                    // don't clear the buffers, this is already done during the action itself. = true;
                iTo.SetIsChangedNoClearBuffers(true);

                    // don't clear the buffers, this is already done during the action itself. = true;
                iMeaning.SetIsChangedNoClearBuffers(true);

                    // don't clear the buffers, this is already done during the action itself. = true;
            }
            else if (Processor.CurrentProcessor != null)
            {
                Processor.CurrentProcessor.Mem.LocksFactory.ReleaseList(iReq);
            }
        }

        /// <summary>
        ///     Raises the destroy event, signalling the rest of the system  that this link is gone.
        ///     This is seperate cause when the entire neuron gets delete, we need to call this seperatly.
        /// </summary>
        internal void RaiseDestroyEvent()
        {
            if (Brain.Current.HasLinkChangedEvents)
            {
                // we raise the event after the link has been broken otherwise some events might work incorrectly.
                var iArgs = new LinkChangedEventArgs
                                {
                                    Action = BrainAction.Removed, 
                                    OldTo = ToID, 
                                    OldFrom = FromID, 
                                    OldMeaning = MeaningID, 
                                    OriginalSource = this
                                };
                Brain.Current.OnLinkChanged(iArgs);
            }
        }

        /// <summary>
        ///     Destroys the link, even it it's an invalid link. If not all the items exist, only what does exist is updated.
        ///     This is slower, but doesn't raise exceptions for invalid neurons. Is used to clean the network.
        /// </summary>
        public void DestroyHard()
        {
            if (ToID == Neuron.EmptyId && FromID == Neuron.EmptyId && MeaningID == Neuron.EmptyId)
            {
                // check if the link is actually still existing.
                return;
            }

            Neuron iFrom, iTo, iMeaning;
            System.Collections.Generic.List<Neuron> iInfoList;
            var iReq = BuildCompleteLock(true, out iInfoList);

                // we are destroying the lock, so better get a writeable one (this was a big oops).
            LockManager.Current.RequestLocks(iReq);
            try
            {
                if (Brain.Current.TryFindNeuron(FromID, out iFrom))
                {
                    iFrom.RemoveOutgoingLink(this);
                }

                if (Brain.Current.TryFindNeuron(ToID, out iTo))
                {
                    iTo.RemoveInboundLink(this);
                }

                Brain.Current.TryFindNeuron(MeaningID, out iMeaning);

                    // we use this way of retrieving the neuron, otherwise we get an exception if we have an invalid link.  This makes it safer.
                if (iMeaning != null)
                {
                    iMeaning.DecMeaningUnsafe();
                }

                if (fInfo != null)
                {
                    foreach (var i in iInfoList)
                    {
                        i.DecInfoUnsafe();
                    }
                }

                if (Brain.Current.HasLinkChangedEvents)
                {
                    // we raise the event after the link has been broken otherwise some events might work incorrectly.
                    var iArgs = new LinkChangedEventArgs
                                    {
                                        Action = BrainAction.Removed, 
                                        OldTo = ToID, 
                                        OldFrom = FromID, 
                                        OldMeaning = MeaningID, 
                                        OriginalSource = this
                                    };
                    Brain.Current.OnLinkChanged(iArgs);
                }

                ToID = Neuron.EmptyId; // also need to clear the values from the link object
                FromID = Neuron.EmptyId;
                MeaningID = Neuron.EmptyId;
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }

            if (fInfo != null)
            {
                iInfoList.ForEach(i => i.SetIsChangedNoClearBuffers(true));
                Factories.Default.NLists.Recycle(iInfoList);
            }

            iFrom.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iTo.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
            iMeaning.SetIsChangedNoClearBuffers(true);

                // don't clear the buffers, this is already done during the action itself. = true;
        }

        /// <summary>Checks if there is already a link in existence between the specified neurons with the specified meaning.</summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaningID">The meaning ID.</param>
        /// <returns>True if the link exists, false otherwise.</returns>
        public static bool Exists(Neuron from, Neuron to, ulong meaningID)
        {
            var iFrom = from;
            var iReq = LockRequestList.Create();
            iReq.Add(LockRequestInfo.Create(to, LockLevel.LinksIn, false));
            iReq.Add(LockRequestInfo.Create(from, LockLevel.LinksOut, false));
            LockManager.Current.RequestLocks(iReq);
            try
            {
                return Neuron.LinkExistsUnsafe(from, to, meaningID);
            }
            finally
            {
                LockManager.Current.ReleaseLocks(iReq);
            }
        }

        /// <summary>Recreates the Link between the 2 specified items with the new meaning.  This function should
        ///     only be used after <see cref="Link.Destroy"/> was called on a link.</summary>
        /// <remarks>This function is usefull if you need to keep a ref to a Link object that can be destroyed and recreted.</remarks>
        /// <exception cref="BrainException">The link was still valid (<see cref="Link.From"/>, <see cref="Link.To"/>, and<see cref="Link.Meaning"/>) were still filled in.</exception>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="meaning">The meaning.</param>
        public void Recreate(Neuron from, Neuron to, Neuron meaning)
        {
            if (FromID != Neuron.EmptyId || ToID != Neuron.EmptyId || MeaningID != Neuron.EmptyId)
            {
                throw new BrainException("The link must have been destroyed before it can be recreated.");
            }

            var iArgs = new CreateArgs { To = to, From = from, Meaning = meaning };
            InternalCreate(iArgs);
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>The get schema.</summary>
        /// <returns>The <see cref="XmlSchema"/>.</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>we reimplement xml reading/writing cause we don't want to call the setters with each read.</summary>
        /// <param name="reader">The reader.</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            var wasEmpty = reader.IsEmptyElement;

            reader.Read();
            if (wasEmpty)
            {
                return;
            }

            reader.ReadStartElement("to");
            var iVal = reader.ReadString();
            var iConverted = ulong.Parse(iVal);
            ToID = iConverted;
            reader.ReadEndElement();

            reader.ReadStartElement("from");
            iVal = reader.ReadString();
            iConverted = ulong.Parse(iVal);
            FromID = iConverted;
            reader.ReadEndElement();

            reader.ReadStartElement("meaning");
            iVal = reader.ReadString();
            iConverted = ulong.Parse(iVal);
            MeaningID = iConverted;
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        /// <summary>The write xml.</summary>
        /// <param name="writer">The writer.</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("to");
            writer.WriteString(ToID.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("from");
            writer.WriteString(FromID.ToString());
            writer.WriteEndElement();

            writer.WriteStartElement("meaning");
            writer.WriteString(MeaningID.ToString());
            writer.WriteEndElement();
        }

        #endregion

        #region Build locks

        /// <summary>Builds a lock for creating the link.</summary>
        /// <param name="list">The list.</param>
        /// <param name="args">The args.</param>
        private static void BuildLock(System.Collections.Generic.List<LockRequestInfo> list, CreateArgs args)
        {
            var iReq = LockRequestInfo.Create();
            iReq.Neuron = args.From;
            iReq.Level = LockLevel.LinksOut;
            iReq.Writeable = true;
            list.Add(iReq);

            if (args.To != null)
            {
                // to can be null for SetFirstOut
                iReq = LockRequestInfo.Create(args.To, LockLevel.LinksIn, true);
                list.Add(iReq);
            }

            iReq = LockRequestInfo.Create(args.Meaning, LockLevel.Value, true);

                // the meaning also  needs to be locked during the entire operation since we will increment it's counter.
            list.Add(iReq);
        }

        /// <summary>Builds a list of locks so the 'To' part can be changed.</summary>
        /// <param name="list">The list.</param>
        /// <param name="from1">The from1.</param>
        /// <param name="from2">The from2.</param>
        /// <param name="to1">The to1.</param>
        /// <param name="to2">The to2.</param>
        private void BuildLockForTo(System.Collections.Generic.List<LockRequestInfo> list, 
            Neuron from1, 
            Neuron from2, 
            Neuron to1, 
            Neuron to2)
        {
            LockRequestInfo iReq;
            if (from1 != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = from1;
                iReq.Level = LockLevel.LinksOut;
                list.Add(iReq);
            }

            if (from2 != null && from2 != from1)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = from2;
                iReq.Level = LockLevel.LinksOut;
                list.Add(iReq);
            }

            if (to1 != null)
            {
                iReq = LockRequestInfo.Create(to1, LockLevel.LinksIn, true);
                list.Add(iReq);
            }

            if (to2 != null && to2 != to1)
            {
                iReq = LockRequestInfo.Create(to2, LockLevel.LinksIn, true);
                list.Add(iReq);
            }
        }

        /// <summary>Builds a list of locks so the 'From' part can be changed.</summary>
        /// <param name="list">The list.</param>
        /// <param name="from1">The from1.</param>
        /// <param name="from2">The from2.</param>
        /// <param name="to1">The to1.</param>
        /// <param name="to2">The to2.</param>
        private void BuildLockForFrom(System.Collections.Generic.List<LockRequestInfo> list, 
            Neuron from1, 
            Neuron from2, 
            Neuron to1, 
            Neuron to2)
        {
            LockRequestInfo iReq;
            if (from1 != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = from1;
                iReq.Level = LockLevel.LinksOut;
                iReq.Writeable = true;
                list.Add(iReq);
            }

            if (from2 != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = from2;
                iReq.Level = LockLevel.LinksOut;
                iReq.Writeable = true;
                list.Add(iReq);
            }

            if (to1 != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = to1;
                iReq.Level = LockLevel.LinksIn;
                list.Add(iReq);
            }

            if (to2 != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = to2;
                iReq.Level = LockLevel.LinksIn;
                list.Add(iReq);
            }
        }

        /// <summary>Builds a lock for modifying the meaning.</summary>
        /// <param name="list">The list.</param>
        /// <param name="newVal">The new Val.</param>
        private void BuildMeaningLock(System.Collections.Generic.List<LockRequestInfo> list, Neuron newVal)
        {
            LockRequestInfo iReq;
            Neuron iItem;
            Brain.Current.TryFindNeuron(FromID, out iItem);

                // we use this way of retrieving the neuron, otherwise we get an exception if we have an invalid link.  This makes it safer.
            if (iItem != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = iItem;
                iReq.Level = LockLevel.LinksOut;
                iReq.Writeable = false;
                list.Add(iReq);
            }

            Brain.Current.TryFindNeuron(ToID, out iItem);

                // we use this way of retrieving the neuron, otherwise we get an exception if we have an invalid link.  This makes it safer.
            if (iItem != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = iItem;
                iReq.Level = LockLevel.LinksIn;
                iReq.Writeable = false;
                list.Add(iReq);
            }

            Brain.Current.TryFindNeuron(MeaningID, out iItem);
            if (iItem != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = iItem;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                list.Add(iReq);
            }

            if (newVal != null)
            {
                iReq = LockRequestInfo.Create();
                iReq.Neuron = newVal;
                iReq.Level = LockLevel.Value;
                iReq.Writeable = true;
                list.Add(iReq);
            }
        }

        /// <summary>Builds a lock on the meaning, from and to neurons + all info, for reading or writing.
        ///     This is required for deleting a lock for instance.</summary>
        /// <param name="writeable">if set to <c>true</c> [writeable].</param>
        /// <param name="infoList">The info List.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        private LockRequestList BuildCompleteLock(bool writeable, out System.Collections.Generic.List<Neuron> infoList)
        {
            var iDict = LockRequestsFactory.CreateInfoDict();

                // to prevent us from requesting the same 'value' lock multiple times, which could cause problems. This only needs to be done for the 'value' cause there are no other possible duplicates here.
            try
            {
                var iRes = LockRequestList.Create();

                    // originally stored the result in  the dict, but this doesn't work, in case of a duplicate in linkin/linkOut/value - which is allowed.
                LockRequestInfo iReq;
                LockRequestInfo iFound;
                Neuron iItem;
                Brain.Current.TryFindNeuron(FromID, out iItem);

                    // we use this way of retrieving the neuron, otherwise we get an exception if we have an invalid link.  This makes it safer.
                if (iItem != null)
                {
                    iReq = LockRequestInfo.Create();
                    iReq.Neuron = iItem;
                    iReq.Level = LockLevel.LinksOut;
                    iReq.Writeable = writeable;
                    iRes.Add(iReq);
                }

                Brain.Current.TryFindNeuron(ToID, out iItem);

                    // we use this way of retrieving the neuron, otherwise we get an exception if we have an invalid link.  This makes it safer.
                if (iItem != null)
                {
                    iReq = LockRequestInfo.Create();
                    iReq.Neuron = iItem;
                    iReq.Level = LockLevel.LinksIn;
                    iReq.Writeable = writeable;
                    iRes.Add(iReq);
                }

                Brain.Current.TryFindNeuron(MeaningID, out iItem);
                if (iItem != null)
                {
                    iReq = LockRequestInfo.Create();
                    iReq.Neuron = iItem;
                    iReq.Level = LockLevel.Value;
                    iReq.Writeable = writeable;
                    iRes.Add(iReq);
                    iDict.Add(iItem, iReq);
                }

                if (fInfo != null)
                {
                    // only try to access the Info list if there is one, otherwise we create when not required.
                    infoList = Info.ConvertTo<Neuron>();

                        // get the list of neurons, we need them later on to change the infoCount anyway.
                    foreach (var i in infoList)
                    {
                        if (iDict.TryGetValue(i, out iFound) == false
                            || (iFound.Level != LockLevel.Value && iFound.Level != LockLevel.All))
                        {
                            // don't need  no duplicate key registrations
                            iReq = LockRequestInfo.Create();
                            iReq.Neuron = i;
                            iReq.Level = LockLevel.Value;
                            iReq.Writeable = writeable;
                            iRes.Add(iReq);
                            iDict.Add(i, iReq);
                        }
                    }
                }
                else
                {
                    infoList = null;
                }

                return iRes;
            }
            finally
            {
                if (Processor.CurrentProcessor != null)
                {
                    Processor.CurrentProcessor.Mem.LocksFactory.Release(iDict);
                }
            }
        }

        #endregion
    }
}