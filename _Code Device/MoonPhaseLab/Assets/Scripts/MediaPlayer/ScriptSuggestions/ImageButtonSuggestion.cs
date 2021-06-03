using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageButtonSuggestion : MonoBehaviour
{
    public void HandleOnClickEvent()
    {

        gameObject.SendMessage("ImageSwitch", "card_first_quarter_image");

    }

    //Send message uses reflection, which is a costly operation. It also costs more the more components you have on an object
    //Suggestion
    /// [SerializeField]
    /// private ImageChanger changer = null; //This gets assigned in the unity editor, a reference directly to the ImageChanger
    /// 
    /// public void HandleOnClickEvent()
    /// {
    ///     changer.ImageSwitch("card_first_quarter_image");
    /// }
    /// 
    /// OR
    /// 
    /// Add the following function to ImageChanger:
    /// public void HandleOnClick1()
    /// {
    ///     ImageSwitch("card_first_quarter_image");
    /// }
    /// //Then, you can still have the button link to this function, and then it doesn't need a reference 
    /// //to ImageChanger because it is inside it.
}
