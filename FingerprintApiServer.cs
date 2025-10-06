using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Http.Cors;
using sgdm.Controllers;

namespace sgdm
{
    /// <summary>
    /// Self-hosted HTTP server for SecuGen Fingerprint API
    /// </summary>
    public class FingerprintApiServer
    {
        private HttpSelfHostServer server;
        private readonly string baseAddress;

        public FingerprintApiServer(string baseAddress = "http://localhost:8080")
        {
            this.baseAddress = baseAddress;
        }

        /// <summary>
        /// Start the API server
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                var config = new HttpSelfHostConfiguration(baseAddress);
                
                // Enable CORS for all origins, headers, and methods
                config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
                
                // Configure JSON formatter
                config.Formatters.JsonFormatter.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                config.Formatters.Remove(config.Formatters.XmlFormatter);
                
                // Configure routing
                config.Routes.MapHttpRoute(
                    name: "DefaultApi",
                    routeTemplate: "api/{controller}/{action}/{id}",
                    defaults: new { id = RouteParameter.Optional }
                );
                
                // Add controllers
                config.Services.Replace(typeof(System.Web.Http.Dispatcher.IHttpControllerSelector), 
                    new System.Web.Http.Dispatcher.DefaultHttpControllerSelector(config));
                
                // Create and start server
                server = new HttpSelfHostServer(config);
                await server.OpenAsync();
                
                Console.WriteLine($"SecuGen Fingerprint API Server started at {baseAddress}");
                Console.WriteLine("Available endpoints:");
                Console.WriteLine($"  GET  {baseAddress}/api/fingerprint/health");
                Console.WriteLine($"  GET  {baseAddress}/api/fingerprint/device-info");
                Console.WriteLine($"  POST {baseAddress}/api/fingerprint/capture");
                Console.WriteLine($"  POST {baseAddress}/api/fingerprint/compare");
                Console.WriteLine($"  POST {baseAddress}/api/fingerprint/register");
                Console.WriteLine($"  POST {baseAddress}/api/fingerprint/verify");
                Console.WriteLine("\nPress any key to stop the server...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop the API server
        /// </summary>
        public async Task StopAsync()
        {
            if (server != null)
            {
                await server.CloseAsync();
                server.Dispose();
                Console.WriteLine("Server stopped.");
            }
        }

        /// <summary>
        /// Check if server is running
        /// </summary>
        public bool IsRunning => server != null;
    }

    /// <summary>
    /// Main program to run the API server
    /// </summary>
    public class ApiServerProgram
    {
        private static FingerprintApiServer apiServer;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("SecuGen Fingerprint API Server");
            Console.WriteLine("==============================");
            
            try
            {
                // Create and start server
                apiServer = new FingerprintApiServer();
                await apiServer.StartAsync();
                
                // Wait for key press to stop
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (apiServer != null)
                {
                    await apiServer.StopAsync();
                }
            }
        }
    }
}
