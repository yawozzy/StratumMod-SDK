using Stratum;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExampleMod;

// Direct game access: the game's Unity and CarX classes come from the references in the SDK's refs folder
// (made by tools\Get-GameReferences.ps1), the same way as in BepInEx IL2CPP mods.
sealed class GameExamples
{
    readonly StratumMod mod;
    public GameExamples(StratumMod mod) => this.mod = mod;

    public void Draw()
    {
        var ui = mod.UI;
        ui.Label("Unity " + Application.unityVersion + ", scene \"" + SceneManager.GetActiveScene().name + "\"");

        var camera = Camera.main;
        if (camera == null) { ui.Hint("No main camera right now."); return; }
        var p = camera.transform.position;
        ui.Label($"Camera position: {p.x:0.0}, {p.y:0.0}, {p.z:0.0}");
        ui.Label("Field of view: " + camera.fieldOfView.ToString("0"));

        // Changing game objects is safe here: every Stratum.API callback runs on the Unity main thread.
        float fov = camera.fieldOfView;
        if (ui.Slider("Field of view", ref fov, 30, 110, 1)) camera.fieldOfView = fov;
        ui.Hint("The game may reset the field of view when the camera mode changes.");
    }
}
