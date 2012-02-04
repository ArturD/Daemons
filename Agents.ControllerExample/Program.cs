using System;
using Daemons;

namespace Agents.ControllerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var initBarrier = new Barrier(2);
            var manager = DaemonConfig.Default()
                .RegisterServiceInstance(initBarrier)
                .BuildManager();
            manager.SpawnWithReactor<ClientDaemonReactor>();
            manager.SpawnWithReactor<PrinterDaemonReactor>();
            Console.ReadLine();
        }

    }
}
