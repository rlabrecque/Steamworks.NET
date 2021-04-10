using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamAPI.Samples
{
    public class Page3Demo : MonoBehaviour
    {
        public UnityEngine.UI.RawImage avatarImage;
        public UnityEngine.UI.Text userName;

        private void OnEnable()
        {
            /*************************************************************
             * 
             * Heathen Engineering's tools are designed to make
             * working with the Steam API simpler for Unity developers
             * 
             * Discoverabiliity is a key part of that. By discoverable we
             * mean that you should be able to discover features and 
             * capabilities via intelisence in the IDE. To that end we have 
             * structured our objects in a logicle flow. See below for a 
             * simple example.
             * 
             *************************************************************/

            avatarImage.texture = SteamSettings.Client.user.avatar;
            userName.text = SteamSettings.Client.user.DisplayName;

            /*************************************************************
             * 
             * The first call fetches the local user's avatar. Using the 
             * raw Steamworks.NET API you would need to download the file
             * listen on the appropreate callback, convert the resulting 
             * data into a proper Unity Texture 2D and then you could use 
             * it. 
             * 
             * In the second call you could have simply written:
             * SteamFriends.GetPersonaName();
             * Which is fewer characters to type but may not have been 
             * intuative. Note you can still use the raw Steam API without
             * worry of breaking Heathen's extensions.
             * 
             *************************************************************/

        }
    }
}
