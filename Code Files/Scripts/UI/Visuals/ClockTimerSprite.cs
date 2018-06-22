using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Author: Josue
//Last edited: Josue 11/30/2017

public class ClockTimerSprite : MonoBehaviour
{
    [SerializeField]
    private Image SpriteImage;      //sprite image in the center of the circle
    [SerializeField]
    private Image RadialFillImage;  //cooldown radial circle 

    //Set sprite of image to the name provided
    public void SetSprite(string name)
    {
        SpriteImage.sprite = Resources.Load<Sprite>(name);
    }

    //set the fill amount of the radial
    public void SetFillAmount(float value, bool isRoomCountdown = false)
    {
        RadialFillImage.fillAmount = value;

        //fade icon if not 0 for abilities. fade icon if 0 for room cooldown items
        if (isRoomCountdown)
        {
            if (value != 1.0f)
            {
                if (SpriteImage.color.a != 0.5f)
                {
                    SetAlpha(0.5f);
                }

                return;
            }
            else
            {
                RadialFillImage.fillAmount = 0.0f;

                //set icon back to regular opacity
                if (SpriteImage.color.a != 1.0f)
                {
                    SetAlpha(1.0f);
                }

                return;
            }
        }

        if (value != 0)
        {
            //fade out icon
            if (SpriteImage.color.a != 0.5f)
            {
                SetAlpha(0.5f);
            }
        }
        else
        {
            //set icon back to regular opacity
            if (SpriteImage.color.a != 1.0f)
            {
                SetAlpha(1.0f);
            }
        }
    }

    private void SetAlpha(float a)
    {
        Color newColor = SpriteImage.color;
        newColor.a = a;
        SpriteImage.color = newColor;
    }
}
