using System;

namespace sgdm.Models
{
    /// <summary>
    /// Request model for capturing fingerprint
    /// </summary>
    public class CaptureRequest
    {
        /// <summary>
        /// Timeout for capture operation in milliseconds
        /// </summary>
        public int Timeout { get; set; } = 10000;
        
        /// <summary>
        /// Image quality threshold (0-100)
        /// </summary>
        public int Quality { get; set; } = 50;
    }

    /// <summary>
    /// Response model for capture operation
    /// </summary>
    public class CaptureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ImageBase64 { get; set; }
        public int Quality { get; set; }
        public int CaptureTime { get; set; }
        public string Template { get; set; } // Base64 encoded template
    }

    /// <summary>
    /// Request model for comparing fingerprints
    /// </summary>
    public class CompareRequest
    {
        /// <summary>
        /// First fingerprint template (Base64 encoded)
        /// </summary>
        public string Template1 { get; set; }
        
        /// <summary>
        /// Second fingerprint template (Base64 encoded)
        /// </summary>
        public string Template2 { get; set; }
        
        /// <summary>
        /// Security level for matching
        /// </summary>
        public string SecurityLevel { get; set; } = "NORMAL";
    }

    /// <summary>
    /// Response model for compare operation
    /// </summary>
    public class CompareResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Matched { get; set; }
        public int MatchScore { get; set; }
    }

    /// <summary>
    /// Request model for registration (two fingerprints)
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// First fingerprint template (Base64 encoded)
        /// </summary>
        public string Template1 { get; set; }
        
        /// <summary>
        /// Second fingerprint template (Base64 encoded)
        /// </summary>
        public string Template2 { get; set; }
        
        /// <summary>
        /// Security level for registration
        /// </summary>
        public string SecurityLevel { get; set; } = "NORMAL";
    }

    /// <summary>
    /// Response model for registration
    /// </summary>
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Registered { get; set; }
        public int MatchScore { get; set; }
        public string RegisteredTemplate { get; set; } // Combined template
    }

    /// <summary>
    /// Request model for verification
    /// </summary>
    public class VerifyRequest
    {
        /// <summary>
        /// Registered fingerprint template (Base64 encoded)
        /// </summary>
        public string RegisteredTemplate { get; set; }
        
        /// <summary>
        /// Verification fingerprint template (Base64 encoded)
        /// </summary>
        public string VerifyTemplate { get; set; }
        
        /// <summary>
        /// Security level for verification
        /// </summary>
        public string SecurityLevel { get; set; } = "NORMAL";
    }

    /// <summary>
    /// Response model for verification
    /// </summary>
    public class VerifyResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Verified { get; set; }
        public int MatchScore { get; set; }
    }

    /// <summary>
    /// Device information response
    /// </summary>
    public class DeviceInfoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int DeviceID { get; set; }
        public string SerialNumber { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int ImageDPI { get; set; }
        public string FirmwareVersion { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int Gain { get; set; }
    }

    /// <summary>
    /// Error response model
    /// </summary>
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; }
        public string ErrorCode { get; set; }
    }
}
