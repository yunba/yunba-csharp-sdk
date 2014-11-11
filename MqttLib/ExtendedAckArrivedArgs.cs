using System;
using System.Collections.Generic;
using System.Text;

namespace MqttLib
{
    public class ExtendedAckArrivedArgs : EventArgs
    {
        #region Member variables

        private ulong _messageID;
        private int _commondID;
        private int _status;
        private MqttPayload _payload;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a ExtendedAckArrivedArgs object
        /// </summary>
        /// <param name="messageID">Message id</param>
        /// <param name="commondID">Message type id</param>
        /// <param name="status">Message commond status</param>
        /// <param name="payload">Message data</param>
        public ExtendedAckArrivedArgs(ulong messageID, int commondID, int status, MqttPayload payload)
        {
            _messageID = messageID;
            _commondID = commondID;
            _status = status;
            _payload = payload;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Message id
        /// </summary>
        public ulong MessageID
        {
            get
            {
                return _messageID;
            }
        }

        /// <summary>
        /// Message type id
        /// </summary>
        public int CommondID
        {
            get
            {
                return _commondID;
            }
        }

        /// <summary>
        /// Message status
        /// </summary>
        public int Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Message data
        /// </summary>
        public MqttPayload Payload
        {
            get
            {
                return _payload;
            }
        }

        #endregion
    }
}
