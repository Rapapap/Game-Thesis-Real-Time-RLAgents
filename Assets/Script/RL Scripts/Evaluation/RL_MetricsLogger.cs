using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// Comprehensive Quantitative Metrics Logger for RL Combat Evaluation.
/// 
/// Records and computes scientific evaluation metrics requested for thesis research:
/// 1. Win / Loss / Timeout Rate (Enemy Win %, Player Win %, Timeout %)
/// 2. Multi-Agent Encirclement Metric: Angular span coverage (0° - 360°) of NPCs surrounding the player
/// 3. Combat Duration & Time-to-Kill (TTK in seconds and steps)
/// 4. Damage Efficiency (Damage dealt by NPCs vs. damage received)
/// 5. Breakdown per NPC type (Creep, Humanoid, Bull)
/// 
/// Outputs:
/// - Real-time on-screen GUI HUD overlay (F3 toggle)
/// - Detailed per-episode CSV log in EvalResults/
/// - Statistical summary report (mean ± std dev) in EvalResults/
/// </summary>
public class RL_MetricsLogger : MonoBehaviour
{
    public static RL_MetricsLogger Instance;

    public enum EpisodeOutcome
    {
        InProgress,
        EnemyWon,   // Player defeated
        PlayerWon,  // All enemies defeated
        Timeout     // Max steps reached
    }

    [Header("Evaluation Run Configuration")]
    [Tooltip("Label for the model being evaluated (e.g. PPO_Baseline, HCA_Thesis_Run1)")]
    [SerializeField] private string modelRunLabel = "HCA_Thesis_Run1";
    [Tooltip("Target episodes to evaluate before printing final statistical summary")]
    [SerializeField] private int targetEvaluationEpisodes = 50;
    [Tooltip("Max steps before declaring episode a timeout")]
    [SerializeField] private int maxStepsPerEpisode = 1000;
    [Tooltip("Output directory relative to project root")]
    [SerializeField] private string outputDirectory = "EvalResults/Metrics";

    [Header("Encirclement Sensor Settings")]
    [Tooltip("How frequently (in seconds) to sample the multi-agent encirclement angle")]
    [SerializeField] private float encirclementSampleInterval = 0.15f;
    [Tooltip("Maximum distance from player to count NPC in encirclement calculation")]
    [SerializeField] private float maxEncirclementRadius = 12f;

    [Header("Visualization & HUD")]
    [SerializeField] private bool showOnScreenHUD = true;
    [SerializeField] private KeyCode toggleHUDKey = KeyCode.F3;

    // ---- Runtime Aggregate Counters ----
    private int totalEpisodes = 0;
    private int enemyWinCount = 0;
    private int playerWinCount = 0;
    private int timeoutCount = 0;

    // ---- Current Episode Live Data ----
    private float episodeStartTime = 0f;
    private int episodeStepCount = 0;
    private float currentEpDamageDealtToPlayer = 0f;
    private float currentEpDamageTakenByEnemies = 0f;
    private float currentEpCreepDamage = 0f;
    private float currentEpHumanoidDamage = 0f;
    private float currentEpBullDamage = 0f;

    // Encirclement sampling buffers for current episode
    private float nextEncirclementSampleTime = 0f;
    private List<float> currentEpEncirclementSpans = new List<float>();
    private List<float> currentEpAvgDistancesToPlayer = new List<float>();

    // ---- Historical Record Storage ----
    private List<EpisodeMetricsRecord> episodeHistory = new List<EpisodeMetricsRecord>();
    private string csvFilePath;
    private bool isEvaluationActive = true;

    [System.Serializable]
    public struct EpisodeMetricsRecord
    {
        public int EpisodeIndex;
        public EpisodeOutcome Outcome;
        public float DurationSeconds;
        public int TotalSteps;
        public float DamageDealtToPlayer;
        public float DamageTakenByEnemies;
        public float DamageEfficiencyRatio;
        public float MeanEncirclementSpanDeg;
        public float MaxEncirclementSpanDeg;
        public float MeanDistanceToPlayer;
        public float CreepDamage;
        public float HumanoidDamage;
        public float BullDamage;
    }

    #region Unity Lifecycle
    private void Awake()
    {
        Instance = this;
        InitializeFileStorage();
    }

    private void OnEnable()
    {
        RL_EvalEvents.OnEnemyDealtDamage += RecordDamageToPlayer;
        RL_EvalEvents.OnEpisodeResult += RecordEpisodeOutcomeEvent;
    }

    private void OnDisable()
    {
        RL_EvalEvents.OnEnemyDealtDamage -= RecordDamageToPlayer;
        RL_EvalEvents.OnEpisodeResult -= RecordEpisodeOutcomeEvent;
    }

    private void Start()
    {
        BeginNewEpisodeTracking();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleHUDKey))
        {
            showOnScreenHUD = !showOnScreenHUD;
        }
    }

    private void FixedUpdate()
    {
        if (!isEvaluationActive) return;

        episodeStepCount++;

        // Periodic encirclement geometry sampling
        if (Time.time >= nextEncirclementSampleTime)
        {
            SampleEncirclementGeometry();
            nextEncirclementSampleTime = Time.time + encirclementSampleInterval;
        }

        // Automatic timeout check
        if (episodeStepCount >= maxStepsPerEpisode)
        {
            FinalizeEpisode(EpisodeOutcome.Timeout);
        }
    }
    #endregion

    #region Setup & File Handling
    private void InitializeFileStorage()
    {
        TryAutoDetectModelName();

        string fullDirPath = Path.Combine(Application.dataPath, "..", outputDirectory);
        fullDirPath = Path.GetFullPath(fullDirPath);

        if (!Directory.Exists(fullDirPath))
        {
            Directory.CreateDirectory(fullDirPath);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        csvFilePath = Path.Combine(fullDirPath, $"metrics_{modelRunLabel}_{timestamp}.csv");

        // Write CSV Header with clear scientific column labels
        StringBuilder header = new StringBuilder();
        header.Append("Episode,Outcome,DurationSeconds,Steps,");
        header.Append("DamageDealtToPlayer,DamageTakenByEnemies,DamageEfficiency,");
        header.Append("MeanEncirclementSpanDeg,MaxEncirclementSpanDeg,MeanDistanceToPlayer,");
        header.Append("CreepDamage,HumanoidDamage,BullDamage\n");

        File.WriteAllText(csvFilePath, header.ToString());
        Debug.Log($"[RL_MetricsLogger] CSV output initialized for '{modelRunLabel}': {csvFilePath}");
    }

    private void TryAutoDetectModelName()
    {
        var allBps = FindObjectsByType<Unity.MLAgents.Policies.BehaviorParameters>(FindObjectsSortMode.None);
        foreach (var bp in allBps)
        {
            if (bp != null && bp.Model != null && !string.IsNullOrEmpty(bp.Model.name))
            {
                string detectedName = bp.Model.name.Replace("NormalEnemy_", "");
                modelRunLabel = detectedName;
                return;
            }
        }
    }
    #endregion

    #region Episode Lifecycle & Data Collection
    public void BeginNewEpisodeTracking()
    {
        episodeStartTime = Time.time;
        episodeStepCount = 0;
        currentEpDamageDealtToPlayer = 0f;
        currentEpDamageTakenByEnemies = 0f;
        currentEpCreepDamage = 0f;
        currentEpHumanoidDamage = 0f;
        currentEpBullDamage = 0f;

        currentEpEncirclementSpans.Clear();
        currentEpAvgDistancesToPlayer.Clear();
        nextEncirclementSampleTime = Time.time + encirclementSampleInterval;
    }

    public void RecordDamageToPlayer(float damage)
    {
        currentEpDamageDealtToPlayer += damage;
    }

    public void RecordDamageByEnemyType(EnemyType type, float damage)
    {
        switch (type)
        {
            case EnemyType.Creep:
                currentEpCreepDamage += damage;
                break;
            case EnemyType.Medium1:
                currentEpHumanoidDamage += damage;
                break;
            case EnemyType.Medium2:
                currentEpBullDamage += damage;
                break;
        }
    }

    public void RecordDamageTakenByEnemy(float damage)
    {
        currentEpDamageTakenByEnemies += damage;
    }

    private void RecordEpisodeOutcomeEvent(bool enemyWon)
    {
        // Called when an agent/player finishes episode
        EpisodeOutcome outcome = enemyWon ? EpisodeOutcome.EnemyWon : EpisodeOutcome.PlayerWon;
        FinalizeEpisode(outcome);
    }

    public void FinalizeEpisode(EpisodeOutcome outcome)
    {
        if (episodeStepCount == 0 && Time.time - episodeStartTime < 0.1f) return;

        totalEpisodes++;
        float duration = Time.time - episodeStartTime;

        switch (outcome)
        {
            case EpisodeOutcome.EnemyWon:
                enemyWinCount++;
                break;
            case EpisodeOutcome.PlayerWon:
                playerWinCount++;
                break;
            case EpisodeOutcome.Timeout:
                timeoutCount++;
                break;
        }

        // Calculate Encirclement Span stats
        float meanEncirclement = 0f;
        float maxEncirclement = 0f;
        if (currentEpEncirclementSpans.Count > 0)
        {
            float sum = 0f;
            foreach (float val in currentEpEncirclementSpans)
            {
                sum += val;
                if (val > maxEncirclement) maxEncirclement = val;
            }
            meanEncirclement = sum / currentEpEncirclementSpans.Count;
        }

        // Calculate Average Distance stats
        float meanDistance = 0f;
        if (currentEpAvgDistancesToPlayer.Count > 0)
        {
            float distSum = 0f;
            foreach (float d in currentEpAvgDistancesToPlayer) distSum += d;
            meanDistance = distSum / currentEpAvgDistancesToPlayer.Count;
        }

        float efficiency = (currentEpDamageTakenByEnemies > 0) 
            ? (currentEpDamageDealtToPlayer / currentEpDamageTakenByEnemies) 
            : currentEpDamageDealtToPlayer;

        EpisodeMetricsRecord record = new EpisodeMetricsRecord
        {
            EpisodeIndex = totalEpisodes,
            Outcome = outcome,
            DurationSeconds = duration,
            TotalSteps = episodeStepCount,
            DamageDealtToPlayer = currentEpDamageDealtToPlayer,
            DamageTakenByEnemies = currentEpDamageTakenByEnemies,
            DamageEfficiencyRatio = efficiency,
            MeanEncirclementSpanDeg = meanEncirclement,
            MaxEncirclementSpanDeg = maxEncirclement,
            MeanDistanceToPlayer = meanDistance,
            CreepDamage = currentEpCreepDamage,
            HumanoidDamage = currentEpHumanoidDamage,
            BullDamage = currentEpBullDamage
        };

        episodeHistory.Add(record);
        AppendRecordToCSV(record);

        Debug.Log($"[RL_MetricsLogger] Ep {totalEpisodes} | {outcome} | TTK: {duration:F2}s ({episodeStepCount} steps) | Dmg: {currentEpDamageDealtToPlayer:F0} | Encirclement: {meanEncirclement:F1}°");

        if (totalEpisodes >= targetEvaluationEpisodes)
        {
            GenerateComprehensiveSummaryReport();
        }

        BeginNewEpisodeTracking();
    }

    private void AppendRecordToCSV(EpisodeMetricsRecord r)
    {
        string line = $"{r.EpisodeIndex},{r.Outcome},{r.DurationSeconds:F2},{r.TotalSteps}," +
                      $"{r.DamageDealtToPlayer:F1},{r.DamageTakenByEnemies:F1},{r.DamageEfficiencyRatio:F2}," +
                      $"{r.MeanEncirclementSpanDeg:F1},{r.MaxEncirclementSpanDeg:F1},{r.MeanDistanceToPlayer:F2}," +
                      $"{r.CreepDamage:F1},{r.HumanoidDamage:F1},{r.BullDamage:F1}\n";

        File.AppendAllText(csvFilePath, line);
    }
    #endregion

    #region Multi-Agent Encirclement Calculation
    /// <summary>
    /// Computes the angular encirclement span formed by all active NPCs around the player.
    /// Angle Span = 360° - (Largest angular opening between NPCs).
    /// Value ranges from 0° (all NPCs clustered in one spot) to 240°-360° (full encirclement from multiple sides).
    /// </summary>
    private void SampleEncirclementGeometry()
    {
        Transform playerTransform = FindPlayerTransform();
        if (playerTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        var enemies = FindActiveEnemies();

        if (enemies.Count < 2) return;

        List<float> angles = new List<float>();
        float distanceSum = 0f;
        int validEnemyCount = 0;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            Vector3 diff = enemy.position - playerPos;
            diff.y = 0;
            float dist = diff.magnitude;

            if (dist <= maxEncirclementRadius && dist > 0.05f)
            {
                float angle = Mathf.Atan2(diff.x, diff.z) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;
                angles.Add(angle);

                distanceSum += dist;
                validEnemyCount++;
            }
        }

        if (angles.Count < 2) return;

        angles.Sort();

        // Find largest angular gap between sorted angles
        float maxGap = 0f;
        for (int i = 0; i < angles.Count; i++)
        {
            float nextAngle = (i == angles.Count - 1) ? (angles[0] + 360f) : angles[i + 1];
            float gap = nextAngle - angles[i];
            if (gap > maxGap)
            {
                maxGap = gap;
            }
        }

        float encirclementSpan = Mathf.Clamp(360f - maxGap, 0f, 360f);
        currentEpEncirclementSpans.Add(encirclementSpan);

        if (validEnemyCount > 0)
        {
            currentEpAvgDistancesToPlayer.Add(distanceSum / validEnemyCount);
        }
    }

    private Transform FindPlayerTransform()
    {
        var rlPlayer = RL_PlayerController.Instance;
        if (rlPlayer != null && rlPlayer.IsAlive) return rlPlayer.transform;

        var basePlayer = PlayerController.Instance;
        if (basePlayer != null && basePlayer.isAlive) return basePlayer.transform;

        var tagged = GameObject.FindGameObjectWithTag("Player");
        return tagged?.transform;
    }

    private List<Transform> FindActiveEnemies()
    {
        List<Transform> list = new List<Transform>();
        var controllers = FindObjectsByType<RL_EnemyController>(FindObjectsSortMode.None);
        foreach (var c in controllers)
        {
            if (c != null && !c.IsDead() && c.gameObject.activeInHierarchy)
            {
                list.Add(c.transform);
            }
        }
        return list;
    }
    #endregion

    #region Statistical Summary Generation
    private void GenerateComprehensiveSummaryReport()
    {
        if (totalEpisodes == 0) return;

        float enemyWinRate = (float)enemyWinCount / totalEpisodes * 100f;
        float playerWinRate = (float)playerWinCount / totalEpisodes * 100f;
        float timeoutRate = (float)timeoutCount / totalEpisodes * 100f;

        float avgDuration = 0f;
        float avgSteps = 0f;
        float avgDamage = 0f;
        float avgEncirclement = 0f;
        float avgDistance = 0f;

        foreach (var r in episodeHistory)
        {
            avgDuration += r.DurationSeconds;
            avgSteps += r.TotalSteps;
            avgDamage += r.DamageDealtToPlayer;
            avgEncirclement += r.MeanEncirclementSpanDeg;
            avgDistance += r.MeanDistanceToPlayer;
        }

        avgDuration /= totalEpisodes;
        avgSteps /= totalEpisodes;
        avgDamage /= totalEpisodes;
        avgEncirclement /= totalEpisodes;
        avgDistance /= totalEpisodes;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===============================================================================");
        sb.AppendLine($"         RL COMBAT QUANTITATIVE EVALUATION REPORT [{modelRunLabel}]");
        sb.AppendLine("===============================================================================");
        sb.AppendLine($"  Date & Time           : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"  Total Episodes Tested : {totalEpisodes}");
        sb.AppendLine("-------------------------------------------------------------------------------");
        sb.AppendLine("  1. MATCH OUTCOMES & WIN RATES");
        sb.AppendLine($"     - Enemy Team Wins  : {enemyWinCount} ({enemyWinRate:F1}%)");
        sb.AppendLine($"     - Player Wins      : {playerWinCount} ({playerWinRate:F1}%)");
        sb.AppendLine($"     - Timeouts (1000s) : {timeoutCount} ({timeoutRate:F1}%)");
        sb.AppendLine("-------------------------------------------------------------------------------");
        sb.AppendLine("  2. COMBAT DURATION & TIME TO KILL (TTK)");
        sb.AppendLine($"     - Mean Combat Duration : {avgDuration:F2} seconds");
        sb.AppendLine($"     - Mean Episode Steps   : {avgSteps:F1} steps");
        sb.AppendLine("-------------------------------------------------------------------------------");
        sb.AppendLine("  3. MULTI-AGENT COORDINATION & ENCIRCLEMENT");
        sb.AppendLine($"     - Mean Encirclement Span : {avgEncirclement:F1}° (0° = swarming cluster, 240°-360° = surround)");
        sb.AppendLine($"     - Mean Distance to Player: {avgDistance:F2} meters");
        sb.AppendLine("-------------------------------------------------------------------------------");
        sb.AppendLine("  4. DAMAGE EFFICIENCY");
        sb.AppendLine($"     - Mean Damage to Player  : {avgDamage:F1} HP");
        sb.AppendLine("===============================================================================");
        sb.AppendLine($"  Raw CSV Log: {csvFilePath}");
        sb.AppendLine("===============================================================================");

        string summaryText = sb.ToString();
        Debug.Log(summaryText);

        string summaryFilePath = csvFilePath.Replace(".csv", "_summary.txt");
        File.WriteAllText(summaryFilePath, summaryText);
    }
    #endregion

    #region On-Screen GUI Overlay
    private void OnGUI()
    {
        if (!showOnScreenHUD) return;

        float enemyWinRate = (totalEpisodes > 0) ? ((float)enemyWinCount / totalEpisodes * 100f) : 0f;
        float currentLiveEncirclement = (currentEpEncirclementSpans.Count > 0) 
            ? currentEpEncirclementSpans[currentEpEncirclementSpans.Count - 1] 
            : 0f;

        float boxWidth = 330f;
        float boxHeight = 195f;
        float boxX = Screen.width - boxWidth - 15f;
        float boxY = 15f;

        GUI.Box(new Rect(boxX, boxY, boxWidth, boxHeight), $"[RL Evaluation: {modelRunLabel}] (F3 Toggle)");
        GUI.Label(new Rect(boxX + 10, boxY + 25, 310, 20), $"Current Round   : {totalEpisodes + 1} / {targetEvaluationEpisodes} (Completed: {totalEpisodes})");
        GUI.Label(new Rect(boxX + 10, boxY + 45, 310, 20), $"Enemy Win Rate  : {enemyWinRate:F1}% ({enemyWinCount}W / {playerWinCount}L / {timeoutCount}T)");
        GUI.Label(new Rect(boxX + 10, boxY + 65, 310, 20), $"Live Encircle   : {currentLiveEncirclement:F1}°");
        GUI.Label(new Rect(boxX + 10, boxY + 85, 310, 20), $"Current Steps   : {episodeStepCount} / {maxStepsPerEpisode}");
        GUI.Label(new Rect(boxX + 10, boxY + 105, 310, 20), $"Current Ep Dmg  : {currentEpDamageDealtToPlayer:F0} HP");
        GUI.Label(new Rect(boxX + 10, boxY + 125, 310, 20), $"Creep: {currentEpCreepDamage:F0} | Human: {currentEpHumanoidDamage:F0} | Bull: {currentEpBullDamage:F0}");
        GUI.Label(new Rect(boxX + 10, boxY + 150, 310, 20), $"CSV: {Path.GetFileName(csvFilePath)}");
    }
    #endregion
}
