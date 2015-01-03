// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachedTopicsCollection.cs" company="">
//   
// </copyright>
// <summary>
//   Defines the AttachedTopicsCollection type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    /// </summary>
    public class AttachedTopicsCollection : LargeIDCollection
    {
        /// <summary>adds the <paramref name="item"/></summary>
        /// <param name="item">The item.</param>
        protected override void InternalInsert(ulong item)
        {
            var iTitle = BrainData.Current.NeuronInfo.GetDisplayTitleFor(item);
            if (string.IsNullOrEmpty(iTitle) == false)
            {
                Parsers.TopicsDictionary.Add(iTitle, item);
            }

            base.InternalInsert(item);
        }

        /// <summary>The internal remove.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        protected override bool InternalRemove(ulong item)
        {
            var iTitle = BrainData.Current.NeuronInfo.GetDisplayTitleFor(item);
            if (string.IsNullOrEmpty(iTitle) == false)
            {
                Parsers.TopicsDictionary.Remove(iTitle, item);
            }

            return base.InternalRemove(item);
        }

        /// <summary>
        ///     Clears this instance.
        /// </summary>
        public override void Clear()
        {
            foreach (var i in this)
            {
                var iTitle = BrainData.Current.NeuronInfo.GetDisplayTitleFor(i);
                if (string.IsNullOrEmpty(iTitle) == false)
                {
                    Parsers.TopicsDictionary.Remove(iTitle, i);
                }
            }

            base.Clear();
        }
    }
}