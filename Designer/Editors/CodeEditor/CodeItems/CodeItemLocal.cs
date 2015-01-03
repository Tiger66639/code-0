// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemLocal.cs" company="">
//   
// </copyright>
// <summary>
//   a wrapper for local variables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     a wrapper for local variables.
    /// </summary>
    public class CodeItemLocal : CodeItemVariable
    {
        /// <summary>Initializes a new instance of the <see cref="CodeItemLocal"/> class.</summary>
        /// <param name="toWrap">The to wrap.</param>
        /// <param name="isActive">The is active.</param>
        public CodeItemLocal(Local toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        /// <summary>Inheriters should <see langword="override"/> this function to return a
        ///     ui element that should be used to represent it in a<see cref="WPF.Controls.CodePagePanel"/> object.</summary>
        /// <param name="owner"></param>
        /// <param name="panel"></param>
        /// <returns>The <see cref="CPPItemBase"/>.</returns>
        protected internal override WPF.Controls.CPPItemBase CreateDefaultUI(
            WPF.Controls.CPPItemBase owner, 
            WPF.Controls.CodePagePanel panel)
        {
            var iNew = new WPF.Controls.CPPItem(owner, panel);
            iNew.Element = new CtrlLocal();
            iNew.Data = this;
            return iNew;
        }
    }
}