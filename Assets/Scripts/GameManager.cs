using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int numberOfCharacters = 2;

    public static GameManager Instance { get; private set; }

    // properties
    public int characterValue { get; private set; } = 1;

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
