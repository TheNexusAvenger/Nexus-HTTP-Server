/*
 * TheNexusAvenger
 * 
 * Test the Nexus.Http.Server.Test.SplitRequestHttp.Request.SplitRequestHandler class.
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
    public class SplitRequestHandlerTests
    {
        /*
         * Tests the RegisterHandler method.
         */
        [Test]
        public void RegisterHandlerTests()
        {
            // Create the component under testing.
            var CuT = new SplitRequestHandler();
            var handler1 = new TestClientRequestHandler(200,"text/html","test1");
            var handler2 = new TestClientRequestHandler(200,"text/html","test2");
            CuT.RegisterHandler("GET","/test1",handler1);
            CuT.RegisterHandler("POST","/test2",handler2);
            
            // Assert the handlers are correct.
            Assert.AreEqual(CuT.GetRequestHandler(new HttpRequest("GET",URL.FromString("test1"),"","")),handler1);
            Assert.AreEqual(CuT.GetRequestHandler(new HttpRequest("POST",URL.FromString("test1"),"","test2")),null);
            Assert.AreEqual(CuT.GetRequestHandler(new HttpRequest("GET",URL.FromString("test2"),"","")),handler2);
            Assert.AreEqual(CuT.GetRequestHandler(new HttpRequest("POST",URL.FromString("test2"),"","test2")),handler2);
            
            // Assert the responses are correct.
            Assert.AreEqual(CuT.GetResponse(new HttpRequest("GET",URL.FromString("test1"),"","")).GetStatus(),200);
            Assert.AreEqual(CuT.GetResponse(new HttpRequest("POST",URL.FromString("test1"),"","test2")).GetStatus(),400);
            Assert.AreEqual(CuT.GetResponse(new HttpRequest("GET",URL.FromString("test2"),"","")).GetStatus(),200);
            Assert.AreEqual(CuT.GetResponse(new HttpRequest("POST",URL.FromString("test2"),"","test2")).GetStatus(),200);
        }
    }
}