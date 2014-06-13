![AzureNetQ Logo](https://raw.githubusercontent.com/Roysvork/AzureNetQ/gh-pages/design/logo_design_150.png)

A Nice .NET API for Microsoft Azure Service Bus & Service Bus for Windows

* **[Homepage](http://roysvork.github.io/AzureNetQ)**
* **[Documentation](https://github.com/roysvork/AzureNetQ/wiki/Introduction)**
* **[NuGet](http://nuget.org/List/Packages/AzureNetQ)**
* **[EasyNetQ Project](http://github.com/mikehadlow/EasyNetQ)**
* **[EasyNetQ Discussion Group](https://groups.google.com/group/easynetq)**

Goals:

1. To make working with Microsoft Service Bus on .NET as easy as possible.
2. To build an API that is close to interchangable with EasyNetQ.

To connect to Service Bus...

    <add key="Microsoft.ServiceBus.ConnectionString" value="Endpoint=sb://servicebus/ServiceBusDefaultNamespace;StsEndpoint=https://servicebus:10355/ServiceBusDefaultNamespace;RuntimePort=10354;ManagementPort=10355" />

    var bus = AzureBusFactory.CreateBus();

To publish a message...

    bus.Publish(message);

To subscribe to a message...

	bus.Subscribe<MyMessage>(
		msg => Console.WriteLine(msg.Text),
		x => x.WithSubscription("my_subscription_id"));

Remote procedure call...

    var request = new TestRequestMessage {Text = "Hello from the client! "};
    bus.Request<TestRequestMessage, TestResponseMessage>(request, response => 
        Console.WriteLine("Got response: '{0}'", response.Text));

RPC server...

    bus.Respond<TestRequestMessage, TestResponseMessage>(request => 
		new TestResponseMessage{ Text = request.Text + " all done!" });
	

## Getting started

Just open AzureNetQ.sln in VisualStudio 2013 and build.

All the required dependencies for the solution file to build the software are included. To run the explicit tests that send messages you will have to be running the AzureNetQ.Tests.SimpleService application and have a working Service Bus for Windows install (Blog post coming)

## Mono specific

Unlike EasyNetQ, AzureNetQ has not yet been tested on Mono. If you would like to help with this, please get in touch with @roysvork on twitter!
