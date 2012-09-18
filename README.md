[NServiceBus](http://nservicebus.com) is awesome, I've been to Udi's [Advanced Distributed Systems Design](http://www.udidahan.com/training/#Advanced_Distributed_System_Design) course and it changed how I do development. 

Sagas make for a great way to manage long running processes without the need for scheduled tasks and locking threads, but I have found diagnosing issues with them sometimes problematic.  Using the Raven Silverlight client tool you can get some visibility into what sagas are currently in play and what is scheduled, but it's a bit of a pain and it's difficult once the amount of data gets large.

So this project is designed to surface the current state of the data in your sagas and timeouts in a simple way that can help with development and system operations.

At its core there are two parts

 * `NserviceBus.Instrumentation.Agent`: A Windows service that can be installed on each machine that runs NserviceBus services
 * `NserviceBus.Instrumentation.Dashboard`: An MVC website that will load the data collected by the agents.

There is a Database project (`NServiceBus.Instrumentation.Database`) which will setup a SQL Server database for persisting the instrumentation data (SQL Express will work just fine). The other projects are libraries supporting the agent.

Currently the agent scans the computer it runs on for MSMQ Queues that end with `.retries` and treats them as NServiceBus Services. Alternatively there is a config section that can be added to the service to explicitly include or exclude specific queues.

#This is a first release 

It's not feature complete and it has bugs.  I would appreciate any feedback/forks/pulls. 

## References

1. [TopShelf](https://github.com/Topshelf/Topshelf) - Seriously, Microsoft should just abandon its windows service application type in Visual Studio & point everyone to this project, it makes windows services usable!
2. [Dapper](https://github.com/SamSaffron/dapper-dot-net)
3. [Castle Windsor](https://github.com/castleproject/Castle.Windsor-READONLY)
4. [Automapper](https://github.com/AutoMapper/AutoMapper)
5. [Knockout](https://github.com/SteveSanderson/knockout)
6. [Bootstrap](http://twitter.github.com/bootstrap/)
