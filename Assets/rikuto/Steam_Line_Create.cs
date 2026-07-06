using System.Collections.Generic;
using UnityEngine;

public class Steam_Line_Create : MonoBehaviour
{
    [Header("参照")]
    public Camera mainCamera;
    public LineRenderer linePrefab;
    public LayerMask mirrorLayer;

    [Header("描画設定")]
    public float minPointDistance = 0.05f;
    public float pointLifeTime = 1.5f;

    [Header("ゲージ設定")]
    public float maxGauge = 5f;
    public float currentGauge;

    public float usePerDistance = 5f; // ←距離消費
    public float recoveryPerSecond = 1f;

    private class TimedPoint
    {
        public Vector3 worldPos;
        public float createTime;
    }

    private class LineData
    {
        public LineRenderer lineRenderer;
        public EdgeCollider2D edgeCollider;
        public List<TimedPoint> points = new();
    }

    private LineData currentLineData = null;
    private readonly List<LineData> activeLines = new();

    void Start()
    {
        currentGauge = maxGauge;
    }

    void Update()
    {
        UpdateAllLinesLife();

        bool isDrawingInput = Input.GetMouseButton(0);

        if (isDrawingInput)
        {
            currentGauge = Mathf.Max(currentGauge, 0f);

            if (currentGauge > 0f)
            {
                UpdateDrawing();
            }
            else
            {
                FinishCurrentLine();
            }
        }
        else
        {
            currentGauge += recoveryPerSecond * Time.deltaTime;
            currentGauge = Mathf.Min(currentGauge, maxGauge);
        }

        if (Input.GetMouseButtonUp(0))
        {
            FinishCurrentLine();
        }
    }

    private void UpdateDrawing()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        bool isOnMirror = IsMouseOnMirror(mouseWorldPos);

        if (isOnMirror)
        {
            if (currentLineData == null)
            {
                StartNewLine(mouseWorldPos);
                return;
            }

            Vector3 lastPoint = currentLineData.points[currentLineData.points.Count - 1].worldPos;

            float distance = Vector3.Distance(lastPoint, mouseWorldPos);

            if (distance >= minPointDistance)
            {
                // ? ここが追加ポイント（距離消費）
                float cost = distance * usePerDistance;

                if (currentGauge >= cost)
                {
                    currentGauge -= cost;

                    AddPoint(currentLineData, mouseWorldPos);
                    RefreshLineVisual(currentLineData);
                }
                else
                {
                    // 足りなければ終了
                    FinishCurrentLine();
                }
            }
        }
        else
        {
            if (currentLineData != null)
            {
                FinishCurrentLine();
            }
        }
    }

    private bool IsMouseOnMirror(Vector2 mouseWorldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, mirrorLayer);
        return hit.collider != null;
    }

    private void StartNewLine(Vector2 startPos)
    {
        LineRenderer newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);

        EdgeCollider2D edge = newLine.GetComponent<EdgeCollider2D>();

        LineData lineData = new LineData
        {
            lineRenderer = newLine,
            edgeCollider = edge
        };

        activeLines.Add(lineData);
        currentLineData = lineData;

        AddPoint(currentLineData, startPos);
        RefreshLineVisual(currentLineData);
    }

    private void FinishCurrentLine()
    {
        currentLineData = null;
    }

    private void AddPoint(LineData lineData, Vector3 worldPos)
    {
        TimedPoint point = new TimedPoint
        {
            worldPos = worldPos,
            createTime = Time.time
        };

        lineData.points.Add(point);
    }

    private void RefreshLineVisual(LineData lineData)
    {
        int pointCount = lineData.points.Count;

        lineData.lineRenderer.positionCount = pointCount;

        for (int i = 0; i < pointCount; i++)
        {
            lineData.lineRenderer.SetPosition(i, lineData.points[i].worldPos);
        }

        if (lineData.edgeCollider == null)
            return;

        if (pointCount >= 2)
        {
            List<Vector2> localPoints = new List<Vector2>(pointCount);

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 localPos3 = lineData.lineRenderer.transform.InverseTransformPoint(lineData.points[i].worldPos);
                localPoints.Add(new Vector2(localPos3.x, localPos3.y));
            }

            lineData.edgeCollider.enabled = true;
            lineData.edgeCollider.SetPoints(localPoints);
        }
        else
        {
            lineData.edgeCollider.enabled = false;
        }
    }

    private void UpdateAllLinesLife()
    {
        float now = Time.time;

        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            LineData lineData = activeLines[i];

            while (lineData.points.Count > 0 &&
                   now - lineData.points[0].createTime >= pointLifeTime)
            {
                lineData.points.RemoveAt(0);
            }

            if (lineData.points.Count == 0)
            {
                if (currentLineData == lineData)
                {
                    currentLineData = null;
                }

                if (lineData.lineRenderer != null)
                {
                    Destroy(lineData.lineRenderer.gameObject);
                }

                activeLines.RemoveAt(i);
                continue;
            }

            RefreshLineVisual(lineData);
        }
    }
}