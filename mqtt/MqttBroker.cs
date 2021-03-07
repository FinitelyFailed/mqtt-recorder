using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Protocol;


namespace Mqtt
{
    public class MqttBroker
    {
        private IMqttServer _mqttServer;

        public MqttBroker()
        {
            // Start a MQTT server.
            _mqttServer = new MqttFactory().CreateMqttServer();
        }

        public async Task StartAsync()
        {
            MqttServerOptionsBuilder optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(1884)
                .WithStorage(new RetainedMessageHandler())
                .WithConnectionValidator(c =>
                {
                    if (c.ClientId.Length < 10)
                    {
                        c.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                        return;
                    }

                    if (c.Username != "mySecretUser")
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    if (c.Password != "mySecretPassword")
                    {
                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                        return;
                    }

                    c.ReasonCode = MqttConnectReasonCode.Success;
                });

            IMqttServerOptions options = optionsBuilder.Build();

            await _mqttServer.StartAsync(options);
        }

        public async Task StopAsync()
        {
            await _mqttServer.StopAsync();
        }

        public void Publish(string topic, string message)
        {
            MqttApplicationMessage mqttMessage = new MqttApplicationMessageBuilder()
                                .WithTopic(topic)
                                .WithPayload(message)
                                .WithExactlyOnceQoS()
                                .WithRetainFlag()
                                .Build();
            _mqttServer.PublishAsync(mqttMessage, CancellationToken.None);
        }

        // The implementation of the storage:
        // This code uses the JSON library "Newtonsoft.Json".
        public class RetainedMessageHandler : IMqttServerStorage
        {
            private IList<MqttApplicationMessage> _messages = new List<MqttApplicationMessage>();

            public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
            {
                _messages = messages;
                return Task.FromResult(0);
            }

            public Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
            {
                return Task.FromResult(_messages);
            }
        }
    }
}