/*
 * TheNexusAvenger
 *
 * Test the Nexus.Http.Server.Test.Http.Server.HttpServer class.
 */

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using Nexus.Http.Server.Http.Server;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.Http.Server
{
    /*
     * Test client request handler that returns specific results for tests.
     */
    public class TestClientRequestHandler : IClientRequestHandler
    {
        private int ResponseCode;
        private string MimeType;
        private string ResponsePrefix;
        
        /*
         * Creates a client request handler.
         */
        public TestClientRequestHandler(int responseCode,string mimeType,string responsePrefix)
        {
            this.ResponseCode = responseCode;
            this.MimeType = mimeType;
            this.ResponsePrefix = responsePrefix;
        }
        
        /*
         * Returns a response for a given request.
         */
        public HttpResponse GetResponseData(HttpRequest request)
        {
            return new HttpResponse(this.ResponseCode,this.MimeType,this.ResponsePrefix + request.GetBody());
        }
    }
    
    [TestFixture]
    public class HttpServerTests
    {
        public const int TEST_PORT = 20003;
        public const int TEST_SECOND_PORT = 20004;
        private HttpServer CuT;
        private RequestHandler requestHandler;
        
        /*
         * Sets up the components under testing for the test.
         */
        [SetUp]
        public void SetUpComponentsUnderTesting()
        {
            this.requestHandler = new RequestHandler();
            this.requestHandler.RegisterHandler("GET","",new TestClientRequestHandler(200,"text/html","test1"));
            this.requestHandler.RegisterHandler("GET","test1/",new TestClientRequestHandler(200,"text/html","test1"));
            this.requestHandler.RegisterHandler("GET","/test1/test2",new TestClientRequestHandler(201,"text/html","test2"));
            this.requestHandler.RegisterHandler("GET","test1/test3",new TestClientRequestHandler(200,"text/xml","test3"));
            this.requestHandler.RegisterHandler("POST","test1/test3",new TestClientRequestHandler(404,"text/json","test4"));
            this.CuT = new HttpServer(TEST_PORT,this.requestHandler);
        }
        
        /*
         * Sends an HTTP GET request to the server and asserts the response is correct.
         */
        private void AssertGetRequest(int port,string endpoint,HttpStatusCode responseCode,string responseContents)
        {
            // Get the response.
            var response = new HttpClient().GetAsync("http://localhost:" + port + endpoint).Result;
            
            // Assert that the response is correct.
            Assert.AreEqual(response.StatusCode,responseCode);
            Assert.AreEqual(response.Content.ReadAsStringAsync().Result,responseContents);
        }
        
        /*
         * Sends an HTTP POST request to the server and asserts the response is correct.
         */
        private void AssertPostRequest(int port,string endpoint,string requestBody,HttpStatusCode responseCode,string responseContents)
        {
            // Get the response.
            var response = new HttpClient().PostAsync("http://localhost:" + port + endpoint,new StringContent(requestBody)).Result;
            
            // Assert that the response is correct.
            Assert.AreEqual(response.StatusCode,responseCode);
            Assert.AreEqual(response.Content.ReadAsStringAsync().Result,responseContents);
        }
        
        /*
         * Tests starting and stopping the server.
         * This is a functional test that requires port 20003 to be usable.
         */
        [Test]
        public void TestStartingAndStopping()
        {
            // Start the server.
            Task.Run(() => this.CuT.Start());
            
            // Assert starting the server again throws an exception.
            Assert.Throws<WebException>(() => this.CuT.Start());
            
            // Send GET requests and assert they are correct.
            this.AssertGetRequest(TEST_PORT,"",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_PORT,"/",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_PORT,"/test1",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_PORT,"/test1/",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_PORT,"/test1/test2",HttpStatusCode.Created,"test2");
            this.AssertGetRequest(TEST_PORT,"/test1/test2/",HttpStatusCode.Created,"test2");
            this.AssertGetRequest(TEST_PORT,"/test1/test3",HttpStatusCode.OK,"test3");
            this.AssertGetRequest(TEST_PORT,"/test1/test3/",HttpStatusCode.OK,"test3");
            this.AssertGetRequest(TEST_PORT,"/test1/test4",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertGetRequest(TEST_PORT,"/test1/test4/",HttpStatusCode.BadRequest,"Invalid request");
            
            // Send POST requests and assert they are correct.
            this.AssertPostRequest(TEST_PORT,"","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1/","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1/test2","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1/test2/","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1/test3","test5",HttpStatusCode.NotFound,"test4test5");
            this.AssertPostRequest(TEST_PORT,"/test1/test3/","test5",HttpStatusCode.NotFound,"test4test5");
            this.AssertPostRequest(TEST_PORT,"/test1/test4","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/test1/test4/","test5",HttpStatusCode.BadRequest,"Invalid request");
            
            // Stop the server.
            this.CuT.Stop();
            
            // Assert stopping the server again throws an exception.
            Assert.Throws<WebException>(() => this.CuT.Stop());
            
            // Restart the server and assert the server is up.
            Task.Run(() => this.CuT.Start());
            this.AssertGetRequest(TEST_PORT,"",HttpStatusCode.OK,"test1");
            
            // Stop the server.
            this.CuT.Stop();
        }
        
        /*
         * Tests starting and stopping the server with multiple prefixes.
         * This is a functional test that requires ports 20003 and 20004 to be usable.
         */
        [Test]
        public void TestStartingAndStoppingMultiplePrefixes()
        {
            // Start the server.
            this.CuT.AddPrefix("http://localhost:" + TEST_SECOND_PORT + "/");
            Task.Run(() => this.CuT.Start());
            
            // Assert starting the server again throws an exception.
            Assert.Throws<WebException>(() => this.CuT.Start());
            
            // Send GET requests and assert they are correct.
            this.AssertGetRequest(TEST_PORT,"",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_PORT,"/",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_SECOND_PORT,"",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_SECOND_PORT,"/",HttpStatusCode.OK,"test1");
            
            // Send POST requests and assert they are correct.
            this.AssertPostRequest(TEST_PORT,"","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_PORT,"/","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_SECOND_PORT,"","test5",HttpStatusCode.BadRequest,"Invalid request");
            this.AssertPostRequest(TEST_SECOND_PORT,"/","test5",HttpStatusCode.BadRequest,"Invalid request");
            
            // Stop the server.
            this.CuT.Stop();
            
            // Assert stopping the server again throws an exception.
            Assert.Throws<WebException>(() => this.CuT.Stop());
            
            // Restart the server and assert the server is up.
            Task.Run(() => this.CuT.Start());
            this.AssertGetRequest(TEST_PORT,"",HttpStatusCode.OK,"test1");
            this.AssertGetRequest(TEST_SECOND_PORT,"",HttpStatusCode.OK,"test1");
            
            // Stop the server.
            this.CuT.Stop();
        }
    }
}