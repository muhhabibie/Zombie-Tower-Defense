// DebuffReceiver.cs
using System.Collections.Generic;
using UnityEngine;

public class DebuffReceiver : MonoBehaviour
{
    // simpan semua slow dari banyak sumber (kunci = source aura)
    private readonly Dictionary<object, float> _slows = new();

    // multiplier efektif (pakai slow terkuat -> nilai terkecil)
    public float AttackSpeedMul => ComposeMin();
    public float MoveSpeedMul   => ComposeMin();   // kalau nanti butuh gerak player
    public float RotationMul    => ComposeMin();   // untuk putar tower

    public void ApplySlow(object source, float multiplier01)
    {
        _slows[source] = Mathf.Clamp01(multiplier01); // 0..1 (0 = stop, 1 = normal)
    }

    public void RemoveSlow(object source)
    {
        _slows.Remove(source);
    }

    float ComposeMin()
    {
        float m = 1f;
        foreach (var kv in _slows) m = Mathf.Min(m, kv.Value);
        return m;
    }
}
