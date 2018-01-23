using System;
using System.Collections.Generic;
using System.Text;
using MqttLib.Core;

namespace MqttLib
{
  public interface IMqttSubscriber
  {
    /// <summary>
    /// Subscribe to many topics
    /// </summary>
    /// <param name="subscriptions">Array of subscription objects</param>
    /// <returns>Message ID</returns>
    ulong Subscribe(Subscription[] subscriptions);

    /// <summary>
    /// Subscribe to a single topic
    /// </summary>
    /// <param name="subscription">A Subscription object</param>
    /// <returns>Message ID</returns>
    ulong Subscribe(Subscription subscription);

    /// <summary>
    /// Subscribe to a single topic
    /// </summary>
    /// <param name="topic">Name of topic to subscribe to</param>
    /// <param name="qos">QoS</param>
    /// <returns>Message ID</returns>
    ulong Subscribe(string topic, QoS qos);

    /// <summary>
    /// Unsubscribe from a topic
    /// </summary>
    /// <param name="topics">Topic Name</param>
    /// <returns>Message ID</returns>
    ulong Unsubscribe(string[] topics);
	ulong SubscribeWithToken(Subscription[] subscriptions, string token);
	ulong SubscribeWithToken(Subscription subscription, string token);
	ulong SubscribeWithToken(string topic, string token, QoS qos);
	ulong UnsubscribeWithToken(string[] topics, string token);

    /// <summary>
    /// Fired when the Topic the MQTT client is subscribed to receives a message
    /// </summary>
    event PublishArrivedDelegate PublishArrived;

    /// <summary>
    /// Fired when receipt of subscription is confirmed
    /// </summary>
    event CompleteDelegate Subscribed;

    /// <summary>
    /// Fired when receipt of unsubscribe is confirmed
    /// </summary>
    event CompleteDelegate Unsubscribed;
  }
}
