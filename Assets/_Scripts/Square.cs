using UnityEditor.U2D;
using UnityEngine;

public class Square : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color originalColor ;
    public int x,y;
   
    
    [SerializeField] Color highlightColor = Color.yellow;

    public void Init(int xPos, int yPos, Color colorFromGenerator)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        x = xPos;
        y = yPos;
        originalColor = colorFromGenerator;
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

}
