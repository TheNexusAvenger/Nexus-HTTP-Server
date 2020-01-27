/*
 * TheNexusAvenger
 * 
 * Test the Nexus.Http.Server.Test.SplitRequestHttp.Request.PartialHttpRequest class.
 */

using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.SplitHttp.Request;
using NUnit.Framework;

namespace Nexus.Http.Server.Test.SplitRequestHttp.Request
{
    [TestFixture]
    public class PartialHttpRequestTests
    {
        /*
         * Tests creating a complete request.
         */
        [Test]
        public void TestCompleteRequest()
        {
            // Create a partial request and assert it is set up correctly.
            var CuT = new PartialHttpRequest("POST",URL.FromString("/test1?request=test2"),"my.domain",4);
            CuT.AddHeader("header1","value1");
            CuT.AddHeader("header2","value2");
            Assert.AreEqual(CuT.IsComplete(),false);
            Assert.AreEqual(CuT.GetBody(),null);
            
            // Add partial requests until the body is complete.
            CuT.AddPartialPacket(2,"Packet3");
            Assert.AreEqual(CuT.IsComplete(),false);
            Assert.AreEqual(CuT.GetBody(),null);
            CuT.AddPartialPacket(3,"Packet4");
            Assert.AreEqual(CuT.IsComplete(),false);
            Assert.AreEqual(CuT.GetBody(),null);
            CuT.AddPartialPacket(0,"Packet1");
            Assert.AreEqual(CuT.IsComplete(),false);
            Assert.AreEqual(CuT.GetBody(),null);
            CuT.AddPartialPacket(1,"Packet2");
            Assert.AreEqual(CuT.IsComplete(),true);
            Assert.AreEqual(CuT.GetBody(),"Packet1Packet2Packet3Packet4");
            
            // Assert that a single response is complete.
            var response = CuT.ToSingleRequest();
            Assert.AreEqual(response.GetRequestType(),"POST");
            Assert.AreEqual(response.GetHost(),"my.domain");
            Assert.AreEqual(response.GetURL().GetBaseURL(),"test1");
            Assert.AreEqual(response.GetURL().GetParameter("request"),"test2");
            Assert.AreEqual(response.GetBody(), "Packet1Packet2Packet3Packet4");
            Assert.AreEqual(response.GetHeader("header1"),"value1");
            Assert.AreEqual(response.GetHeader("header2"),"value2");
            Assert.AreEqual(response.GetHeader("header3"),null);
        }
    }
}