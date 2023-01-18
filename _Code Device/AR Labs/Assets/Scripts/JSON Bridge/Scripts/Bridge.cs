using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MagicLeapTools;
using TMPro;
using System.Linq;

public class Bridge
{
    static string canvasText;

    #region Public Methods
    // THIS FUNCTION CALLS A FUNCTION SIMILAR TO THE FUNCTION BELOW IT
    // POTENTIAL REFACTOR
    /// <summary>
    /// Builds scene from JSON spec
    /// One of the two main entry points to the bridge module
    /// </summary>
    /// <param name="data">JSON string specifying objects to make</param>
    public void ParseJson(string data)
    {
        makeScene(JsonUtility.FromJson<ObjectInfoCollection>(data));
    }

    /// <summary>
    /// Build/update objects as specified by data in passed array
    /// </summary>
    /// <param name="objectList">Array of ObjectInfo specifying what objects to make or update</param>
    public void makeObjects(ObjectInfo[] objectList)
    {
        foreach (ObjectInfo obj in objectList)
        {
            if (obj.transmittable == false) makeObject(obj);
            else if (obj.transmittable == true) makeTransmissionObject(obj);
        }
    }

    /// <summary>
    /// Creates an object and attaches all scripts and components as specified in the ObjectInfo argument
    /// </summary>
    /// <param name="obj">Specification of the object to create</param>
    public void makeObject(ObjectInfo obj)
    {
        GameObject myObject;       //Object being created

        LabLogger.Instance.InfoLog( this.GetType().ToString(), "Debug",
            $"Creating object: {obj.name}, type: {obj.type}");

        // Check if object exists in the scene already
        myObject = GameObject.Find(obj.name);
        // If it does not, create it and perform first time setup
        if (myObject == null)
        {
            // Instantiate base GameObject
            myObject = dealWithType(obj.type);
            // Set some initial values
            myObject.name = obj.name;
            obj.newPosition = true;
            obj.newScale = true;
            obj.newEulerAngles = true;
            if (obj.parentName != "")
            {
                GameObject parent = GameObject.Find(obj.parentName);
                myObject.transform.SetParent(parent?.transform);
            }
            // Add logger calls for when the object is targetted or dragged
            if (obj.type.Contains("moveableSphere"))
            {
                PointerReceiver pr = myObject.GetComponent<PointerReceiver>();
                pr.OnTargetEnter.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Object Targeted",
                    myObject.name));
                pr.OnTargetExit.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Object Detargeted",
                    myObject.name));
                pr.OnDragBegin.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Drag Start",
                    myObject.name));
                pr.OnDragEnd.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Drag End",
                    $"{myObject.name}:{myObject.transform.position.ToString("F3")}:{myObject.transform.eulerAngles.ToString("F3")}"));
            }
        }

        // three new key words have been added to the objectInfo class.
        // The keywords allow use to not override the postions, scales, and
        // orientation of an existing object if we don't want to do that.
        if (obj.newPosition)
            myObject.transform.localPosition = obj.position;
        if (obj.newScale)
            myObject.transform.localScale = obj.scale;
        if (obj.newEulerAngles)
            myObject.transform.localEulerAngles = obj.eulerAngles;
        LabLogger.Instance.InfoLog( this.GetType().ToString(), "Debug",
            $"Position: {obj.position.ToString("F3")}, Scale: {obj.scale.ToString("F3")}, EulerAngles: {obj.eulerAngles.ToString("F3")}");

        // Add custom scripted components to the object
        if (obj.componentsToAdd != null)
        {
            LabLogger.Instance.InfoLog( this.GetType().ToString(), "Debug",
                $"Adding components: {obj.componentsToAdd.ToList().Aggregate("", (acc, x) => acc + $"{x} ")}");

            foreach (string compStr in obj.componentsToAdd)
            {
                // Read component into a simple class to just get its name
                ComponentName cName = JsonUtility.FromJson<ComponentName>(compStr);
                // Use name to create the actual component and initialize its values
                Component myComp = myObject.GetComponent(Type.GetType(cName.name));
                if (myComp == null)
                    myComp = myObject.AddComponent(Type.GetType(cName.name));
                JsonUtility.FromJsonOverwrite(compStr, myComp);
            }
        }

        // Set object tag
        if (obj.tag != "")
            myObject.tag = obj.tag;

        // Get renderer to be modified
        Renderer rend = myObject.GetComponent<Renderer>();
        // Set object material
        if (obj.material != "" && obj.material != null)
            rend.material = Resources.Load<Material>(obj.material); // Material must be in a recources folder.

        // Set object Texture
        if (obj.texture != "" )
            rend.material.mainTexture = MediaCatalogue.Instance.GetTexture(obj.texture);

        // Set the object texture
        if (obj.textureByURL != "")
        {
            rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
            rend.material.mainTexture = MediaCatalogue.Instance.GetTexture(obj.texture);
        }

        // Set the base color of the material
        if (obj.color != null)
        {
            if (obj.color.Length == 4)
                rend.material.color = new Color( obj.color[0], obj.color[1], obj.color[2], obj.color[3]);
        }

        // Set the color of a child object
        if (obj.childColor != null && obj.childName != "" && obj.childColor.Length == 4)
        {
            GameObject childObject = myObject.transform.Find(obj.childName).gameObject;
            if (childObject != null)
            {
                Renderer childRend = childObject.GetComponent<Renderer>();
                rend?.material.SetColor("_Color", new Color(obj.childColor[0], obj.childColor[1], obj.childColor[2], obj.childColor[3])); //, obj.color[3]));
            }
        }

        // Set a rigidbody
        if (obj.RigidBody!= null)
        {
            Rigidbody mycomp = myObject.GetComponent<Rigidbody>();
            if (mycomp != null)
            {
                // Can't overwrite a rigidbody directly, need to use an intermediate helper class
                rigidBodyClass rb = new rigidBodyClass();

                // Copy the current values from the rigid body component to the helper
                rb.useGravity = mycomp.useGravity;
                rb.isKinematic = mycomp.isKinematic;
                rb.mass = mycomp.mass;
                rb.drag = mycomp.drag;
                rb.angularDrag = mycomp.angularDrag;

                // The constraints for Rigid body use a bit flag system
                // 2 = x constraint
                // 4 = y constraint
                // 8 = z constraint
                // 16 = x rotation constraint
                // 32 = y rotation constraint
                // 64 = z rotation constraint
                // the sum of the variables gives you the constraint, so 2+4 = 6 
                // would constrain x and y
                rb.xConstraint = ((byte)mycomp.constraints & (1 << 1)) != 0;
                rb.yConstraint = ((byte)mycomp.constraints & (1 << 2)) != 0;
                rb.zConstraint = ((byte)mycomp.constraints & (1 << 3)) != 0;
                rb.xRotationConstraint = ((byte)mycomp.constraints & (1 << 4)) != 0;
                rb.yRotationConstraint = ((byte)mycomp.constraints & (1 << 5)) != 0;
                rb.zRotationConstraint = ((byte)mycomp.constraints & (1 << 6)) != 0;

                // Update the helper class with the new data.
                JsonUtility.FromJsonOverwrite(obj.RigidBody, rb);
                
                // Transfer the helper class back to the real rigid body 
                mycomp.useGravity = rb.useGravity;
                mycomp.isKinematic = rb.isKinematic;
                mycomp.mass = rb.mass;
                mycomp.drag = rb.drag;
                mycomp.angularDrag = rb.angularDrag;

                // This is a bit tedeous, but it seems to work
                mycomp.constraints = RigidbodyConstraints.None;
                if (rb.xConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionX;
                if (rb.yConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionY;
                if (rb.zConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezePositionZ;
                if (rb.xRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationX;
                if (rb.yRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationY;
                if (rb.zRotationConstraint) mycomp.constraints = mycomp.constraints | RigidbodyConstraints.FreezeRotationZ;
            }
        }

        // Setup a PointerReceiver component
        if (obj.PointerReceiver != null) 
        {
            PointerReceiver mycomp = myObject.GetComponent<PointerReceiver>();
            if (mycomp != null)
            {
                // Copy data from component into class that can be overwritten from JSON
                pointerReceiverClass pr = new pointerReceiverClass();
                pr.draggable = mycomp.draggable;
                pr.kinematicWhileIdle= mycomp.kinematicWhileIdle;
                pr.faceWhileDragging = mycomp.faceWhileDragging;
                pr.matchWallWhileDragging= mycomp.matchWallWhileDragging;
                pr.invertForward= mycomp.invertForward;

                // Update helper with JSON data
                JsonUtility.FromJsonOverwrite(obj.PointerReceiver, pr);

                // Transfer data back to the actual component
                mycomp.draggable = pr.draggable;
                mycomp.kinematicWhileIdle= pr.kinematicWhileIdle;
                mycomp.faceWhileDragging = pr.faceWhileDragging;
                mycomp.matchWallWhileDragging= pr.matchWallWhileDragging;
                mycomp.invertForward= pr.invertForward;
            }
        }

        // Set canvas text on object?
        if (obj.canvasText !=null)
        {
            Text tt = myObject.GetComponent<Text>();
            if (tt == null)
                tt = myObject.AddComponent<Text>();
            // I'm not sure why this variable is neccesary
            canvasText = obj.canvasText;
            tt.text = obj.canvasText;
        }

        // Set text mesh pro object
        TextMeshPro textBox = myObject.GetComponent<TextMeshPro>();
        if (textBox != null)
        {
            if (obj.tmp != null)
            {
                // Copy data from component into a class that can be overwritten from JSON
                textProClass tpc = new textProClass();
                tpc.textField = textBox.text;
                tpc.color = textBox.color;
                tpc.fontSize = textBox.fontSize;
                tpc.wrapText = textBox.enableWordWrapping;

                // Update the class with the JSON data
                JsonUtility.FromJsonOverwrite(obj.tmp, tpc);

                // Copy the info back into the component
                textBox.SetText(tpc.textField);
                textBox.fontSize = tpc.fontSize;
                textBox.color = tpc.color;
                textBox.enableWordWrapping = tpc.wrapText;
                textBox.SetAllDirty();
            }
            textBox.SetAllDirty();
        }
        
        // Log the size of the mesh
        MeshRenderer mesh = myObject.GetComponent<MeshRenderer>();
        if(mesh != null && mesh.bounds.extents != Vector3.zero)
        LabLogger.Instance.InfoLog( this.GetType().ToString(), "Debug",
            $"Mesh bounding box: {mesh.bounds.ToString("F3")}");

        // Enable the object
        myObject.SetActive(obj.enabled);
    }

    /// <summary>
    /// Deletes all objects specified in the data argument
    /// </summary>
    /// <param name="data">JSON list of objects to delete</param>
    public void CleanUp(string data)
    {
        ObjectInfoCollection info = JsonUtility.FromJson<ObjectInfoCollection>(data);
        foreach (ObjectInfo obj in info.objects)
        {
            GameObject obj_dstry = GameObject.Find(obj.name);
            PointerReceiver pr = obj_dstry.GetComponent<PointerReceiver>();
            if (pr != null)
            {
                pr.OnTargetEnter.RemoveAllListeners();
                pr.OnTargetExit.RemoveAllListeners();
                pr.OnDragBegin.RemoveAllListeners();
                pr.OnDragEnd.RemoveAllListeners();
            }

            GameObject.Destroy(obj_dstry);
        }
    }
    #endregion Public Methods

    #region Private Methods
    private void makeScene(ObjectInfoCollection info)
    {
        //In the event that transmission objects need to be created this will send each to thier apropriet category
        foreach (ObjectInfo obj in info.objects)
        {
            if (obj.transmittable == false) makeObject(obj);
            else if (obj.transmittable == true) makeTransmissionObject(obj);
        }
    }

    /// <summary>
    /// Create an object using the Transmission System
    /// </summary>
    /// <param name="obj">specification of the object to create</param>
    private void makeTransmissionObject(ObjectInfo obj)
    {
        LabLogger.Instance.InfoLog( this.GetType().ToString(), "Debug",
            $"Creating object: {obj.name}, type: {obj.type}");

        GameObject myObject = GameObject.Find(obj.name);
        TransmissionObject myTransObject;

        // If it does not, create it and perform first time setup
        if (myObject == null)
        {
            // Instantiate base GameObject
            myTransObject = Transmission.Spawn(
                obj.type,
                obj.position,
                Quaternion.Euler(obj.eulerAngles),
                obj.scale
            );
        }

        // Now that the transmission object is made, send the rest of the info to the peers
        String objInfoString = JsonUtility.ToJson(obj);
        RPCMessage rpcMessage = new RPCMessage("handleTransmissionObjInfo", objInfoString);


            // Set some initial values
            myObject.name = obj.name;
            obj.newPosition = true;
            obj.newScale = true;
            obj.newEulerAngles = true;
            if (obj.parentName != "")
            {
                GameObject parent = GameObject.Find(obj.parentName);
                myObject.transform.SetParent(parent?.transform);
            }
            // Add logger calls for when the object is targetted or dragged
            if (obj.type.Contains("moveableSphere"))
            {
                PointerReceiver pr = myObject.GetComponent<PointerReceiver>();
                pr.OnTargetEnter.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Object Targeted",
                    myObject.name));
                pr.OnTargetExit.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Object Detargeted",
                    myObject.name));
                pr.OnDragBegin.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Drag Start",
                    myObject.name));
                pr.OnDragEnd.AddListener((x) => LabLogger.Instance.InfoLog(pr.name, "Drag End",
                    $"{myObject.name}:{myObject.transform.position.ToString("F3")}:{myObject.transform.eulerAngles.ToString("F3")}"));
            }
        }
        if (!GameObject.Find(obj.name))
        {
            if (obj.parentName != "")
            {
                parent = GameObject.Find(obj.parentName);
            }

            myTransObject = Transmission.Spawn(obj.type, obj.position, Quaternion.Euler(0, 0, 0), obj.scale);
            myTransObject.name = obj.name;
            myObject = myTransObject.gameObject;

            for (int i = 0; i < obj.componentsToAdd.Length; i++)
            {
                //Parse once to get the name of the component
                ComponentName cName = JsonUtility.FromJson<ComponentName>(obj.componentsToAdd[i]);
                //Check if the component already exists (ie, the mesh renderer on aprimitive)
                Component myComp = myObject.GetComponent(Type.GetType(cName.name));
                if (myComp == null)
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myObject.AddComponent(Type.GetType(cName.name)));
                    //This is causing problems becuase of AddCompnent
                }
                else
                {
                    JsonUtility.FromJsonOverwrite(obj.componentsToAdd[i], myComp);
                }
            }

            if (obj.parentName != "")
                myObject.transform.parent = parent.transform;

            if (obj.material != "")
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material = Resources.Load<Material>(obj.material); //material must be in a recources folder.
            }

            if (obj.texture != "")
            {
                Renderer rend = myObject.GetComponent<Renderer>();
                rend.material.mainTexture = Resources.Load<Texture2D>(obj.texture);
            }
        }
    }

    /// <summary>
    /// Instantiates a base GameObject depending on the contents of `type`
    /// </summary>
    /// <param name="type">Specifies what base GameObject to instantiate</param>
    /// <returns></returns>
    private GameObject dealWithType(string type)
    {
        GameObject myObject;

        switch (type)
        {
            case "":
                myObject = new GameObject();
                break;
            case "empty":
                myObject = new GameObject();
                break;
            case "plane":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                break;
            case "cube":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "sphere":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "capsule":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                break;
            case "cylinder":
                myObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            default:
                myObject = GameObject.Instantiate(Resources.Load(type, typeof(GameObject)) as GameObject);
                //Note that the above line requires your prefab to be located in a resources folder.
                break;
        }

        return myObject;
    }
    #endregion Private Methods
}
