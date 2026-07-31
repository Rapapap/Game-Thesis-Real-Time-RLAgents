using UnityEngine;
using Unity.MLAgents.Sensors;

/// <summary>
/// SensorComponent wrapper for ManagerObservationSensor.
/// 
/// ML-Agents discovers sensors via SensorComponent attached to the agent.
/// This component creates the ManagerObservationSensor (plain C# ISensor, not MonoBehaviour)
/// and passes it the required references.
///
/// Usage:
///   1. Add this component to the agent GameObject (alongside BehaviorParameters)
///   2. It will automatically register as an additional observation sensor
///   3. The HCA Python trainer separates this from worker observations by index
/// </summary>
[RequireComponent(typeof(NormalEnemyAgent))]
public class ManagerObservationSensorComponent : SensorComponent
{
    [Header("Arena Configuration")]
    [Tooltip("Half-size of the arena in X. Used for normalizing world positions.")]
    [SerializeField] private float arenaHalfSizeX = 10f;
    
    [Tooltip("Half-size of the arena in Z. Used for normalizing world positions.")]
    [SerializeField] private float arenaHalfSizeZ = 10f;
    
    [Tooltip("Center offset of the arena (if arena is not at world origin).")]
    [SerializeField] private Vector3 arenaCenter = Vector3.zero;
    
    [Header("Observation Mode")]
    [Tooltip("If true, uses legacy observation mode (health ratio at idx 7, ordinal quadrant at idx 13). If false (default), uses redesigned observation mode (center distance at idx 7, border proximity at idx 13).")]
    [SerializeField] private bool useFallbackLegacyObservations = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos;

    private ManagerObservationSensor sensor;

    public override ISensor[] CreateSensors()
    {
        var agent = GetComponent<NormalEnemyAgent>();
        var controller = GetComponent<RL_EnemyController>();
        
        // Auto-detect arena center from parent if not set
        if (arenaCenter == Vector3.zero && transform.parent != null)
        {
            arenaCenter = transform.parent.position;
        }
        
        // Create plain C# sensor (not a MonoBehaviour — no Inspector serialization issues)
        sensor = new ManagerObservationSensor(
            transform, agent, controller,
            arenaCenter, arenaHalfSizeX, arenaHalfSizeZ,
            useFallbackLegacyObservations
        );
        
        return new ISensor[] { sensor };
    }
    
    /// <summary>
    /// Access the underlying sensor for recording attack stats.
    /// </summary>
    public ManagerObservationSensor Sensor => sensor;

    /// <summary>
    /// Set arena bounds at runtime (e.g., from training manager).
    /// </summary>
    public void SetArenaBounds(Vector3 center, float halfX, float halfZ)
    {
        arenaCenter = center;
        arenaHalfSizeX = halfX;
        arenaHalfSizeZ = halfZ;
        
        sensor?.UpdateArenaBounds(center, halfX, halfZ);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(arenaCenter, new Vector3(arenaHalfSizeX * 2, 1f, arenaHalfSizeZ * 2));
    }
}

