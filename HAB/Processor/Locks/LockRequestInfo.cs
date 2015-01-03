using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JaStDev.HAB
{
   /// <summary>
   /// A simple class to pass along multiple lock requests at once (for thread safety) to the 
   /// <see cref="LockManager"/>.
   /// </summary>
   public class LockRequestInfo
   {
      /// <summary>
      /// Gets or sets the neuron to lock.
      /// </summary>
      /// <value>The neuron.</value>
      public Neuron Neuron { get; set; }
      /// <summary>
      /// Gets or sets the level of the lock to request for the neuron.
      /// </summary>
      /// <value>The level.</value>
      public LockLevel Level { get; set; }
      /// <summary>
      /// Gets or sets a value indicating whether the lock should be writeable.
      /// </summary>
      /// <value><c>true</c> if writeable; otherwise, <c>false</c>.</value>
      public bool Writeable { get; set; }

      /// <summary>
      /// Gets the key that was created for the lock, which is returned by the <see cref="LockManager"/> and
      /// which is internally used to unlock the neuron again.
      /// </summary>
      /// <value>The key.</value>
      public object Key { get; internal set; }
   }
}
