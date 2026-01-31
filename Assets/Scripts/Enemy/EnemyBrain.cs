using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private string initState;
    [SerializeField] private FSMState[] states;

    public FSMState CurrentState { get; set; }
    public Transform Player { get; set; }

    private void Start()
    {
        // Player suchen
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player")?.transform;
            if (Player == null)
            {
                Debug.LogWarning($"[{gameObject.name}] Kein Player gefunden!");
            }
        }

        // Validierung
        if (string.IsNullOrEmpty(initState))
        {
            Debug.LogError($"[{gameObject.name}] Init State ist leer!");
            return;
        }

        if (states == null || states.Length == 0)
        {
            Debug.LogError($"[{gameObject.name}] Keine States konfiguriert!");
            return;
        }

        Debug.Log($"[{gameObject.name}] Starte FSM mit State: {initState}");
        ChangeState(initState);
    }

    private void Update()
    {
        if (CurrentState == null)
        {
            if (Time.frameCount % 300 == 0)
            {
                Debug.LogWarning($"[{gameObject.name}] CurrentState ist null!");
            }
            return;
        }

        // NUR State Actions ausführen - KEINE Rotation hier!
        // Die Actions (Wander, Chase, Attack) kümmern sich selbst um Rotation
        CurrentState.UpdateState(this);
    }

    public void ChangeState(string newStateID)
    {
        FSMState newState = GetState(newStateID);

        if (newState == null)
        {
            Debug.LogError($"[{gameObject.name}] State '{newStateID}' nicht gefunden!");
            return;
        }

        string previousState = CurrentState != null ? CurrentState.ID : "NULL";
        CurrentState = newState;

        Debug.Log($"[{gameObject.name}] State: '{previousState}' → '{newStateID}'");
    }

    private FSMState GetState(string newStateID)
    {
        if (states == null || states.Length == 0) return null;

        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] != null && states[i].ID == newStateID)
            {
                return states[i];
            }
        }

        return null;
    }
}