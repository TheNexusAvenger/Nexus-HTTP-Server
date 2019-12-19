/*
 * TheNexusAvenger
 * 
 * Specific handlers for client requests.
 */

using Nexus.Http.Server.Http.Response;

namespace Nexus.Http.Server.Http.Request
{
    /*
     * Interface for a client request handler.
     */
    public interface IClientRequestHandler
    {
        /*
         * Returns a response for a given request.
         */
        HttpResponse GetResponseData(HttpRequest request);
    }
}