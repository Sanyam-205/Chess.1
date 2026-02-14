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
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private bool isKingInCheck = false; 
    private bool isSelected = false;

    public void SetCheckHighlight(bool active)
    {
        if (isKingInCheck == active) return;
        isKingInCheck = active;
        UpdateColor();
    }

    
    public void ResetColor()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (isSelected)
        {
            spriteRenderer.color = highlightColor;
        }
        else if (isKingInCheck) 
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = originalColor; 
        }
    }
    
    public void SetHighlight (bool highlighted)
    {
        isSelected = highlighted;
        UpdateColor();
    }

     public void SetHighlight2 (bool highlighted)
    {
        if (highlighted)
        {
            spriteRenderer.color = highlightColor2;
        }
        else
        {
            ResetColor();
        }
    }
    public void SetHighlight3(bool highlighted)
    {
        if (highlighted)
        {
            spriteRenderer.color = highlightColor3;
        }
        else
        {
            ResetColor();
        }
    }
}
