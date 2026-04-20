using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.TestSupport;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// NAudioManager.PlayOneShot()를 패치하여
/// "res://" 경로로 시작하는 사운드를 Godot AudioStreamPlayer로 직접 재생합니다.
/// FMOD 이벤트를 등록할 수 없는 모드에서 커스텀 사운드를 사용하는 방식입니다.
/// </summary>
[HarmonyPatch(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot), typeof(string),
    typeof(Dictionary<string, float>), typeof(float))]
public static class NAudioManagerPatch
{
    [HarmonyPrefix]
    private static bool Prefix(string path, Dictionary<string, float> parameters, float volume,
        NAudioManager __instance)
    {
        if (TestMode.IsOn) return true;
        if (!path.StartsWith("res://")) return true;

        var audioStream = GD.Load<AudioStream>(path);
        if (audioStream is null) return true;

        AudioStreamPlayer2D audioPlayer = new()
        {
            Bus = "SFX",
            VolumeDb = Mathf.LinearToDb(volume * 7),
            Stream = audioStream,
        };

        __instance.GetTree().Root.AddChild(audioPlayer);
        audioPlayer.Finished += audioPlayer.QueueFree;
        audioPlayer.Play();

        return false; // 기본 FMOD 호출 스킵
    }
}
