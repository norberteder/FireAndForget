using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireAndForget.Core.Persistence
{
    public class DatabaseManager : IRepository
    {
        private static object lockObject = new object();
        private static DatabaseManager instance;        
        private Type RepositoryType { get; set; }

        private DatabaseManager()
        {
            Configure();
        }

        public static DatabaseManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseManager();
                    }
                    return instance;
                }
            }
        }

        private void Configure()
        {
            string type = ConfigurationManager.AppSettings["repository"];
            RepositoryType = Type.GetType(type);
        }

        public void Add(BusTask task)
        {
            var repository = Activator.CreateInstance(RepositoryType) as IRepository;
            repository.Add(task);
        }

        public void Update(BusTask task)
        {
            var repository = Activator.CreateInstance(RepositoryType) as IRepository;
            repository.Update(task);
        }

        public IEnumerable<BusTask> GetAllOpenTasks()
        {
            var repository = Activator.CreateInstance(RepositoryType) as IRepository;
            return repository.GetAllOpenTasks();
        }

        public IEnumerable<BusTask> GetAllOpenTasksForQueue(string queueName)
        {
            var repository = Activator.CreateInstance(RepositoryType) as IRepository;
            return repository.GetAllOpenTasksForQueue(queueName);
        }
    }
}
