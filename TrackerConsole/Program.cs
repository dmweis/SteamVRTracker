using System;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TrackerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            bool playAreaSent = false;
            Console.WriteLine("Starting");
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
            .WithTcpServer("mqtt.local", 1883) // Port is optional
            .Build();
            var client = mqttClient.ConnectAsync(options, CancellationToken.None).Result;
            using (TrackerService trackerService = new TrackerService())
            {
                trackerService.NewPoseUpdate += (sender, data) =>
                {
                    string json = JsonConvert.SerializeObject(data, serializerSettings);
                    Console.WriteLine(json);
                    var message = new MqttApplicationMessageBuilder()
                    .WithTopic("tracking/pose")
                    .WithPayload(json)
                    .WithAtMostOnceQoS()
                    .Build();
                    mqttClient.PublishAsync(message, CancellationToken.None).Wait();
                };
                while (!playAreaSent)
                {
                    if (trackerService.PlayArea != null)
                    {
                        var playArea = trackerService.PlayArea;
                        string json = JsonConvert.SerializeObject(playArea, serializerSettings);
                        Console.WriteLine(json);
                        var message = new MqttApplicationMessageBuilder()
                        .WithTopic("tracking/play_area")
                        .WithPayload(json)
                        .WithAtMostOnceQoS()
                        .WithRetainFlag()
                        .Build();
                        mqttClient.PublishAsync(message, CancellationToken.None).Wait();
                        playAreaSent = true;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                Console.WriteLine("press enter to exit");
                Console.ReadLine();
            }
            Console.WriteLine("Shutdown complete");
        }
    }
}
