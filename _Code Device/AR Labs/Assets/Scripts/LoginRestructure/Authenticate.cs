using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    const string pin_id_csv_url = "http://cyberlearnar.cs.mtsu.edu/show_uploaded/pin_ids.csv";
    const string pin_id_csv_filepath = "login/pin_ids";
    // Runtime holds the pins and ids, used to authenticate user logins
    private List<pin_id_record> ids = null;
    #endregion Variables

    #region Unity Methods
    void Start()
    {
        // Get the most recent csv file downloaded
        DownloadUtility.Instance.DownloadFile(pin_id_csv_url, pin_id_csv_filepath + ".csv", downloadComplete);
    }
    #endregion Unity Methods

    #region Public Methods
    /// <summary>
    /// Reads in the csv file with users and pins, stores it in runtime data structs
    /// </summary>
    public void ParseCSV()
    {
        // Create new list of pin_id_records
        ids = new List<pin_id_record>();

        // Open csv as raw text, split into row entries
        string[] records = Resources.Load<TextAsset>(pin_id_csv_filepath).text.Split('\n');

        // Loop over the entries, skip the header row
        for (int i = 1; i < records.Length; i++)
        {
            // Split row into fields
            string[] fields = records[i].Split(',');

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
    }

    /// <summary>
    /// Returns true if the passed pin is found in the file of users and pins
    /// </summary>
    /// <param name="pin">Six digit pin number</param>
    /// <returns>true if the pin is found, false if not</returns>
    public bool AuthenticatePin(string pin)
    {
        return ids.Exists(x => x.pin == pin);
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
    private void downloadComplete()
    {
        ParseCSV();
    }
    #endregion Event Handlers
}
