using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGridRenderer : Graphic
{
    [Header("Grid properties")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(1, 1);
    public float thickness = 1.5f;
    [Range(0, 10)] public int horizontalDashes = 2;
    [Range(0, 10)] public int verticalDashes = 2;
    [SerializeField] private bool equalDashes = false;
    

    enum CellType { Solid, Dashed }
    [Header("Cell properties")]
    [SerializeField] private CellType cellType = CellType.Solid;

    private delegate void Function(int x, int y, int index, VertexHelper vh);

    private Function[] functions;
    private Function drawFunction;

    [Space]
    [SerializeField] private bool debug = false;

    public Vector2Int GridSize
    {
        set
        {
            gridSize = value;
            UpdateGeometry();
        }
    }

    private float width;
    private float height;
    private float cellWidth;
    private float cellHeight;

    private float distance;
    private float horizontalDashWidth;
    private float verticalDashWidth;

    private Function GetFunction() => functions[(int) cellType];
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        functions = new Function[] { DrawCell, DrawDashedCell };

        drawFunction = GetFunction();
        
        vh.Clear();
        
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        cellWidth = width / gridSize.x;
        cellHeight = height / gridSize.y;

        int count = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                drawFunction(x, y, count, vh);
                count++;
            }
        }
    }

    private void DrawCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x;
        float yPos = cellHeight * y;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
        distance = thickness / Mathf.Sqrt(2f);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddCellTriangles(ref vh, index);
    }
    
    private void DrawDashedCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x;
        float yPos = cellHeight * y;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
        distance = thickness / Mathf.Sqrt(2f);
        
        horizontalDashWidth = cellWidth / (horizontalDashes * 2 + 2);

        if (equalDashes)
        {
            verticalDashWidth = horizontalDashWidth;
            verticalDashes = Mathf.CeilToInt((cellHeight / verticalDashWidth - 1) / 2);
            verticalDashWidth = cellHeight / (verticalDashes * 2 + 2);
        }
        else
        {
            verticalDashWidth = cellHeight / (verticalDashes * 2 + 2);
        }

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddDashesVertices(ref vertex, ref vh, xPos, yPos);

        AddCornerCellTriangles(ref vh, index);
        AddDashesTriangles(ref vh, index);
    }

    private void AddCornerVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos);
        vh.AddVert(vertex);
    }

    private void AddInternalCornerVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        
        vertex.position = new Vector3(xPos + distance, yPos + distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + distance);
        vh.AddVert(vertex);
    }
    
    private void AddMiddleVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + cellHeight);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos);
        vh.AddVert(vertex);
    }
    
    private void AddInternalMiddleVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos + distance, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + cellHeight - distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + distance);
        vh.AddVert(vertex);
    }
    
    private void AddDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        for (int i = 0; i < horizontalDashes; i++)
        {
            if (debug) vertex.color = new Color32(0, 0, 255, 255);
            AddHorizontalDashesVertices(ref vertex, ref vh, xPos, yPos, horizontalDashWidth, i);
        }
        
        for (int i = 0; i < verticalDashes; i++)
        {
            if (debug) vertex.color = new Color32( 255, 0, 0, 255);
            AddVerticalDashesVertices(ref vertex, ref vh, xPos, yPos, verticalDashWidth, i);
        }
    }
    
    private void AddHorizontalDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
        float dashWidth, float dashIndex)
    {
        // Bottom dashes vertices
        float xDashPos = xPos + horizontalDashWidth / 2 + horizontalDashWidth + horizontalDashWidth * 2 * dashIndex;
        float yDashPos = yPos;
        
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos);
        vh.AddVert(vertex);
        
        // Top dashes vertices
        yDashPos = yPos + cellHeight - distance;
        
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos);
        vh.AddVert(vertex);
    }

    private void AddVerticalDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
        float dashWidth, float dashIndex)
    {
        // Left dashes vertices
        float xDashPos = xPos;
        float yDashPos = yPos + verticalDashWidth / 2 + verticalDashWidth + verticalDashWidth * 2 * dashIndex;
        
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos);
        vh.AddVert(vertex);
        
        // Right dashes vertices
        xDashPos = xPos + cellWidth - distance;
        
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos);
        vh.AddVert(vertex);
    }

    private void AddCornerCellTriangles(ref VertexHelper vh, int index)
    {
        int cellOffset = index * (24 + 4 * horizontalDashes * 2 + 4 * verticalDashes * 2);
        
        // Left bottom Corner
        vh.AddTriangle(cellOffset + 15, cellOffset + 4, cellOffset + 23);
        vh.AddTriangle(cellOffset + 0, cellOffset + 4, cellOffset + 15);
        vh.AddTriangle(cellOffset + 0, cellOffset + 8, cellOffset + 4);
        vh.AddTriangle(cellOffset + 8, cellOffset + 16, cellOffset + 4);
        
        // Left top Corner
        vh.AddTriangle(cellOffset + 9, cellOffset + 5, cellOffset + 17);
        vh.AddTriangle(cellOffset + 9, cellOffset + 1, cellOffset + 5);
        vh.AddTriangle(cellOffset + 1, cellOffset + 10, cellOffset + 5);
        vh.AddTriangle(cellOffset + 5, cellOffset + 10, cellOffset + 18);
        
        // Right top Corner
        vh.AddTriangle(cellOffset + 19, cellOffset + 11, cellOffset + 6);
        vh.AddTriangle(cellOffset + 11, cellOffset + 2, cellOffset + 6);
        vh.AddTriangle(cellOffset + 2, cellOffset + 12, cellOffset + 6);
        vh.AddTriangle(cellOffset + 20, cellOffset + 6, cellOffset + 12);
        
        // Right bottom Corner
        vh.AddTriangle(cellOffset + 21, cellOffset + 13, cellOffset + 7);
        vh.AddTriangle(cellOffset + 13, cellOffset + 3, cellOffset + 7);
        vh.AddTriangle(cellOffset + 7, cellOffset + 3, cellOffset + 14);
        vh.AddTriangle(cellOffset + 14, cellOffset + 22, cellOffset + 7);
    }
    
    private void AddDashesTriangles(ref VertexHelper vh, int index)
    {
        int cellOffset = index * (24 + 4 * horizontalDashes * 2 + 4 * verticalDashes * 2);

        for (int dashIndex = 0; dashIndex < horizontalDashes; dashIndex++)
        {
            int dashOffset = dashIndex * 4;
            
            // Bottom dashes
            int bottomDashesStart = cellOffset + 24;
            vh.AddTriangle(bottomDashesStart + dashOffset + 0, bottomDashesStart + dashOffset + 1, bottomDashesStart + dashOffset + 3);
            vh.AddTriangle(bottomDashesStart + dashOffset + 1, bottomDashesStart + dashOffset + 2, bottomDashesStart + dashOffset + 3);

            // Top dashes
            int topDashesStart = bottomDashesStart + 4 * horizontalDashes;
            vh.AddTriangle(topDashesStart + dashOffset + 0, topDashesStart + dashOffset + 1, topDashesStart + dashOffset + 3);
            vh.AddTriangle(topDashesStart + dashOffset + 1, topDashesStart + dashOffset + 2, topDashesStart + dashOffset + 3);
        }
        
        for (int dashIndex = 0; dashIndex < verticalDashes; dashIndex++)
        {
            int dashOffset = dashIndex * 4;
            
            // Left dashes
            int leftDashesStart = cellOffset + 24 + 4 * horizontalDashes * 2;
            vh.AddTriangle(leftDashesStart + dashOffset + 0, leftDashesStart + dashOffset + 1, leftDashesStart + dashOffset + 3);
            vh.AddTriangle(leftDashesStart + dashOffset + 1, leftDashesStart + dashOffset + 2, leftDashesStart + dashOffset + 3);
        
            // Right dashes
            int rightDashesStart = leftDashesStart + 4 * verticalDashes;
            vh.AddTriangle(rightDashesStart + dashOffset + 0, rightDashesStart + dashOffset + 1, rightDashesStart + dashOffset + 3);
            vh.AddTriangle(rightDashesStart + dashOffset + 1, rightDashesStart + dashOffset + 2, rightDashesStart + dashOffset + 3);
        }
    }

    private void AddCellTriangles(ref VertexHelper vh, int index)
    {
        int cellOffset = index * 8;
        
        // Left Edge
        vh.AddTriangle(cellOffset + 0, cellOffset + 1, cellOffset + 5);
        vh.AddTriangle(cellOffset + 5, cellOffset + 4, cellOffset + 0);
        
        // Top Edge
        vh.AddTriangle(cellOffset + 1, cellOffset + 2, cellOffset + 6);
        vh.AddTriangle(cellOffset + 6, cellOffset + 5, cellOffset + 1);
        
        // Right Edge
        vh.AddTriangle(cellOffset + 2, cellOffset + 3, cellOffset + 7);
        vh.AddTriangle(cellOffset + 7, cellOffset + 6, cellOffset + 2);
        
        // Bottom Edge
        vh.AddTriangle(cellOffset + 3, cellOffset + 0, cellOffset + 4);
        vh.AddTriangle(cellOffset + 4, cellOffset + 7, cellOffset + 3);
    }
}
