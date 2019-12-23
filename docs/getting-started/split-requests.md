# Split Requests
!!! warning
    Split requests should not be used for public-facing
    applications. No protections are provided that
    would result in out-of-memory errors from malicious
    requests.

Split requests intended to be used for cases where
a lot of data needs to be transferred via POST requests,
and this amount of data may be beyond what the client
can send or recieve, like Roblox Studio.

## Protocol
The protocol is able to have split requests and split
responses. For this example, a client is only able to
send 6 characters in the body and the server is configured
to only send back 4 characters. The POST body sent is then
echoed back without modification.

### Standalone
If no split-request parameters are speicifed, the request
is treated as non-split and the response is sent back
complete.

```
POST http://localhost:8080 "Test1 Test2 Test3"
     HTTP 200 "Test1 Test2 Test3"
```

This implementation allows for clients without needing
to get around limits to not need to use the implementation,
such as external end-to-end testing of applications.

### Sending Split Requests
Split requests take data as URL arguments, which include the following:
- `maxPackets` - The total packets that will be sent
- `requestId` - The id of the packet to send (not sent in the first request)
- `packet` - The id of the pakcet to send, starting at 0 (optional in the first request)

When a request is incomplete (not all packets sent), a JSON
string is returned with the following
- `status` - `"incomplete"`
- `id` - The value to use in `requestId` to continue the request

When a response is complete, the following is returned:
- `status` - `"success"`
- `id` - The value to use in `responseId` for getting responses
- `currentPacket` - The id of the packet, starting at 0.
- `maxPackets` - The total packets to recieve
- `packet` - The data in the pakcet

For getting additional responses (maxPackets > 1), the following
arguments can be used:
- `getResponse = true` - Specifies that responses are being fetched
- `responseId` - The response id to read from
- `packet` - The packet id to get from the response, starting at 0

For the example above, the requests would look like the following:
```
POST http://localhost:8080?maxPackets=3 "Test1 "
     HTTP 200 "{"status\":\"incomplete\",\"id\":0}"
POST http://localhost:8080?maxPackets=3&packet=1&requestId=0 "Test2 "
     HTTP 200 "{"status\":\"incomplete\",\"id\":0}"
POST http://localhost:8080?maxPackets=3&packet=2&requestId=0 "Test 3"
     HTTP 200 "{"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":4 \"packet\":\"Test1\"}"
GET http://localhost:8080?getResponse=true&responseId=0&packet=1 "Test 3"
     HTTP 200 "{"status\":\"success\",\"id\":0,\"currentPacket\":1,\"maxPackets\":4,\"packet\":\" Tes\"}"
GET http://localhost:8080?getResponse=true&responseId=0&packet=2 "Test 3"
     HTTP 200 "{"status\":\"success\",\"id\":0,\"currentPacket\":2,\"maxPackets\":4,\"packet\":\"t2 T\"}"
GET http://localhost:8080?getResponse=true&responseId=0&packet=3 "Test 3"
     HTTP 200 "{"status\":\"success\",\"id\":0,\"currentPacket\":3,\"maxPackets\":4,\"packet\":\"est3\"}"
```

After a response is fully retrieved, it is removed to save memory.
This however does cause a vulnerability since malicous (or just incorrect
usage of the protocol) will result in a memory leak. Same goes for sending
requests since they need to be stored and combined. Additionally, there
is no checks for the size, so denial of service attacks can be performed
by sending large, incomplete requests that won't get cleared. This system
is not intended for publicly facing applications.

### Code Example
The code from the previous example can be tweaked to work with this (ignoring
the limit of sending back 4 characters) by using the base class `SplitClientRequestHandler`
for the client handler and using `SplitRequestHandler` which ensures that
the `GET` handlers are added.

```csharp
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using Nexus.Http.Server.Http.Server;
using Nexus.Http.Server.SplitHttp.Request;

namespace Demo
{
    public class Program
    {
        public class Handler : SplitClientRequestHandler
        {
            /*
             * Returns a response for a given complete request.
             */
            public override HttpResponse GetCompleteResponseData(HttpRequest request)
            {
                return HttpResponse.CreateSuccessResponse(request.GetBody());
            }
        }
        
        public static void Main(string[] args)
        {
            // Create the request handlers.
            var handler = new SplitRequestHandler();
            handler.RegisterHandler("POST","/",new Handler());
            handler.RegisterHandler("POST","/test",new Handler());

            // Create and start the server.
            var server = new HttpServer(8080,handler);
            server.Start();
        }
    }
}
```