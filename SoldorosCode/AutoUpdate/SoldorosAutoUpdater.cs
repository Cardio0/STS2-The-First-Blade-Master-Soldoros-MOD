using System;
using System.Diagnostics;
using System.IO;
using HttpClient = System.Net.Http.HttpClient;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SystemVersion = System.Version;

namespace Soldoros.SoldorosCode.AutoUpdate;

public static class SoldorosAutoUpdater
{
    // TODO: Update this to your actual GitHub repo path before publishing
    private const string RepoLatestReleaseUrl = "https://api.github.com/repos/Cardio0/STS2-The-First-Blade-Master-Soldoros-MOD/releases/latest";
    private const string HttpUserAgent = "SoldorosAutoUpdater/1.0";
    private const string PendingUpdateDirName = "_pending_update";
    private const string PendingVersionFileName = "prepared_version.txt";
    private const string UpdateToastText = "솔도로스 모드가 업데이트 되었습니다. 재실행시 적용됩니다.";
    private const float ToastWidth = 900f;
    private const float ToastHeight = 54f;
    private const float ToastBottom = 26f;
    private const float ToastRight = 34f;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private static readonly string RawVersionText = Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";
    private static readonly SystemVersion CurrentVersion = ParseVersion(RawVersionText);

    private static readonly string? ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    private static Task? _updateTask;
    private static string? _preparedVersion;
    private static Node? _menuContext;
    private static bool _pendingToast;
    private static CanvasLayer? _toastLayer;
    private static Control? _toastRoot;
    private static Label? _toastLabel;
    private static Tween? _toastTween;

    internal static void OnMainMenuReady(NMainMenu mainMenu)
    {
        _menuContext = mainMenu;
        if (_pendingToast)
        {
            ShowToast(mainMenu);
            _pendingToast = false;
        }
        _updateTask ??= CheckForUpdatesAsync();
    }

    private static async Task CheckForUpdatesAsync()
    {
        try
        {
            CleanupIfApplied();
            GitHubRelease? release = await FetchLatestReleaseAsync();
            if (release == null) return;

            SystemVersion latestVersion = ParseVersion(release.TagName);
            if (latestVersion <= CurrentVersion) return;

            GitHubAsset? zipAsset = FindZipAsset(release);
            if (zipAsset?.BrowserDownloadUrl == null)
            {
                GD.Print("[Soldoros] Latest release has no downloadable zip asset.");
                return;
            }

            string latestVersionStr = latestVersion.ToString();
            if (IsPreparedAlreadyQueued(latestVersionStr))
            {
                _preparedVersion = latestVersionStr;
                return;
            }

            if (_preparedVersion == latestVersionStr)
            {
                QueueToast();
                return;
            }

            await DownloadAndPrepareAsync(latestVersionStr, zipAsset.BrowserDownloadUrl);
            _preparedVersion = latestVersionStr;
            QueueToast();
            GD.Print($"[Soldoros] Auto-update prepared: {CurrentVersion} → {latestVersionStr}");
        }
        catch (Exception ex)
        {
            GD.Print($"[Soldoros] Auto-update check failed: {ex.Message}");
        }
    }

    private static async Task<GitHubRelease?> FetchLatestReleaseAsync()
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpUserAgent);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        string json = await client.GetStringAsync(RepoLatestReleaseUrl);
        return JsonSerializer.Deserialize<GitHubRelease>(json, JsonOptions);
    }

    private static GitHubAsset? FindZipAsset(GitHubRelease release)
    {
        if (release.Assets == null) return null;
        foreach (GitHubAsset asset in release.Assets)
        {
            if (!string.IsNullOrWhiteSpace(asset.Name) &&
                asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                return asset;
        }
        return null;
    }

    private static async Task DownloadAndPrepareAsync(string version, string downloadUrl)
    {
        if (string.IsNullOrWhiteSpace(ModDirectory)) return;

        string updateDir = GetUpdateDir();
        Directory.CreateDirectory(updateDir);

        // 이전 버전 zip/스크립트 제거 → 게임 종료 시 구버전 스크립트가 새 파일을 덮어쓰는 Race Condition 방지
        PurgeOldUpdateFiles(updateDir, keepVersion: version);

        string zipPath = Path.Combine(updateDir, $"soldoros-v{version}.zip");

        if (!File.Exists(zipPath))
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(HttpUserAgent);
            await using Stream downloadStream = await client.GetStreamAsync(downloadUrl);
            await using FileStream fileStream = File.Create(zipPath);
            await downloadStream.CopyToAsync(fileStream);
        }

        string scriptPath = Path.Combine(updateDir, $"apply_update_v{version}.ps1");
        string script = BuildScript(version, zipPath, ModDirectory, Process.GetCurrentProcess().Id);
        File.WriteAllText(scriptPath, script, new UTF8Encoding(false));
        WritePreparedVersion(version);

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File \"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }

    private static string BuildScript(string version, string zipPath, string targetDir, int pid)
    {
        string v = EscapePs(version);
        string z = EscapePs(zipPath);
        string t = EscapePs(targetDir);
        return $@"$ErrorActionPreference = 'Stop'
$targetPid = {pid}
$zipPath = '{z}'
$targetDir = '{t}'
$version = '{v}'

while (Get-Process -Id $targetPid -ErrorAction SilentlyContinue) {{
    Start-Sleep -Milliseconds 500
}}

Start-Sleep -Milliseconds 800
Add-Type -AssemblyName System.IO.Compression.FileSystem
$extractDir = Join-Path ([System.IO.Path]::GetDirectoryName($zipPath)) ('extract-' + $version)
if (Test-Path -LiteralPath $extractDir) {{
    Remove-Item -LiteralPath $extractDir -Recurse -Force
}}

[System.IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $extractDir)
$payloadDir = Join-Path $extractDir 'soldoros'
if (-not (Test-Path -LiteralPath $payloadDir)) {{
    $payloadDir = $extractDir
}}

New-Item -ItemType Directory -Force -Path $targetDir | Out-Null
Get-ChildItem -LiteralPath $payloadDir -Force | ForEach-Object {{
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $targetDir $_.Name) -Recurse -Force
}}

Remove-Item -LiteralPath $extractDir -Recurse -Force -ErrorAction SilentlyContinue
";
    }

    private static string EscapePs(string value) => value.Replace("'", "''");

    // 새 버전 준비 전에 이전 버전의 zip/ps1 파일 삭제
    // keepVersion에 해당하는 파일은 유지 (prepared_version.txt도 유지)
    private static void PurgeOldUpdateFiles(string updateDir, string keepVersion)
    {
        try
        {
            foreach (string file in Directory.GetFiles(updateDir))
            {
                string name = Path.GetFileName(file);
                if (name == PendingVersionFileName) continue;
                if (name == $"soldoros-v{keepVersion}.zip") continue;
                if (name == $"apply_update_v{keepVersion}.ps1") continue;
                File.Delete(file);
            }
        }
        catch { }
    }

    private static bool IsPreparedAlreadyQueued(string version)
    {
        if (string.IsNullOrWhiteSpace(ModDirectory)) return false;
        string? prepared = ReadPreparedVersion();
        if (!string.Equals(prepared, version, StringComparison.Ordinal)) return false;
        string updateDir = GetUpdateDir();
        return File.Exists(Path.Combine(updateDir, $"soldoros-v{version}.zip")) &&
               File.Exists(Path.Combine(updateDir, $"apply_update_v{version}.ps1"));
    }

    private static void CleanupIfApplied()
    {
        string? prepared = ReadPreparedVersion();
        if (string.IsNullOrWhiteSpace(prepared)) return;
        if (ParseVersion(prepared) <= CurrentVersion)
            DeletePreparedVersionFile();
    }

    private static string GetUpdateDir() => Path.Combine(ModDirectory!, PendingUpdateDirName);

    private static string GetPreparedVersionPath() => Path.Combine(GetUpdateDir(), PendingVersionFileName);

    private static string? ReadPreparedVersion()
    {
        try
        {
            string path = GetPreparedVersionPath();
            if (!File.Exists(path)) return null;
            return File.ReadAllText(path, Encoding.UTF8).Trim();
        }
        catch { return null; }
    }

    private static void WritePreparedVersion(string version)
    {
        try
        {
            Directory.CreateDirectory(GetUpdateDir());
            File.WriteAllText(GetPreparedVersionPath(), version, new UTF8Encoding(false));
        }
        catch { }
    }

    private static void DeletePreparedVersionFile()
    {
        try
        {
            string path = GetPreparedVersionPath();
            if (File.Exists(path)) File.Delete(path);
        }
        catch { }
    }

    private static SystemVersion ParseVersion(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new SystemVersion(0, 0, 0);
        string trimmed = text.Trim().TrimStart('v', 'V');
        int cut = trimmed.IndexOfAny(['-', '+', ' ']);
        if (cut >= 0) trimmed = trimmed[..cut];
        return SystemVersion.TryParse(trimmed, out SystemVersion? version) ? version : new SystemVersion(0, 0, 0);
    }

    private static void QueueToast()
    {
        _pendingToast = true;
        if (!GodotObject.IsInstanceValid(_menuContext)) return;
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(_menuContext)) return;
            ShowToast(_menuContext!);
            _pendingToast = false;
        }).CallDeferred();
    }

    private static void ShowToast(Node context)
    {
        Window? root = context.GetTree()?.Root;
        if (root == null) return;

        EnsureToast(root);
        if (_toastRoot == null || _toastLabel == null) return;

        _toastRoot.Size = new Vector2(ToastWidth, ToastHeight);
        _toastRoot.Position = GetToastPosition(root);
        _toastLabel.Text = UpdateToastText;
        _toastTween?.Kill();
        _toastRoot.Scale = Vector2.One;
        _toastRoot.Modulate = Colors.White;
        _toastTween = _toastRoot.CreateTween();
        _toastTween.TweenInterval(4.0);
        _toastTween.TweenProperty(_toastRoot, "modulate:a", 0f, 0.45);
        _toastTween.TweenProperty(_toastRoot, "scale", Vector2.One * 0.98f, 0.45);
    }

    private static void EnsureToast(Window root)
    {
        if (GodotObject.IsInstanceValid(_toastLayer) &&
            GodotObject.IsInstanceValid(_toastRoot) &&
            GodotObject.IsInstanceValid(_toastLabel))
            return;

        _toastLayer = new CanvasLayer { Name = "SoldorosUpdateToastLayer", Layer = 200 };
        root.AddChild(_toastLayer);

        _toastRoot = new Control
        {
            Name = "SoldorosUpdateToast",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Modulate = new Color(1f, 1f, 1f, 0f),
            TopLevel = true,
            Size = new Vector2(ToastWidth, ToastHeight)
        };
        _toastLayer.AddChild(_toastRoot);

        _toastLabel = new Label
        {
            Name = "SoldorosUpdateToastLabel",
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 8f,
            OffsetTop = 4f,
            OffsetRight = -8f,
            OffsetBottom = -4f,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _toastLabel.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.85f, 1f));
        _toastLabel.AddThemeColorOverride("font_outline_color", new Color(0.16f, 0.11f, 0.06f, 1f));
        _toastLabel.AddThemeConstantOverride("outline_size", 8);
        _toastLabel.AddThemeFontSizeOverride("font_size", 20);
        _toastRoot.AddChild(_toastLabel);
    }

    private static Vector2 GetToastPosition(Window root)
    {
        Vector2 size = root.Size;
        float x = Math.Max(24f, size.X - ToastWidth - ToastRight);
        float y = Math.Max(24f, size.Y - ToastHeight - ToastBottom);
        return new Vector2(x, y);
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; init; }

        [JsonPropertyName("assets")]
        public GitHubAsset[]? Assets { get; init; }
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("browser_download_url")]
        public string? BrowserDownloadUrl { get; init; }
    }
}

[HarmonyPatch(typeof(NMainMenu), "_Ready")]
internal static class SoldorosMainMenuReadyPatch
{
    [HarmonyPostfix]
    private static void Postfix(NMainMenu __instance)
    {
        SoldorosAutoUpdater.OnMainMenuReady(__instance);
    }
}
