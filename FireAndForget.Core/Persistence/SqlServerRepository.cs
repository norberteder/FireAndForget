using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireAndForget.Core.Persistence
{
    public class SqlServerRepository : IRepository
    {
        private string ConnectionString { get; set;}
        private string ProviderName { get; set; } 
        private DbProviderFactory Factory { get; set; }
        private IDbConnection Connection { get; set; }

        public SqlServerRepository()
        {
            ReadConnectionInformation();
            CreateFactory();
            CreateTableIfNecessary();
        }

        private void ReadConnectionInformation()
        {
            if (ConfigurationManager.ConnectionStrings.Count > 0)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["sqlserver"].ConnectionString;
                ProviderName = ConfigurationManager.ConnectionStrings["sqlserver"].ProviderName;
            }
        }

        private void CreateFactory()
        {
            Factory = DbProviderFactories.GetFactory(ProviderName);
        }

        private void CreateTableIfNecessary()
        {
            Open();

            var command = Connection.CreateCommand();
            command.CommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'BusTask') BEGIN CREATE TABLE BusTask ([Id] [bigint] IDENTITY(1,1) NOT NULL,[MessageType] [nvarchar](max) NOT NULL,[Data] [nvarchar](max) NOT NULL,[Queue] [nvarchar](50) NOT NULL,[Received] [datetime] NOT NULL,[Finished] [datetime] NULL,[Error] [nvarchar](max) NULL,[State] [int] NOT NULL,CONSTRAINT [PK_BusTask] PRIMARY KEY CLUSTERED ([Id] ASC)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]) ON [PRIMARY] END";
            command.ExecuteNonQuery();

            command = Connection.CreateCommand();
            command.CommandText = "IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='bt_state' AND object_id = OBJECT_ID('BusTask')) BEGIN CREATE NONCLUSTERED INDEX [bt_state] ON BusTask ([State] ASC)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] END";
            command.ExecuteNonQuery();

            Close();
        }

        public void Add(BusTask task)
        {
            Open();

            var command = Connection.CreateCommand();
            command.CommandText = "INSERT INTO BusTask (MessageType, Data, Queue, Received, Finished, Error, State) VALUES (@messageType,@data,@queue,@received,@finished,@error,@state); SELECT @@IDENTITY";
            command.Parameters.Add(CreateParameter(DbType.String, task.MessageType, "messageType"));
            command.Parameters.Add(CreateParameter(DbType.String, task.Data, "data"));
            command.Parameters.Add(CreateParameter(DbType.String, task.Queue, "queue"));
            command.Parameters.Add(CreateParameter(DbType.DateTime, task.Received, "received"));
            command.Parameters.Add(CreateParameter(DbType.DateTime, task.Finished, "finished"));
            command.Parameters.Add(CreateParameter(DbType.String, string.IsNullOrEmpty(task.Error) ? string.Empty : task.Error, "error"));
            command.Parameters.Add(CreateParameter(DbType.Int32, task.State, "state"));

            var returnedIdentity = command.ExecuteScalar();
            Int64 parsedIdentity;

            if (long.TryParse(returnedIdentity.ToString(), out parsedIdentity))
                task.Id = parsedIdentity;

            Close();
        }

        public void Update(BusTask task)
        {
            Open();

            var command = Connection.CreateCommand();
            command.CommandText = "UPDATE BusTask SET MessageType=@messageType, Data=@data, Queue=@queue, Received=@received, Finished=@finished, Error=@error, State=@state where Id=@id";
            command.Parameters.Add(CreateParameter(DbType.String, task.MessageType, "messageType"));
            command.Parameters.Add(CreateParameter(DbType.String, task.Data, "data"));
            command.Parameters.Add(CreateParameter(DbType.String, task.Queue, "queue"));
            command.Parameters.Add(CreateParameter(DbType.DateTime, task.Received, "received"));
            command.Parameters.Add(CreateParameter(DbType.DateTime, task.Finished, "finished"));
            command.Parameters.Add(CreateParameter(DbType.String, string.IsNullOrEmpty(task.Error) ? string.Empty : task.Error, "error"));
            command.Parameters.Add(CreateParameter(DbType.Int32, task.State, "state"));
            command.Parameters.Add(CreateParameter(DbType.Int64, task.Id, "id"));

            command.ExecuteNonQuery();

            Close();
        }

        public IEnumerable<BusTask> GetAllOpenTasks()
        {
            Open();

            var command = Connection.CreateCommand();
            command.CommandText = "SELECT MessageType, Data, Queue, Received, Finished, Error, State, Id FROM BusTask where State <> 2 order by Received ASC";

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    BusTask task = new BusTask(reader.GetString(2), reader.GetString(0), reader.GetString(1));
                    task.Received = reader.GetDateTime(3);
                    task.Finished = reader.GetDateTime(4);
                    task.Error = reader.GetString(5);
                    task.State = (BusTaskState)reader.GetInt32(6);
                    task.Id = reader.GetInt64(7);

                    yield return task;
                }
            }

            Close();
        }

        public IEnumerable<BusTask> GetAllOpenTasksForQueue(string queueName)
        {
            Open();

            var command = Connection.CreateCommand();
            command.CommandText = "SELECT MessageType, Data, Queue, Received, Finished, Error, State, Id FROM BusTask where State <> 2 and Queue = @queue order by Received ASC";
            command.Parameters.Add(CreateParameter(DbType.String, queueName, "queue"));

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    BusTask task = new BusTask(reader.GetString(2), reader.GetString(0), reader.GetString(1));
                    task.Received = reader.GetDateTime(3);
                    task.Finished = reader.GetDateTime(4);
                    task.Error = reader.GetString(5);
                    task.State = (BusTaskState)reader.GetInt32(6);
                    task.Id = reader.GetInt64(7);

                    yield return task;
                }
            }

            Close();
        }

        /// <summary>
        /// Opens a connection
        /// </summary>
        private void Open()
        {
            Connection = Factory.CreateConnection();
            Connection.ConnectionString = ConnectionString;
            Connection.Open();
        }

        /// <summary>
        /// Closes a connection
        /// </summary>
        private void Close()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        private DbParameter CreateParameter(DbType type, object value, string parameterName)
        {
            var parameter = Factory.CreateParameter();
            parameter.DbType = type;
            parameter.Value = value;
            parameter.ParameterName = parameterName;
            return parameter;
        }
    }
}
