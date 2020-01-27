/*
 * TheNexusAvenger
 * 
 * Stores information about requests.
 */

using System.Collections.Generic;
using System.Text;

namespace Nexus.Http.Server.Http.Request
{
    /*
     * Data class representing a request.
     */
    public class HttpRequest
    {
        private string Type;
        private URL Target;
        private string Host;
        private byte[] Body;
        private Dictionary<string,string> Headers;

        /*
         * Creates a request object.
         */
        public HttpRequest(string type,URL target,string host,byte[] body)
        {
            this.Type = type;
            this.Target = target;
            this.Host = host;
            this.Body = body;
            this.Headers = new Dictionary<string,string>();
        }

        /*
         * Creates a request object.
         */
        public HttpRequest(string type,URL target,string host,string body) : this(type,target,host,Encoding.UTF8.GetBytes(body))
        {
            
        }

        /*
         * Creates a request object.
         */
        public HttpRequest(string type,string target,string host,string body) : this(type,URL.FromString(target),host,body)
        {
            
        }

        /*
         * Returns the request type.
         */
        public string GetRequestType()
        {
            return this.Type;
        }

        /*
         * Returns the request URL.
         */
        public URL GetURL()
        {
            return this.Target;
        }

        /*
         * Returns the request host.
         */
        public string GetHost()
        {
            return this.Host;
        }

        /*
         * Returns the request body.
         */
        public byte[] GetBody()
        {
            return this.Body;
        }
        
        /*
         * Returns the value of a header.
         * If it doesn't exist, null is returned.
         */
        public string GetHeader(string header)
        {
            header = header.ToLower();
            
            // Return null if the header doesn't exist.
            if (!this.Headers.ContainsKey(header))
            {
                return null;
            }
            
            // Return the header.
            return this.Headers[header];
        }
        
        /*
         * Returns all of the headers of the request.
         */
        public Dictionary<string,string> GetHeaders()
        {
            return new Dictionary<string,string>(this.Headers);
        }
        
        /*
         * Adds a header to the request.
         */
        public void AddHeader(string name, string value)
        {
            this.Headers[name.ToLower()] = value;
        }
    }
}