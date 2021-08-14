using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private bool startAtZero = false;

    [SerializeField] private float minXDistanceBetweenPoints = 0;
    
    //[SerializeField] private int numberOfPointsToDisplay = -1;
    private const float MinYDiff = 5f;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectsList;

    private void Awake()
    {
        graphContainer = transform.Find("Graph container").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("Label template X").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("Label template Y").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("Dash template X").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("Dash template Y").GetComponent<RectTransform>();
        gameObjectsList = new List<GameObject>();
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(330, 10, 150, 50), "Show graph"))
        {
            ShowGraph(2, SpiceParser.Variables);

            //List<int> testList = new List<int>
            //{
            //    5, 98, 56, 46, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33, 5, 98, 56,
            //    46, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33
            //};
            //ShowGraphTest(testList, numberOfPointsToDisplay,_i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));
            //testList[0] = 20;
            //ShowGraphTest(testList, _i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));
            
            //StartCoroutine(ShowRandomGraph(15, 0.5f));
        }
    }

    /*private IEnumerator ShowRandomGraph(int size, float repeatRate)
    {
        while (true)
        {
            List <int> list = new List<int>();
            for (int i = 0; i < size; i++)
                list.Add(UnityEngine.Random.Range(0, 500));
            
            ShowGraphTest(list, numberOfPointsToDisplay,_i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));

            yield return new WaitForSeconds(repeatRate);
        }
    }*/

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

    private void ShowGraph(int variableIndex, in Dictionary<int, SpiceVariable> variables,  int maxVisibleAmount = -1, 
        Func<float, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        getAxisLabelX ??= f => Math.Round(f, GetSignificantFigurePos(f) + 2).ToString(CultureInfo.InvariantCulture);
        getAxisLabelY ??= f => Math.Round(f, GetSignificantFigurePos(f) + 2).ToString(CultureInfo.InvariantCulture);

        foreach (GameObject gameObject in gameObjectsList)
        {
            Destroy(gameObject);
        }
        gameObjectsList.Clear();
        
        variables.TryGetValue(variableIndex, out SpiceVariable variable);
        variables.TryGetValue(0, out SpiceVariable time);
        
        if (variable == null || time == null) return;

        if (maxVisibleAmount < 0)
            maxVisibleAmount = variable.Values.Count;

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        var (yMin, yMax, yStep, startIndex) = GetAxisValues(variable.Values, maxVisibleAmount);
        var (xMin, xMax, xStep, _) = GetAxisValues(time.Values);

        GameObject lastCircle = null;
        for (int i = startIndex; i < variable.Values.Count; i++)
        {
            float xPos = (time.Values[i] - xMin) / (xMax - xMin) * graphWidth;
            float yPos = (variable.Values[i] - yMin) / (yMax - yMin) * graphHeight;
            
            Vector2 dataPoint = new Vector2(xPos, yPos);

            if (lastCircle && 
                xPos - lastCircle.GetComponent<RectTransform>().anchoredPosition.x < minXDistanceBetweenPoints) 
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
        
        for (float xSeparatorPos = xMin; xSeparatorPos <= xMax; xSeparatorPos += xStep)
        {
            float normalizedValue = xSeparatorPos / (xMax - xMin);
            
            CreateLabel(labelTemplateX, new Vector2(normalizedValue * graphWidth, -8f), 
                getAxisLabelX(xSeparatorPos));
            CreateDash(dashTemplateX, new Vector2(normalizedValue * graphWidth, -5f));

        }
        
        for (float ySeparatorPos = yMin; ySeparatorPos <= yMax; ySeparatorPos += yStep)
        {
            float normalizedValue = ySeparatorPos / (yMax - yMin);
            
            CreateLabel(labelTemplateY, new Vector2(-14f, normalizedValue * graphHeight),
                getAxisLabelY(ySeparatorPos));
            CreateDash(dashTemplateY,new Vector2(-4, normalizedValue * graphHeight));
        }
    }
    
    /*private void ShowGraphTest(List<int> testList, int maxVisibleAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        getAxisLabelX ??= _i => _i.ToString();
        getAxisLabelY ??= _f => Mathf.RoundToInt(_f).ToString();
        
        if (maxVisibleAmount < 0)
            maxVisibleAmount = testList.Count;
        
        foreach (GameObject gameObject in gameObjectsList)
        {
            Destroy(gameObject);
        }
        gameObjectsList.Clear();
        
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        
        int startIndex = Mathf.Max(testList.Count - maxVisibleAmount, 0);
        
        float xSize = graphWidth / (maxVisibleAmount + 1);
        
        float yMax = testList[0];
        float yMin = testList[0];

        for (int i = startIndex; i < testList.Count; i++)
        {
            int value = testList[i];
            if (value > yMax)
                yMax = value;
            else if (value < yMin)
                yMin = value;
        }

        float yDiff = yMax - yMin <= 0 ? MinYDiff : yMax - yMin;
        yMax += yDiff * 0.2f;
        yMin -= yDiff * 0.2f;

        if (startAtZero)
            yMin = 0;
        
        GameObject lastCircle = null;
        int xIndex = 0;
        for (int i = startIndex; i < testList.Count; i++, xIndex++)
        {
            float xPos = xSize + xIndex * xSize;
            float yPos = (testList[i] - yMin) / (yMax - yMin) * graphHeight;
            
            Vector2 dataPoint = new Vector2(xPos, yPos);
            
            GameObject circle = CreateCircle(dataPoint);
            gameObjectsList.Add(circle);

            if (lastCircle != null)
            {
                GameObject connection = CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                    circle.GetComponent<RectTransform>().anchoredPosition);
                gameObjectsList.Add(connection);
            }

            lastCircle = circle;

            CreateLabel(labelTemplateX, new Vector2(xPos, -8f), getAxisLabelX(i));
            CreateDash(dashTemplateX, new Vector2(xPos, -5f));
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            float normalizedValue = (float) i / separatorCount;
            
            CreateLabel(labelTemplateY, new Vector2(-14f, normalizedValue * graphHeight),
                getAxisLabelY(yMin + normalizedValue * (yMax - yMin)));
            CreateDash(dashTemplateY,new Vector2(-4, normalizedValue * graphHeight));
        }
    }*/

    private (float min, float max, float step, int startIndex) GetAxisValues(IReadOnlyList<float> valueList, 
        int? maxVisibleAmount = null)
    {
    int startIndex = Mathf.Max(valueList.Count - maxVisibleAmount ?? 0, 0);
        
        float max = valueList[0];
        float min = valueList[0];
        
        for (int i = startIndex; i < valueList.Count; i++)
        {
            float value = valueList[i];
            if (value > max)
                max = value;
            else if (value < min)
                min = value;
        }

        float diff = max - min <= 0 ? MinYDiff : max - min;
        
        int significantFigurePos = GetSignificantFigurePos(diff);
        float significantFigure = diff * Mathf.Pow(10, significantFigurePos);

        float step = Mathf.Pow(10, -significantFigurePos) / (significantFigure > 5 ? 1f : 2f);

        max += step;
        min -= step;
        
        if (startAtZero)
            min = 0;

        return (min , max, step, startIndex);
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
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorDegrees(dir));

        return connection;
    }

    private int GetSignificantFigurePos(float number)
    {
        float absNumber = Math.Abs(number);

        if (absNumber > 1 || absNumber == 0) return 0;
        
        int decimalPlaces = 0;
        while (Math.Floor(absNumber) == 0)
        {
            absNumber *= 10;
            decimalPlaces++;
        }

        return decimalPlaces;
    }

    private float GetAngleFromVectorDegrees(Vector2 vector)
    {
        return (float) (Math.Atan(vector.y / vector.x) * 180 / Math.PI);
    }
}   
