using System;

/// <summary>
/// Event bus statis untuk komunikasi antar komponen evaluasi.
/// Tidak perlu referensi langsung — agent/controller cukup memanggil static events ini.
/// </summary>
public static class RL_EvalEvents
{
    /// <summary>
    /// Dipanggil setiap kali enemy berhasil mengenai player.
    /// Parameter: damage amount (float)
    /// </summary>
    public static event Action<float> OnEnemyDealtDamage;

    /// <summary>
    /// Dipanggil saat episode berakhir.
    /// Parameter: true jika enemy menang (player mati), false jika enemy kalah (enemy mati)
    /// </summary>
    public static event Action<bool> OnEpisodeResult;

    /// <summary>Panggil dari RL_EnemyController saat serangan enemy mengenai player</summary>
    public static void RaiseEnemyDealtDamage(float damage)
        => OnEnemyDealtDamage?.Invoke(damage);

    /// <summary>Panggil dari NormalEnemyAgent saat episode berakhir</summary>
    public static void RaiseEpisodeResult(bool enemyWon)
        => OnEpisodeResult?.Invoke(enemyWon);
}
