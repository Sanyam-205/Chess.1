using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;
using Unity.Collections;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private PieceManager pieceManager;
    [SerializeField] private Piece piece;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private GameManager gameManager;
    int deltaX, deltaY;
    public Piece promotedPiece;
    Vector2Int kingPos;

    public bool MovePiece(Square startSquare, Square endSquare)
    {   
        Piece pieceToMove = pieceManager.GetPieceAtGrid (startSquare.x, startSquare.y);

        
        
        // finalSquare = endSquare;
        
        // initialSquare = startSquare;
        
        if (pieceToMove == null) return false;

        Piece OccupiedPiece = pieceManager.GetPieceAtGrid(endSquare.x, endSquare.y);

        if(!CanPieceMove(pieceToMove, startSquare, endSquare)) return false;

        if(OccupiedPiece != null)
        {
            if(pieceToMove.team == OccupiedPiece.team)
            {
                return false; 
                
            }
            Piece enemyPiece = OccupiedPiece;
            Destroy(enemyPiece.gameObject);
        }
        Vector3 targetPos = endSquare.transform.position;

        pieceToMove.transform.position = new Vector3(targetPos.x, targetPos.y, -1);

        pieceManager.UpdateGrid(startSquare.x, startSquare.y, endSquare.x, endSquare.y, pieceToMove);
        
        
        ClearHighlights();
        
        
        
        // pieceToMove.transform.position = new Vector3(endSquare.x, endSquare.y, -1);
        // pieceManager.UpdateGrid(startSquare.x, startSquare.y, endSquare.x, endSquare.y, pieceToMove);
    
        if (CheckPromotionSquareReached(pieceToMove, endSquare))
        {
            // A. Save the Pawn so we can delete it LATER (after pressing Q/R/B/K)
            promotedPiece = pieceToMove; 

            // B. Pause the game loop
            gameManager.currentState = GameManager.GameState.PendingPromotion;
            gameManager.DisplayGameState();

            // C. Return true to finish the move, BUT DO NOT SwitchTurn() yet!
            return true; 
        }
       
        gameManager.SwitchTurn();
        return true;
    
    }

    public bool CheckPromotionSquareReached(Piece piece, Square endSquare)
    {
        if(piece.type == PieceType.Pawn)
        {
            if((piece.team == TeamColor.White && endSquare.y == 7)|| (piece.team == TeamColor.Black && endSquare.y == 0))
            {
    
                return true;
            }
        }
        return false;   
    }

    private void PromotePawn(PieceType newType)
    {
        int x = (int)promotedPiece.transform.position.x;
        int y = (int)promotedPiece.transform.position.y;
        TeamColor team = promotedPiece.team;
        Transform parent = promotedPiece.transform.parent;

        Destroy(promotedPiece.gameObject);

        pieceManager.SpawnSinglePiece(newType, team, x, y, parent);

        if (team == TeamColor.White)
        {
            gameManager.currentState = GameManager.GameState.WhiteTurn; 
        }
        else
        {
            gameManager.currentState = GameManager.GameState.BlackTurn;
        }
        promotedPiece = null;
        gameManager.SwitchTurn();
    }
    void Update()
    {
        UpdateKingState();
        
        if (gameManager.currentState == GameManager.GameState.PendingPromotion)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) PromotePawn(PieceType.Queen);
            if (Keyboard.current.rKey.wasPressedThisFrame) PromotePawn(PieceType.Rook);
            if (Keyboard.current.kKey.wasPressedThisFrame) PromotePawn(PieceType.Knight);
            if (Keyboard.current.cKey.wasPressedThisFrame) PromotePawn(PieceType.Bishop);
        }
    }

    public bool IsPathClear(Square start, Square end)
    {
        
        int stepX = Math.Sign(end.x - start.x);
        int stepY = Math.Sign(end.y - start.y);

        int currentX = start.x + stepX;
        int currentY = start.y + stepY;

        int safetyBuffer = 0;   
        while(currentX != end.x || currentY != end.y)
        {
            if(safetyBuffer++ > 8) break;
            if (pieceManager.GetPieceAtGrid(currentX, currentY)!= null) return false;
            currentX+=stepX;
            currentY+=stepY;
        }
        
        
        return true;
    }

    public bool IsValidPatern(Piece piece, Square start, Square end)
    {
        deltaX = Mathf.Abs(start.x - end.x);
        deltaY = Mathf.Abs(start.y - end.y);

        int pawnDirection = piece.team == TeamColor.White ? 1 : -1;
        int startRank = piece.team == TeamColor.White ? 1 : 6;
                
        switch(piece.type)
        {
            
            case PieceType.Pawn:
            if (
                
                (pieceManager.GetPieceAtGrid(end.x, end.y)==null && deltaX ==0 && end.y-start.y == pawnDirection)||
                ((startRank ==start.y) && 
                pieceManager.GetPieceAtGrid(end.x, end.y) == null && pieceManager.GetPieceAtGrid(end.x, end.y-pawnDirection) == null 
                && deltaX ==0 && end.y-start.y == pawnDirection*2)          
                
                )
            {
                return true;
            }
            if (deltaX == 1 && end.y - start.y == pawnDirection)
            {
                Piece targetPiece = pieceManager.GetPieceAtGrid(end.x, end.y);

                // Is there a piece there? AND is it on the other team?
                if (targetPiece != null && targetPiece.team != piece.team)
                {
                    return true; 
                }
            }
            
            
            break;
           
            case PieceType.Rook:
            if ((deltaX == 0 || deltaY ==0) && IsPathClear(start, end))
            {
                return true;
            }
            break;
            
        
            case PieceType.Bishop:
            if ((deltaX==deltaY) && IsPathClear(start, end))
            {
                return true;
            }
            break;

            case PieceType.Queen:
            if((deltaX==deltaY || deltaX == 0 || deltaY ==0) && IsPathClear(start, end))
            {
                return true;                
            }
            break;

            case PieceType.Knight:
            if(deltaX ==2 && deltaY == 1 || deltaY ==2 && deltaX ==1)
            {
                return true;
            }
            break;

            case PieceType.King:
            if(deltaX <= 1 && deltaY  <=1)
            {
                return true;
            }
            break;
            
        }
        

        
        return false;
    }
    
    
    // Returns TRUE if the move DOES NOT leave the King in check
    private bool IsMoveSafe(Piece piece, Square start, Square end)
    {
        // 1. Snapshot the current board state
        Piece targetPiece = pieceManager.GetPieceAtGrid(end.x, end.y); // The piece we might capture
        Piece movingPiece = pieceManager.GetPieceAtGrid(start.x, start.y); // The piece moving

        // 2. APPLY THE VIRTUAL MOVE
        // Move the piece in the data grid (not visually)
        pieceManager.SetPieceAtGrid(end.x, end.y, movingPiece);
        pieceManager.SetPieceAtGrid(start.x, start.y, null);

        // Track the King's position (in case the King itself is moving!)
        Vector2Int kingPos = (piece.team == TeamColor.White) ? gameManager.whiteKingPos : gameManager.blackKingPos;
        if (piece.type == PieceType.King)
        {
            kingPos = new Vector2Int(end.x, end.y);
        }
        
        // 3. CHECK FOR DANGER
        // Is the King under attack in this new reality?
        bool kingInCheck = IsSquareUnderAttack(kingPos, (piece.team == TeamColor.White) ? TeamColor.Black : TeamColor.White);

        // 4. UNDO THE MOVE (CRITICAL!)
        // Put everything back exactly how it was
        pieceManager.SetPieceAtGrid(start.x, start.y, movingPiece);
        pieceManager.SetPieceAtGrid(end.x, end.y, targetPiece);

        // 5. Result
        return !kingInCheck; // If king is in check return false
    }

    public bool CanPieceMove(Piece piece, Square start, Square end)
    {
        
        // Step 1: Geometry Check (Your existing big switch statement)
        if (!IsValidPatern(piece, start, end))
        {   
            return false;
        }

        // Step 2: Safety Check (The Simulation)
        if (!IsMoveSafe(piece, start, end))
        {
            
            return false;
            
        }

        // If we passed both checks, it's a legal move!
        
        return true;
    }


    public bool IsSquareUnderAttack(Vector2Int targetPos, TeamColor attackerColor)
    {

        GameObject targetObj = boardGenerator.board[targetPos.x, targetPos.y];
        Square targetSquare = targetObj.GetComponent<Square>();

        for (int x = 0; x<8; x++)
        {
            for (int y = 0; y<8; y++)
            {
                Piece attacker = pieceManager.pieceGrid[x,y];

                if(attacker!=null && attacker.team == attackerColor)
                {
                    GameObject startObj = boardGenerator.board[x,y];
                    Square startSquare = startObj.GetComponent<Square>();
                    if(IsValidPatern(attacker, startSquare, targetSquare))
                    {
                        return true;
                    }
                }

            }
        }
        
        
        return false;
    }

    public bool CheckForGameEnd(TeamColor team)
    {
        Vector2Int kingPos = (team == TeamColor.White) ? gameManager.whiteKingPos : gameManager.blackKingPos;
        
        bool isInCheck = IsSquareUnderAttack(kingPos, (team == TeamColor.White) ? TeamColor.Black : TeamColor.White);

        if(isInCheck)
        {
            if(!HasAnyLegalMoves(team))
            {
                // Debug.Log("Checkmate" +  ((team == TeamColor.White)? "Black" : "White" ) + " wins") ;
                // gameManager.currentState = GameManager.GameState.Checkmate;
                if(team == TeamColor.White)
                {
                    Debug.Log("Black has won by checkmate!");
                    gameManager.currentState = GameManager.GameState.BlackWin;
                }
                else
                {
                    Debug.Log("White has won by checkmate!");
                    gameManager.currentState = GameManager.GameState.WhiteWin;
                }
            }
            // else
            // {
            //     Debug.Log ("Check");
            //     return false;
            // }
        }
        else
        {
            if(!HasAnyLegalMoves(team))
            {
                Debug.Log("Draw");
                gameManager.currentState = GameManager.GameState.Stalemate;
            }
        }
        return true;

    }

    private bool HasAnyLegalMoves(TeamColor team)
    {
    
        for (int startX = 0; startX<8; startX++)
        {
            for (int startY = 0; startY<8; startY++)
            {
                Piece p = pieceManager.GetPieceAtGrid(startX, startY);

                if(p!= null && p.team == team)
                {
                    for (int endX = 0; endX <8; endX++)
                    {
                        for (int endY = 0; endY<8; endY++)
                        {
                            if(startX == endX && startY == endY) continue;
                            
                            Square startSq = boardGenerator.board[startX, startY].GetComponent<Square>();
                            Square endSq = boardGenerator.board[endX, endY].GetComponent<Square>();

                            if(IsValidPatern(p, startSq, endSq))
                            {
                                Piece targetPiece = pieceManager.GetPieceAtGrid(endX, endY);

                                if(targetPiece!=null && targetPiece.team == team) continue;


                                if (IsMoveSafe(p, startSq, endSq))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
        }
    
    
    
    
        return false;
    }

    public bool IsInsufficientMaterial()
    {
        int whitePieceCount = 0;
        int blackPieceCount = 0;
        
        
        for (int x= 0; x<8; x++)
        {
            for (int y = 0; y<8; y++)
            {
                Piece p = pieceManager.GetPieceAtGrid(x,y);
                              
                if (p == null) continue;
                
                if (p!= null)
                {
                    if(p.type == PieceType.Pawn || p.type == PieceType.Rook || p.type == PieceType.Queen) return false;
                }

                if(p.team == TeamColor.White)
                {
                    whitePieceCount++;
                }
                else
                {
                    blackPieceCount++;
                }

            }
        }

        return (whitePieceCount <=2 && blackPieceCount <=2);
        
    }

    public void GridScanner(Square selectedSquare)
    {
        Piece selectedPiece = pieceManager.GetPieceAtGrid(selectedSquare.x, selectedSquare.y);
        if (selectedPiece == null) return;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject tileObject = boardGenerator.board[i, j];

                if (tileObject != null)
                {
                    Square targetSquare = tileObject.GetComponent<Square>();
                    bool isValid = IsValidPatern(selectedPiece, selectedSquare, targetSquare);

                    // 1. Get the piece at the target (if any)
                    Piece targetPiece = pieceManager.GetPieceAtGrid(targetSquare.x, targetSquare.y);

                    // 2. Filter out Friendly Fire
                    if (isValid)
                    {
                        if (targetPiece != null && targetPiece.team == selectedPiece.team)
                        {
                            isValid = false;
                        }
                    }

                    // 3. Final Checks and Highlight
                    if (isValid)
                    {
                        // Check if the move is safe for the King
                        if (IsMoveSafe(selectedPiece, selectedSquare, targetSquare))
                        {
                            if (targetPiece != null)
                                {
                                    // Case A: Valid move + Piece exists = MUST be Enemy (Attack)
                                    targetSquare.SetHighlight3(true); 
                                }
                                else
                                {
                                    // Case B: Valid move + No piece = Empty Square (Movement)
                                    targetSquare.SetHighlight2(true); 
                                }
                        }
                    }
                }
            }
        }
    }

    // Inside GameManager.cs


    

    public void UpdateKingState()
    {
        // 1. Get the Kings' current Squares
        // (Assuming you have access to the Square scripts via your board array)
        Square whiteKingSquare = boardGenerator.board[gameManager.whiteKingPos.x, gameManager.whiteKingPos.y].GetComponent<Square>();
        Square blackKingSquare = boardGenerator.board[gameManager.blackKingPos.x, gameManager.blackKingPos.y].GetComponent<Square>();

        // 2. Reset them to normal (remove old red highlights)
        whiteKingSquare.ResetColor(); 
        blackKingSquare.ResetColor();

        // 3. Check White King's Safety
        // We reuse the exact same logic IsMoveSafe uses!
        if (IsSquareUnderAttack(gameManager.whiteKingPos, TeamColor.Black))
        {
            // King is in check! Turn him RED.
            whiteKingSquare.SetCheckHighlight(true); 
        }

        // 4. Check Black King's Safety
        if (IsSquareUnderAttack(gameManager.blackKingPos, TeamColor.White))
        {
            blackKingSquare.SetCheckHighlight(true);
        }
    }



    public void ClearHighlights()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                GameObject tileObject = boardGenerator.board[i, j];
                if (tileObject != null)
                {
                    Square square = tileObject.GetComponent<Square>();
                    square.SetHighlight(false);
                }
            }
        }
    }

}
