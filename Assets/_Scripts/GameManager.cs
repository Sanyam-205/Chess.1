using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using UnityEditor;
using Unity.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GameObject gridPivot;
    
    [Header("rotation time")]
    [SerializeField]  float rotationTime = 1f;

    [SerializeField] GameObject pieceManager;
    [SerializeField] Movement movementSystem;
    private Square sourceSquare = null;

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
        pieceManager.GetComponent<PieceManager>().SpawnPieces(gridPivot.transform);
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
    // private void ProcessClick()
    // {
    //     Vector2 mousPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    //     RaycastHit2D hit = Physics2D.Raycast(mousPos, Vector2.zero);
    //     if (hit.collider != null)
    //     {
    //         Square clickedSquare = hit.collider.GetComponent<Square>();
    //         if (clickedSquare!= null)
    //         {
    //             SelectSquare(clickedSquare);
    //         }
            
    //     }
    //     if(hit.collider == null)
    //     {
    //        DeSelectSquare();
    //     }
    // }

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

        else if (currentState == GameState.BlackTurn)
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
    
    Vector2 GetGridPos()
    {
        
        Vector3 mouseCoordinates =  new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 10);
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint( mouseCoordinates );

        // Vector3Int mousePos = Vector3Int.FloorToInt(mousePosition);

        // Vector2Int mouseXYPos = new Vector2Int (mousePos.x, mousePos.y);

        Vector2 mouseXYPos = new Vector2(mousePosition.x, mousePosition.y);

        return mouseXYPos;

        
    }

  private void ProcessClick()
{
    // 1. RaycastAll to handle clicks that might hit a Piece collider first
    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    RaycastHit2D[] hits = Physics2D.RaycastAll(GetGridPos(), Vector2.zero);

    foreach (RaycastHit2D hit in hits)
    {
        Square clickedSquare = hit.collider.GetComponent<Square>();
        
        // We found a square! Let's handle the logic.
        if (clickedSquare != null)
        {
            // A. Check what is sitting on this square right now
            PieceManager pManager = pieceManager.GetComponent<PieceManager>();
            Piece pieceOnSquare = pManager.GetPieceAtGrid(clickedSquare.x, clickedSquare.y);

            // B. LOGIC BRANCHING
            
            // Case 1: Nothing is selected yet
            if (selectedSquare == null)
            {
                // We can only select if it's a piece AND it's OUR turn
                if (pieceOnSquare != null && IsMyTurn(pieceOnSquare))
                {
                    SelectSquare(clickedSquare);
                }
            }
            // Case 2: We already have a piece selected
            else
            {
                // 2a: Did we click the EXACT SAME square? -> Deselect (Toggle off)
                if (clickedSquare == selectedSquare)
                {
                    DeSelectSquare();
                }
                // 2b: Did we click ANOTHER of OUR OWN pieces? -> Switch Selection
                else if (pieceOnSquare != null && IsMyTurn(pieceOnSquare))
                {
                    DeSelectSquare();        // Unhighlight the old one
                    SelectSquare(clickedSquare); // Highlight the new one
                }
                // 2c: Clicked an Empty square or an Enemy -> Try to Move
                else
                {
                    
                    /// Call MovePiece and check if it returned TRUE (Success)
                    bool moveWasSuccessful = movementSystem.MovePiece(selectedSquare, clickedSquare);

                    if (moveWasSuccessful)
                    {
                        // Move worked! End the turn.
                        DeSelectSquare();
                        CompleteTurn(); // Rotates board + Switches state
                        Debug.Log(currentState);
                    }
                    else
                    {
                        // Move failed (e.g. Friendly Fire rule in Movement.cs prevented it)
                        // Just deselect, do NOT rotate board
                        DeSelectSquare(); 
                    }
                    //
                }
            }
            
            // We found our square, stop looping through the raycast hits
            return; 
        }
    }
    
    // If we missed the board entirely
    DeSelectSquare();
}

private bool IsMyTurn(Piece piece)
{
    if (piece == null) return false;
    
    // Check if the piece color matches the current GameState
    if (currentState == GameState.WhiteTurn && piece.team == TeamColor.White) return true;
    if (currentState == GameState.BlackTurn && piece.team == TeamColor.Black) return true;
    
    return false;
}




}
