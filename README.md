# SecuGen Fingerprint API Integration

This project extends the existing SecuGen fingerprint matching application with a comprehensive Web API that enables JavaScript-based fingerprint capture, comparison, and matching operations.

## 🚀 Features

### New API Capabilities
- **Simple HTTP API** for fingerprint operations (no complex Web API dependencies)
- **Base64 Image Support** - Capture and return fingerprint images as Base64 strings
- **JavaScript Integration** - Complete JavaScript client library
- **CORS Support** - Cross-origin requests enabled
- **Real-time Operations** - Capture, compare, register, and verify fingerprints
- **Device Information** - Get detailed device specifications
- **Error Handling** - Comprehensive error reporting
- **Integrated UI** - Start/Stop API server from the main application

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/fingerprint/health` | Health check |
| `GET` | `/api/fingerprint/device-info` | Get device information |
| `POST` | `/api/fingerprint/capture` | Capture fingerprint image |
| `POST` | `/api/fingerprint/compare` | Compare two templates |
| `POST` | `/api/fingerprint/register` | Register two fingerprints |
| `POST` | `/api/fingerprint/verify` | Verify against registered template |

## 📁 Project Structure

```
├── Models/
│   └── FingerprintApiModels.cs        # Request/response models
├── Services/
│   └── FingerprintApiService.cs      # Business logic service
├── www/
│   ├── index.html                    # Demo web page
│   └── js/
│       └── fingerprint-api.js        # JavaScript client
├── FingerprintApiConsole.cs          # Simple HTTP server
├── mainform.cs                       # Original WinForms application with API controls
└── packages.config                   # NuGet packages
```

## 🛠️ Setup Instructions

### Prerequisites
- Visual Studio 2015 or later
- .NET Framework 4.8
- SecuGen fingerprint device and drivers
- SecuGen.FDxSDKPro.Windows.dll

### Installation

1. **Install NuGet Packages**
   ```bash
   Install-Package Newtonsoft.Json -Version 13.0.3
   ```

2. **Build the Project**
   - Open `matching_cs2015.sln` in Visual Studio
   - Restore NuGet packages
   - Build the solution

3. **Run the Application**
   - Run the WinForms application
   - Click "Start API Server" button to start the HTTP API
   - The API will be available at `http://localhost:8080`

## 🚀 Usage

### Starting the API Server

**Option 1: Using the WinForms Application**
1. Run the application
2. Click "Start API Server" button
3. API will be available at `http://localhost:8080`

**Option 2: Using Console Application**
```csharp
// Run the console application
FingerprintApiConsole.Main(args);
```

### JavaScript API Usage

```javascript
// Initialize the API client
const api = new FingerprintAPI('http://localhost:8080/api/fingerprint');

// Capture fingerprint
const captureResponse = await api.capture({
    timeout: 10000,
    quality: 50
});

// Display captured image
api.displayImage(captureResponse.imageBase64, 'imageElement');

// Compare two templates
const compareResponse = await api.compare(template1, template2, 'NORMAL');

// Register fingerprints
const registerResponse = await api.register(template1, template2, 'NORMAL');

// Verify fingerprint
const verifyResponse = await api.verify(registeredTemplate, verifyTemplate, 'NORMAL');
```

### API Request Examples

#### Capture Fingerprint
```http
POST /api/fingerprint/capture
Content-Type: application/json

{
    "timeout": 10000,
    "quality": 50
}
```

#### Compare Templates
```http
POST /api/fingerprint/compare
Content-Type: application/json

{
    "template1": "base64_encoded_template_1",
    "template2": "base64_encoded_template_2",
    "securityLevel": "NORMAL"
}
```

#### Register Fingerprints
```http
POST /api/fingerprint/register
Content-Type: application/json

{
    "template1": "base64_encoded_template_1",
    "template2": "base64_encoded_template_2",
    "securityLevel": "NORMAL"
}
```

#### Verify Fingerprint
```http
POST /api/fingerprint/verify
Content-Type: application/json

{
    "registeredTemplate": "base64_encoded_registered_template",
    "verifyTemplate": "base64_encoded_verify_template",
    "securityLevel": "NORMAL"
}
```

## 📱 Demo Web Interface

1. **Start the API server**
2. **Open `www/index.html` in a web browser**
3. **Configure the API URL** (default: `http://localhost:8080/api/fingerprint`)
4. **Click "Connect"** to establish connection
5. **Use the interface to:**
   - Get device information
   - Capture fingerprints
   - Compare fingerprints
   - Register fingerprints
   - Verify fingerprints

## 🔧 Configuration

### API Server Configuration
```csharp
// Customize server settings
var config = new HttpSelfHostConfiguration("http://localhost:8080");
config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
```

### Security Levels
- `LOWEST` - Most permissive matching
- `LOWER` - Low security
- `LOW` - Low security
- `BELOW_NORMAL` - Below normal security
- `NORMAL` - Normal security (default)
- `ABOVE_NORMAL` - Above normal security
- `HIGH` - High security
- `HIGHER` - Higher security
- `HIGHEST` - Most restrictive matching

## 🐛 Troubleshooting

### Common Issues

1. **Device Not Found**
   - Ensure SecuGen device is connected
   - Check device drivers are installed
   - Verify device is not in use by another application

2. **API Connection Failed**
   - Check if port 8080 is available
   - Ensure firewall allows the connection
   - Verify the API server is running

3. **CORS Errors**
   - Ensure CORS is properly configured
   - Check browser console for specific errors
   - Verify API URL is correct

### Error Codes
- `0` - Success
- `1-7` - General errors
- `51-61` - Device errors
- `101-106` - Template/matching errors

## 📊 Response Formats

### Success Response
```json
{
    "success": true,
    "message": "Operation completed successfully",
    "data": { ... }
}
```

### Error Response
```json
{
    "success": false,
    "message": "Error description",
    "errorCode": "ERROR_CODE"
}
```

## 🔒 Security Considerations

- **CORS Configuration**: Currently set to allow all origins for development
- **Template Storage**: Templates are Base64 encoded but not encrypted
- **Network Security**: Use HTTPS in production environments
- **Access Control**: Implement authentication/authorization as needed

## 📈 Performance

- **Capture Time**: Typically 100-500ms depending on device
- **Template Size**: ~400 bytes per template
- **Image Size**: Varies by device resolution
- **Concurrent Requests**: Limited by device hardware

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📄 License

This project extends the existing SecuGen sample application. Please refer to SecuGen's licensing terms for the SDK components.

## 🆘 Support

For technical support:
- Check the troubleshooting section
- Review the API documentation
- Test with the demo web interface
- Verify device connectivity and drivers
