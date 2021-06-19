using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace sorting
{
        public class sortInfo
        {
            public GameObject theObject;
            public int sortedOrder;
            public bool isSorted;

            public float fractionalDistance;

            // implement IComparable interface
            public int CompareTo(object obj)
            {
                if (obj is sortInfo)
                {
                    return this.fractionalDistance.CompareTo((obj as sortInfo).fractionalDistance);  // compare user names
                }
                else
                {
                    return 0;
                }
                //throw new ArgumentException("Object is not a sortInfo");
            }
        }


}
