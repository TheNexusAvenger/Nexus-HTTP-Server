﻿/*
 * TheNexusAvenger
 * 
 * Handles "split" requests from a client.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using Nexus.Http.Server.Http.Request;
using Nexus.Http.Server.Http.Response;
using Nexus.Http.Server.SplitHttp.Response;

namespace Nexus.Http.Server.SplitHttp.Request
{
    /*
     * Data class for a partial response.
     */
    public class PartialResponseData
    {
        public string status;
        public int id;
        public int currentPacket;
        public int maxPackets;
        public string packet;
    }
    
    /*
     * Class representing a split client request handler.
     */
    public abstract class SplitClientRequestHandler : IClientRequestHandler
    {
        private TemporaryStorage<PartialHttpRequest> IncompleteRequests;
        private TemporaryStorage<PartialResponse> UnfinishedResponses;

        /*
         * Creates a split client request handler object.
         */
        public SplitClientRequestHandler()
        {
            this.IncompleteRequests = new TemporaryStorage<PartialHttpRequest>();
            this.UnfinishedResponses = new TemporaryStorage<PartialResponse>();
        }

        /*
         * Returns a response for a given request.
         */
        public HttpResponse GetResponseData(HttpRequest request)
        {
            var url = request.GetURL();

            if (url.ParameterExists("getResponse") && url.GetParameter("getResponse").ToLower() == "true")
            {
                // Handle reading a split response.
                var responseId = 0;
                var packetId = 0;

                // Get the packet info.
                if (url.ParameterExists("responseId"))
                {
                    responseId = int.Parse(url.GetParameter("responseId"));
                }
                if (url.ParameterExists("packet"))
                {
                    packetId = int.Parse(url.GetParameter("packet"));
                }

                // Return the partial packet.
                return this.GetPartialResponse(responseId,packetId);
            } else
            {
                // Handle sending a split request.
                var requestId = -1;
                var packetId = -1;
                var maxPackets = -1;
                PartialHttpRequest completeRequest;

                // Get the packet info.
                if (url.ParameterExists("requestId"))
                {
                    requestId = int.Parse(url.GetParameter("requestId"));
                }
                if (url.ParameterExists("packet"))
                {
                    packetId = int.Parse(url.GetParameter("packet"));
                }
                if (url.ParameterExists("maxpackets"))
                {
                    maxPackets = int.Parse(url.GetParameter("maxpackets"));
                }
                
                // Treat the request as standalone if the parameters aren't specified.
                if (requestId == -1 && packetId == -1 && maxPackets == -1) {
                    return GetCompleteResponseData(request);
                }
                else {
                    if (requestId == -1)
                    {
                        packetId = 0;
                    }

                    if (maxPackets == -1)
                    {
                        maxPackets = 1;
                    }
                }

                // Get the split request to use.
                if (requestId == -1)
                {
                    completeRequest = new PartialHttpRequest(request.GetRequestType(),url,request.GetHost(),maxPackets);
                    requestId = this.IncompleteRequests.Store(completeRequest);
                } else
                {
                    completeRequest = this.IncompleteRequests.Get(requestId);
                }

                // Add the packet to the request.
                completeRequest.AddPartialPacket(packetId,request.GetBody());

                // Return the response.
                if (completeRequest.IsComplete())
                {
                    this.IncompleteRequests.Remove(requestId);
                    var partialResponse = PartialResponse.SplitResponse(this.GetCompleteResponseData(completeRequest.ToSingleRequest()));
                    
                    var responseId = this.UnfinishedResponses.Store(partialResponse);
                    return this.GetPartialResponse(responseId,0);
                } else
                {
                    return this.CreateIncompleteResponse(requestId);
                }
            }
        }

        /*
         * Returns the response of a partial response and clears it if it is read.
         */
        public HttpResponse GetPartialResponse(int responseId,int packetId)
        {
            // Get the partial response and response to return.
            PartialResponse completeResponse = this.UnfinishedResponses.Get(responseId);
            HttpResponse response = completeResponse.GetResponseFromId(packetId);

            // Remove the response if it has been full read.
            if (completeResponse.AllResponsesSent())
            {
                this.UnfinishedResponses.Remove(responseId);
            }

            // Return the response.
            return CreatePartialResponse(responseId,packetId,completeResponse.GetNumberOfResponses() - 1,response.GetResponseData());
        }

        /*
         * Creates an error response.
         */
        public HttpResponse CreateInvalidResponse(string message)
        {
            // Create the request.
            var requestData = new Dictionary<string,string>();
            requestData.Add("status","error");
            requestData.Add("message",message);
            
            // Return the formatted request.
            var request = JsonConvert.SerializeObject(requestData);
            return HttpResponse.CreateBadRequestResponse(request);
        }

        /*
         * Creates an incomplete response.
         */
        public HttpResponse CreateIncompleteResponse(int packetId)
        {
            return HttpResponse.CreateSuccessResponse("{\"status\":\"incomplete\",\"id\":" + packetId + "}");
        }

        /*
         * Creates a partial response.
         */
        public HttpResponse CreatePartialResponse(int packetId,int currentPacket,int maxPacket,string message)
        {
            // Create the request.
            var requestData = new PartialResponseData();
            requestData.status = "success";
            requestData.id = packetId;
            requestData.currentPacket = currentPacket;
            requestData.maxPackets = maxPacket;
            requestData.packet = message;
            
            // Return the formatted request.
            var request = JsonConvert.SerializeObject(requestData);
            return HttpResponse.CreateSuccessResponse(request);
        }

        /*
         * Returns a response for a given complete request.
         */
        public abstract HttpResponse GetCompleteResponseData(HttpRequest request);
    }
}