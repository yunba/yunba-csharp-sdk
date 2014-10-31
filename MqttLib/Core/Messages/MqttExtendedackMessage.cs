using System;
using System.Collections.Generic;
using System.Text;

namespace MqttLib.Core.Messages
{
    internal class MqttExtendedackMessage : MqttAcknowledgeMessage
    {
        private byte[] _payload;

        public MqttExtendedackMessage(System.IO.Stream str, byte header)
            : base(str, header)
        {
          // Nothing to construct
        }

        protected override void ConstructFromStream(System.IO.Stream str)
        {
            int payloadLen = base.variableHeaderLength;

            _messageID = ReadUlongFromStream(str);
            payloadLen -= 8;

            _payload = new byte[payloadLen];

            ReadCompleteBuffer(str, _payload);
        }
    }
}
