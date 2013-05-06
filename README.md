# FireAndForget

`FindAndForget` is an easy to configure and lightweight asynchronous Message Bus. It is accessible via JSON REST API.

## Features

* Available via JSON REST API
* Multiple worker (queues)
* Configurable executors for messages
* Support for Microsoft SQL Server (another storage systems can be implemented easily)
* Very low dependencies

## Configuration

There is not a lot to configure. All configurations are stored in the `app.config` of the console application. Per default the Message Bus is executed as a self hosted HTTP container within a console application. The section `ServiceBusConfiguration` is necessary for `FireAndForget` to run. There are very important settings:

* `appSettings`: Use the key `repository` to configure the implemenation of data access. This is by default the SQL Server. The connection string is configured in the default settings of the `app.config`.
* `Server`: This is the URI the server listening for messages
* `Workers`: Configure named workers here. For each worker an own queue is created
* `Executors`: Executors are mapped to message types (see section *Messages* below) and a specific worker. Please note: there is no registration for message events, it is a direct mapping, nothing more.

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
	  <configSections>
	    <section name="ServiceBusConfiguration" 
	             type="FireAndForget.Configuration.ServiceBusConfigurationSection, FireAndForget"/>
	  </configSections>
	  
	  <appSettings>
	    <add key="repository" 
	         value="FireAndForget.Core.Persistence.SqlServerRepository, FireAndForget.Core"/>
	  </appSettings>
	  <connectionStrings>
	    <add name="sqlserver" connectionString="Server=localhost;Database=FireAndForget;Trusted_Connection=True;" providerName="System.Data.SqlClient"/>
	  </connectionStrings>

	  <ServiceBusConfiguration>
	    <Server uri="http://localhost:2900"/>
	    <Workers>
	      <Worker name="default"/>
	      <Worker name="test"/>
	    </Workers>
	    <Executors>
	      <Executor worker="default" 
	                type="FireAndForget.TestClient.TaskExecutors.DefaultTaskExecutor, FireAndForget.TestClient"/>
	      <Executor worker="test" 
	                type="FireAndForget.TestClient.TaskExecutors.TestTaskExecutor, FireAndForget.TestClient"/>
	    </Executors>
	  </ServiceBusConfiguration>
	</configuration>

## Message

This is how a message looks like:

	{
	   "Data":{
	      "MessageType":"DefaultTask",
	      "Data":"This is some data for the default handler0"
	   }
	}

Very important is `MessageType`. This has to match with an executor that handles this kind of messages. `Data` includes the serialized data for the executor. 

## API

These are the possible requests for controlling FireAndForget:

*Add a message*

	POST http://localhost:2900/api/v1/queue HTTP/1.1
	Host: localhost:2900
	content-type: application/json
	content-length: 72

	{
	  "Data": {
	    "MessageType": "Default",
	    "Data": "Muh"
	  }
	}

*Requeue all erroneous tasks*

	POST http://localhost:2900/api/v1/retry HTTP/1.1
	Host: localhost:2900
	content-type: application/json
	content-length: 0

*Requeue all erroneous tasks of a specific queue*

	POST http://localhost:2900/api/v1/retry/default HTTP/1.1
	Host: localhost:2900
	content-type: application/json
	content-length: 0

There is a test client in the solution to show how it works. It's really simple!

## License

`Fire And Forget` is licensed under MIT.
