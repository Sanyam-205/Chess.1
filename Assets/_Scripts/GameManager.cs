using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GameObject gridPivot;
    
    [Header("rotation time")]
    [SerializeField] private float rotationTime = 1f;

    private bool isFlipped = false, isBusy = false;
    public enum GameState
    {
        WhiteTurn,
        BlackTurn,
        Checkmate,
        Stalemate
    }
    public GameState currentState;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: keeps it alive between scenes
        }
        else
        {
            // If one already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //Game starts as white
        currentState = GameState.WhiteTurn;
    }
    void Update()
    {
        if(!canAcceptInput()) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CompleteTurn();
        }
    }
    public void CompleteTurn()
    {
        isFlipped = !isFlipped;
        float targetAngle = isFlipped?180f:0f;
       

        StartCoroutine(RotateBoard(targetAngle));
        SwitchTurn();        
        
    }

    private void SwitchTurn()
    {
        if (currentState == GameState.WhiteTurn)
        {
            currentState = GameState.BlackTurn;
        }

        if (currentState == GameState.BlackTurn)
        {
            currentState = GameState.WhiteTurn;
        }
    }

    private bool canAcceptInput ()
    {
        if(isBusy) return false;

        // check conditions go here

        return true;
    }

    private IEnumerator RotateBoard (float targetRotation)
    {
        isBusy = true;
        float duration = rotationTime;
        float elapsed = 0f;
        Quaternion startRot = gridPivot.transform.rotation;
        Quaternion endRot = Quaternion.Euler(0,0,targetRotation);

        while (elapsed < duration)
        {
            gridPivot.transform.rotation = Quaternion.Slerp (startRot, endRot, elapsed/duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        gridPivot.transform.rotation = endRot;
        
        isBusy = false;
    }
    



}
