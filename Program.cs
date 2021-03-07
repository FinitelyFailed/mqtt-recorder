using System;

namespace mqtt_recorder
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display title as the C# console calculator app.
            Console.WriteLine("MQTT recorder\r");
            Console.WriteLine("------------------------\n");

            App app = new App(); 

            app.Run(args).Wait();

            Console.WriteLine("Done\n");
        }
    }
}
