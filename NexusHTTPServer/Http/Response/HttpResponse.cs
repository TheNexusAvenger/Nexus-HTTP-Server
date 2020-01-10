/*
 * TheNexusAvenger
 * 
 * Sends responses back to the client.
 */

using WebSocketSharp.Net;
using System.Text;
using WebSocketSharp;

namespace Nexus.Http.Server.Http.Response
{
    /*
     * Handles sending responses.
     */
    public class HttpResponse
    {
        private int Status;
        private string MimeType;
        private string ResponseData;

        /*
         * Creates a response.
         */
        public HttpResponse(int status,string mimetype,string responseData)
        {
            this.Status = status;
            this.MimeType = mimetype;
            this.ResponseData = responseData;
        }

        /*
         * Creates a success response (HTTP 200).
         */
        public static HttpResponse CreateSuccessResponse(string responseData)
        {
            return new HttpResponse(200,"text/html",responseData);
        }

        /*
         * Creates a bad request response (HTTP 400).
         */
        public static HttpResponse CreateBadRequestResponse(string responseData)
        {
            return new HttpResponse(400,"text/html",responseData);
        }

        /*
         * Returns the status.
         */
        public int GetStatus()
        {
            return this.Status;
        }

        /*
         * Returns the mime type.
         */
        public string GetMimeType()
        {
            return this.MimeType;
        }

        /*
         * Returns the response data.
         */
        public string GetResponseData()
        {
            return this.ResponseData;
        }

        /*
         * Sends a response to the client.
         */
        public void SendResponse(HttpListenerContext requestContext)
        {
            // Set up the response.
            var httpResponse = requestContext.Response;
            httpResponse.ContentEncoding = Encoding.UTF8;
            httpResponse.StatusCode = this.Status;

            // Send the data.
            var contentBytes = Encoding.UTF8.GetBytes(this.ResponseData);
            httpResponse.ContentLength64 = contentBytes.LongLength;
            var outputStream = httpResponse.OutputStream;
            outputStream.Write(contentBytes,0,(int) contentBytes.LongLength);
            outputStream.Close();
        }
    }
}