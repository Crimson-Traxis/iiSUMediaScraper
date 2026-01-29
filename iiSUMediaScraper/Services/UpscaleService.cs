using iiSUMediaScraper.Contracts.Services;
using iiSUMediaScraper.Models.Configurations;
using iiSUMediaScraper.Models.Upscale;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Settings for UpscalerService, used for dependency injection
/// </summary>
public class UpscalerServiceSettings
{
    /// <summary>
    /// Directory where SeedVR2 code and models will be stored.
    /// The server will automatically download the SeedVR2 repository and models on first run.
    /// </summary>
    public string ServerDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Path to python executable. Leave empty or null for auto-detection.
    /// </summary>
    public string? PythonPath { get; set; }

    /// <summary>
    /// Host to bind server to
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Port to run server on
    /// </summary>
    public int Port { get; set; } = 8000;

    /// <summary>
    /// Request timeout in minutes. Set high enough for slow hardware.
    /// Default: 120 minutes (2 hours)
    /// </summary>
    public int TimeoutMinutes { get; set; } = 120;

    /// <summary>
    /// Maximum time to wait for server startup in seconds (includes model download time).
    /// Default: 1800 seconds (30 minutes) to allow for large model downloads.
    /// </summary>
    public int ServerStartupTimeoutSeconds { get; set; } = 1800;
}

/// <summary>
/// Request model sent to the Python SeedVR2 server
/// </summary>
internal class UpscaleRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("resolution")]
    public int Resolution { get; set; } = 1080;

    [JsonPropertyName("max_resolution")]
    public int MaxResolution { get; set; } = 0;

    [JsonPropertyName("seed")]
    public int Seed { get; set; } = 42;

    [JsonPropertyName("color_correction")]
    public string ColorCorrection { get; set; } = "lab";

    [JsonPropertyName("input_noise_scale")]
    public double InputNoiseScale { get; set; } = 0.0;

    [JsonPropertyName("latent_noise_scale")]
    public double LatentNoiseScale { get; set; } = 0.0;

    [JsonPropertyName("image_base64")]
    public string ImageBase64 { get; set; } = string.Empty;
}

/// <summary>
/// Response model from the Python SeedVR2 server
/// </summary>
internal class UpscaleResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("image_base64")]
    public string? ImageBase64 { get; set; }

    [JsonPropertyName("width")]
    public int? Width { get; set; }

    [JsonPropertyName("height")]
    public int? Height { get; set; }
}

/// <summary>
/// Client service for the SeedVR2 Upscaler API.
/// Automatically manages the Python server lifecycle - starting it on first request
/// and restarting if it closes.
/// Implements a lock to ensure only one HTTP request is sent at a time.
/// </summary>
public class UpscalerService : IUpscalerService
{
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _requestLock = new(1, 1);
    private readonly string _serverDirectory;
    private readonly string _pythonPath;
    private readonly string _host;
    private readonly int _port;
    private readonly int _serverStartupTimeoutSeconds;

    private Process? _serverProcess;
    private bool _disposed;

    /// <summary>
    /// Creates a new UpscalerService instance using IOptions for dependency injection
    /// </summary>
    /// <param name="options">Configuration options</param>
    /// <param name="logger">Logger instance for diagnostic output</param>
    public UpscalerService(IOptions<UpscalerServiceSettings> options, ILogger<UpscalerService> logger)
        : this(
            options.Value.ServerDirectory,
            options.Value.PythonPath,
            options.Value.Host,
            options.Value.Port,
            options.Value.TimeoutMinutes,
            options.Value.ServerStartupTimeoutSeconds,
            logger)
    {
    }

    /// <summary>
    /// Creates a new UpscalerService instance
    /// </summary>
    /// <param name="serverDirectory">Directory where SeedVr2Server.py is located and where SeedVR2 will be downloaded</param>
    /// <param name="pythonPath">Path to python executable (null or empty for auto-detection)</param>
    /// <param name="host">Host to bind server to (default: "127.0.0.1")</param>
    /// <param name="port">Port to run server on (default: 8000)</param>
    /// <param name="timeoutMinutes">Request timeout in minutes (default: 10)</param>
    /// <param name="serverStartupTimeoutSeconds">Maximum time to wait for server startup including model download (default: 600)</param>
    /// <param name="logger">Logger instance for diagnostic output</param>
    public UpscalerService(
        string serverDirectory,
        string? pythonPath = null,
        string host = "127.0.0.1",
        int port = 8000,
        int timeoutMinutes = 10,
        int serverStartupTimeoutSeconds = 600,
        ILogger<UpscalerService>? logger = null)
    {
        Logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<UpscalerService>.Instance;
        _serverDirectory = serverDirectory;
        _pythonPath = string.IsNullOrWhiteSpace(pythonPath) ? DetectPythonPath() : pythonPath;
        _host = host;
        _port = port;
        _serverStartupTimeoutSeconds = serverStartupTimeoutSeconds;

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(timeoutMinutes)
        };

        // Ensure server directory exists
        Directory.CreateDirectory(serverDirectory);

        string serverScriptPath = Path.Combine(serverDirectory, "SeedVr2Server.py");

        if (!File.Exists(serverScriptPath))
            throw new FileNotFoundException("Server script not found. Please copy SeedVr2Server.py to the server directory.", serverScriptPath);
    }

    /// <summary>
    /// Detects the Python executable path by checking common locations and PATH
    /// </summary>
    private static string DetectPythonPath()
    {
        // Try common executable names via PATH first
        string[] pythonCommands = { "python", "python3", "py" };

        foreach (string command in pythonCommands)
        {
            string? path = FindExecutableInPath(command);
            if (path != null && IsPythonValid(path))
            {
                return path;
            }
        }

        // Check common Windows installation locations
        List<string> commonPaths = [];

        // Python Launcher for Windows
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // Check Python versions 3.13 down to 3.8
        for (int version = 13; version >= 8; version--)
        {
            commonPaths.Add(Path.Combine(localAppData, "Programs", "Python", $"Python3{version}", "python.exe"));
            commonPaths.Add(Path.Combine(programFiles, $"Python3{version}", "python.exe"));
            commonPaths.Add(Path.Combine(programFilesX86, $"Python3{version}", "python.exe"));
            commonPaths.Add(Path.Combine(userProfile, "AppData", "Local", "Programs", "Python", $"Python3{version}", "python.exe"));
        }

        // Anaconda/Miniconda paths
        commonPaths.Add(Path.Combine(userProfile, "anaconda3", "python.exe"));
        commonPaths.Add(Path.Combine(userProfile, "miniconda3", "python.exe"));
        commonPaths.Add(Path.Combine(programFiles, "Anaconda3", "python.exe"));
        commonPaths.Add(Path.Combine(programFiles, "Miniconda3", "python.exe"));

        foreach (string path in commonPaths)
        {
            if (File.Exists(path) && IsPythonValid(path))
            {
                return path;
            }
        }

        // Fallback to "python" and hope it's in PATH
        return "python";
    }

    /// <summary>
    /// Finds an executable in the system PATH
    /// </summary>
    private static string? FindExecutableInPath(string executableName)
    {
        string? pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
            return null;

        string[] extensions = new[] { "", ".exe", ".cmd", ".bat" };
        string[] paths = pathVariable.Split(Path.PathSeparator);

        foreach (string path in paths)
        {
            foreach (string? extension in extensions)
            {
                string fullPath = Path.Combine(path, executableName + extension);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Validates that the Python executable works and is version 3.8+
    /// </summary>
    private static bool IsPythonValid(string pythonPath)
    {
        try
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit(5000);

            // Check for Python 3.8+
            if (output.Contains("Python 3."))
            {
                Match versionMatch = System.Text.RegularExpressions.Regex.Match(output, @"Python 3\.(\d+)");
                if (versionMatch.Success && int.TryParse(versionMatch.Groups[1].Value, out int minorVersion))
                {
                    return minorVersion >= 8;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private void StartServer()
    {
        try
        {
            string seedVr2Directory = Environment.GetEnvironmentVariable("SEEDVR2_DIR") ?? _serverDirectory;

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"set SEEDVR2_DIR={seedVr2Directory} && {_pythonPath} -m uvicorn SeedVr2Server:app --host {_host} --port {_port}\"",
                    WorkingDirectory = _serverDirectory,
                    UseShellExecute = true,
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            _serverProcess.Start();
            Logger.LogDebug("Started upscaler server process on {Host}:{Port}", _host, _port);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start upscaler server on {Host}:{Port}", _host, _port);
            throw;
        }
    }

    private async Task<bool> WaitForServerReadyAsync(CancellationToken cancellationToken)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(_serverStartupTimeoutSeconds);

        while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            if (!IsServerProcessRunning)
            {
                return false;
            }

            if (await IsHealthyAsync(cancellationToken))
            {
                return true;
            }

            await Task.Delay(2000, cancellationToken);
        }

        return false;
    }

    private async Task<bool> EnsureServerRunningAsync(CancellationToken cancellationToken)
    {
        // Check if server is already responding
        if (await IsHealthyAsync(cancellationToken))
        {
            return true;
        }

        // Start server if process is not running
        if (!IsServerProcessRunning)
        {
            StartServer();
        }

        // Wait for server to be ready (includes time for downloading repo and models)
        return await WaitForServerReadyAsync(cancellationToken);
    }

    private void StopServer()
    {
        if (_serverProcess == null || _serverProcess.HasExited)
        {
            return;
        }

        try
        {
            _serverProcess.CloseMainWindow();

            if (!_serverProcess.WaitForExit(5000))
            {
                _serverProcess.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Process may have already exited
        }
        finally
        {
            _serverProcess.Dispose();
            _serverProcess = null;
        }
    }

    /// <summary>
    /// Checks if the server is healthy and the model is loaded
    /// </summary>
    public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{ServerUrl}/health", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Upscales an image using the provided configuration.
    /// This method is thread-safe and ensures only one request is processed at a time.
    /// Automatically starts the server if it's not running.
    /// </summary>
    /// <param name="configuration">Upscaler configuration (quality settings)</param>
    /// <param name="imageData">Raw image bytes to upscale</param>
    /// <param name="targetWidth">Target output width in pixels</param>
    /// <param name="targetHeight">Target output height in pixels</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the upscaled image data</returns>
    public async Task<UpscaleResult> UpscaleAsync(
        UpscalerConfiguration configuration,
        byte[] imageData,
        int targetWidth,
        int targetHeight,
        CancellationToken cancellationToken = default)
    {
        await _requestLock.WaitAsync(cancellationToken);
        try
        {
            // Ensure server is running before making request
            bool serverReady = await EnsureServerRunningAsync(cancellationToken);
            if (!serverReady)
            {
                return new UpscaleResult
                {
                    Success = false,
                    Message = "Failed to start upscaler server"
                };
            }

            // SeedVR2 uses the shortest edge as resolution, so pick the smaller of target dimensions
            int resolution = Math.Min(targetWidth, targetHeight);

            UpscaleRequest request = new UpscaleRequest
            {
                Name = configuration.Name,
                Resolution = resolution,
                MaxResolution = configuration.MaxResolution,
                Seed = configuration.Seed,
                ColorCorrection = configuration.ColorCorrection,
                InputNoiseScale = configuration.InputNoiseScale,
                LatentNoiseScale = configuration.LatentNoiseScale,
                ImageBase64 = Convert.ToBase64String(imageData)
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"{ServerUrl}/upscale",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new UpscaleResult
                {
                    Success = false,
                    Message = $"Server error ({response.StatusCode}): {errorContent}"
                };
            }

            UpscaleResponse? upscaleResponse = await response.Content.ReadFromJsonAsync<UpscaleResponse>(
                cancellationToken: cancellationToken);

            if (upscaleResponse == null)
            {
                return new UpscaleResult
                {
                    Success = false,
                    Message = "Failed to deserialize server response"
                };
            }

            return new UpscaleResult
            {
                Success = upscaleResponse.Success,
                Message = upscaleResponse.Message,
                ImageData = upscaleResponse.ImageBase64 != null
                    ? Convert.FromBase64String(upscaleResponse.ImageBase64)
                    : null,
                Width = upscaleResponse.Width,
                Height = upscaleResponse.Height
            };
        }
        catch (TaskCanceledException)
        {
            return new UpscaleResult
            {
                Success = false,
                Message = "Request was cancelled or timed out"
            };
        }
        catch (HttpRequestException exception)
        {
            return new UpscaleResult
            {
                Success = false,
                Message = $"HTTP request failed: {exception.Message}"
            };
        }
        catch (Exception exception)
        {
            return new UpscaleResult
            {
                Success = false,
                Message = $"Unexpected error: {exception.Message}"
            };
        }
        finally
        {
            _requestLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        StopServer();
        _httpClient.Dispose();
        _requestLock.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    protected ILogger Logger { get; private set; }

    private string ServerUrl => $"http://{_host}:{_port}";

    private bool IsServerProcessRunning => _serverProcess != null && !_serverProcess.HasExited;
}
