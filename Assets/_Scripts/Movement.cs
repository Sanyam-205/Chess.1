using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;
using Unity.Collections;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Net;

public class Movement : MonoBehaviour
{
    [SerializeField] private PieceManager pieceManager;
    [SerializeField] private Piece piece;
    [SerializeField] private BoardGenerator boardGenerator;
    [SerializeField] private GameManager gameManager;
    int deltaX, deltaY;
    public Piece promotedPiece, enPasantVulnerablePawn;
    Square enPasantStartSquare, enPasantEndSquare;
    
    Vector2Int kingPos;
    public bool MovePiece(Square startSquare, Square endSquare)
    {   
        Piece pieceToMove = pieceManager.GetPieceAtGrid (startSquare.x, startSquare.y);
        if (pieceToMove == null) return false;

        Piece OccupiedPiece = pieceManager.GetPieceAtGrid(endSquare.x, endSquare.y);

        

        // ---------------------------------------------------------
        // 1. VALIDATION AND SECONDARY MOVEMENT (The Split Path)
        // ---------------------------------------------------------
        if ((pieceToMove.type == PieceType.King) && Math.Abs(endSquare.x - startSquare.x) == 2)
        {
            // CASTLING PATH
            if (!CanCastle (pieceToMove, startSquare, endSquare)) return false;

            int direction = (endSquare.x - startSquare.x > 0) ? 1 : -1;
            int rookOrgX = (direction == 1) ? 7 : 0;
            int rookTargetX = (direction == 1) ? 5 : 3;

            Piece rook = pieceManager.GetPieceAtGrid(rookOrgX, startSquare.y);

            if (rook != null)
            {
                Square rookTargetSq = boardGenerator.board[rookTargetX, startSquare.y].GetComponent<Square>();
                
                // Move Rook Physically
                rook.transform.position = new Vector3(rookTargetSq.transform.position.x, rookTargetSq.transform.position.y, -1);
                
                // Move Rook Data
                pieceManager.UpdateGrid(rookOrgX, startSquare.y, rookTargetX, startSquare.y, rook);
                rook.hasMoved = true;
            }
        }
        else
        
        {
            // STANDARD MOVE PATH
            if (!CanPieceMove(pieceToMove, startSquare, endSquare)) return false;

            if ((pieceToMove.type == PieceType.Pawn)&& (startSquare.x != endSquare.x))
            {
                if (enPasantVulnerablePawn!= null)
                {
                    
                    
                    if (enPasantStartSquare.x == endSquare.x &&
                        enPasantEndSquare.y == startSquare.y)
                    {
                        Destroy(enPasantVulnerablePawn.gameObject);
                    }
                }
            }
            
            if (OccupiedPiece != null)
            {
                if (pieceToMove.team == OccupiedPiece.team) return false; 
                
                Piece enemyPiece = OccupiedPiece;
                Destroy(enemyPiece.gameObject);
            }
        }

        

        // ---------------------------------------------------------
        // 2. PRIMARY MOVEMENT (The Common Path)
        // Both Castling AND Standard moves need this to happen!
        // ---------------------------------------------------------
        Vector3 targetPos = endSquare.transform.position;
        pieceToMove.transform.position = new Vector3(targetPos.x, targetPos.y, -1);
        pieceManager.UpdateGrid(startSquare.x, startSquare.y, endSquare.x, endSquare.y, pieceToMove);
        
        // CRITICAL: Ensure the piece knows it has moved!
        pieceToMove.hasMoved = true; 
        
        ClearHighlights();
        
        enPasantVulnerablePawn = null;
        if((pieceToMove.type == PieceType.Pawn) && Math.Abs(endSquare.y - startSquare.y) == 2)
        {
            enPasantVulnerablePawn = pieceToMove;
            enPasantStartSquare = startSquare;
            enPasantEndSquare = endSquare;
        }

        // ---------------------------------------------------------
        // 3. PROMOTION AND TURN SWITCHING
        // ---------------------------------------------------------
        if (CheckPromotionSquareReached(pieceToMove, endSquare))
        {
            promotedPiece = pieceToMove; 
            gameManager.currentState = GameManager.GameState.PendingPromotion;
            gameManager.DisplayGameState();
            return true; 
        }

        gameManager.SwitchTurn();
        return true;
    }
    public bool CanCastle (Piece king, Square kingSquare, Square targetSquare)
    {
        if(king.hasMoved) return false;
        //if(targetSquare != null) return false;
        int direction = (targetSquare.x - kingSquare.x > 0)? 1: -1;
        int rookX = (direction == 1)? 7: 0;
        int rookY = kingSquare.y;

        Piece rook = pieceManager.GetPieceAtGrid(rookX, rookY);
        if (rook == null || rook.type != PieceType.Rook || rook.team != king.team || rook.hasMoved == true) return false;

        for (int i = 1; i < Math.Abs(targetSquare.x - kingSquare.x); i++)
        {
            if (pieceManager.GetPieceAtGrid((kingSquare.x + (1*direction)), rookY) != null) return false;
        }

        // Is king currently in Check?
        if(IsSquareUnderAttack(new Vector2Int (kingSquare.x, kingSquare.y), (king.team == TeamColor.White) ? TeamColor.Black : TeamColor.White)) return false;

        Vector2Int middleSquare = new Vector2Int(kingSquare.x + direction, kingSquare.y);
        Vector2Int destSquare = new Vector2Int(targetSquare.x, targetSquare.y);
        

        TeamColor enemyTeam = (king.team == TeamColor.White) ? TeamColor.Black : TeamColor.White;
        //Check if castling makes king move through check
        if (IsSquareUnderAttack(middleSquare, enemyTeam)) return false; // Crossing check
        if (IsSquareUnderAttack(destSquare, enemyTeam)) return false;

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
            if (Keyboard.current.bKey.wasPressedThisFrame) PromotePawn(PieceType.Bishop);
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
                if (targetPiece == null && enPasantVulnerablePawn != null)
            {
                // The vulnerable pawn must be "Next to" our start position
                // Logic: It should be at [end.x] (the file we are moving to)
                // and [start.y] (the rank we are currently on).
                
                Piece vulnerablePawn = enPasantVulnerablePawn;
                
                // Check coordinates (assuming your Piece script has x/y or transforms)
                int vPawnX = (int)vulnerablePawn.transform.position.x;
                int vPawnY = (int)vulnerablePawn.transform.position.y;

                if (vPawnX == end.x && vPawnY == start.y)
                {
                    // Also ensure it's an enemy!
                    if (vulnerablePawn.team != piece.team)
                    {
                        return true;
                    }
                }
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
            // if(piece.hasMoved == false)
            // {
            //     return true;   
            // }
            break;
            
        }
        

        
        return false;
    }
    
    
    // Returns TRUE if the move DOES NOT leave the King in check

    private bool IsMoveSafe(Piece piece, Square start, Square end)
    {
        // 1. SNAPSHOT CURRENT STATE
        Piece targetPiece = pieceManager.GetPieceAtGrid(end.x, end.y);
        Piece movingPiece = pieceManager.GetPieceAtGrid(start.x, start.y);

        bool isEnPassant = false;
        Piece enPassantVictim = null;

        // Check: Is it a Pawn moving Diagonally to an Empty Square?
        if (piece.type == PieceType.Pawn && start.x != end.x && targetPiece == null)
        {
            isEnPassant = true;
            enPassantVictim = enPasantVulnerablePawn; 
        }

        // 2. APPLY THE VIRTUAL MOVE
        pieceManager.SetPieceAtGrid(end.x, end.y, movingPiece);
        pieceManager.SetPieceAtGrid(start.x, start.y, null);

        // --- EN PASSANT SPECIAL STEP ---
        // The victim is located at [end.x, start.y]
        if (isEnPassant && enPassantVictim != null)
        {
            // Hide the victim using derived coordinates
            pieceManager.SetPieceAtGrid(end.x, start.y, null);
        }
        // -------------------------------

        // Track the King's position
        Vector2Int kingPos = (piece.team == TeamColor.White) ? gameManager.whiteKingPos : gameManager.blackKingPos;
        if (piece.type == PieceType.King)
        {
            kingPos = new Vector2Int(end.x, end.y);
        }
        
        // 3. CHECK FOR DANGER
        bool kingInCheck = IsSquareUnderAttack(kingPos, (piece.team == TeamColor.White) ? TeamColor.Black : TeamColor.White);

        // 4. UNDO THE MOVE (Restore everything)
        pieceManager.SetPieceAtGrid(start.x, start.y, movingPiece);
        pieceManager.SetPieceAtGrid(end.x, end.y, targetPiece);

        // --- EN PASSANT RESTORE STEP ---
        // Put the victim back at [end.x, start.y]
        if (isEnPassant && enPassantVictim != null)
        {
            pieceManager.SetPieceAtGrid(end.x, start.y, enPassantVictim);
        }
        // -------------------------------

        return !kingInCheck;
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

                    // Special check for Castling (since we removed it from IsValidPatern to avoid recursion)
                    if (!isValid && selectedPiece.type == PieceType.King && Math.Abs(targetSquare.x - selectedSquare.x) == 2 && targetSquare.y == selectedSquare.y)
                    {
                        if (CanCastle(selectedPiece, selectedSquare, targetSquare))
                        {
                            isValid = true;
                        }
                    }

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
        // 1. Clear all check highlights to prevent trails
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (boardGenerator.board[x, y] != null)
                {
                    boardGenerator.board[x, y].GetComponent<Square>().SetCheckHighlight(false);
                }
            }
        }

        // 2. Get the Kings' current Squares
        Square whiteKingSquare = boardGenerator.board[gameManager.whiteKingPos.x, gameManager.whiteKingPos.y].GetComponent<Square>();
        Square blackKingSquare = boardGenerator.board[gameManager.blackKingPos.x, gameManager.blackKingPos.y].GetComponent<Square>();

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
