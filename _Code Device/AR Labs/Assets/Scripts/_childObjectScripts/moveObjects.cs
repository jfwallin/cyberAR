



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// moves a gameObject from an initial location, size and oriention to its final locaiton, size and orientation
// written by J. Wallin 2021

/*

Example use:

            // set the positions for the move
            sortData[i].theObject.GetComponent<moveObjects>().StartPos = sortData[i].theObject.transform.position;
            sortData[i].theObject.GetComponent<moveObjects>().MidPos = new Vector3(0.001f, 0.001f, 0.001f);
            sortData[i].theObject.GetComponent<moveObjects>().FinalPos = new Vector3(0.001f, 0.001f, 0.001f);

            // set the sizes
            sortData[i].theObject.GetComponent<moveObjects>().StartSize = sortData[i].theObject.transform.localScale;
            sortData[i].theObject.GetComponent<moveObjects>().FinalSize = sortData[i].theObject.transform.localScale;

            // set the angles
            sortData[i].theObject.GetComponent<moveObjects>().StartAngle = sortData[i].theObject.transform.eulerAngles;
            sortData[i].theObject.GetComponent<moveObjects>().FinalAngle = sortData[i].theObject.transform.eulerAngles;

            // set the time range - starts at game time 20 and ends at game time 25
            sortData[i].theObject.GetComponent<moveObjects>().TimeRange = new Vector2(20.0f,  25.0f);

            // this would disable the move since the times are before the game started
            // sortData[i].theObject.GetComponent<moveObjects>().TimeRange = new Vector2(-100.0f, -90.0f);
 
*/

public class moveObjects : MonoBehaviour
{

    /*
    private float deltaT;
    private float timeStart ;
    private float timeEnd;
    */

    #region Properties 
    public Vector3 startPos = new Vector3(-5.0f, -10.0f, -10.0f);
    public Vector3 StartPos
    {
        get { return startPos; }
        set { startPos = (Vector3)value; }
    }

    public Vector3 midPos = new Vector3(1.0f, -10.0f, -10.0f);
    public Vector3 MidPos
    {
        get { return midPos; }
        set { midPos = value; }
    }

    public Vector3 finalPos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 FinalPos
    {
        get { return finalPos; }
        set { finalPos = value; }
    }


    public Vector3 startSize = new Vector3(0.001f, 0.001f, 0.001f);
    public Vector3 StartSize
    {
        get { return startSize; }
        set { startSize = value; }
    }
    public Vector3 finalSize = new Vector3(3.0f, 3.0f, 3.0f);
    public Vector3 FinalSize
    {
        get { return finalSize; }
        set { finalSize = value; }
    }

    public Vector3 startAngle = new Vector3(-1000.0f, -1000.0f, -1000.0f);
    public Vector3 StartAngle
    {
        get { return startAngle; }
        set { startAngle = value; }
    }

    public Vector3 finalAngle = new Vector3(0.0f, 180.0f, 0.0f);
    public Vector3 FinalAngle
    {
        get { return finalAngle; }
        set { finalAngle = value; }
    }

    public Vector2 timeRange = new Vector3(-100.0f, -90.0f);
    public Vector2 TimeRange
    {
        get { return timeRange; }
        set { timeRange = value; }
    }

    #endregion // Properties 

    #region Variables
    private static int ndim = 3;
    public float[,] positionCoefficients = new float[ndim, ndim];
    public float[,] sizeCoefficients = new float[ndim, ndim];
    public float[,] angleCoefficients = new float[ndim, ndim];

    private MagicLeapTools.InputReceiver _inputReceiver;
    public bool enableHandleDrag = true;

    #endregion // Variables

    #region Private Methods
    private void Awake()
    {
        _inputReceiver = GetComponent<MagicLeapTools.InputReceiver>();
        if (_inputReceiver == null)
            Debug.Log("input receiver not found");

    }

    private void OnEnable()
    {
        if (enableHandleDrag)
            _inputReceiver.OnDragEnd.AddListener(HandleOnClick);

    }

    private void OnDisable()
    {
        if (enableHandleDrag)
            _inputReceiver.OnDragEnd.RemoveListener(HandleOnClick);

    }

    private void HandleOnClick(GameObject sender)
    {

        GameObject sorter = GameObject.Find("sortingManager");

        if (sorter != null)
            sorter.GetComponent<sortingActivity>().resort();

    }


    #endregion // private functions




    #region Public Functions
    // Use this for initialization
    void Start()
    {

        initializePath();
    }

    public void initializePath()
    {

        positionCoefficients = threeVectorQuadratic(startPos, midPos, finalPos, timeRange);
        sizeCoefficients = twoVectorLinear(startSize, finalSize, timeRange);
        angleCoefficients = twoVectorLinear(startAngle, finalAngle, timeRange);
    }

    private static Vector3 calcLocation(float[,] coefficients, float myTime)
    {
        Vector3 location;
        float[] xx = new float[3];


        for (int k = 0; k < ndim; k++)
        {
            xx[k] = coefficients[k, 0] * myTime * myTime + coefficients[k, 1] * myTime + coefficients[k, 2];
        }

        location = new Vector3(xx[0], xx[1], xx[2]);
        return location;
    }

    private static float[,] twoVectorLinear(Vector3 s, Vector3 f, Vector2 timeRange)
    {

        float[] x = new float[ndim];
        float[] y = new float[ndim];
        float[,] parabolaCoeffients = new float[ndim, ndim];

        float dy, dx, slope, intercept;

        x[0] = 0.0f;
        x[1] = timeRange[1] - timeRange[0];
        dx = x[1];

        //x[0] = timeRange[0];
        //x[1] = timeRange[1];
        //dx = timeRange[1] - timeRange[0];

        for (int k = 0; k < ndim; k++)
        {
            y[0] = s[k];
            y[1] = f[k];
            dy = y[1] - y[0];

            slope = dy / dx;
            intercept = y[0] - slope * x[0];

            parabolaCoeffients[k, 0] = 0.0f;
            parabolaCoeffients[k, 1] = slope;
            parabolaCoeffients[k, 2] = intercept;
        }


        return parabolaCoeffients;
    }



    private static float[,] threeVectorQuadratic(Vector3 s, Vector3 m, Vector3 f, Vector2 timeRange)
    {
        // does a quadratic fit to 3 points in 3d space
        // it returns the coefficients for three parabola


        float[] x = new float[ndim];
        float[] y = new float[ndim];

        float[,] aa = new float[ndim, ndim];
        float det1, det2, det3, det;
        float[,] cof = new float[ndim, ndim];
        float[,] matinv = new float[ndim, ndim];
        float[,] parabolaCoeffients = new float[ndim, ndim];

        x[0] = 0.0f;
        x[1] = 0.5f * (timeRange[1] - timeRange[0]);
        x[2] = 2 * x[1];

        //x[0] = timeRange[0];
        //x[1] = 0.5f * (timeRange[0] + timeRange[1]);
        //x[2] = timeRange[1];

        for (int k = 0; k < ndim; k++)
        {
            y[0] = s[k];
            y[1] = m[k];
            y[2] = f[k];

            // form the matrix we need to invert
            for (int i = 0; i < ndim; i++)
            {
                for (int j = 0; j < ndim; j++)
                {
                    aa[0, i] = x[i] * x[i];
                    aa[1, i] = x[i];
                    aa[2, i] = 1;
                }
            }

            // calculate the determinant
            det1 = aa[0, 0] * (aa[1, 1] * aa[2, 2] - aa[2, 1] * aa[1, 2]);
            det2 = aa[0, 1] * (aa[1, 0] * aa[2, 2] - aa[2, 0] * aa[1, 2]);
            det3 = aa[0, 2] * (aa[1, 0] * aa[2, 1] - aa[2, 0] * aa[1, 1]);
            det = det1 - det2 + det3;

            // calculate the cofactors
            cof[0, 0] = (aa[1, 1] * aa[2, 2] - aa[2, 1] * aa[1, 2]);
            cof[0, 1] = -(aa[1, 0] * aa[2, 2] - aa[2, 0] * aa[1, 2]);
            cof[0, 2] = (aa[1, 0] * aa[2, 1] - aa[2, 0] * aa[1, 1]);

            cof[1, 0] = -(aa[0, 1] * aa[2, 2] - aa[2, 1] * aa[0, 2]);
            cof[1, 1] = (aa[0, 0] * aa[2, 2] - aa[2, 0] * aa[0, 2]);
            cof[1, 2] = -(aa[0, 0] * aa[2, 1] - aa[2, 0] * aa[0, 1]);

            cof[2, 0] = (aa[0, 1] * aa[1, 2] - aa[1, 1] * aa[0, 2]);
            cof[2, 1] = -(aa[0, 0] * aa[1, 2] - aa[1, 0] * aa[0, 2]);
            cof[2, 2] = (aa[0, 0] * aa[1, 1] - aa[1, 0] * aa[0, 1]);

            // form the matrix we need to invert
            for (int i = 0; i < ndim; i++)
            {
                for (int j = 0; j < ndim; j++)
                {
                    matinv[i, j] = cof[j, i] / det;
                }
            }

            for (int i = 0; i < ndim; i++)
            {
                parabolaCoeffients[k, i] = matinv[0, i] * y[0] + matinv[1, i] * y[1] + matinv[2, i] * y[2];
            }
        }


        return parabolaCoeffients;

    }




    // Update is called once per frame
    void Update()
    {

        float myTime;
        Vector3 angles;
        if (Time.time > timeRange[0] && Time.time < timeRange[1])
        {

            myTime = Time.time - timeRange[0];
            //myTime = Time.time;

            transform.localPosition = calcLocation(positionCoefficients, myTime);
            angles = calcLocation(angleCoefficients, myTime);
            transform.eulerAngles = angles;
            transform.localScale = calcLocation(sizeCoefficients, myTime);
        }

    }

    #endregion // Public Functions
}