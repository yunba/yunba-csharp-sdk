using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttLib
{
    public interface IMqttExtend
    {
        /// <summary>
        /// Publish a message to the MQTT message broker with some options
        /// </summary>
        /// <param name="topic">Destination of message</param>
        /// <param name="payload">Message body</param>
        /// <param name="qos">QoS level</param>
        /// <param name="ttl">Time for message will be stored on server, in seconds. If set 0, the message will be forever on the server.</param>
        /// <param name="apn_json">APNS</param>
        /// <returns>Message ID</returns>
        ulong Publish2(string topic, MqttPayload payload, QoS qos, int ttl, string apn_json);

        /// <summary>
        /// Publish a message to a specific client with alias and some options
        /// </summary>
        /// <param name="alias">alias name</param>
        /// <param name="payload">Message body</param>
        /// <param name="qos">QoS level</param>
        /// <param name="ttl">Time for message will be stored on server, in seconds. If set 0, the message will be forever on the server.</param>
        /// <param name="apn_json">APNS</param>
        /// <returns>Message ID</returns>
        ulong Publish2Alias(string alias, MqttPayload payload, QoS qos, int ttl, string apn_json);

        /// <summary>
        /// Request the client's alias
        /// </summary>
        /// <param name="cb">callback function</param>
        void GetAlias(ExtendedAckArrivedDelegate cb);

        /// <summary>
        /// Request the client's topics
        /// </summary>
        /// <param name="cb">callback function</param>
        void GetTopicList(ExtendedAckArrivedDelegate cb);

        /// <summary>
        /// Request a specific client's topics
        /// </summary>
        /// <param name="alias">the client's alias</param>
        /// <param name="cb">callback function</param>
        void GetTopicList(string alias, ExtendedAckArrivedDelegate cb);

        /// <summary>
        /// Request a specific topic's alias
        /// </summary>
        /// <param name="topic">topic</param>
        /// <param name="cb">callback function</param>
        void GetAliasList(string topic, ExtendedAckArrivedDelegate cb);

        /// <summary>
        /// Request a specific client's state
        /// </summary>
        /// <param name="alias">the topic's alias</param>
        /// <param name="cb">callback function</param>
        void GetState(string alias, ExtendedAckArrivedDelegate cb);
    }
}
