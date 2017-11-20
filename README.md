yunba-csharp-sdk
================

本仓库基于 MqttDotNet。更多相关信息，请参考：

http://github.com/stevenlovegrove/MqttDotNet

http://www.doc.ic.ac.uk/~sl203/

***提示*** **.net sdk 默认会把连接信息存在 Mqtt.dll.config 里面，在连接的时候优先读取缓存的信息，避免每次初始化的重新申请以加快速度。在打包之前，请先移除该文件内的登陆信息，这些登陆信息包括：`username`、`password`和`client_id`，均以 K-V 形式存在，sdk 没有找到这些消息的话就会重新去申请登陆信息。**

## 依赖

.NET Framework 2.0

[Newtonsoft.Json][1]


## 如何编译
1. 用 Visual Studio 2013 打开 MqttDotNet.sln 工程文件。
2. 编译 MqttLib 库
3. 编译 Sample 工程，进行测试

## 测试

```
Sample.exe YourYunbaAppkey
```

## 清除用户的注册信息
清除可执行路径下的 *.config 文件。

## API 说明

### CreateClientWithAppkey(string yunbaAppkey)
用你的 [Appkey][3] 创建一个客户端实例。

* `yunbaAppkey` 是用户 [在云巴 Portal 创建新应用][4] 后获取到的一串字符。

```C#
IMqtt client = MqttClientFactory.CreateClientWithAppkey("YourYunbaAppkey");
```

### Start()
初始化客户端。

```C#
client.Start();
```

### Subscribe(string topic, QoS qos)
订阅一个 [频道][5]。

* `topic` 是待订阅的频道的名称。只支持英文数字下划线，长度不超过 50 个字符。
* `qos` 是 [QoS][7] 等级，此处的 QoS 暂未实现，请填 QoS.AtLeastOnce。

```C#
client.Subscribe("topic_name", QoS.AtLeastOnce);
```

### Unsubscribe(string[] topics)
取消对某一个或多个 [频道][5] 的订阅。取消订阅后，不会再收到来自该频道的消息。

* `topics` 是待取消的频道数组。

```C#
string[] topics = {"topic_name"};
client.Unsubscribe(topics);
```

### Publish(string topic, MqttPayload payload, QoS qos, bool retained)
向某频道发布一条消息，并指定 QoS 等级。

* `topic` 是目标频道。只支持英文数字下划线，长度不超过 50 个字符。
* `payload` 是待发布的消息内容。
* `qos` 是 [QoS][7] 等级。
* `retained` 暂未实现，请填 false。

```C#
client.Publish("topic_name", "message_content", QoS.AtLeastOnce, false);
```

### SetAlias(string alias)
为客户端设置一个 [别名][6]。

* `alias` 是客户端的 [别名][6]。只支持英文数字下划线，长度不超过 50 个字符。

```C#
client.SetAlias("myname");
```

### GetAlias(ExtendedAckArrivedDelegate cb)
获取客户端的 [别名][6]。

* `cb` 是收到回应时调用的回调函数。

```C#
client.GetAlias(delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Alias: " + e.Payload);
});
```


### PublishToAlias(string alias, MqttPayload payload, QoS qos, bool retained)
向某个客户端 [别名][6] 发消息。

* `alias` 是客户端的 [别名][6]。只支持英文数字下划线，长度不超过 50 个字符。
* `payload` 是待发送的消息内容。
* `qos` 是 [QoS][7] 等级。
* `retained` 暂未实现，请填 false。

```C#
client.PublishToAlias("myname", "message_content", QoS.AtLeastOnce, false);
```

### GetTopicList(ExtendedAckArrivedDelegate cb)
获取客户端订阅的 [频道][5] 列表。

* `cb` 是收到回应时调用的回调函数。

```C#
client.GetTopicList(delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Topics: " + e.Payload);
});
```

### GetTopicList(string alias, ExtendedAckArrivedDelegate cb)
获取客户端订阅的 [频道][5] 列表。

* `alias` 是客户端的 [别名][6]。只支持英文数字下划线，长度不超过 50 个字符。
* `cb` 是收到回应时调用的回调函数。

```C#
client.GetTopicList("peer_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("Topics: " + e.Payload);
});
```

### GetAliasList(string topic, ExtendedAckArrivedDelegate cb)
获取某 [频道][5] 的所有订阅者的 [别名][6]。

* `topic` 是目标频道。只支持英文数字下划线，长度不超过 50 个字符。
* `cb` 是收到回应时调用的回调函数。

```C#
client.GetAliasList("topic_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("AliasList: " + e.Payload);
});
```

### GetState(string alias, ExtendedAckArrivedDelegate cb)
获取某个客户端的在线状态。

* `alias` 是客户端的 [别名][6]。只支持英文数字下划线，长度不超过 50 个字符。
* `cb` 是收到回应时调用的回调函数。

```C#
client.GetState("peer_name", delegate(object sender, ExtendedAckArrivedArgs e)
{
	Console.WriteLine("State: " + e.Payload);
});
```

### Publish2(string topic, MqttPayload payload, QoS qos, int ttl, string apn_json)
`Publish2` 是 `publish` 的升级版本，可带更多参数。

* `topic` 是目标频道。只支持英文数字下划线，长度不超过 50 个字符。
* `payload` 是待发布的消息内容。
* `qos` 是 [QoS][7] 等级。
* `ttl` 是消息在服务器上存储的时间，单位是秒（例如，“3600”代表1小时），默认值为 5 天，最大不超过 15 天。
* `apn_json` 是 APN 选项。

```C#
JObject apn_json = new JObject();
JObject aps = new JObject();

aps.Add("sound", "bingbong.aiff");
aps.Add("badge", 9);
aps.Add("alert", "msg from .net");
apn_json.Add("aps", aps);

client.Publish2("topic_name", "message_content", QoS.AtLeastOnce, 30, JsonConvert.SerializeObject(apn_json));
```

### Publish2Alias(string alias, MqttPayload payload, QoS qos, int ttl, string apn_json)
向某个 [别名][6] 发布消息，可带有部分选项。

* `alias` 是客户端的 [别名][6]。只支持英文数字下划线，长度不超过 50 个字符。
* `payload` 是待发布的消息内容。
* `qos` 是 [QoS][7] 等级。
* `ttl` 是消息在服务器上存储的时间，单位是秒（例如，“3600”代表1小时），默认值为 5 天，最大不超过 15 天。
* `apn_json` 是 APN 选项。

```C#
client.Publish2Alias("peer_name", "message_content", QoS.AtLeastOnce, 0, "");
```

## 事件
### Connected
连接成功建立。

```C#
client.Connected += new ConnectionDelegate(client_Connected);

void client_Connected(object sender, EventArgs e)
{
	Console.WriteLine("Client connected\n");
}
```

### ConnectionLost
连接断开。

```C#
client.ConnectionLost += new ConnectionDelegate(client_ConnectionLost);

void client_ConnectionLost(object sender, EventArgs e)
{
	Console.WriteLine("Client connection lost\n");
}
```

### PublishArrived
收到消息。

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

**声明：对于自行修改 sdk 而导致的问题，云巴恕不提供技术支持。如果对于 sdk 有个性化需求，可以联系商务定制开发：xieting@yunba.io**

[1]: https://github.com/yunba/yunba-csharp-sdk/tree/master/packages/Newtonsoft.Json.6.0.4/lib/net40
[2]: http://yunba.io/account/
[3]: https://github.com/yunba/kb/blob/master/AppKey.md
[4]: https://github.com/yunba/kb/blob/master/Portal.md#%E5%A6%82%E4%BD%95%E5%9C%A8%E4%BA%91%E5%B7%B4-portal-%E4%B8%8A%E5%88%9B%E5%BB%BA%E6%96%B0%E5%BA%94%E7%94%A8
[5]: https://github.com/yunba/kb/blob/master/%E9%A2%91%E9%81%93%E5%92%8C%E5%88%AB%E5%90%8D.md#%E9%A2%91%E9%81%93topic
[6]: https://github.com/yunba/kb/blob/master/%E9%A2%91%E9%81%93%E5%92%8C%E5%88%AB%E5%90%8D.md#%E5%88%AB%E5%90%8Dalias
[7]: https://github.com/yunba/kb/blob/master/QoS.md
