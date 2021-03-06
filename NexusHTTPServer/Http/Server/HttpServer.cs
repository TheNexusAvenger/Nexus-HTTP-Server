﻿/*
 * TheNexusAvenger
 * 
 * Class that handles accepts client connections.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Http.Server.Http.Request;
using HttpListener = WebSocketSharp.Net.HttpListener;
using HttpListenerContext = WebSocketSharp.Net.HttpListenerContext;

namespace Nexus.Http.Server.Http.Server
{
    /*
     * Class for the server.
     */
    public class HttpServer
    {
        private List<string> Prefixes;
        private bool Running;
        private HttpListener Listener;
        private RequestHandler Handlers;
        private EventWaitHandle ConnectionAcceptedEvent;
        private EventWaitHandle ConnectionLoopEndedEvent;

        /*
         * Creates a server object.
         */
        public HttpServer(RequestHandler requestHandler)
        {
            this.Prefixes = new List<string>();
            this.Running = false;
            this.Handlers = requestHandler;
            this.ConnectionAcceptedEvent = new EventWaitHandle(false,EventResetMode.AutoReset);
            this.ConnectionLoopEndedEvent = new EventWaitHandle(false,EventResetMode.AutoReset);
        }
        
        /*
         * Creates a server object.
         */
        public HttpServer(int port,RequestHandler requestHandler) : this(requestHandler)
        {
            this.AddPrefix("http://*:" + port + "/");
        }

        /*
         * Adds a prefix to listen to.
         */
        public void AddPrefix(string prefix)
        {
            this.Prefixes.Add(prefix);
        }

        /*
         * Handles a new request.
         */
        public void HandleRequest(HttpListenerContext httpRequestContext)
        {
            // Create and start the handler.
            var handler = new ContextHandler(httpRequestContext,this.Handlers);
            handler.StartHandling();
        }

        /*
         * Starts the server.
         */
        public void Start()
        {
            // Throw an exception if the server is running.
            if (this.Running)
            {
                throw new WebException("Server is already running. Stop the server before running.");
            }
            
            // Set up the HTTP listener.
            this.Running = true;
            this.Listener = new HttpListener();
            foreach (var prefix in this.Prefixes)
            {
                this.Listener.Prefixes.Add(prefix);
            }
            this.Listener.Start();
            
            // Run a loop to accept client connections until it is closed.
            while (Running)
            {
                // Start the context fetching async and wait for it to be completed or to be cancelled.
                var task = Task.Run(() => this.Listener.GetContext());
                task.ContinueWith((result) =>
                {
                    this.HandleRequest(result.Result);
                    this.ConnectionAcceptedEvent.Set();
                });
                
                // Wait for a connection to be accepted or the listener to close.
                this.ConnectionAcceptedEvent.WaitOne();
            }

            // Signal that the loop ended.
            this.ConnectionLoopEndedEvent.Set();
        }

        /*
         * Stops the server.
         */
        public void Stop()
        {
            // Throw an exception if the server isn't running.
            if (!this.Running)
            {
                throw new WebException("Server isn't running. Start the server before stopping.");
            }
            
            // Close the listener.
            this.Running = false;
            foreach (var prefix in this.Listener.Prefixes.ToArray())
            {
                this.Listener.Prefixes.Remove(prefix);
            }
            this.Listener.Stop();
            this.Listener = null;
            
            // Signal to end the loop and wait for the loop to end.
            this.ConnectionAcceptedEvent.Set();
            this.ConnectionLoopEndedEvent.WaitOne();
        }
    }
}