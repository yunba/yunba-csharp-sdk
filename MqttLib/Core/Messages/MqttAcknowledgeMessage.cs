using System;
using System.Collections.Generic;
using System.Text;

namespace MqttLib.Core.Messages
{
    /// <summary>
    /// Abstract superclass for all ACK type messages
    /// </summary>
    internal abstract class MqttAcknowledgeMessage : MqttMessage
    {
        protected ulong _ackID;

        public ulong AckID
        {
            get { return _ackID; }
        }

        public MqttAcknowledgeMessage(MessageType msgType, int varLength, ulong ackID)
            : base(msgType, varLength)
        {
            _ackID = ackID;
        }

        public MqttAcknowledgeMessage(System.IO.Stream str, byte header) : base(str, header)
        {
          // Nothing to construct
        }
    }
}
