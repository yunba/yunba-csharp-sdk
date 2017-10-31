using System;
using System.IO;
using System.Text;

namespace MqttLib.Core.Messages
{
    internal class MqttPubrecMessage : MqttMessage
    {
        private ulong _ackID;

        public ulong AckID
        {
            get
            {
                return _ackID;
            }
        }

        public MqttPubrecMessage(ulong ackID)
            : base(MessageType.PUBREC, 8)
        {
            _ackID = ackID;
            // Ensure that this message will be re-sent unless acknowledged
            msgQos = QoS.BestEfforts;
        }

        public MqttPubrecMessage(Stream str, byte header) : base(str, header)
        {
            // Ensure that this message can get resent in the event of failure
            this.msgQos = QoS.BestEfforts;
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
