using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace Mqtt
{
    public class MqttService
    {
        public IObservable<MqttMessage> Message => _messagesSubject.AsObservable();

        private IMqttClient _mqttClient;
        public string Url { get; private set; }
        public string Topic { get; private set; }
        private IMqttClientOptions _options;
        private Subject<MqttMessage> _messagesSubject = new Subject<MqttMessage>();

        public MqttService(string url, string topic)
        {
            Url = url;
            Topic = topic;
            _options = CreateOptions(url);

            // Create a new MQTT client.
            MqttFactory factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();      

            _mqttClient.UseConnectedHandler(async e =>
            {
                Console.WriteLine("### CONNECTED WITH SERVER ###");

                // Subscribe to a topic
                if (string.IsNullOrEmpty(topic))
                {
                    await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build());
                }

                Console.WriteLine("### SUBSCRIBED ###");
            });

            _mqttClient.UseDisconnectedHandler(async e =>
            {
                Console.WriteLine("### DISCONNECTED FROM SERVER ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _mqttClient.ConnectAsync(_options, CancellationToken.None); // Since 3.0.5 with CancellationToken
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");
                }
            });  

            _mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                //Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"{e.ApplicationMessage.Topic}");
                //Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                //Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                //Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                //Console.WriteLine();

                //Task.Run(() => _mqttClient.PublishAsync("hello/world"));
                MqttMessage mqttMessage = new MqttMessage() {
                    Topic = e.ApplicationMessage.Topic,
                    Message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload)//,
                    //QoS = e.ApplicationMessage.QualityOfServiceLevel,
                    //Retain = e.ApplicationMessage.Retain
                };

                _messagesSubject.OnNext(mqttMessage);
            });
        }

        public async Task Connect()
        {
            await _mqttClient.ConnectAsync(_options, CancellationToken.None); // Since 3.0.5 with CancellationToken            
        }

        public async Task Disconnect()
        {
            await _mqttClient.DisconnectAsync();
        }

        public async Task PublishAsync(string topic, string message)
        {
            MqttApplicationMessage mqttMessage = new MqttApplicationMessageBuilder()
                                .WithTopic(topic)
                                .WithPayload(message)
                                .WithExactlyOnceQoS()
                                .WithRetainFlag()
                                .Build();
            await _mqttClient.PublishAsync(mqttMessage, CancellationToken.None);
        }

        private IMqttClientOptions CreateOptions(string url)
        {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder();
            builder.WithClientId("MqttRecorder");
            if (url.StartsWith("ws")) 
            {
                builder.WithWebSocketServer(Url);
            }
            else
            {
                builder.WithTcpServer(url);                            
            }
            //.WithCredentials("bud", "%spencer%")
            //.WithTls()
            //.WithCleanSession()

            return builder.Build();
        }
    }
}