using UnityEngine;

public class RL_TrainingPlayer : MonoBehaviour
{
    private RL_TrainingPlayerSpawner spawner;
    private bool isBeingDestroyed = false;

    public void Initialize(RL_TrainingPlayerSpawner targetSpawner) => spawner = targetSpawner;

    public void ForceNotifyDestruction()
    {
        if (!isBeingDestroyed)
            HandleDestruction();
    }

    private void OnDestroy() => HandleDestruction();

    private void HandleDestruction()
    {
        if (isBeingDestroyed) return;
        
        isBeingDestroyed = true;
        spawner?.OnTargetDestroyed(gameObject);
    }
}