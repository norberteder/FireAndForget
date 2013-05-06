using System;
using System.Configuration;
using System.Reflection;
using FireAndForget.Configuration;
using FireAndForget.Core;
using FireAndForget.Exceptions;

namespace FireAndForget
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var configuration = GetConfiguration();

                InitializeServiceBus(configuration);

                var host = new Host(configuration.Server.Uri);
                host.Run();
            }
            catch (ServiceBusConfigurationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Stop();
            }
            catch (ServiceBusInitializationException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Stop();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
                Stop();
            }
        }

        private static void InitializeServiceBus(ServiceBusConfigurationSection configuration)
        {
            foreach (WorkerConfigurationElement worker in configuration.Workers)
            {
                BusManager.Instance.RegisterWorker(worker.Name);
                Console.WriteLine(String.Format("Installed worker '{0}'", worker.Name));
            }

            foreach (ExecutorConfigurationElement executor in configuration.Executors)
            {
                try
                {
                    var type = Type.GetType(executor.Type);
                    if (type == null)
                        throw new ServiceBusInitializationException(string.Format("Type {0} is invalid", executor.Type));

                    BusManager.Instance.RegisterExecutor(executor.Worker, type);
                    Console.WriteLine(String.Format("Installed executor '{0}' for worker '{1}'", executor.Type, executor.Worker));
                }
                catch (TargetInvocationException ex)
                {
                    throw new ServiceBusInitializationException(string.Format("Cannot invoke type '{0}'", executor.Type), ex);
                }
                catch (TypeLoadException ex)
                {
                    throw new ServiceBusInitializationException(string.Format("Cannot load type '{0}'", executor.Type), ex);
                }
            }
        }

        private static ServiceBusConfigurationSection GetConfiguration()
        {
            try
            {
                ServiceBusConfigurationSection configuration = (ServiceBusConfigurationSection)System.Configuration.ConfigurationManager.GetSection("ServiceBusConfiguration");
                if (configuration == null)
                    throw new ServiceBusConfigurationException("Could not read configuration");
                if (configuration.Workers == null || configuration.Workers.Count == 0)
                    throw new ServiceBusConfigurationException("No configured workers for service bus found");
                if (configuration.Executors == null || configuration.Executors.Count == 0)
                    throw new ServiceBusConfigurationException("No configured executors for service bus found");

                return configuration;
            }
            catch (ConfigurationErrorsException configEx)
            {
                throw new ServiceBusConfigurationException(configEx.Message, configEx);
            }
        }

        private static void Stop()
        {
            Console.WriteLine("Press ENTER to shutdown service bus");
            Console.ReadLine();
        }
    }
}
