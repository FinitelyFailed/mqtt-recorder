using System;
using System.IO;
using System.Threading.Tasks;
using Mqtt;

namespace Sender 
{
    public class SenderApp : ISubApp 
    {
        private string _input;
        //private MqttBroker _mqttBroker;
        private MqttService _mqttService;

        public SenderApp(string input)
        {
            _input = input;

            _mqttService = new MqttService("localhost", "#");
        }

        public async Task StartAsync()
        {
            await _mqttService.Connect();
            Console.WriteLine("Will send a recording over local MQTT broker, will use localhost:1883 ...");

            //Console.WriteLine($"Local MQTT Broker running at localhost:1883. Press any key when ready.");
            //Console.WriteLine($"Local MQTT Broker running at localhost:1883. Press any key when ready.");
            //Console.ReadKey();
            Console.WriteLine($"Starting to send messages soon ...");

            double currentTime = 0.0;

            using (FileStream stream = File.Open(_input, FileMode.Open))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                string line = streamReader.ReadLine();
                while (line != null)
                {
                    string[] parts = line.Split(',', 3);

                    if (parts.Length != 3) 
                    {
                        Console.WriteLine("ERROR: Malformed input recording.");
                        continue;
                    }

                    double time = double.Parse(parts[0]);
                    string topic = parts[1];
                    string message = parts[2];

                    int waitTime = (int)((time - currentTime) * 1000);                
                    if (waitTime > 0 && currentTime > 0) 
                    {
                        await Task.Delay(waitTime);                    
                    }
                    currentTime = time;

                    await PublishMessage(topic, message);

                    line = streamReader.ReadLine();
                }
                
            }
        }

        public Task StopAsync()
        {
            return Task.FromResult(true);
        }

        private async Task PublishMessage(string topic, string message)
        {
            Console.WriteLine($"Publishing topic: {topic} message: {message}");
            await _mqttService.PublishAsync(topic, message);
        }
    }
}