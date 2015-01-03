using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using JaStDev.HAB;

namespace AiciWeb.Web
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class AiciService : IAiciService
    {
        Dictionary<string, TextChannel> fOpenChannels = new Dictionary<string, TextChannel>();
        Queue<TextSin> fAvailableChannels = new Queue<TextSin>();
        Aici fAici = new Aici();

        #region IAiciService Members

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="composite">The composite.</param>
        public void SendMessage(AiciMesage composite)
        {
            TextChannel iChannel;
            if (fOpenChannels.TryGetValue(composite.Channel, out iChannel) == true)
                iChannel.Process(composite.InputMessage);
        }

        /// <summary>
        /// Requests a new channel for communication with the network.
        /// All requested channels must be released again.
        /// </summary>
        /// <returns></returns>
        public string RequestChannel()
        {
            StartAici();
            TextChannel iNew = new TextChannel();
            iNew.Id = Guid.NewGuid().ToString();
            if (fAvailableChannels.Count == 0)
            {
                TextSin iSin = new TextSin();
                Brain.Current.Add(iSin);
                iSin.SetFirstOutgoingLinkTo(25257, Brain.Current[25141]);                                  //this makes certain that the sin has a normal state sin->[state]Normal state
                //iSin.SetFirstOutgoingLinkTo((ulong)PredefinedNeurons.ActionsForInput, Brain.Current[7903]);//the code for all the sins that needs to be called after each input sequence (actions on the last input neuron).
                iNew.Sin = iSin;
            }
            else
                iNew.Sin = fAvailableChannels.Dequeue();
            fOpenChannels.Add(iNew.Id, iNew);
            return iNew.Id;
        }

        private void StartAici()
        {
            if (fAici.IsOpen == false)
            {
                fAici.IsOpen = true;
                fAvailableChannels = new Queue<TextSin>(fAici.Channels);
            }
        }

        /// <summary>
        /// Releases the channel so that it can be reused.
        /// </summary>
        /// <param name="id">The id.</param>
        public void ReleaseChannel(string id)
        {
            TextChannel iChannel;
            if (fOpenChannels.TryGetValue(id, out iChannel) == true)
            {
                fAvailableChannels.Enqueue(iChannel.Sin);
                iChannel.Close();
                fOpenChannels.Remove(iChannel.Id);
                if (fOpenChannels.Count == 0)                                                           //we only keep the network open when there is activitiy, otherwise wee close, so we can easely update.
                {
                    CloseAiciTimers();
                    fAici.IsOpen = false;
                    fAvailableChannels = null;
                }
            }
        }

        /// <summary>
        /// Makes certain that there is no timer running anymore when we close the network.
        /// </summary>
        private void CloseAiciTimers()
        {
            TimerSin iTimer = Brain.Current[25316] as TimerSin;
            if (iTimer != null)
                iTimer.IsActive = false;
        }

        #endregion
    }

}
