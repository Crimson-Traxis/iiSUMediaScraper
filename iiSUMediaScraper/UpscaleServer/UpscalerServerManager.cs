using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace iiSUMediaScraper.UpscaleServer;

/// <summary>
/// Manages the Python SeedVR2 server process lifecycle.
/// Handles package installation and launches the server in a visible console window.
/// </summary>
public class UpscalerServerManager : IDisposable
{
    private Process? _serverProcess;
    private readonly string _pythonPath;
    private readonly string _serverScriptPath;
    private readonly string _requirementsPath;
    private readonly string _serverDirectory;
    private readonly string _host;
    private readonly int _port;
    private bool _disposed;

    /// <summary>
    /// Creates a new UpscalerServerManager
    /// </summary>
    /// <param name="serverDirectory">Directory containing seedvr2_server.py and requirements.txt</param>
    /// <param name="logger">Logger instance for diagnostic output.</param>
    /// <param name="pythonPath">Path to python executable (default: "python")</param>
    /// <param name="host">Host to bind server to (default: "127.0.0.1")</param>
    /// <param name="port">Port to run server on (default: 8000)</param>
    public UpscalerServerManager(
        string serverDirectory,
        ILogger<UpscalerServerManager> logger,
        string pythonPath = "python",
        string host = "127.0.0.1",
        int port = 8000)
    {
        Logger = logger;
        _pythonPath = pythonPath;
        _serverDirectory = serverDirectory;
        _serverScriptPath = Path.Combine(serverDirectory, "seedvr2_server.py");
        _requirementsPath = Path.Combine(serverDirectory, "requirements.txt");
        _host = host;
        _port = port;

        if (!File.Exists(_serverScriptPath))
            throw new FileNotFoundException("Server script not found", _serverScriptPath);

        if (!File.Exists(_requirementsPath))
            throw new FileNotFoundException("Requirements file not found", _requirementsPath);
    }

    /// <summary>
    /// The base URL of the running server
    /// </summary>
    public string ServerUrl => $"http://{_host}:{_port}";

    /// <summary>
    /// Whether the server process is currently running
    /// </summary>
    public bool IsProcessRunning => _serverProcess != null && !_serverProcess.HasExited;

    /// <summary>
    /// Installs required Python packages in a visible console window.
    /// This includes cloning/updating the SeedVR2 repository and installing dependencies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if installation succeeded</returns>
    public async Task<bool> InstallPackagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"{_pythonPath} -m pip install -r \"{_requirementsPath}\" && echo. && echo Installation complete! Press any key to continue... && pause\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            process.Exited += (_, _) =>
            {
                taskCompletionSource.TrySetResult(process.ExitCode == 0);
                process.Dispose();
            };

            cancellationToken.Register(() =>
            {
                taskCompletionSource.TrySetCanceled();
                try { process.Kill(); } catch { }
            });

            process.Start();

            return await taskCompletionSource.Task;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Package installation was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install packages asynchronously");
            return false;
        }
    }

    /// <summary>
    /// Installs packages and waits for completion (blocking call with visible window).
    /// Use this if you want to ensure packages are installed before proceeding.
    /// </summary>
    /// <returns>Exit code from pip install, or -1 if an error occurred</returns>
    public int InstallPackagesAndWait()
    {
        try
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{_pythonPath} -m pip install -r \"{_requirementsPath}\"\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to install packages");
            return -1;
        }
    }

    /// <summary>
    /// Starts the Python server in a visible console window.
    /// </summary>
    /// <returns>True if the server started successfully, false otherwise.</returns>
    public bool StartServer()
    {
        try
        {
            if (IsProcessRunning)
                return true;

            // Set SEEDVR2_DIR environment variable to point to the SeedVR2 installation
            string seedVr2Directory = Environment.GetEnvironmentVariable("SEEDVR2_DIR") ?? _serverDirectory;

            _serverProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k \"set SEEDVR2_DIR={seedVr2Directory} && {_pythonPath} -m uvicorn seedvr2_server:app --host {_host} --port {_port}\"",
                    WorkingDirectory = Path.GetDirectoryName(_serverScriptPath),
                    UseShellExecute = true,
                    CreateNoWindow = false
                },
                EnableRaisingEvents = true
            };

            _serverProcess.Start();
            Logger.LogDebug("Started upscaler server process on {Host}:{Port}", _host, _port);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start upscaler server on {Host}:{Port}", _host, _port);
            return false;
        }
    }

    /// <summary>
    /// Starts the server and waits until it's ready to accept requests.
    /// </summary>
    /// <param name="timeoutSeconds">Maximum time to wait for server to be ready (default: 300 for model loading)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if server started and is responding</returns>
    public async Task<bool> StartServerAndWaitAsync(
        int timeoutSeconds = 300,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!StartServer())
                return false;

            using HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            DateTime deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

            while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
            {
                if (!IsProcessRunning)
                    return false;

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{ServerUrl}/health", cancellationToken);
                    if (response.IsSuccessStatusCode)
                        return true;
                }
                catch
                {
                    // Server not ready yet
                }

                await Task.Delay(2000, cancellationToken);
            }

            return false;
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("Server startup wait was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start server and wait for ready state");
            return false;
        }
    }

    /// <summary>
    /// Stops the server process.
    /// </summary>
    public void StopServer()
    {
        if (_serverProcess == null || _serverProcess.HasExited)
            return;

        try
        {
            // Try graceful shutdown first
            _serverProcess.CloseMainWindow();

            if (!_serverProcess.WaitForExit(5000))
            {
                _serverProcess.Kill(entireProcessTree: true);
            }

            Logger.LogDebug("Stopped upscaler server process");
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error while stopping server process (may have already exited)");
        }
        finally
        {
            _serverProcess.Dispose();
            _serverProcess = null;
        }
    }

    /// <summary>
    /// Convenience method: installs packages, starts server, and waits until ready.
    /// </summary>
    /// <param name="skipInstallIfHealthy">Skip package installation if server is already running</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if server is ready</returns>
    public async Task<bool> EnsureServerRunningAsync(
        bool skipInstallIfHealthy = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if already running
            if (skipInstallIfHealthy)
            {
                using HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync($"{ServerUrl}/health", cancellationToken);
                    if (response.IsSuccessStatusCode)
                        return true;
                }
                catch
                {
                    // Not running, continue with setup
                }
            }

            // Install packages (visible window)
            int installationExitCode = InstallPackagesAndWait();
            if (installationExitCode != 0)
                return false;

            // Start server and wait (longer timeout for model loading)
            return await StartServerAndWaitAsync(timeoutSeconds: 300, cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogDebug("EnsureServerRunning was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to ensure server is running");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        StopServer();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    protected ILogger Logger { get; private set; }
}