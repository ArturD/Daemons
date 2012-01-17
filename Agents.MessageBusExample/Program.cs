using System;

namespace Agents.MessageBusExample
{
    class Program
    {
        static void Main(string[] args)
        {
            //var processFactory = new ProcessManager();
            //processFactory.BuildProcess(
            //    process =>
            //        {
            //        // główny process
            //            process.OnShutdown(processFactory.Dispose);
            //            process.MessageBus.Subscribe<string>(
            //                "/shutdown/signal", (m, c) =>
            //                        {
            //                            Console.WriteLine("Recived {0}", m);
            //                            c.Response("agree");
            //                        });
									
            //            processFactory.BuildProcess(
            //                process2 =>
            //                    {
            //                        // proces prosi o pozwolenie na zamknięcie apliakcji 
            //                        process2.MessageBus.Publish("/shutdown/signal", "kill")
            //                            .ExpectResponse((message, context) =>
            //                                                {
            //                                                    Console.WriteLine("Response: " + message);
																
            //                                                    if(message == "agree") {
            //                                                        processFactory.Dispose();
            //                                                    }
            //                                                });
            //                        Console.WriteLine("Sending kill");
            //                    });
            //        });
        }
    }
}
