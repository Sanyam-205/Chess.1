using System;
using UnityEngine;

//using UnityEngine.PlayerLoop;
public enum PieceType
{
    Rook, Knight, Bishop, King, Queen, Pawn
} 
public enum TeamColor
{
    White, Black
}



[RequireComponent(typeof(SpriteRenderer))]
public class Piece : MonoBehaviour
{

    public bool hasMoved = false;
    public PieceType type;
    public TeamColor team;
    private SpriteRenderer spriteRenderer;
    
    public void Init(PieceType newType, TeamColor newTeam, Sprite newSprite)
    {   
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        type = newType;
        team = newTeam;

        spriteRenderer.sprite = newSprite;

        // if (team == TeamColor.White)
        // {
        //     spriteRenderer.color = Color.white;
        // }
        // else
        // {
        //     spriteRenderer.color = Color.black;
        // }

    }

    private void LateUpdate()
{
    // Force the global rotation to always be 0 (Upright),
    // even if the parent (Board) is spinning like crazy.
    transform.rotation = Quaternion.identity;
}

//   public void Init(PieceType newType, TeamColor newTeam, Sprite newSprite)
// {
//     Debug.Log($"🕵️ Init called on object: '{gameObject.name}'");

//     // 1. Try to get the renderer
//     spriteRenderer = GetComponent<SpriteRenderer>();



//     // If we survived the checks, run the logic
//     type = newType;
//     team = newTeam;
//     spriteRenderer.sprite = newSprite;
    
//     if (team == TeamColor.Black)
//         spriteRenderer.color = new Color(0.2f, 0.2f, 0.2f);
//     else
//         spriteRenderer.color = Color.white;
        
//     Debug.Log("✅ Init Success!");
// }


}
