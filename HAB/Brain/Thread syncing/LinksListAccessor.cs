// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LinksListAccessor.cs" company="">
//   
// </copyright>
// <summary>
//   provides thread save access to a list of links.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     provides thread save access to a list of links.
    /// </summary>
    public class LinksListAccessor : ListAccessor<Link>
    {
        /// <summary>Initializes a new instance of the <see cref="LinksListAccessor"/> class. 
        ///     Initializes a new instance of the <see cref="LinksListAccessor"/>
        ///     class.</summary>
        /// <param name="level">The level.</param>
        public LinksListAccessor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="LinksListAccessor"/> class.</summary>
        /// <param name="list">The list.</param>
        /// <param name="neuron">The neuron.</param>
        /// <param name="level">The level.</param>
        /// <param name="writeable">The writeable.</param>
        public LinksListAccessor(System.Collections.Generic.IList<Link> list, 
            Neuron neuron, 
            LockLevel level, 
            bool writeable)
            : base(list, neuron, level, writeable)
        {
        }

        /// <summary>Locks all.</summary>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> LockAll()
        {
            throw new System.NotSupportedException(
                "LInks accessors are read-only, they do not support locking all items at the same time.");
        }

        /// <summary>Locks the specified item.</summary>
        /// <param name="item">The item.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public override Neuron Lock(Link item)
        {
            throw new System.NotSupportedException(
                "LInks accessors are read-only, they do not support locking all items at the same time.");
        }

        /// <summary>Locks the specified a.</summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> Lock(Link a, Link b)
        {
            throw new System.NotSupportedException(
                "LInks accessors are read-only, they do not support locking all items at the same time.");
        }

        /// <summary>Locks the specified array.</summary>
        /// <param name="array">The array.</param>
        /// <returns>The <see cref="List"/>.</returns>
        public override System.Collections.Generic.List<Neuron> Lock(System.Collections.Generic.IEnumerable<Link> array)
        {
            throw new System.NotSupportedException(
                "LInks accessors are read-only, they do not support locking all items at the same time.");
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing,
        ///     or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            if (Level == LockLevel.LinksOut)
            {
                Factories.Default.LinksOutAccFactory.Release(this);
            }
            else
            {
                Factories.Default.LinksInAccFactory.Release(this);
            }
        }
    }
}