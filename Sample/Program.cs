using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MqttLib;

namespace Sample
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
                Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " Appkey");
				return;
			}

			Console.WriteLine("Starting MqttDotNet sample program.");
			
			Program prog = new Program(args[0]);
			prog.Start();

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            Console.WriteLine("The available commands(press 'q' to exits): ");
            Console.WriteLine("1. get the client's alias");
            Console.WriteLine("2. get the client's topics");
            Console.WriteLine("3. get a specific client's topics");
            Console.WriteLine("4. get a specific topic's alias list");
            Console.WriteLine("5. get a specific client's state");
            Console.WriteLine("6. set the client's alias");
            Console.WriteLine("7. subscribe the topic");
            Console.WriteLine("8. unsubscribe the topic");
            Console.WriteLine("9. publish message");
            Console.WriteLine("a. publish message to alias");
            Console.WriteLine("b. publish2 message");
            Console.WriteLine("c. publish2 message to alias");
            Console.WriteLine();

            Console.Write("Enter the commond id: ");

            char input;
            while ((input = Console.ReadKey().KeyChar) != 'q')
            {
                Console.WriteLine();

                switch (input)
                {
                    case '1':
                        {
                            Console.WriteLine("Getting Alias...");

                            _client.GetAlias(delegate(object sender, ExtendedAckArrivedArgs e)
                            {
                                Console.WriteLine("Alias: " + e.Payload);
                                Console.Write("Enter the commond id: ");
                            });
                        }
                        break;
                    case '2':
                        {
                            Console.WriteLine("Getting topics...");

                            _client.GetTopicList(delegate(object sender, ExtendedAckArrivedArgs e)
                            {
                                Console.WriteLine("Topics: " + e.Payload);
                                Console.Write("Enter the commond id: ");
                            });
                        }
                        break;
                    case '3':
                        {
                            Console.Write("The client alias: ");
                            string a = Console.ReadLine();

                            Console.WriteLine("Getting topics...");

                            _client.GetTopicList(a, delegate(object sender, ExtendedAckArrivedArgs e)
                            {
                                Console.WriteLine("Topics: " + e.Payload);
                                Console.Write("Enter the commond id: ");
                            });
                        }
                        break;
                    case '4':
                        {
                            Console.Write("The topic name: ");
                            string a = Console.ReadLine();

                            Console.WriteLine("Getting AliasList...");

                            _client.GetAliasList(a, delegate(object sender, ExtendedAckArrivedArgs e)
                            {
                                Console.WriteLine("AliasList: " + e.Payload);
                                Console.Write("Enter the commond id: ");
                            });
                        }
                        break;
                    case '5':
                        {
                            Console.Write("The client alias: ");
                            string a = Console.ReadLine();

                            Console.WriteLine("Getting state...");

                            _client.GetState(a, delegate(object sender, ExtendedAckArrivedArgs e)
                            {
                                Console.WriteLine("State: " + e.Payload);
                                Console.Write("Enter the commond id: ");
                            });
                        }
                        break;
                    case '6':
                        {
                            Console.Write("The client alias: ");
                            string a = Console.ReadLine();

                            _client.SetAlias(a);
                        }
                        break;
                    case '7':
                        {
                            Console.Write("The topic name: ");
                            string a = Console.ReadLine();

                            _client.Subscribe(a, QoS.AtLeastOnce);
                        }
                        break;
                    case '8':
                        {
                            Console.Write("The topic name: ");
                            string a = Console.ReadLine();

                            string[] ts = { a };
                            _client.Unsubscribe(ts);
                        }
                        break;
                    case '9':
                        {
                            Console.Write("The topic name: ");
                            string a = Console.ReadLine();

                            Console.Write("The message: ");
                            string m = Console.ReadLine();

                            _client.Publish(a, m, QoS.AtLeastOnce, false);
                        }
                        break;
                    case 'a':
                        {
                            Console.Write("The alias name: ");
                            string a = Console.ReadLine();

                            Console.Write("The message: ");
                            string m = Console.ReadLine();

                            _client.PublishToAlias(a, m, QoS.AtLeastOnce, false);
                        }
                        break;
                    case 'b':
                        {
                            Console.Write("The topic name: ");
                            string a = Console.ReadLine();

                            Console.Write("The message: ");
                            string m = Console.ReadLine();

                            _client.Publish2(a, m, QoS.AtLeastOnce, 30, "");
                        }
                        break;
                    case 'c':
                        {
                            Console.Write("The alias name: ");
                            string a = Console.ReadLine();

                            Console.Write("The message: ");
                            string m = Console.ReadLine();

                            _client.Publish2Alias(a, m, QoS.AtLeastOnce, 30, "");
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown commond id");
                        Console.Write("Enter the commond id: ");
                        break;
                }
            }

            Console.WriteLine();

			prog.Stop();
		}

		static IMqtt _client;

        Program(string appkey)
		{
            Console.WriteLine("Initialize the client with the appkey: " + appkey + "\n");

            try
            {
                // Instantiate client using MqttClientFactory with appkey
                _client = MqttClientFactory.CreateClientWithAppkey(appkey);
            }
            catch(Exception)
            {
                Console.WriteLine("Instantiate the client failed. Please check your network and app key, then try it again.");
                Environment.Exit(0);
            }

            // Enable auto reconnect
            _client.AutoReconnect = true;

			// Setup some useful client delegate callbacks
			_client.Connected += new ConnectionDelegate(client_Connected);
			_client.ConnectionLost += new ConnectionDelegate(_client_ConnectionLost);
			_client.PublishArrived += new PublishArrivedDelegate(client_PublishArrived);
            _client.Published += new CompleteDelegate(_client_Published);
            _client.Subscribed += new CompleteDelegate(_client_Subscribed);
            _client.Unsubscribed += new CompleteDelegate(_client_Unsubscribed);
		}

		void Start()
		{
			Console.WriteLine("Client connecting\n");
			_client.Start(true);
		}

		void Stop()
		{
			if (_client.IsConnected)
			{
				Console.WriteLine("Client disconnecting\n");
				_client.Stop();
				Console.WriteLine("Client disconnected\n");
			}
		}

		void client_Connected(object sender, EventArgs e)
		{
			Console.WriteLine("Client connected\n");

            /*
            JObject opts = new JObject();
            JObject apn_json = new JObject();
            JObject aps = new JObject();
            aps.Add("sound", "bingbong.aiff");
            aps.Add("badge", 9);
            aps.Add("alert", "msg from .net");
            apn_json.Add("aps", aps);
            opts.Add("apn_json", apn_json);

            _client.Publish2("pubtest2", "ÖÐÎÄ", QoS.AtLeastOnce, 0, JsonConvert.SerializeObject(opts));
             */
		}

		void _client_ConnectionLost(object sender, EventArgs e)
		{
			Console.WriteLine("Client connection lost\n");
		}

		void RegisterOurSubscriptions()
		{
            Console.WriteLine("Subscribing to pubtest\n");
            _client.Subscribe("pubtest", QoS.AtLeastOnce);
		}

        static void PublishSomething(string topic, string msg)
		{
			Console.WriteLine("Publishing on " + topic + ": " + msg + "\n");
            _client.Publish(topic, msg, QoS.AtLeastOnce, false);

            Console.WriteLine("Publishing to alias test2: " + msg + "\n");
            _client.PublishToAlias("test2", msg, QoS.AtLeastOnce, false);
		}

		bool client_PublishArrived(object sender, PublishArrivedArgs e)
		{
			Console.WriteLine("Received Message");
			Console.WriteLine("Topic: " + e.Topic);
			Console.WriteLine("Payload: " + e.Payload);
			Console.WriteLine();
			return true;
		}

        void _client_Published(object sender, CompleteArgs e)
        {
            Console.WriteLine("Received publish ack");
            Console.WriteLine("message id: " + e.MessageID);
            Console.WriteLine();
        }

        void _client_Subscribed(object sender, CompleteArgs e)
        {
            Console.WriteLine("Received subscribe ack");
            Console.WriteLine("message id: " + e.MessageID);
            Console.WriteLine();
        }

        void _client_Unsubscribed(object sender, CompleteArgs e)
        {
            Console.WriteLine("Received unsubscribe ack");
            Console.WriteLine("message id: " + e.MessageID);
            Console.WriteLine();
        }
	}

}
