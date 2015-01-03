// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PredefinedNeurons.cs" company="">
//   
// </copyright>
// <summary>
//   Defines all the Id's of the neurons that are known by the system + 1
//   extra item to declare the start of the dynamic list. This is used by the
//   <see cref="Brain" /> to initialize the start of the dynamic neuron id
//   count.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB
{
    /// <summary>
    ///     Defines all the Id's of the neurons that are known by the system + 1
    ///     extra item to declare the start of the dynamic list. This is used by the
    ///     <see cref="Brain" /> to initialize the start of the dynamic neuron id
    ///     count.
    /// </summary>
    public enum PredefinedNeurons : ulong
    {
        // instructions
        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        PopInstruction = 2, // 0 and 1 are empty and temporary

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        PushInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        AddLinkInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        AddInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        AddChildInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        IndexOfChildInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        IndexOfInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        IndexOfLinkInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        CountInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        CompleteSequenceInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        NewInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        OutputInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        CopyChildrenInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        CopyInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        DeleteInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        DuplicateInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        MoveChildrenInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        MoveInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        PeekInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        RemoveChildInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        RemoveLinkInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        RemoveInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        InsertChildInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        InsertLinkInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        InsertInfoInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        MakeClusterInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        ChangeLinkTo, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        ChangeLinkFrom, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        ChangeLinkMeaning, 

        // default neurons

        /// <summary>
        ///     ID for the neuron used as a link meaning to indicate the neuron local
        ///     entry points of a <see cref="Sin" /> .
        /// </summary>
        EntryPoints, 

        /// <summary>
        ///     ID for the neuron used a s link meaning to indicate the cluster that
        ///     should be called (executed) whenever the sin local entrypoints have
        ///     been recreated (to build further links for instance).
        /// </summary>
        EntryPointsCreated, 

        /// <summary>
        ///     This neuron is used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> to indicate the
        ///     presence of a word in an input stream that was found in the dict. This
        ///     is used as the meaning of a link when it adds words to a
        ///     <see cref="KnoledgeNeuron" /> . This is resolved from there on.
        /// </summary>
        ContainsWord, 

        /// <summary>
        ///     Identifies a neuron used as the meaning of a link pointing to a
        ///     cluster containing letters (where the letter is expressed as an
        ///     <see langword="int" /> neuron).
        /// </summary>
        LetterSequence, 

        /// <summary>
        ///     This neuron is used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> to indicate the
        ///     presence of a <see langword="double" /> in an input stream This is used
        ///     as the meaning of a link when it adds words to a
        ///     <see cref="KnoledgeNeuron" /> . This is resolved from there on.
        /// </summary>
        ContainsDouble, 

        /// <summary>
        ///     This neuron is used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> to indicate the
        ///     presence of an <see langword="int" /> in an input stream. This is used
        ///     as the meaning of a link when it adds words to a
        ///     <see cref="KnoledgeNeuron" /> . This is resolved from there on.
        /// </summary>
        ContainsInt, 

        /// <summary>
        ///     Id for the neuron used as link meaning to indicate the red color of a
        ///     pixel in an image.
        /// </summary>
        Red, 

        /// <summary>
        ///     Id for the neuron used as link meaning to indicate the green color of
        ///     a pixel in an image.
        /// </summary>
        Green, 

        /// <summary>
        ///     Id for the neuron used as link meaning to indicate the blue color of a
        ///     pixel in an image.
        /// </summary>
        Blue, 

        /// <summary>
        ///     Id for the neuron used as link meaning to indicate the gray value of a
        ///     pixel in an image.
        /// </summary>
        Gray, 

        /// <summary>
        ///     <para>
        ///         Id for the neuron used a a link meaning to indicate the width of
        ///         something. This is used by the
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.ImageSin" />
        ///     </para>
        ///     <para>to indicate the size of the image.</para>
        /// </summary>
        Width, 

        /// <summary>
        ///     <para>
        ///         Id for the neuron used a a link meaning to indicate the
        ///         <see cref="Height" /> of something. This is used by the
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.ImageSin" />
        ///     </para>
        ///     <para>to indicate the size of the image.</para>
        /// </summary>
        Height, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ByRefExpression" />
        /// </summary>
        ByRefExpression, 

        /// <summary>
        ///     Identifies the sensory <see langword="interface" /> that provides acces
        ///     to the WordNet text database.
        /// </summary>
        WordNetSin, 

        /// <summary>
        ///     Used as the meaning for a link between a neuron and a cluster
        ///     containing expressions that will be executed when the link is being
        ///     solved.
        /// </summary>
        Rules, 

        /// <summary>
        ///     A neuron that identifies a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NeuronCluster" /> as an
        ///     actions list used by a neuron as the list that contains all the
        ///     expressions used to evaluate what to do after a neuron was processed
        ///     by the processor.
        /// </summary>
        Actions, 

        /// <summary>
        ///     <para>
        ///         A neuron id that identifies a link to a neuron that is to be used as
        ///         the condition of a <see cref="ConditionalExpression" />
        ///     </para>
        ///     <para>
        ///         or the cluster containing all the ConditionalExpressions of a
        ///         <see cref="ConditionalGroup" /> . Also used as the meaning for a link
        ///         from a patternRule to a list of contidional outputs.
        ///     </para>
        /// </summary>
        Condition, 

        /// <summary>
        ///     A neuron id for the neuron used as the
        ///     <see cref="NeuronCluster.Meaning" /> of a cluster of expressions (a
        ///     code list).
        /// </summary>
        Code, 

        /// <summary>
        ///     A neuron used as the meaninf for a link to a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NeuronCluster" /> used to
        ///     hold sub expressions of a <see cref="ConditionalExpression" />
        /// </summary>
        Statements, 

        /// <summary>
        ///     A neuron used as the meaning for a link to another neuron by a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> to
        ///     identify it's operator. the link usually points to
        ///     <see cref="PredefinedNeurons.Equals" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Smaller" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Bigger" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Different" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Or" /> , or
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.And" />
        /// </summary>
        Operator, 

        /// <summary>
        ///     Identifies neuron used as meaning for link between a boolexpression
        ///     and it's left part.
        /// </summary>
        LeftPart, 

        /// <summary>
        ///     Identifies neuron used as meaning for link between a boolexpression
        ///     and it's right part.
        /// </summary>
        RightPart, 

        /// <summary>
        ///     Identifies a neuron that means the equal <see langword="operator" />
        ///     '==' used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Equal, 

        /// <summary>
        ///     Identifies a neuron that means the Less than
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Smaller, 

        /// <summary>
        ///     Identifies a neuron that means the bigger than
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Bigger, 

        /// <summary>
        ///     Identifies a neuron that means the Less than or equal
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        SmallerOrEqual, 

        /// <summary>
        ///     Identifies a neuron that means the bigger than or equal
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        BiggerOrEqual, 

        /// <summary>
        ///     Identifies a neuron that means the not equal
        ///     <see langword="operator" /> (!=) used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Different, 

        /// <summary>
        ///     Identifies a neuron that means the 'Contains'
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Contains, 

        /// <summary>
        ///     Identifies the neuron that means the boolean 'True' value, used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> .
        /// </summary>
        True, 

        /// <summary>
        ///     Identifies the neuron that means the boolean 'False' value, used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> .
        /// </summary>
        False, 

        /// <summary>
        ///     Identifies the neuron used to provide a meaning to the link used
        ///     between a <see cref="JaStDev.HAB.PredefinedNeurons.Statement" /> and
        ///     the <see cref="JaStDev.HAB.PredefinedNeurons.Instruction" /> it
        ///     performs. This is also used as the meaning of a link from a neuron
        ///     sent to a sin as output, to a neuron that reflects the instruction
        ///     that the sin should perform. Also used as the identifier returned by
        ///     'typeof' for an instruction. (or for filtering on children
        ///     GetChildrenOfTypeInstruction.
        /// </summary>
        /// <remarks>
        /// </remarks>
        Instruction, 

        /// <summary>
        ///     Identifies the neuron used to provide a meaning to the link between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Statement" /> and the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NeuronCluster" /> containing
        ///     all the arguments for the statement.
        /// </summary>
        /// <remarks>
        ///     This is also used as the meaning of a link from a neuron sent to a sin
        ///     as output, to a neuron that represents an the argument for the
        ///     instruction. Also used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link from an output neuron to a cluster containing all
        ///     the arguments of function to call/generate.
        /// </remarks>
        Arguments, 

        /// <summary>
        ///     Identifies the neuron used to provide a meaning to the link used
        ///     between a <see cref="ConditionalGroup" /> and the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Neuron" /> that determins how
        ///     the conditional group should loop: not, Case, Looped, CaseLooped,
        ///     ForEach, For, Until.
        /// </summary>
        LoopStyle, 

        /// <summary>
        ///     Identifies the neuron used as link meaning to identify the link to the
        ///     VariableExpression that holds the current item in a loop (for or
        ///     <see langword="foreach" /> loop).
        /// </summary>
        LoopItem, 

        /// <summary>
        ///     Identifies the neuron used as link meaning to identify the link to the
        ///     VariableExpression that holds the item to check in a case statement.
        /// </summary>
        CaseItem, 

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should not loop, but perform a normal if statement.
        /// </summary>
        Normal, 

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should use a case statement.
        /// </summary>
        Case, 

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should loop using a regular while xxx do yyy.
        /// </summary>
        Looped, 

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should loop and evaulate the conditional expressions like a
        ///     case, loop stops when no case matches.
        /// </summary>
        CaseLooped, 

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should use a <see langword="foreach" /> style loop.
        /// </summary>
        ForEach, 

        ///// <summary>
        ///// Identifies the neuron used to let a <see cref="ConditionalGroup"/> know he should use a for(x;y;z) style loop.
        ///// </summary>
        // For,

        /// <summary>
        ///     Identifies the neuron used to let a <see cref="ConditionalGroup" />
        ///     know he should loop with the conditional check at the end.
        /// </summary>
        Until, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Variable" /> and it's initial
        ///     <see cref="Variable.Value" /> .
        /// </summary>
        Value, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SearchExpression" /> and it's
        ///     <see cref="SearchExpression.SearchFor" /> .
        /// </summary>
        SearchFor, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SearchExpression" /> and it's
        ///     <see cref="SearchExpression.ListToSearch" /> . Also between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> and
        ///     <see cref="BoolExpression.ListToSearch" />
        /// </summary>
        ListToSearch, 

        /// <summary>
        ///     Used as the meaning for a link from a parsed thesaurus variable to
        ///     another neuron, to indicate which final value that the path should
        ///     search on the result. This is used to indicate that the thes path
        ///     should get the 'opposite', 'superlative',... of something.
        /// </summary>
        ToSearch, 

        /// <summary>
        ///     used by the 'Contains' <see langword="operator" /> to indicate that the
        ///     list of children should be checked.
        /// </summary>
        Children, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element to 'true' or
        ///     'false/no link' to indicate that the frame element can have multiple
        ///     result items in the flow (following each other in the sequence).
        /// </summary>
        FrameElementAllowMulti, 

        /// <summary>
        ///     <para>
        ///         Identifies the neuron used in
        ///         <see cref="SearchExpression.ListToSearch" /> to indicate that the
        ///         <see cref="Neuron.LinksIn" />
        ///     </para>
        ///     <para>list should be searched.</para>
        /// </summary>
        In, 

        /// <summary>
        ///     <para>
        ///         Identifies the neuron used in
        ///         <see cref="SearchExpression.ListToSearch" /> to indicate that the
        ///         <see cref="Neuron.LinksOut" />
        ///     </para>
        ///     <para>list should be searched.</para>
        /// </summary>
        Out, 

        /// <summary>
        ///     Identifies the neuron used in
        ///     <see cref="SearchExpression.SearchExpression" /> to indicate which
        ///     info to search for (in the info list).
        /// </summary>
        InfoToSearchFor, 

        /// <summary>
        ///     Identifies the neuron that represents a verb (be it textual or in any
        ///     other way). Used by the thesaurus.
        /// </summary>
        Verb, 

        /// <summary>
        ///     Identifies the neuron that represents a noun (be it textual or in any
        ///     other way). Used by the thesaurus.
        /// </summary>
        Noun, 

        /// <summary>
        ///     Identifies the neuron that represents an adjective (be it textual or
        ///     in any other way). Used by the thesaurus.
        /// </summary>
        Adjective, 

        /// <summary>
        ///     Identifies the neuron that represents an adverb (be it textual or in
        ///     any other way). Used by the thesaurus.
        /// </summary>
        Adverb, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Neuron" /> type. This is used
        ///     by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        Neuron, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NeuronCluster" /> type. This
        ///     is used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" />
        ///     to find out which type of neuron it should create.
        /// </summary>
        NeuronCluster, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DoubleNeuron" /> type. This
        ///     is used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" />
        ///     to find out which type of neuron it should create.
        /// </summary>
        DoubleNeuron, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntNeuron" /> type. This is
        ///     used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to
        ///     find out which type of neuron it should create.
        /// </summary>
        IntNeuron, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextNeuron" /> type. This is
        ///     used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to
        ///     find out which type of neuron it should create.
        /// </summary>
        TextNeuron, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> type. This
        ///     is used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" />
        ///     to find out which type of neuron it should create.
        /// </summary>
        BoolExpression, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="ConditionalExpression" /> type. This is used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        ConditionalPart, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ConditionalStatement" />
        ///     type. This is used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        ConditionalStatement, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Statement" /> type. This is
        ///     used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to
        ///     find out which type of neuron it should create.
        /// </summary>
        Statement, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ResultStatement" /> type.
        ///     This is used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        ResultStatement, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SearchExpression" /> type.
        ///     This is used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        SearchExpression, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Variable" /> type. This is
        ///     used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to
        ///     find out which type of neuron it should create.
        /// </summary>
        Variable, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Assignment" /> type. This is
        ///     used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to
        ///     find out which type of neuron it should create.
        /// </summary>
        Assignment, 

        /// <summary>
        ///     Identifies the neuron that represents the 'empty' state. This is a
        ///     neuron that can be used as a return value for instructions to indicate
        ///     no result. ( <see cref="InInstruction" /> returns this value if it
        ///     encountered an error).
        /// </summary>
        Empty, 

        /// <summary>
        ///     Identifies the neuron that represents the variable used to access the
        ///     neuron found in the 'from' part of the link that is being executed in
        ///     order to solve the neuron. <see cref="In" /> other words, this is the
        ///     neuron being solved.
        /// </summary>
        CurrentFrom, 

        /// <summary>
        ///     Identifies the neuron that represents the variable used to access the
        ///     neuron found in the 'to' part of the link that is being executed in
        ///     order to solve the neuron.
        /// </summary>
        CurrentTo, 

        /// <summary>
        ///     Identifies the neuron that represents the variable used to access the
        ///     neuron found in the 'Meaning' part of the link that is being executed
        ///     in order to solve the neuron.
        /// </summary>
        CurrentMeaning, 

        /// <summary>
        ///     Identifies the neuron that represents the variable used to access the
        ///     neuron found in the 'Info' part of the link that is being executed in
        ///     order to solve the neuron.
        /// </summary>
        CurrentInfo, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstOutInstruction" />
        /// </summary>
        GetFirstOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetFirstOutInstruction" />
        /// </summary>
        SetFirstOutInstruction, 

        /// <summary>
        ///     Identifies the neuron that represents a preposition (be it textual or
        ///     in any other way). Usually used as a meaning.
        /// </summary>
        /// <remarks>
        ///     A preposition links nouns, pronouns and phrases to other words in a
        ///     sentence. The word or phrase that the preposition introduces is called
        ///     the object of the preposition. A preposition usually indicates the
        ///     temporal, spatial or logical relationship of its object to the rest of
        ///     the sentence as in the following examples: The book is on the table.
        ///     The book is beneath the table. The book is leaning against the table.
        ///     The book is beside the table.
        /// </remarks>
        Preposition, 

        /// <summary>
        ///     Identifies the neuron that represents a <see cref="Conjunction" /> (be
        ///     it textual or in any other way). Usually used as a meaning.
        /// </summary>
        /// <remarks>
        ///     You can use a conjunction to link words, phrases, and clauses, as in
        ///     the following example: I ate the pizza and the pasta. Call the movers
        ///     when you are ready.
        /// </remarks>
        Conjunction, 

        /// <summary>
        ///     Identifies the neuron that represents an Intersection (be it textual
        ///     or in any other way). Usually used as a meaning.
        /// </summary>
        /// <remarks>
        ///     An interjection is a word added to a sentence to convey emotion. It is
        ///     not grammatically related to any other part of the sentence. You
        ///     usually follow an interjection with an exclamation mark. Interjections
        ///     are uncommon in formal academic prose, except in direct quotations.The
        ///     highlighted words in the following sentences are interjections: Ouch,
        ///     that hurt! Oh no, I forgot that the exam was today. Hey! Put that
        ///     down!
        /// </remarks>
        Interjection, 

        /// <summary>
        ///     <para>
        ///         Identies the neuron used as the meaning of a link to a letter of the
        ///         alphabet expressed as an int. This is used by the
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" />
        ///     </para>
        ///     <para>to process a string as a stream of letters.</para>
        /// </summary>
        Letter, 

        /// <summary>
        ///     Identifies the neuron used as the meaning of a link from a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TimerNeuron" /> to a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DoubleNeuron" /> to indicate
        ///     the time between 2 processing ticks.
        /// </summary>
        Interval, 

        /// <summary>
        ///     Idtentifies the neuron used as the meaning of a link from a neuron
        ///     sent to a <see cref="JaStDev.HAB.PredefinedNeurons.TimerNeuron" /> as
        ///     output, to either <see cref="JaStDev.HAB.PredefinedNeurons.True" /> or
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.False" /> to indicate that is
        ///     should be acitve or not.
        /// </summary>
        IsActive, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link between a sin and
        ///     a cluster containing expressions that the sin will assign to all the
        ///     data it sends for solving as input.
        /// </summary>
        ActionsForInput, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExpressionsBlock" /> neuron
        ///     type. This is used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> .
        /// </summary>
        ExpressionsBlock, 

        /// <summary>
        ///     Identifies the variable that contains a reference to the current
        ///     <see cref="Sin" /> which is the
        /// </summary>
        CurrentSin, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AwakeInstruction" />
        /// </summary>
        AwakeInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SuspendInstruction" />
        /// </summary>
        SuspendInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.WarningInstruction" />
        /// </summary>
        WarningInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ErrorInstruction" />
        /// </summary>
        ErrorInstruction, 

        /// <summary>The and.</summary>
        And, 

        /// <summary>
        ///     Identifies a neuron that stands for the logical <see cref="Or" />
        ///     <see langword="operator" /> '||' used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" />
        /// </summary>
        Or, 

        /// <summary>
        ///     Identifies the neuron that is used by the 'Contains'
        ///     <see langword="operator" /> to indicate that the list of owning
        ///     clusters should be checked.
        /// </summary>
        Clusters, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExecuteInstruction" />
        /// </summary>
        ExecuteInstruction, 

        /// <summary>
        ///     Identifies the neuron used as the link between a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ByRefExpression" /> and it's
        ///     argument.
        /// </summary>
        Argument, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenInstruction" />
        /// </summary>
        GetChildrenInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TypeOfInstruction" />
        /// </summary>
        TypeOfInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersInstruction" />
        /// </summary>
        GetClustersInstruction, 

        /// <summary>
        ///     Identifies the Instruction <see cref="GetClusterInstruction" />
        /// </summary>
        GetFirstClusterInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenOfTypeInstruction" />
        /// </summary>
        GetChildrenOfTypeInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClusterMeaningInstruction" />
        /// </summary>
        GetClusterMeaningInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetClusterMeaningInstruction" />
        /// </summary>
        SetClusterMeaningInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExitLinkInstruction" />
        /// </summary>
        ExitLinkInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitInstruction" />
        /// </summary>
        SplitInstruction, 

        /// <summary>
        ///     Identifies the neuron used as the meaning of a cluster that contains
        ///     the arguments for a statement or resultstatement.
        /// </summary>
        ArgumentsList, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a cluster that
        ///     represents an object (cluster that represents a sinlge point of
        ///     knoledge).
        /// </summary>
        Object, 

        /// <summary>
        ///     Identifies the 'Part of speech' neuron, used as the link meaning from
        ///     an object(or PosGroup) to
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Verb" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Noun" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Adverb" /> ,
        ///     <see cref="PredefinedNeurons.Adjvective" /> ,...
        /// </summary>
        POS, 

        /// <summary>
        ///     Identifies the neuron used as a linkmeaning from an object to an
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntNeuron" /> , which lets
        ///     the brain know that the info was loaded from wordnet + where exactly.
        /// </summary>
        SynSetID, 

        /// <summary>
        ///     Identifies the neuron used as the linkmeaning for cluster that
        ///     contains <see cref="JaStDev.HAB.PredefinedNeurons.TextNeuron" /> s to
        ///     build a compound word (a word that actually exists out of combining
        ///     several words) ex: doppler effect, piano grande,...
        /// </summary>
        /// <remarks>
        ///     I don't like the idea of compound words. I don't think it's
        ///     grammatically correct. <see cref="In" /> the examples, 'doppler' is a
        ///     property of effect, it's a specific effect, the doppler one. But,
        ///     that's how wordnet stores many words, so this is how we load them in
        ///     the brain. It might change this structure at some point.
        /// </remarks>
        CompoundWord, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.UnionInstruction" />
        /// </summary>
        UnionInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntersectInstruction" />
        /// </summary>
        IntersectInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InterleafInstruction" />
        /// </summary>
        InterleafInstruction, 

        /// <summary>
        ///     Identifies the Instruction <see cref="DirectOutputInstruction" />
        /// </summary>
        SolveInstruction, 

        /// <summary>
        ///     Identifies the Instruction <see cref="BlockedOutputInstruction" />
        /// </summary>
        BlockedSolveInstruction, 

        /// <summary>
        ///     Identifies the NeuronCluster that contains all the relationships known
        ///     by the <see cref="WordnetSin" /> neuron.
        /// </summary>
        WordNetRelationships, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetWeightInstruction" />
        /// </summary>
        GetWeightInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetMaxWeightInstruction" />
        /// </summary>
        GetMaxWeightInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DecreaseWeightInstruction" />
        /// </summary>
        DecreaseWeightInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IncreaseWeightInstruction" />
        /// </summary>
        IncreaseWeightInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExitNeuronInstruction" />
        /// </summary>
        ExitNeuronInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ExitSolveInstruction" />
        /// </summary>
        ExitSolveInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearChildrenInstruction" />
        /// </summary>
        ClearChildrenInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearInfoInstruction" />
        /// </summary>
        ClearInfoInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearLinksInInstruction" />
        /// </summary>
        ClearLinksInInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearLinksOutInstruction" />
        /// </summary>
        ClearLinksOutInstruction, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> to indicate that
        ///     it should start a new 'textblock'.
        /// </summary>
        /// <remarks>
        ///     See <see cref="PredefinedNeurons.EndBlock" /> for more info.
        /// </remarks>
        BeginTextBlock, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> to indicate that
        ///     it should start a new 'textblock'.
        /// </summary>
        /// <remarks>
        ///     If the TextSin needs to form sentences or other blocks instead of
        ///     simply sending word per word or lette per letter, you can use the
        ///     'BeginBlock' and 'EndBlock' neurons to indicate a block. See more in
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.TextSin" /> .
        /// </remarks>
        EndTextBlock, 

        /// <summary>
        ///     Used as the meaning for a link from a topic (pattern editor) to a
        ///     cluster containing conditional output clusters. Also Used as a link
        ///     from an outputpattern to itself, to indicate that itself is a question
        ///     and no following questions are allowed.
        /// </summary>
        Questions, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link to indicate that it should try to call the function
        ///     that is linked to the 'out' part of the link.
        /// </summary>
        ReflectionSinCall, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link from an output neuron and the name of the
        ///     function/field/property to call/render. When this is used to call a
        ///     function, it must first have been loaded. So that the sin knows about
        ///     the function.
        /// </summary>
        NameOfMember, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link from an output neuron to a cluster containing the
        ///     body of the assembly/type/function (the neurons linking to the opcodes
        ///     and the arguments for the opcodes).
        /// </summary>
        BodyOfMember, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link from an output neuron to a textneuron that contains
        ///     the fully qualified name of a type.
        /// </summary>
        TypeOfMember, 

        /// <summary>
        ///     Identifies the neuron used by the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReflectionSin" /> as the
        ///     meaning for a link from an output neuron to a member.
        /// </summary>
        MemberType, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a neuron cluster that
        ///     contains frame elements, which identify the parts that need to be
        ///     present in a frame. A frame defines all the required parts of an
        ///     information block.
        /// </summary>
        Frame, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element to a frame, to
        ///     indicate that the element evokes the frame. This allows us to search
        ///     faster in the set of frames by only looking at those that have been
        ///     evoked. Note that the element should be a child of the frame to which
        ///     it points.
        /// </summary>
        IsFrameEvoker, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a cluster (and as the
        ///     meaning for the link between a frame and this cluster) that contains
        ///     clusters that represent 'known sequences. Each cluster in this list
        ///     should have a meaning of 'FrameSequence'.
        /// </summary>
        FrameSequences, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a cluster that
        ///     represents a single 'knonw' sequence for a <see cref="Frame" />
        ///     cluster.
        /// </summary>
        FrameSequence, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from an 'object'
        ///     cluster to an <see langword="int" /> indicating the Id of the frame
        ///     element that corresponds to the object.
        /// </summary>
        FrameElementId, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from an 'object'
        ///     cluster to either 'Frame_Core', 'Frame_peripheral' or
        ///     'Frame_extra_thematic' to indicate the importance of a frame element
        ///     within the set of elements for the frame.
        /// </summary>
        FrameImportance, 

        /// <summary>
        ///     Identifies the neuron used to indicate that a frame element is of core
        ///     importance, or required.
        /// </summary>
        Frame_Core, 

        /// <summary>
        ///     Identifies the neuron used to indicate that a frame element is a
        ///     peripheral item, so not required.
        /// </summary>
        Frame_peripheral, 

        /// <summary>
        ///     Identifies the neuron used to indicate that a frame element is a
        ///     thematic item.
        /// </summary>
        Frame_extra_thematic, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from an 'object'
        ///     cluster to an <see langword="int" /> indicating the 'lemma' id of the
        ///     object as it is defined in framenet. a lemma is similar as a synset in
        ///     wordnet.
        /// </summary>
        LemmaId, 

        /// <summary>
        ///     Identifies the neuron that represents an article (the/a).
        /// </summary>
        Article, 

        /// <summary>
        ///     Identifies the neuron that represents a determiner (elke/deze/welke).
        /// </summary>
        Determiner, 

        /// <summary>
        ///     Identifies the neuron that represents a conjunction (dat/of). ->
        ///     onderschikkend voegwoord.
        /// </summary>
        Complementizer, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CiToSInstruction" />
        /// </summary>
        CiToSInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CcToSInstruction" />
        /// </summary>
        CcToSInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DToIInstruction" />
        /// </summary>
        DToIInstruction, 

        /// <summary>
        ///     {35A90EBF-F421-44A3-BE3A-47C72AFE47FE}
        /// </summary>
        DToSInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IToDInstruction" />
        /// </summary>
        IToDInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IToSInstruction" />
        /// </summary>
        IToSInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToCcInstruction" />
        /// </summary>
        SToCcInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToCiInstruction" />
        /// </summary>
        SToCiInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ResetWeightInstruction" />
        /// </summary>
        ResetWeightInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddSplitResultInstruction" />
        /// </summary>
        AddSplitResultInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveSplitResultInstruction" />
        /// </summary>
        RemoveSplitResultInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearSplitResultsInstruction" />
        /// </summary>
        ClearSplitResultsInstruction, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetSplitResultsInstruction" />
        /// </summary>
        GetSplitResultsInstruction, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a neuroncluster used as
        ///     a flow.
        /// </summary>
        Flow, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a NeuronCluster used as
        ///     a loop or option in a flow.
        /// </summary>
        FlowItemConditional, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a NeuronCluster used as
        ///     a part of a loop or option in a flow.
        /// </summary>
        FlowItemConditionalPart, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from a
        ///     <see cref="FlowItemConditional" /> to <see cref="True" /> or false, to
        ///     indicate that it is looped or not.
        /// </summary>
        FlowItemIsLoop, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from a
        ///     <see cref="FlowItemConditional" /> to <see cref="True" /> or false, to
        ///     indicate that a selection between one of the parts is required, the
        ///     conditional can't be skipped. This reference is only usefull if there
        ///     are multiple conditional parts.
        /// </summary>
        FlowItemRequiresSelection, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersWithMeaningInstruction" />
        /// </summary>
        GetClustersWithMeaningInstruction, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from a
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Global" /> to
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Duplicate" /> ,
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Copy" /> or
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Empty" /> to indicate the
        ///     action to take when the processor gets split.
        /// </summary>
        SplitReaction, 

        /// <summary>
        ///     Identifies a neuron used to attach to a var with the 'SplitReaction'
        ///     neuron to indicate that the content of the global should be
        ///     duplcated/cloned when a split is done on the processor.
        /// </summary>
        Duplicate, 

        /// <summary>
        ///     Identifies a neuron used to attach to a var with the 'SplitReaction'
        ///     neuron to indicate that the content of global should be copied when a
        ///     split is done on the processor.
        /// </summary>
        Copy, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Global" /> type. This is used
        ///     by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create.
        /// </summary>
        Global, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstInInstruction" />
        /// </summary>
        GetFirstInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstInfoInstruction" />
        /// </summary>
        GetFirstInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstChildInstruction" />
        /// </summary>
        GetFirstChildInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastChildInstruction" />
        /// </summary>
        GetLastChildInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastClusterInstruction" />
        /// </summary>
        GetLastClusterInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastInfoInstruction" />
        /// </summary>
        GetLastInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastInInstruction" />
        /// </summary>
        GetLastInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastOutInstruction" />
        /// </summary>
        GetLastOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextChildInstruction" />
        /// </summary>
        GetNextChildInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextClusterInstruction" />
        /// </summary>
        GetNextClusterInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextInfoInstruction" />
        /// </summary>
        GetNextInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextInInstruction" />
        /// </summary>
        GetNextInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetNextOutInstruction" />
        /// </summary>
        GetNextOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevChildInstruction" />
        /// </summary>
        GetPrevChildInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevInfoInstruction" />
        /// </summary>
        GetPrevInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevInInstruction" />
        /// </summary>
        GetPrevInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevInInstruction" />
        /// </summary>
        GetPrevOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetPrevClusterInstruction" />
        /// </summary>
        GetPrevClusterInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetFirstInstruction" />
        /// </summary>
        GetFirstInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLastInstruction" />
        /// </summary>
        GetLastInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContinueInstruction" />
        /// </summary>
        ContinueInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BreakInstruction" />
        /// </summary>
        BreakInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReverseInstruction" />
        /// </summary>
        ReverseInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StackCountInstruction" />
        /// </summary>
        StackCountInstruction, 

        /// <summary>
        ///     Identifies the neuron used as the to part for a link from a 'flow'
        ///     cluster with meaning 'FlowType' to indicate that the flow can appear
        ///     anywhere is a stream or not. Other flows and parts will be broken up
        ///     when this type of floating flow is found. To prevent this, use
        ///     FlowIsNonDestructiveFloating.
        /// </summary>
        FlowIsFloating, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CallInstruction" />
        /// </summary>
        CallInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClearInstruction" />
        /// </summary>
        ClearInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToDInstruction" />
        /// </summary>
        SToDInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SToIInstruction" />
        /// </summary>
        SToIInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClustersFilteredInstruction" />
        /// </summary>
        GetClustersFilteredInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenFilteredInstruction" />
        /// </summary>
        GetChildrenFilteredInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.FreezeInstruction" />
        /// </summary>
        FreezeInstruction, 

        /// <summary>
        ///     Used as the meaning for the link between a <see cref="Frame" />
        ///     element(comming from a verbnet role element) and the role object.
        /// </summary>
        VerbNetRole, 

        /// <summary>
        ///     Used as the meaning for the link between a <see cref="Frame" />
        ///     element(comming from a verbnet role element) and the restriction
        ///     object. Also used as the meaning for the cluster that contains all the
        ///     restrictions for a role.
        /// </summary>
        VerbNetRestrictions, 

        /// <summary>
        ///     Identifies the neuron used as the meaning for a link from a
        ///     verbNet-restrictions cluster to Or/And to indicate the logic value of
        ///     the record (is it optional, required, not specified).
        /// </summary>
        VerbNetLogicValue, 

        /// <summary>
        ///     Used as the meaning for a link from a <see cref="Frame" /> sequence
        ///     item, to the value it represents (the frame element, or a general
        ///     purpose item).
        /// </summary>
        FrameSequenceItemValue, 

        /// <summary>
        ///     Used as the meaning for the link between a <see cref="Frame" />
        ///     element-Restriction segment (comming from a verbnet role
        ///     element-selectional restriction) and the restriction object, or also
        ///     used as the meaning for a restriction cluster that contains segments.
        /// </summary>
        VerbNetRestriction, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element restriction item
        ///     (from a verbnet import) and
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RestrictionModifierInclude" />
        ///     or
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RestrictionModifierExclude" />
        /// </summary>
        VerbNetRestrictionModifier, 

        /// <summary>
        ///     <para>
        ///         This neuron is attached to a frame element restriction (with the
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.VerbNetRestrictionModifier" />
        ///     </para>
        ///     <para>
        ///         neuron as meaning), to indicate that all items that comply to the
        ///         restriction should be included.
        ///     </para>
        /// </summary>
        RestrictionModifierInclude, 

        /// <summary>
        ///     <para>
        ///         This neuron is attached to a frame element restriction (with the
        ///         <see cref="JaStDev.HAB.PredefinedNeurons.VerbNetRestrictionModifier" />
        ///     </para>
        ///     <para>
        ///         neuron as meaning), to indicate that all items that comply to the
        ///         restriction should be excluded.
        ///     </para>
        /// </summary>
        RestrictionModifierExclude, 

        /// <summary>
        ///     This neuron is used as the meaning for a link from a looped flow
        ///     item-conditional to 'True' or 'False' to indicate that 2 consecutive
        ///     conditional parts of the loop need to have a floating flow in between
        ///     them (as in: between an integer, <see langword="float" /> and word,
        ///     there need to be spaces).
        /// </summary>
        RequiresFloatingSeparator, 

        /// <summary>
        ///     This neuron is used as the meaning for a link from a looped flow
        ///     item-conditional to 'True' (or 'False') to indicate that the flow
        ///     conditional stops collection parts if a floating flow is encountered.
        /// </summary>
        FloatingFlowSplits, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChildCountInstruction" />
        /// </summary>
        ChildCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ClusterCountInstruction" />
        /// </summary>
        ClusterCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LinkInCountInstruction" />
        /// </summary>
        LinkInCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LinkOutCountInstruction" />
        /// </summary>
        LinkOutCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InfoCountInstruction" />
        /// </summary>
        InfoCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsInitializedInstruction" />
        /// </summary>
        IsInitializedInstruction, 

        /// <summary>
        ///     Used as the meaning for a cluster that contains object clusters and
        ///     textneurons that share the same text and pos. This allows for faster
        ///     parsing since only 1 split is performed per pos type.
        /// </summary>
        POSGroup, 

        /// <summary>
        ///     Used as the meaning for a link from a textneuron to a
        ///     <see cref="MorphOf" /> cluster (cluster with this neuron as meaning),
        ///     to indicate that the textneuron is a morphed representation of the
        ///     same word (for instance: bought is morphed word of buy. The
        ///     <see cref="MorphOf" /> cluster contains the posgroup cluster for which
        ///     it is a morph, and the tranformation info (for instance, the
        ///     textneuron 'ing' to indicate that this is appended to it, or a neuron
        ///     indicating it is the past form of the verb.
        /// </summary>
        MorphOf, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyInstruction" />
        /// </summary>
        MultiplyInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModulusInstruction" />
        /// </summary>
        ModulusInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusInstruction" />
        /// </summary>
        MinusInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DivideInstruction" />
        /// </summary>
        DivideInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AdditionInstruction" />
        /// </summary>
        AdditionInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInAtInstruction" />
        /// </summary>
        GetInAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetClusterAtInstruction" />
        /// </summary>
        GetClusterAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoAtInstruction" />
        /// </summary>
        GetInfoAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildAtInstruction" />
        /// </summary>
        GetChildAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetOutAtInstruction" />
        /// </summary>
        GetOutAtInstruction, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element restriction item
        ///     (from a verbnet import) and another neuron (usually one from the list
        ///     of default meanings), to indicate which link path to search on in the
        ///     thesaurus, when checking if an item is allowed according to the
        ///     filter.
        /// </summary>
        VerbNetRestrictionSearchDirection, 

        /// <summary>
        ///     Used as the meaning for a link from a neuron to 'true' or 'false', to
        ///     indicate that it is a recursive value or not. This is used by neurons
        ///     that represent (wordnet)relationship types.
        /// </summary>
        IsRecursive, 

        /// <summary>
        ///     Represents the pronoun part of speech (I, you, me, mine)
        /// </summary>
        Pronoun, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DistinctInstruction" />
        /// </summary>
        DistinctInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoInstruction" />
        /// </summary>
        GetInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetOutgoingInstruction" />
        /// </summary>
        GetOutgoingInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetIncommingInstruction" />
        /// </summary>
        GetIncommingInstruction, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element to another frame,
        ///     to indicate that the other frame should be used to evaluate the result
        ///     value link to the frame element.
        /// </summary>
        SubFrame, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetoutFilteredInstruction" />
        /// </summary>
        GetoutFilteredInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetoutFilteredInstruction" />
        /// </summary>
        GetInFilteredInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetInfoFilteredInstruction" />
        /// </summary>
        GetInfoFilteredInstruction, 

        /// <summary>
        ///     Identifies a neuron that means the 'not Contains'
        ///     <see langword="operator" /> used by
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BoolExpression" /> , which is
        ///     the reverse of the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Contains" /> operator.
        /// </summary>
        NotContains, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsAllChildrenInstruction" />
        /// </summary>
        ContainsAllChildrenInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsClusteredByInstruction" />
        /// </summary>
        IsClusteredByInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LinkExistsInstruction" />
        /// </summary>
        LinkExistsInstruction, 

        /// <summary>
        ///     Used as the meaning for a link between a frame element restriction or
        ///     restrictiongroup and 'true' or 'false' to indicate wether the
        ///     restriction defines every element that should be included in the
        ///     result, or if the result is allowed to have more items.
        /// </summary>
        RestrictionDefinesFullContent, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ComplementInstruction" />
        /// </summary>
        ComplementInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveChildAtInstruction" />
        /// </summary>
        RemoveChildAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveInfoAtInstruction" />
        /// </summary>
        RemoveInfoAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkInAtInstruction" />
        /// </summary>
        RemoveLinkInAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkInAtInstruction" />
        /// </summary>
        RemoveLinkOutAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkInAtInstruction" />
        /// </summary>
        SubstractInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinkInAtInstruction" />
        /// </summary>
        FilterInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinksOutInstruction" />
        /// </summary>
        RemoveLinksOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveLinksInInstruction" />
        /// </summary>
        RemoveLinksInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetAtInstruction" />
        /// </summary>
        GetAtInstruction, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LockExpression" /> type. This
        ///     is used by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" />
        ///     to find out which type of neuron it should create.
        /// </summary>
        LockExpression, 

        /// <summary>
        ///     Identifies a neuron used as the meaning for a link from a
        ///     lockExpression to a neuroncluster that contains expressions for
        ///     retrieving the neurons that need to be locked.
        /// </summary>
        NeuronsToLock, 

        /// <summary>
        ///     Identifies a neuron used as the meaning for a link from a
        ///     lockExpression to a neuroncluster that contains expressions for
        ///     retrieving the links that need to be locked, that is, the neurons that
        ///     identify the link.
        /// </summary>
        LinksToLock, 

        /// <summary>
        ///     Identifies the Instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitWeightedInstruction" />
        /// </summary>
        SplitWeightedInstruction, 

        /// <summary>
        ///     used as the meaning for a link from a neuron to a textneuron, to
        ///     indicate that the function call (defined by the 'from' neuron), caused
        ///     an exception with the specified name (the to part).
        /// </summary>
        ReflectionSinException, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element to a neuron to
        ///     indicate that this is used as the result
        /// </summary>
        FrameElementResultType, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsLinksInInstruction" />
        /// </summary>
        ContainsLinksInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsLinksOutInstruction" />
        /// </summary>
        ContainsLinksOutInstruction, 

        /// <summary>
        ///     Used as the to part for a link from a flow with meaning 'FlowType', to
        ///     indicate that that the flow is floating, and non destructive, so this
        ///     type of flow event won't <see langword="break" /> the middle of a
        ///     conditional part. Conditionals can determine themselves how they
        ///     handle the presence of a float, but when this value is set, it
        ///     overrules that of the contitional: the flow will be dropped, so the
        ///     conditional can't see a floatin flow-.
        /// </summary>
        FlowIsNonDestructiveFloating, 

        /// <summary>
        ///     Used as the meaning for a link to indicate that a floating flow should
        ///     retain the data in the result set. Normally, floating flows drop their
        ///     result data. If this is not desirable, attach 'true' with this
        ///     meaning.
        /// </summary>
        FloatingFlowKeepsData, 

        /// <summary>
        ///     Used as the meaning for a link from a flow to 'FlowIsFloating' or
        ///     'FLowIsNonDestructiveFloating', to indicate it's
        ///     <see langword="float" /> type. When this link is present, the flow is
        ///     always floating.
        /// </summary>
        FlowType, 

        /// <summary>
        ///     This is a system variable that can be used to retrieve the current
        ///     system time. Also: The neuron used to represent the type of the 'Time'
        ///     variable. Also used as the cluster meaning for all cluster that the
        ///     'time' variable generates.
        /// </summary>
        Time, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AvgInstruction" />
        /// </summary>
        AvgInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MaxInstruction" />
        /// </summary>
        MaxInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinInstruction" />
        /// </summary>
        MinInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StDevInstruction" />
        /// </summary>
        StDevInstruction, 

        /// <summary>
        ///     This neuron is used as the meaning for a cluster to indicate that it
        ///     contains a timespan value, similar like a time clustser, except that
        ///     this is used in calculations with Time clusters: Time - Time =
        ///     <see cref="TimeSpan" /> Time + <see cref="TimeSpan" /> = Time Avg(Time)
        ///     = Time Agv(TimeSpan) = <see cref="TimeSpan" /> Max(Time) = Time
        ///     Max(TimeSpan) = <see cref="TimeSpan" /> Min(Time) = Time Min(TimeSpan)
        ///     = time StDev(TimeSpan) = <see cref="TimeSpan" />
        /// </summary>
        TimeSpan, 

        /// <summary>
        ///     A neuronCluster that is called as code when the network is first
        ///     loaded/started.
        /// </summary>
        OnStarted, 

        /// <summary>
        ///     A neuronCluster that is called as code when the network is
        ///     closed/cleared.
        /// </summary>
        OnShutDown, 

        /// <summary>
        ///     A neuronCluster that is called as code when there is activity on a
        ///     sin.
        /// </summary>
        OnSinActivity, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeInfoInstruction" />
        /// </summary>
        ChangeInfoInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeChildInstruction" />
        /// </summary>
        ChangeChildInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ChangeParentInstruction" />
        /// </summary>
        ChangeParentInstruction, 

        /// <summary>
        ///     Used as the meaning for a link from an object or posgroup) to an
        ///     intNeuron, to indicate the offset that should be applied to the weight
        ///     of the processor. This is used when a word has multiple pos meanings,
        ///     some of which get precedence over others.
        /// </summary>
        ImportanceLevel, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsClusteredByAnyInstruction" />
        /// </summary>
        IsClusteredByAnyInstruction, 

        /// <summary>
        ///     Used as the meaning for an asset cluster, or as the meaning for a link
        ///     from an object to it's asset cluster. This is also a cluster, so that
        ///     it can keep reference of all the entry point assets available to the
        ///     system.
        /// </summary>
        Asset, 

        /// <summary>
        ///     Used as the meaning for a link from an asset item to a neuron that
        ///     indicates the 'attribute' (type) of the asset item. ex values: hand,
        ///     name, color,...
        /// </summary>
        Attribute, 

        /// <summary>
        ///     Used as the meaning for a link from an asset item to an integer or
        ///     double, to indicate the number of instances that the asset represents.
        /// </summary>
        Amount, 

        /// <summary>
        ///     Used as the meaning for a link from an object to a cluster that
        ///     contains frames. Also used as the meaning for the cluster that
        ///     contains those frames.
        /// </summary>
        Frames, 

        /// <summary>
        ///     Used as the meaning for a link from a frame element restriction to a
        ///     neuron, to indicate which part of the 'result' should be inspected.
        ///     This is used by the object frames to define which 'semantic' value to
        ///     compare.
        /// </summary>
        ValueToInspect, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.CallSaveInstruction" />
        /// </summary>
        CallSaveInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonParentsFilteredInstruction" />
        /// </summary>
        GetCommonParentsFilteredInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonParentsInstruction" />
        /// </summary>
        GetCommonParentsInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonParentsInstruction" />
        /// </summary>
        GetCommonParentsWithMeaningInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetChildrenRangeInstruction" />
        /// </summary>
        GetChildrenRangeInstruction, 

        /// <summary>
        ///     Used as the meaning of a cluster that represents a visual frame
        ///     filter.
        /// </summary>
        VisualFrame, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetSplitCountInstruction" />
        ///     isntruction.
        /// </summary>
        GetSplitCountInstruction, 

        /// <summary>
        ///     Used as the meaning for a cluster that groups a set of rules together.
        ///     Also used as the meaning for a link from the final neuron in the
        ///     parsed pattern, to the pattern group that it belongs to, so we can
        ///     find the outputs for a certain pattern.
        /// </summary>
        TextPatternTopic, 

        /// <summary>
        ///     Used as the meaning for a cluster that represents a single rule
        ///     definition.
        /// </summary>
        PatternRule, 

        /// <summary>
        ///     Used as the meaning for a cluster and a link from a TextPattern to a
        ///     cluster with this meaning, to indicate that the textpattern should use
        ///     the specified outputs.
        /// </summary>
        TextPatternOutputs, 

        /// <summary>
        ///     Used as the meaning for a link from a text neuron to a cluster (which
        ///     has this meaning), to indicate the outputs that should be used if
        ///     there was no valid response given to the outputvalue found in 'to'.
        /// </summary>
        InvalidResponsesForPattern, 

        /// <summary>
        ///     Used as the meaning for a link from a textneuron (pattern-output) to a
        ///     cluster containing textneurons, to indicate all the valid questions
        ///     for which this can be a response.
        /// </summary>
        ResponseForOutputs, 

        /// <summary>
        ///     Used as the meaning for a link from a TextNeuron (single text pattern
        ///     item) to another textNeuron which is the start of the parse tree for
        ///     the pattern item. This allows us to find the start of an parse for
        ///     removing it.
        /// </summary>
        ParsedPatternStart, 

        /// <summary>
        ///     Used as the meaing for a link from a TextPattern item (single text
        ///     neuron), to all the neurons (except the start) that make up the parsed
        ///     pattern tree. This allows us to retrieve all the nodes that need to be
        ///     removed for a single pattern item, in case it gets reparsed.
        /// </summary>
        ParsedPatternPart, 

        /// <summary>
        ///     Used as the meaning for a link from a textneuron (TextPatternOutput)
        ///     to a cluster (with same meaning), to indicate how the output pattern
        ///     should be rendered to the output.
        /// </summary>
        ParsedPatternOutput, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInfoInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomInfoInstruction, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInfoInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomClusterInstruction, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomOutInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomOutInstruction, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomInInstruction, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomChildInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomChildInstruction, 

        /// <summary>
        ///     Identifies the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRandomInstruction" />
        ///     isntruction.
        /// </summary>
        GetRandomInstruction, 

        /// <summary>
        ///     Identifies a neuroncluster that contains output statements which
        ///     should be used when there is no valid pattern match found.
        /// </summary>
        ResponsesForEmptyParse, 

        /// <summary>
        ///     Used as the meaning for a link from a <see cref="TextPatternOutputs" />
        ///     cluster to another cluster, also with this as meaning. This cluster
        ///     stores all the responses that have already been used, and should
        ///     therefor not be used again until all others have been used.
        /// </summary>
        UsedResponses, 

        /// <summary>
        ///     Identifies a neuroncluster that contains output statements which
        ///     should be used as openings to start the conversation.
        /// </summary>
        ConversationStarts, 

        /// <summary>
        ///     Identifies a neuroncluster that contains variable definitions to parse
        ///     variable content. The variable defs contained in here, are the
        ///     possible starts that need to be processed. The cluster is also used as
        ///     the meaning for the clusters that represent the actual parsed
        ///     variables.
        /// </summary>
        ParsedVariable, 

        /// <summary>
        ///     Identifies a neuroncluster that contains thesaurus variable
        ///     definitions to parse thesaurus dependend-variable content. The
        ///     thesaurus variable defs contained in here, are the possible starts
        ///     that need to be processed. The cluster is also used as the meaning for
        ///     the clusters that represent the actual parsed thes variables.
        /// </summary>
        ParsedThesVar, 

        /// <summary>
        ///     Used as the meaning for a cluster that contains 'do patterns'. Also
        ///     used as the meaning for a link between a
        ///     <see cref="PatternOutputsCollection" /> (cluster) and the
        ///     <see cref="DoPatternCollection" /> cluster.
        /// </summary>
        DoPatterns, 

        /// <summary>
        ///     Identifies a neuroncluster that contains asset variable definitions to
        ///     parse asset dependend-variable content. The asset items form a path to
        ///     be executed. The cluster is also used as the meaning for the clusters
        ///     that represent the actual parsed asset variables.
        /// </summary>
        ParsedAssetVar, 

        /// <summary>
        ///     Used as the meaning of a link to indicate that the right side (a
        ///     cluster), should be 'solved when the left side (a textneuron, used to
        ///     express a conditional pattern in the textpatterns), gets activated.
        /// </summary>
        Operand, 

        /// <summary>
        ///     Used as the meaning for a link from a textneuron (do pattern) to a
        ///     neuron to indicate what needs to be done.
        /// </summary>
        ParsedDoPattern, 

        /// <summary>
        ///     Identifies the neuron that represents the += symbol. For the
        ///     assets/thesaurus, this is used as an 'add' instruction,
        /// </summary>
        AssignAdd, 

        /// <summary>
        ///     Identifies the neuron that represents the -= symbol. For the
        ///     assets/thesaurus, this is used as an 'remove' instruction,
        /// </summary>
        AssignRemove, 

        /// <summary>
        ///     Stores all the previous conversation log items.
        /// </summary>
        ConversationLogHistory, 

        /// <summary>
        ///     Determins how an output is picked from it's list: at random, or in
        ///     sequence. Used as the meaning for a link from an ouput cluster.
        /// </summary>
        OutputListTraversalMode, 

        /// <summary>
        ///     Used as a value (to part of a link) for the
        ///     <see cref="OutputListTraversalMode" /> of the list. it indicates that
        ///     random items should be picked from hte list
        /// </summary>
        Random, 

        /// <summary>
        ///     Used as a value (to part of a link) for the
        ///     <see cref="OutputListTraversalMode" /> of the list. it indicates that
        ///     items should be picked in sequence from the list
        /// </summary>
        Sequence, 

        /// <summary>
        ///     A cluster that contains do patterns that should be executed after each
        ///     input/output processing.
        /// </summary>
        DoAfterStatement, 

        /// <summary>
        ///     Used as the link from an input patttern to
        ///     <see cref="PartialInputPattern" /> or
        ///     <see cref="PartialInputPatternFallback" /> to indicate that it is a
        ///     partial pattern + which type of partial pattern.
        /// </summary>
        InputPatternPartialMode, 

        /// <summary>
        ///     Used as the to part for a link from an inputpattern with meaning
        ///     'InputPatternPartialMode', to indicate that the pattern can be
        ///     declared multiple times. The context will be determined by the pattern
        ///     editor that contains it (only 1 editor may contain the same input
        ///     pattern). The pattern that belongs to the editor that was last
        ///     activated, gets activated.
        /// </summary>
        PartialInputPattern, 

        /// <summary>
        ///     Used as the to part for a link from an inputpattern with meaning
        ///     'InputPatternPartialMode', to indicate that the pattern can be
        ///     declared multiple times. The context will be determined by the pattern
        ///     editor that contains it (only 1 editor may contain the same input
        ///     pattern). If there is no context found (no editor already
        ///     encountered), use the pattern with this value. Only 1 allowed per
        ///     duplication set.
        /// </summary>
        PartialInputPatternFallback, 

        /// <summary>
        ///     Used as the meaning for a cluster that is parth of a thesaurus or
        ///     asset path, to indicate that the value at the specified index should
        ///     be used.
        /// </summary>
        Index, 

        /// <summary>
        ///     A cluster that stores 'pattern output clusters' that can be used when
        ///     repetition was encountered in the input.
        /// </summary>
        RepeatOutputPatterns, 

        /// <summary>
        ///     used to indicate the textsin that the next word (or letter) needs to
        ///     have it's first letter capitilized
        /// </summary>
        FirstUppercase, 

        /// <summary>
        ///     used to indicate the textsin that the next word (or letter) needs to
        ///     be capitilized completely
        /// </summary>
        AllUppercase, 

        /// <summary>
        ///     used as the meaning for a cluster to indicate that a capitalization
        ///     map should be used for the next word. The cluster contains all the
        ///     indexes at which the letters need to be capitalized.
        /// </summary>
        UppercaseMap, 

        /// <summary>
        ///     A cluster that contains do statements which should be executed when
        ///     the application starts (called by the 'OnStarted' event neuron)
        /// </summary>
        DoOnStartup, 

        /// <summary>
        ///     executes the HasReferences instruction.
        /// </summary>
        HasReferencesInstruction, 

        /// <summary>
        ///     Used as the meaning for a link from a rule to a do-patterns cluster.
        ///     These do patterns will be executed when the rule is activated, before
        ///     any of the conditions on the output are executed.
        /// </summary>
        Calculate, 

        /// <summary>
        ///     Used as the meaning for a link from a bot to a outputs cluster (no
        ///     code, no conditions), to indicate how to calculate the context values
        ///     for callbacks like 'Attribute'. This context is defiened in the
        ///     <see cref="ChatbotProperties" /> class.
        /// </summary>
        Context, 

        /// <summary>
        ///     Used as the meaning for a link from an input pattern to itself, to
        ///     indicate that the pattern can only produce a valid match if the first
        ///     word of the input is matched by the pattern (so the pattern needs to
        ///     be at the start of the input).
        /// </summary>
        PatternAtStartOfInput, 

        /// <summary>
        ///     Used as the meaning for a link from an input pattern to itself, to
        ///     indicate that the pattern can only produce a valid match if the last
        ///     word of the input is matched by the pattern (so the pattern needs to
        ///     be at the end of the input).
        /// </summary>
        PatternAtEndOfInput, 

        /// <summary>
        ///     Used as the meaning for a link from a parsed to pattern neuron, to
        ///     itself. It indicates that the execution of the do pattern should be
        ///     delayed untill the rest has been processed.
        /// </summary>
        Delay, 

        /// <summary>
        ///     Instructs an input variable (in an input pattern), to collect all the
        ///     spaces between the words that it collects (if it's more than 1).
        /// </summary>
        CollectSpaces, 

        /// <summary>
        ///     Used as the meaning for a link from a pattern to parts that represent
        ///     sub topics. Also the start neuron for patterns that start with sub
        ///     topics.
        /// </summary>
        SubTopics, 

        /// <summary>
        ///     Used as the meaning for a link from a pattern to parts that represent
        ///     sub rules. Also the start neuron for patterns that start with sub
        ///     rules.
        /// </summary>
        SubRules, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetLinkMeaningInstruction" />
        /// </summary>
        GetLinkMeaningInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetAllIncommingInstruction" />
        /// </summary>
        GetAllIncommingInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetAllOutgoingInstruction" />
        /// </summary>
        GetAllOutgoingInstruction, 

        /// <summary>
        ///     Identifies the neuron that represents the !+= symbol. For the assets,
        ///     this is used as an 'add' non terminals,
        /// </summary>
        AssignAddNot, 

        /// <summary>
        ///     Used to indicate a general number neuron (IntNeuron or DoubleNeuron).
        ///     This is only used in Thesaurus paths.
        /// </summary>
        Number, 

        /// <summary>
        ///     Used as the meaning for a cluster that contains a single child. The
        ///     cluster is usually part of a path (like variable or thesuaurus path),
        ///     to indicate that a direct link out should be taken. We use a seperate
        ///     neuron for this instead of 'out' (which was originaly used), cause
        ///     'out' is also used to indicate output statements in the log, which
        ///     might be confusing for searches.
        /// </summary>
        LinkOut, 

        /// <summary>
        ///     Used as the meaning for a cluster to indicate that it contains a list
        ///     of items. This is used when the input contains a list like : a,b, c,d
        /// </summary>
        List, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.EndsWithInstruction" />
        /// </summary>
        EndsWithInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitStringInstruction" />
        /// </summary>
        SplitStringInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IToDTInstruction" />
        /// </summary>
        IToDTInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ContainsChildrenInstruction" />
        /// </summary>
        ContainsChildrenInstruction, 

        /// <summary>
        ///     Represents a cluster that contains code which should be executed when
        ///     a new session is started. This happens when a new sin is created.
        /// </summary>
        OnSinCreated, 

        /// <summary>
        ///     Represents a cluster that contains code which should be executed when
        ///     a new session is closed. This happens when a sin is destroyed.
        /// </summary>
        OnSinDestroyed, 

        /// <summary>
        ///     Identifies a TextSin
        /// </summary>
        TextSin, 

        /// <summary>
        ///     Identifies a TimerNeuron
        /// </summary>
        TimerNeuron, 

        /// <summary>
        ///     Identifies a AudioSin
        /// </summary>
        AudioSin, 

        /// <summary>
        ///     Identifies a FaceSin
        /// </summary>
        IntSin, 

        /// <summary>
        ///     Identifies a <see cref="GuiSin" />
        /// </summary>
        GuiSin, 

        /// <summary>
        ///     Identifies a ImageSin
        /// </summary>
        ImageSin, 

        /// <summary>
        ///     Identifies a ReflectionSin
        /// </summary>
        ReflectionSin, 

        /// <summary>
        ///     used as the meaning for a link from an input-pattern to a do-pattern
        ///     list to indicate which list to execute when furher evaluation is
        ///     needed to check if one pattern is more probable then another.
        /// </summary>
        Evaluate, 

        /// <summary>
        ///     Used as the meaning for a link between a cluster representing a thes
        ///     path and a value, indicating that an asset item should be added with
        ///     the specified attrib/value pair to the already loaded asset. If no
        ///     asset is loaded, create a new one. This is used by the wordnet sin to
        ///     load data (like from an android contacts list).
        /// </summary>
        LoadAssetItem, 

        /// <summary>
        ///     Used as the meaning for a link between a cluster representing a thes
        ///     path and a value, indicating that the thes relationship should be
        ///     created with the specified attrib/value pair . This is used by the
        ///     wordnet sin to load data (like from an android contacts list).
        /// </summary>
        LoadThesValue, 

        /// <summary>
        ///     Used as the meaning for a link from an invalidResponseFor pattern to
        ///     itself, to indicate that the bot still requires a valid response after
        ///     the InvalidResponsefor value has been rendered (otherwise, the system
        ///     no longer requires a response after an invalid response is rendered).
        /// </summary>
        RequiresResponse, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IndexOfClusterInstruction" />
        /// </summary>
        IndexOfClusterInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetAtInstruction" />
        /// </summary>
        SetAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IntersectVarInstruction" />
        /// </summary>
        IntersectVarInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PerformResultInstruction" />
        /// </summary>
        PerformResultInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PerformInstruction" />
        /// </summary>
        PerformInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveAtInstruction" />
        /// </summary>
        RemoveAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddStoreIntInstruction" />
        /// </summary>
        AddStoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.NotInstruction" />
        /// </summary>
        NotInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusInstruction" />
        /// </summary>
        InvertSignInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertDoubleInstruction" />
        /// </summary>
        InvertDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertIntInstruction" />
        /// </summary>
        InvertIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertIntListInstruction" />
        /// </summary>
        InvertIntListInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InvertDoubleListInstruction" />
        /// </summary>
        InvertDoubleListInstruction, 

        /// <summary>
        ///     Identifies the neuron that represents the
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.Local" /> type. This is used
        ///     by <see cref="JaStDev.HAB.PredefinedNeurons.NewInstruction" /> to find
        ///     out which type of neuron it should create. Also used to indicate that
        ///     a topic has local input pattern definitions only.
        /// </summary>
        Local, 

        /// <summary>
        ///     identifies the system variable that is used to retrieve the result of
        ///     the last function call.
        /// </summary>
        ReturnValue, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PushValueInstruction" />
        /// </summary>
        PushValueInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PopValueInstruction" />
        /// </summary>
        PopValueInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DecrementInstruction" />
        /// </summary>
        DecrementInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IncrementInstruction" />
        /// </summary>
        IncrementInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnValueInstruction" />
        /// </summary>
        ReturnValueInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoreIntInstruction" />
        /// </summary>
        StoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddStoreDoubleInstruction" />
        /// </summary>
        AddStoreDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoreDoubleInstruction" />
        /// </summary>
        StoreDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddAssignIntInstruction" />
        /// </summary>
        AddAssignIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddAssignDoubleInstruction" />
        /// </summary>
        AddAssignDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddInstruction" />
        /// </summary>
        AddInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusAssignDoubleInstruction" />
        /// </summary>
        MinusAssignDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusAssignIntInstruction" />
        /// </summary>
        MinusAssignIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModIntInstruction" />
        /// </summary>
        ModIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModDoubleInstruction" />
        /// </summary>
        ModDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DivIntInstruction" />
        /// </summary>
        DivIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DivDoubleInstruction" />
        /// </summary>
        DivDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyIntInstruction" />
        /// </summary>
        MultiplyIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyDoubleInstruction" />
        /// </summary>
        MultiplyDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusIntInstruction" />
        /// </summary>
        MinusIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusDoubleInstruction" />
        /// </summary>
        MinusDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddIntInstruction" />
        /// </summary>
        AddIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.AddDoubleInstruction" />
        /// </summary>
        AddDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ToSInstruction" />
        /// </summary>
        ToSInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.LengthInstruction" />
        /// </summary>
        LengthInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetRangeInstruction" />
        /// </summary>
        GetRangeInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DeleteFrozenInstruction" />
        /// </summary>
        DeleteFrozenInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.UnFreezeInstruction" />
        /// </summary>
        UnFreezeInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ApplyWeightInstruction" />
        /// </summary>
        ApplyWeightInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PrepareLocalInstruction" />
        /// </summary>
        PrepareLocalInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnInstruction" />
        /// </summary>
        ReturnInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ParamStackCountInstruction" />
        /// </summary>
        ParamStackCountInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.JmpIfInstruction" />
        /// </summary>
        JmpIfInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReturnValueIfInstruction" />
        /// </summary>
        ReturnValueIfInstruction, 

        /// <summary>
        ///     Used to indicate the special query/select loop style for looping over
        ///     query data.
        /// </summary>
        QueryLoop, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonOutInstruction" />
        /// </summary>
        GetCommonOutInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetCommonInInstruction" />
        /// </summary>
        GetCommonInInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.BlockedCallInstruction" />
        /// </summary>
        BlockedCallInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SortInstruction" />
        /// </summary>
        SortInstruction, 

        /// <summary>
        ///     <para>
        ///         Used to indicate the special query/select loop style for looping over
        ///         the incomming links of a neuron.
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 or 3 Variables should be used: for from and meaning, optionally info
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        QueryLoopIn, 

        /// <summary>
        ///     <para>
        ///         Used to indicate the special query/select loop style for looping over
        ///         the outgoing links of a neuron.
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 or 3 Variables should be used: for to and meaning, optionally info
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        QueryLoopOut, 

        /// <summary>
        ///     <para>
        ///         Used to indicate the special query/select loop style for looping over
        ///         the children of a cluster. This differs from a a normal foreach(x in
        ///         GetChildren), since the <see langword="foreach" /> will buffer all the
        ///         neurons, the select works on ids, so it can work over longer lists
        ///         with less mem consumption.
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 or 3 Variables should be used: for from and meaning, optionally info
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        QueryLoopChildren, 

        /// <summary>
        ///     <para>
        ///         Used to indicate the special query/select loop style for looping over
        ///         the parents of a neuron. This differs from a a normal foreach(x in
        ///         GetClusters), since the <see langword="foreach" /> will buffer all the
        ///         neurons, the select works on ids, so it can work over longer lists
        ///         with less mem consumption.
        ///     </para>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 or 3 Variables should be used: for from and meaning, optionally info
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        QueryLoopClusters, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SplitAccumInstruction" />
        /// </summary>
        SplitAccumInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IncreaseWeightOfInstruction" />
        /// </summary>
        IncreaseWeightOfInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DecreaseWeightOfInstruction" />
        /// </summary>
        DecreaseWeightOfInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoLiInstruction" />
        /// </summary>
        StoLiInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoLiInstruction" />
        /// </summary>
        SubStringInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StoLiInstruction" />
        /// </summary>
        GetCharAtInstruction, 

        /// <summary>
        ///     Identifies a neuron used to attach to a variable with the
        ///     'SplitReaction' neuron to indicate that the content of global should
        ///     be <see cref="shared" /> when a split is done on the processor.
        /// </summary>
        shared, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.RemoveInstruction" />
        /// </summary>
        RemoveInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyStoreIntInstruction" />
        /// </summary>
        MultiplyStoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusStoreIntInstruction" />
        /// </summary>
        MinusStoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DivStoreIntInstruction" />
        /// </summary>
        DivStoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModStoreIntInstruction" />
        /// </summary>
        ModStoreIntInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.DivStoreDoubleInstruction" />
        /// </summary>
        DivStoreDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MinusStoreDoubleInstruction" />
        /// </summary>
        MinusStoreDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ModStoreDoubleInstruction" />
        /// </summary>
        ModStoreDoubleInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.MultiplyStoreDoubleIntruction" />
        /// </summary>
        MultiplyStoreDoubleIntruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetStringInstruction" />
        /// </summary>
        SetStringInstruction, 

        /// <summary>
        ///     the content of the next item will be put into lowercase.
        /// </summary>
        AllLowerCase, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.ReplaceInstruction" />
        /// </summary>
        ReplaceInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.InsertInstruction" />
        /// </summary>
        InsertInstruction, 

        /// <summary>
        ///     used as the meaning for a link from a a patterntopic to a cluster
        ///     (that also has this as meaning), to indicate the list of patterns that
        ///     should be used to filter agains #user.topic
        /// </summary>
        TopicFilter, 

        /// <summary>
        ///     Identifies a GridSin
        /// </summary>
        GridSin, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetWeightOfInstruction" />
        /// </summary>
        GetWeightOfInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.PassFrozenToCallerInstruction" />
        /// </summary>
        PassFrozenToCallerInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetChildAtInstruction" />
        /// </summary>
        SetChildAtInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetChildAtInstruction" />
        /// </summary>
        SplitFixedInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.IsTimerActiveInstruction" />
        /// </summary>
        IsTimerActiveInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StartTimerInstruction" />
        /// </summary>
        StartTimerInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.StopTimerInstruction" />
        /// </summary>
        StopTimerInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.SetTimerIntervalInstruction" />
        /// </summary>
        SetTimerIntervalInstruction, 

        /// <summary>
        ///     Identifies the instruction
        ///     <see cref="JaStDev.HAB.PredefinedNeurons.GetTimerIntervalInstruction" />
        /// </summary>
        GetTimerIntervalInstruction, 

        /// <summary>
        ///     this indicates the actual end of statically defined items by the
        ///     system. This is used by the explorer to find allow for a gap between
        ///     <see langword="static" /> and dynamic items.
        /// </summary>
        EndOfStatic, 

        /// <summary>
        ///     This indicates the start of the dynamic list count (this is the start
        ///     value that the brain can use to dynamically assign id to new neurons.
        /// </summary>
        Dynamic = 1000
    }
}