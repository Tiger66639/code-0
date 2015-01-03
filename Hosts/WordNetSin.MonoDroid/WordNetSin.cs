//-----------------------------------------------------------------------
// <copyright file="WordNetSin.cs">
//     Copyright (c) 2008-2012 Jan Bogaerts. All rights reserved.
// </copyright> 
// <authorJan Bogaerts</author>
// <email>Jan.Bogaerts@telenet.be</email>
// <date>08/09/2012</date>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JaStDev.HAB;

namespace JaStDev.HAB
{
   [NeuronID((ulong)PredefinedNeurons.WordNetSin)]
   [NeuronID((ulong)PredefinedNeurons.SynSetID, typeof(Neuron))]
   [NeuronID((ulong)PredefinedNeurons.CompoundWord, typeof(Neuron))]
   [NeuronID((ulong)PredefinedNeurons.WordNetRelationships, typeof(NeuronCluster))]
   [NeuronID((ulong)PredefinedNeurons.MorphOf, typeof(Neuron))]
   [NeuronID((ulong)PredefinedNeurons.IsRecursive, typeof(Neuron))]
   [NeuronID((ulong)PredefinedNeurons.LoadAssetItem, typeof(Neuron))]
   [NeuronID((ulong)PredefinedNeurons.LoadThesValue, typeof(Neuron))]
   public class WordNetSin : Sin
   {
      /// <summary>
      /// Flushes this instance.
      /// </summary>
      public override void Flush()
      {
         //don't do anything, wordnet is not supported for android.
      }

      /// <summary>
      /// Outputs the specified to send.
      /// </summary>
      /// <param name="toSend">To send.</param>
      public override void Output(IList<Neuron> toSend)
      {
         //don't do anything, wordnet is not supported for android.
      }


      /// <summary>
      /// Used to create the start of an instruction list that needs to be sent to the processor.
      /// This uses the SynsetId as a link and not the LoadAssetItem, cause we don't want to add the 'lookup-kye' 
      /// to the thesaurus.
      /// </summary>
      /// <param name="id"></param>
      /// <param name="result"></param>
      public void BeginProcess(string id, List<Neuron> result)
      {
         NeuronCluster iCluster = NeuronFactory.GetCluster();
         Brain.Current.Add(iCluster);
         TextNeuron iId = NeuronFactory.GetText(id);                                       //always create a new textneuron, not through the dict, but as a non registered neuron. This way, we make certain that the exact string stays preserved.
         Brain.Current.Add(iId);
         Link.Create(iCluster, iId, (ulong)PredefinedNeurons.SynSetID);
         result.Add(iCluster);
      }

      /// <summary>
      /// creates a parsed thespath and links this to the string value with 'LoadAssetItem'.
      /// This is used to instruct the android/chatbot network to load this data both as thes relationships
      /// </summary>
      /// <param name="p"></param>
      /// <param name="thesPath"></param>
      /// <param name="value"></param>
      /// <param name="result"></param>
      public void AddToProcess(ulong pos, string[] thesPath, string value, List<Neuron> result)
      {
         NeuronCluster iCluster = NeuronFactory.GetCluster();
         Brain.Current.Add(iCluster);
         iCluster.Meaning = (ulong)PredefinedNeurons.ParsedThesVar;
         using (IDListAccessor iChildren = iCluster.ChildrenW)
         {
            iChildren.Add(pos);
            foreach (string i in thesPath)
            {
               Neuron iChild = BrainHelper.GetNeuronForText(i);
               iChildren.Add(iChild);
            }
         }
         Neuron iVal = BrainHelper.GetNeuronForText(value);
         Link iLink = Link.Create(iCluster, iVal, (ulong)PredefinedNeurons.LoadAssetItem);
         result.Add(iCluster);
      }


      /// <summary>
      /// sends the list of neurons to the network for processing.
      /// </summary>
      /// <remarks>
      /// what the network does with this input is up to the implementation inside the network, but
      /// in general, the values are added as thesuarus items to the thespath. If an id is specified, 
      /// an asset is created with the id as value (and attribute something like 'id') + the thespath as attrib with the 'value' as values part of the asset item.
      /// </remarks>
      /// <param name="toSend"></param>
      /// <param name="title">a possible log text.</param>
      public void Process(List<Neuron> toSend, string title, Processor proc = null)
      {
         if(proc == null)
            proc = ProcessorFactory.GetProcessor();
         toSend.Reverse();                                                                   //we reverse the list cause we want the synset to be executed first (first in the list), but we are workin with a stack, so the last becomes first,...
         Process(toSend, proc, title);
      }

   }
}