using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private bool startAtZero = false;

    [SerializeField] private float minDistanceBetweenPoints = 0;
    [SerializeField] private int maxVisibleAmount = -1;
    
    private const float MinYDiff = 5f;

    #region GraphObjects
    private RectTransform graphContainer;
    private RectTransform title;
    private RectTransform xAxisTitle;
    private RectTransform yAxisTitle;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;
    #endregion

    #region GraphData
    private float graphWidth;
    private float graphHeight;
    
    private float xMin;
    private float xMax;
    private float yMin;
    private float yMax;

    private float xStep;
    private float yStep;

    private int startIndex;
    #endregion
    
    private void Awake()
    {
        graphContainer = transform.Find("Graph container").GetComponent<RectTransform>();
        graphWidth = graphContainer.sizeDelta.x;
        graphHeight = graphContainer.sizeDelta.y;
        
        title = graphContainer.Find("Title").GetComponent<RectTransform>();
        xAxisTitle = graphContainer.Find("X axis title").GetComponent<RectTransform>();
        yAxisTitle = graphContainer.Find("Y axis title").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("Label template X").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("Label template Y").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("Dash template X").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("Dash template Y").GetComponent<RectTransform>();
        gameObjectsList = new List<GameObject>();
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(330, 10, 150, 50), "Show graph"))
            ShowGraph(2, SpiceParser.Variables, maxVisibleAmount);
    }

    private void ShowGraph(int variableIndex, in Dictionary<int, SpiceVariable> variables,  int visibleAmount = -1, 
        Func<float, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        getAxisLabelX ??= f => 
            Math.Round(f, NumberUtils.GetSignificantFigurePos(f) + 2).ToString(CultureInfo.InvariantCulture);
        getAxisLabelY ??= f => 
            Math.Round(f, NumberUtils.GetSignificantFigurePos(f) + 2).ToString(CultureInfo.InvariantCulture);

        EmptyGameObjectList(gameObjectsList);

        if (!variables.TryGetValue(0, out var xVariable) || !variables.TryGetValue(variableIndex, out var yVariable))
            return;
        List<float> xValues = xVariable.GetValues(NumberUtils.Unit.m);
        List<float> yValues = yVariable.GetValues();
        
        if (visibleAmount < 0) 
            visibleAmount = yVariable.GetValues().Count;

        (yMin, yMax, yStep, startIndex) = GetAxisValues(yValues, visibleAmount);
        (xMin, xMax, xStep, _) = GetAxisValues(xValues);

        SetTitles(xVariable, yVariable);
        CreateLabelsAndDashes(getAxisLabelX, getAxisLabelY);
        CreateDotsAndConnections(xValues, yValues);

    }
    
    private void EmptyGameObjectList(List<GameObject> list)
    {
        foreach (GameObject element in list)
        {
            Destroy(element);
        }
        list.Clear();
    }

    private void SetTitles(SpiceVariable xVariable, SpiceVariable yVariable)
    {
        title.GetComponent<Text>().text = SpiceParser.Title;
        title.gameObject.SetActive(true);

        string unit = String.Empty;

        if (xVariable.DisplayName.Equals("time"))
            unit = " (" + NumberUtils.Time + ")";

        xAxisTitle.GetComponent<Text>().text = xVariable.DisplayName + unit;
        xAxisTitle.gameObject.SetActive(true);
        
        if (yVariable.Name.StartsWith("v"))
            unit = " (" + NumberUtils.Voltage + ")";
        else if (yVariable.Name.StartsWith("i"))
            unit = " (" + NumberUtils.Intensity + ")";
        
        yAxisTitle.GetComponent<Text>().text = yVariable.DisplayName + unit;
        yAxisTitle.gameObject.SetActive(true);
    }
    
    private void CreateLabelsAndDashes(Func<float, string> getAxisLabelX, Func<float, string> getAxisLabelY)
    {
        for (float xSeparatorPos = xMin; xSeparatorPos <= xMax; xSeparatorPos += xStep)
        {
            float graphPosX = GetGraphPosX(xSeparatorPos);
            CreateLabel(labelTemplateX, new Vector2(graphPosX, -8f), getAxisLabelX(xSeparatorPos));
            CreateDash(dashTemplateX, new Vector2(graphPosX, -5f));
        }
        
        for (float ySeparatorPos = yMin; ySeparatorPos <= yMax; ySeparatorPos += yStep)
        {
            float graphPosY = GetGraphPosY(ySeparatorPos);
            CreateLabel(labelTemplateY, new Vector2(-14f, graphPosY), getAxisLabelY(ySeparatorPos));
            CreateDash(dashTemplateY,new Vector2(-4, graphPosY));
        }
    }
    
    private void CreateDotsAndConnections(in List<float> xValues, in List<float> yValues)
    {
        
        GameObject lastCircle = null;
        
        for (int i = startIndex; i < yValues.Count; i++)
        {
            Vector2 dataPoint = new Vector2(GetGraphPosX(xValues[i]), GetGraphPosY(yValues[i]));

            if (lastCircle &&
                (dataPoint - lastCircle.GetComponent<RectTransform>().anchoredPosition).magnitude < minDistanceBetweenPoints) 
                continue;
            
            GameObject circle = CreateCircle(dataPoint);
            gameObjectsList.Add(circle);


            if (lastCircle && circle)
            {
                GameObject connection = CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                    circle.GetComponent<RectTransform>().anchoredPosition);
                gameObjectsList.Add(connection);
            }

            lastCircle = circle;
        }
    }
    
    private GameObject CreateCircle(Vector2 anchoredPos)
    {
        GameObject dot = new GameObject("circle", typeof(Image));
        dot.transform.SetParent(graphContainer, false);
        dot.GetComponent<Image>().sprite = circleSprite;

        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return dot;
    }
    
    private float GetGraphPosX(float xPos) => (xPos - xMin) / (xMax - xMin) * graphWidth;
    private float GetGraphPosY(float yPos) => (yPos - yMin) / (yMax - yMin) * graphHeight;

    private (float min, float max, float step, int startIndex) GetAxisValues(IReadOnlyList<float> valueList, 
        int? visibleAmount = null)
    {
    int startValueIndex = Mathf.Max(valueList.Count - visibleAmount ?? 0, 0);
        
        float max = valueList[0];
        float min = valueList[0];
        
        for (int i = startValueIndex; i < valueList.Count; i++)
        {
            float value = valueList[i];
            if (value > max)
                max = value;
            else if (value < min)
                min = value;
        }

        float diff = max - min <= 0 ? MinYDiff : max - min;
        
        int significantFigurePos = NumberUtils.GetSignificantFigurePos(diff);
        float significantFigure = diff * Mathf.Pow(10, significantFigurePos);

        float step = Mathf.Pow(10, significantFigurePos) / (significantFigure > 5f ? 1f : 2f);
        
        max += step;
        min -= step;
        
        if (startAtZero)
            min = 0;

        return (min , max, step, startValueIndex);
    }   
    
    private void CreateLabel(RectTransform labelTemplate, Vector2 position, string labelText)
    {
        RectTransform label = Instantiate(labelTemplate, graphContainer, false);
        label.gameObject.SetActive(true);
        label.anchoredPosition = position;
        label.GetComponent<Text>().text = labelText;
        
        gameObjectsList.Add(label.gameObject);
    }

    private void CreateDash(RectTransform dashTemplate, Vector2 position)
    {
        RectTransform dash = Instantiate(dashTemplate, graphContainer, false);
        dash.gameObject.SetActive(true);
        dash.anchoredPosition = position;
        
        gameObjectsList.Add(dash.gameObject);
    }

    private GameObject CreateConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject connection = new GameObject("dotConnection", typeof(Image));
        connection.transform.SetParent(graphContainer, false);
        connection.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);

        RectTransform rectTransform = connection.GetComponent<RectTransform>();
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA +  0.5f * distance * dir;
        rectTransform.localEulerAngles = 
            new Vector3(0, 0, NumberUtils.GetAngleFromVector(dir, NumberUtils.Degrees) ?? 0f);

        return connection;
    }
}   
