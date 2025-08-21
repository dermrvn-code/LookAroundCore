using System;
using System.Collections.Generic;
using UnityEngine;

public struct Piece
{
    public int id;
    public bool isCollected;
    public Texture2D texture;

    public Piece(int id, Texture2D texture)
    {
        this.id = id;
        this.texture = texture;
        isCollected = false;
    }
}

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

    List<Piece> pieces = new List<Piece>();

    public bool isActive = false;
    public bool isCompleted = false;

    public Action onCompleted;



    void Awake()
    {
        if (!puzzleDisplay) puzzleDisplay = FindFirstObjectByType<PuzzleDisplay>();
        puzzleDisplay.SetActive(false);
    }

    public void SetupPuzzle(Texture2D newMainTex, int pieceAmount, Action finished = null)
    {
        if (pieceAmount % 2 != 0)
        {
            Debug.Log("Pieces were odd, incrementing to " + (pieceAmount + 1));
            pieceAmount++;
        }

        if (finished != null)
        {
            onCompleted = finished;
        }

        mainTex = newMainTex;
        puzzleDisplay.image = mainTex;
        puzzleDisplay.LoadImage();

        GetBestGrid(mainTex.width, mainTex.height, pieceAmount);
        CreateMask();
        CreatePiecesFromMainTex();
    }

    public void StartPuzzle()
    {
        puzzleDisplay.SetActive(true);
        isActive = true;
        isCompleted = false;
    }

    public void ResetPuzzle()
    {
        isCompleted = false;
        isActive = false;
        puzzleDisplay.SetActive(false);

        for (int i = 0; i < pieces.Count; i++)
        {
            Piece piece = pieces[i];
            piece.isCollected = false;
            pieces[i] = piece;

            TogglePiece(i, true, false);
        }
    }

    public void RestartPuzzle()
    {
        ResetPuzzle();
        StartPuzzle();
    }

    public bool CheckCompleted()
    {
        foreach (Piece piece in pieces)
        {
            if (!piece.isCollected) return false;
        }
        isCompleted = true;
        return true;
    }

    public void GetBestGrid(int width, int height, int pieces)
    {
        float aspect = width / height;
        int bestCols = 1, bestRows = pieces;
        float bestDiff = float.MaxValue;

        for (int cols = 1; cols <= pieces; cols++)
        {
            if (pieces % cols != 0) continue; // ensure whole rows
            int rows = pieces / cols;

            float gridAspect = cols / rows;
            float diff = Mathf.Abs(aspect - gridAspect);

            if (diff < bestDiff)
            {
                bestDiff = diff;
                bestCols = cols;
                bestRows = rows;
            }
        }
        cols = bestCols;
        rows = bestRows;

        pieceWidth = width / cols;
        pieceHeight = height / rows;
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

                pieces.Add(new Piece(pieceCount, pieceTex));
                pieceCount++;
            }
        }
    }

    public bool CollectPiece(int index)
    {
        if (!CanCollect(index)) return false;

        Piece piece = pieces[index];
        piece.isCollected = true;
        pieces[index] = piece;

        TogglePiece(index);

        if (CheckCompleted())
        {
            onCompleted?.Invoke();
        }
        return true;
    }

    public bool CanCollect(int index)
    {
        if (!isActive || isCompleted) return false;

        if (index < 0 || index >= pieces.Count) return false;

        Piece piece = pieces[index];
        if (piece.isCollected) return false;

        return true;
    }

    public Piece GetPiece(int index)
    {
        if (index < 0 || index >= pieces.Count) return default;
        return pieces[index];
    }


    void TogglePiece(int index, bool overwrite = false, bool overwriteValue = false)
    {
        if (index < 0 || index >= pieces.Count) return;

        int r = index / cols;
        int c = index % cols;

        int startX = c * pieceWidth;
        int startY = (rows - 1 - r) * pieceHeight;
        Color current = maskTex.GetPixel(startX, startY);
        bool isVisible = current.r > 0.5f;

        Color newColor = isVisible ? Color.black : Color.white;

        if (overwrite)
        {
            newColor = overwriteValue ? Color.white : Color.black;
        }

        // Update mask for this piece
        for (int y = 0; y < pieceHeight; y++)
            for (int x = 0; x < pieceWidth; x++)
                maskTex.SetPixel(startX + x, startY + y, newColor);

        maskTex.Apply();
    }
}
