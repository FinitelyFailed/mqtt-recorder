using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Recorder;
using Sender;
using Mqtt;

public class App 
{
    private bool _isRunning = true;

    public async Task Run(string[] args)
    {
        ISubApp subApp = ParseInputs(args);

        if (subApp == null) 
        {
            return;
        }

        await subApp.StartAsync();
    }

    private ISubApp ParseInputs(string[] args) 
    {
        List<string> argList = args.ToList();

        if (argList.Count <= 0) 
        {
            Console.WriteLine("No arguments given.");
            return null;
        }

        if (argList.Contains("--help") || argList.Contains("-h")) 
        {
            PrintHelp();
            return null;
        }        

        if (argList.Contains("-record") || argList.Contains("-r")) {            
            argList.RemoveAt(0);
            return ParseRecordInput(argList.ToArray());
        } else if (argList.Contains("-send") || argList.Contains("-s")) {
            argList.RemoveAt(0);
            return ParseSendInput(argList.ToArray());
        }

        Console.WriteLine("ERROR: Invalid args, choose either -record or -send");

        return null;
    }

    private ISubApp ParseSendInput(string[] args)
    {
        string input = null;
        string url = null;

        for (int i = 0; i < args.Length; ++i)
        {
            switch (args[i]) {
                case "-input":
                    {
                        if (!ValidateArgWithParam(i, args)) { return null; }
                        ++i;
                        input = args[i];
                        break;
                    }
                    case "-url":
                    {
                        if (!ValidateArgWithParam(i, args)) { return null; }
                        ++i;
                        url = args[i];
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"WARNING: Argument {args[i]} does nothing.");
                        break;
                    }
            }
        }

        SenderApp senderApp = new SenderApp(input, url);
        return senderApp;
    }

    private ISubApp ParseRecordInput(string[] args)
    {
        List<string> argList = args.ToList();

        // if (argList.Contains("-c")) 
        // {
        //     if (!ParseConfig(argList)) 
        //     {
        //         return null;
        //     }
        // }

        string url = null;
        string topic = null;
        string output = null;

        for (int i = 0; i < args.Length; ++i)
        {
            switch (args[i]) {
                case "-url":
                    {
                        if (!ValidateArgWithParam(i, args)) { return null; }
                        ++i;
                        url = args[i];
                        break;
                    }
                case "-topic":
                    {
                        if (!ValidateArgWithParam(i, args)) { return null; }
                        ++i;
                        topic = args[i];
                        break;
                    }
                case "-output":
                    {
                        if (!ValidateArgWithParam(i, args)) { return null; }
                        ++i;
                        output = args[i];
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"WARNING: Argument {args[i]} does nothing.");
                        break;
                    }
            }
        }

        MqttService mqttService = new MqttService(url, topic);
        RecorderApp recorder = new RecorderApp(mqttService, output);

        return recorder;
    }


    private bool ValidateArgWithParam(int argIndex, string[] args)
    {
        int paramIndex = argIndex + 1;
        if ((paramIndex == args.Length) ||
            (args[paramIndex].StartsWith("-")))
        {
            Console.WriteLine("Error: -topic, No topic arg.");
            return false;                
        }

        return true;
    }

    private bool ParseConfig(List<string> argList) 
    {
        int index = argList.IndexOf("-c");
        int configFileIndex = index + 1;
        if (configFileIndex >= argList.Count) {
            Console.WriteLine("Error: -c, No config args.");
            return false;
        }

        string configFile = argList[configFileIndex];


        return true;
    }

    private void PrintHelp()
    {
        Console.WriteLine("TODO");
    }
}