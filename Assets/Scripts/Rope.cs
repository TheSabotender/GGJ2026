using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    [Range(0, 1)] public float Elasticity;
    public int segmentCount = 35;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.25f;
    private float currentSegLen;

    private LineRenderer LineRenderer
    {
        get {
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
            return lineRenderer;
        }
    }

    private void OnEnable()
    {
        LineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        LineRenderer.enabled = false;
    }

    // Use this for initialization
    void Start()
    {
        currentSegLen = ropeSegLen;
        for (int i = 0; i < segmentCount; i++)
        {
            this.ropeSegments.Add(new RopeSegment(StartPoint));
            StartPoint.y -= ropeSegLen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        UpdateSegmentLength();

        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1f);

        for (int i = 1; i < this.segmentCount; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.StartPoint;
        this.ropeSegments[0] = firstSegment;


        //Constrant to Second Point 
        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        endSegment.posNow = this.EndPoint;
        this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < this.segmentCount - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.currentSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > currentSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < currentSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    private void UpdateSegmentLength()
    {
        float desiredSegLen = Vector3.Distance(StartPoint, EndPoint) / (segmentCount - 1);
        if (desiredSegLen > currentSegLen)
        {
            currentSegLen = desiredSegLen;
        }
        else if (Elasticity > 0f)
        {
            currentSegLen = Mathf.Lerp(currentSegLen, desiredSegLen, Elasticity * Time.fixedDeltaTime);
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[this.segmentCount];
        for (int i = 0; i < this.segmentCount; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow;
        }

        LineRenderer.positionCount = ropePositions.Length;
        LineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
