using RabbitMQ.Client;
using System;
using System.Text;
using Newtonsoft.Json;

namespace TrackerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            ConnectionFactory factory = new ConnectionFactory() { HostName = "localhost" };
            using (IConnection connection = factory.CreateConnection())
            using (IModel channel = connection.CreateModel())
            using (TrackerService trackerService = new TrackerService())
            {
                channel.ExchangeDeclare("TrackerService", "fanout");
                trackerService.NewPoseUpdate += (sender, data) =>
                {
                    string json = JsonConvert.SerializeObject(data);
                    channel.BasicPublish("TrackerService", string.Empty, null, Encoding.UTF8.GetBytes(json));
                };
                Console.WriteLine("press enter to exit");
                Console.ReadLine();
            }
            Console.WriteLine("Shutdown complete");
            //Console.WriteLine("Start");
            //TrackerService trackerService = new TrackerService();
            //Console.WriteLine("press enter to exit");
            //Console.ReadLine();
            //trackerService.Dispose();
            //Console.WriteLine("Shutdown complete");
        }
    }
}
