using UnityEngine;


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

        

    }

    /*
    --------------------------------------------------------------------
    Removed Rotation feature. Late update allowed pieces to stay upright
    --------------------------------------------------------------------

    private void LateUpdate()
    {
        // Force the global rotation to always be 0 (Upright),
        
        transform.rotation = Quaternion.identity;
    }
    */

}
