using System.IO;
using System.Xml;
using UnityEngine;

public static class XmlUtilities
{
    //Get's an item from the XML file related to the items. Takes the rarity of the item and the index it's located at in the list
    public static string GetNameFromItemXML()
    {
        string name = string.Empty;
        //get the file path of the Xml item list
        TextAsset xmlfile = Resources.Load("Xmls/Items") as TextAsset;

        using (XmlReader reader = XmlReader.Create(new StringReader(xmlfile.text)))
        {
            reader.ReadToFollowing("Amount");

            //get the number of items in the XML under the rarity
            int index = Random.Range(1, reader.ReadElementContentAsInt() + 1);

            for (int i = 0; i < index; i++)
            {
                //read to the next name value, we do this until we get to the desired value
                reader.ReadToFollowing("Name");
            }

            //save the value into the name
            name = reader.ReadElementContentAsString();
        }
        //return that name value which get's used to instantiate a type
        return name;
    }

    public static string GetNameFromShopItemXML(string rarity, out int value)
    {
        string name = string.Empty;
        //get the file path of the Xml item list
        TextAsset xmlfile = Resources.Load("Xmls/ShopItems") as TextAsset;

        int itemValue = -1;

        using (XmlReader reader = XmlReader.Create(new StringReader(xmlfile.text)))
        {
            reader.ReadToFollowing(rarity);
            reader.ReadToFollowing("Amount");

            int index = Random.Range(1, reader.ReadElementContentAsInt() + 1);

            for (int i = 0; i < index; i++)
            {
                //read to the next name value, we do this until we get to the desired value
                reader.ReadToFollowing("Name");
                //save the value into the name
                name = reader.ReadElementContentAsString();
                //read to the value of the item
                reader.ReadToFollowing("Value");
                //save the value as an int which represents the items actual value in the shop
                itemValue = reader.ReadElementContentAsInt();
            }
        }
        //set the out value
        value = itemValue;

        //return that name value which get's used to instantiate a type
        return name;
    }

    public static string GetNameFromMonsterXML(int minWeight, int maxWeight)
    {
        //the name of the monster that we are looking to get from the file
        string name = string.Empty;

        TextAsset xmlfile = Resources.Load("Xmls/MonsterManualXML") as TextAsset;

        using (XmlReader reader = XmlReader.Create(new StringReader(xmlfile.text)))
        {
            //get the amount of items that are currently in that tier that we can choose from. IE if it pulls 2 then the value will be randomized between 1 and 2
            int count = -1;

            //get a value between the two weights
            int weightToCheck = Random.Range(minWeight, maxWeight + 1);

            reader.ReadToFollowing("Weight" + weightToCheck);       //read to the proper weight value                   

            reader.ReadToFollowing("Count");
            //finally read the element as a int and if the count is greater then zero that means we can grab an enemy from that tier
            //else we repeat until we find a usable weight    
            count = reader.ReadElementContentAsInt();

            if (count < 1)
                return string.Empty;

            int index = Random.Range(0, count) + 1;

            for (int i = 0; i < index; i++)
            {
                //read to the next value, name
                reader.ReadToFollowing("Name");
            }

            //save the value in the reader
            name = reader.ReadElementContentAsString();
        }

        return name;
    }
}
