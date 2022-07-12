using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

/// <summary>
/// stores identity data about a user, links name to pin/mNumber
/// </summary>
public struct pin_id_record
{
    public string lastName;
    public string firstName;
    public string mNumber;
    public string pin;
}

/// <summary>
/// Downloads user list and authenticates submitted user/pin combos
/// </summary>
public class Authenticate : MonoBehaviour
{
    #region Variables
    // Used to download and store pin csv file
    const string PIN_ID_CSV_URL = "http://cyberlearnar.cs.mtsu.edu/show_uploaded/pin_ids.csv";
    //const string PIN_ID_CSV_FILEPATH = "login/pin_ids";
    private FileInfo pinCsvFileInfo;    // Used to interact w/ downloaded file

    // Runtime holds the pins and ids, used to authenticate user logins
    private List<pin_id_record> ids = null;

    // Whether the file has been parsed
    private bool authReady = false;
    
    private LabLogger logger; // Logger used to write info to file
    private string entity;    // Name of this class as a string, initialized in start
    public bool Ready
    {
        get { return authReady; }
    }
    #endregion Variables

    #region Unity Methods
    private void Awake()
    {
        // Variable Initializations
        entity = this.GetType().ToString();
        logger = LabLogger.Instance;

        // filepath: pdp/login/pin_ids.csv
        pinCsvFileInfo = new FileInfo(Path.Combine(Application.persistentDataPath,
                                                   Path.Combine("login","pin_ids.csv")));
        pinCsvFileInfo.Directory.Create();
    }
    void Start()
    {
        logger.InfoLog(entity, "Trace", "Starting pin-csv download");

        // Get the most recent csv file downloaded
        DownloadUtility.Instance.DownloadFile(PIN_ID_CSV_URL, pinCsvFileInfo.FullName, downloadComplete);
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Reads in the csv file with users and pins, stores it in runtime data structs
    /// </summary>
    public void ParseCSV()
    {
        logger.InfoLog(entity, "Trace", "Starting to parse csv file");
        // Create new list of pin_id_records
        ids = new List<pin_id_record>();

        // Open csv as raw text, split into row entries
        StreamReader sr = pinCsvFileInfo.OpenText();
        string[] records = sr.ReadToEnd().Split('\n');
        sr.Close();

        // Loop over the entries, skip the header row
        for (int i = 1; i < records.Length; i++)
        {
            // Split row into fields
            string[] fields = records[i].Trim().Split(',');

            // Create new struct to store read data
            pin_id_record newUsr;

            // Store fields in pin_id_records struct
            newUsr.lastName = fields[0];
            newUsr.firstName = fields[1];
            newUsr.mNumber = fields[2];
            newUsr.pin = fields[3];

            // Add record to ids list
            ids.Add(newUsr);
        }

        if (Debug.isDebugBuild)
        {
            foreach (pin_id_record id in ids)
                Debug.Log(id.pin);
        }

        logger.InfoLog(entity, "Trace", "Csvs parsed");
        // Set flag to allow auth queries
        authReady = true;
    }

    /// <summary>
    /// Returns true if the passed pin is found in the file of users and pins
    /// </summary>
    /// <param name="pin">Six digit pin number</param>
    /// <returns>true if the pin is found, false if not</returns>
    public bool AuthenticatePin(string pin)
    {
        logger.InfoLog(entity, "Trace", $"Attempting to authenticate pin: {pin}");
        return ids.Exists(x => { Debug.Log($"pin :{pin}:, length : {pin.Length}, x.pin :{x.pin}:, length : {x.pin.Length}, x.pin == pin :{x.pin==pin}:"); return x.pin == pin; });
    }

    /// <summary>
    /// Returns the full name matching the sent pin
    /// </summary>
    /// <param name="pin">six digit pin numver</param>
    /// <returns>string containing full name corresponding to sent pin, or empty string if not found</returns>
    public string PinToName(string pin)
    {
        if (AuthenticatePin(pin))
        {
            pin_id_record usr = ids.Find(x => x.pin == pin);
            return usr.firstName + " " + usr.lastName;
        }
        else
            return "";
    }

    /// <summary>
    /// converts usr pin to their full m number
    /// </summary>
    /// <param name="pin">six digit pin</param>
    /// <returns>full m number of students, or empty string if not found</returns>
    public string PinToMNum(string pin)
    {
        if (AuthenticatePin(pin))
            return ids.Find(x => x.pin == pin).mNumber;
        else
            return "";
    }
    #endregion Public Methods

    #region Event Handlers
    /// <summary>
    /// Called by DownloadUtility once the file is stored. Starts the csv parse
    /// </summary>
    private void downloadComplete(int rc)
    {
        if(rc == 0)
        {
            ParseCSV();
        }
        else // Download failed
        {
            logger.InfoLog(entity, "Error", "PinCSV file failed to download, Cannot authenticate pins");
            Debug.LogError("PinCSV file failed to download, Cannot authenticate pins");
        }
    }
    #endregion Event Handlers
}
