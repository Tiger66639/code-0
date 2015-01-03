// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameCore.cs" company="">
//   
// </copyright>
// <summary>
//   Base class for frame data elements: provides access to the
//   <see cref="FrameNet" /> root object.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Framenet
{
    /// <summary>
    ///     Base class for frame data elements: provides access to the
    ///     <see cref="FrameNet" /> root object.
    /// </summary>
    public class FrameCore : Data.OwnedObject
    {
        #region Root

        /// <summary>
        ///     Gets the root object of this framenet data set.
        /// </summary>
        public FrameNet Root
        {
            get
            {
                Data.IOwnedObject iRoot = this;
                while (iRoot != null && !(iRoot.Owner is FrameNet))
                {
                    iRoot = iRoot.Owner as Data.IOwnedObject;
                }

                if (iRoot != null)
                {
                    return iRoot.Owner as FrameNet;
                }

                return null;
            }
        }

        #endregion
    }
}