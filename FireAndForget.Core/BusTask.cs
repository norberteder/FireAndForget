using System;

namespace FireAndForget.Core
{
    public class BusTask
    {
        public long Id { get; set; }
        /// <summary>
        /// Gets the message type this message represents
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string MessageType { get; private set; }
        /// <summary>
        /// Gets the data that's important for this task. 
        /// </summary>
        /// <value>
        /// The data.
        /// </value>
        public string Data { get; private set; }
        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        /// <value>
        /// The queue.
        /// </value>
        public string Queue { get; private set; }

        /// <summary>
        /// Gets the receiving date of this task
        /// </summary>
        /// <value>
        /// The received.
        /// </value>
        public DateTime Received { get; set; }
        /// <summary>
        /// Gets the finishing date
        /// </summary>
        /// <value>
        /// The finished.
        /// </value>
        public DateTime Finished { get; set; }
        /// <summary>
        /// Gets or sets the date/time when to execute the task
        /// </summary>
        /// <value>
        /// The execute at.
        /// </value>
        public DateTime? ExecuteAt { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusTask" /> class.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="messageType">The name.</param>
        /// <param name="data">The data.</param>
        /// <exception cref="System.ArgumentException">name must not be null</exception>
        public BusTask(string queue, string messageType, string data)
        {
            if (string.IsNullOrEmpty(messageType))
                throw new ArgumentException("messageType must not be null");
            if (string.IsNullOrEmpty("queue"))
                throw new ArgumentException("queue must not be null");

            MessageType = messageType;
            Data = data;
            Queue = queue;

            Received = DateTime.Now;
            Finished = new DateTime(1970, 1, 1);
            State = BusTaskState.NotStarted;
        }

        /// <summary>
        /// Sets a possible found error.
        /// </summary>
        /// <param name="error">The error of type <see cref="Exception"/></param>
        public void SetError(Exception error)
        {
            Error = error.Message;
            State = BusTaskState.Error;
        }

        /// <summary>
        /// Marks this task as finished
        /// </summary>
        public void Finish()
        {
            Finished = DateTime.Now;
            State = BusTaskState.Finished;
        }

        /// <summary>
        /// Marks this task as delayed
        /// </summary>
        public void Delayed(DateTime executeAt)
        {
            ExecuteAt = executeAt;
            State = BusTaskState.Delayed;
        }

        /// <summary>
        /// Marks this task as started
        /// </summary>
        public void Start()
        {
            State = BusTaskState.Started;
        }

        /// <summary>
        /// Gets a possible occured error, otherwise not set
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the state of this object
        /// </summary>
        /// <value>
        /// The <see cref=" BusTaskState"/>
        /// </value>
        public BusTaskState State { get; set; }
    }
}
