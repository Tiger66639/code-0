// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NNLNeuronNode.cs" company="">
//   
// </copyright>
// <summary>
//   The different types of neurons that can be declared as objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     The different types of neurons that can be declared as objects.
    /// </summary>
    internal enum NeuronType
    {
        /// <summary>The neuron.</summary>
        Neuron, 

        /// <summary>The statement.</summary>
        Statement, 

        /// <summary>The result statement.</summary>
        ResultStatement, 

        /// <summary>The assignment.</summary>
        Assignment, 

        /// <summary>The bool expression.</summary>
        BoolExpression, 

        /// <summary>The conditional expression.</summary>
        ConditionalExpression, 

        /// <summary>The conditional statement.</summary>
        ConditionalStatement, 

        /// <summary>The lock.</summary>
        Lock, 

        /// <summary>The by ref.</summary>
        ByRef, 

        /// <summary>The int.</summary>
        Int, 

        /// <summary>The double.</summary>
        Double, 

        /// <summary>The string.</summary>
        String, 

        /// <summary>The variable.</summary>
        Variable, 

        /// <summary>The global.</summary>
        Global, 

        /// <summary>The text sin.</summary>
        TextSin, 

        /// <summary>The audio sin.</summary>
        AudioSin, 

        /// <summary>The int sin.</summary>
        IntSin, 

        /// <summary>The image sin.</summary>
        ImageSin, 

        /// <summary>The reflection sin.</summary>
        ReflectionSin, 

        /// <summary>The timer sin.</summary>
        TimerSin, 

        /// <summary>The query.</summary>
        Query
    }

    /// <summary>The nnl neuron node.</summary>
    internal class NNLNeuronNode : NNLNode
    {
        /// <summary>The f value.</summary>
        private object fValue;

        /// <summary>Initializes a new instance of the <see cref="NNLNeuronNode"/> class.</summary>
        /// <param name="type">The type.</param>
        public NNLNeuronNode(NeuronType type)
            : base(NodeType.Neuron)
        {
            InDict = false;
            Neurontype = type;
        }

        /// <summary>Initializes a new instance of the <see cref="NNLNeuronNode"/> class. 
        ///     ctor for reading from obj files.</summary>
        internal NNLNeuronNode()
            : base(NodeType.Neuron)
        {
            InDict = false;
        }

        #region Neurontype

        /// <summary>
        ///     Gets/sets the type of the neuron that should be created.
        /// </summary>
        public NeuronType Neurontype { get; set; }

        #endregion

        /// <summary>
        ///     when a var or global are declared, they can also have a splitreaction.
        ///     needs to be stored and rendered.
        /// </summary>
        public ulong SplitReaction { get; set; }

        /// <summary>renders this node to the specified module compiler.</summary>
        /// <param name="renderTo"></param>
        internal override void Render(NNLModuleCompiler renderTo)
        {
            if (Item == null)
            {
                var iRes = FindObject(renderTo);
                if (iRes == null)
                {
                    iRes = CreateObject();
                    Brain.Current.Add(iRes);
                    Item = iRes;
                    if (string.IsNullOrEmpty(Name) == false)
                    {
                        NNLModuleCompiler.NetworkDict.SetName(iRes, Name);
                    }

                    if (Item is Variable)
                    {
                        if (SplitReaction != 0)
                        {
                            ((Variable)Item).SplitReaction = Brain.Current[SplitReaction];
                        }

                        if (Value != null)
                        {
                            var iStat = (NNLStatementNode)Value;
                            iStat.Render(renderTo);
                            ((Variable)Item).Value = iStat.Item;
                        }
                    }
                }

                if (ID == Neuron.EmptyId)
                {
                    // don't try to add the neuron to the module if the id was specified (and thus got linked in, from the network), we prevent from adding to the module, cause otherwise it could get delted.
                    renderTo.Add(this);
                }
                else
                {
                    renderTo.AddExternal(Item);
                }

                RenderFunctions(renderTo);
                RenderLinks(renderTo);
                base.Render(renderTo); // still need to render any possible children?
            }
        }

        /// <summary>The create object.</summary>
        /// <returns>The <see cref="Neuron"/>.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private Neuron CreateObject()
        {
            switch (Neurontype)
            {
                case NeuronType.Neuron:
                    return NeuronFactory.GetNeuron();
                case NeuronType.Statement:
                    return NeuronFactory.Get<Statement>();
                case NeuronType.ResultStatement:
                    return NeuronFactory.Get<ResultStatement>();
                case NeuronType.Assignment:
                    return NeuronFactory.Get<Assignment>();
                case NeuronType.BoolExpression:
                    return NeuronFactory.Get<BoolExpression>();
                case NeuronType.ConditionalExpression:
                    return NeuronFactory.Get<ConditionalExpression>();
                case NeuronType.ConditionalStatement:
                    return NeuronFactory.Get<ConditionalStatement>();
                case NeuronType.Lock:
                    return NeuronFactory.Get<LockExpression>();
                case NeuronType.ByRef:
                    return NeuronFactory.Get<ByRefExpression>();
                case NeuronType.Variable:
                    return NeuronFactory.Get<Variable>();
                case NeuronType.Global:
                    return NeuronFactory.Get<Global>();

                case NeuronType.TextSin:
                    return new TextSin(); // these are hardly ever use?
                case NeuronType.AudioSin:
                    return new AudioSin();
                case NeuronType.IntSin:
                    return new IntSin();
                case NeuronType.TimerSin:
                    return new TimerNeuron();
                case NeuronType.ImageSin:
                    return new ImageSin();
                case NeuronType.ReflectionSin:
                    return new ReflectionSin();

                case NeuronType.Int:
                    if (Value != null)
                    {
                        return NeuronFactory.GetInt((int)Value);
                    }

                    return NeuronFactory.GetInt();
                case NeuronType.Double:
                    if (Value != null)
                    {
                        return NeuronFactory.GetDouble((double)Value);
                    }

                    return NeuronFactory.GetDouble();
                case NeuronType.String:
                    if (Value != null)
                    {
                        if (InDict)
                        {
                            TextNeuron.GetFor((string)Value);
                        }
                        else
                        {
                            return NeuronFactory.GetText((string)Value);
                        }
                    }
                    else
                    {
                        return NeuronFactory.Get<TextNeuron>();
                    }

                    break;
                default:
                    throw new System.NotImplementedException();
            }

            return null;
        }

        /// <summary>determins if the neuron points to an instruction that doesn't allow a
        ///     function call, like filters.</summary>
        /// <returns>The <see cref="bool"/>.</returns>
        internal bool GetAllowFunctionsCalls()
        {
            switch (Item.ID)
            {
                case (ulong)PredefinedNeurons.GetChildrenFilteredInstruction:
                    return false;
                case (ulong)PredefinedNeurons.GetClustersFilteredInstruction:
                    return false;
                case (ulong)PredefinedNeurons.GetCommonParentsFilteredInstruction:
                    return false;
                case (ulong)PredefinedNeurons.GetInFilteredInstruction:
                    return false;
                case (ulong)PredefinedNeurons.GetInfoFilteredInstruction:
                    return false;
                case (ulong)PredefinedNeurons.GetoutFilteredInstruction:
                    return false;
                default:
                    return true;
            }
        }

        #region Value

        /// <summary>
        ///     Gets/sets the value (for ints, doubles, strings) that should be
        ///     assigned to the object.
        /// </summary>
        public object Value
        {
            get
            {
                return fValue;
            }

            set
            {
                if (fValue != value)
                {
                    fValue = value;
                    var iNode = value as NNLStatementNode;
                    if (iNode != null)
                    {
                        iNode.Parent = this;
                    }
                }
            }
        }

        #region InDict

        /// <summary>
        ///     Gets/sets the wether the <see langword="static" /> string should be in
        ///     the dictionary or not, <see langword="false" /> by default.
        /// </summary>
        public bool InDict { get; set; }

        #endregion

        #endregion
    }
}