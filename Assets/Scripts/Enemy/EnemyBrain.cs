using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState; // Initial state ID
    [SerializeField] private FSMState[] states; // Array of all states

    public FSMState CurrentState { get; set; }
    public Transform Player { get; set; }

    private void Start()
    {
        // Player suchen (optional, für spätere Features)
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player")?.transform;
            if (Player == null)
            {
                Debug.LogWarning($"[{gameObject.name}]!");
            }
        }

        if (string.IsNullOrEmpty(initState))
        {
            Debug.LogError($"[{gameObject.name}] Init State ist leer! Bitte im Inspector setzen.");
            return;
        }

        if (states == null || states.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] Keine States konfiguriert! Bitte im Inspector setzen.");
            return;
        }

        Debug.Log($"[{gameObject.name}] Starte FSM mit State: {initState}");
        ChangeState(initState);
    }

    private void Update()
    {
        if (CurrentState == null)
        {
            Debug.LogWarning($"[{gameObject.name}] CurrentState ist null!");
            return;
        }

        CurrentState.UpdateState(this);

        if (Player != null)
        {
            Vector3 directionToPlayer = new Vector3(Player.position.x, transform.position.y, Player.position.z) - transform.position;

            if (directionToPlayer.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    public void ChangeState(string newStateID)
    {
        FSMState newState = GetState(newStateID);
        if (newState == null)
        {
            Debug.LogError($"[{gameObject.name}] State '{newStateID}' nicht gefunden!");
            return;
        }

        Debug.Log($"[{gameObject.name}] Wechsel zu State: {newStateID}");
        CurrentState = newState;

        if (newState.Actions != null && newState.Actions.Length > 0)
        {
            Debug.Log($"[{gameObject.name}] State hat {newState.Actions.Length} Actions");
            for (int i = 0; i < newState.Actions.Length; i++)
            {
                if (newState.Actions[i] != null)
                {
                    Debug.Log($"  - Action {i}: {newState.Actions[i].GetType().Name}");
                }
                else
                {
                    Debug.LogWarning($"  - Action {i}: NULL!");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] State '{newStateID}' hat keine Actions!");
        }
    }

    private FSMState GetState(string newStateID)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].ID == newStateID)
            {
                return states[i];
            }
        }
        return null;
    }
}