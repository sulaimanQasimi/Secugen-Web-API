using System;
using System.Drawing;
using System.Text;
using SecuGen.FDxSDKPro.Windows;
using sgdm.Models;

namespace sgdm.Services
{
    /// <summary>
    /// Service class to handle SecuGen fingerprint operations for API
    /// </summary>
    public class FingerprintApiService
    {
        private SGFingerPrintManager m_FPM;
        private Int32 m_ImageWidth;
        private Int32 m_ImageHeight;
        private bool m_DeviceInitialized = false;
        
        public bool IsDeviceInitialized => m_DeviceInitialized;

        public FingerprintApiService()
        {
            try
            {
                Console.WriteLine("Creating SGFingerPrintManager...");
                m_FPM = new SGFingerPrintManager();
                Console.WriteLine("SGFingerPrintManager created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create SGFingerPrintManager: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize the fingerprint device
        /// </summary>
        public bool InitializeDevice()
        {
            try
            {
                Console.WriteLine("Initializing device...");
                if (m_DeviceInitialized)
                {
                    Console.WriteLine("Device already initialized");
                    return true;
                }

                // Enumerate devices
                Console.WriteLine("Enumerating devices...");
                Int32 iError = m_FPM.EnumerateDevice();
                Console.WriteLine($"EnumerateDevice result: {iError}, Number of devices: {m_FPM.NumberOfDevice}");
                
                if (iError != (Int32)SGFPMError.ERROR_NONE || m_FPM.NumberOfDevice == 0)
                {
                    Console.WriteLine("No devices found or enumeration failed");
                    return false;
                }

                // Try to open first available device
                SGFPMDeviceList[] devList = new SGFPMDeviceList[m_FPM.NumberOfDevice];
                for (int i = 0; i < m_FPM.NumberOfDevice; i++)
                {
                    devList[i] = new SGFPMDeviceList();
                    m_FPM.GetEnumDeviceInfo(i, devList[i]);
                }

                // Initialize with first device
                Console.WriteLine($"Initializing device: {devList[0].DevName}, ID: {devList[0].DevID}");
                iError = m_FPM.Init(devList[0].DevName);
                Console.WriteLine($"Init result: {iError}");
                
                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    Console.WriteLine("Opening device...");
                    iError = m_FPM.OpenDevice(devList[0].DevID);
                    Console.WriteLine($"OpenDevice result: {iError}");
                    
                    if (iError == (Int32)SGFPMError.ERROR_NONE)
                    {
                        // Get device info
                        Console.WriteLine("Getting device info for initialization...");
                        SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
                        iError = m_FPM.GetDeviceInfo(pInfo);
                        Console.WriteLine($"GetDeviceInfo result: {iError}");
                        
                        if (iError == (Int32)SGFPMError.ERROR_NONE)
                        {
                            m_ImageWidth = pInfo.ImageWidth;
                            m_ImageHeight = pInfo.ImageHeight;
                            m_DeviceInitialized = true;
                            Console.WriteLine($"Device initialized successfully: {m_ImageWidth}x{m_ImageHeight}");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"GetDeviceInfo failed during initialization: {iError}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"OpenDevice failed: {iError}");
                    }
                }
                else
                {
                    Console.WriteLine($"Init failed: {iError}");
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Capture fingerprint image
        /// </summary>
        public CaptureResponse CaptureFingerprint(CaptureRequest request)
        {
            var response = new CaptureResponse();

            try
            {
                if (!m_DeviceInitialized)
                {
                    Console.WriteLine("Device not initialized, attempting to initialize...");
                    if (!InitializeDevice())
                    {
                        response.Success = false;
                        response.Message = "Device not initialized - please check device connection";
                        return response;
                    }
                    Console.WriteLine("Device initialized successfully");
                }

                Int32 elap_time = Environment.TickCount;
                Byte[] fp_image = new Byte[m_ImageWidth * m_ImageHeight];
                Int32 img_qlty = 0;

                // Capture image
                Console.WriteLine("Attempting to capture image...");
                Int32 iError = m_FPM.GetImage(fp_image);
                Console.WriteLine($"GetImage result: {iError}");
                
                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    elap_time = Environment.TickCount - elap_time;
                    Console.WriteLine($"Image captured in {elap_time}ms");
                    
                    // Get image quality
                    m_FPM.GetImageQuality(m_ImageWidth, m_ImageHeight, fp_image, ref img_qlty);
                    Console.WriteLine($"Image quality: {img_qlty}");

                    // Convert image to Base64
                    string imageBase64 = ConvertImageToBase64(fp_image);
                    Console.WriteLine($"Image converted to Base64, length: {imageBase64.Length}");
                    
                    // Create template
                    Byte[] template = new Byte[400];
                    iError = m_FPM.CreateTemplate(fp_image, template);
                    Console.WriteLine($"CreateTemplate result: {iError}");
                    
                    if (iError == (Int32)SGFPMError.ERROR_NONE)
                    {
                        response.Success = true;
                        response.Message = "Fingerprint captured successfully";
                        response.ImageBase64 = imageBase64;
                        response.Quality = img_qlty;
                        response.CaptureTime = elap_time;
                        response.Template = Convert.ToBase64String(template);
                        Console.WriteLine("Capture completed successfully");
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = "Template creation failed: " + GetErrorMessage("CreateTemplate()", iError);
                        Console.WriteLine($"Template creation failed: {iError}");
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = GetErrorMessage("GetImage()", iError);
                    Console.WriteLine($"GetImage failed: {iError}");
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Exception: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Compare two fingerprint templates
        /// </summary>
        public CompareResponse CompareFingerprints(CompareRequest request)
        {
            var response = new CompareResponse();

            try
            {
                if (!m_DeviceInitialized && !InitializeDevice())
                {
                    response.Success = false;
                    response.Message = "Device not initialized";
                    return response;
                }

                // Convert Base64 templates to byte arrays
                Byte[] template1 = Convert.FromBase64String(request.Template1);
                Byte[] template2 = Convert.FromBase64String(request.Template2);

                // Get security level
                SGFPMSecurityLevel secu_level = GetSecurityLevel(request.SecurityLevel);

                // Match templates
                bool matched = false;
                Int32 match_score = 0;
                Int32 iError = m_FPM.MatchTemplate(template1, template2, secu_level, ref matched);
                
                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    iError = m_FPM.GetMatchingScore(template1, template2, ref match_score);
                    if (iError == (Int32)SGFPMError.ERROR_NONE)
                    {
                        response.Success = true;
                        response.Message = matched ? "Fingerprints match" : "Fingerprints do not match";
                        response.Matched = matched;
                        response.MatchScore = match_score;
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = GetErrorMessage("GetMatchingScore()", iError);
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = GetErrorMessage("MatchTemplate()", iError);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Exception: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Register two fingerprints
        /// </summary>
        public RegisterResponse RegisterFingerprints(RegisterRequest request)
        {
            var response = new RegisterResponse();

            try
            {
                if (!m_DeviceInitialized && !InitializeDevice())
                {
                    response.Success = false;
                    response.Message = "Device not initialized";
                    return response;
                }

                // Convert Base64 templates to byte arrays
                Byte[] template1 = Convert.FromBase64String(request.Template1);
                Byte[] template2 = Convert.FromBase64String(request.Template2);

                // Get security level
                SGFPMSecurityLevel secu_level = GetSecurityLevel(request.SecurityLevel);

                // Match templates for registration
                bool matched = false;
                Int32 match_score = 0;
                Int32 iError = m_FPM.MatchTemplate(template1, template2, secu_level, ref matched);
                
                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    iError = m_FPM.GetMatchingScore(template1, template2, ref match_score);
                    if (iError == (Int32)SGFPMError.ERROR_NONE)
                    {
                        response.Success = true;
                        response.Registered = matched;
                        response.MatchScore = match_score;
                        
                        if (matched)
                        {
                            response.Message = "Registration successful";
                            // For simplicity, we'll use template1 as the registered template
                            // In a real implementation, you might want to store both templates
                            response.RegisteredTemplate = request.Template1;
                        }
                        else
                        {
                            response.Message = "Registration failed - fingerprints do not match";
                        }
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = GetErrorMessage("GetMatchingScore()", iError);
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = GetErrorMessage("MatchTemplate()", iError);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Exception: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Verify fingerprint against registered template
        /// </summary>
        public VerifyResponse VerifyFingerprint(VerifyRequest request)
        {
            var response = new VerifyResponse();

            try
            {
                if (!m_DeviceInitialized && !InitializeDevice())
                {
                    response.Success = false;
                    response.Message = "Device not initialized";
                    return response;
                }

                // Convert Base64 templates to byte arrays
                Byte[] registeredTemplate = Convert.FromBase64String(request.RegisteredTemplate);
                Byte[] verifyTemplate = Convert.FromBase64String(request.VerifyTemplate);

                // Get security level
                SGFPMSecurityLevel secu_level = GetSecurityLevel(request.SecurityLevel);

                // Match templates for verification
                bool matched = false;
                Int32 match_score = 0;
                Int32 iError = m_FPM.MatchTemplate(registeredTemplate, verifyTemplate, secu_level, ref matched);
                
                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    iError = m_FPM.GetMatchingScore(registeredTemplate, verifyTemplate, ref match_score);
                    if (iError == (Int32)SGFPMError.ERROR_NONE)
                    {
                        response.Success = true;
                        response.Verified = matched;
                        response.MatchScore = match_score;
                        response.Message = matched ? "Verification successful" : "Verification failed";
                    }
                    else
                    {
                        response.Success = false;
                        response.Message = GetErrorMessage("GetMatchingScore()", iError);
                    }
                }
                else
                {
                    response.Success = false;
                    response.Message = GetErrorMessage("MatchTemplate()", iError);
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Exception: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Get device information
        /// </summary>
        public DeviceInfoResponse GetDeviceInfo()
        {
            var response = new DeviceInfoResponse();

            try
            {
                Console.WriteLine("Getting device info...");
                if (!m_DeviceInitialized)
                {
                    Console.WriteLine("Device not initialized, attempting to initialize...");
                    if (!InitializeDevice())
                    {
                        response.Success = false;
                        response.Message = "Device not initialized - please check device connection";
                        Console.WriteLine("Device initialization failed");
                        return response;
                    }
                    Console.WriteLine("Device initialized successfully");
                }

                SGFPMDeviceInfoParam pInfo = new SGFPMDeviceInfoParam();
                Console.WriteLine("Calling GetDeviceInfo...");
                Int32 iError = m_FPM.GetDeviceInfo(pInfo);
                Console.WriteLine($"GetDeviceInfo result: {iError}");

                if (iError == (Int32)SGFPMError.ERROR_NONE)
                {
                    response.Success = true;
                    response.Message = "Device information retrieved successfully";
                    response.DeviceID = pInfo.DeviceID;
                    response.SerialNumber = Encoding.ASCII.GetString(pInfo.DeviceSN).TrimEnd('\0');
                    response.ImageWidth = pInfo.ImageWidth;
                    response.ImageHeight = pInfo.ImageHeight;
                    response.ImageDPI = pInfo.ImageDPI;
                    response.FirmwareVersion = pInfo.FWVersion.ToString("X");
                    response.Brightness = pInfo.Brightness;
                    response.Contrast = pInfo.Contrast;
                    response.Gain = pInfo.Gain;
                    Console.WriteLine($"Device info retrieved: ID={pInfo.DeviceID}, Size={pInfo.ImageWidth}x{pInfo.ImageHeight}");
                }
                else
                {
                    response.Success = false;
                    response.Message = GetErrorMessage("GetDeviceInfo()", iError);
                    Console.WriteLine($"GetDeviceInfo failed: {iError}");
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Exception: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Convert fingerprint image to Base64 string
        /// </summary>
        private string ConvertImageToBase64(Byte[] imgData)
        {
            try
            {
                Bitmap bmp = new Bitmap(m_ImageWidth, m_ImageHeight);
                
                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)
                    {
                        int colorval = (int)imgData[(j * m_ImageWidth) + i];
                        bmp.SetPixel(i, j, Color.FromArgb(colorval, colorval, colorval));
                    }
                }

                using (var ms = new System.IO.MemoryStream())
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get security level enum from string
        /// </summary>
        private SGFPMSecurityLevel GetSecurityLevel(string securityLevel)
        {
            switch (securityLevel.ToUpper())
            {
                case "LOWEST": return (SGFPMSecurityLevel)0; // Index 0
                case "LOWER": return (SGFPMSecurityLevel)1;  // Index 1
                case "LOW": return (SGFPMSecurityLevel)2;    // Index 2
                case "BELOW_NORMAL": return (SGFPMSecurityLevel)3; // Index 3
                case "NORMAL": return (SGFPMSecurityLevel)4; // Index 4
                case "ABOVE_NORMAL": return (SGFPMSecurityLevel)5; // Index 5
                case "HIGH": return (SGFPMSecurityLevel)6;   // Index 6
                case "HIGHER": return (SGFPMSecurityLevel)7;  // Index 7
                case "HIGHEST": return (SGFPMSecurityLevel)8; // Index 8
                default: return (SGFPMSecurityLevel)4; // Default to NORMAL (index 4)
            }
        }

        /// <summary>
        /// Get error message from error code
        /// </summary>
        private string GetErrorMessage(string funcName, int iError)
        {
            string text = "";

            switch (iError)
            {
                case 0: text = "Error none"; break;
                case 1: text = "Can not create object"; break;
                case 2: text = "Function Failed"; break;
                case 3: text = "Invalid Parameter"; break;
                case 4: text = "Not used function"; break;
                case 5: text = "Can not create object"; break;
                case 6: text = "Can not load device driver"; break;
                case 7: text = "Can not load sgfpamx.dll"; break;
                case 51: text = "Can not load driver kernel file"; break;
                case 52: text = "Failed to initialize the device"; break;
                case 53: text = "Data transmission is not good"; break;
                case 54: text = "Time out"; break;
                case 55: text = "Device not found"; break;
                case 56: text = "Can not load driver file"; break;
                case 57: text = "Wrong Image"; break;
                case 58: text = "Lack of USB Bandwith"; break;
                case 59: text = "Device is already opened"; break;
                case 60: text = "Device serial number error"; break;
                case 61: text = "Unsupported device"; break;
                case 101: text = "The number of minutiae is too small"; break;
                case 102: text = "Template is invalid"; break;
                case 103: text = "1st template is invalid"; break;
                case 104: text = "2nd template is invalid"; break;
                case 105: text = "Minutiae extraction failed"; break;
                case 106: text = "Matching failed"; break;
                default: text = "Unknown error"; break;
            }

            return funcName + " Error # " + iError + " : " + text;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (m_DeviceInitialized)
            {
                m_FPM?.CloseDevice();
                m_DeviceInitialized = false;
            }
        }
    }
}
