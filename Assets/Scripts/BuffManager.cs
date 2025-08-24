using UnityEngine;

public static class BuffManager
{
    // ===== Buff multipliers (global) =====
    public static float PlayerDamageMul     = 1f;  // dipakai PlayerMelee
    public static float TurretDamageMul     = 1f;  // dipakai Projectile (damage)
    public static float TowerAttackRangeMul = 1f;  // dipakai Tower (range)
    public static float PlacementRangeMul   = 1f;  // dipakai TowerPlacement (jarak taruh)
    public static float BulletSpeedMul      = 1f;  // dipakai Projectile (speed)

    // ===== Enemy scaling per round (global, diupdate oleh EnemySpawner) =====
    public static float CurrentEnemyHpMul    = 1f;
    public static float CurrentEnemySpeedMul = 1f;

    public enum BuffType
    {
        PlayerDamage,
        TurretDamage,
        TowerAttackRange,
        PlacementRange,
        BulletSpeed
    }

    public static void ApplyBuff(BuffType type, float value)
    {
        switch (type)
        {
            case BuffType.PlayerDamage:     PlayerDamageMul     *= value; break;
            case BuffType.TurretDamage:     TurretDamageMul     *= value; break;
            case BuffType.TowerAttackRange: TowerAttackRangeMul *= value; break;
            case BuffType.PlacementRange:   PlacementRangeMul   *= value; break;
            case BuffType.BulletSpeed:      BulletSpeedMul      *= value; break;
        }
        Debug.Log($"[BUFF] {type} x{value} | PD:{PlayerDamageMul:F2} TD:{TurretDamageMul:F2} TR:{TowerAttackRangeMul:F2} PR:{PlacementRangeMul:F2} BS:{BulletSpeedMul:F2}");
    }
}
