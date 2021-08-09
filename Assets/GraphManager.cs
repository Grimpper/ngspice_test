using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private bool startAtZero = false;
    [SerializeField] private int numberOfPointsToDisplay = -1;
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
            //ShowGraph(1, SpiceParser.Variables);

            List<int> testList = new List<int>
            {
                5, 98, 56, 46, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33, 5, 98, 56,
                46, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33
            };
            ShowGraphTest(testList, numberOfPointsToDisplay,_i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));
            //testList[0] = 20;
            //ShowGraphTest(testList, _i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));
            
            //StartCoroutine(ShowRandomGraph(15, 0.5f));
        }
    }

    private IEnumerator ShowRandomGraph(int size, float repeatRate)
    {
        while (true)
        {
            List <int> list = new List<int>();
            for (int i = 0; i < size; i++)
                list.Add(UnityEngine.Random.Range(0, 500));
            
            ShowGraphTest(list, numberOfPointsToDisplay,_i => "Day " + (_i + 1), _f => "$" + Mathf.RoundToInt(_f));

            yield return new WaitForSeconds(repeatRate);
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

    private void ShowGraph(int variableIndex, in Dictionary<int, SpiceVariable> variables, 
        Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
    {
        getAxisLabelX ??= _i => _i.ToString();
        getAxisLabelY ??= f => Mathf.RoundToInt(f).ToString();

        foreach (GameObject gameObject in gameObjectsList)
        {
            Destroy(gameObject);
        }
        gameObjectsList.Clear();

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        variables.TryGetValue(variableIndex, out SpiceVariable variable);
        variables.TryGetValue(0, out SpiceVariable time);
        
        if (variable == null || time == null) return;
        
        float xMax = (float) time.Values.Max();
        
        float yMax = (float) variable.Values.Max();
        float yMin = (float) variable.Values.Min();

        yMax += (yMax - yMin) * 0.2f;
        yMin -= (yMax - yMin) * 0.2f;

        Debug.Log("Drawing: " + variable.Name);

        GameObject lastCircle = null;
        for (int i = 0; i < variable.Values.Count; i++)
        {
            float xPos = (float) (time.Values[i] / xMax) * graphWidth;
            float yPos = (float) (variable.Values[i] / yMax) * graphHeight;
            
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
            
            CreateLabelX(xPos, getAxisLabelX(i));
            CreateDashX(xPos);
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            float normalizedValue = (float) i / separatorCount;
            
            CreateLabelY(normalizedValue, graphHeight, getAxisLabelY(normalizedValue * yMax));
            CreateDashY(normalizedValue, graphHeight);
        }
    }
    
    private void ShowGraphTest(List<int> testList, int maxVisibleAmount = -1, Func<int, string> getAxisLabelX = null, Func<float, string> getAxisLabelY = null)
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

            CreateLabelX(xPos, getAxisLabelX(i));
            CreateDashX(xPos);
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            float normalizedValue = (float) i / separatorCount;
            
            CreateLabelY(normalizedValue, graphHeight, getAxisLabelY(yMin + normalizedValue * (yMax - yMin)));
            CreateDashY(normalizedValue, graphHeight);
        }
    }

    private void CreateLabelX(float xPos, string labelText)
    {
        RectTransform labelX = Instantiate(labelTemplateX, graphContainer, false);
        labelX.gameObject.SetActive(true);
        labelX.anchoredPosition = new Vector2(xPos, -8f);
        labelX.GetComponent<Text>().text = labelText;
        
        gameObjectsList.Add(labelX.gameObject);
    }
        
    private void CreateLabelY(float normalizedValue, float graphHeight, string labelText)
    {
        RectTransform labelY = Instantiate(labelTemplateY, graphContainer, false);
        labelY.gameObject.SetActive(true);
        
        labelY.anchoredPosition = new Vector2(-14f, normalizedValue * graphHeight);
        labelY.GetComponent<Text>().text = labelText;
        
        gameObjectsList.Add(labelY.gameObject);
    }
    
    private void CreateDashX(float xPos)
    {
        RectTransform dashX = Instantiate(dashTemplateX, graphContainer, false);
        dashX.gameObject.SetActive(true);
        dashX.anchoredPosition = new Vector2(xPos, -5f);
        
        gameObjectsList.Add(dashX.gameObject);
    }

    private void CreateDashY(float normalizedValue, float graphHeight)
    {
        RectTransform dashY = Instantiate(dashTemplateY, graphContainer, false);
        dashY.gameObject.SetActive(true);
        dashY.anchoredPosition = new Vector2(-4, normalizedValue * graphHeight);
        
        gameObjectsList.Add(dashY.gameObject);
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

    private static float GetAngleFromVectorDegrees(Vector2 vector)
    {
        return (float) (Math.Atan(vector.y / vector.x) * 180 / Math.PI);
    }
}   
