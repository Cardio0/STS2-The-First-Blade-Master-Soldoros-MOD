using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Soldoros.SoldorosCode.Patches;

/// <summary>
/// On the main branch (v0.99.1) NEnergyCounter._Ready() expects child nodes
/// named %BurstBack / %BurstFront (CpuParticles2D), while on the beta branch
/// (v0.102.0) it expects %EnergyVfxBack / %EnergyVfxFront (NParticlesContainer).
/// BaseLib's FromLegacy only creates the beta-style nodes, so after Create()
/// returns the LegacyEnergyCounter (before it enters the tree), we add dummy
/// CpuParticles2D nodes so _Ready() won't crash on the main branch.
/// </summary>
[HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter.Create))]
public static class EnergyCounterReadyPatch
{
    private static readonly bool IsMainBranch =
        typeof(NEnergyCounter).GetField("_backParticles", BindingFlags.NonPublic | BindingFlags.Instance) != null;

    [HarmonyPostfix]
    private static void AddMissingNodes(NEnergyCounter __result)
    {
        if (!IsMainBranch || __result == null) return;

        if (__result.GetNodeOrNull("BurstBack") == null)
        {
            var node = new CpuParticles2D { Name = "BurstBack", Emitting = false };
            node.UniqueNameInOwner = true;
            __result.AddChild(node);
            node.Owner = __result;
        }

        if (__result.GetNodeOrNull("BurstFront") == null)
        {
            var node = new CpuParticles2D { Name = "BurstFront", Emitting = false };
            node.UniqueNameInOwner = true;
            __result.AddChild(node);
            node.Owner = __result;
        }
    }
}
