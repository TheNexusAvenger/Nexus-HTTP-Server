/*
 * TheNexusAvenger
 *
 * Test the Nexus.Http.Server.Test.Http.Response.HttpResponse class.
 */

using Nexus.Http.Server.Http.Response;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.Http.Response
{
    [TestFixture]
    public class HttpResponseTests
    {
        /*
         * Tests the constructor.
         */
        [Test]
        public void TestConstructor()
        {
            // Test the base constructor.
            var CuT1 = new HttpResponse(404,"text/xml","Not found");
            Assert.AreEqual(CuT1.GetStatus(),404);
            Assert.AreEqual(CuT1.GetMimeType(),"text/xml");
            Assert.AreEqual(CuT1.GetResponseData(),"Not found");
            
            // Tests creating a successful response.
            var CuT2 = HttpResponse.CreateSuccessResponse("Test response");
            Assert.AreEqual(CuT2.GetStatus(),200);
            Assert.AreEqual(CuT2.GetMimeType(),"text/html");
            Assert.AreEqual(CuT2.GetResponseData(),"Test response");
            
            // Tests creating an invalid response.
            var CuT3 = HttpResponse.CreateBadRequestResponse("Test response");
            Assert.AreEqual(CuT3.GetStatus(),400);
            Assert.AreEqual(CuT3.GetMimeType(),"text/html");
            Assert.AreEqual(CuT3.GetResponseData(),"Test response");
        }
    }
}