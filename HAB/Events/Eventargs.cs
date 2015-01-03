// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Eventargs.cs" company="">
//   
// </copyright>
// <summary>
//   A generic event arg type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JaStDev.HAB
{
    /// <summary>A generic event arg type.</summary>
    /// <typeparam name="T"></typeparam>
    public class Eventargs<T> : System.EventArgs
    {
        /// <summary>
        ///     the value for the event.
        /// </summary>
        public T Value { get; set; }
    }
}