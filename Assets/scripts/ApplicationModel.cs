using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Persistent data throughout application cycle
//used since main menu is own scene
public static class ApplicationModel
{
    //memory of current level for saving the game
    static public LevelAssociations currentLevel = LevelAssociations.MAINMENU;
    //bool to determine if player position and rotation should be loaded when the scene is loaded
    static public bool loadSaveGame = false;
    //memory of which savegame index to load from the save file
    static public int loadGameIndex = -1;
    //create the memory space that holds the savegame data once it gets loaded
    static public SaveGameContainer savegames = new SaveGameContainer();
    //flag to indicate state of loaded savegame display, -1 means not current 
    //should change to bool if not used to indicate different meaning other than true or false
    static public int isCurrent = -1;
    //flag to indicate prefs have been initialised, without this it kept giving an error
    static public bool prefsLoaded = false;
    //total amount of butterflies to be gotten in the demo level
    static public int maxButterflies = 5;
}

//Create well readable condition to understand which level should be loaded
//Set up for more than one level --> CA2
public enum LevelAssociations
{
    MAINMENU,
    LEVEL1, 
    LEVEL2, 
    LEVEL3,
    ENDING
}
