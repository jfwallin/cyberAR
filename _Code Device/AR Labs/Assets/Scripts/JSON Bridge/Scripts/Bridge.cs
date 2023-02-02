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
    #region Variables
    private string msgParts;         // Holds pieces of large messages still being sent
    private static Bridge _instance; // Singleton instance variable

    // Singleton access
    public static Bridge Instance
    {
        get
        {
            if (_instance == null)
            {
                LabLogger.Instance.InfoLog(
                    "BRIDGE",
                    "TRACE",
                    "Creating Bridge Instance");
                _instance = new Bridge();
            }

            return _instance;
        }
    }
    #endregion Variables

    #region Public Methods
    public void ConnectToTransmission()
    {
        Transmission.Instance.OnStringMessage.AddListener(handleStringMessage);
    }

    public void DisconnectFromTransmission()
    {
        Transmission.Instance.OnStringMessage.RemoveAllListeners();
    }

    /// <summary>
    /// Creates an object and attaches all scripts and components as specified in the ObjectInfo argument
    /// </summary>
    /// <param name="obj">Specification of the object to create</param>
    public void MakeObject(ObjectInfo obj)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), "Debug",
            $"Creating object: {obj.name}, type: {obj.type}");

        // Try to find the object
        GameObject myObject = GameObject.Find(obj.name);


        // If it doesn't exist already, create it
        if (myObject == null)
        {
            myObject = instantiateObject(obj.type, obj.position, obj.eulerAngles, obj.scale, obj.transmittable);
            myObject = dealWithType(obj.type);
            initializeObject(myObject, obj);
        }
        else
        {
            // Modify existing object
            modifyObject(myObject, obj);
        }
    }
    private GameObject instantiateObject(string prefabName, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool transmittable)
    {
        GameObject myObject;

        if(transmittable)
        {
            myObject = Transmission.Spawn(prefabName, position, Quaternion.Euler(eulerAngles), scale).gameObject;
        }
        else // Not transmitted
        {
            // Instantiate object
            switch (prefabName)
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
                    myObject = GameObject.Instantiate(Resources.Load(prefabName, typeof(GameObject)) as GameObject);
                    //Note that the above line requires your prefab to be located in a resources folder.
                    break;
            }

            // Set transform values
            myObject.transform.position = position;
            myObject.transform.eulerAngles = eulerAngles;
            myObject.transform.localScale = scale;
        }

        return myObject;
    }

    /// <summary>
    /// Build/update objects as specified by data in passed array
    /// </summary>
    /// <param name="objectList">Array of ObjectInfo specifying what objects to make or update</param>
    public void MakeObjects(ObjectInfo[] objectList)
    {
        foreach (ObjectInfo obj in objectList)
            MakeObject(obj);
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
    public void handleStringMessage(StringMessage msg)
    {
        // Check if this is about updating objects 
        if (msg.d == "INIT" || msg.d == "CHNG")
        {
            GameObject go;
            ObjectInfo newObjectInfo;
            // Find the objects to modify, and extract the ObjectInfo data
            (go, newObjectInfo) = handleTransmissionObjectString(msg.v);

            if (msg.d == "INIT")
                initializeObject(go, newObjectInfo);
            if (msg.d == "MODIFY")
                modifyObject(go, newObjectInfo);
        }
        if (msg.d == "PART")
        {
            msgParts += msg.v;
        }
        if (msg.d.StartsWith("END_"))
        {
            GameObject go;
            ObjectInfo newObjectinfo;
            (go, newObjectinfo) = handleTransmissionObjectString(msgParts + msg.v);
            msgParts = "";

            if (msg.d.EndsWith("INIT"))
                initializeObject(go, newObjectinfo);
            if (msg.d.EndsWith("CHNG"))
                modifyObject(go, newObjectinfo);
        }
    }
    #endregion Public Callbacks

    #region Private Methods
    private void transmitObject(ObjectInfo obj, string guid, bool init)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), "Debug",
            $"Sending Object Info to Peer, Obj Name: {obj.name}");

        // Build the test message
        // Convert object information to json
        String objInfoString = JsonUtility.ToJson(obj);
        // Add the guid of the object to the front of the data
        String message = guid + "::_::" + objInfoString;
        // Specify what function should get the rpc call
        String instruction = init ? "INIT" : "CHNG";
        // Create the message object
        StringMessage strMessage = new StringMessage(message, instruction);

        // Check the size of the message
        String serialized = JsonUtility.ToJson(strMessage);
        byte[] bytes = Encoding.UTF8.GetBytes(serialized);
        int msgSize = bytes.Length;
        int bufSize = Transmission.Instance.bufferSize;
        // If the message is larger than the buffer, split message into smaller pieces
        if (msgSize > bufSize)
        {
            // Each message portion must be msgPartSize bytes long
            int msgPartSize = msgSize - bufSize - 4;
            // Calculate the number of messages to send
            int numMessages = (int)(msgSize / msgPartSize) + 1;
            for (int i=0; i < numMessages; i++)
            {
                // Specify whether this is an obj update or init with the last message
                string data = i == numMessages - 1 ? "END_"+instruction : "PART";
                // Build and send the message
                StringMessage msgPart = new StringMessage(message.Substring(i * msgPartSize, msgPartSize), data);
                Transmission.Send(msgPart);
            }
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

        transmitObject(obj, trObj.guid, initialize);

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
