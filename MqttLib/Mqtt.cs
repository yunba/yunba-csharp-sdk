using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MqttLib.Core.Messages;
using MqttLib.Core;
using MqttLib;
//using System.Timers;
using System.Threading;
using System.Diagnostics;
using MqttLib.Logger;
using MqttLib.MatchTree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MqttLib
{
	internal class Mqtt : IMqtt, IMqttShared
	{

		#region Member Variables

		private StreamManager manager = null;
		private QoSManager qosManager = null;
		private IPersistence _store = null;

		private TopicTree<PublishArrivedDelegate> topicTree = null;

		private string _clientID;
		private string _username;
		private string _password;
		private ushort _keepAlive = 30;
		private Timer keepAliveTimer = null;
		private DateTime EPOCH = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
		private bool isActiveStop = true;
		private bool autoReconnect = false;
		private int autoReconnectTimeout = 500;        // ms
		private MqttConnectMessage connectMsg;
		private Hashtable extendAckCallBacks = new Hashtable();
		private Random messageidRandom = new Random();

		#endregion

		#region IMqtt Members

		public ushort KeepAliveInterval
		{
			get { return _keepAlive; }
			set { _keepAlive = value; }
		}

		public long ResendInterval
		{
			get { return qosManager.ResendInterval; }
			set { qosManager.ResendInterval = value; }
		}

		public bool AutoReconnect
		{
			get { return autoReconnect; }
			set { autoReconnect = value; }
		}

		public int AutoReconnectTimeout
		{
			get { return autoReconnectTimeout; }
			set { autoReconnectTimeout = value; }
		}

		public Mqtt(string connString, string clientID, string username, string password, IPersistence store)
		{
			_store = store;
			qosManager = new QoSManager(_store);
			manager = new StreamManager(connString, qosManager);
			qosManager.MessageReceived += new QoSManager.MessageReceivedDelegate(qosManager_MessageReceived);
			_clientID = clientID;
			_username = username;
			_password = password;

			this.ConnectionLost += Mqtt_ConnectionLost;
		}

		void Mqtt_ConnectionLost(object sender, EventArgs e)
		{
			extendAckCallBacks.Clear();

			if (!isActiveStop)
				DoReconnect();
		}

		void tmrCallback(object args)
		{
			try
			{
				manager.SendMessage(new MqttPingReqMessage());
			}
			catch (Exception e)
			{
				Log.Write(LogLevel.ERROR, e.ToString());
				// We've probably lost a connection. The time will be cancelled when we are
				// notified by the stream manager. In the mean time, just ignore the Exception.
			}
		}

		void qosManager_MessageReceived(object sender, MqttMessageReceivedEventArgs e)
		{
			if (e.Message == null)
			{
				//a null message means we have disconnected from the broker
				OnConnectionLost(new EventArgs());
				return;
			}

			switch (e.Message.MsgType)
			{
				case MessageType.CONNACK:
					var connack = ((MqttConnackMessage)e.Message);
					if (connack.Response == MqttConnectionResponse.Accepted)
						OnConnected(new EventArgs());
					else
						OnConnectionLost(new MqttConnackEventArgs(connack.Response));
					break;
				case MessageType.DISCONNECT:
					break;
				case MessageType.PINGREQ:
					manager.SendMessage(new MqttPingRespMessage());
					break;
				case MessageType.PUBACK:
					MqttPubackMessage puback = (MqttPubackMessage)e.Message;
					OnPublished(new CompleteArgs(puback.AckID));
					break;
				case MessageType.PUBCOMP:
					break;
				case MessageType.PUBLISH:
					MqttPublishMessage m = (MqttPublishMessage)e.Message;
					OnPublishArrived(m);
					break;
				case MessageType.PUBREC:
					break;
				case MessageType.PUBREL:
					MqttPubrelMessage pubrel = (MqttPubrelMessage)e.Message;
					OnPublished(new CompleteArgs(pubrel.AckID));
					break;
				case MessageType.SUBACK:
					MqttSubackMessage m1 = (MqttSubackMessage)e.Message;
					OnSubscribed(new CompleteArgs(m1.AckID));
					break;
				case MessageType.UNSUBACK:
					MqttUnsubackMessage m2 = (MqttUnsubackMessage)e.Message;
					OnUnsubscribed(new CompleteArgs(m2.AckID));
					break;
				case MessageType.EXTENDEDACK:
					MqttExtendedackMessage mm = (MqttExtendedackMessage)e.Message;
					if (mm.CommondId == 8)        // publish2 ack or publish2alias ack
						OnPublish2ed(mm);
					else
						OnExtendedAckArrived(new ExtendedAckArrivedArgs(mm.AckID, mm.CommondId, mm.Status, mm.Payload));
					break;
				case MessageType.PINGRESP:
					break;
				case MessageType.UNSUBSCRIBE:
				case MessageType.CONNECT:
				case MessageType.SUBSCRIBE:
				default:
					throw new Exception("Unsupported Message Type");

			}
		}

		public void Start()
		{
			DoConnect(new MqttConnectMessage(
			  _clientID, _username, _password, _keepAlive, false
			));
		}

		public void Start(string willTopic, QoS willQoS, MqttPayload willMsg, bool willRetain)
		{
			DoConnect(new MqttConnectMessage(
			  _clientID, _username, _password, _keepAlive, willTopic, willMsg.TrimmedBuffer, willQoS, willRetain, false
			));
		}

		public void Start(bool cleanStart)
		{
			DoConnect(new MqttConnectMessage(
			  _clientID, _username, _password, _keepAlive, cleanStart
			));
		}

		public void Start(string willTopic, QoS willQoS, MqttPayload willMsg, bool willRetain, bool cleanStart)
		{
			DoConnect(new MqttConnectMessage(
			  _clientID, _username, _password, _keepAlive, willTopic, willMsg.TrimmedBuffer, willQoS, willRetain, cleanStart
			));
		}

		private void DoConnect(MqttConnectMessage conmsg)
		{
			connectMsg = conmsg;
			isActiveStop = false;

			try
			{
				manager.Connect();
				manager.SendMessage(conmsg);
				manager.WaitForResponse();
				TimerCallback callback = new TimerCallback(tmrCallback);
				// TODO: Set Keep Alive interval and keepAlive time as property of client
				int keepAliveInterval = 1000 * _keepAlive;
				keepAliveTimer = new Timer(callback, null, keepAliveInterval, keepAliveInterval);
			}
			catch (Exception e)
			{
				throw new MqttBrokerUnavailableException("Unable to connect to the broker", e);
			}
		}

		public void Stop()
		{
			isActiveStop = true;
			extendAckCallBacks.Clear();

			manager.SendMessage(new MqttDisconnectMessage());
			if (keepAliveTimer != null)
			{
				keepAliveTimer.Dispose();
				keepAliveTimer = null;
			}
			manager.Disconnect();

		}

		public ulong Publish(string topic, MqttPayload payload, QoS qos, bool retained)
		{
			if (manager.IsConnected)
			{
				// Reset the PINGREQ timer as this publish will reset the server's counter
				if (keepAliveTimer != null)
				{
					int kmillis = 1000 * _keepAlive;
					keepAliveTimer.Change(kmillis, kmillis);
				}
				ulong messID = MessageID;
				manager.SendMessage(new MqttPublishMessage(messID, topic, payload.TrimmedBuffer, qos, retained));
				return messID;
			}
			else
			{
				throw new MqttNotConnectedException("You need to connect to a broker before trying to Publish");
			}
		}

		public ulong Publish(MqttParcel parcel)
		{
			return Publish(parcel.Topic, parcel.Payload, parcel.Qos, parcel.Retained);
		}

		public ulong Publish2(string topic, MqttPayload payload, QoS qos, int ttl, string apn_json)
		{
			if (manager.IsConnected)
			{
				ulong messID = MessageID;

				manager.SendMessage(new MqttExtendedackMessage(messID, topic, payload.TrimmedBuffer, qos, ttl, apn_json));
				return messID;
			}
			else
			{
				throw new MqttNotConnectedException("You need to connect to a broker before trying to Publish");
			}
		}

		public ulong PublishToAlias(string alias, MqttPayload payload, QoS qos, bool retained)
		{
			return Publish(",yta/" + alias, payload, qos, retained);
		}

		public ulong Publish2Alias(string alias, MqttPayload payload, QoS qos, int ttl, string apn_json)
		{
			return Publish2(",yta/" + alias, payload, qos, ttl, apn_json);
		}

		public ulong Subscribe(Subscription[] subscriptions)
		{
			if (manager.IsConnected)
			{
				ulong messID = MessageID;
				manager.SendMessage(new MqttSubscribeMessage(messID, subscriptions));
				return messID;
			}
			else
			{
				throw new MqttNotConnectedException("You need to connect to a broker before trying to Publish");
			}
		}

		public ulong Subscribe(Subscription subscription)
		{
			return Subscribe(new Subscription[] { subscription });
		}

		public ulong Subscribe(string topic, QoS qos)
		{
			return Subscribe(new Subscription(topic, qos));
		}

		public ulong Unsubscribe(string[] topics)
		{
			if (manager.IsConnected)
			{
				ulong messID = MessageID;
				manager.SendMessage(new MqttUnsubscribeMessage(messID, topics));
				return messID;
			}
			else
			{
				throw new MqttNotConnectedException("You need to connect to a broker before trying to Publish");
			}
		}

		public ulong SetAlias(string alias)
		{
			return Publish(",yali", alias, QoS.AtLeastOnce, false);
		}

		public void GetAlias(ExtendedAckArrivedDelegate cb)
		{
			SendExtendMessage(1, "", cb);
		}

		public void GetTopicList(ExtendedAckArrivedDelegate cb)
		{
			SendExtendMessage(3, "", cb);
		}

		public void GetTopicList(string alias, ExtendedAckArrivedDelegate cb)
		{
			SendExtendMessage(3, alias, cb);
		}

		public void GetAliasList(string topic, ExtendedAckArrivedDelegate cb)
		{
			SendExtendMessage(5, topic, cb);
		}

		public void GetState(string alias, ExtendedAckArrivedDelegate cb)
		{
			SendExtendMessage(9, alias, cb);
		}

		public bool IsConnected
		{
			get
			{
				return manager.IsConnected;
			}
		}

		public event PublishArrivedDelegate PublishArrived;

		public event CompleteDelegate Published;

		public event CompleteDelegate Subscribed;

		public event CompleteDelegate Unsubscribed;

		public event ConnectionDelegate ConnectionLost;

		public event ConnectionDelegate Connected;

		#endregion

		#region IMqttSharedSubscriber Members

		public void Subscribe(Subscription subscription, PublishArrivedDelegate subscriber)
		{
			if (topicTree == null)
			{
				topicTree = new TopicTree<PublishArrivedDelegate>();
			}

			topicTree.Add(subscription.Topic, subscriber);

			// TODO: Check if we're already subscribed.
			Subscribe(subscription);
		}

		public void Unsubscribe(string topic, PublishArrivedDelegate subscriber)
		{
			topicTree.Remove(topic, subscriber);

			// TODO: Check if this is the last subscriber
			Unsubscribe(new string[] { topic });
		}

		#endregion

		#region Event Raising functions

		protected void OnPublishArrived(MqttPublishMessage m)
		{
			bool accepted = false;
			PublishArrivedArgs e = new PublishArrivedArgs(m.Topic, m.Payload, m.Retained, m.QualityOfService);

			if (PublishArrived != null)
			{
				try
				{
					accepted |= PublishArrived(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}

			if (topicTree != null)
			{
				List<PublishArrivedDelegate> subscribers = topicTree.CollectMatches(new Topic(m.Topic));
				foreach (PublishArrivedDelegate pad in subscribers)
				{
					try
					{
						accepted |= pad(this, e);
					}
					catch (Exception ex)
					{
						MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
					}
				}
			}

			if (m.QualityOfService > QoS.BestEfforts)
			{
				qosManager.PublishAccepted(m.MessageID, accepted);
			}

		}

		protected void OnPublished(CompleteArgs e)
		{
			if (Published != null)
			{
				try
				{
					Published(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}
		}

		protected void OnPublish2ed(MqttExtendedackMessage m)
		{
			OnPublished(new CompleteArgs(m.MessageID));
		}

		protected void OnExtendedAckArrived(ExtendedAckArrivedArgs e)
		{
			/*
            Console.WriteLine("Extended ack arrived");
            Console.WriteLine("Commond id: " + e.CommondID);
            Console.WriteLine("Commond status: " + e.Status);
            Console.WriteLine("Payload: " + e.Payload);
            Console.WriteLine();
            */

			if (extendAckCallBacks.Contains(e.MessageID))
			{
				((ExtendedAckArrivedDelegate)extendAckCallBacks[e.MessageID])(this, e);
				extendAckCallBacks.Remove(e.MessageID);
			}
		}

		protected void OnSubscribed(CompleteArgs e)
		{
			if (Subscribed != null)
			{
				try
				{
					Subscribed(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}
		}

		protected void OnUnsubscribed(CompleteArgs e)
		{
			if (Unsubscribed != null)
			{
				try
				{
					Unsubscribed(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}
		}

		protected void OnConnectionLost(EventArgs e)
		{
			if (keepAliveTimer != null)
			{
				keepAliveTimer.Dispose();
				keepAliveTimer = null;
			}

			if (ConnectionLost != null)
			{
				try
				{
					ConnectionLost(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}
		}

		protected void OnConnected(EventArgs e)
		{
			if (Connected != null)
			{
				try
				{
					Connected(this, e);
				}
				catch (Exception ex)
				{
					MqttLib.Logger.Log.Write(LogLevel.ERROR, "MqttLib: Uncaught exception from user delegate: " + ex.ToString());
				}
			}
		}



		#endregion

		private ulong MessageID
		{
			get
			{
				ulong ms = (ulong)Math.Round((DateTime.Now - EPOCH).TotalMilliseconds);
				ms = ms << 23;

				ulong id = ms | (uint)(messageidRandom.Next() & 0x7FFFFF);
				return id;
			}
		}

		private void DoReconnect()
		{
			while (autoReconnect)
			{
				// Log.Write(LogLevel.INFO, "Client reconnect.");

				try
				{
					DoConnect(connectMsg);
					break;
				}
				catch (Exception)
				{
					Thread.Sleep(autoReconnectTimeout);
				}
			}
		}

		private void SendExtendMessage(byte cmdID, string msg, ExtendedAckArrivedDelegate cb)
		{
			if (manager.IsConnected)
			{
				ulong messID = MessageID;

				manager.SendMessage(new MqttExtendedackMessage(messID, cmdID, msg));

				extendAckCallBacks.Add(messID, cb);
			}
			else
			{
				throw new MqttNotConnectedException("You need to connect to a broker before trying to Publish");
			}
		}

		#region pub/pubToAlias/pub2/pub2toAlias; /sub/unsub/subP/unsubP; setAlias with token

		public ulong PublishWithToken(string topic, string token, MqttPayload payload, QoS qos, bool retained)
		{
			string topicWithToken = ",yam" + token + "_" + topic;
			return Publish(topicWithToken, payload, qos, retained);
		}

		public ulong PublishToAliasWithToken(string alias, string token, MqttPayload payload, QoS qos, bool retained)
		{
			string AliasWithToken = ",yam" + token + "_" + ",yta/" + alias;
			return Publish(AliasWithToken, payload, qos, retained);
		}

		public ulong Publish2WithToken(string topic, string token, MqttPayload payload, QoS qos, int ttl, string apn_json)
		{ 
			string topicWithToken = ",yam" + token + "_" + topic;
			return Publish2(topicWithToken, payload, qos, ttl, apn_json);
		}

		public ulong Publish2ToAliasWithToken(string alias, string token, MqttPayload payload, QoS qos, int ttl, string apn_json)
		{ 
			string AliasWithToken = ",yam" + token + "_" + ",yta/" + alias;
			return Publish2(AliasWithToken, payload, qos, ttl, apn_json);
		}

		public ulong SubscribeWithToken(Subscription[] subscriptions, string token) 
		{
			List<Subscription> subscriptionsWithToken = new List<Subscription>();
			foreach (var subscription in subscriptions)
			{
				string topicWithToken = ",yam" + token + "_" + subscription.Topic;
				Subscription subscriptionWithToken = new Subscription(topicWithToken, subscription.QualityOfService);
				subscriptionsWithToken.Add(subscriptionWithToken);
			}
			Subscription[] SubscriptionsWithTokens = subscriptionsWithToken.ToArray();
			return Subscribe(SubscriptionsWithTokens);
		}

		public ulong SubscribeWithToken(Subscription subscription, string token)
		{
			string topicWithToken = ",yam" + token + "_" + subscription.Topic;
			Subscription SubWithToken = new Subscription(topicWithToken, subscription.QualityOfService);
			return Subscribe(new Subscription[] { SubWithToken });
		}

		public ulong SubscribeWithToken(string topic, string token, QoS qos)
		{
			return SubscribeWithToken(new Subscription(topic, qos), token);
		}

		public ulong UnsubscribeWithToken(string[] topics, string token)
		{
			List<string> topicsWithToken = new List<string>();
			foreach (var topic in topics)
			{
				string topicWithToken = ",yam" + token + "_" + topic;
				topicsWithToken.Add(topicWithToken);
			}
			string[] topicsWithTokens = topicsWithToken.ToArray();
			return Unsubscribe(topicsWithTokens);
		}

		public ulong SetAliasWithToken(string alias, string token)
		{
			string AliasWithToken = ",yam" + token + "_" + ",yali";
			return Publish(AliasWithToken, alias, QoS.AtLeastOnce, false);;
		}
		#endregion
	}
}