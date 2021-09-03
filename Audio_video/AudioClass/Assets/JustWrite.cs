using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace Assets
{
    [CreateAssetMenu(fileName = "JustWrite", menuName = "ScriptableObjects/JustWrite", order = 1)]
    public abstract class JustWrite : ScriptableObject
    {

        public int number = 4;
        public abstract void WrtieToSring(string item1, string item2, string item3 , string item4);

        //public string path = "Assets/Resources/test2.txt";
      /*
        public void start()
        {
            WriteString();
        }
            public void WriteString(string type = null, string name = null, string info = null, string test = null)
            {


                string Path = "Assets/Resources/test2.txt";
                //Write some text to the test.txt file
                StreamWriter writer = new StreamWriter(Path, true);
                // Console.WriteLine("write to String has been called");
                writer.WriteLine($"inside WriteToFile {name} is {type}");
                writer.Close();
            }
      */ 
        }

    
}
