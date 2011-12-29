﻿using System;

namespace Agents.MessageBusExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var processFactory = new ProcessFactory();
            processFactory.BuildProcess(
                process =>
                    {
                        process.OnShutdown(processFactory.Dispose);
                        process.MessageBus.Subscribe<string>(
                            "/shutdown/signal", (m, c) =>
                                    {
                                        Console.WriteLine("Recived {0}", m);
                                        c.Response("agree");
                                    });
                        processFactory.BuildProcess(
                            process2 =>
                                {
                                    process2.MessageBus.Publish("/shutdown/signal", "kill")
                                        .ExpectMessage((message, context) => process.Shutdown());
                                    Console.WriteLine("Sending kill");
                                });
                    });
        }
    }
}