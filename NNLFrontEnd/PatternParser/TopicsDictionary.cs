// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopicsDictionary.cs" company="">
//   
// </copyright>
// <summary>
//   An exception generated when the topic couldn't find a name.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace JaStDev.HAB.Parsers
{
    /// <summary>
    ///     An exception generated when the topic couldn't find a name.
    /// </summary>
    [System.Serializable]
    public class TopicsDictException : System.Exception
    {
        /// <summary>Initializes a new instance of the <see cref="TopicsDictException"/> class.</summary>
        public TopicsDictException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TopicsDictException"/> class.</summary>
        /// <param name="name">The name.</param>
        public TopicsDictException(string name)
            : base("Failed to find: " + name)
        {
            Name = name;
        }

        /// <summary>Initializes a new instance of the <see cref="TopicsDictException"/> class.</summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public TopicsDictException(string message, System.Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TopicsDictException"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="topic">The topic.</param>
        public TopicsDictException(string name, Neuron topic)
            : base("Failed to find: " + name)
        {
            Name = name;
            Topic = topic;
        }

        /// <summary>Initializes a new instance of the <see cref="TopicsDictException"/> class.</summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected TopicsDictException(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>Gets or sets the topic.</summary>
        public Neuron Topic { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string Name { get; set; }
    }

    /// <summary>
    ///     Manages a dictionary with topics that map to names that the parsers can
    ///     use. This is required cause topics don't store their names intermally,
    ///     it's a designer feature, which needs to supply the names. Before using
    ///     make certain that
    /// </summary>
    public class TopicsDictionary
    {
        /// <summary>The f topics.</summary>
        private static readonly System.Collections.Generic.Dictionary<string, ulong> fTopics =
            new System.Collections.Generic.Dictionary<string, ulong>();

        /// <summary>Initializes static members of the <see cref="TopicsDictionary"/> class.</summary>
        static TopicsDictionary()
        {
            NameGetter = null;
        }

        /// <summary>
        ///     Gets or sets the function that provides names for neurons. This is
        ///     used to find the rule names.
        /// </summary>
        /// <value>
        ///     The name getter.
        /// </value>
        public static System.Func<ulong, string> NameGetter { get; set; }

        /// <summary>Adds the specified topic to the dictionary under the specified name,
        ///     case insensitive.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true"/> if the item could be added,<see langword="false"/> if there already was an item with the same
        ///     value.</returns>
        public static bool Add(string id, Neuron value)
        {
            return Add(id, value.ID);
        }

        /// <summary>Adds the specified topic to the dictionary under the specified name,
        ///     case insensitive.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true"/> if the item could be added,<see langword="false"/> if there already was an item with the same
        ///     value.</returns>
        public static bool Add(string id, ulong value)
        {
            id = id.ToLower();
            if (fTopics.ContainsKey(id) == false)
            {
                fTopics.Add(id, value);
                return true;
            }

            return false;
        }

        /// <summary>Removes the topic with the specified name.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        public static void Remove(string id, Neuron value)
        {
            Remove(id, value.ID);
        }

        /// <summary>Removes the topic with the specified name.</summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        public static void Remove(string id, ulong value)
        {
            id = id.ToLower();
            ulong iFound;
            if (fTopics.TryGetValue(id, out iFound) && value == iFound)
            {
                fTopics.Remove(id);
            }
        }

        /// <summary>
        ///     Clears the dictionary
        /// </summary>
        public static void Clear()
        {
            fTopics.Clear();
        }

        /// <summary>Gets the topic neuron for the specified name. If not found, an
        ///     exception is thrown.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron Get(string id)
        {
            id = id.ToLower();
            ulong iFound;
            if (fTopics.TryGetValue(id, out iFound))
            {
                return Brain.Current[iFound];
            }

            throw new TopicsDictException(id);
        }

        /// <summary>Tries to find the topic with the specified name. If not found,<see langword="null"/> is returned.</summary>
        /// <param name="id">The id.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron Find(string id)
        {
            id = id.ToLower();
            ulong iFound;
            if (fTopics.TryGetValue(id, out iFound))
            {
                return Brain.Current[iFound];
            }

            return null;
        }

        /// <summary>Walks through all the keys untill it can build a name with the
        ///     specified string and a nr appended to it, that forms a unique value.</summary>
        /// <param name="value">The value.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetUnique(string value)
        {
            var i = 1;
            var iBuild = new System.Text.StringBuilder(value);
            iBuild.Append(i);
            ulong iTemp;
            var iRes = iBuild.ToString();
            while (fTopics.TryGetValue(iRes.ToLower(), out iTemp))
            {
                i++;
                iBuild.Clear();
                iBuild.Append(value);
                iBuild.Append(i);
                iRes = iBuild.ToString();
            }

            return iRes;
        }

        /// <summary>Walks through all the children of the item, if cluster, and tries to
        ///     render a unique name based on the specified prefix, by adding a nr to
        ///     it.</summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="parent">The parent.</param>
        /// <returns>The <see cref="string"/>.</returns>
        public static string GetUniqueRuleName(string prefix, Neuron parent)
        {
            var iRes = prefix;
            if (NameGetter == null)
            {
                throw new System.InvalidOperationException("Please provide a name supplier for the topicDictionary.");
            }

            var iTopic = parent as NeuronCluster;
            if (iTopic != null)
            {
                System.Collections.Generic.List<Neuron> iToSearch;
                using (var iList = iTopic.Children) iToSearch = iList.ConvertTo<Neuron>();
                try
                {
                    var i = 1;
                    var iBuild = new System.Text.StringBuilder(prefix);
                    iBuild.Append(i);
                    iRes = iBuild.ToString();
                    var iFound = true;
                    while (iFound)
                    {
                        iFound = false;
                        foreach (var iChild in iToSearch)
                        {
                            if (NameGetter(iChild.ID) == iRes)
                            {
                                iFound = true;
                                i++;
                                iBuild.Clear();
                                iBuild.Append(prefix);
                                iBuild.Append(i);
                                iRes = iBuild.ToString();
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iToSearch);
                }
            }

            return iRes;
        }

        /// <summary>Checks if the <paramref name="name"/> is unique within the specified
        ///     context.</summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="item">The item who's <paramref name="name"/> to check.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        public static bool CheckUniqueRuleName(string name, Neuron parent, Neuron item)
        {
            name = name.ToLower();
            if (NameGetter == null)
            {
                throw new System.InvalidOperationException("Please provide a name supplier for the topicDictionary.");
            }

            var iTopic = parent as NeuronCluster;
            if (iTopic != null)
            {
                System.Collections.Generic.List<Neuron> iChildren;
                using (var iList = iTopic.Children) iChildren = iList.ConvertTo<Neuron>();
                try
                {
                    foreach (var iChild in iChildren)
                    {
                        if (iChild != item)
                        {
                            var iName = NameGetter(iChild.ID); // could be that there is no name found.
                            if (string.IsNullOrEmpty(iName) == false && iName.ToLower() == name)
                            {
                                return false;
                            }
                        }
                    }
                }
                finally
                {
                    Factories.Default.NLists.Recycle(iChildren);
                }
            }

            return true;
        }

        /// <summary>Gets the rule with the specified <paramref name="name"/> in the
        ///     specified topic. If not found, an exceptio is thrown.</summary>
        /// <param name="topic">The topic.</param>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron GetRule(Neuron topic, string name)
        {
            name = name.ToLower();
            if (NameGetter == null)
            {
                throw new System.InvalidOperationException("Please provide a name supplier for the topicDictionary.");
            }

            var iTopic = topic as NeuronCluster;
            if (iTopic != null)
            {
                System.Collections.Generic.List<ulong> iToSearch;
                using (var iList = iTopic.Children)
                {
                    iToSearch = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);
                    iToSearch.AddRange(iList); // only get the id's, convert as needed.
                }

                try
                {
                    foreach (var i in iToSearch)
                    {
                        var iName = NameGetter(i);
                        if (string.IsNullOrEmpty(iName) == false && iName.ToLower() == name)
                        {
                            return Brain.Current[i];
                        }
                    }
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iToSearch);
                }
            }

            throw new TopicsDictException(name, topic);
        }

        /// <summary>Gets the rule with the specified <paramref name="name"/> in the
        ///     specified topic. IF not found, <see langword="null"/> is returend.</summary>
        /// <param name="topic">The topic.</param>
        /// <param name="name">The name.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron FindRule(Neuron topic, string name)
        {
            name = name.ToLower();
            if (NameGetter == null)
            {
                throw new System.InvalidOperationException("Please provide a name supplier for the topicDictionary.");
            }

            var iTopic = topic as NeuronCluster;
            if (iTopic != null)
            {
                System.Collections.Generic.List<ulong> iToSearch;
                using (var iList = iTopic.Children)
                {
                    iToSearch = Factories.Default.IDLists.GetBuffer(iList.CountUnsafe);
                    iToSearch.AddRange(iList); // only get the id's, convert as needed.
                }

                try
                {
                    foreach (var i in iToSearch)
                    {
                        var iName = NameGetter(i);
                        if (string.IsNullOrEmpty(iName) == false && iName.ToLower() == name)
                        {
                            return Brain.Current[i];
                        }
                    }
                }
                finally
                {
                    Factories.Default.IDLists.Recycle(iToSearch);
                }
            }

            return null;
        }

        /// <summary>Gets the specified <paramref name="topic"/> and or rule, if not null.</summary>
        /// <param name="topic">The topic.</param>
        /// <param name="rule">The rule.</param>
        /// <returns>The <see cref="Neuron"/>.</returns>
        public static Neuron Get(string topic, string rule)
        {
            if (topic != null)
            {
                var iRes = Get(topic);
                if (rule != null && iRes != null)
                {
                    iRes = GetRule(iRes, rule);
                }

                return iRes;
            }

            return null;
        }
    }
}