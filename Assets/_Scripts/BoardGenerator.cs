using System;
using UnityEngine;


public class BoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Color tileColor1, tileColor2;
    [Header("Camera Settings")]
    [SerializeField] private float camaraX = 3.5f, cameraY = 3.5f, cameraSize = 5f;
    

    public GameObject[,] board = new GameObject[8, 8];

   
    public Color[,] boardColorMap = new Color[8,8];

    void Start()
    {
        GenerateBoard();
    }
    void Awake() 
{
    // 1. If this array is static, it MUST be cleared manually!
    if (board != null)
    {
        Array.Clear(board, 0, board.Length); 
    }
    
    // 2. Re-initialize the array size (Crucial!)
    board = new GameObject[8, 8]; 
}

    void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                bool isOffset = (x + y) % 2 == 0;

                GameObject newTile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);

                Square script = newTile.GetComponent<Square>();

                newTile.transform.parent = transform;
                newTile.name = $"Tile [{x},{y}]";

                Color assignedColor = isOffset? tileColor1 : tileColor2;
                
                script.Init(x,y,assignedColor);
                


                

                newTile.GetComponent<SpriteRenderer>().color = isOffset ? tileColor1 : tileColor2;


                
                board[x, y] = newTile;


            }
        }

        CameraSettings();
    }

    private void CameraSettings()
    {
        Camera.main.transform.position = new Vector3(camaraX, cameraY, -10);
        Camera.main.orthographicSize = cameraSize;
    }

    
}
