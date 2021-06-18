
using UnityEngine;
using System.Collections;
using System;



public class placeObjects 
{
    public Vector3[] ptLocation;
    private int maxObjects;
    public float distanceBetweenObjects;
    public int nObjects;
    private int nx, ny;
    private float height, width;
   
    public placeObjects(int k, float h, float w)
    {
        nObjects = k;
        height = h;
        width = w;
        float number = (float)k;
        float xx = Mathf.Sqrt(number);
        
        // set the dimensions equal to the integer of the square root
        // this is the minimum size we need
        nx = (int)xx;
        ny = nx;

        // if the number of positions is too small, expand the x count by one
        if (nx * ny < nObjects)
            nx = nx + 1;

        // if the total number of positions is still too small, expand y as well
        if (nx * ny < nObjects)
            ny = ny + 1;

        maxObjects = nx * ny; 
        ptLocation = new Vector3[maxObjects];
        float a;
        int b;
        (b, a) = nearestObject(Vector3.zero);
        Debug.Log("check " + a.ToString() + "   " + b.ToString());
    }

    public Tuple <int,float> nearestObject(Vector3 pos)
    {
        int nearest;
        float distance;
        nearest = -1;
        distance = (float)1e10;
        float currentDistance;

        for(int i = 0; i < nObjects; i++ )
        {
            currentDistance = Vector3.Distance(pos, ptLocation[i]);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                nearest = i;
            }
        }


        return Tuple.Create(nearest, distance);
    }

    public void createRing(bool horizontal)
    {
        float a, b;
        float x, y;
        float dTheta;

        // place the central point

        a = width / 2.0f;
        b = height / 2.0f;
        dTheta = 2.0f * Mathf.PI / (float)nObjects;
        for (int j = 0; j < nObjects; j++)
        {
            Debug.Log(j.ToString() + " > " + a.ToString() + ", " + b.ToString());
            x = a * Mathf.Cos(dTheta * j );
            y = b * Mathf.Sin(dTheta * j );
            if (horizontal)
            {
                ptLocation[j] = new Vector3(x, 0.0f, y);
            }
            else 
            {
                ptLocation[j] = new Vector3(x, y, 0.0f);
            }
        }

    }


    public void createRings(int nrings)
    {
        float dh = height / (float)nrings;
        float dw = width / (float)nrings;
        
        float a, b;
        float x, y;
        int numberInRing;
        int numberLeft = nObjects;
        float totalCount = (float)(nrings + 1) * (float)nrings / 2.0f;
        int dCount;
        float dTheta, dThetaOld;
        int kk;

        // place the central point
        ptLocation[0] = new Vector3(0.0f, 0.0f, 0.0f);
        numberLeft = numberLeft - 1;
        kk = 1;
        dThetaOld = 0.0f;

        Debug.Log("dw dh " + dw.ToString() + ", " + dh.ToString());
        // loop over the rings from the inner to the outer
        for (int i = 0; i < nrings; i++)
        {
            a = dw * (i + 1);
            b = dh * (i + 1);
            dCount =(int) ( ((float)(i +1)/ totalCount) * nObjects);
            numberInRing = (int)dCount;
            
            if (numberInRing > numberLeft)
            {
                numberInRing = numberLeft;
            }
            dTheta = 2.0f * Mathf.PI / (float)numberInRing ;
            for (int j = 0; j < numberInRing; j++)
            {
                Debug.Log(j.ToString() + " > " + a.ToString() + ", " + b.ToString());
                x = a * Mathf.Cos(dTheta * j + dThetaOld);
                y = b * Mathf.Sin(dTheta * j + dThetaOld);
                ptLocation[kk] = new Vector3(x, y, 0.0f);
                kk = kk + 1;
            }
            dThetaOld = dTheta / 2.0f;
            numberLeft = numberLeft - numberInRing;

        }



    }

    

    public void createOddGrid()
    {

        // create the grid
        createGrid();

        // center the bottom row
        int istart = nx * (ny - 1);
        int ibottom = nObjects - istart;
        float dx = width / (nx - 1);
        float offset = dx * (nx - ibottom) * 0.5f;
        Debug.Log(offset.ToString() + "    " + ibottom.ToString() + "    " + nx.ToString() + "--" + ny.ToString() + "   " + maxObjects.ToString());
        for (int i = istart; i < maxObjects; i++)
            ptLocation[i][0] = ptLocation[i][0] + offset;
        
        float dy = height / (ny - 1);
        distanceBetweenObjects = Mathf.Min(dx, dy);

    }
   
    

    public void createGrid()
    {
        // spacing between objects
        float dx = width / (nx - 1);
        float dy = height / (ny - 1);

        // top left corner
        float xedge = -width / 2.0f;
        float yedge = height / 2.0f;

        float x, y;
        int k = 0;


        for (int j = 0; j < ny; j++)
        {
            for (int i = 0; i < nx; i++)
            {
                x = xedge + dx * i;
                y = yedge - dy * j;
                ptLocation[k] = new Vector3(x, y, 0.0f);
                k = k + 1;
                //Debug.Log(k.ToString() + "   " + x.ToString() + ", " + y.ToString() + "-------");
            }
        }
    }

    public void transformGrid(float distance, float angle, float xoffset, float yoffset, float zoffset)
    {

        float x, y, z;
        float r, theta;
        float xnew, ynew, znew;
        float angleCenter;
        for (int i = 0; i < nObjects; i++)
        {
            x = ptLocation[i][0];
            y = ptLocation[i][1];
            z = ptLocation[i][2] + distance;

            theta = Mathf.Atan2(x, z);
            r = Mathf.Sqrt(x * x + z * z);

            // convert the direction of the sorting line to be in radians
            angleCenter = angle * Mathf.PI / 180.0f;

            xnew = r * Mathf.Sin(angleCenter + theta) + xoffset;
            ynew = y + yoffset;
            znew = r * Mathf.Cos(angleCenter + theta) + zoffset;

            ptLocation[i] = new Vector3(xnew, ynew, znew);
        }

    }

}


// utility to place game objects into a packed configuration in 1d, 2d, and 3d
/*
public static class placeObjects 
{
    //public GameObject myPrefab;
    //public Texture[] myTexture = new Texture[maxObjects];
    //public String[] tnames = new String[maxObjects];
    //public GameObject[] markers = new GameObject[maxObjects];
    //public float mscale = 0.1f;
    //public GameObject markerPrefab;



    public static Vector3[] createOddGrid(int k, float height, float width)
    {
        float number, xx;
        int nx, ny;

        // determine the square root of the number of objects
        number = (float)k;
        xx = Mathf.Sqrt(number);

        // set the dimensions equal to the integer of the square root
        // this is the minimum size we need
        nx = (int)xx;
        ny = nx;

        // if the number of positions is too small, expand the x count by one
        if (nx * ny < k)
            nx = nx + 1;

        // if the total number of positions is still too small, expand y as well
        if (nx * ny < k)
            ny = ny + 1;

        // create the grid
        Vector3[] ptLocation = new Vector3[nx * ny];
        ptLocation = createGrid(nx, ny, 2.0f, 3.0f);

        // center the bottom row
        int istart = nx * (ny - 1);
        int ibottom = k - istart;
        float dx = width / (nx - 1);
        float offset = dx * (nx - ibottom) * 0.5f;
        for (int i = istart; i < k; i++)
            ptLocation[i][0] = ptLocation[i][0] + offset;

        return ptLocation;
    }
   
    

    public static float distanceBetweenObjects(int k, float height, float width)
    {
        float number, xx;
        int nx, ny;

        // determine the square root of the number of objects
        number = (float)k;
        xx = Mathf.Sqrt(number);

        // set the dimensions equal to the integer of the square root
        // this is the minimum size we need
        nx = (int)xx;
        ny = nx;

        // if the number of positions is too small, expand the x count by one
        if (nx * ny < k)
            nx = nx + 1;

        // if the total number of positions is still too small, expand y as well
        if (nx * ny < k)
            ny = ny + 1;

        float dx = width / (nx - 1);
        float dy = height / (ny - 1);
        return Mathf.Min(dx, dy);
    }
    
    

    public static Vector3[] createGrid(int nx, int ny, float height, float width)
    {
        // spacing between objects
        Vector3[] pointList = new Vector3[nx * ny];

        float dx = width / (nx - 1);
        float dy = height / (ny - 1);

        // top left corner
        float xedge = -width / 2.0f;
        float yedge = height / 2.0f;

        float x, y;
        int k = 0;


        for (int j = 0; j < ny; j++)
        {
            for (int i = 0; i < nx; i++)
            {
                x = xedge + dx * i;
                y = yedge - dy * j;
                pointList[k] = new Vector3(x, y, 0.0f);
                k = k + 1;
                //Debug.Log(k.ToString() + "   " + x.ToString() + ", " + y.ToString() + "-------");
            }
        }

        return pointList;
    }

    public static Vector3[] transformGrid(Vector3[] ptLocation, int nObjects, float distance, float angle, float xoffset, float yoffset, float zoffset)
    {

        float x, y, z;
        float r, theta;
        float xnew, ynew, znew;
        float angleCenter;
        for (int i = 0; i < nObjects; i++)
        {
            x = ptLocation[i][0];
            y = ptLocation[i][1];
            z = ptLocation[i][2] + distance;

            theta = Mathf.Atan2(x, z);
            r = Mathf.Sqrt(x * x + z * z);

            // convert the direction of the sorting line to be in radians
            angleCenter = angle * Mathf.PI / 180.0f;

            xnew = r * Mathf.Sin(angleCenter + theta) + xoffset;
            ynew = y + yoffset;
            znew = r * Mathf.Cos(angleCenter + theta) + zoffset;

            ptLocation[i] = new Vector3(xnew, ynew, znew);
        }

        return ptLocation;
    }

}
*/
