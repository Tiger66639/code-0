// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeItemGlobal.cs" company="">
//   
// </copyright>
// <summary>
//   Wraps a global expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Designer
{
    /// <summary>
    ///     Wraps a global expression.
    /// </summary>
    public class CodeItemGlobal : CodeItemVariable
    {
        #region ctor

        /// <summary>Initializes a new instance of the <see cref="CodeItemGlobal"/> class.</summary>
        /// <param name="toWrap">To wrap.</param>
        /// <param name="isActive">The is Active.</param>
        public CodeItemGlobal(Global toWrap, bool isActive)
            : base(toWrap, isActive)
        {
        }

        #endregion

        #region Overrides

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
            iNew.Element = new CtrlGlobal();
            iNew.Data = this;
            return iNew;
        }

        #endregion
    }
}