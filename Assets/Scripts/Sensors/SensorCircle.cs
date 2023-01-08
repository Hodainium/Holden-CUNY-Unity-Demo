using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorCircle : SensorObject
{
    [Header("Options")]

    [Tooltip("Raycast every N FixedUpdate iteration frame (1 = default) (Higher = more performant but raycasting is less frequent and thus less accurate)")]
    [Range(1, 8)]
    public int raycastRate = 1;
    

    [Header("How many sensor points along circle")]
    public int sensorAmount = 8;
    public bool playOnStart = true;

    [Space(8f)]

    [Header("Raycast methods")]
    public bool _allRaycasts = false;
    public bool _gridRaycasts = false;
    public bool _outlineRaycasts = true;
    public bool _centerRaycasts = true;
    //not these
    public bool _pastRaycastsCross = true;
    public bool _pastRaycastsX = true;
    public bool _pastRaycastsAll = false;
    //---------
    public bool _HDMultiplier = true;
    public float HDValueMultiplier = 2;


    [Header("Hit handling options")]
    [Tooltip("Saves resources by stopping raycasting instantly after the first hit")]
    public bool stopAfterFirstHit = false;

    [Header("References")]
    [SerializeField] public Transform centerParentTransform;
    public Transform radiusTransform;
    public bool useRadiusTransform = false;
    public float radiusManual = 1;

    [Header("Debug")]
    [Tooltip("Display raycasts in editor (runtime)")]
    public bool showDebugRays = true;

    [Tooltip("Frame count: Don't edit")]
    public int frames = 0;

    private float debugRayLifetime = 0.4f;
    private Vector2[] sensors, sensorsCenterHD;
    private Vector2[] sensorsLastPositions, sensorsCrossLastPositions, sensorsXLastPositions;

    private RaycastHit2D[] hits;
    private enum RaycastType { Horizontal, Vertical, Center, IntersectionBackward, Past };

    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();

    [HideInInspector] public bool playing = false;

    float _radius;

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
        _radius = radiusManual;

        InitializeSensors(); //Perform sensor initilization in here

        //Reset lastPositions
        //check if all raycasts FIRST, if not then check if just corners
        if (_pastRaycastsAll)
        {
            for (int i = 0; i < sensorsLastPositions.Length; i++)
            {
                sensorsLastPositions[i] = GetTransformPoint(sensors[i]);
            }
        }
        else
        {
            int crossPointOffset = sensors.Length / 4; 
            int xPointOffset = sensors.Length / 8; 

            if (_pastRaycastsCross)
            {

                for (int i = 0; i < sensorsCrossLastPositions.Length; i++)
                {
                    sensorsCrossLastPositions[i] = GetTransformPoint(sensors[i * crossPointOffset]);
                }
            }
            if (_pastRaycastsX)
            {
                for (int i = 0; i < sensorsXLastPositions.Length; i++)
                {
                    sensorsXLastPositions[i] = GetTransformPoint(sensors[xPointOffset + (i * crossPointOffset)]);
                }
            }
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
        sensors = new Vector2[sensorAmount]; //points of circle
        if (_pastRaycastsAll)
        {
            sensorsLastPositions = new Vector2[sensorAmount]; //lines of rectangle
        }
        if (_pastRaycastsCross)
        {
            sensorsCrossLastPositions = new Vector2[4]; //just the corners
        }
        if (_pastRaycastsX)
        {
            sensorsXLastPositions = new Vector2[4];
        }
        if (_centerRaycasts)
        {
            sensorsCenterHD = new Vector2[(int)(sensorAmount * HDValueMultiplier)];
        }
        

        //Create extra sensors along X's
        float d = 1f / (sensorAmount); 
        d *= 2*Mathf.PI; //multiply d by 2pi radians
        float radianIncrement = 0f;


        for (int i = 0; i < sensorAmount; i++)
        {
            sensors[i].x = _radius * Mathf.Cos(radianIncrement);
            sensors[i].y = _radius * Mathf.Sin(radianIncrement);

            radianIncrement += d;
        }

        d *= 1/HDValueMultiplier; //multiply d by 2pi radians
        radianIncrement = 0f;


        for (int i = 0; i < sensorAmount*HDValueMultiplier; i++)
        {
            sensorsCenterHD[i].x = _radius * Mathf.Cos(radianIncrement);
            sensorsCenterHD[i].y = _radius * Mathf.Sin(radianIncrement);

            radianIncrement += d;
        }

    }

    /// <summary>
    /// Returns position relative to target parent transform 
    /// </summary>
    private Vector2 GetTransformPoint(Vector2 v)
    {
        return centerParentTransform.TransformPoint(v);
    }

    private void RaycastProcedure()
    {
        Vector2 currentPositionFrom, currentPositionTo;
        Vector2 currentCenterTransformPosition = centerParentTransform.position;

        if (_outlineRaycasts) //points to next in array, doesn't oob
        {
            for (int i = 0; i < sensors.Length-1; i++)
            {
                currentPositionFrom = GetTransformPoint(sensors[sensors.Length - i - 1]); //7->1
                currentPositionTo = GetTransformPoint(sensors[sensors.Length - i - 2]); //6->0
                Raycast(currentPositionFrom, currentPositionTo);
            }
            currentPositionFrom = GetTransformPoint(sensors[0]);
            currentPositionTo = GetTransformPoint(sensors[sensors.Length - 1]);
            Raycast(currentPositionFrom, currentPositionTo);
        }
        if (_centerRaycasts)
        {
            for (int i = 0; i < sensors.Length; i++) //implement so that you can increase center slice resolution, multiply centercasts
            {
                currentPositionFrom = currentCenterTransformPosition;
                currentPositionTo = GetTransformPoint(sensors[i]);
                Raycast(currentPositionFrom, currentPositionTo, RaycastType.Center);
            }
        }
        if (_HDMultiplier)
        {
            for (int i = 0; i < sensorsCenterHD.Length; i++) //implement so that you can increase center slice resolution, multiply centercasts
            {
                currentPositionFrom = currentCenterTransformPosition;
                currentPositionTo = GetTransformPoint(sensorsCenterHD[i]);
                Raycast(currentPositionFrom, currentPositionTo, RaycastType.Center);
            }
        }


        ////Vertical
        //if (verticalRaycasts)
        //    Raycast(currentPositionX1, currentPositionX2, RaycastType.Vertical);

        ////Past corners ALL
        //if (pastRaycastsAll)
        //{
        //    Raycast(lastPositionsX1[i], currentPositionX1, RaycastType.Past);
        //    lastPositionsX1[i] = currentPositionX1;

        //    Raycast(lastPositionsX2[i], currentPositionX2, RaycastType.Past);
        //    lastPositionsX2[i] = currentPositionX2;
        //}




        //if (!pastRaycastsAll && pastRaycastsCorners) //Do past raycasts for corners
        //{
        //    //////X
        //    currentPositionX1 = GetTransformPoint(sensorsX1[0]);
        //    Raycast(lastPositionsX1[0], currentPositionX1, RaycastType.Past);
        //    lastPositionsX1[0] = currentPositionX1;

        //    currentPositionX1 = GetTransformPoint(sensorsX1[sensorCountHorizontalX - 1]);
        //    Raycast(lastPositionsX1[1], currentPositionX1, RaycastType.Past);
        //    lastPositionsX1[1] = currentPositionX1;

        //    currentPositionX2 = GetTransformPoint(sensorsX2[0]);
        //    Raycast(lastPositionsX2[0], currentPositionX2, RaycastType.Past);
        //    lastPositionsX2[0] = currentPositionX2;

        //    currentPositionX2 = GetTransformPoint(sensorsX2[sensorCountHorizontalX - 1]);
        //    Raycast(lastPositionsX2[1], currentPositionX2, RaycastType.Past);
        //    lastPositionsX2[1] = currentPositionX2;
        //}

        //if (intersectionRaycastsForward)
        //{
        //    Raycast(GetTransformPoint(sensorsX2[0]), GetTransformPoint(sensorsX1[sensorCountHorizontalX - 1]), RaycastType.IntersectionForward);
        //}

        //if (intersectionRaycastsBackward)
        //{
        //    Raycast(GetTransformPoint(sensorsX1[0]), GetTransformPoint(sensorsX2[sensorCountHorizontalX - 1]), RaycastType.IntersectionBackward);
        //}
    }


    private void Raycast(Vector2 from, Vector2 to, RaycastType rayType = RaycastType.Vertical)
    {
        bool hitDetected = false;

        hits = Physics2D.LinecastAll(from, to);

        //Iterate results
        foreach (RaycastHit2D hit in hits)
        {
            //Check if collider exists and is not the collider attached to target transform (self)
            if (hit.collider != null && hit.collider.transform != centerParentTransform)
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

                case RaycastType.Center:
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
        if (useRadiusTransform == false)
        {
            _radius = radiusManual;
            Gizmos.DrawWireSphere(centerParentTransform.position, _radius);
        }
        else
        {
            //use radius transform
        }

        

        //if (Application.isPlaying)
        //{
        //    Gizmos.color = Color.green;
        //    for (int i = 0; i < sensorCountHorizontalX; i++)
        //    {
        //        Gizmos.DrawWireSphere(GetTransformPoint(sensorsX1[i]), 0.075f);
        //        Gizmos.DrawWireSphere(GetTransformPoint(sensorsX2[i]), 0.075f);
        //    }
        //    for (int i = 0; i < sensorCountVerticalY; i++)
        //    {
        //        Gizmos.DrawWireSphere(GetTransformPoint(sensorsY1[i]), 0.075f);
        //        Gizmos.DrawWireSphere(GetTransformPoint(sensorsY2[i]), 0.075f);
        //    }

        //    for (int i = 0; i < sensorsX1.Length; i++) //X points and verticals
        //    {

        //        //Vertical
        //        if (verticalRaycasts)
        //        {
        //            Gizmos.DrawLine(sensorsX1[i], sensorsX2[i]);
        //        }
        //    }

        //}

        //if (topleft != null && topright != null && bottomleft != null && bottomright != null)
        //{
        //    Gizmos.color = Color.magenta;
        //    Gizmos.DrawWireSphere(topleft.position, 0.1f);
        //    Gizmos.DrawWireSphere(topright.position, 0.1f);
        //    Gizmos.DrawWireSphere(bottomleft.position, 0.1f);
        //    Gizmos.DrawWireSphere(bottomright.position, 0.1f);
        //    //Gizmos.color = Color.magenta;
        //    Gizmos.DrawLine(topleft.position, topright.position);
        //    Gizmos.DrawLine(topleft.position, bottomleft.position);
        //    Gizmos.DrawLine(bottomleft.position, bottomright.position);
        //    Gizmos.DrawLine(bottomright.position, topright.position);

        //    Gizmos.DrawLine(topleft.position, bottomright.position);
        //    Gizmos.DrawLine(bottomleft.position, topright.position);

        //}


    }
    #endregion
}
