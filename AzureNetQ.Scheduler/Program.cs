using log4net.Config;
using Topshelf;

namespace AzureNetQ.Scheduler
{
    public class Program
    {
        static void Main()
        {
            XmlConfigurator.Configure();

            HostFactory.Run(hostConfiguration =>
            {
                hostConfiguration.RunAsLocalSystem();
                hostConfiguration.SetDescription("AzureNetQ.Scheduler");
                hostConfiguration.SetDisplayName("AzureNetQ.Scheduler");
                hostConfiguration.SetServiceName("AzureNetQ.Scheduler");

                hostConfiguration.Service<ISchedulerService>(serviceConfiguration =>
                {
                    serviceConfiguration.ConstructUsing(_ => SchedulerServiceFactory.CreateScheduler());

                    serviceConfiguration.WhenStarted((service, _) =>
                    {
                        service.Start();
                        return true;
                    });
                    serviceConfiguration.WhenStopped((service, _) =>
                    {
                        service.Stop();
                        return true;
                    });
                });
            });
        }
    }
}
