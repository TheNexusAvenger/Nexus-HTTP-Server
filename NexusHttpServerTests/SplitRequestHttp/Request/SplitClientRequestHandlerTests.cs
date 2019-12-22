/*
 * TheNexusAvenger
 * 
 * Test the Nexus.Http.Server.Test.SplitRequestHttp.Request.SplitClientRequestHandler class.
 */

using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using Nexus.Http.Server.SplitHttp.Request;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.SplitRequestHttp.Request
{
    /*
     * Test client request handler that returns specific results for tests.
     */
    public class TestSplitClientRequestHandler : SplitClientRequestHandler
    {
        private int ResponseCode;
        private string MimeType;
        
        /*
         * Creates a client request handler.
         */
        public TestSplitClientRequestHandler(int responseCode,string mimeType)
        {
            this.ResponseCode = responseCode;
            this.MimeType = mimeType;
        }
        
        /*
         * Returns a response for a given request.
         */
        public override HttpResponse GetCompleteResponseData(HttpRequest request)
        {
            return new HttpResponse(this.ResponseCode,this.MimeType,request.GetBody());
        }
    }
    
    [TestFixture]
    public class SplitClientRequestHandlerTests
    {
        /*
         * Tests getting a response for standalone request.
         */
        [Test]
        public void TestStandaloneRequest()
        {
            // Create the component under testing.
            var CuT = new TestSplitClientRequestHandler(200,"text/html");
            
            // Send a request and assert it is standalone.
            var response = CuT.GetResponseData(new HttpRequest("GET","/test","","Test body"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"Test body");
        }
        
        /*
         * Tests getting responses for a split requests.
         */
        [Test]
        public void TestSplitRequests()
        {
            // Create the component under testing.
            var CuT = new TestSplitClientRequestHandler(200,"text/html");
            
            // Send split requests and assert they get the correct responses.
            var response = CuT.GetResponseData(new HttpRequest("GET","/test?maxpackets=3","","Tes"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":0}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=2&maxpackets=3","","et 1"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":0}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?maxpackets=3","","Tes"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":1}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=1&maxpackets=3","","t Pack"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":1,\"packet\":\"Test Packet 1\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=1&packet=1&maxpackets=3","","t Pack"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":1}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=1&packet=2&maxpackets=3","","et 2"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":1,\"packet\":\"Test Packet 2\"}");
        }

        /*
         * Tests getting responses for a split requests in the incorrect order.
         */
        [Test]
        public void TestSplitRequestsIncorrectOrder()
        {
            // Create the component under testing.
            var CuT = new TestSplitClientRequestHandler(200,"text/html");
            
            // Send split requests and assert they get the correct responses.
            var response = CuT.GetResponseData(new HttpRequest("GET","/test?maxpackets=3&packet=2","","et"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":0}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=0&maxpackets=3","","Tes"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":0}");

            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=1&maxpackets=3","","t Pack"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":1,\"packet\":\"Test Packet\"}");
        }
        
        /*
         * Tests getting responses for a split requests with a request being out of bounds.
         */
        [Test]
        public void TestSplitRequestsOutOfBounds()
        {
            // Create the component under testing.
            var CuT = new TestSplitClientRequestHandler(200,"text/html");
            
            // Send split requests and assert they get the correct responses.
            var response = CuT.GetResponseData(new HttpRequest("GET","/test?maxpackets=3&packet=2","","et"));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"incomplete\",\"id\":0}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=3&maxpackets=3",""," 1!"));
            Assert.AreEqual(response.GetStatus(),400);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"error\",\"message\":\"Packet index invalid\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?requestId=0&packet=-1&maxpackets=3","","!"));
            Assert.AreEqual(response.GetStatus(),400);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"error\",\"message\":\"Packet index invalid\"}");
        }
        
        /*
         * Tests getting split responses.
         */
        [Test]
        public void TestSplitResponses()
        {
            // Create the component under testing.
            var CuT = new TestSplitClientRequestHandler(200,"text/html");
            
            // Send request and assert it gets the correct responses.
            var response = CuT.GetResponseData(new HttpRequest("POST","/test?maxpackets=1","",new string('A',140000)));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":3,\"packet\":\"" + new string('A',64000) + "\"}");
            
            // Assert that requests with missing parameters are filled in.
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true","",""));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":3,\"packet\":\"" + new string('A',64000) + "\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&responseId=0","",""));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":0,\"maxPackets\":3,\"packet\":\"" + new string('A',64000) + "\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&packet=1","",""));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":1,\"maxPackets\":3,\"packet\":\"" + new string('A',64000) + "\"}");
            
            // Assert out-of-bounds cases are set.
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&responseId=1&packet=2","",""));
            Assert.AreEqual(response.GetStatus(),400);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"error\",\"message\":\"Response index invalid\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&responseId=0&packet=4","",""));
            Assert.AreEqual(response.GetStatus(),400);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"error\",\"message\":\"Packet index invalid\"}");
            
            // Assert a packet is removed after being read.
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&responseId=0&packet=2","",""));
            Assert.AreEqual(response.GetStatus(),200);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"success\",\"id\":0,\"currentPacket\":2,\"maxPackets\":3,\"packet\":\"" + new string('A',12000) + "\"}");
            
            response = CuT.GetResponseData(new HttpRequest("GET","/test?getResponse=true&responseId=1&packet=2","",""));
            Assert.AreEqual(response.GetStatus(),400);
            Assert.AreEqual(response.GetMimeType(),"text/html");
            Assert.AreEqual(response.GetResponseData(),"{\"status\":\"error\",\"message\":\"Response index invalid\"}");
        }
    }
}