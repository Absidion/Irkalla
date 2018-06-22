using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedManager : Photon.PunBehaviour
{
    public InputField SeedFieldMaster;                        //the input field where the player is able to set the seed value. This is also the location where the seed string is visually represented. MasterClient.
    public InputField SeedFieldClient;                        //the input field where the player is able to set the seed value. This is also the location where the seed string is visually represented. Client.

    void Awake()
    {
        if (SeedFieldMaster == null)
        {        
            Debug.LogError("SeedField input field is null, please add the correct input field to this object ", this);
        }
        //set the input fields onValueChanged to look at the ValueChanged function when the value within the inputfield is changed
        SeedFieldMaster.onValueChanged.AddListener(delegate { ValueChanged(); });
        RandomizeSeed();   
    }

    private void Update()
    {
        //if it's not my photonview don't update the scene
        if (SeedFieldMaster == null || SeedFieldClient == null)
            return;

        //if the SeedField's text component isn't equal to the m_SeedString value then set them to be equal
        if (SeedFieldMaster.text != GameManager.Instance.SeedString)
            SeedFieldMaster.text = GameManager.Instance.SeedString;

        SeedFieldClient.text = SeedFieldMaster.text;
    }

    private void RandomizeSeed()
    {
        //create a temporary string to hold the value of our seed
        string randomizedSeed = string.Empty;

        //loop through for as many characters as we need to fill the inputfield
        for (int i = 0; i < SeedFieldMaster.characterLimit; i++)
        {
            //check whether or not the seed should be filled with a letter or number
            int letterOrNumber = Random.Range(0, 2);
            if (letterOrNumber == 0)
            {
                //add a random value to the seed between 0-9
                randomizedSeed += Random.Range(1, 10);
            }
            else
            {
                char value = (char)Random.Range(65, 90);
                randomizedSeed += value;
            }
        }
        //save the seed value
        GameManager.Instance.SeedString = randomizedSeed;
        //set the value within the seedField
        SeedFieldMaster.text = GameManager.Instance.SeedString;
        ConvertSeedToNumberValue();
    }


    public void RandomizeSeedMasterClient()
    {
        //make sure that only the photon master client can randomize the seed
        if (!PhotonNetwork.isMasterClient)
            return;

        RandomizeSeed();
    }

    //converts the seed value into a number
    private void ConvertSeedToNumberValue()
    {
        GameManager.Instance.SeedValue = 0;
        //loop through the elements in the seedstring 
        for (int i = 0; i < GameManager.Instance.SeedString.Length; i++)
        {
            //and multiply the seedvalue by them
            GameManager.Instance.SeedValue += GameManager.Instance.SeedString[i];
            GameManager.Instance.SeedValue *= GameManager.Instance.SeedString[i];
        }
        //get the absolute value of the seedvalue that way we don't have any negative numbers
        GameManager.Instance.SeedValue = Mathf.Abs(GameManager.Instance.SeedValue);        
    }

    //the value changed function which is called when anything within the SeedField inputfield is changed
    private void ValueChanged()
    {
        //if we aren't the master client then don't allow them to change the seed's value
        if (!PhotonNetwork.isMasterClient)
            return;

        //make sure that both the seed string and seed value are set to what they're suppossed to be set to
        GameManager.Instance.SeedString = SeedFieldMaster.text;
        ConvertSeedToNumberValue();
    }
}
