﻿using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueBroadcastHub.Server
{
    class EmbedIOServer
    {
        private WebServer webServer;

        public static WebSocketIngameServer socketServer;

        public EmbedIOServer(string location, int port)
        {
            var uri = $"http://{location}:{port}/";

            webServer = CreateWebServer(uri);

            webServer.RunAsync();
            System.Diagnostics.Debug.WriteLine($"WebServer running on {uri}");
        }

        public void Restart()
        {

        }

        public void Stop()
        {
            webServer.Dispose();
            System.Diagnostics.Debug.WriteLine($"WebServer stopped");
        }

        private static WebServer CreateWebServer(string url)
        {
            var webRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
            System.Diagnostics.Debug.WriteLine($"File system starting on: {webRoot}");
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                // First, we will configure our web server by adding Modules.
                .WithLocalSessionManager()
                .WithCors()
                .WithModule(socketServer = new WebSocketIngameServer("/api"))
                .WithModule(new FileModule("/cache", 
                    new FileSystemProvider(webRoot, false)) { 
                        DirectoryLister = DirectoryLister.Html })
                //.WithStaticFolder("/", $"{Directory.GetCurrentDirectory()}cache", true, m => m
                //    .WithContentCaching(true)) // Add static files after other modules to avoid conflicts
                //.WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })))
                ;

            // Listen for state changes.
            server.StateChanged += (s, e) => System.Diagnostics.Debug.WriteLine($"WebServer New State - {e.NewState}");

            return server;
        }
    }
}