using UnityEngine;
using Unity.MLAgents;

public class NormalEnemyRewards : MonoBehaviour
{
    #region Reward Configuration
    [Header("Massive Rewards/Punishments (+1 / -1)")]
    public float KillPlayerReward = +1f;
    public float DiedByPlayerPunishment = -1f;

    [Header("Major Rewards/Punishments (one-shot events)")]
    public float DetectPlayerReward = +0.10f;
    public float PatrolCompleteReward = +0.15f;
    public float ChasePlayerReward = +0.20f;
    public float AttackPlayerReward = +0.30f;
    public float HitByPlayerPunishment = -0.20f;

    [Header("Rewards/Punishments (per-step / frequent, time-scaled)")]
    public float PatrolStepReward = +0.001f;
    public float PatrolWrongStepPunishment = -0.001f;
    public float ObstaclePunishment = -0.02f;
    public float NoMovementPunishment = -0.001f;
    public float ApproachPlayerReward = +0.005f;
    public float ChaseStepReward = +0.005f;
    public float DoesntChasePlayerPunishment = -0.005f;
    public float FailApproachPlayerPunishment = -0.005f;
    public float DoesntAttackInstantlyPunishment = -0.02f;
    public float AttackMissedPunishment = -0.35f;

    #endregion

    #region Massive Rewards
    public void AddKillPlayerReward(Agent agent)
    {
        agent.AddReward(KillPlayerReward);
    }

    public void AddDeathPunishment(Agent agent)
    {
        agent.AddReward(DiedByPlayerPunishment);
    }
    #endregion

    #region Major Rewards
    public void AddDetectionReward(Agent agent)
    {
        agent.AddReward(DetectPlayerReward);
    }

    public void AddPatrolReward(Agent agent)
    {
        agent.AddReward(PatrolCompleteReward);
    }

    public void AddChasePlayerReward(Agent agent)
    {
        agent.AddReward(ChasePlayerReward);
    }

    public void AddAttackReward(Agent agent)
    {
        agent.AddReward(AttackPlayerReward);
    }

    public void AddDamagePunishment(Agent agent)
    {
        agent.AddReward(HitByPlayerPunishment);
    }

    public void AddObstaclePunishment(Agent agent, float deltaTime)
    {
        agent.AddReward(ObstaclePunishment * deltaTime);
    }

    #endregion

        #region Rewards

    public void AddPatrolStepReward(Agent agent, float deltaTime)
    {
        agent.AddReward(PatrolStepReward * deltaTime);
    }

    public void AddChaseStepReward(Agent agent, float deltaTime)
    {
        agent.AddReward(ChaseStepReward * deltaTime);
    }

    public void AddNoMovementPunishment(Agent agent, float deltaTime)
    {
        agent.AddReward(NoMovementPunishment * deltaTime);
    }

    public void AddApproachPlayerReward(Agent agent, float deltaTime)
    {
        agent.AddReward(ApproachPlayerReward * deltaTime);
    }

    public void AddDoesntChasePlayerPunishment(Agent agent, float deltaTime)
    {
        agent.AddReward(DoesntChasePlayerPunishment * deltaTime);
    }

    public void AddPatrolWrongStepPunishment(Agent agent)
    {
        agent.AddReward(PatrolWrongStepPunishment * Time.deltaTime);
    }

    public void AddFailApproachPlayerPunishment(Agent agent)
    {
        agent.AddReward(FailApproachPlayerPunishment);
    }

    public void AddDoesntAttackInstantlyPunishment(Agent agent)
    {
        agent.AddReward(DoesntAttackInstantlyPunishment * Time.deltaTime);
    }

    public void AddAttackMissedPunishment(Agent agent)
    {
        agent.AddReward(AttackMissedPunishment);
    }

    #endregion
}

