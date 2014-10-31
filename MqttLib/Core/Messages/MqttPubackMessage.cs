using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MqttLib.Core.Messages
{
    internal class MqttPubackMessage : MqttAcknowledgeMessage
    {

        public MqttPubackMessage(ulong ackID) : base(MessageType.PUBACK, 8, ackID)
        {
            // Nothing to construct
        }

        public MqttPubackMessage(Stream str, byte header) : base(str, header)
        {
          // Nothing to construct
        }

        protected override void ConstructFromStream(System.IO.Stream str)
        {
            _ackID = ReadUlongFromStream(str);
        }

        protected override void SendPayload(Stream str)
        {
            WriteToStream(str, _ackID);
        }
    }
}
