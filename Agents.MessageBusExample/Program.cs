using System;

namespace Agents.MessageBusExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var processFactory = new ProcessManager();
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
                                        .ExpectResponse((message, context) =>
                                                            {
                                                                Console.WriteLine("Response: " + message);
                                                                process.Shutdown();
                                                                processFactory.Dispose();
                                                            });
                                    Console.WriteLine("Sending kill");
                                });
                    });
        }
    }
}
