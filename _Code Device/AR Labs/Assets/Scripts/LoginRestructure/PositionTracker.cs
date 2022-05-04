using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using MagicLeapTools;

/// <summary>
/// Used to log position and rotation data of the headset and controller for later analysis
/// </summary>
public class PositionTracker : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private Transform controller;  // Used to track controller position + rotation
    [SerializeField]
    private Transform headset;     // Used to track headset position + rotation

    [SerializeField]
    private float logDeltaTime = 0.1f; // Time between writing to log file
    private float prevTime = 0f;       // time of last log output

    private LabLogger logger;      // Used to write output to file, init in Start
    private string entity;         // Name of this script as a string, init in Start
    #endregion Variables

    #region Unity Methods
    private void Awake()
    {
        // Fail fast assertions
        if(controller == null)
            controller = FindObjectOfType<ControlInput>().transform;
        Assert.IsNotNull(controller);
        if (headset == null)
            headset = Camera.main.transform;
        Assert.IsNotNull(headset);

        // Setup logger information
        entity = this.GetType().ToString();
        logger = LabLogger.Instance;
    }

    void Start()
    {
        // Start logging
        logger.InfoLog(entity, "Trace", "Starting Position Tracker");
        logTransforms();

        // Set the time of last log
        prevTime = Time.time;
    }

    void Update()
    {
        // Log transforms every "logDeltaTime" seconds
        if (Time.time - prevTime > logDeltaTime)
            logTransforms();
    }
    #endregion Unity Methods

    #region Private Methods
    /// <summary>
    /// Logs the position and rotation of the headset and controller
    /// </summary>
    private void logTransforms()
    {
        logger.InfoLog(entity, "Headset Position", headset.position.ToString());
        logger.InfoLog(entity, "Headset Rotation", headset.eulerAngles.ToString());
        logger.InfoLog(entity, "Controller Position", controller.position.ToString());
        logger.InfoLog(entity, "Controller Rotation", controller.eulerAngles.ToString());
    }
    #endregion
}
