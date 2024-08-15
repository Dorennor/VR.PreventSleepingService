using System.ServiceProcess;

namespace VR.PreventSleepingService
{
    public class PreventSleepingService : ServiceBase
    {
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            PowerUtilities.PowerUtilities.PreventPowerSave();

            Console.ReadLine();
        }
    }
}