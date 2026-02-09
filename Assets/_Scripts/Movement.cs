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
        
        finalSquare = endSquare;
        
        initialSquare = startSquare;
        
        if (pieceToMove == null) return false;

        Piece OccupiedPiece = pieceManager.GetPieceAtGrid(endSquare.x, endSquare.y);

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

    public void IsValidPatern()
    {
        deltaX = Math.Abs(initialSquare.x - finalSquare.x);
        deltaY = Math.Abs(initialSquare.y - finalSquare.y);
    }



}
