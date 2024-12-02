using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int numberOfCharacters = 2;

    public static GameManager Instance { get; private set; }

    // properties
    private int character_value = 0;
    public int characterValue
    {
        get { return character_value; }
        set
        {
            character_value = Mathf.Clamp(value, 0, numberOfCharacters - 1);
            OnCharacterValueUpdate?.Invoke();
        }
    }

    private AIType ai_type = AIType.MACHINE_LEARNING;
    public AIType TypeOfAI
    {
        get { return ai_type; }
        set { ai_type = value; }
    }
    
    public enum AIType
    {
        MACHINE_LEARNING, FSM
    }

    // events
    public event Action OnCharacterValueUpdate;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            gameObject.SetActive(false);
        }
    }
}
