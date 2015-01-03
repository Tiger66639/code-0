// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLReferenceNode.cs" company="">
//   
// </copyright>
// <summary>
//   a node that references another node. This is used for statics,
//   parameters,...
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     a node that references another node. This is used for statics,
    ///     parameters,...
    /// </summary>
    internal class NNLReferenceNode : NNLStatementNode
    {
        /// <summary>The f param values.</summary>
        private NNLStatementNode fParamValues;

        /// <summary>Initializes a new instance of the <see cref="NNLReferenceNode"/> class.</summary>
        public NNLReferenceNode()
            : base(NodeType.Path)
        {
            IsFunctionCall = false;
        }

        #region Reference

        /// <summary>
        ///     Gets/sets the name of the node being referenced.
        /// </summary>
        public string Reference { get; set; }

        #endregion

        /// <summary>Gets or sets a value indicating whether is function call.</summary>
        public bool IsFunctionCall { get; set; }

        /// <summary>
        ///     when true, the function call needs to be rendered with a 'CallBlocked'
        ///     instruction.
        /// </summary>
        public bool IsWaitFor { get; set; }

        #region ParamValues

        /// <summary>
        ///     Gets the possible parameter values. This can be a listnode, which
        ///     represents a comma seperated list.
        /// </summary>
        public NNLStatementNode ParamValues
        {
            get
            {
                return fParamValues;
            }

            set
            {
                fParamValues = value;
                if (value != null)
                {
                    value.Parent = this;
                }
            }
        }

        #endregion

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (fParamValues != null)
            {
                return string.Format("{0}({1})", Reference, fParamValues);
            }

            return Reference;
        }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo">The render to.</param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            LogPosError("Invalid reference to: " + Reference, renderTo);
        }
    }
}