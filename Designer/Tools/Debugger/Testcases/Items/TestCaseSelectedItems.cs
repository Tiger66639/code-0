// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCaseSelectedItems.cs" company="">
//   
// </copyright>
// <summary>
//   Manages all the selected items on a testcase.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer.Test
{
    /// <summary>
    ///     Manages all the selected items on a testcase.
    /// </summary>
    public class TestCaseSelectedItems : System.Collections.ObjectModel.ObservableCollection<TestCaseItem>
    {
        /// <summary>The f owner.</summary>
        private readonly TestCase fOwner;

        /// <summary>Initializes a new instance of the <see cref="TestCaseSelectedItems"/> class.</summary>
        /// <param name="owner">The owner.</param>
        public TestCaseSelectedItems(TestCase owner)
        {
            System.Diagnostics.Debug.Assert(owner != null);
            fOwner = owner;
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (var i in this)
            {
                i.SetSelected(false);
            }

            base.ClearItems();
            fOwner.SelectedItem = null;
        }

        /// <summary>The insert item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void InsertItem(int index, TestCaseItem item)
        {
            item.SetSelected(true);
            base.InsertItem(index, item);
        }

        /// <summary>The remove item.</summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            var iItem = this[index];
            if (fOwner.SelectedItem == iItem)
            {
                if (index > 0)
                {
                    fOwner.SetSelectedItem(this[index - 1]);
                }
                else if (index < Count)
                {
                    fOwner.SetSelectedItem(this[index + 1]);
                }
                else
                {
                    fOwner.SetSelectedItem(null);
                }
            }

            iItem.SetSelected(false);
            base.RemoveItem(index);
        }

        /// <summary>The set item.</summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        protected override void SetItem(int index, TestCaseItem item)
        {
            fOwner.SetSelectedItem(item);
            item.SetSelected(true);
            base.SetItem(index, item);
        }

        /// <summary>
        ///     a Clear without reset of the TestCase.
        /// </summary>
        internal void ClearInternal()
        {
            if (Count > 0)
            {
                foreach (var i in this)
                {
                    i.SetSelected(false);
                }

                base.ClearItems();
            }
        }
    }
}