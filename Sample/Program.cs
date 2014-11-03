using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using MqttLib;

namespace Sample
{
	class Program
	{
        static int count = 0;

		static void Main(string[] args)
		{
			if (args.Length != 1)
			{
                Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " Appkey");
				return;
			}

			Console.WriteLine("Starting MqttDotNet sample program.");
			Console.WriteLine("Press any key to stop\n");
			
			Program prog = new Program(args[0]);
			prog.Start();

            while(true)
            {
                Thread.Sleep(2000);

                if (_client.IsStopped)
                {
                    PublishSomething("pubtest", "message " + (++count));
                }
            }

			Console.ReadKey();
			prog.Stop();
		}

		static IMqtt _client;

        Program(string appkey)
		{
            Console.WriteLine("Initialize the client with the appkey: " + appkey + "\n");

			// Instantiate client using MqttClientFactory with appkey
            _client = MqttClientFactory.CreateClientWithAppkey(appkey);

            // Enable auto reconnect
            _client.AutoReconnect = true;

			// Setup some useful client delegate callbacks
			_client.Connected += new ConnectionDelegate(client_Connected);
			_client.ConnectionLost += new ConnectionDelegate(_client_ConnectionLost);
			_client.PublishArrived += new PublishArrivedDelegate(client_PublishArrived);
		}

		void Start()
		{
			Console.WriteLine("Client connecting\n");
			_client.Start(true);
		}

		void Stop()
		{
			if (_client.IsStopped)
			{
				Console.WriteLine("Client disconnecting\n");
				_client.Stop();
				Console.WriteLine("Client disconnected\n");
			}
		}

		void client_Connected(object sender, EventArgs e)
		{
			Console.WriteLine("Client connected\n");
			RegisterOurSubscriptions();
            PublishSomething("pubtest", "connection is ok.");
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
		}

		bool client_PublishArrived(object sender, PublishArrivedArgs e)
		{
			Console.WriteLine("Received Message");
			Console.WriteLine("Topic: " + e.Topic);
			Console.WriteLine("Payload: " + e.Payload);
			Console.WriteLine();
			return true;
		}

	}

}
