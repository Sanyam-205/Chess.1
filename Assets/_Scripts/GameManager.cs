using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private GameObject gridPivot;
    
    //Unused Rotation variables
    // [Header("rotation time")]
    // [SerializeField]  float rotationTime = 1f;
    //private bool isFlipped = false, isBusy = false;

    [SerializeField] GameObject pieceManager, instructionsGO, startButtonGO, playAgainGO;
    [SerializeField] Movement movementSystem;

    [SerializeField] TextMeshProUGUI gameStateText;
    [SerializeField] Button startButton;
    string stateText = "";
    
    
    private Square selectedSquare;
    public Vector2Int whiteKingPos;
    public Vector2Int blackKingPos;
    public enum GameState
    {
        WhiteTurn,
        BlackTurn,
        Stalemate,
        WhiteWin,
        BlackWin,
        PendingPromotion,
        AwaitingStart
    }
    public GameState currentState;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
       if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (Instance = null)
        {
            Instance = this;  
        }
        
    }    

    
    void Start()
    {
        pieceManager.GetComponent<PieceManager>().SpawnPieces(gridPivot.transform);
        InitializeKingPositions();
        DisplayGameState();
        instructionsGO.SetActive(false);
        startButtonGO.SetActive(true);
        playAgainGO.SetActive(false);
        currentState = GameState.AwaitingStart;
    }
    void Update()
    {
        // Unused Rotation code
        // if (!canAcceptInput()) return;

        HandleInput();
        
    }

    public void OnStart()
    {
        currentState = GameState.WhiteTurn;
        startButtonGO.SetActive(false);
        instructionsGO.SetActive(true);
    }
    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void DisplayGameState()
    {
        RectTransform rect = playAgainGO.GetComponent<RectTransform>();
        
        switch(currentState)
        {
            case GameState.WhiteTurn:
                stateText = "White's Turn";
                break;

            case GameState.BlackTurn:
                stateText = "Black's Turn";
                break;
            case GameState.WhiteWin:
                playAgainGO.SetActive(true);
                rect.anchoredPosition = new Vector2 (485f, 2f);
                stateText = "White wins by checkmate!";
                break;
            case GameState.BlackWin:
                playAgainGO.SetActive(true);
                rect.anchoredPosition = new Vector2 (485f, -384f);
                stateText = "Black wins by checkmate!";
                break;
            case GameState.PendingPromotion:
                stateText = "Pending Promotion. \n Press 'q' to promote to Queen \n Press 'r' to promote to Rook \n Press 'b' to promote to Bishop \n Press 'k' to promote to Knight.";
                break;
             case GameState.Stalemate:
                playAgainGO.SetActive(true);
                rect.anchoredPosition = new Vector2 (-416f, -193f);
                
                stateText = "Draw!";
                break;
            default:
                stateText = "Unknown State";
                break;
        
        }
        
        gameStateText.text = stateText;
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ProcessClick();
        }
    }
   

    private  void DeSelectSquare ()
    {
         if (selectedSquare!=null)
            {
                movementSystem.ClearHighlights();
                selectedSquare.SetHighlight(false);
                selectedSquare = null;
            }
    }
        


    private void SelectSquare (Square newSquare)
    {
        if (selectedSquare != null) selectedSquare.SetHighlight(false);

        selectedSquare = newSquare;
        selectedSquare.SetHighlight(true);

       
    }

    public void SwitchTurn()
    {
        
        if(currentState == GameState.PendingPromotion) 
        {
            DisplayGameState();
            return;
        }
        
        if(movementSystem.IsInsufficientMaterial())
        {
            currentState = GameState.Stalemate;
            DisplayGameState();
            return;
        }

        
        if (currentState == GameState.WhiteTurn)
        {
            currentState = GameState.BlackTurn;
            DisplayGameState();
            if(movementSystem.CheckForGameEnd(TeamColor.Black))
            {
                DisplayGameState();
                return;
            }
        }
        
        else if (currentState == GameState.BlackTurn)
        {
            currentState = GameState.WhiteTurn;
            DisplayGameState();
            if(movementSystem.CheckForGameEnd(TeamColor.White))
            {
                DisplayGameState();
                return;
            }
        }
        
    }

    

/*
------------------------------------------------------------------------------------
    Removed Rotation feature. Switched CompleteTurn() with SwitchState()
------------------------------------------------------------------------------------    
    
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

    public void CompleteTurn()
    {
        isFlipped = !isFlipped;
        float targetAngle = isFlipped?180f:0f;
       

        StartCoroutine(RotateBoard(targetAngle));
        SwitchTurn();        
        
    }
    private bool canAcceptInput ()
    {
        if(isBusy) return false;


        return true;
    }

*/
    
    Vector2 GetGridPos()
    {
        
        Vector3 mouseCoordinates =  new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 10);
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint( mouseCoordinates );


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
                    movementSystem.GridScanner(clickedSquare);
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
                    SelectSquare(clickedSquare);
                    movementSystem.GridScanner(clickedSquare);
                    
                    
                     // Highlight the new one
                }
                // 2c: Clicked an Empty square or an Enemy -> Try to Move
                else
                {
                    Piece movingPiece = pManager.GetPieceAtGrid(selectedSquare.x, selectedSquare.y);
                    
                    /// Call MovePiece and check if it returned TRUE (Success)
                    bool moveWasSuccessful = movementSystem.MovePiece(selectedSquare, clickedSquare);

                    if (moveWasSuccessful)
                    {
                        // Track King Position
                        if (movingPiece.type == PieceType.King)
                        {
                            if (movingPiece.team == TeamColor.White) whiteKingPos = new Vector2Int(clickedSquare.x, clickedSquare.y);
                            else blackKingPos = new Vector2Int(clickedSquare.x, clickedSquare.y);
                        }

                        // Move worked! End the turn.
                        DeSelectSquare();
                        
                        //CompleteTurn(); // Rotates board + Switches state
                        // SwitchTurn();
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
    
    if (currentState == GameState.WhiteTurn && piece.team == TeamColor.White) return true;
    if (currentState == GameState.BlackTurn && piece.team == TeamColor.Black) return true;
    
    return false;
}

    private void InitializeKingPositions()
    {
        PieceManager pManager = pieceManager.GetComponent<PieceManager>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece p = pManager.GetPieceAtGrid(x, y);
                if (p != null && p.type == PieceType.King)
                {
                    if (p.team == TeamColor.White) whiteKingPos = new Vector2Int(x, y);
                    else blackKingPos = new Vector2Int(x, y);
                }
            }
        }
    }


}
