using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace JaStDev.HAB.Designer
{
   /// <summary>
   /// A toolboxItem that Produces an item that is the reference to a neuron.
   /// </summary>
   public class NeuronToolBoxItem : ToolBoxItem, IXmlSerializable, INeuronInfo
   {

      #region Fields
      Neuron fItem;
      NeuronData fNeuronInfo;
      #endregion

      #region prop

      #region Item

      /// <summary>
      /// Gets/sets the <see cref="Neuron"/> object that this object wraps.
      /// </summary>
      /// <remarks>
      /// When set, we will try to retrieve the category and name from the
      /// links to list.
      /// </remarks>
      public Neuron Item
      {
         get { return fItem; }
         set
         {
            if (value != null)
            {
               fNeuronInfo = BrainData.Current[value.ID];
            }
            fItem = value;
         }
      }
      #endregion

      #region INeuronInfo Members

      public NeuronData NeuronInfo
      {
         get { return fNeuronInfo; }
      }

      #endregion 

      #endregion



      #region IXmlSerializable Members

      public XmlSchema GetSchema()
      {
         return null;
      }

      public void ReadXml(XmlReader reader)
      {
         bool wasEmpty = reader.IsEmptyElement;

         reader.Read();
         if (wasEmpty) return;

         reader.ReadStartElement("Neuron");
         string iVal = reader.ReadString();
         ulong iConverted = ulong.Parse(iVal);
         Item = Brain.Current[iConverted];
         reader.ReadEndElement();

         reader.ReadEndElement();
      }

      public void WriteXml(XmlWriter writer)
      {
         if (fItem != null)
         {
            writer.WriteStartElement("Neuron");
            writer.WriteString(fItem.ID.ToString());
            writer.WriteEndElement();
         }
      }

      #endregion
   }
}
