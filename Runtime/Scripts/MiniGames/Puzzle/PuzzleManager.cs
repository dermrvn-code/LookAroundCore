using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public Texture2D mainTex;
    public int totalPieces = 4;

    PuzzleDisplay puzzleDisplay;
    private Texture2D maskTex;
    private int rows;
    private int cols;
    private int pieceWidth;
    private int pieceHeight;

    public List<Texture2D> pieces = new List<Texture2D>();

    void Awake()
    {
        if (!puzzleDisplay) puzzleDisplay = FindFirstObjectByType<PuzzleDisplay>();
        puzzleDisplay.SetActive(false);
    }

    public void SetupPuzzle(Texture2D newMainTex)
    {
        // Make a readable copy of the main texture
        mainTex = MakeTextureReadable(newMainTex);

        puzzleDisplay.image = mainTex;

        SetupGrid();
        CreateMask();
        CreatePiecesFromMainTex();

        // Example: reveal the first piece
        TogglePiece(0);

        puzzleDisplay.SetActive(true);
    }

    Texture2D MakeTextureReadable(Texture2D source)
    {
        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTex.filterMode = source.filterMode;
        readableTex.wrapMode = source.wrapMode;
        Graphics.CopyTexture(source, readableTex);
        return readableTex;
    }

    void SetupGrid()
    {
        cols = Mathf.CeilToInt(Mathf.Sqrt(totalPieces));
        rows = Mathf.CeilToInt((float)totalPieces / cols);
        pieceWidth = mainTex.width / cols;
        pieceHeight = mainTex.height / rows;
    }

    void CreateMask()
    {
        maskTex = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGB24, false);
        maskTex.filterMode = FilterMode.Point;
        maskTex.wrapMode = TextureWrapMode.Clamp;

        FillMask(Color.black);
        maskTex.Apply();

        puzzleDisplay.puzzleMaterial.SetTexture("_MaskTex", maskTex);
    }

    void FillMask(Color color)
    {
        for (int y = 0; y < maskTex.height; y++)
            for (int x = 0; x < maskTex.width; x++)
                maskTex.SetPixel(x, y, color);
    }

    void CreatePiecesFromMainTex()
    {
        pieces.Clear();
        int pieceCount = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (pieceCount >= totalPieces) break;

                Texture2D pieceTex = new Texture2D(pieceWidth, pieceHeight, mainTex.format, false);
                pieceTex.filterMode = mainTex.filterMode;
                pieceTex.wrapMode = TextureWrapMode.Clamp;

                // Copy pixels from readable mainTex
                int startX = c * pieceWidth;
                int startY = mainTex.height - (r + 1) * pieceHeight; // flip y-axis
                Color[] pixels = mainTex.GetPixels(startX, startY, pieceWidth, pieceHeight);
                pieceTex.SetPixels(pixels);
                pieceTex.Apply();

                pieces.Add(pieceTex);
                pieceCount++;
            }
        }
    }

    public void TogglePiece(int index)
    {
        if (index < 0 || index >= pieces.Count) return;

        int r = index / cols;
        int c = index % cols;

        int startX = c * pieceWidth;
        int startY = (rows - 1 - r) * pieceHeight;
        Color current = maskTex.GetPixel(startX, startY);
        bool isVisible = current.r > 0.5f;

        Color newColor = isVisible ? Color.black : Color.white;

        // Update mask for this piece
        for (int y = 0; y < pieceHeight; y++)
            for (int x = 0; x < pieceWidth; x++)
                maskTex.SetPixel(startX + x, startY + y, newColor);

        maskTex.Apply();
    }
}
