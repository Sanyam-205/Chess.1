using UnityEditor.U2D;
using UnityEngine;

public class Square : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor ;
    
    public int x,y;
    
    
    
    [SerializeField] Color highlightColor = Color.yellow;
    [SerializeField] Color highlightColor2 = Color.green;
    [SerializeField] Color highlightColor3;

    public void Init(int xPos, int yPos, Color colorFromGenerator)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        x = xPos;
        y = yPos;
        originalColor = colorFromGenerator;
    }
    // Inside Square.cs
    private bool isKingInCheck = false; // New flag

    public void SetCheckHighlight(bool active)
    {
        isKingInCheck = active;
        
        if (active)
        {
            // Make it distinct (e.g., Deep Red)
            spriteRenderer.color = Color.red; 
        }
        else
        {
            // Reset to normal board color
            ResetColor(); 
        }
    }

    // Update your existing ResetColor/HideHighlight method:
    public void ResetColor()
    {
        // CRITICAL: If this square is a King in Check, ignore the reset signal!
        if (isKingInCheck) return; 

        // Otherwise, go back to Black/White or original color
        spriteRenderer.color = originalColor; 
    }
    // void Start()
    // {
    //     spriteRenderer = GetComponent<SpriteRenderer>();
    //     originalColor = spriteRenderer.color;
    // }
    public void SetHighlight (bool highlighted)
    {
        spriteRenderer.color = highlighted? highlightColor : originalColor;
    }

     public void SetHighlight2 (bool highlighted)
    {
        spriteRenderer.color = highlighted? highlightColor2 : originalColor;
    }
    public void SetHighlight3(bool highlighted)
    {
        spriteRenderer.color = highlighted? highlightColor3 : originalColor;
    }
}
