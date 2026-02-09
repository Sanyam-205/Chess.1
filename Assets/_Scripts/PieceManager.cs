using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
public class PieceManager : MonoBehaviour
{
    [Header("Elements")]
    public GameObject piecePrefab;
    public Sprite[] whiteSprites, blackSprites;

    public Piece[,] pieceGrid = new Piece[8,8];

    public Piece GetPieceAtGrid(int x, int y)
    {
        if (x < 0 || x >= 8 || y < 0 || y >= 8) return null;
        return pieceGrid[x,y];
    }

    public void UpdateGrid(int oldX, int oldY, int newX, int newY, Piece piece)
    {
        pieceGrid[oldX, oldY] = null; 
        pieceGrid[newX, newY] = piece; 
    }

    public void SpawnPieces(Transform gridParent)
    {
        for(int i = 0; i<8; i++)
        {
             //pawns
            SpawnSinglePiece(PieceType.Pawn, TeamColor.White, i, 1, gridParent);
            SpawnSinglePiece(PieceType.Pawn, TeamColor.Black, i, 6, gridParent);
        }
        SpawnBackRank(TeamColor.White, 0, gridParent);
        SpawnBackRank(TeamColor.Black, 7, gridParent);
    }

    private void SpawnBackRank(TeamColor team, int y, Transform gridParent)
    {
        PieceType[] pattern = {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        for (int i = 0; i<8; i++)
        {
            SpawnSinglePiece(pattern[i], team, i, y, gridParent);   
        }
    }

    private void SpawnSinglePiece(PieceType type, TeamColor team, int x, int y, Transform gridParent)
    {
        Vector3 spawnPos = new Vector3(x,y,-1);
        GameObject newPieceGO = Instantiate(piecePrefab, spawnPos, Quaternion.identity);


        newPieceGO.transform.SetParent(gridParent);


        Piece newPiece = newPieceGO.GetComponent<Piece>();
        
        Sprite[] targetSprites = (team == TeamColor.White) ? whiteSprites : blackSprites;

        newPiece.Init(type, team, targetSprites[(int)type] );
        newPieceGO.name = $"{team}_{type}";
        pieceGrid[x,y] = newPiece;
    }

}
