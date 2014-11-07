using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace MqttLib
{
  public interface IMqttPublisher
  {
    /// <summary>
    /// Publish a message to the MQTT message broker
    /// </summary>
    /// <param name="topic">Destination of message</param>
    /// <param name="payload">Message body</param>
    /// <param name="qos">QoS level</param>
    /// <param name="retained">Whether the message is retained by the broker</param>
    /// <returns>Message ID</returns>
    ulong Publish(string topic, MqttPayload payload, QoS qos, bool retained);

    /// <summary>
    /// Publish a message to the MQTT message broker
    /// </summary>
    /// <param name="parcel">Parcel containing destination topic, message body, QoS and if the message should be retained</param>
    /// <returns>Message ID</returns>
    ulong Publish(MqttParcel parcel);

    /// <summary>
    /// Publish a message to a specific client with alias
    /// </summary>
    /// <param name="alias">alias name</param>
    /// <param name="payload">Message body</param>
    /// <param name="qos">QoS level</param>
    /// <param name="retained">Whether the message is retained by the broker</param>
    /// <returns>Message ID</returns>
    ulong PublishToAlias(string alias, MqttPayload payload, QoS qos, bool retained);

    /// <summary>
    /// Fired when receipt of publication is confirmed
    /// </summary>
    event CompleteDelegate Published;
  }
}
