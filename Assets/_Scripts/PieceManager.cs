using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;
public class PieceManager : MonoBehaviour
{
    [Header("Elements")]
    public GameObject piecePrefab;
    public Sprite[] whiteSprites, blackSprites;

    private Piece[,] pieceGrid = new Piece[8,8];


    public void SpawnPieces()
    {
        for(int i = 0; i<8; i++)
        {
             //pawns
            SpawnSinglePiece(PieceType.Pawn, TeamColor.White, i, 1);
            SpawnSinglePiece(PieceType.Pawn, TeamColor.Black, i, 6);
        }
        SpawnBackRank(TeamColor.White, 0);
        SpawnBackRank(TeamColor.Black, 7);
    }

    private void SpawnBackRank(TeamColor team, int y)
    {
        PieceType[] pattern = {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen,
            PieceType.King, PieceType.Bishop, PieceType.Knight, PieceType.Rook
        };

        for (int i = 0; i<8; i++)
        {
            SpawnSinglePiece(pattern[i], team, i, y);   
        }
    }

    private void SpawnSinglePiece(PieceType type, TeamColor team, int x, int y)
    {
        Vector3 spawnPos = new Vector3(x,y,-1);
        GameObject newPieceGO = Instantiate(piecePrefab, spawnPos, Quaternion.identity);
        Piece newPiece = newPieceGO.GetComponent<Piece>();
        
        Sprite[] targetSprites = (team == TeamColor.White) ? whiteSprites : blackSprites;

        newPiece.Init(type, team, targetSprites[(int)type] );
        newPieceGO.name = $"{team}_{type}";
        pieceGrid[x,y] = newPiece;
    }

}
