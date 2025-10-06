using System;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using sgdm.Models;
using sgdm.Services;

namespace sgdm
{
    /// <summary>
    /// Simple HTTP server for SecuGen Fingerprint API using HttpListener
    /// This avoids the complex Web API dependencies
    /// </summary>
    public class SimpleFingerprintApiServer
    {
        private HttpListener listener;
        private FingerprintApiService fingerprintService;
        private readonly string baseUrl;

        public SimpleFingerprintApiServer(string baseUrl = "http://localhost:8080")
        {
            this.baseUrl = baseUrl;
            this.fingerprintService = new FingerprintApiService();
        }

        /// <summary>
        /// Start the HTTP server
        /// </summary>
        public async Task StartAsync()
        {
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(baseUrl + "/");
                listener.Start();

                Console.WriteLine($"SecuGen Fingerprint API Server started at {baseUrl}");
                Console.WriteLine("Available endpoints:");
                Console.WriteLine($"  GET  {baseUrl}/api/fingerprint/health");
                Console.WriteLine($"  GET  {baseUrl}/api/fingerprint/device-info");
                Console.WriteLine($"  POST {baseUrl}/api/fingerprint/capture");
                Console.WriteLine($"  POST {baseUrl}/api/fingerprint/compare");
                Console.WriteLine($"  POST {baseUrl}/api/fingerprint/register");
                Console.WriteLine($"  POST {baseUrl}/api/fingerprint/verify");
                Console.WriteLine("\nPress any key to stop the server...");

                // Start listening for requests
                while (listener.IsListening)
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start server: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Process HTTP request
        /// </summary>
        private async Task ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;

                // Add CORS headers
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // Handle preflight requests
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                string responseText = "";
                response.ContentType = "application/json";

                try
                {
                    // Route the request
                    if (request.Url.AbsolutePath.StartsWith("/api/fingerprint/"))
                    {
                        responseText = await HandleApiRequest(request);
                    }
                    else
                    {
                        responseText = JsonConvert.SerializeObject(new { error = "Not found" });
                        response.StatusCode = 404;
                    }
                }
                catch (Exception ex)
                {
                    responseText = JsonConvert.SerializeObject(new { error = ex.Message });
                    response.StatusCode = 500;
                }

                // Send response
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle API requests
        /// </summary>
        private async Task<string> HandleApiRequest(HttpListenerRequest request)
        {
            string path = request.Url.AbsolutePath.ToLower();
            string method = request.HttpMethod.ToUpper();

            switch (path)
            {
                case "/api/fingerprint/health":
                    if (method == "GET")
                    {
                        return JsonConvert.SerializeObject(new { success = true, message = "Fingerprint API is running", timestamp = DateTime.UtcNow });
                    }
                    break;

                case "/api/fingerprint/test":
                    if (method == "GET")
                    {
                        return JsonConvert.SerializeObject(new { 
                            success = true, 
                            message = "Test endpoint working",
                            deviceInitialized = fingerprintService.IsDeviceInitialized,
                            timestamp = DateTime.UtcNow 
                        });
                    }
                    break;

                case "/api/fingerprint/test-capture":
                    if (method == "POST")
                    {
                        try
                        {
                            Console.WriteLine("Test capture endpoint called");
                            var captureRequest = await ReadRequestBody<CaptureRequest>(request);
                            return JsonConvert.SerializeObject(new { 
                                success = true, 
                                message = "Test capture request received",
                                receivedRequest = captureRequest,
                                timestamp = DateTime.UtcNow 
                            });
                        }
                        catch (Exception ex)
                        {
                            return JsonConvert.SerializeObject(new { 
                                success = false, 
                                message = "Test capture failed: " + ex.Message,
                                error = ex.ToString()
                            });
                        }
                    }
                    break;

                case "/api/fingerprint/device-info":
                    if (method == "GET")
                    {
                        try
                        {
                            var deviceInfo = fingerprintService.GetDeviceInfo();
                            return JsonConvert.SerializeObject(deviceInfo);
                        }
                        catch (Exception ex)
                        {
                            return JsonConvert.SerializeObject(new { 
                                success = false, 
                                message = "Device info failed: " + ex.Message,
                                error = ex.ToString()
                            });
                        }
                    }
                    break;

                case "/api/fingerprint/capture":
                    if (method == "POST")
                    {
                        try
                        {
                            Console.WriteLine("Processing capture request...");
                            var captureRequest = await ReadRequestBody<CaptureRequest>(request);
                            Console.WriteLine($"Capture request received: Timeout={captureRequest.Timeout}, Quality={captureRequest.Quality}");
                            
                            var captureResponse = fingerprintService.CaptureFingerprint(captureRequest);
                            Console.WriteLine($"Capture response: Success={captureResponse.Success}, Message={captureResponse.Message}");
                            
                            return JsonConvert.SerializeObject(captureResponse);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Capture endpoint exception: {ex.Message}");
                            Console.WriteLine($"Exception details: {ex}");
                            return JsonConvert.SerializeObject(new { 
                                success = false, 
                                message = "Capture failed: " + ex.Message,
                                error = ex.ToString()
                            });
                        }
                    }
                    break;

                case "/api/fingerprint/compare":
                    if (method == "POST")
                    {
                        var compareRequest = await ReadRequestBody<CompareRequest>(request);
                        var compareResponse = fingerprintService.CompareFingerprints(compareRequest);
                        return JsonConvert.SerializeObject(compareResponse);
                    }
                    break;

                case "/api/fingerprint/register":
                    if (method == "POST")
                    {
                        var registerRequest = await ReadRequestBody<RegisterRequest>(request);
                        var registerResponse = fingerprintService.RegisterFingerprints(registerRequest);
                        return JsonConvert.SerializeObject(registerResponse);
                    }
                    break;

                case "/api/fingerprint/verify":
                    if (method == "POST")
                    {
                        var verifyRequest = await ReadRequestBody<VerifyRequest>(request);
                        var verifyResponse = fingerprintService.VerifyFingerprint(verifyRequest);
                        return JsonConvert.SerializeObject(verifyResponse);
                    }
                    break;
            }

            return JsonConvert.SerializeObject(new { error = "Method not allowed" });
        }

        /// <summary>
        /// Read request body and deserialize to specified type
        /// </summary>
        private async Task<T> ReadRequestBody<T>(HttpListenerRequest request)
        {
            try
            {
                Console.WriteLine($"Reading request body - ContentLength: {request.ContentLength64}");
                
                if (request.ContentLength64 <= 0)
                {
                    Console.WriteLine("No content in request body, returning default instance");
                    return Activator.CreateInstance<T>();
                }
                
                // Read the raw body
                byte[] buffer = new byte[request.ContentLength64];
                await request.InputStream.ReadAsync(buffer, 0, buffer.Length);
                string json = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"Request body JSON: {json}");
                
                if (string.IsNullOrEmpty(json))
                {
                    Console.WriteLine("Empty JSON body, returning default instance");
                    return Activator.CreateInstance<T>();
                }
                
                var result = JsonConvert.DeserializeObject<T>(json);
                Console.WriteLine($"Deserialized object: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading request body: {ex.Message}");
                Console.WriteLine($"Exception details: {ex}");
                return Activator.CreateInstance<T>();
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            if (listener != null && listener.IsListening)
            {
                listener.Stop();
                listener.Close();
                Console.WriteLine("Server stopped.");
            }
        }
    }

    /// <summary>
    /// Console application to run the fingerprint API server
    /// </summary>
    public class FingerprintApiConsole
    {
        private static SimpleFingerprintApiServer server;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("SecuGen Fingerprint API Server");
            Console.WriteLine("==============================");
            
            try
            {
                // Create and start server
                server = new SimpleFingerprintApiServer();
                await server.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }
    }
}
