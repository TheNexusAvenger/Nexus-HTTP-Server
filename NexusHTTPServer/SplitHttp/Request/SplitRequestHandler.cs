/*
 * TheNexusAvenger
 * 
 * Extends the RequestHandler class for SplitClientRequestHandler since
 * any non-GET request handler also needs to register the GET request
 * handler for getting complete parts of packets.
 */

using Nexus.Http.Server.Http.Request;

namespace Nexus.Http.Server.SplitHttp.Request
{
    /*
     * Class for handling split client request handlers.
     */
    public class SplitRequestHandler : RequestHandler
    {
        /*
         * Registers a request handler.
         */
        public new void RegisterHandler(string requestType,string urlBase,IClientRequestHandler clientRequestHandler)
        {
            // Register the base request.
            base.RegisterHandler(requestType,urlBase,clientRequestHandler);

            // Register the GET request.
            if (requestType != "GET")
            {
                base.RegisterHandler("GET",urlBase,clientRequestHandler);
            }
        }
    }
}