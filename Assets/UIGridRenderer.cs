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
    [Range(0, 10)] public int dashes = 2;
    

    enum CellType { Solid, Dashed }
    [Header("Cell properties")]
    [SerializeField] private CellType cellType = CellType.Solid;

    private delegate void Function(int x, int y, int index, VertexHelper vh);

    private Function[] functions;
    private Function drawFunction;

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
        vertex.color = color;
        
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
        vertex.color = color;
        
        distance = thickness / Mathf.Sqrt(2f);
        horizontalDashWidth = cellWidth / (dashes * 2 + 2);
        verticalDashWidth = cellHeight / (dashes * 2 + 2);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddDashesVertices(ref vertex, ref vh, xPos, yPos);

        AddCornerCellTriangles(ref vh, index);
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
        for (int i = 0; i < dashes; i++)
        {
            AddHorizontalDashVertices(ref vertex, ref vh, xPos, yPos, horizontalDashWidth, i);
            AddVerticalDashVertices(ref vertex, ref vh, xPos, yPos, verticalDashWidth, i);
        }
    }
    
    private void AddHorizontalDashVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
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
        
        vertex.position = new Vector3(xDashPos, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos);
        vh.AddVert(vertex);
    }

    private void AddVerticalDashVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
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
        yDashPos = yPos + verticalDashWidth / 2 + verticalDashWidth;
        
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
        int offset = index * (24 + 4 * dashes * 4);
        
        Debug.Log("Vertex count: " + vh.currentVertCount);
        
        // Left bottom Corner
        vh.AddTriangle(offset + 15, offset + 4, offset + 23);
        vh.AddTriangle(offset + 0, offset + 4, offset + 15);
        vh.AddTriangle(offset + 0, offset + 8, offset + 4);
        vh.AddTriangle(offset + 8, offset + 16, offset + 4);
        
        // Left top Corner
        vh.AddTriangle(offset + 9, offset + 5, offset + 17);
        vh.AddTriangle(offset + 9, offset + 1, offset + 5);
        vh.AddTriangle(offset + 1, offset + 10, offset + 5);
        vh.AddTriangle(offset + 5, offset + 10, offset + 18);
        
        // Right top Corner
        vh.AddTriangle(offset + 19, offset + 11, offset + 6);
        vh.AddTriangle(offset + 11, offset + 2, offset + 6);
        vh.AddTriangle(offset + 2, offset + 12, offset + 6);
        vh.AddTriangle(offset + 20, offset + 6, offset + 12);
        
        // Right bottom Corner
        vh.AddTriangle(offset + 21, offset + 13, offset + 7);
        vh.AddTriangle(offset + 13, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 3, offset + 14);
        vh.AddTriangle(offset + 14, offset + 22, offset + 7);
    }
    
    private void AddDashesTriangles(ref VertexHelper vh, int index)
    {
        int offset = index * (24 + 4 * dashes * 4);
        
        // TODO: create dashes triangles
    }
    
    private void AddCellTriangles(ref VertexHelper vh, int index)
    {
        int offset = index * 8;
        
        // Left Edge
        vh.AddTriangle(offset + 0, offset + 1, offset + 5);
        vh.AddTriangle(offset + 5, offset + 4, offset + 0);
        
        // Top Edge
        vh.AddTriangle(offset + 1, offset + 2, offset + 6);
        vh.AddTriangle(offset + 6, offset + 5, offset + 1);
        
        // Right Edge
        vh.AddTriangle(offset + 2, offset + 3, offset + 7);
        vh.AddTriangle(offset + 7, offset + 6, offset + 2);
        
        // Bottom Edge
        vh.AddTriangle(offset + 3, offset + 0, offset + 4);
        vh.AddTriangle(offset + 4, offset + 7, offset + 3);
    }
}
