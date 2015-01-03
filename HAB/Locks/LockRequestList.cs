// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LockRequestList.cs" company="">
//   
// </copyright>
// <summary>
//   a custom list used by the locking mechanisme.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     a custom list used by the locking mechanisme.
    /// </summary>
    public class LockRequestList : System.Collections.Generic.List<LockRequestInfo>
    {
        /// <summary>Initializes a new instance of the <see cref="LockRequestList"/> class. 
        ///     Prevents a default instance of the <see cref="LockRequestList"/>
        ///     class from being created.</summary>
        internal LockRequestList()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LockRequestList"/> class.</summary>
        /// <param name="items">The items.</param>
        public LockRequestList(System.Collections.Generic.IEnumerable<LockRequestInfo> items)
            : base(items)
        {
        }

        /// <summary>The create big.</summary>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public static LockRequestList CreateBig()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetBigList();
            }

            return new LockRequestList();
        }

        /// <summary>The create.</summary>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        public static LockRequestList Create()
        {
            if (Processor.CurrentProcessor != null)
            {
                return Processor.CurrentProcessor.Mem.LocksFactory.GetList();
            }

            return new LockRequestList();
        }

        /// <summary>duplicates the content of the specified list to a new one. When this
        ///     is done, we don't try to re-use existing objects, but always create
        ///     new ones. That's because this is always done during the construction
        ///     of a new processor, so it can't yet have any objects in reserve (and
        ///     therefor needs to create new ones anyway). + we don't know for which
        ///     proc it is (currentProcessor is not yet set).</summary>
        /// <param name="locks">The locks.</param>
        /// <returns>The <see cref="LockRequestList"/>.</returns>
        internal static LockRequestList Duplicate(LockRequestList locks)
        {
            var iList = new LockRequestList();
            foreach (var i in locks)
            {
                var iNew = new LockRequestInfo { Level = i.Level, Neuron = i.Neuron, Writeable = i.Writeable };
                iList.Add(iNew);
            }

            return iList;
        }
    }
}