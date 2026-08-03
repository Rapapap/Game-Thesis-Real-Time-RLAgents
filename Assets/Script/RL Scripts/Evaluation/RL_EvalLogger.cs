using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Mencatat metrik evaluasi (Win-Rate dan Average Damage Dealt) ke file CSV.
/// Pasang script ini di GameManager atau Training Manager di scene evaluasi.
/// Aktif saat inference mode (TrainingActive = false).
/// </summary>
public class RL_EvalLogger : MonoBehaviour
{
    [Header("Evaluation Settings")]
    [Tooltip("Nama run yang sedang dievaluasi (contoh: PPO_v1 atau HCA_v1)")]
    [SerializeField] private string runLabel = "PPO_v1";
    [Tooltip("Jumlah episode yang dievaluasi sebelum menyimpan ringkasan")]
    [SerializeField] private int targetEpisodes = 50;
    [Tooltip("Simpan CSV ke folder ini (relatif dari root project)")]
    [SerializeField] private string outputFolder = "EvalResults";

    [Header("References")]
    [SerializeField] private bool autoFindAgents = true;

    // ---- Statistik per-episode ----
    private int episodeCount = 0;
    private int enemyWins = 0;          // Enemy berhasil membunuh player
    private float totalDamageDealt = 0f; // Total damage enemy ke player (semua episode)

    // ---- Data per-episode untuk CSV detail ----
    private List<EpisodeRecord> records = new List<EpisodeRecord>();

    // ---- Data per episode aktif saat ini ----
    private float currentEpisodeDamage = 0f;
    private bool currentEpisodeEnemyWon = false;

    private string csvPath;

    private struct EpisodeRecord
    {
        public int Episode;
        public float DamageDealt;
        public bool EnemyWon;
    }

    #region Unity Lifecycle
    private void Awake()
    {
        // Buat folder output jika belum ada
        string dir = Path.Combine(Application.dataPath, "..", outputFolder);
        dir = Path.GetFullPath(dir);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        csvPath = Path.Combine(dir, $"eval_{runLabel}_{timestamp}.csv");

        // Tulis header CSV
        File.WriteAllText(csvPath, "Episode,DamageDealt,EnemyWon\n");
        Debug.Log($"[RL_EvalLogger] Output: {csvPath}");
    }

    private void OnEnable()
    {
        // Subscribe ke event dari agent dan controller
        RL_EvalEvents.OnEnemyDealtDamage += HandleEnemyDamage;
        RL_EvalEvents.OnEpisodeResult += HandleEpisodeResult;
    }

    private void OnDisable()
    {
        RL_EvalEvents.OnEnemyDealtDamage -= HandleEnemyDamage;
        RL_EvalEvents.OnEpisodeResult -= HandleEpisodeResult;
    }
    #endregion

    #region Event Handlers
    private void HandleEnemyDamage(float damage)
    {
        currentEpisodeDamage += damage;
        totalDamageDealt += damage;
    }

    private void HandleEpisodeResult(bool enemyWon)
    {
        episodeCount++;
        currentEpisodeEnemyWon = enemyWon;

        if (enemyWon)
            enemyWins++;

        // Simpan record episode ini
        records.Add(new EpisodeRecord
        {
            Episode = episodeCount,
            DamageDealt = currentEpisodeDamage,
            EnemyWon = currentEpisodeEnemyWon
        });

        // Append ke CSV
        File.AppendAllText(csvPath, $"{episodeCount},{currentEpisodeDamage:F2},{(currentEpisodeEnemyWon ? 1 : 0)}\n");

        Debug.Log($"[RL_EvalLogger] Episode {episodeCount}: Damage={currentEpisodeDamage:F2}, EnemyWon={enemyWon}");

        // Reset per-episode data
        currentEpisodeDamage = 0f;
        currentEpisodeEnemyWon = false;

        // Cetak ringkasan jika target episode tercapai
        if (episodeCount >= targetEpisodes)
            PrintSummary();
    }
    #endregion

    #region Summary
    private void PrintSummary()
    {
        float winRate = (float)enemyWins / episodeCount * 100f;
        float avgDamage = totalDamageDealt / episodeCount;

        string summary =
            $"\n=== EVALUATION SUMMARY [{runLabel}] ===\n" +
            $"  Total Episodes  : {episodeCount}\n" +
            $"  Enemy Win-Rate  : {winRate:F1}% ({enemyWins}/{episodeCount})\n" +
            $"  Avg Damage/Ep   : {avgDamage:F2}\n" +
            $"  CSV Output      : {csvPath}\n" +
            $"==========================================";

        Debug.Log(summary);

        // Simpan ringkasan ke file terpisah
        string summaryPath = csvPath.Replace(".csv", "_summary.txt");
        File.WriteAllText(summaryPath, summary);
    }

    /// <summary>Panggil dari UI button untuk mencetak ringkasan kapan saja</summary>
    public void PrintCurrentSummary() => PrintSummary();
    #endregion
}
