using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

/// <summary>
/// Manager Observation Sensor for HCA (Hierarchical Critic Assignment).
/// 
/// Plain C# class implementing ISensor (NOT a MonoBehaviour).
/// Created and owned by ManagerObservationSensorComponent.
///
/// Provides GLOBAL (arena-wide) observations for the manager critic (20 dimensions):
///   [0-1]   Agent world position (normalized x, z)
///   [2-3]   Player world position (normalized x, z)
///   [4-5]   Relative direction agent->player (world, normalized)
///   [6]     Player distance (normalized to arena diagonal)
///   [7]     Agent distance from arena center (normalized to half-diagonal)
///   [8-12]  Behavior state one-hot (patrolling, chasing, attacking, fleeing, idle)
///   [13]    Arena border proximity (normalized 0 to 1, boundary awareness)
///   [14]    Episode time pressure (stepCount / maxStep)
///   [15]    Engagement success ratio
///   [16-17] Teammate 1 world position (normalized x, z)
///   [18-19] Teammate 2 world position (normalized x, z)
///
/// Based on: Cao and Lin (2020) - "Reinforcement Learning from Hierarchical Critics"
/// </summary>
public class ManagerObservationSensor : ISensor
{
    private const int ObservationSize = 20;
    private const string SensorName = "ManagerObservation";
    private const int EngagementWindow = 20;

    // References passed from SensorComponent
    private readonly Transform agentTransform;
    private readonly NormalEnemyAgent agent;
    private readonly RL_EnemyController controller;

    // Arena config
    private Vector3 arenaCenter;
    private float arenaHalfSizeX;
    private float arenaHalfSizeZ;
    private float arenaDiagonal;

    // Engagement tracking
    private int recentAttackAttempts;
    private int recentAttackHits;

    // Cached player and teammates
    private Transform cachedPlayerTransform;
    private float lastPlayerSearchTime;
    private NormalEnemyAgent[] cachedTeammates;

    private readonly float[] observations = new float[ObservationSize];

    private readonly bool useFallbackLegacy;

    public ManagerObservationSensor(
        Transform agentTransform,
        NormalEnemyAgent agent,
        RL_EnemyController controller,
        Vector3 arenaCenter,
        float arenaHalfSizeX,
        float arenaHalfSizeZ,
        bool useFallbackLegacy = false)
    {
        this.agentTransform = agentTransform;
        this.agent = agent;
        this.controller = controller;
        this.arenaCenter = arenaCenter;
        this.arenaHalfSizeX = arenaHalfSizeX;
        this.arenaHalfSizeZ = arenaHalfSizeZ;
        this.arenaDiagonal = Mathf.Sqrt(arenaHalfSizeX * arenaHalfSizeX + arenaHalfSizeZ * arenaHalfSizeZ) * 2f;
        this.useFallbackLegacy = useFallbackLegacy;
    }

    public void UpdateArenaBounds(Vector3 center, float halfX, float halfZ)
    {
        arenaCenter = center;
        arenaHalfSizeX = halfX;
        arenaHalfSizeZ = halfZ;
        arenaDiagonal = Mathf.Sqrt(halfX * halfX + halfZ * halfZ) * 2f;
    }

    public void RecordAttackAttempt(bool hit)
    {
        recentAttackAttempts++;
        if (hit) recentAttackHits++;

        if (recentAttackAttempts > EngagementWindow)
        {
            recentAttackAttempts = Mathf.CeilToInt(recentAttackAttempts * 0.5f);
            recentAttackHits = Mathf.CeilToInt(recentAttackHits * 0.5f);
        }
    }

    public void ResetEngagement()
    {
        recentAttackAttempts = 0;
        recentAttackHits = 0;
    }

    // ---- ISensor Implementation ----

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(ObservationSize);
    }

    public int Write(ObservationWriter writer)
    {
        CollectObservations();
        for (int i = 0; i < ObservationSize; i++)
        {
            writer[i] = observations[i];
        }
        return ObservationSize;
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public string GetName()
    {
        return SensorName;
    }

    public void Update() { }

    public void Reset()
    {
        ResetEngagement();
        cachedTeammates = null;
        System.Array.Clear(observations, 0, ObservationSize);
    }

    // ---- Observation Collection ----

    private void CollectObservations()
    {
        try
        {
            // Guard: if the agent transform was destroyed or null, write zeros
            if (agentTransform == null)
            {
                System.Array.Clear(observations, 0, ObservationSize);
                return;
            }

            int idx = 0;

            // [0-1] Agent world position (normalized to arena bounds)
            Vector3 agentRelPos = agentTransform.position - arenaCenter;
            observations[idx++] = Mathf.Clamp(agentRelPos.x / (arenaHalfSizeX > 0 ? arenaHalfSizeX : 1f), -1f, 1f);
            observations[idx++] = Mathf.Clamp(agentRelPos.z / (arenaHalfSizeZ > 0 ? arenaHalfSizeZ : 1f), -1f, 1f);

            // [2-6] Player-related global observations
            Transform player = FindPlayer();
            if (player != null)
            {
                Vector3 playerRelPos = player.position - arenaCenter;
                observations[idx++] = Mathf.Clamp(playerRelPos.x / (arenaHalfSizeX > 0 ? arenaHalfSizeX : 1f), -1f, 1f);
                observations[idx++] = Mathf.Clamp(playerRelPos.z / (arenaHalfSizeZ > 0 ? arenaHalfSizeZ : 1f), -1f, 1f);

                Vector3 relativeDir = player.position - agentTransform.position;
                float distance = relativeDir.magnitude;

                if (distance > 0.01f)
                {
                    relativeDir = relativeDir.normalized;
                    observations[idx++] = relativeDir.x;
                    observations[idx++] = relativeDir.z;
                }
                else
                {
                    observations[idx++] = 0f;
                    observations[idx++] = 0f;
                }

                float diag = arenaDiagonal > 0 ? arenaDiagonal : 1f;
                observations[idx++] = Mathf.Clamp01(distance / diag);
            }
            else
            {
                observations[idx++] = 0f;
                observations[idx++] = 0f;
                observations[idx++] = 0f;
                observations[idx++] = 0f;
                observations[idx++] = 1f;
            }

            // [7] Agent distance from arena center (Redesigned) or Health ratio (Legacy Fallback)
            if (useFallbackLegacy)
            {
                if (controller != null && controller.enemyData != null && controller.enemyData.enemyHealth > 0)
                    observations[idx++] = Mathf.Clamp01(controller.enemyHP / controller.enemyData.enemyHealth);
                else
                    observations[idx++] = 1f;
            }
            else
            {
                float distFromCenter = agentRelPos.magnitude;
                float halfDiag = (arenaDiagonal > 0 ? arenaDiagonal : 1f) * 0.5f;
                observations[idx++] = Mathf.Clamp01(distFromCenter / halfDiag);
            }

            // [8-12] Behavior state one-hot: patrolling, chasing, attacking, fleeing, idle
            bool isPatrolling = false, isChasing = false, isAttacking = false, isFleeing = false, isIdle = false;

            if (agent != null && !agent.IsDead)
            {
                string state = agent.CurrentBehaviorState ?? "Idle";
                string action = agent.CurrentBehaviorAction ?? "Idle";

                if (state == "Attacking" || action == "Attacking")
                    isAttacking = true;
                else if (state == "Fleeing" || action == "Fleeing")
                    isFleeing = true;
                else if (state == "Chasing" || action == "Chasing")
                    isChasing = true;
                else if (state == "Patrolling" || action == "Patrolling")
                    isPatrolling = true;
                else
                    isIdle = true;
            }

            observations[idx++] = isPatrolling ? 1f : 0f;
            observations[idx++] = isChasing ? 1f : 0f;
            observations[idx++] = isAttacking ? 1f : 0f;
            observations[idx++] = isFleeing ? 1f : 0f;
            observations[idx++] = isIdle ? 1f : 0f;

            // [13] Arena border proximity (Redesigned) or Quadrant ordinal (Legacy Fallback)
            if (useFallbackLegacy)
            {
                int quadrant = GetArenaQuadrant(agentRelPos);
                observations[idx++] = quadrant / 3f;
            }
            else
            {
                float halfX = arenaHalfSizeX > 0 ? arenaHalfSizeX : 1f;
                float halfZ = arenaHalfSizeZ > 0 ? arenaHalfSizeZ : 1f;
                float borderProximity = Mathf.Max(Mathf.Abs(agentRelPos.x / halfX), Mathf.Abs(agentRelPos.z / halfZ));
                observations[idx++] = Mathf.Clamp01(borderProximity);
            }

            // [14] Episode time pressure
            if (agent != null)
            {
                float maxStep = agent.MaxStep > 0 ? agent.MaxStep : 5000f;
                observations[idx++] = Mathf.Clamp01(agent.StepCount / maxStep);
            }
            else
            {
                observations[idx++] = 0f;
            }

            // [15] Engagement success ratio
            float engagementRatio = recentAttackAttempts > 0
                ? (float)recentAttackHits / recentAttackAttempts
                : 0f;
            observations[idx++] = engagementRatio;

            // [16-17] Teammate 1 world position (normalized x, z)
            // [18-19] Teammate 2 world position (normalized x, z)
            int teammatesAdded = 0;
            if (agentTransform != null && agentTransform.parent != null)
            {
                if (cachedTeammates == null || cachedTeammates.Length == 0)
                {
                    cachedTeammates = agentTransform.parent.GetComponentsInChildren<NormalEnemyAgent>();
                }

                for (int i = 0; i < cachedTeammates.Length; i++)
                {
                    var tm = cachedTeammates[i];
                    if (teammatesAdded >= 2) break;
                    if (tm == null || tm.transform == agentTransform || tm.IsDead || !tm.gameObject.activeInHierarchy)
                        continue;

                    Vector3 tmRelPos = tm.transform.position - arenaCenter;
                    observations[idx++] = Mathf.Clamp(tmRelPos.x / (arenaHalfSizeX > 0 ? arenaHalfSizeX : 1f), -1f, 1f);
                    observations[idx++] = Mathf.Clamp(tmRelPos.z / (arenaHalfSizeZ > 0 ? arenaHalfSizeZ : 1f), -1f, 1f);
                    teammatesAdded++;
                }
            }

            // Fill missing teammate observations with 0f if fewer than 2 teammates found
            while (teammatesAdded < 2)
            {
                observations[idx++] = 0f;
                observations[idx++] = 0f;
                teammatesAdded++;
            }

            // Fill missing teammate observations with 0f if fewer than 2 teammates found
            while (teammatesAdded < 2)
            {
                observations[idx++] = 0f;
                observations[idx++] = 0f;
                teammatesAdded++;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[ManagerObservationSensor] Exception guarded in CollectObservations: {ex.Message}");
            System.Array.Clear(observations, 0, ObservationSize);
        }
    }

    private int GetArenaQuadrant(Vector3 relativePosition)
    {
        if (relativePosition.x >= 0 && relativePosition.z >= 0) return 0;
        if (relativePosition.x < 0 && relativePosition.z >= 0) return 1;
        if (relativePosition.x < 0 && relativePosition.z < 0) return 2;
        return 3;
    }

    private Transform FindPlayer()
    {
        if (cachedPlayerTransform != null && cachedPlayerTransform.gameObject.activeInHierarchy)
        {
            return cachedPlayerTransform;
        }

        if (Time.time - lastPlayerSearchTime > 1f)
        {
            lastPlayerSearchTime = Time.time;

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                cachedPlayerTransform = playerObj.transform;
                return cachedPlayerTransform;
            }
        }

        return cachedPlayerTransform;
    }
}

