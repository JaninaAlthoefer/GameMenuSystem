using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveGameScripts : MonoBehaviour
{
    //reference to camera to get the screenshot for savegame from
    private Camera playerCamera;
    //reference to player to get savedata from
    private GameObject player;

    //reference to which slot of the savegame file is to be used
    private int slotIndex = -1;

    //references to the UI elements for showing the save name, date, image
    //and the complete panel to show which of the panels is activated, if any
    public UnityEngine.UI.Text[] names, dates;
    public UnityEngine.UI.Image[] images, panels;

    //string to reference the persistent data path of the app, to store the savegame files in
    private string folderpath;

    //filename and image sub-name atrings
    private const string filepath = "/data.gpca";
    private const string imagepath = "/SaveScreen";

    //Initialise core objects for the player and camera 
    //unsure if more than one camera will be used later, would need to modify if true
    void Start () {
        
        //set up necessary references for saving game
        //i.e. player to save data from and camera to take screenshot from
        player = GameObject.FindGameObjectWithTag("Player");
        playerCamera = Camera.main;
        folderpath = Application.persistentDataPath;

        //if coming from load screen, load player data
        if (ApplicationModel.loadSaveGame)
        {
            SceneStartLoadData();
        }
    }

    //testing functionality during development
    private void Update()
    {
        //testing functionality
        //if (Input.GetKeyDown("b"))
        //{
        //    SaveGame();
        //}
        //else if (Input.GetKeyDown("n"))
        //{
        //    displaySaveData();
        //}
        //else if (Input.GetKeyDown("m"))
        //{
        //    LoadData();
        //}
    }

    //update which savegame slot the player wants to save / load from
    //calls function to update view of slots
    public void changeSlotIndex(int index)
    {
        //if for any reason index doesn't match, return from function without errors
        if (index < -1 || index > 3)
        {
            slotIndex = -1;
            return;
        }

        //if slot already selected, deselect it
        if (slotIndex == index)
        {
            panels[slotIndex-1].color = new Color32(255, 255, 255, 100);
            slotIndex = -1;
            return;
        }

        //otherwise, update index of savegame data slot
        slotIndex = index;
        //update the view to show which slot is selected
        updateView();
    }

    //change colors of save slots to show which slot is used
    private void updateView()
    {
        for (int i = 0; i<3; i++) 
        {
            if ((slotIndex-1) == i)
            {
                //darker grey to signify activation
                panels[i].color = new Color32(150, 150, 150, 100);
            }
            else
            {
                //white color to signify deactivated state
                panels[i].color = new Color32(255, 255, 255, 100);
            }
        }
    }
    
    //adjust set up parameters to load save upon new scene load, load appropriate scene
    //uses specified index for data to load
    public void LoadData()
    {
        //debugging load, disable after full implementation
        //LoadSaveGames();

        //return from function if selected slot is not in use
        if (ApplicationModel.savegames.data[slotIndex-1].IsInUse == false)
            return;

        //set flag to load player state at start of next scene load
        ApplicationModel.loadSaveGame = true;
        //set which slot to load from upon next scene load
        ApplicationModel.loadGameIndex = slotIndex - 1;

        //switch statement to determine which level/scene is to be loaded
        switch(ApplicationModel.savegames.data[slotIndex-1].levelToLoad)
        {
            case LevelAssociations.LEVEL1:
                ApplicationModel.currentLevel = LevelAssociations.LEVEL1; //set current level to loaded level, for saving purposes
                UnityEngine.SceneManagement.SceneManager.LoadScene("test_scene"); //load scene
                //UnityEngine.SceneManagement.SceneManager.LoadScene("destroyed_city"); //load scene
                break;
            case LevelAssociations.LEVEL2:
                //ApplicationModel.currentLevel = LevelAssociations.LEVEL2;
                //break;  --> not used atm since no level 2
            case LevelAssociations.LEVEL3:
                //ApplicationModel.currentLevel = LevelAssociations.LEVEL3;
                //break;  --> not used atm since no level 3
            default: //if no cases match, load main menu --> basic error handling
                ApplicationModel.loadSaveGame = false; //don't load player values since no player exists in mainmenu
                ApplicationModel.loadGameIndex = -1; //reset savedata index
                ApplicationModel.currentLevel = LevelAssociations.MAINMENU;  //set current level to level being loaded
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); //load scene main menu
                break; //in case further switch cases are added after this

        }

        
    }

    //load player parameters and other save game features from loading the scene
    public void SceneStartLoadData()
    {
        //return if savegame shouldn't be loaded or index doesn't match savedata slots
        if (!ApplicationModel.loadSaveGame || ApplicationModel.loadGameIndex < 0)
            return;

        //return if savegame in specified slot isn't used
        if (ApplicationModel.savegames.data[ApplicationModel.loadGameIndex].IsInUse == false)
            return;

        //apply saved data to player
        player.transform.position = ApplicationModel.savegames.data[ApplicationModel.loadGameIndex].PlayerPosition;
        player.transform.rotation = ApplicationModel.savegames.data[ApplicationModel.loadGameIndex].PlayerRotation;
        player.GetComponent<PlayerInput>().setCanSpawn(ApplicationModel.savegames.data[ApplicationModel.loadGameIndex].canSpawn);
        player.GetComponent<PlayerInput>().SetSpawnerImage(ApplicationModel.savegames.data[ApplicationModel.loadGameIndex].canSpawn);

        //reset savedata flags for next scene load
        ApplicationModel.loadGameIndex = -1;
        ApplicationModel.loadSaveGame = false;
    }

    //Take screenshot, save to disk, then save neccessary values to the savegames
    //specified in slot and save data to disk
    public void SaveGame()
    {
        //set up a target texture to hold the screenshot image 
        RenderTexture rt = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24);
        //retain reference to currently used render texture
        RenderTexture currentRt = RenderTexture.active;
        //briefly set the rendered image to be put to the screenshot texture
        RenderTexture.active = rt;
        //briefly change the target of the camera to the screenshot texture instead of the computer screen
        playerCamera.targetTexture = rt;
        //render an image to the screenshot texture
        playerCamera.Render();
        //create a texture to hold the rendertexture / screenshot image before it's saved to disk
        Texture2D screen = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        //get each pixel from the rendered screenshot stored in the texture
        screen.ReadPixels(new Rect(0,0, rt.width, rt.height), 0, 0);
        //store the pixels in the texture
        screen.Apply();

        //reset rendertexture and target texture to the prior values to show game on screen again
        RenderTexture.active = currentRt;
        playerCamera.targetTexture = null;

        //create byte structure to hold the screenshot image in file, encoded as PNG
        byte[] image = screen.EncodeToPNG();
        //get the file path for the image
        string path = folderpath + imagepath + slotIndex + ".png";
        //write the image to the disk
        File.WriteAllBytes(path, image);

        //create new savedata object to serialise to json to store to file
        SaveGameData save = new SaveGameData(slotIndex-1, player.transform.position, player.transform.rotation, player.GetComponent<PlayerInput>().getCanSpawn(), path);
        //assign new savedata object to the specified slot of the savedata list
        ApplicationModel.savegames.data[slotIndex-1] = save;

        //create Json from the serialisable savegame data
        string json = JsonUtility.ToJson(ApplicationModel.savegames);

        //Debug.Log("Savegame json: ");  --> testing functionality
        //try to set up binary writer to open a file stream and save data to the specified file
        //code in using statement won't be executed if binary writer can't be initialised
        using (BinaryWriter bw = new BinaryWriter(File.Open(folderpath + filepath, FileMode.Create)))
        {
            //write data to file and close the file stream
            bw.Write(json);
            bw.Close();
        }

        //reset all data flags for 
        ApplicationModel.isCurrent = -1;
        panels[slotIndex-1].color = new Color32(255, 255, 255, 100);
        slotIndex = -1;
        //update the savedata display after save was made
        displaySaveData();
    }

    //File IO to read the savegame file and convert from json string to class structure
    private void LoadSaveGames()
    {
        //if savegame file is unchanged since last time, return from loading file
        if (ApplicationModel.isCurrent > 0)
            return;

        //string to hold serialised json 
        string json;

        Debug.Log(folderpath + filepath);

        //if the file containing the savegame data exists
        if (File.Exists(folderpath + filepath))
        {
            //try to open the filestream to read the data
            using (BinaryReader br = new BinaryReader(File.Open(folderpath + filepath, FileMode.Open)))
            {
                //read the string contained in the file into the json holder and close filestream
                json = br.ReadString();
                br.Close();
                //convert json string to savegame class structure
                ApplicationModel.savegames = JsonUtility.FromJson<SaveGameContainer>(json);
                //set flag to not load file again if not unchanged somewhere else
                ApplicationModel.isCurrent = 1;
            } 
        }

        
        //Debug.Log(json);
    }

    //Display all savedata on the Save/Load screen
    public void displaySaveData()
    {
        //load current savegames into the data structure
        LoadSaveGames();

        // if none of the savegame slots are in use, return
        if (ApplicationModel.savegames.data[0].IsInUse == false 
            && ApplicationModel.savegames.data[1].IsInUse == false
            && ApplicationModel.savegames.data[2].IsInUse == false)
            return;

        //if there's at least one slot in use, iterate through all savegame slots
        for (int i = 0; i < ApplicationModel.savegames.data.Count; i++)
        {
            //if the current savegame slot is in use
            if (ApplicationModel.savegames.data[i].IsInUse)
            {
                //set savegame name and date
                names[i].text = ApplicationModel.savegames.data[i].Name;
                dates[i].text = ApplicationModel.savegames.data[i].Date;

                //load image from specified file on disk into byte array
                byte[] image = File.ReadAllBytes(ApplicationModel.savegames.data[i].Screenshot);
                //make texture to hold image to be displayed on the savegame panel
                Texture2D tex = new Texture2D(1,1);
                //load image bystes from the array into the texture
                tex.LoadImage(image);

                //make a sprite to be used on the savegame image texture
                Sprite sprite; // = new Sprite();
                //create the sprite with the loaded texture and it's specified size
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width/2, tex.height/2));

                //set the created image sprite to the image on the panel
                images[i].sprite = sprite;
            }
        }
    }

}

//Data class for each save game
//Data format, simple but efficient for actual game data and savegame display
//Properties do not work with JSON serialisation, the Utility only returns "{}"
[System.Serializable]
public class SaveGameData
{
    public bool IsInUse; // { get; set; }
    public string Name; // { get; set; }
    public string Date; // { get; set; }
    public Vector3 PlayerPosition; // { get; set; }
    public Quaternion PlayerRotation; // { get; set; }
    public LevelAssociations levelToLoad; // { get; set; }
    public bool canSpawn; //is player able to spawn items yet
    public string Screenshot; // { get; set; }

    //standard constructor for unused savegame slot
    public SaveGameData()
    {
        this.IsInUse = false;
        this.Name = "";
        this.Date = "";
        this.PlayerPosition = Vector3.zero;
        this.PlayerRotation = Quaternion.identity;
        this.levelToLoad = LevelAssociations.MAINMENU;
        this.canSpawn = false;
        this.Screenshot = "";
    }

    //constructor for used savegame slot, containing current player data, screenshot and index to be stored in
    //all other data can be derived from game and level
    public SaveGameData(int index, Vector3 position, Quaternion rotation, bool spawner, string screenshotPath)
    {
        this.IsInUse = true;
        this.Name = "Savegame" + (index+1);
        this.Date = System.DateTime.Now + "";
        this.PlayerPosition = position;
        this.PlayerRotation = rotation;
        this.levelToLoad = ApplicationModel.currentLevel;
        this.canSpawn = spawner;
        this.Screenshot = screenshotPath;
    }
}


//Container for all the different save games, although a list is used for ease of access
//the save games are limited to 3 slots
//Properties do not work with JSON serialisation
[System.Serializable]
public class SaveGameContainer
{
    
    public List<SaveGameData> data = new List<SaveGameData>(); // { get; set; }

    public SaveGameContainer()
    {
        data.Add(new SaveGameData());
        data.Add(new SaveGameData());
        data.Add(new SaveGameData());
    }

}