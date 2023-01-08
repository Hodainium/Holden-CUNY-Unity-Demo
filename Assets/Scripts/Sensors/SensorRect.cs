using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorRect : SensorObject
{
    [Header("Options")]

    [Tooltip("Raycast every N FixedUpdate iteration frame (1 = default) (Higher = more performant but raycasting is less frequent and thus less accurate)")]
    [Range(1, 8)]
    public int raycastRate = 1;

    [Header("How many sensor points along each line")]
    [Tooltip("sensorCountHorizontalX")]
    public int sensorCountHorizontalX = 3;
    [Tooltip("sensorCountVerticalY")]
    public int sensorCountVerticalY = 3;
    public bool playOnStart = true;

    [Space(8f)]

    [Header("Raycast methods")]
    public bool horizontalRaycasts = true;
    public bool verticalRaycasts = true;
    public bool intersectionRaycastsForward = true;
    public bool intersectionRaycastsBackward = true;
    public bool pastRaycastsCorners = true;
    public bool pastRaycastsAll = false;


    [Header("Hit handling options")]
    [Tooltip("Saves resources by stopping raycasting instantly after the first hit")]
    public bool stopAfterFirstHit = false;

    [Header("References")]
    public Transform targetParentTransform;
    public Transform topleft;
    public Transform topright;
    public Transform bottomleft;
    public Transform bottomright;

    [Header("Debug")]
    [Tooltip("Display raycasts in editor (runtime)")]
    public bool showDebugRays = true;

    [Tooltip("Frame count: Don't edit")]
    public int frames = 0;

    private float debugRayLifetime = 0.4f;
    private Vector2[] sensors, sensorsX1, sensorsX2, sensorsY1, sensorsY2;
    private Vector2[] lastPositions, lastPositionsX1, lastPositionsX2, lastPositionsY1, lastPositionsY2;

    private RaycastHit2D[] hits;
    private enum RaycastType { Horizontal, Vertical, IntersectionForward, IntersectionBackward, Past };

    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();

    [HideInInspector] public bool playing = false;

    private void Awake()
    {
        //InitializeSensors(); TEST just put it in Play()
        //sensorLogic = targetParentTransform.GetComponent<SensorLogic>();

        if (sensorLogic == null)
        {
            Stop();
            Debug.LogError("Sensor cannot see sensorLogic and is not bound. Stopping sensor.");
        }
        else
        {
            if (playOnStart)
            Play();
        }
    }

    /// <summary>
    /// Resets hit position and starts raycasting
    /// </summary>
    public override void Play() //Only gets call on Awake()
    {
        InitializeSensors(); //Perform sensor initilization in here

        //Reset lastPositions
        //check if all raycasts FIRST, if not then check if just corners
        if (pastRaycastsAll)
        {
            //X's
            for (int i = 0; i < lastPositionsX1.Length; i++)
            {
                lastPositionsX1[i] = GetTransformPoint(sensorsX1[i]);
                lastPositionsX2[i] = GetTransformPoint(sensorsX2[i]);
            }

            //Y's
            for (int i = 0; i < lastPositionsY1.Length; i++)
            {
                lastPositionsY1[i] = GetTransformPoint(sensorsY1[i]);
                lastPositionsY2[i] = GetTransformPoint(sensorsY2[i]);
            }
        }
        else if (pastRaycastsCorners)
        {
            lastPositionsX1[0] = GetTransformPoint(sensorsX1[0]);
            lastPositionsX2[1] = GetTransformPoint(sensorsX2[sensorCountHorizontalX-1]);

            lastPositionsY1[0] = GetTransformPoint(sensorsY1[0]);
            lastPositionsY2[1] = GetTransformPoint(sensorsY2[sensorCountVerticalY - 1]);
        }

        //Reset hitObjects
        hitObjects.Clear();

        playing = true;
    }

    /// <summary>
    /// Stops the raycasting
    /// </summary>
    public void Stop()
    {
        playing = false;
    }

    
    private void FixedUpdate()
    {
        if (!playing)
            return;

        frames++;
        if (frames % raycastRate != 0)
            return;

        frames = 0;

        RaycastProcedure();
    }

    /// <summary>
    /// Initializes sensor positions
    /// </summary>
    public override void InitializeSensors()
    {
        sensorsX1 = new Vector2[sensorCountHorizontalX]; //lines of rectangle
        sensorsX2 = new Vector2[sensorCountHorizontalX];
        sensorsY1 = new Vector2[sensorCountVerticalY];
        sensorsY2 = new Vector2[sensorCountVerticalY];
        if (pastRaycastsAll)
        {
            lastPositionsX1 = new Vector2[sensorCountHorizontalX]; //lines of rectangle
            lastPositionsX2 = new Vector2[sensorCountHorizontalX];
            lastPositionsY1 = new Vector2[sensorCountVerticalY];
            lastPositionsY2 = new Vector2[sensorCountVerticalY];
        }
        else if (pastRaycastsCorners)
        {
            lastPositionsX1 = new Vector2[2]; //just the corners
            lastPositionsX2 = new Vector2[2];
            lastPositionsY1 = new Vector2[2];
            lastPositionsY2 = new Vector2[2];
        }
        else
        {
            lastPositionsX1 = null; //set all to null because it won't be checked
            lastPositionsX2 = null;
            lastPositionsY1 = null;
            lastPositionsY2 = null;
        }

        //Create extra sensors along X's
        float d = 1f / (sensorCountHorizontalX - 1);
        float lerpValue = 0f;

        for (int i = 0; i < sensorCountHorizontalX; i++)
        {
            sensorsX1[i] = Vector2.Lerp(topleft.localPosition, topright.localPosition, lerpValue); //top line: topleft to topright
            sensorsX2[i] = Vector2.Lerp(bottomleft.localPosition, bottomright.localPosition, lerpValue); //bottom line

            lerpValue += d;
        }

        //Now do the Y's
        d = 1f / (sensorCountVerticalY - 1);
        lerpValue = 0f;

        for (int i = 0; i < sensorCountVerticalY; i++)
        {
            sensorsY1[i] = Vector2.Lerp(topleft.localPosition, bottomleft.localPosition, lerpValue); //leftline: topleft to bottomleft
            sensorsY2[i] = Vector2.Lerp(topright.localPosition, bottomright.localPosition, lerpValue); //right line

            lerpValue += d;
        }
    }

    /// <summary>
    /// Returns position relative to target parent transform 
    /// </summary>
    private Vector2 GetTransformPoint(Vector2 v)
    {
        return targetParentTransform.TransformPoint(v);
    }

    private void RaycastProcedure()
    {
        Vector2 currentPositionX1, currentPositionX2;

        for (int i = 0; i < sensorsX1.Length; i++) //X points and verticals
        {
            currentPositionX1 = GetTransformPoint(sensorsX1[i]);
            currentPositionX2 = GetTransformPoint(sensorsX2[i]);

            //Vertical
            if (verticalRaycasts)
                Raycast(currentPositionX1, currentPositionX2, RaycastType.Vertical);

            //Past corners ALL
            if (pastRaycastsAll)
            {
                Raycast(lastPositionsX1[i], currentPositionX1, RaycastType.Past);
                lastPositionsX1[i] = currentPositionX1;

                Raycast(lastPositionsX2[i], currentPositionX2, RaycastType.Past);
                lastPositionsX2[i] = currentPositionX2;
            }
        }

        Vector2 currentPositionY1, currentPositionY2;

        for (int i = 0; i < sensorsY1.Length; i++) //Y points and horizontals
        {
            currentPositionY1 = GetTransformPoint(sensorsY1[i]);
            currentPositionY2 = GetTransformPoint(sensorsY2[i]);

            //Horizontal
            if (horizontalRaycasts)
                Raycast(currentPositionY1, currentPositionY2, RaycastType.Horizontal);

            //Past corners ALL
            if (pastRaycastsAll)
            {
                Raycast(lastPositionsY1[i], currentPositionY1, RaycastType.Past);
                lastPositionsY1[i] = currentPositionY1;

                Raycast(lastPositionsY2[i], currentPositionY2, RaycastType.Past);
                lastPositionsY2[i] = currentPositionY2;
            }
        }

        if (!pastRaycastsAll && pastRaycastsCorners) //Do past raycasts for corners
        {
            //////X
            currentPositionX1 = GetTransformPoint(sensorsX1[0]);
            Raycast(lastPositionsX1[0], currentPositionX1, RaycastType.Past);
            lastPositionsX1[0] = currentPositionX1;

            currentPositionX1 = GetTransformPoint(sensorsX1[sensorCountHorizontalX - 1]);
            Raycast(lastPositionsX1[1], currentPositionX1, RaycastType.Past);
            lastPositionsX1[1] = currentPositionX1;

            currentPositionX2 = GetTransformPoint(sensorsX2[0]);
            Raycast(lastPositionsX2[0], currentPositionX2, RaycastType.Past);
            lastPositionsX2[0] = currentPositionX2;

            currentPositionX2 = GetTransformPoint(sensorsX2[sensorCountHorizontalX - 1]);
            Raycast(lastPositionsX2[1], currentPositionX2, RaycastType.Past);
            lastPositionsX2[1] = currentPositionX2;

            /////////////////////////////////////

            /////Y
            currentPositionY1 = GetTransformPoint(sensorsY1[0]);
            Raycast(lastPositionsY1[0], currentPositionY1, RaycastType.Past);
            lastPositionsY1[0] = currentPositionY1;

            currentPositionY1 = GetTransformPoint(sensorsY1[sensorCountVerticalY - 1]);
            Raycast(lastPositionsY1[1], currentPositionY1, RaycastType.Past);
            lastPositionsY1[1] = currentPositionY1;

            currentPositionY2 = GetTransformPoint(sensorsY2[0]);
            Raycast(lastPositionsY2[0], currentPositionY2, RaycastType.Past);
            lastPositionsY2[0] = currentPositionY2;

            currentPositionY2 = GetTransformPoint(sensorsY2[sensorCountVerticalY - 1]);
            Raycast(lastPositionsY2[1], currentPositionY2, RaycastType.Past);
            lastPositionsY2[1] = currentPositionY2;
        }

        if (intersectionRaycastsForward)
        {
            Raycast(GetTransformPoint(sensorsX2[0]), GetTransformPoint(sensorsX1[sensorCountHorizontalX - 1]), RaycastType.IntersectionForward);
        }

        if (intersectionRaycastsBackward)
        {
            Raycast(GetTransformPoint(sensorsX1[0]), GetTransformPoint(sensorsX2[sensorCountHorizontalX - 1]), RaycastType.IntersectionBackward);
        }
    }

    
    private void Raycast(Vector2 from, Vector2 to, RaycastType rayType)
    {
        bool hitDetected = false;

        hits = Physics2D.LinecastAll(from, to);

        //Iterate results
        foreach (RaycastHit2D hit in hits)
        {
            //Check if collider exists and is not the collider attached to target transform (self)
            if (hit.collider != null && hit.collider.transform != targetParentTransform)
            {
                hitDetected = true;
                HandleHit(hit);
            }
        }

        #region Debug rays
        if (showDebugRays)
        {
            Color lineColor = Color.white;

            switch (rayType)
            {
                case RaycastType.Horizontal:
                    lineColor = Color.white;
                    break;

                case RaycastType.Vertical:
                    lineColor = Color.cyan;
                    break;

                case RaycastType.IntersectionForward:
                    lineColor = Color.magenta;
                    break;

                case RaycastType.IntersectionBackward:
                    lineColor = Color.yellow;
                    break;

                case RaycastType.Past:
                    lineColor = Color.blue;
                    break;
            }

            //Red whenever it hits something
            Debug.DrawLine(from, to, hitDetected ? Color.red : lineColor, debugRayLifetime);
        }
        #endregion

    }

    private void HandleHit(RaycastHit2D hit)
    {
        //Ignore objects that have already been hit
        if (hitObjects.Contains(hit.collider))
            return;
        else
            hitObjects.Add(hit.collider);

        //Debug.Log("Hit detected! gameObject's name: " + hit.collider.gameObject.name);        

        if (showDebugRays)
        {
            //Draw a + symbol on hit point
            Debug.DrawRay(hit.point + new Vector2(0, 0.2f), Vector2.down * 0.4f, Color.red, debugRayLifetime * 1.5f);
            Debug.DrawRay(hit.point + new Vector2(-0.2f, 0), Vector2.right * 0.4f, Color.red, debugRayLifetime * 1.5f);
        }

        //////////////////////////////////////////////////////////////////
        // You probably want to implement a interface or something here //
        //////////////////////////////////////////////////////////////////

        sensorLogic.SendHit(hit);

        
        

        if (stopAfterFirstHit)
            Stop();

        
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            for(int i = 0; i < sensorCountHorizontalX; i++)
            {
                Gizmos.DrawWireSphere(GetTransformPoint(sensorsX1[i]), 0.075f);
                Gizmos.DrawWireSphere(GetTransformPoint(sensorsX2[i]), 0.075f);
            }
            for (int i = 0; i < sensorCountVerticalY; i++)
            {
                Gizmos.DrawWireSphere(GetTransformPoint(sensorsY1[i]), 0.075f);
                Gizmos.DrawWireSphere(GetTransformPoint(sensorsY2[i]), 0.075f);
            }

            for (int i = 0; i < sensorsX1.Length; i++) //X points and verticals
            {

                //Vertical
                if (verticalRaycasts)
                {
                    Gizmos.DrawLine(sensorsX1[i], sensorsX2[i]);
                }
            }

        }

        if (topleft != null && topright != null && bottomleft != null && bottomright != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(topleft.position, 0.1f);
            Gizmos.DrawWireSphere(topright.position, 0.1f);
            Gizmos.DrawWireSphere(bottomleft.position, 0.1f);
            Gizmos.DrawWireSphere(bottomright.position, 0.1f);
            //Gizmos.color = Color.magenta;
            Gizmos.DrawLine(topleft.position, topright.position);
            Gizmos.DrawLine(topleft.position, bottomleft.position);
            Gizmos.DrawLine(bottomleft.position, bottomright.position);
            Gizmos.DrawLine(bottomright.position, topright.position);

            Gizmos.DrawLine(topleft.position, bottomright.position);
            Gizmos.DrawLine(bottomleft.position, topright.position);

        }
    }
    #endregion
}
