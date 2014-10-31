using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MqttLib.Core.Messages
{
    internal class MqttPubrelMessage : MqttMessage
    {
        private ulong _ackID;

        public ulong AckID
        {
            get
            {
                return _ackID;
            }
        }

        public MqttPubrelMessage(ulong ackID)
            : base(MessageType.PUBREL, 8)
        {
            _ackID = ackID;
        }

        public MqttPubrelMessage(Stream str, byte header) : base(str, header)
        {
            // Nothing to construct
        }

        protected override void SendPayload(System.IO.Stream str)
        {
            WriteToStream(str, _ackID);
        }

        protected override void ConstructFromStream(Stream str)
        {
            _ackID = ReadUlongFromStream(str);
        }
    }
}
