using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TripPlanner.E2E;

public class AppFactory : WebApplicationFactory<Program>, IDisposable
{
    private Process? _appProcess;
    private string? _rootUri;
    private string? _dbPath;

    public string RootUri
    {
        get
        {
            if (_rootUri is null)
            {
                StartAppProcess();
                WaitUntilAvailable(_rootUri!, TimeSpan.FromSeconds(30));
            }
            return _rootUri!;
        }
    }

    private void StartAppProcess()
    {
        // Allocate an HTTP port
        var port = GetFreeTcpPort();
        _rootUri = $"http://127.0.0.1:{port}";

        // Find TripPlanner.dll built output
        var appDll = FindTripPlannerDll()
            ?? throw new InvalidOperationException("Could not locate TripPlanner.dll. Build the solution before running E2E tests.");

        // Use a per-run file-based SQLite DB
        _dbPath = Path.Combine(Path.GetTempPath(), $"TripPlannerE2E-{Guid.NewGuid():N}.db");
        var e2eConn = $"Data Source={_dbPath};Cache=Shared";

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{appDll}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(appDll)!,
        };

        // Environment for Testing
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Testing";
        psi.Environment["ASPNETCORE_URLS"] = _rootUri!;
        // Inject E2E connection string read by Program.cs
        psi.Environment["ConnectionStrings__E2E"] = e2eConn;

        _appProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
        _appProcess.OutputDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.WriteLine("[APP] " + e.Data); };
        _appProcess.ErrorDataReceived += (_, e) => { if (!string.IsNullOrEmpty(e.Data)) Debug.WriteLine("[APP ERR] " + e.Data); };

        if (!_appProcess.Start())
        {
            throw new InvalidOperationException("Failed to start TripPlanner app process.");
        }
        _appProcess.BeginOutputReadLine();
        _appProcess.BeginErrorReadLine();
    }

    private static string? FindTripPlannerDll()
    {
        // Search upwards from the test bin folder for TripPlanner/bin/*/net9.0/TripPlanner.dll
        var baseDir = AppContext.BaseDirectory;
        var repoRoot = Directory.GetParent(baseDir);
        for (int i = 0; i < 6 && repoRoot is not null; i++) repoRoot = repoRoot.Parent;

        var root = repoRoot?.FullName ?? Directory.GetCurrentDirectory();
        var candidates = Directory.EnumerateFiles(root, "TripPlanner.dll", SearchOption.AllDirectories)
            .Where(p => p.EndsWith(Path.Combine("TripPlanner", "bin", "Debug", "net9.0", "TripPlanner.dll"), StringComparison.OrdinalIgnoreCase)
                     || p.EndsWith(Path.Combine("TripPlanner", "bin", "Release", "net9.0", "TripPlanner.dll"), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return candidates.OrderByDescending(File.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static void WaitUntilAvailable(string baseUrl, TimeSpan timeout)
    {
        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };

        var start = DateTime.UtcNow;
        Exception? last = null;
        while (DateTime.UtcNow - start < timeout)
        {
            try
            {
                var resp = client.GetAsync(baseUrl + "/").GetAwaiter().GetResult();
                if ((int)resp.StatusCode is >= 200 and < 400)
                    return;
            }
            catch (Exception ex)
            {
                last = ex;
            }
            System.Threading.Thread.Sleep(250);
        }
        throw new TimeoutException($"App did not become available at {baseUrl} within {timeout}. Last error: {last?.Message}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            try
            {
                if (_appProcess is { HasExited: false })
                {
                    _appProcess.Kill(entireProcessTree: true);
                    _appProcess.WaitForExit(5000);
                }
            }
            catch { /* ignore */ }
            finally
            {
                _appProcess?.Dispose();
                _appProcess = null;
            }

            if (!string.IsNullOrEmpty(_dbPath) && File.Exists(_dbPath))
            {
                try { File.Delete(_dbPath); } catch { /* ignore */ }
            }
        }
    }
}