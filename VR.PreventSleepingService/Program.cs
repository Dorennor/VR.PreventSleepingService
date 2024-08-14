using System.ServiceProcess;

namespace VR.PreventSleepingService
{
    public class Program
    {
        private static void Main(string[] args)
        {
            ServiceBase.Run(new PreventSleepingService());
        }
    }
}