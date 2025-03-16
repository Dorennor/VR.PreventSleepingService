using System.ServiceProcess;

namespace VR.PreventSleepingService.Service
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServiceBase.Run(new PreventSleepingService());
        }
    }
}