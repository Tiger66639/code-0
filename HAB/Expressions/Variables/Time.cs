// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Time.cs" company="">
//   
// </copyright>
// <summary>
//   This variables returns the current system time as a neuroncluster, where
//   year, month, day, hour, min, sec are preseneted as <see langword="int" />
//   neurons.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    using Enumerable = System.Linq.Enumerable;

    /// <summary>
    ///     This variables returns the current system time as a neuroncluster, where
    ///     year, month, day, hour, min, sec are preseneted as <see langword="int" />
    ///     neurons.
    /// </summary>
    [NeuronID((ulong)PredefinedNeurons.Time)]
    [NeuronID((ulong)PredefinedNeurons.TimeSpan, typeof(Neuron))]
    public class Time : SystemVariable
    {
        /// <summary>The f current.</summary>
        private static volatile Time fCurrent;

        /// <summary>The f days.</summary>
        private System.Collections.Generic.IList<ulong> fDays;

        /// <summary>The f days lock.</summary>
        private volatile ListLock fDaysLock = new ListLock();

        /// <summary>The f days of week.</summary>
        private System.Collections.Generic.IList<ulong> fDaysOfWeek;

        /// <summary>The f days of week lock.</summary>
        private volatile ListLock fDaysOfWeekLock = new ListLock();

        /// <summary>The f hours.</summary>
        private System.Collections.Generic.IList<ulong> fHours;

        /// <summary>The f hours lock.</summary>
        private volatile ListLock fHoursLock = new ListLock();

        /// <summary>The f minutes.</summary>
        private System.Collections.Generic.IList<ulong> fMinutes;

        /// <summary>The f minutes lock.</summary>
        private volatile ListLock fMinutesLock = new ListLock();

        /// <summary>The f months.</summary>
        private System.Collections.Generic.IList<ulong> fMonths;

        /// <summary>The f months lock.</summary>
        private volatile ListLock fMonthsLock = new ListLock();

        /// <summary>The f seconds.</summary>
        private System.Collections.Generic.List<ulong> fSeconds;

        /// <summary>The f seconds lock.</summary>
        private volatile ListLock fSecondsLock = new ListLock();

                                  // we use Nullable<bool> so we can keep track of the fact if the data needs to be flushed or not.

        /// <summary>The f years.</summary>
        private System.Collections.Generic.Dictionary<int, ulong> fYears;

        /// <summary>The f years lock.</summary>
        private volatile ListLock fYearsLock = new ListLock();

        /// <summary>
        ///     Initializes a new instance of the <see cref="Time" /> class.
        /// </summary>
        internal Time()
        {
        }

        /// <summary>
        ///     Gets the Time object in the current network.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public static Time Current
        {
            get
            {
                if (fCurrent == null)
                {
                    fCurrent = Brain.Current[(ulong)PredefinedNeurons.Time] as Time;
                }

                return fCurrent;
            }
        }

        /// <summary>
        ///     gets wether <see cref="JaStDev.HAB.Time.Current" /> is loaded or not.
        ///     This is used during the flush, to make certain that we don't need to
        ///     load unneeded data.
        /// </summary>
        public static bool CurrentLoaded
        {
            get
            {
                return fCurrent != null;
            }
        }

        #region Seconds

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent seconds.
        /// </summary>
        public System.Collections.Generic.IList<ulong> Seconds
        {
            get
            {
                lock (fSecondsLock)
                {
                    if (fSeconds == null)
                    {
                        fSeconds =
                            Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                                typeof(Time), 
                                "Seconds");
                        if (fSeconds == null || fSeconds.Count < 60)
                        {
                            // in case something is wrong with the values?
                            fSeconds = new System.Collections.Generic.List<ulong>();
                            for (var i = 0; i < 60; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fSeconds.Add(iInt.ID);
                            }

                            fSecondsLock.NeedsFlush = true;
                        }
                        else
                        {
                            foreach (var i in fSeconds)
                            {
                                var iItem = Brain.Current[i];
                                if (iItem.InfoUsageCount == 0)
                                {
                                    iItem.IncInfoUsageCount(); // just a temp solution, so everything is back ok.
                                }
                            }
                        }
                    }

                    return fSeconds;
                }
            }
        }

        #endregion

        #region Minutes

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent minutes.
        /// </summary>
        public System.Collections.Generic.IList<ulong> Minutes
        {
            get
            {
                lock (fMinutesLock)
                {
                    if (fMinutes == null)
                    {
                        fMinutes =
                            Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                                typeof(Time), 
                                "Minutes");
                        if (fMinutes == null)
                        {
                            fMinutes = new System.Collections.Generic.List<ulong>();
                            for (var i = 0; i < 60; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fMinutes.Add(iInt.ID);
                            }

                            fMinutesLock.NeedsFlush = true;
                        }
                    }

                    return fMinutes;
                }
            }
        }

        #endregion

        #region Hours

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent hours
        /// </summary>
        public System.Collections.Generic.IList<ulong> Hours
        {
            get
            {
                lock (fHoursLock)
                {
                    if (fHours == null)
                    {
                        fHours = Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                            typeof(Time), 
                            "Hours");
                        if (fHours == null)
                        {
                            fHours = new System.Collections.Generic.List<ulong>();
                            for (var i = 0; i <= 24; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fHours.Add(iInt.ID);
                            }

                            fHoursLock.NeedsFlush = true;
                        }
                    }

                    return fHours;
                }
            }
        }

        #endregion

        #region Days

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent hours
        /// </summary>
        public System.Collections.Generic.IList<ulong> Days
        {
            get
            {
                lock (fDaysLock)
                {
                    if (fDays == null)
                    {
                        fDays = Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                            typeof(Time), 
                            "Days");
                        if (fDays == null)
                        {
                            fDays = new System.Collections.Generic.List<ulong>();
                            for (var i = 1; i <= 31; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fDays.Add(iInt.ID);
                            }
                        }

                        fDaysLock.NeedsFlush = true;
                    }

                    return fDays;
                }
            }
        }

        #endregion

        #region DaysOfWeek

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent hours
        /// </summary>
        public System.Collections.Generic.IList<ulong> DaysOfWeek
        {
            get
            {
                lock (fDaysOfWeekLock)
                {
                    if (fDaysOfWeek == null)
                    {
                        fDaysOfWeek =
                            Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                                typeof(Time), 
                                "DaysOfWeek");
                        if (fDaysOfWeek == null)
                        {
                            fDaysOfWeek = new System.Collections.Generic.List<ulong>();
                            for (var i = 1; i <= 7; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fDaysOfWeek.Add(iInt.ID);
                            }
                        }

                        fDaysOfWeekLock.NeedsFlush = true;
                    }

                    return fDaysOfWeek;
                }
            }
        }

        #endregion

        #region Months

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent hours
        /// </summary>
        public System.Collections.Generic.IList<ulong> Months
        {
            get
            {
                lock (fMonthsLock)
                {
                    if (fMonths == null)
                    {
                        fMonths = Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                            typeof(Time), 
                            "Months");
                        if (fMonths == null)
                        {
                            fMonths = new System.Collections.Generic.List<ulong>();
                            for (var i = 1; i <= 12; i++)
                            {
                                var iInt = NeuronFactory.GetInt(i);
                                Brain.Current.Add(iInt);
                                iInt.IncInfoUsageCount();

                                    // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                                fMonths.Add(iInt.ID);
                            }

                            fMonthsLock.NeedsFlush = true;
                        }
                    }

                    return fMonths;
                }
            }
        }

        #endregion

        #region Years

        /// <summary>
        ///     Gets the list with id's of <see langword="int" /> neurons that
        ///     represent hours
        /// </summary>
        public System.Collections.Generic.Dictionary<int, ulong> Years
        {
            get
            {
                lock (fYearsLock)
                {
                    if (fYears == null)
                    {
                        System.Collections.Generic.IList<ulong> iYears =
                            Brain.Current.Storage.GetProperty<System.Collections.Generic.List<ulong>>(
                                typeof(Time), 
                                "Years");
                        fYears = new System.Collections.Generic.Dictionary<int, ulong>();
                        if (iYears != null)
                        {
                            for (var i = 0; i < iYears.Count; i++)
                            {
                                fYears.Add(i, iYears[i]);
                            }
                        }
                    }

                    return fYears;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Gets the type of this neuron expressed as a Neuron.
        /// </summary>
        /// <value>
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Variable" /> .
        /// </value>
        public override ulong TypeOfNeuronID
        {
            get
            {
                return (ulong)PredefinedNeurons.Time;
            }
        }

        /// <summary>Gets the value.</summary>
        /// <param name="proc">The proc.</param>
        internal override void GetValue(Processor proc)
        {
            proc.Mem.ArgumentStack.Peek().Add(GetTimeCluster(System.DateTime.Now, proc));
        }

        /// <summary>gets the result of the variable without putting anything on the stack
        ///     but by simply returning the value.</summary>
        /// <param name="proc"></param>
        /// <returns>The <see cref="List"/>.</returns>
        protected internal override System.Collections.Generic.List<Neuron> ExtractValue(Processor proc)
        {
            var iRes = Factories.Default.NLists.GetBuffer();
            iRes.Add(GetTimeCluster(System.DateTime.Now, proc));
            return iRes;
        }

        /// <summary>The get second.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetSecond(System.DateTime time)
        {
            var iEntries = Seconds;
            return Brain.Current[iEntries[time.Second]]; // seconds can be 0, so don't do -1
        }

        /// <summary>The get minute.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetMinute(System.DateTime time)
        {
            var iEntries = Minutes;
            return Brain.Current[iEntries[time.Minute]]; // minutes can be 0, so don't do -1
        }

        /// <summary>The get hour.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetHour(System.DateTime time)
        {
            var iEntries = Hours;
            return Brain.Current[iEntries[time.Hour]]; // hours can be 0, so don't do -1
        }

        /// <summary>The get day.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetDay(System.DateTime time)
        {
            var iEntries = Days;
            return Brain.Current[iEntries[time.Day - 1]];
        }

        /// <summary>The get day of week.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetDayOfWeek(System.DateTime time)
        {
            var iEnties = DaysOfWeek;
            return Brain.Current[iEnties[(int)time.DayOfWeek]]; // day of week is 0 based: sunday = 0
        }

        /// <summary>The get month.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetMonth(System.DateTime time)
        {
            var iEntries = Months;
            return Brain.Current[iEntries[time.Month - 1]];
        }

        /// <summary>The get year.</summary>
        /// <param name="time">The time.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        private Neuron GetYear(System.DateTime time)
        {
            var iEntries = Years;
            ulong iFound;
            lock (fYearsLock)
            {
                if (iEntries.TryGetValue(time.Year, out iFound) == false)
                {
                    var iNew = NeuronFactory.GetInt(time.Year);
                    Brain.Current.Add(iNew);
                    iNew.IncInfoUsageCount();

                        // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                    fYears.Add(time.Year, iNew.ID);
                    fYearsLock.NeedsFlush = true;
                    return iNew;
                }

                return Brain.Current[iFound];
            }
        }

        /// <summary>Gets the year as an</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="ulong"/>.</returns>
        internal ulong GetYear(IntNeuron value)
        {
            var iEntries = Years;
            ulong iFound;
            lock (fYearsLock)
            {
                if (iEntries.TryGetValue(value.Value, out iFound) == false)
                {
                    var iNew = NeuronFactory.GetInt(value.Value);

                        // always create a new neuron for the year, so that these are exclusive for date stuff.
                    Brain.Current.Add(iNew);
                    iNew.IncInfoUsageCount();

                        // we increment the usage count of the year so that it can never be deleted accidentally as long as the value is used by the time var.
                    fYears.Add(value.Value, iNew.ID);
                    fYearsLock.NeedsFlush = true;
                    return iNew.ID;
                }

                return iFound;
            }
        }

        /// <summary>
        ///     Returns a <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents the current
        ///     <see cref="object" /> .
        /// </returns>
        public override string ToString()
        {
            return "Time";
        }

        /// <summary>
        ///     Called when the network is cleared.
        /// </summary>
        public static void Reset()
        {
            if (fCurrent != null)
            {
                fCurrent.fYears = null;
                fCurrent.fMonths = null;
                fCurrent.fDays = null;
                fCurrent.fMinutes = null;
                fCurrent.fHours = null;
                fCurrent.fSeconds = null;
                fCurrent.fDaysOfWeek = null;
                fCurrent = null;
            }
        }

        /// <summary>Converts the specified time cluster to a DateTime object.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <returns>The <see cref="DateTime?"/>.</returns>
        public static System.DateTime? GetTime(NeuronCluster toConvert)
        {
            if (toConvert != null && toConvert.Meaning == (ulong)PredefinedNeurons.Time)
            {
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = toConvert.Children) iChildren = iList.ConvertTo<Neuron>();
                try
                {
                    IntNeuron iYear, iMonth, iDay, iHour, iMinute, iSecond;
                    var iCount = iChildren.Count;
                    if (iCount > 0)
                    {
                        iYear = iChildren[0] as IntNeuron;
                        if (iCount > 1)
                        {
                            iMonth = iChildren[1] as IntNeuron;
                            if (iCount > 2)
                            {
                                iDay = iChildren[2] as IntNeuron;
                                if (iCount > 3)
                                {
                                    iHour = iChildren[3] as IntNeuron;
                                    if (iCount > 4)
                                    {
                                        iMinute = iChildren[4] as IntNeuron;
                                        if (iCount > 5)
                                        {
                                            iSecond = iChildren[5] as IntNeuron;
                                            if (iYear != null && iMonth != null && iDay != null && iHour != null
                                                && iMinute != null && iSecond != null)
                                            {
                                                return new System.DateTime(
                                                    iYear.Value, 
                                                    iMonth.Value, 
                                                    iDay.Value, 
                                                    iHour.Value, 
                                                    iMinute.Value, 
                                                    iSecond.Value);
                                            }
                                            else
                                            {
                                                return null;
                                            }
                                        }
                                        else if (iYear != null && iMonth != null && iDay != null && iHour != null
                                                 && iMinute != null)
                                        {
                                            return new System.DateTime(
                                                iYear.Value, 
                                                iMonth.Value, 
                                                iDay.Value, 
                                                iHour.Value, 
                                                iMinute.Value, 
                                                0);
                                        }
                                        else
                                        {
                                            return null;
                                        }
                                    }
                                    else if (iYear != null && iMonth != null && iDay != null && iHour != null)
                                    {
                                        return new System.DateTime(
                                            iYear.Value, 
                                            iMonth.Value, 
                                            iDay.Value, 
                                            iHour.Value, 
                                            0, 
                                            0);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                else if (iYear != null && iMonth != null && iDay != null)
                                {
                                    return new System.DateTime(iYear.Value, iMonth.Value, iDay.Value);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iChildren);
                }
            }

            return null;
        }

        /// <summary>Converts the specified time cluster to a DateTime object.</summary>
        /// <param name="toConvert">To convert.</param>
        /// <returns>The <see cref="TimeSpan?"/>.</returns>
        public static System.TimeSpan? GetTimeSpan(NeuronCluster toConvert)
        {
            if (toConvert != null && toConvert.Meaning == (ulong)PredefinedNeurons.TimeSpan)
            {
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = toConvert.Children) iChildren = iList.ConvertTo<Neuron>();
                try
                {
                    var iDay = iChildren[0] as IntNeuron;
                    var iHour = iChildren[1] as IntNeuron;
                    var iMinute = iChildren[2] as IntNeuron;
                    var iSecond = iChildren[3] as IntNeuron;
                    if (iDay != null && iHour != null && iMinute != null && iSecond != null)
                    {
                        return new System.TimeSpan(iDay.Value, iHour.Value, iMinute.Value, iSecond.Value);
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iChildren);
                }
            }

            return null;
        }

        /// <summary>Converts the specified dateTime to a time cluster. Automatically
        ///     freezes the result value.</summary>
        /// <param name="val">The val.</param>
        /// <param name="proc">The proc.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetTimeCluster(System.DateTime val, Processor proc)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.Time;
            var iItems = Factories.Default.NLists.GetBuffer();
            try
            {
                iItems.Add(GetYear(val));
                iItems.Add(GetMonth(val));
                iItems.Add(GetDay(val));
                iItems.Add(GetHour(val));
                iItems.Add(GetMinute(val));
                iItems.Add(GetSecond(val));
                iItems.Add(GetDayOfWeek(val));
                using (var iList = iCluster.ChildrenW) iList.AddRange(iItems);
            }
            finally
            {
                Factories.Default.NLists.Recycle(iItems);
            }

            iCluster.SetIsFrozen(true, proc); // we freeze, so that, if not used, it gets deleted.
            return iCluster;
        }

        /// <summary>Converts the specified dateTime to a time cluster. Doesn't
        ///     automatically freeze the resulting cluster.</summary>
        /// <param name="val">The val.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetTimeCluster(System.DateTime val)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.Time;
            FillTimeCluster(val, iCluster);
            return iCluster;
        }

        /// <summary>Converts the specified dateTime to a time cluster. Doesn't
        ///     automatically freeze the resulting cluster. The cluster only contains
        ///     date info (year, month, day) time info is dropped. This is used for
        ///     times when there is no time info availble (and we would get lots of
        ///     0).</summary>
        /// <param name="val"></param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetShortTimeCluster(System.DateTime val)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.Time;
            var iValues = Factories.Default.NLists.GetBuffer();
            try
            {
                iValues.Add(GetYear(val));
                iValues.Add(GetMonth(val));
                iValues.Add(GetDay(val));
                using (var iList = iCluster.ChildrenW) iList.AddRange(iValues);
            }
            finally
            {
                Factories.Default.NLists.Recycle(iValues);
            }

            return iCluster;
        }

        /// <summary>Fills the cluster with the neurons that represent the specified time.</summary>
        /// <param name="val">The val.</param>
        /// <param name="iCluster">The i Cluster.</param>
        public void FillTimeCluster(System.DateTime val, NeuronCluster iCluster)
        {
            var iItems = Factories.Default.NLists.GetBuffer();
            try
            {
                iItems.Add(GetYear(val));
                iItems.Add(GetMonth(val));
                iItems.Add(GetDay(val));
                iItems.Add(GetHour(val));
                iItems.Add(GetMinute(val));
                iItems.Add(GetSecond(val));
                iItems.Add(GetDayOfWeek(val));
                using (var iList = iCluster.ChildrenW) iList.AddRange(iItems);
            }
            finally
            {
                Factories.Default.NLists.Recycle(iItems);
            }
        }

        /// <summary>Checks if the cluster isn't used anymore and then releases it.</summary>
        /// <param name="val">The val.</param>
        public void ReleaseTimeCluster(NeuronCluster val)
        {
            if (val.CanBeDeleted)
            {
                LockManager.Current.RequestLock(val, LockLevel.All, false);
                try
                {
                    if (val.LinksInIdentifier != null)
                    {
                        if (val.LinksInIdentifier.Count > 0)
                        {
                            return;
                        }
                    }

                    if (val.LinksOutIdentifier != null)
                    {
                        if (val.LinksOutIdentifier.Count > 0)
                        {
                            return;
                        }
                    }

                    if (val.ClusteredByIdentifier != null)
                    {
                        if (val.ClusteredByDirect.Count > 0)
                        {
                            return;
                        }
                    }
                }
                finally
                {
                    LockManager.Current.ReleaseLock(val, LockLevel.All, false);
                }

                DeleteTimeCluster(val);
            }
        }

        /// <summary>Deletes the time cluster without checking if it is still used or not.
        ///     Makes certain that Year values are removed if they are no longer used.</summary>
        /// <param name="val">The val.</param>
        public void DeleteTimeCluster(NeuronCluster val)
        {
            ulong iYearId;

            using (var iList = val.Children) iYearId = iList[0];
            var iYear = Brain.Current[iYearId] as IntNeuron;

            if (Brain.Current.Delete(val))
            {
                if (iYear.InfoUsageCount == 1 && iYear.MeaningUsageCount == 0)
                {
                    if (iYear.LinksInIdentifier != null)
                    {
                        using (var iList = iYear.LinksIn)
                            if (iList.Count > 0)
                            {
                                return;
                            }
                    }

                    if (iYear.LinksOutIdentifier != null)
                    {
                        using (var iList = iYear.LinksOut)
                            if (iList.Count > 0)
                            {
                                return;
                            }
                    }

                    if (iYear.ClusteredByIdentifier != null)
                    {
                        using (var iList = iYear.ClusteredBy)
                            if (iList.Count > 0)
                            {
                                return;
                            }
                    }

                    var iEntries = Years;
                    lock (fYearsLock) iEntries.Remove(iYear.Value);
                    iYear.InfoUsageCount = 0; // need to set to 0, othewise we can't delete.
                    Brain.Current.Delete(iYear);
                }
            }
        }

        /// <summary>Converts the specified dateTime to a time cluster.</summary>
        /// <param name="iVal">The i val.</param>
        /// <param name="proc">The processor. This is required cause we need to freeze the objects.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetTimeSpanCluster(System.TimeSpan iVal, Processor proc)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.TimeSpan;

            var iItems = Factories.Default.NLists.GetBuffer();
            try
            {
                var iNew = NeuronFactory.GetInt(iVal.Days);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Hours);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Minutes);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Seconds);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);
                using (var iList = iCluster.ChildrenW) iList.AddRange(iItems);
                iItems.ForEach(i => i.SetIsFrozen(true, proc));
            }
            finally
            {
                Factories.Default.NLists.Recycle(iItems);
            }

            iCluster.SetIsFrozen(true, proc); // we freeze, so that, if not used, it gets deleted.
            return iCluster;
        }

        /// <summary>The get time span cluster.</summary>
        /// <param name="iVal">The i val.</param>
        /// <returns>The <see cref="NeuronCluster"/>.</returns>
        public NeuronCluster GetTimeSpanCluster(System.TimeSpan iVal)
        {
            var iCluster = NeuronFactory.GetCluster();
            Brain.Current.Add(iCluster);
            iCluster.Meaning = (ulong)PredefinedNeurons.TimeSpan;

            var iItems = Factories.Default.NLists.GetBuffer();
            try
            {
                var iNew = NeuronFactory.GetInt(iVal.Days);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Hours);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Minutes);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);

                iNew = NeuronFactory.GetInt(iVal.Seconds);
                Brain.Current.Add(iNew);
                iItems.Add(iNew);
                using (var iList = iCluster.ChildrenW) iList.AddRange(iItems);
            }
            finally
            {
                Factories.Default.NLists.Recycle(iItems);
            }

            return iCluster;
        }

        /// <summary>
        ///     Flushes all the data used by this instance to the database.
        /// </summary>
        public void Flush()
        {
            if (fYearsLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Years", Enumerable.ToList(fYears.Values));
            }

            if (fMonthsLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Months", fMonths);
            }

            if (fHoursLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Hours", fHours);
            }

            if (fSecondsLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Seconds", fSeconds);
            }

            if (fMinutesLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Minutes", fMinutes);
            }

            if (fDaysLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "Days", fDays);
            }

            if (fDaysOfWeekLock.NeedsFlush)
            {
                Brain.Current.Storage.SaveProperty(typeof(Time), "DaysOfWeek", fDaysOfWeek);
            }
        }

        /// <summary>
        ///     Touches the memory so that everything is loaded into ram. This is used
        ///     for creating new projects from a template.
        /// </summary>
        /// <remarks>
        ///     We need to laod the time lists (entry points).
        /// </remarks>
        internal static void TouchMem()
        {
            var iDummy = Current.Years.Count;
            Current.fYearsLock.NeedsFlush = true;
            iDummy = Current.Months.Count;
            Current.fMonthsLock.NeedsFlush = true;
            iDummy = Current.Days.Count;
            Current.fDaysLock.NeedsFlush = true;

            iDummy = Current.DaysOfWeek.Count;
            Current.fDaysOfWeekLock.NeedsFlush = true;

            iDummy = Current.Hours.Count;
            Current.fHoursLock.NeedsFlush = true;
            iDummy = Current.Minutes.Count;
            Current.fMinutesLock.NeedsFlush = true;
            iDummy = Current.Seconds.Count;
            Current.fSecondsLock.NeedsFlush = true;
        }

        /// <summary>
        ///     Provides a lock + flush info for the lists.
        /// </summary>
        private class ListLock
        {
            #region NeedsFlush

            /// <summary>
            ///     Gets or sets a value indicating whether the data for this lock
            ///     needs to be flushed.
            /// </summary>
            public bool NeedsFlush { get; set; }

            #endregion
        }
    }
}