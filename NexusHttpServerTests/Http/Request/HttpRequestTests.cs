/*
 * TheNexusAvenger
 * 
 * Test the Nexus.Http.Server.Test.Http.Request.HttpRequest class.
 */

using Nexus.Http.Server.Http.Request;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.Http.Request
{
    [TestFixture]
    public class HttpRequestTests
    {
        /*
         * Tests that the constructor works without failing.
         */
        [Test]
        public void ConstructorTest()
        {
            // Create the component under testing with a string url and assert it was created correctly.
            var CuT1 = new HttpRequest("POST","/test/page?param=test","https://my.domain","Test body");
            Assert.AreEqual(CuT1.GetRequestType(),"POST");
            Assert.AreEqual(CuT1.GetURL().GetBaseURL(),"test/page");
            Assert.AreEqual(CuT1.GetURL().GetParameter("param"),"test");
            Assert.AreEqual(CuT1.GetHost(),"https://my.domain");
            Assert.AreEqual(CuT1.GetBody(),"Test body");
            
            // Create the component under testing with a url type and assert it was created correctly.
            var CuT2 = new HttpRequest("POST",URL.FromString("/test/page?param=test"),"https://my.domain","Test body");
            Assert.AreEqual(CuT2.GetRequestType(),"POST");
            Assert.AreEqual(CuT2.GetURL().GetBaseURL(),"test/page");
            Assert.AreEqual(CuT2.GetURL().GetParameter("param"),"test");
            Assert.AreEqual(CuT2.GetHost(),"https://my.domain");
            Assert.AreEqual(CuT2.GetBody(),"Test body");
        }
    }
}