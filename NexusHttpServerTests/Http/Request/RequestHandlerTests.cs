/*
 * TheNexusAvenger
 * 
 * Test the Nexus.Http.Server.Test.Http.Request.RequestHandler class.
 */

using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.Http.Request
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
    public class RequestHandlerTests
    {
        private RequestHandler CuT;
        private TestClientRequestHandler handler1;
        private TestClientRequestHandler handler2;
        private TestClientRequestHandler handler3;
        private TestClientRequestHandler handler4;
        
        
        /*
         * Sets up the components under testing for the test.
         */
        [SetUp]
        public void SetUpComponentsUnderTesting()
        {
            this.CuT = new RequestHandler();
            this.handler1 = new TestClientRequestHandler(200,"text/html","test1");
            this.handler2 = new TestClientRequestHandler(201,"text/html","test2");
            this.handler3 = new TestClientRequestHandler(200,"text/xml","test3");
            this.handler4 = new TestClientRequestHandler(300,"text/json","test4");
            this.CuT.RegisterHandler("GET","",this.handler1);
            this.CuT.RegisterHandler("GET","test1/",this.handler1);
            this.CuT.RegisterHandler("GET","/test1/test2",this.handler2);
            this.CuT.RegisterHandler("GET","test1/test3",this.handler3);
            this.CuT.RegisterHandler("POST","test1/test3",this.handler4);
        }

        
        /*
         * Tests the GetRequestHandler method.
         */
        [Test]
        public void GetRequestHandlerTest()
        {
            // Assert the handlers are correct.
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/","","")),this.handler1);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test2","","")),this.handler2);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test2","","")),this.handler2);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test2/","","")),this.handler2);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test2/","","")),this.handler2);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test2","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test2","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test2/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test2/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test3","","")),this.handler3);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test3","","")),this.handler3);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test3/","","")),this.handler3);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test3/","","")),this.handler3);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test3","","")),this.handler4);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test3","","")),this.handler4);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test3/","","")),this.handler4);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test3/","","")),this.handler4);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test3?parameter=value","","")),this.handler3);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test3?parameter=value","","")),this.handler4);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test4","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test4","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","test1/test4/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("GET","/test1/test4/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test4","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test4","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","test1/test4/","","")),null);
            Assert.AreEqual(this.CuT.GetRequestHandler(new HttpRequest("POST","/test1/test4/","","")),null);
        }
        
        /*
         * Asserts that a response is correct.
         */
        public void AssertResponse(HttpResponse response,int responseCode,string mimeType,string body)
        {
            Assert.AreEqual(response.GetStatus(),responseCode);
            Assert.AreEqual(response.GetMimeType(),mimeType);
            Assert.AreEqual(response.GetResponseData(),body);
        }

        /*
         * Tests the GetResponse method.
         */
        [Test]
        public void GetResponseTest()
        {
            // Assert that invalid responses are correct.
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("POST","test1/","","")),400,"text/html","Invalid request");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("POST","test1/test4/","","")),400,"text/html","Invalid request");
            
            // Assert that valid responses are valid.
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","","","")),200,"text/html","test1");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1","","")),200,"text/html","test1");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1/","","")),200,"text/html","test1");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1/test2","","")),201,"text/html","test2");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1/test2/","","")),201,"text/html","test2");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1/test3","","")),200,"text/xml","test3");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("GET","/test1/test3/","","")),200,"text/xml","test3");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("POST","/test1/test3","","test5")),300,"text/json","test4test5");
            this.AssertResponse(this.CuT.GetResponse(new HttpRequest("POST","/test1/test3/","","test6")),300,"text/json","test4test6");
        }
    }
}