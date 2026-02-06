using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GameObject gridPivot;
    
    [Header("rotation time")]
    [SerializeField] private float rotationTime = 1f;

    [SerializeField] GameObject pieceManager;
    private bool isFlipped = false, isBusy = false;
    private Square selectedSquare;
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
       //pieceManager script =pieceManager.GetComponent<PieceManager>(); 
    }

    void Start()
    {
        pieceManager.GetComponent<PieceManager>().SpawnPieces();
        //Game starts as white
        currentState = GameState.WhiteTurn;
    }
    void Update()
    {
        if (!canAcceptInput()) return;


        HandleInput();
        
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ProcessClick();
        }
    }
    private void ProcessClick()
    {
        Vector2 mousPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousPos, Vector2.zero);
        if (hit.collider != null)
        {
            Square clickedSquare = hit.collider.GetComponent<Square>();
            if (clickedSquare!= null)
            {
                SelectSquare(clickedSquare);
            }
            
        }
        if(hit.collider == null)
        {
           DeSelectSquare();
        }
    }

    private  void DeSelectSquare ()
    {
         if (selectedSquare!=null)
            {
                selectedSquare.SetHighlight(false);
                selectedSquare = null;
            // Square DeSquare = hit.collider.GetComponent<Square>();
            // DeSelectSquare(DeSquare);
            }
    }
        


    private void SelectSquare (Square newSquare)
    {
        if (selectedSquare != null) selectedSquare.SetHighlight(false);

    // Select new one
        selectedSquare = newSquare;
        selectedSquare.SetHighlight(true);

       // CompleteTurn();
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
