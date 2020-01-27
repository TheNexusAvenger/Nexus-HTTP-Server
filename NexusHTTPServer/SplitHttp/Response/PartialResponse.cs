/*
 * TheNexusAvenger
 * 
 * Splits up responses.
 */

using System.Collections.Generic;
using Nexus.Http.Server.Http.Response;

namespace Nexus.Http.Server.SplitHttp.Response
{
    /*
     * Class for spliting up responses.
     */
    public class PartialResponse : HttpResponse
    {
        public const int DEFAULT_MAX_RESPONSE_LENGTH = 64000;

        private List<HttpResponse> Responses;
        private bool[] ResponsesRead;
        private int ResponsesToRead;

        /*
         * Creates a partial response.
         */
        public PartialResponse(List<HttpResponse> responses,string fullMessage) : base(responses[0].GetStatus(), responses[0].GetMimeType(), fullMessage)
        {
            this.Responses = responses;
            this.ResponsesToRead = responses.Count;

            // Set up tbe bools.
            this.ResponsesRead = new bool[responses.Count];
            for (var i = 0; i < responses.Count; i++)
            {
                this.ResponsesRead[i] = false;
            }
        }

        /*
         * Creates a PartialResponse from a request and a max response size.
         */
        public static PartialResponse SplitResponse(HttpResponse response,int maxLength)
        {
            // Get the base response data.
            var status = response.GetStatus();
            var mimeType = response.GetMimeType();
            var completeResponseData = response.GetResponseData();

            // Split the responses.
            var splitResponses = new List<HttpResponse>();
            var remainingResponseData = completeResponseData;
            while (remainingResponseData.Length != 0)
            {
                // Create the response.
                HttpResponse newResponse = null;
                if (remainingResponseData.Length <= maxLength)
                {
                    newResponse = new HttpResponse(status,mimeType,remainingResponseData);
                    remainingResponseData = "";
                } else
                {
                    newResponse = new HttpResponse(status,mimeType,remainingResponseData.Substring(0,maxLength));
                    remainingResponseData = remainingResponseData.Substring(maxLength);
                }
                
                // Add the headers and add the response object.
                foreach (var header in response.GetHeaders())
                {
                    newResponse.AddHeader(header.Key,header.Value);
                }
                splitResponses.Add(newResponse);
            }

            // Return the partial response.
            return new PartialResponse(splitResponses, completeResponseData);
        }

        /*
         * Creates a PartialResponse from a request and a max response size.
         */
        public static PartialResponse SplitResponse(HttpResponse response)
        {
            return SplitResponse(response,DEFAULT_MAX_RESPONSE_LENGTH);
        }

        /*
         * Returns the amount of responses.
         */
        public int GetNumberOfResponses()
        {
            return this.Responses.Count;
        }

        /*
         * Returns the response for the id.
         */
        public HttpResponse GetResponseFromId(int index)
        {
            // Mark the response as read.
            if (this.ResponsesRead[index] == false)
            {
                this.ResponsesRead[index] = true;
                this.ResponsesToRead += -1;
            }

            // Return the response.
            return this.Responses[index];
        }

        /*
         * Returns if all of the responses have been sent.
         */
        public bool AllResponsesSent()
        {
            return (this.ResponsesToRead == 0);
        }
    }
}