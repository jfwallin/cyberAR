using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class autofill : MonoBehaviour
{
    #region Public Variables 
    [Tooltip("This is how many characters are typed until a new list of names is requested.")]
    public int lengthQueue = 3;
    public int optionLimit = 5;
    #endregion 

    #region Private Variables
    private List<string> names = new List<string>();
    private Text input;
    private string currText;
    private Dropdown dropdown; 
    private int showing = 0;
    private List<User> users = new List<User>();
    public Dictionary<string, Dictionary<string, string>> crnLabsUrl;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        // initialize textfield 
        input = transform.GetChild(0).GetComponent<Text>();
        dropdown = GetComponent<Dropdown>();
        currText = input.text;
        refreshText();


        // TESTING - Name bank to pull autofill from 
        // string[] nameslist = new string[] { "guest", "jim", "sam", "chad", "cronie", "crestin", "crestin2", "crestion", "crestolemu", "crester", "crestunker", "Michael", "Christopher", "Jessica", "Matthew", "Ashley", "Jennifer", "Joshua", "Amanda", "Daniel", "David", "James", "Robert", "John", "Joseph", "Andrew", "Ryan", "Brandon", "Jason", "Justin", "Sarah", "William", "Jonathan", "Stephanie", "Brian", "Nicole", "Nicholas", "Anthony", "Heather", "Eric", "Elizabeth", "Adam", "Megan", "Melissa", "Kevin", "Steven", "Thomas", "Timothy", "Christina", "Kyle", "Rachel", "Laura", "Lauren", "Amber", "Brittany", "Danielle", "Richard", "Kimberly", "Jeffrey", "Amy", "Crystal", "Michelle", "Tiffany", "Jeremy", "Benjamin", "Mark", "Emily", "Aaron", "Charles", "Rebecca", "Jacob", "Stephen", "Patrick", "Sean", "Erin", "Zachary", "Jamie", "Kelly", "Samantha", "Nathan", "Sara", "Dustin", "Paul", "Angela", "Tyler", "Scott", "Katherine", "Andrea", "Gregory", "Erica", "Mary", "Travis", "Lisa", "Kenneth", "Bryan", "Lindsey", "Kristen", "Jose", "Alexander", "Jesse", "Katie", "Lindsay", "Shannon", "Vanessa", "Courtney", "Christine", "Alicia", "Cody", "Allison", "Bradley", "Samuel", "Shawn", "April", "Derek", "Kathryn", "Kristin", "Chad", "Jenna", "Tara", "Maria", "Krystal", "Jared", "Anna", "Edward", "Julie", "Peter", "Holly", "Marcus", "Kristina", "Natalie", "Jordan", "Victoria", "Jacqueline", "Corey", "Keith", "Monica", "Juan", "Donald", "Cassandra", "Meghan", "Joel", "Shane", "Phillip", "Patricia", "Brett", "Ronald", "Catherine", "George", "Antonio", "Cynthia", "Stacy", "Kathleen", "Raymond", "Carlos", "Brandi", "Douglas", "Nathaniel", "Ian", "Craig", "Brandy", "Alex", "Valerie", "Veronica", "Cory", "Whitney", "Gary", "Derrick", "Philip", "Luis", "Diana", "Chelsea", "Leslie", "Caitlin", "Leah", "Natasha", "Erika", "Casey", "Latoya", "Erik", "Dana", "Victor", "Brent", "Dominique", "Frank", "Brittney", "Evan", "Gabriel", "Julia", "Candice", "Karen", "Melanie", "Adrian", "Stacey", "Margaret", "Sheena", "Wesley", "Vincent", "Alexandra", "Katrina", "Bethany", "Nichole", "Larry", "Jeffery", "Curtis", "Carrie", "Todd", "Blake", "Christian", "Randy", "Dennis", "Alison", "Trevor", "Seth", "Kara", "Joanna", "Rachael", "Luke", "Felicia", "Brooke", "Austin", "Candace", "Jasmine", "Jesus", "Alan", "Susan", "Sandra", "Tracy", "Kayla", "Nancy", "Tina", "Krystle", "Russell", "Jeremiah", "Carl"}; // TESTING
        // foreach (string name in nameslist) { names.Add(name.ToLower()); }    
        names.Add("guest"); users.Add(new User("guest","guest","0000000")); crnLabsUrl = new Dictionary<string, Dictionary<string, string>>() {{"0000000",new Dictionary<string, string>() {{"guest_Lab","www.guestLab.json/url"}}}};

        // building localized User databases
        crnLabsUrl = pullCSV(); // NOT WORKING ON LEAP
        sort(names);

        // Print out User and crn data to make sure everything is loading in properly 
        // foreach (string crn in crnLabs.Keys) { string outtie = ""; foreach (string str in crnLabs[crn]) { outtie += str + " "; } print(outtie);  }
        
        // Prints each user with crn and password for testing
        // foreach (User usr in users) { print(usr.ToString()); }
    }


    // Update is called once per frame
    void Update()
    {
        // Only way to consistantly update and make it visible 
        if (input.text.Length >= lengthQueue && showing > 0) { dropdown.Show(); }
        else { dropdown.Hide(); }

        // On change of text 
        if (!currText.Equals(input.text))
        {
            currText = input.text;
            print("Username updated to: " + currText);     // for debugging

            // update options 
            updateDropdown();
        }
    }


    #region Public Methods 
    // helps re-adjust dropdown options if authentication fails
    public void refreshText()
    {
        currText = input.text;
        showing = 0;

    }


    // Authenticates given usr/pas - NOT TO BE STORED ON DEVICE LONGTERM
    public bool authenticate(string usr, string pas)
    {
        try {
            // Find User
            User user = users.Find(x => x.usr.Equals(usr));

            // Check if pass is correct
            bool authenticated = user.pas.Equals(pas);
            Debug.Log("Username: " + usr + ", Password: " + pas + "\n\t    Authenticated: " + authenticated);
            return authenticated;
        } catch  {
            print("Authentication Failed: Invalid Username");
            return false;
        }
    }


    // Called when textbox is changed: checks if enough letters are typed before giving up to a set limit of possible usernames
    public void updateDropdown()
    {
        // Helps refresh dropdown menu
        dropdown.Hide(); 


        if (input.text.Length >= lengthQueue)
        {
            // Empty list before adding elemnts
            dropdown.options.Clear();   
            int count = 0;

            // Cycle names list and pick ones that contain provided string
            foreach (string name in names)
            {
                if (name.Contains(input.text) && count < optionLimit)
                {
                    dropdown.options.Add(new Dropdown.OptionData(name));
                    count++;
                }
            } 
            showing = count;    // Ensures that if no options appear, a blank dropdown won't appear
        }

    }


    // Returns a Dictionary of <string lab, string jsonURL>
    // Can only be called after the user is authenticated -> throws error if cant find CRN provided for authenticated user in Dictionary
    public Dictionary<string,string> getLabs(string usr)
    {
        string crn = "0000000";  // Guest crn: seven 0s
        try { 
            crn = users.Find(x => x.usr.Equals(usr)).crn; 
        } catch 
        { 
            Debug.Log("CSV not loaded, or forced next();");
        }

        try { 
            return crnLabsUrl[crn]; 
        } catch 
        { 
            print("CRN Dictionary is missing, returning a blank dicitonary"); 
            return new Dictionary<string, string>(){ { "guest","default_json_url"} }; 
        }
    }
    #endregion


    #region Private Methods 
    /* Loads names into a list, Username+Password+CRN into a list of UserObjects, and CRN + Hashset<labs> in a dictionary.
     * @return: Dictionary of crns with set of lab IDs.
     */
    private Dictionary<string, Dictionary<string, string>> pullCSV()
    {
        var result = new Dictionary<string, Dictionary<string, string>>();

        // Set path for where to pull data from
        string namesPath = "Assets/Login Scene/csv bank/test_names.csv";
        string crnPath = "Assets/Login Scene/csv bank/crn_to_labs.csv";

        // Get String array of the lines and read them off
        string[] lines = System.IO.File.ReadAllLines(namesPath);      

        // 0: username, 1: Name, 2: CRN, 3: Instructor, 4: Password
        for (int i = 1; i < lines.Length; i++)  // Skips labeling row
        {
            // Split into columns
            string[] columns = lines[i].Split(',');

            // Add names into autofill list and create new User(username, passowrd, crn)
            names.Add(columns[0]);
            users.Add(new User(columns[0], columns[4], columns[2]));
        }

        // Load in CRN - Lab/JsonUrl dictionary
        lines = System.IO.File.ReadAllLines(crnPath);            

        // 0: CRN, Odd: Lab Name, Even: JsonUrl associated with lab
        for (int i = 1; i < lines.Length; i++)  // Skips labeling row
        {
            // Split row into columns 
            string[] columns = lines[i].Split(',');
            
            // Create new Dictionary and add values for each pair in the row 
            var tempDic = new Dictionary<string, string>();
            try
            {
                for (int j = 1; j < columns.Length; j++)
                {
                    tempDic.Add(columns[j], columns[++j]);
                }
            } 
            catch 
            { 
                Debug.Log("crn " + columns[0] + " has an incomplete crn/jsonpath pair.");
            }

            // Prints CRN with associated dictionary values. If there are multiple lines dedicated to the same crn, they will print on different lines 
            // written on one line for quick toggling 
            // string strs = ""; foreach (string s in tempDic.Keys) { strs += "Lab: " + s + "\t url: " + tempDic[s] + "\n"; } print(strs);

            // Adds crn and Lab/json to dictionary unless it exists => adds to existing
            try { 
                result.Add(columns[0], tempDic);
            }
            catch {
                foreach (string str in tempDic.Keys) 
                { 
                    result[columns[0]].Add(str, tempDic[str]); 
                } 
            }
        }

        return result;
    }


    // Sorts names in the autofilled list
    private void sort(List<string> names)
    {
        names.Sort();                                               // used for List<string> implementation 
        // Array.Sort(names, (s1, s2) => String.Compare(s1, s2));   // used for string[] implementation
    }
    #endregion
}


/* Used to store Student Usernames, Passwords, and CRNs as of now 
 * All objects are added to a list where they can be searched
*/
class User 
{
    public string usr, pas, crn;

    public User(string usr, string pas, string crn)
    {
        this.usr = usr;
        this.pas = pas;
        this.crn = crn; 
    }

    public override string ToString() { return ("Username: " + usr + "\tPassword: " + pas + "\t\tCRN: " + crn); }
}
