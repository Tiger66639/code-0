// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefCountedList.cs" company="">
//   
// </copyright>
// <summary>
//   provides a way to share the list accross multile threads and recycle it
//   when all are done.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>provides a way to share the list accross multile threads and recycle it
    ///     when all are done.</summary>
    /// <typeparam name="T"></typeparam>
    public class RefCountedList<T>
    {
        /// <summary>The ref count.</summary>
        public int RefCount;

        /// <summary>The source.</summary>
        public System.Collections.Generic.List<T> Source;
    }
}