using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Runtime.Serialization;

namespace AiciWeb.Web
{
    // NOTE: If you change the interface name "IService1" here, you must also update the reference to "IService1" in App.config.
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IAiciCallback))]
    public interface IAiciService
    {
        /// <summary>
        /// Requests a new channel for communication with the network.
        /// All requested channels must be released again.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        string RequestChannel();

        /// <summary>
        /// Releases the channel so that it can be reused. 
        /// </summary>
        /// <param name="id">The id.</param>
        [OperationContract]
        void ReleaseChannel(string id);

        [OperationContract]
        void SendMessage(AiciMesage composite);


        // TODO: Add your service operations here
    }

    /// <summary>
    /// The interface that needs to be implemented by the client to make certain it can receive
    /// events from the network server.
    /// </summary>
    public interface IAiciCallback
    {
        /// <summary>
        /// Called when there is output to render by the client.
        /// </summary>
        /// <param name="value">The value.</param>
        [OperationContract(IsOneWay = true)]
        void Output(string value);
    }

    // Use a data contract as illustrated in the sample below to add composite types to service operations
    [DataContract]
    public class AiciMesage
    {
        /// <summary>
        /// Gets or sets the input message.
        /// </summary>
        /// <value>The input message.</value>
        [DataMember]
        public string InputMessage { get; set; }

        /// <summary>
        /// Gets or sets the id of the channel over which to send the message. This must be for an open/requested channel.
        /// </summary>
        /// <value>The channel.</value>
        [DataMember]
        public string Channel { get; set; }
    }
}