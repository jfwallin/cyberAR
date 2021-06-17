using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class testPlacement: MonoBehaviour
{
    private const int maxpts = 39;
    public int nObjects;
    Vector3[] ptLocation = new Vector3[maxpts];
    // Start is called before the first frame update
    public float height, width;

    placeObjects PO;
    void Start()
    {
        nObjects = 8;
        height = 2.0f;
        width = 3.0f;
        PO = new placeObjects(nObjects, height, width); 

        //float theSize = PO.distanceBetweenObjects(nObjects, 1.0f, 3.0f); 
        //PO.createOddGrid();

        //PO.createRings(3);
        PO.createRing(false);
        PO.transformGrid(2.0f, 45.0f, 0.0f, 1.0f, 0.0f);
        ptLocation = PO.ptLocation;
        showGrid();
    }


    void showGrid()
    {
        GameObject[] go;
        go = new GameObject[nObjects];

        for (int i = 0; i < nObjects; i++)
        {
            go[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go[i].transform.position = ptLocation[i];
            go[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
             

        }

    }


}
