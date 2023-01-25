using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MagicLeapTools;
using TMPro;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
        GameObject myObject = GameObject.Find(obj.name); //Object being created

        LabLogger.Instance.InfoLog(this.GetType().ToString(), "Debug",
            $"Creating object: {obj.name}, type: {obj.type}");

        // If it does not, create it and perform first time setup
        if (myObject == null)
        {
            myObject = dealWithType(obj.type);
            initializeObject(myObject, obj);
        }
        else
        {
            // Modify existing object
            modifyObject(myObject, obj);
        }
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

    #region Public Callbacks
    /// <summary>
    /// RPC by transmission, used to set up Transmision Object for the first time
    /// </summary>
    /// <param name="data">contains guid of the Transmission Object
    ///                    and the json describing it.</param>
    public void remoteInitObject(string data)
    {
        GameObject go;
        ObjectInfo newObjectInfo;
        // Find the object to modify, and extract the ObjectInfo data
        (go, newObjectInfo) = handleTransmissionObjectString(data);
        // Setup the object
        initializeObject(go, newObjectInfo);
    }

    /// <summary>
    /// RPC by transmission, used to modify and existing Transmission Object
    /// </summary>
    /// <param name="data">contains guid of the Transmission Object
    ///                    and the json describing it.</param>
    public void remoteModifyObject(string data)
    {
        GameObject go;
        ObjectInfo newObjectInfo;
        // Find the object to modify, and extract the ObjectInfo data
        (go, newObjectInfo) = handleTransmissionObjectString(data);
        // Update the object
        modifyObject(go, newObjectInfo);
    }

    public void handleBufferSizeMessage()
    {
        // receives info about what to change local buffer size to
        
        // Then send back an acknowledgement
    }
    #endregion Public Callbacks

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
    /// Create an object using the Transmission System,
    /// Called once by the host of the shared experience
    /// </summary>
    /// <param name="obj">specification of the object to create</param>
    private void makeTransmissionObject(ObjectInfo obj)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), "Debug",
            $"Creating object: {obj.name}, type: {obj.type}");

        GameObject myObject = GameObject.Find(obj.name);
        TransmissionObject trObj = myObject?.GetComponent<TransmissionObject>();
        bool initialize = false;

        // If it does not, create it and perform first time setup
        if (myObject == null)
        {
            // Instantiate base GameObject
            trObj = Transmission.Spawn(
                obj.type,
                obj.position,
                Quaternion.Euler(obj.eulerAngles),
                obj.scale
            );
            myObject = trObj.gameObject;
            initialize = true;
        }

        LabLogger.Instance.InfoLog(this.GetType().ToString(), "Debug",
            $"Sending Object Info to Peer, Obj Name: {obj.name}");

        // Now that the transmission object is made, send the rest of the info to the peers
        // Convert object information to json
        String objInfoString = JsonUtility.ToJson(obj);
        // Add the guid of the object to the front of the data
        String message = trObj.guid + "::_::" + objInfoString;
        // Specify what function should get the rpc call
        String method = initialize ? "remoteInitObject" : "remoteModifyObject";
        // Create the message object
        RPCMessage rpcMessage = new RPCMessage(method, message);
        // Check the size of the message
        String serialized = JsonUtility.ToJson(rpcMessage);
        byte[] bytes = Encoding.UTF8.GetBytes(serialized);
        // Send message indicating to change the buffer size (use float message b/c there's no intmessage)
        FloatMessage sizeMessage = new FloatMessage(bytes.Length, "BUFFER_SIZE");
        Transmission.Send(sizeMessage);

        // Wait for all known peers to send back acknowledgements

        // Send back the message containing the object info
        

        // Now Modify the object that was created
        if (initialize)
            initializeObject(myObject, obj);
        else
            modifyObject(myObject, obj);
    }

    /// <summary>
    /// Parses the guid and json from a string message sent 
    /// to setup or modify a Transmission Object
    /// </summary>
    /// <param name="data">starts with guid of Transmission Object to be modified,
    ///                    followed by the json describing that object</param>
    /// <returns></returns>
    private (GameObject, ObjectInfo) handleTransmissionObjectString(string data)
    {
        ObjectInfo objInfo;
        String guid = "";

        // Get the guid from the front of the string data
        Regex rg = new Regex(@"^.+(?<guid>\w+)::_::");
        Match match = rg.Match(data);
        if (match.Success)
        {
            objInfo = JsonUtility.FromJson<ObjectInfo>(data.Substring(match.Index + match.Length));
            guid = match.Groups["guid"].Value;
        }
        else // Failed to get guid and parse information
        {
            LabLogger.Instance.InfoLog(
                this.ToString(),
                "DEBUG",
                "Failed to parse Transmission object information");
            return (null, null);
        }

        // Check if the object we need has been spawned
        if (TransmissionObject.Exists(guid))
        {
            GameObject go = TransmissionObject.Get(guid).gameObject;
            return (go, objInfo);
        }
        // Else, log failure
        LabLogger.Instance.InfoLog(
            this.ToString(),
            "DEBUG",
            $"Failed to find the transmission object with guid: {guid}");
        return (null, objInfo);
    }

    /// <summary>
    /// Performs firsttime setup on an object
    /// </summary>
    /// <param name="myObject">object to be modified</param>
    /// <param name="obj">specification of the object</param>
    private void initializeObject(GameObject myObject, ObjectInfo obj)
    {
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

        // Now that we are done initializing, modify the object
        modifyObject(myObject, obj);
    }

    /// <summary>
    /// Updates features on an existing object
    /// </summary>
    /// <param name="myObject"></param>
    /// <param name="obj"></param>
    private void modifyObject(GameObject myObject, ObjectInfo obj)
    {
        // Do all the object setup

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
