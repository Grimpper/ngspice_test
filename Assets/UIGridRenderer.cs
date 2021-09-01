using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGridRenderer : Graphic
{
    [SerializeField] private Vector2Int gridSize = new Vector2Int(1, 1);
    
    public Vector2Int GridSize
    {
        set
        {
            gridSize = value;
            UpdateGeometry();
        }
    }

    public float thickness = 10f;

    private float width;
    private float height;
    private float cellWidth;
    private float cellHeight;
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
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
                DrawDashedCell(x, y, count, vh);
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
        
        float distance = thickness / Mathf.Sqrt(2f);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos, distance);
        AddCellTriangles(ref vh, index);
    }
    
    private void DrawDashedCell(int x, int y, int index, VertexHelper vh)
    {
        float xPos = cellWidth * x;
        float yPos = cellHeight * y;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        
        float distance = thickness / Mathf.Sqrt(2f);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos, distance);
        AddMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalMiddleVertices(ref vertex, ref vh, xPos, yPos, distance);
        AddDashedCellTriangles(ref vh, index);
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

    private void AddInternalCornerVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, float distance)
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
        vertex.position = new Vector3(xPos, yPos + cellHeight / 3f);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight - cellHeight / 3f);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth / 3f, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - cellWidth / 3f, yPos + cellHeight);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight - cellHeight / 3f);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight / 3f);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - cellWidth / 3f, yPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth / 3f, yPos);
        vh.AddVert(vertex);
    }
    
    private void AddInternalMiddleVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, float distance)
    {
        vertex.position = new Vector3(xPos + distance, yPos + cellHeight / 3f);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + cellHeight - cellHeight / 3f);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth / 3f, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - cellWidth / 3f, yPos + cellHeight - distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - cellHeight / 3f);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight / 3f);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - cellWidth / 3f, yPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth / 3f, yPos + distance);
        vh.AddVert(vertex);
    }

    private void AddDashedCellTriangles(ref VertexHelper vh, int index)
    {
        int offset = index * 24;
        
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
        vh.AddTriangle(offset + 7, offset + 3, offset + 16);
        vh.AddTriangle(offset + 14, offset + 22, offset + 7);
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
