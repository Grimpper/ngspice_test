using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    enum LineConnection { Intersect, Bisect }
    [SerializeField] private LineConnection lineConnection = LineConnection.Intersect;

    private delegate void Function(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint);

    private Function[] functions;
    
    private Function GetFunction() => functions[(int) lineConnection];

    [SerializeField] private Vector2Int gridSize;
    public List<Vector2> points;

    private float width;
    private float height;
    private float unitWidth;
    private float unitHeight;

    [SerializeField] private float thickness = 10f;

    public Vector2Int GridSize
    {
        set
        {
            gridSize = value;
            UpdateGeometry();
        }
    }

    [SerializeField] private bool debug = false;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        functions = new Function[] { DrawIntersecting, DrawBisecting };

        Function drawFunction = GetFunction();
        
        vh.Clear();

        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        unitWidth = width / gridSize.x;
        unitHeight = height / gridSize.y;
        
        if (points.Count < 2) 
            return;

        DrawFirstPointVertices(ref vh, points[0], points[1]);
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 lastPoint = points[i - 1];
            Vector2 point = points[i];
            Vector2 nextPoint = points[i + 1];

            drawFunction(ref vh, lastPoint, point, nextPoint);
        }
        DrawLastPointVertices(ref vh, points[points.Count - 2], points[points.Count - 1]);
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawTriangles(ref vh, i);
        }
    }

    private void DrawIntersecting(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint)
    {
        NumberUtils.Line lineA = new NumberUtils.Line(lastPoint, point - lastPoint);
        NumberUtils.Line lineAPlus = GetLinePlus(lineA);
        NumberUtils.Line lineAMinus = GetLineMinus(lineA);
            
        NumberUtils.Line lineB = new NumberUtils.Line(point, nextPoint - point);
        NumberUtils.Line lineBPlus = GetLinePlus(lineB);
        NumberUtils.Line lineBMinus = GetLineMinus(lineB);

        Vector2 abPlusIntersection = NumberUtils.LineIntersection(lineAPlus, lineBPlus);
        Vector2 abMinusIntersection = NumberUtils.LineIntersection(lineAMinus, lineBMinus);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 0, 255, 255) : (Color32)color;
            
        vertex.position = new Vector3(abPlusIntersection.x, abPlusIntersection.y);
        vh.AddVert(vertex);
            
        vertex.position = new Vector3(abMinusIntersection.x, abMinusIntersection.y);
        vh.AddVert(vertex);
    }

    private void DrawBisecting(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint)
    {
        NumberUtils.Line lineA = new NumberUtils.Line(lastPoint, point - lastPoint);
        NumberUtils.Line lineB = new NumberUtils.Line(point, nextPoint - point);
        
        float bisectorAngle = NumberUtils.GetAngleBisectorAngle(lineA.dir, lineB.dir);

        Vector2 bisectorUnitVector = new Vector2(Mathf.Cos(bisectorAngle), Mathf.Sin(bisectorAngle));
            
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 0, 255, 255) : (Color32)color;
            
        float xPos = point.x + thickness * bisectorUnitVector.x;
        float yPos = point.y + thickness * bisectorUnitVector.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
            
        xPos = point.x - thickness * bisectorUnitVector.x;
        yPos = point.y - thickness * bisectorUnitVector.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
    }

    private NumberUtils.Line GetLinePlus(NumberUtils.Line line)
    {
        float xPosPlus = line.GetPoint().x + thickness * line.normal.x;
        float yPosPlus = line.GetPoint().y + thickness * line.normal.y;
        Vector2 pointPlus = new Vector2(xPosPlus, yPosPlus);
        return new NumberUtils.Line(pointPlus, line.dir);
    }
    
    private NumberUtils.Line GetLineMinus(NumberUtils.Line line)
    {
        float xPosMinus = line.GetPoint().x - thickness * line.normal.x;
        float yPosMinus = line.GetPoint().y - thickness * line.normal.y;
        Vector2 pointMinus = new Vector2(xPosMinus, yPosMinus);
        return new NumberUtils.Line(pointMinus, line.dir);
    }

    private void DrawFirstPointVertices(ref VertexHelper vh, Vector2 firstPoint, Vector2 secondPoint)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(255, 255, 255, 255) : (Color32)color;

        Vector2 normal = NumberUtils.GetNormalVector(secondPoint - firstPoint);

        float xPos = firstPoint.x + thickness * normal.x;
        float yPos = firstPoint.y + thickness * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
        
        vertex.color = debug ? new Color32(255, 0, 255, 255) : (Color32)color;
        
        xPos = firstPoint.x - thickness * normal.x;
        yPos = firstPoint.y - thickness * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
    }
    
    private void DrawLastPointVertices(ref VertexHelper vh, Vector2 secondLastPoint, Vector2 lastPoint)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(255, 0, 0, 255) : (Color32)color;

        Vector2 normal = NumberUtils.GetNormalVector(lastPoint - secondLastPoint);

        float xPos = lastPoint.x + thickness * normal.x;
        float yPos = lastPoint.y + thickness * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
        
        vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
        xPos = lastPoint.x - thickness * normal.x;
        yPos = lastPoint.y - thickness * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
    }

    private void DrawTriangles(ref VertexHelper vh, int index)
    {
        int vertexCoupleOffset = index * 2;
        
        vh.AddTriangle(vertexCoupleOffset + 0, vertexCoupleOffset + 1, vertexCoupleOffset + 2);
        vh.AddTriangle(vertexCoupleOffset + 2, vertexCoupleOffset + 3, vertexCoupleOffset + 1);
    }
}
