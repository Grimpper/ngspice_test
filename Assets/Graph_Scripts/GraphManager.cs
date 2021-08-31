using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Material circleMaterial;
    [SerializeField] private float circleDiameter = 11;
    [SerializeField] private bool startAtZero = false;

    [SerializeField] private float minDistanceBetweenPoints = 0;
    [SerializeField] private int maxVisibleAmount = -1;

    #region GraphObjects
    private RectTransform graphContainer;
    private RectTransform labelsBackground;
    private RectTransform titlesBackground;
    private RectTransform title;
    private RectTransform xAxisTitle;
    private RectTransform yAxisTitle;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;

    private UIGridRenderer uiGridRenderer;
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
        labelsBackground = transform.Find("Labels background").GetComponent<RectTransform>();
        titlesBackground = transform.Find("Titles background").GetComponent<RectTransform>();
        
        graphContainer = transform.Find("Graph container").GetComponent<RectTransform>();
        graphWidth = graphContainer.sizeDelta.x;
        graphHeight = graphContainer.sizeDelta.y;
        
        title = titlesBackground.Find("Title").GetComponent<RectTransform>();
        xAxisTitle = titlesBackground.Find("X axis title").GetComponent<RectTransform>();
        yAxisTitle = titlesBackground.Find("Y axis title").GetComponent<RectTransform>();
        
        labelTemplateX = labelsBackground.Find("Label template X").GetComponent<RectTransform>();
        labelTemplateY = labelsBackground.Find("Label template Y").GetComponent<RectTransform>();
        
        dashTemplateX = graphContainer.Find("Dash template X").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("Dash template Y").GetComponent<RectTransform>();
        SetTemplateSizes();
        
        gameObjectsList = new List<GameObject>();

        uiGridRenderer = transform.Find("UIGridRenderer").GetComponent<UIGridRenderer>();
    }

    private void SetTemplateSizes()
    {
        dashTemplateX.sizeDelta = new Vector2(graphWidth / dashTemplateX.transform.localScale.x, dashTemplateX.sizeDelta.y);
        dashTemplateY.sizeDelta = new Vector2(graphHeight / dashTemplateY.transform.localScale.x, dashTemplateY.sizeDelta.y);
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(330, 10, 150, 50), "Show graph"))
            ShowGraph(2, SpiceParser.Variables, maxVisibleAmount);
    }

    private void ShowGraph(int variableIndex, in Dictionary<int, SpiceVariable> variables,  int visibleAmount = -1, 
        Func<float, float, string> getAxisLabelX = null, Func<float, float,  string> getAxisLabelY = null)
    {
        getAxisLabelX ??= (number, diff) => 
            Math.Round(number, Mathf.Abs(NumberUtils.GetSignificantFigurePos(diff)) + 2).ToString(CultureInfo.InvariantCulture);
        getAxisLabelY ??= (number, diff) => 
            Math.Round(number, Mathf.Abs(NumberUtils.GetSignificantFigurePos(diff)) + 2).ToString(CultureInfo.InvariantCulture);

        EmptyGameObjectList(gameObjectsList);

        if (!variables.TryGetValue(0, out var xVariable) || !variables.TryGetValue(variableIndex, out var yVariable))
            return;
        List<float> xValues = xVariable.GetValues(NumberUtils.Unit.m);
        List<float> yValues = yVariable.GetValues();
        
        if (visibleAmount < 0) 
            visibleAmount = yVariable.GetValues().Count;

        (yMin, yMax, yStep, startIndex) = GetAxisValues(yValues, visibleAmount);
        (xMin, xMax, xStep, _) = GetAxisValues(xValues);
        Debug.Log(xMax);

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
        title.GetComponent<TextMeshProUGUI>().text = SpiceParser.Title;
        title.anchoredPosition = new Vector2(0, -(titlesBackground.sizeDelta.y - labelsBackground.sizeDelta.y) / 4f);
        title.gameObject.SetActive(true);

        string unit = String.Empty;

        if (xVariable.DisplayName.Equals("time"))
            unit = " (" + NumberUtils.Time + ")";

        xAxisTitle.GetComponent<TextMeshProUGUI>().text = xVariable.DisplayName + unit;
        xAxisTitle.anchoredPosition = new Vector2(0, (titlesBackground.sizeDelta.y - labelsBackground.sizeDelta.y) / 4f);
        xAxisTitle.gameObject.SetActive(true);
        
        if (yVariable.Name.StartsWith("v"))
            unit = " (" + NumberUtils.Voltage + ")";
        else if (yVariable.Name.StartsWith("i"))
            unit = " (" + NumberUtils.Intensity + ")";
        
        yAxisTitle.GetComponent<TextMeshProUGUI>().text = yVariable.DisplayName + unit;
        yAxisTitle.anchoredPosition = new Vector2((titlesBackground.sizeDelta.x - labelsBackground.sizeDelta.x) / 4f, 0);
        yAxisTitle.gameObject.SetActive(true);
    }
    
    private void CreateLabelsAndDashes(Func<float, float, string> getAxisLabelX, Func<float, float, string> getAxisLabelY)
    {
        int xLabelCount = 0;
        for (float xSeparatorPos = xMin; xSeparatorPos < xMax + xStep; xSeparatorPos += xStep, xLabelCount++)
        {
            float diff = xMax - xMin;
            float graphPosX = GetGraphPosX(xSeparatorPos);
            float yLabelPos = (labelsBackground.sizeDelta.y - graphHeight) / 4f;
            CreateLabel(labelTemplateX, new Vector2(graphPosX, -yLabelPos), getAxisLabelX(xSeparatorPos, diff));
            
            //if (!IsLastOrFistDash(xSeparatorPos, xMin, xMax, xStep)) 
            //    CreateDash(dashTemplateX, new Vector2(graphPosX, 0));
        }

        int yLabelCount = 0;
        for (float ySeparatorPos = yMin; ySeparatorPos < yMax + yStep; ySeparatorPos += yStep, yLabelCount++)
        {
            float diff = yMax - yMin;
            float graphPosY = GetGraphPosY(ySeparatorPos);
            float xLabelPos = (labelsBackground.sizeDelta.x - graphWidth) / 4f;
            CreateLabel(labelTemplateY, new Vector2(-xLabelPos, graphPosY), getAxisLabelY(ySeparatorPos, diff));
            
            //if (!IsLastOrFistDash(ySeparatorPos, yMin, yMax, yStep))
            //    CreateDash(dashTemplateY,new Vector2(0, graphPosY));
        }
        
        int xDivisions = --xLabelCount;
        int yDivisions = --yLabelCount;
        uiGridRenderer.GridSize = new Vector2Int(xDivisions, yDivisions);
    }

    private bool IsLastOrFistDash(float pos, float min, float max, float step)
    {
        return pos < min + step / 2f || pos > max - step / 2f;
    }
    
    private void CreateDotsAndConnections(in List<float> xValues, in List<float> yValues)
    {
        
        GameObject lastCircle = null;
        Vector2? lastDataPoint = null;
        
        for (int i = startIndex; i < yValues.Count; i++)
        {
            Vector2 dataPoint = new Vector2(GetGraphPosX(xValues[i]), GetGraphPosY(yValues[i]));

            if (!lastCircle || !((dataPoint - lastCircle.GetComponent<RectTransform>().anchoredPosition).magnitude <
                                 minDistanceBetweenPoints))
            {
                GameObject circle = CreateCircle(dataPoint);
                gameObjectsList.Add(circle);

                lastCircle = circle;
            }

            if (lastDataPoint != null)
            {
                GameObject connection = CreateConnection((Vector2) lastDataPoint,
                    dataPoint);
                gameObjectsList.Add(connection);
            }

            lastDataPoint = dataPoint;
        }
    }
    
    private GameObject CreateCircle(Vector2 anchoredPos)
    {
        GameObject dot = new GameObject("circle", typeof(Image));
        dot.transform.SetParent(graphContainer, false);
        dot.GetComponent<Image>().material = circleMaterial;

        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(circleDiameter, circleDiameter);
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

        float diff = max - min;
        
        int significantFigurePos = NumberUtils.GetSignificantFigurePos(diff);
        float significantFigure = diff * Mathf.Pow(10, significantFigurePos);

        float step = Mathf.Pow(10, significantFigurePos) / (significantFigure > 5f ? 1f : 2f);
        
        //max += step;
        //min -= step;
        
        if (startAtZero)
            min = 0;

        return (min , max, step, startValueIndex);
    }   
    
    private void CreateLabel(RectTransform labelTemplate, Vector2 position, string labelText)
    {
        RectTransform label = Instantiate(labelTemplate, graphContainer, false);
        label.gameObject.SetActive(true);
        label.anchoredPosition = position;
        label.GetComponent<TextMeshProUGUI>().text = labelText;
        
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
        Debug.Log(dir);
        float distance = Vector2.Distance(dotPositionA, dotPositionB);
        
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 3f);
        rectTransform.anchoredPosition = dotPositionA + 0.5f * distance * dir;
        rectTransform.localEulerAngles = 
            new Vector3(0, 0, NumberUtils.GetAngleFromVector(dir, NumberUtils.Degrees) ?? 0f);

        return connection;
    }
}   
