using UnityEngine;
using System.Collections.Generic;

//included 'Current_Season' from 'VOC_SeasonalThemes.cs'
public enum Current_Season { NONE, HALLOWEEN, NOVEMBER, CHRISTMAS, WINTER, ANNIVERSARY_INITIAL_RELEASE, ANNIVERSARY_STEAM_RELEASE }

[System.Serializable]
public class StringMapDictionary : SerializableDictionary<Current_Season, string> { }

[CreateAssetMenu(menuName = "Federico/All/Map")]
public class MapInformation : ScriptableObject
{
    //you should care about this
    public string mapName = "mapnameinhost";
    public string sceneName = "mapnameinbuildsettings";
    public StringMapDictionary sceneNameSeasons;
    public List<string> mapsMode = new List<string>() { "CORE_VOC_DM", "CORE_VOC_TDM", "CORE_VOC_TDM_ZOMBIE", "CORE_VOC_DM_MELEE_FIGHT", "CORE_VOC_DM_ARMS_RACE" };
    public string description = "Insert Description";
    public Sprite mapPicture;
    public Sprite loadingImage;


    //ignore this
    public bool isselectable = true;
    public bool isWorkshopLevelEditor = false;
}