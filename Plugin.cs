using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlyAndLocate;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "com.snowby.flyandlocate";
    public const string PluginName = "Fly&Locate";
    public const string PluginVersion = "1.0.0";
    internal static ManualLogSource Logger = null!;
    private FlightLocator? _controller;

    public override void Load()
    {
        Logger = Log;
        try { _controller = AddComponent<FlightLocator>(); Logger.LogInfo($"{PluginName} {PluginVersion} loaded."); }
        catch (Exception ex) { Logger.LogError($"Startup failed safely: {ex}"); }
    }
}

internal sealed class FlightLocator : MonoBehaviour
{
    private bool _flight;
    private bool _locator = true;
    private Transform? _local;
    private float _nextDiscovery;
    private readonly List<Transform> _players = new();
    private GUIStyle? _style;

    private void Update()
    {
        try
        {
            if (Input.GetKeyDown(KeyCode.F)) _flight = !_flight;
            if (Input.GetKeyDown(KeyCode.L)) _locator = !_locator;
            if (Time.unscaledTime >= _nextDiscovery) { Discover(); _nextDiscovery = Time.unscaledTime + 1f; }
            if (_flight && _local != null) Fly(_local);
        }
        catch (Exception ex) { Plugin.Logger.LogWarning($"Update recovered: {ex.Message}"); }
    }

    private void Discover()
    {
        _players.Clear();
        try
        {
            var all = FindObjectsOfType<Transform>();
            foreach (var t in all)
            {
                if (!t || !t.gameObject.activeInHierarchy) continue;
                string n = t.name.ToLowerInvariant();
                if (_local == null && (n.Contains("localplayer") || n.Contains("playerlocal") || n == "player")) _local = t;
                if ((n.Contains("player") || n.Contains("avatar") || n.Contains("character")) && !_players.Contains(t)) _players.Add(t);
            }
            if (_local == null && Camera.main != null) _local = Camera.main.transform.root;
            if (_local != null) _players.RemoveAll(p => p == _local || p.root == _local.root);
        }
        catch (Exception ex) { Plugin.Logger.LogWarning($"Discovery recovered: {ex.Message}"); }
    }

    private static void Fly(Transform player)
    {
        Vector3 horizontal = new(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        float vertical = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q) ? 1f : 0f);
        float speed = Input.GetKey(KeyCode.LeftShift) ? 24f : 8f;
        Vector3 move = (Camera.main != null ? Camera.main.transform.TransformDirection(horizontal) : horizontal);
        move.y = vertical; player.position += move.normalized * speed * Time.deltaTime;
    }

    private void OnGUI()
    {
        try
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.white } };
            GUI.Label(new Rect(12, 12, 500, 28), $"Fly&Locate | Flight: {(_flight ? "ON" : "OFF")} [F] | Locator: {(_locator ? "ON" : "OFF")} [L]", _style);
            if (!_locator || _local == null) return;
            int y = 44;
            foreach (var p in _players.Where(p => p != null && p.gameObject.activeInHierarchy).OrderBy(p => Vector3.Distance(_local.position, p.position)))
            {
                Vector3 delta = p.position - _local.position; float distance = delta.magnitude;
                float bearing = Mathf.Repeat(Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, 360f);
                GUI.Label(new Rect(18, y, 600, 24), $"{p.name}: {distance:0.0} m  {Arrow(bearing)} {bearing:0}°", _style); y += 23;
            }
        }
        catch (Exception ex) { Plugin.Logger.LogWarning($"GUI recovered: {ex.Message}"); }
    }

    private static string Arrow(float bearing) => bearing switch { >= 337.5f or < 22.5f => "↑", < 67.5f => "↗", < 112.5f => "→", < 157.5f => "↘", < 202.5f => "↓", < 247.5f => "↙", < 292.5f => "←", _ => "↖" };
}
