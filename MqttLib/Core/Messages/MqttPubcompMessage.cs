using System;
using System.IO;
using System.Text;

namespace MqttLib.Core.Messages
{
    internal class MqttPubcompMessage : MqttMessage
    {
        private ulong _ackID;

        public ulong AckID
        {
            get
            {
                return _ackID;
            }
        }

        public MqttPubcompMessage(ulong ackID)
            : base(MessageType.PUBCOMP, 8)
        {
            _ackID = ackID;
        }

        public MqttPubcompMessage(Stream str, byte header): base(str, header)
        {
            // Nothing to construct
        }

        protected override void ConstructFromStream(Stream str)
        {
            _ackID = ReadUlongFromStream(str);
        }

        protected override void SendPayload(Stream str)
        {
            WriteToStream(str, _ackID);
        }
    }
}
