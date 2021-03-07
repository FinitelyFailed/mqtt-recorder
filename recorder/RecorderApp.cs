using System;
using System.IO;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using Mqtt;

namespace Recorder 
{
    public class RecorderApp : ISubApp
    {
        public bool IsRunning { get; private set; }
        private MqttService _mqttService;
        private string _output;
        private StreamWriter _logWriter;
        private DateTime _startRecordTime;

        public RecorderApp(MqttService mqttService, string output)
        {
            _mqttService = mqttService;
            _output = output;
            if (string.IsNullOrEmpty(_output)) 
            {
                _output = "recording";
            }        
        }

        public async Task StartAsync()
        {
            Console.WriteLine($"Will record MQTT stream from url: {_mqttService.Url} topic: {_mqttService.Topic} ...");

            IsRunning = true;
            _startRecordTime = DateTime.UtcNow;

            //string logPath = Path.GetTempFileName();
            FileStream logFile = File.Create(_output);
            _logWriter = new StreamWriter(logFile);

            IObserver<MqttMessage> messageObserver = Observer.Create<MqttMessage>(
                message => RecordMessage(message),
                ex => Console.WriteLine("$OnError: {ex.Message}"),
                () => Console.WriteLine("OnCompleted"));
            _mqttService.Message.Subscribe(messageObserver);

            //logWriter.Dispose();

            await _mqttService.Connect();
        }

        public async Task StopAsync()
        {
            IsRunning = false;

            await _mqttService.Disconnect();
        }

        private void RecordMessage(MqttMessage message) 
        {
            double offset = (DateTime.UtcNow - _startRecordTime).TotalSeconds;

            _logWriter?.WriteLine($"{offset},{message.Topic},{message.Message}");
            _logWriter?.Flush();
        }
    }
}