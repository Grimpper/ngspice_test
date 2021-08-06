using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private void Awake()
    {
        graphContainer = transform.Find("Graph container").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("Label template X").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("Label template Y").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("Dash template X").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("Dash template Y").GetComponent<RectTransform>();
    }
    
    private void OnGUI()
    {
        if (GUI.Button(new Rect(330, 10, 150, 50), "Show graph"))
        {
            List<int> testList = new List<int>() {5, 98, 56, 46, 30, 22, 17, 15, 13, 17, 25, 37, 40, 36, 33};
            //ShowGraph(1, SpiceParser.Variables);
            ShowGraphTest(testList);
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

    private void ShowGraph(int variableIndex, in Dictionary<string, SpiceVariable> variables)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        variables.TryGetValue(variableIndex.ToString(), out SpiceVariable variable);
        variables.TryGetValue("0", out SpiceVariable time);
        
        if (variable == null || time == null) return;
        
        float yMax = (float) variable.Values.Max(y => y);
        float xMax = (float) time.Values.Max(t => t);

        Debug.Log("Drawing: " + variable.Name);

        GameObject lastCircle = null;
        for (int i = 0; i < variable.Values.Count; i++)
        {
            float xPos = (float) (time.Values[i] / xMax) * graphWidth;
            float yPos = (float) (variable.Values[i] / yMax) * graphHeight;
            
            Vector2 dataPoint = new Vector2(xPos, yPos);
            GameObject circle = CreateCircle(dataPoint);

            if (lastCircle != null)
            {
                CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                    circle.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircle = circle;
        }
    }
    
    private void ShowGraphTest(List<int> testList)
    {
        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;
        
        float xSize = 28f;
        float yMax = 100f;

        GameObject lastCircle = null;
        for (int i = 0; i < testList.Count; i++)
        {
            float xPos = xSize + i * xSize;
            float yPos = testList[i] / yMax * graphHeight;
            
            Vector2 dataPoint = new Vector2(xPos, yPos);
            GameObject circle = CreateCircle(dataPoint);

            if (lastCircle != null)
            {
                CreateConnection(lastCircle.GetComponent<RectTransform>().anchoredPosition,
                    circle.GetComponent<RectTransform>().anchoredPosition);
            }

            lastCircle = circle;

            RectTransform labelX = Instantiate(labelTemplateX, graphContainer, false);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPos, -5f);
            labelX.GetComponent<Text>().text = i.ToString();
            
            RectTransform dashX = Instantiate(dashTemplateX, graphContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(xPos, -5f);
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY, graphContainer, false);
            labelY.gameObject.SetActive(true);
            
            float normalizedValue = (float) i / separatorCount;
            labelY.anchoredPosition = new Vector2(-14f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = Mathf.RoundToInt(normalizedValue * yMax).ToString();
            
            RectTransform dashY = Instantiate(dashTemplateY, graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4, normalizedValue * graphHeight);
        }
    }

    private void CreateConnection(Vector2 dotPositionA, Vector2 dotPositionB)
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
    }

    private static float GetAngleFromVectorDegrees(Vector2 vector)
    {
        return (float) (Math.Atan(vector.y / vector.x) * 180 / Math.PI);
    }
}   
