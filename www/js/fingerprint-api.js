/**
 * SecuGen Fingerprint API JavaScript Client
 * Provides methods to interact with the SecuGen fingerprint API
 */
class FingerprintAPI {
    constructor(baseUrl = 'http://localhost:8080/api/fingerprint') {
        this.baseUrl = baseUrl;
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
    }

    /**
     * Make HTTP request to the API
     * @param {string} endpoint - API endpoint
     * @param {string} method - HTTP method
     * @param {Object} data - Request data
     * @returns {Promise} - API response
     */
    async makeRequest(endpoint, method = 'GET', data = null) {
        const url = `${this.baseUrl}${endpoint}`;
        const options = {
            method: method,
            headers: this.defaultHeaders
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }

        try {
            console.log('Making API request:', { url, method, options });
            const response = await fetch(url, options);
            console.log('HTTP response status:', response.status);
            
            const result = await response.json();
            console.log('Parsed response:', result);
            
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${result.message || result.Message || 'Unknown error'}`);
            }
            
            return result;
        } catch (error) {
            console.error('API Request failed:', error);
            throw error;
        }
    }

    /**
     * Capture fingerprint image
     * @param {Object} options - Capture options
     * @param {number} options.timeout - Capture timeout in milliseconds
     * @param {number} options.quality - Image quality threshold
     * @returns {Promise<Object>} - Capture response with Base64 image and template
     */
    async capture(options = {}) {
        const request = {
            timeout: options.timeout || 10000,
            quality: options.quality || 50
        };

        return await this.makeRequest('/capture', 'POST', request);
    }

    /**
     * Compare two fingerprint templates
     * @param {string} template1 - First template (Base64)
     * @param {string} template2 - Second template (Base64)
     * @param {string} securityLevel - Security level for matching
     * @returns {Promise<Object>} - Compare response
     */
    async compare(template1, template2, securityLevel = 'NORMAL') {
        const request = {
            template1: template1,
            template2: template2,
            securityLevel: securityLevel
        };

        return await this.makeRequest('/compare', 'POST', request);
    }

    /**
     * Register two fingerprints (enrollment)
     * @param {string} template1 - First template (Base64)
     * @param {string} template2 - Second template (Base64)
     * @param {string} securityLevel - Security level for registration
     * @returns {Promise<Object>} - Register response
     */
    async register(template1, template2, securityLevel = 'NORMAL') {
        const request = {
            template1: template1,
            template2: template2,
            securityLevel: securityLevel
        };

        return await this.makeRequest('/register', 'POST', request);
    }

    /**
     * Verify fingerprint against registered template
     * @param {string} registeredTemplate - Registered template (Base64)
     * @param {string} verifyTemplate - Template to verify (Base64)
     * @param {string} securityLevel - Security level for verification
     * @returns {Promise<Object>} - Verify response
     */
    async verify(registeredTemplate, verifyTemplate, securityLevel = 'NORMAL') {
        const request = {
            registeredTemplate: registeredTemplate,
            verifyTemplate: verifyTemplate,
            securityLevel: securityLevel
        };

        return await this.makeRequest('/verify', 'POST', request);
    }

    /**
     * Get device information
     * @returns {Promise<Object>} - Device information
     */
    async getDeviceInfo() {
        return await this.makeRequest('/device-info');
    }

    /**
     * Health check
     * @returns {Promise<Object>} - Health status
     */
    async health() {
        return await this.makeRequest('/health');
    }

    /**
     * Test capture request format
     * @param {Object} options - Capture options
     * @returns {Promise<Object>} - Test response
     */
    async testCapture(options = {}) {
        const request = {
            timeout: options.timeout || 10000,
            quality: options.quality || 50
        };

        return await this.makeRequest('/test-capture', 'POST', request);
    }

    /**
     * Display fingerprint image from Base64 data
     * @param {string} base64Image - Base64 encoded image
     * @param {string} elementId - HTML element ID to display image
     */
    displayImage(base64Image, elementId) {
        const imgElement = document.getElementById(elementId);
        if (imgElement && base64Image) {
            imgElement.src = `data:image/png;base64,${base64Image}`;
            imgElement.style.display = 'block';
        }
    }

    /**
     * Download fingerprint image
     * @param {string} base64Image - Base64 encoded image
     * @param {string} filename - Download filename
     */
    downloadImage(base64Image, filename = 'fingerprint.png') {
        if (base64Image) {
            const link = document.createElement('a');
            link.href = `data:image/png;base64,${base64Image}`;
            link.download = filename;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        }
    }

    /**
     * Convert Base64 image to Blob
     * @param {string} base64Image - Base64 encoded image
     * @returns {Blob} - Image blob
     */
    base64ToBlob(base64Image) {
        const byteCharacters = atob(base64Image);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        return new Blob([byteArray], { type: 'image/png' });
    }
}

/**
 * Fingerprint API Demo Class
 * Provides example usage of the FingerprintAPI
 */
class FingerprintAPIDemo {
    constructor(apiBaseUrl) {
        this.api = new FingerprintAPI(apiBaseUrl);
        this.templates = {
            template1: null,
            template2: null,
            registered: null
        };
    }

    /**
     * Initialize the demo
     */
    async init() {
        try {
            // Check API health
            const health = await this.api.health();
            console.log('API Health:', health);
            
            // Get device info
            const deviceInfo = await this.api.getDeviceInfo();
            console.log('Device Info:', deviceInfo);
            
            this.updateStatus('API connected successfully');
            return true;
        } catch (error) {
            console.error('Failed to initialize:', error);
            this.updateStatus('Failed to connect to API: ' + error.message);
            return false;
        }
    }

    /**
     * Capture fingerprint and display result
     */
    async captureFingerprint() {
        try {
            this.updateStatus('Capturing fingerprint...');
            
            const response = await this.api.capture({
                timeout: 10000,
                quality: 50
            });

            console.log('Capture response received:', response);

            if (response.success || response.Success) {
                this.updateStatus(`Fingerprint captured successfully. Quality: ${response.quality || response.Quality}, Time: ${response.captureTime || response.CaptureTime}ms`);
                
                // Display image
                this.api.displayImage(response.imageBase64 || response.ImageBase64, 'capturedImage');
                
                // Store template
                this.templates.template1 = response.template || response.Template;
                
                return response;
            } else {
                this.updateStatus('Capture failed: ' + (response.message || response.Message || 'Unknown error'));
                return null;
            }
        } catch (error) {
            this.updateStatus('Capture error: ' + error.message);
            return null;
        }
    }

    /**
     * Capture second fingerprint for comparison
     */
    async captureSecondFingerprint() {
        try {
            this.updateStatus('Capturing second fingerprint...');
            
            const response = await this.api.capture({
                timeout: 10000,
                quality: 50
            });

            console.log('Second capture response received:', response);

            if (response.success || response.Success) {
                this.updateStatus(`Second fingerprint captured successfully. Quality: ${response.quality || response.Quality}`);
                
                // Display image
                this.api.displayImage(response.imageBase64 || response.ImageBase64, 'capturedImage2');
                
                // Store template
                this.templates.template2 = response.template || response.Template;
                
                return response;
            } else {
                this.updateStatus('Second capture failed: ' + (response.message || response.Message || 'Unknown error'));
                return null;
            }
        } catch (error) {
            this.updateStatus('Second capture error: ' + error.message);
            return null;
        }
    }

    /**
     * Compare two captured fingerprints
     */
    async compareFingerprints() {
        if (!this.templates.template1 || !this.templates.template2) {
            this.updateStatus('Please capture both fingerprints first');
            return;
        }

        try {
            this.updateStatus('Comparing fingerprints...');
            
            const response = await this.api.compare(
                this.templates.template1,
                this.templates.template2,
                'NORMAL'
            );

            console.log('Compare response received:', response);

            if (response.success || response.Success) {
                const matched = response.matched || response.Matched;
                const matchScore = response.matchScore || response.MatchScore;
                const matchText = matched ? 'MATCH' : 'NO MATCH';
                this.updateStatus(`Comparison result: ${matchText} (Score: ${matchScore})`);
                
                // Update UI based on result
                const resultElement = document.getElementById('comparisonResult');
                if (resultElement) {
                    resultElement.textContent = matchText;
                    resultElement.className = matched ? 'match' : 'no-match';
                }
            } else {
                this.updateStatus('Comparison failed: ' + (response.message || response.Message || 'Unknown error'));
            }
        } catch (error) {
            this.updateStatus('Comparison error: ' + error.message);
        }
    }

    /**
     * Register fingerprints (enrollment)
     */
    async registerFingerprints() {
        if (!this.templates.template1 || !this.templates.template2) {
            this.updateStatus('Please capture both fingerprints first');
            return;
        }

        try {
            this.updateStatus('Registering fingerprints...');
            
            const response = await this.api.register(
                this.templates.template1,
                this.templates.template2,
                'NORMAL'
            );

            console.log('Register response received:', response);

            if (response.success || response.Success) {
                const registered = response.registered || response.Registered;
                const matchScore = response.matchScore || response.MatchScore;
                const registeredTemplate = response.registeredTemplate || response.RegisteredTemplate;
                
                if (registered) {
                    this.templates.registered = registeredTemplate;
                    this.updateStatus(`Registration successful! (Score: ${matchScore})`);
                } else {
                    this.updateStatus('Registration failed: ' + (response.message || response.Message || 'Unknown error'));
                }
            } else {
                this.updateStatus('Registration failed: ' + (response.message || response.Message || 'Unknown error'));
            }
        } catch (error) {
            this.updateStatus('Registration error: ' + error.message);
        }
    }

    /**
     * Verify against registered fingerprint
     */
    async verifyFingerprint() {
        if (!this.templates.registered) {
            this.updateStatus('Please register fingerprints first');
            return;
        }

        try {
            this.updateStatus('Capturing fingerprint for verification...');
            
            // Capture new fingerprint for verification
            const captureResponse = await this.api.capture({
                timeout: 10000,
                quality: 50
            });

            if (captureResponse.success || captureResponse.Success) {
                this.updateStatus('Verifying fingerprint...');
                
                const response = await this.api.verify(
                    this.templates.registered,
                    captureResponse.template || captureResponse.Template,
                    'NORMAL'
                );

                console.log('Verify response received:', response);

                if (response.success || response.Success) {
                    const verified = response.verified || response.Verified;
                    const matchScore = response.matchScore || response.MatchScore;
                    const verifyText = verified ? 'VERIFIED' : 'NOT VERIFIED';
                    this.updateStatus(`Verification result: ${verifyText} (Score: ${matchScore})`);
                    
                    // Update UI based on result
                    const resultElement = document.getElementById('verificationResult');
                    if (resultElement) {
                        resultElement.textContent = verifyText;
                        resultElement.className = verified ? 'verified' : 'not-verified';
                    }
                } else {
                    this.updateStatus('Verification failed: ' + (response.message || response.Message || 'Unknown error'));
                }
            } else {
                this.updateStatus('Capture for verification failed: ' + (captureResponse.message || captureResponse.Message || 'Unknown error'));
            }
        } catch (error) {
            this.updateStatus('Verification error: ' + error.message);
        }
    }

    /**
     * Update status message
     * @param {string} message - Status message
     */
    updateStatus(message) {
        const statusElement = document.getElementById('status');
        if (statusElement) {
            statusElement.textContent = message;
        }
        console.log('Status:', message);
    }

    /**
     * Clear all stored templates
     */
    clearTemplates() {
        this.templates = {
            template1: null,
            template2: null,
            registered: null
        };
        this.updateStatus('Templates cleared');
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { FingerprintAPI, FingerprintAPIDemo };
}
