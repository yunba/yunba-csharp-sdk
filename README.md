yunba-csharp-sdk
================

This repository is based on MqttDotNet. Please visit:

http://github.com/stevenlovegrove/MqttDotNet

http://www.doc.ic.ac.uk/~sl203/

for information about MqttDotNet

## Dependency
.NET Framework 4.5

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

[1]: https://github.com/yunba/yunba-csharp-sdk/tree/master/packages/Newtonsoft.Json.6.0.4/lib/net45
[2]: http://yunba.io/account/
