using System;

[Serializable]
public class FSMState
{
    public string ID;
    public FSMAction[] Actions;
    public FSMTransition[] Transitions;

    public void UpdateState(EnemyBrain enemyBrain)
    {
        ExecuteActions();
        ExecuteTransitions(enemyBrain);
    }

    private void ExecuteActions()
    {
        if (Actions == null || Actions.Length == 0) return;

        for (int i = 0; i < Actions.Length; i++)
        {
            if (Actions[i] != null)
            {
                Actions[i].Act();
            }
        }
    }

    private void ExecuteTransitions(EnemyBrain enemyBrain)
    {
        if (Transitions == null || Transitions.Length <= 0) return;

        for (int i = 0; i < Transitions.Length; i++)
        {
            // Skip ungültige Transitions
            if (Transitions[i] == null || Transitions[i].Decision == null)
            {
                continue;
            }

            bool decisionResult = Transitions[i].Decision.Decide();

            if (decisionResult)
            {
                // Decision ist TRUE → TrueState
                if (!string.IsNullOrEmpty(Transitions[i].TrueState))
                {
                    enemyBrain.ChangeState(Transitions[i].TrueState);
                    return; 
                }
                // Wenn TrueState leer ist, bleibt Enemy im aktuellen State
            }
            else
            {
                // Decision ist FALSE → FalseState
                if (!string.IsNullOrEmpty(Transitions[i].FalseState))
                {
                    enemyBrain.ChangeState(Transitions[i].FalseState);
                    return; 
                }
                // Wenn FalseState leer ist, bleibt Enemy im aktuellen State
            }
        }
    }
}