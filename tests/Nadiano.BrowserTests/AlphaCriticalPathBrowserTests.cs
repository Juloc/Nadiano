using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

using Microsoft.Playwright;

namespace Nadiano.BrowserTests;

public sealed class AlphaCriticalPathBrowserTests
{
    private static readonly string[] PrerequisiteLessons =
    [
        "foundation-keyboard-map-01",
        "foundation-sitting-01",
        "foundation-tension-release-01",
        "foundation-finger-numbers-01",
        "foundation-hand-shape-01",
        "foundation-arm-weight-01",
        "foundation-controlled-tone-01",
        "pulse-steady-beat-01",
        "technique-finger-transfer-01",
    ];

    [Fact]
    public async Task LearnerCanReachAndCompleteFirstMidiLessonWithFakeWebMidi()
    {
        if (Environment.GetEnvironmentVariable("NADIANO_RUN_BROWSER_TESTS") != "1")
        {
            return;
        }

        var repositoryRoot = FindRepositoryRoot();
        var dataPath = Path.Combine(Path.GetTempPath(), $"nadiano-browser-{Guid.NewGuid():N}");
        var port = ReservePort();
        var baseUrl = $"http://127.0.0.1:{port}";
        Directory.CreateDirectory(dataPath);

        using var app = StartApplication(repositoryRoot, dataPath, port);
        try
        {
            await WaitUntilReadyAsync(baseUrl, app);

            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                Locale = "de-DE",
            });
            var page = await context.NewPageAsync();
            await InstallFakeMidiAndAudioAsync(page);

            foreach (var lessonId in PrerequisiteLessons)
            {
                var response = await page.GotoAsync($"{baseUrl}/Learn/Lesson/{lessonId}");
                Assert.NotNull(response);
                Assert.Equal((int)HttpStatusCode.OK, response.Status);
                await page.Locator("#lesson-dry-task-button").ClickAsync();
                await page.Locator("#lesson-dry-task-status").WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                });
            }

            await page.GotoAsync($"{baseUrl}/Learn/Lesson/exercise-single-tone-01");
            await page.Locator("a[href*='/Practice']").ClickAsync();
            await page.Locator("#practice-workspace").WaitForAsync();

            Assert.Equal("exercise-single-tone-01", await page.Locator("#practice-workspace").GetAttributeAsync("data-lesson-id"));
            Assert.Equal("66", await page.Locator("#practice-workspace").GetAttributeAsync("data-target-tempo"));

            await page.Locator("#workspace-connect-button").ClickAsync();
            await ExpectTextAsync(page.Locator("#workspace-device-status"), "MIDI-Gerät verbunden.");

            await page.Locator("#workspace-start-button").ClickAsync();
            await WaitUntilEnabledAsync(page.Locator("#workspace-stop-button"));

            for (var index = 0; index < 8; index++)
            {
                await page.EvaluateAsync("window.__nadianoEmitMidi(60)");
                await page.WaitForTimeoutAsync(40);
            }

            await page.Locator("#workspace-result-section").WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
            });
            await page.WaitForTimeoutAsync(500);

            await page.GotoAsync($"{baseUrl}/Progress");
            await ExpectTextAsync(page.Locator("body"), "Ein gehaltener Ton");
        }
        finally
        {
            StopApplication(app);
            TryDeleteDirectory(dataPath);
        }
    }

    private static async Task InstallFakeMidiAndAudioAsync(IPage page)
    {
        await page.AddInitScriptAsync(
            """
            (() => {
              const input = {
                id: "nadiano-fake-midi",
                name: "Nadiano Fake MIDI",
                manufacturer: "Nadiano",
                state: "connected",
                connection: "open",
                onmidimessage: null
              };

              Object.defineProperty(navigator, "requestMIDIAccess", {
                configurable: true,
                value: async () => ({
                  inputs: new Map([[input.id, input]]),
                  onstatechange: null
                })
              });

              window.__nadianoEmitMidi = (note) => {
                if (typeof input.onmidimessage === "function") {
                  input.onmidimessage({
                    data: new Uint8Array([0x90, note, 100]),
                    timeStamp: performance.now()
                  });
                }
              };

              class FakeAudioContext {
                constructor() {
                  this.state = "running";
                  this.destination = {};
                  this.currentTime = 0;
                }
                async resume() { this.state = "running"; }
                createOscillator() {
                  return {
                    frequency: { value: 0 },
                    connect() {},
                    start() {},
                    stop() {}
                  };
                }
                createGain() {
                  return {
                    gain: {
                      setValueAtTime() {},
                      exponentialRampToValueAtTime() {}
                    },
                    connect() {}
                  };
                }
              }

              window.AudioContext = FakeAudioContext;
              window.webkitAudioContext = FakeAudioContext;
            })();
            """);
    }

    private static Process StartApplication(string repositoryRoot, string dataPath, int port)
    {
        var projectPath = Path.Combine(repositoryRoot, "src", "Nadiano.Web", "Nadiano.Web.csproj");
        var startInfo = new ProcessStartInfo("dotnet")
        {
            Arguments = $"run --project \"{projectPath}\" -c Release --no-build --no-launch-profile",
            WorkingDirectory = repositoryRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        startInfo.Environment["Nadiano__DataPath"] = dataPath;
        startInfo.Environment["Nadiano__ContentPath"] = Path.Combine(repositoryRoot, "content");
        startInfo.Environment["Nadiano__ApplyMigrations"] = "true";

        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Nadiano process could not be started.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static async Task WaitUntilReadyAsync(string baseUrl, Process process)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (process.HasExited)
            {
                throw new InvalidOperationException($"Nadiano exited before startup with code {process.ExitCode}.");
            }

            try
            {
                var response = await client.GetAsync($"{baseUrl}/health/ready");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Startup is still in progress.
            }
            catch (TaskCanceledException)
            {
                // Startup is still in progress.
            }

            await Task.Delay(250);
        }

        throw new TimeoutException("Nadiano did not become ready within 30 seconds.");
    }

    private static async Task WaitUntilEnabledAsync(ILocator locator)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (!await locator.IsDisabledAsync())
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail("The practice session did not start within 10 seconds.");
    }

    private static async Task ExpectTextAsync(ILocator locator, string expected)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if ((await locator.InnerTextAsync()).Contains(expected, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Fail($"Expected text was not found: {expected}");
    }

    private static int ReservePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Nadiano.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Nadiano repository root was not found.");
    }

    private static void StopApplication(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        process.Kill(entireProcessTree: true);
        process.WaitForExit(5_000);
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            Directory.Delete(path, recursive: true);
        }
        catch (IOException)
        {
            // Best-effort cleanup.
        }
        catch (UnauthorizedAccessException)
        {
            // Best-effort cleanup.
        }
    }
}