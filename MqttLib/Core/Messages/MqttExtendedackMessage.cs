using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace MqttLib.Core.Messages
{
    internal class MqttExtendedackMessage : MqttAcknowledgeMessage
    {
        private byte _commondId;
        private byte _status;
        private ushort _leftLength;
        private byte[] _payload;

        public MqttExtendedackMessage(ulong msgID, byte commondId, string payloadStr)
            : base(MessageType.EXTENDEDACK, 8, msgID)
      {
            _commondId = commondId;

            UTF8Encoding enc = new UTF8Encoding();
            byte[] payload = enc.GetBytes(payloadStr);
            if (payload.Length <= 65535)
            {
                _leftLength = (ushort)payload.Length;
                _payload = payload;
            }
            else
                throw new ArgumentOutOfRangeException("payload length is longer then 65535.");

            base.variableHeaderLength = 11 + _payload.Length;
      }

        public MqttExtendedackMessage(ulong msgID, string topic, byte[] payload, QoS qos, string apn_json)
            : base(MessageType.EXTENDEDACK, 8, msgID)
        {
            _commondId = 7;

            MemoryStream pay = new MemoryStream();

            pay.WriteByte(0);
            WriteToStream(pay, topic);

            pay.WriteByte(1);
            WriteToStream(pay, payload);

            string[] qos2str = {"0", "1", "2"};
            pay.WriteByte(6);
            WriteToStream(pay, qos2str[(int)qos]);

            if (apn_json != null)
            {
                pay.WriteByte(7);
                WriteToStream(pay, apn_json);
            }

            byte[] paybytes = pay.ToArray();
            if (paybytes.Length <= 65535)
            {
                _leftLength = (ushort)paybytes.Length;
                _payload = paybytes;
            }
            else
                throw new ArgumentOutOfRangeException("payload length is longer then 65535.");

            base.variableHeaderLength = 11 + _payload.Length;
        }

        public MqttExtendedackMessage(System.IO.Stream str, byte header)
            : base(str, header)
        {}

        protected override void SendPayload(System.IO.Stream str)
        {
            WriteToStream(str, _ackID);

            str.WriteByte(_commondId);
            WriteToStream(str, _leftLength);

            str.Write(_payload, 0, _payload.Length);
        }

        protected override void ConstructFromStream(System.IO.Stream str)
        {
            int payloadLen = base.variableHeaderLength;

            _messageID = ReadUlongFromStream(str);
            payloadLen -= 8;

            _commondId = (byte)str.ReadByte();
            _status = (byte)str.ReadByte();
            _leftLength = ReadUshortFromStream(str);

            if (_leftLength > 0)
            {
                _payload = new byte[_leftLength];
                ReadCompleteBuffer(str, _payload);
            }
        }

        public int CommondId
        {
            get
            {
                return _commondId;
            }
        }

        public int Status
        {
            get
            {
                return _status;
            }
        }

        public MqttPayload Payload
        {
            get
            {
                return new MqttPayload(_payload, 0);
            }
        }
    }
}
