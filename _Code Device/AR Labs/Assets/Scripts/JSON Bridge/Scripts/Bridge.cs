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
    private bool transmissionEnabled = false; // Flag marking if Transmission has been setup
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
                    LabLogger.LogTag.TRACE,
                    "Creating Bridge Instance");
                _instance = new Bridge();
            }

            return _instance;
        }
    }
    #endregion Variables

    #region Public Methods
    /// <summary>
    /// Called to link the bridge to Transmission so it can handle String messages
    /// </summary>
    public void ConnectToTransmission()
    {
        LabLogger.Instance.InfoLog(
            this.ToString(),
            LabLogger.LogTag.TRACE,
            "ConnectToTransmission()"
        );
        Transmission.Instance.OnStringMessage.AddListener(handleStringMessage);
        transmissionEnabled = true;
    }

    /// <summary>
    /// Unlinks the String message handler from the bridge
    /// </summary>
    public void DisconnectFromTransmission()
    {
        LabLogger.Instance.InfoLog(
            this.ToString(),
            LabLogger.LogTag.TRACE,
            "DisconnectFromTransmission()"
        );
        Transmission.Instance.OnStringMessage.RemoveAllListeners();
        transmissionEnabled = false;
    }

    /// <summary>
    /// Creates an object and attaches all scripts and components as specified in the ObjectInfo argument
    /// </summary>
    /// <param name="obj">Specification of the object to create</param>
    public void MakeObject(ObjectInfo obj)
    {
        // Variables
        bool initialize = false;    // Whether to setup new object, or modify existing
        GameObject myObject = null; // Reference to gameobject being worked on

        // Log Event
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE,
            $"MakeObject() : {obj.name}, type: {obj.type}, transmitted: {obj.transmittable}");

        // Try to find the object
        myObject = GameObject.Find(obj.name);

        // If it doesn't exist already, create it
        if (myObject == null)
        {
            myObject = instantiateObject(obj.type, obj.position, obj.eulerAngles, obj.scale, obj.transmittable);
            // Flag that we are initializing a new object
            initialize = true;
        }

        // If a transmission object and Transmission is ready, transmit object details to peers
        // if(obj.transmittable && transmissionEnabled)
        if(transmissionEnabled)
        {
            transmitObject(obj, myObject.GetComponent<TransmissionObject>().guid, initialize);
        }
        
        // Setup the object
        if (initialize)
            initializeObject(myObject, obj);
        else
            modifyObject(myObject, obj);
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

    #region Callbacks
    /// <summary>
    /// Receives string message from peer that has part of an objInfo serialization
    /// </summary>
    /// <param name="msg">the message data object</param>
    public void handleStringMessage(StringMessage msg)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE, $"handleStringMessage() : {msg}");

        // If the object was able to be sent as one message
        if (msg.d == "INIT" || msg.d == "CHNG")
        {
            GameObject go;
            ObjectInfo newObjectInfo;
            // Find the objects to modify, and extract the ObjectInfo data
            (go, newObjectInfo) = parseTransmissionObjectString(msg.v);

            // modify the object
            if (msg.d == "INIT")
                initializeObject(go, newObjectInfo);
            if (msg.d == "MODIFY")
                modifyObject(go, newObjectInfo);
        }

        // If we are receiving just part of the objectInfo
        if (msg.d == "PART")
        {
            msgParts += msg.v;
        }

        // If we are receiving the last of the objectInfo
        if (msg.d.StartsWith("END_"))
        {
            GameObject go;
            ObjectInfo newObjectinfo;
            // Find the objects to modify, and extract the ObjectInfo data
            (go, newObjectinfo) = parseTransmissionObjectString(msgParts + msg.v);
            // Reset the variable holding parts of the message
            msgParts = "";

            // modify the object
            if (msg.d.EndsWith("INIT"))
                initializeObject(go, newObjectinfo);
            if (msg.d.EndsWith("CHNG"))
                modifyObject(go, newObjectinfo);
        }
    }
    #endregion Callbacks
    #endregion Public Methods

    #region Private Methods
    /// <summary>
    /// Creates an object either from a prefab instantiation, or a Transmission spawn
    /// </summary>
    /// <param name="prefabName">Name of the prefab to spawn from a resources folder</param>
    /// <param name="position">Vector position of the object</param>
    /// <param name="eulerAngles">Vector orientation of the object</param>
    /// <param name="scale">Vector size of the object</param>
    /// <param name="transmittable">Whether to use the transmission system</param>
    /// <returns>Instantiated GameObject</returns>
    private GameObject instantiateObject(string prefabName, Vector3 position, Vector3 eulerAngles, Vector3 scale, bool transmittable)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE, "instantiateObject()");

        GameObject myObject; // Reference to object being created

        // Only spawn transmission if we know Transmission has been setup
        // if(transmittable && transmissionEnabled)
        if(transmissionEnabled)
        {
            // Create the object using the transmission system
            try
            {
                myObject = Transmission.Spawn(prefabName, position, Quaternion.Euler(eulerAngles), scale).gameObject;
            }
            catch (Exception ex)
            {
                LabLogger.Instance.InfoLog(
                    this.ToString(),
                    LabLogger.LogTag.ERROR,
                    ex.ToString()
                );
                myObject = null;
            }
        }
        else // Not transmitted
        {
            // Instantiate object normally
            myObject = GameObject.Instantiate(Resources.Load(prefabName, typeof(GameObject)) as GameObject);
            //Note that the above line requires your prefab to be located in a resources folder.

            // Set transform values
            myObject.transform.position = position;
            myObject.transform.eulerAngles = eulerAngles;
            myObject.transform.localScale = scale;
        }

        return myObject;
    }

    /// <summary>
    /// Sends objInfo data for a transmission to peers
    /// </summary>
    /// <param name="obj">objInfo to be sent</param>
    /// <param name="guid">guid of transmission object to modify</param>
    /// <param name="init">whether the object is being setup or not</param>
    private void transmitObject(ObjectInfo obj, string guid, bool init)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE,
            $"transmitObject(), Obj Name: {obj.name}, guid: {guid}");

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
            // Include 4 byte allowance for additional message labels
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
        else
        {
            Transmission.Send(strMessage);
        }

        return;
    }

    /// <summary>
    /// Parses the guid and json from a string message sent 
    /// to setup or modify a Transmission Object.
    /// Finds the transmission object to modify and builds the ObjectInfo
    /// </summary>
    /// <param name="data">starts with guid of Transmission Object to be modified,
    ///                    followed by the json describing that object</param>
    /// <returns></returns>
    private (GameObject, ObjectInfo) parseTransmissionObjectString(string data)
    {
        // Variables
        ObjectInfo objInfo; // Holds the data about the object
        String guid = "";   // The transmission object's id

        // Get the guid from the front of the string data
        Regex rg = new Regex(@"^(?<guid>[\w-]+)::_::");
        Match match = rg.Match(data);
        if (match.Success)
        {
            // Use the rest of the string after the regex match for the object info
            objInfo = JsonUtility.FromJson<ObjectInfo>(data.Substring(match.Index + match.Length));
            guid = match.Groups["guid"].Value;
        }
        else // Failed to get guid and parse information
        {
            LabLogger.Instance.InfoLog(
                this.ToString(),
                LabLogger.LogTag.ERROR,
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
            LabLogger.LogTag.ERROR,
            $"Failed to find the transmission object with guid: {guid}");
        return (null, objInfo);
    }

    /// <summary>
    /// Performs firsttime setup on an object, calls modifyObject once done
    /// </summary>
    /// <param name="myObject">object to be modified</param>
    /// <param name="obj">specification of the object</param>
    private void initializeObject(GameObject myObject, ObjectInfo obj)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE, $"initializeObject() : {obj.name}");

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
            pr.OnTargetEnter.AddListener((x) => LabLogger.Instance.InfoLog(pr.GetType().ToString(), LabLogger.LogTag.EVENT, $"{obj.name} Targetted"));
            pr.OnTargetExit.AddListener((x) => LabLogger.Instance.InfoLog(pr.GetType().ToString(), LabLogger.LogTag.EVENT, $"{obj.name} UnTargetted"));
            pr.OnDragBegin.AddListener((x) => LabLogger.Instance.InfoLog(pr.GetType().ToString(), LabLogger.LogTag.EVENT, $"{obj.name} Targetted"));
            pr.OnDragEnd.AddListener((x) => LabLogger.Instance.InfoLog(pr.GetType().ToString(), LabLogger.LogTag.EVENT,
                $"{myObject.name}:{myObject.transform.position.ToString("F3")}:{myObject.transform.eulerAngles.ToString("F3")}"));
        }

        // Now that we are done initializing, modify the object
        modifyObject(myObject, obj);
    }

    /// <summary>
    /// Updates features on an existing object
    /// </summary>
    /// <param name="myObject">object to be modified</param>
    /// <param name="obj">specification of the modifications</param>
    private void modifyObject(GameObject myObject, ObjectInfo obj)
    {
        LabLogger.Instance.InfoLog(this.GetType().ToString(), LabLogger.LogTag.TRACE, $"modifyObject() : {obj.name}");
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
        LabLogger.Instance.InfoLog( this.GetType().ToString(), LabLogger.LogTag.DEBUG,
            $"Position: {obj.position.ToString("F3")}, Scale: {obj.scale.ToString("F3")}, EulerAngles: {obj.eulerAngles.ToString("F3")}");

        // Add custom scripted components to the object
        if (obj.componentsToAdd != null)
        {
            LabLogger.Instance.InfoLog( this.GetType().ToString(), LabLogger.LogTag.DEBUG,
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
        LabLogger.Instance.InfoLog( this.GetType().ToString(), LabLogger.LogTag.DEBUG,
            $"Mesh bounding box: {mesh.bounds.ToString("F3")}");

        // Enable the object
        myObject.SetActive(obj.enabled);
    }
    #endregion Private Methods
}
