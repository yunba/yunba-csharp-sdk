yunba-csharp-sdk
================

This repository is based on MqttDotNet. Please visit:

http://github.com/stevenlovegrove/MqttDotNet

http://www.doc.ic.ac.uk/~sl203/

for information about MqttDotNet

## Dependency
.NET Framework 2.0

[Newtonsoft.Json][1]

## How to build
1. Open the included solution file using Visual Studio 2013: MqttDotNet.sln
2. Build the MqttLib project for your application
3. Build the Sample project for test

## Test
```
Sample.exe YourYunbaAppkey
```

## The basic APIs
### CreateClientWithAppkey(string yunbaAppkey)
Create a client instance using your yunba appkey.
* yunbaAppkey is a string associated with [YunBa account][2].

```C#
IMqtt client = MqttClientFactory.CreateClientWithAppkey("YourYunbaAppkey");
```

### Start()
Start up the client.

```C#
client.Start();
```

### Subscribe(string topic, QoS qos)
Subscribe a topic with a specific Qos level.
* topic is a string topic to subscribe to.
* qos is the granted qos level on it.

```C#
client.Subscribe("topic_name", QoS.AtLeastOnce);
```

### Unsubscribe(string[] topics)
Unsubscribe from topics.
* topics is an array of topics to unsubscribe from.

```C#
string[] topics = {"topic_name"};
client.Unsubscribe(topics);
```

### Publish(string topic, MqttPayload payload, QoS qos, bool retained)
Publish a message to a topic with a specific Qos level.
* topic is the topic to publish to.
* payload is the message to publish.
* qos is the qos level.
* retained is the retain flag.

```C#
client.Publish("topic_name", "message_content", QoS.AtLeastOnce, false);
```

### SetAlias(string alias)
Set an alias for the client.
* alias is a string.

```C#
client.SetAlias("myname");
```

### GetAlias(ExtendedAckArrivedDelegate cb)
Request the client's alias.
* cb is the callback function fired when the response arrived.

```C#
client.GetAlias(delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Alias: " + e.Payload);
});
```

### PublishToAlias(string alias, MqttPayload payload, QoS qos, bool retained)
Publish a message to a specific client with alias.
* alias is a string.
* payload is the message to publish.
* qos is the qos level.
* retained is the retain flag.

```C#
client.PublishToAlias("myname", "message_content", QoS.AtLeastOnce, false);
```

### GetTopicList(ExtendedAckArrivedDelegate cb)
Request the topics that the client subscribed.
* cb is the callback function fired when the response arrived.

```C#
client.GetTopicList(delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Topics: " + e.Payload);
});
```

### GetTopicList(string alias, ExtendedAckArrivedDelegate cb)
Request the topics that the alias client subscribed.
* alias is a string.
* cb is the callback function fired when the response arrived.

```C#
client.GetTopicList("peer_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Topics: " + e.Payload);
});
```

### GetAliasList(string topic, ExtendedAckArrivedDelegate cb)
Request a specific topic's alias.
* topic is the topic to fetch alias list to.
* cb is the callback function fired when the response arrived.

```C#
client.GetAliasList("topic_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("AliasList: " + e.Payload);
});
```

### GetState(string alias, ExtendedAckArrivedDelegate cb)
Request a specific client's state.
* alias is a string.
* cb is the callback function fired when the response arrived.

```C#
client.GetState("peer_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("State: " + e.Payload);
});
```

### Publish2(string topic, MqttPayload payload, QoS qos, int ttl, string apn_json)
Publish a message to a topic with some options.
* topic is the topic to publish to.
* payload is the message to publish.
* qos is the qos level.
* ttl is the time for message will be stored on server, in seconds. If set 0, the message will be forever stored on the server.
* apn_json is the APN options.

```C#
JObject opts = new JObject();
JObject apn_json = new JObject();
JObject aps = new JObject();

aps.Add("sound", "bingbong.aiff");
aps.Add("badge", 9);
aps.Add("alert", "msg from .net");
apn_json.Add("aps", aps);
opts.Add("apn_json", apn_json);

client.Publish2("topic_name", "message_content", QoS.AtLeastOnce, 30, JsonConvert.SerializeObject(opts));
```

### Publish2Alias(string alias, MqttPayload payload, QoS qos, int ttl, string apn_json)
Publish a message to a specific client with alias and some options
* alias is a string.
* payload is the message to publish.
* qos is the qos level.
* ttl is the time for message will be stored on server, in seconds. If set 0, the message will be forever stored on the server.
* apn_json is the APN options.

```C#
client.Publish2Alias("peer_name", "message_content", QoS.AtLeastOnce, 0, "");
```

## The basic events
### Connected
Fired when a connection is made.

```C#
client.Connected += new ConnectionDelegate(client_Connected);

void client_Connected(object sender, EventArgs e)
{
	Console.WriteLine("Client connected\n");
}
```

### ConnectionLost
Fired when the connection was lost.

```C#
client.ConnectionLost += new ConnectionDelegate(client_ConnectionLost);

void client_ConnectionLost(object sender, EventArgs e)
{
	Console.WriteLine("Client connection lost\n");
}
```

### PublishArrived
Fired when a message arrived.

```C#
client.PublishArrived += new PublishArrivedDelegate(client_PublishArrived);

bool client_PublishArrived(object sender, PublishArrivedArgs e)
{
	Console.WriteLine("Receive Message");
	Console.WriteLine("Topic: " + e.Topic);
	Console.WriteLine("Payload: " + e.Payload);
	return true;
}
```

[1]: https://github.com/yunba/yunba-csharp-sdk/tree/master/packages/Newtonsoft.Json.6.0.4/lib/net40
[2]: http://yunba.io/account/
