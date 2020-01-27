/*
 * TheNexusAvenger
 * 
 * Stores the request handlers for the server.
 */

using System;
using System.Collections.Generic;
using Nexus.Http.Server.Http.Response;

namespace Nexus.Http.Server.Http.Request
{
    public class RequestHandler
    {
        private Dictionary<string,Dictionary<string,IClientRequestHandler>> Handlers;

        /*
         * Creates a request handler.
         */
        public RequestHandler()
        {
            this.Handlers = new Dictionary<string,Dictionary<string,IClientRequestHandler>>();
        }

        /*
         * Returns the request handler for the given request.
         */
        public IClientRequestHandler GetRequestHandler(HttpRequest request)
        {
            // Get the type and base url.
            var requestType = request.GetRequestType().ToLower();
            var baseURL = request.GetURL().GetBaseURL().ToLower();

            // Return if no handler for the type exists.
            if (!this.Handlers.ContainsKey(requestType))
            {
                return null;
            }

            // Return if no handler for the request exists.
            var typeRequests = this.Handlers[requestType];
            if (!typeRequests.ContainsKey(baseURL))
            {
                return null;
            }

            // Return the request handler.
            return typeRequests[baseURL];
        }

        /*
         * Returns the response for a request.
         */
        public virtual HttpResponse GetResponse(HttpRequest request)
        {
            // Get the handler.
            var handler = this.GetRequestHandler(request);

            // If the handler exists, return the response.
            if (handler != null)
            {
                try
                {
                    return handler.GetResponseData(request);
                }
                catch (Exception error)
                {
                    var errorMessage = error.GetType().Name + ": " + error.Message + "\n" + error.StackTrace;
                    Console.WriteLine(errorMessage);
                    return new HttpResponse(500,"text/html","Server error.\n" +  errorMessage);
                }
            }

            // Return an invalid response error.
            return HttpResponse.CreateBadRequestResponse("Invalid request");
        }

        /*
         * Registers a request handler.
         */
        public void RegisterHandler(string requestType,string urlBase,IClientRequestHandler clientRequestHandler)
        {
            requestType = requestType.ToLower();
            urlBase = URL.FromString(urlBase).GetBaseURL().ToLower();

            // Add the request type handler if it doesn't exist.
            if (!this.Handlers.ContainsKey(requestType))
            {
                this.Handlers.Add(requestType,new Dictionary<string,IClientRequestHandler>());
            }

            //Add the handler.
            this.Handlers[requestType].Add(urlBase,clientRequestHandler);
        }
    }
}