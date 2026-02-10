using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using JetBrains.Annotations;
using System;

public class Movement : MonoBehaviour
{
    [SerializeField] private PieceManager pieceManager;
    int deltaX, deltaY;

    Square initialSquare, finalSquare;

    public bool MovePiece(Square startSquare, Square endSquare)
    {   
        Piece pieceToMove = pieceManager.GetPieceAtGrid (startSquare.x, startSquare.y);
        
        // finalSquare = endSquare;
        
        // initialSquare = startSquare;
        
        if (pieceToMove == null) return false;

        Piece OccupiedPiece = pieceManager.GetPieceAtGrid(endSquare.x, endSquare.y);

        if(!IsValidPatern(pieceToMove, startSquare, endSquare)) return false;

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
        
        
        // pieceToMove.transform.position = new Vector3(endSquare.x, endSquare.y, -1);
        // pieceManager.UpdateGrid(startSquare.x, startSquare.y, endSquare.x, endSquare.y, pieceToMove);
    
        return true;
    
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



}
