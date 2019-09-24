# ConsulRestHelper
This is a utility can help you to send web requests to services registered to Consul.Like RestTemplate in Spring Cloud for Java.

It has some functions below.
1. Service discovery.

    It can transfer virtual service request to a real instance service request.
    Example: "http://ProductService/api/product/" => "http://192.168.1.10:8080/api/product/".

2. Load balancing

    By default,we use the milliseconds elapsed since the system started to mocke up the total count of services registered to consul to get one of the service instance.

    ```csharp
    services.ElementAt(Environment.TickCount % services.Count());
    ```

3. Rest Request
    
    sending Get/Post/Put/Delete web requests to services registered in consul.


> Documents

* https://netcore.colinchang.net/pages/microservice-consul.html

> [Nuget](https://www.nuget.org/packages/ColinChang.ConsulRestHelper/)

```sh
# .NET CLI
dotnet add package ColinChang.ConsulRestHelper

# Package Manager
Install-Package ColinChang.ConsulRestHelper
```
