using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour {

    //references to all different menu canvases
    //to show and hide depending on user input
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public GameObject saveMenu;

    //reference to audiomixer used to influence sound volume
    public UnityEngine.Audio.AudioMixer audioMixer;
    //array of sounds to play in menu
    public AudioClip[] feedbackSounds;
    //mixergroup to assign the audio to
    public UnityEngine.Audio.AudioMixerGroup feedbackMixer;
    //audiosource to play menu sounds from
    private AudioSource audioSource;

    //since dictionary doesnt work
    //refence to all different sliders that coincide with the audio groups they influence
    public UnityEngine.UI.Slider masterSlider;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider feedbackSlider;
    public UnityEngine.UI.Slider sfxSlider;
    public UnityEngine.UI.Slider envSlider;  //env = environment

    //reference to dropdowns which will influence the graphics quality and game resolution
    public UnityEngine.UI.Dropdown qualityDropdown;
    public UnityEngine.UI.Dropdown resolutionDropdown;

    //reference to the fullscreen toggle, which will toggle fullscreen on and off
    public UnityEngine.UI.Toggle fullscreenToggle;

    //game paused flag, used to keep pause menu from showing when in different sub menus
    private bool paused = false;

    //reference to savegame script to load different savegames and display them on the savegame canvas
    private SaveGameScripts svScript;

    //hold all available screen resolutions of the current computer
    private List<string> resolutions = new List<string>();
    // current resolution index for the dropdown
    private int currentResolutionIndex = 0;

    //should be Awake() function but audiomixer is bugged in the Awake function 
    //bugged since 2015!!! 
    public void Start()
    {
        //get reference to savegame script from same gameobject
        svScript = gameObject.GetComponent<SaveGameScripts>();

        //get a list of strings with the available resolutions to be displayed in the dropdown
        Resolution[] options = Screen.resolutions;
        for (int i = 0; i < options.Length; i++) //iterate through all options to populate dropdown
        {
            //make string from width and height value
            resolutions.Add(options[i].width + "x" + options[i].height);

            if (options[i].width == Screen.currentResolution.width &&
                options[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        //make audio source to play sounds from and change mixer group
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = feedbackMixer;

        //threw error here if loaded before for unknown reason
        if (ApplicationModel.prefsLoaded)
            return;

        //if no player prefs exist, make them
        //otherwise load data from playerprefs to all volume groups
        if (!PlayerPrefs.HasKey("MasterVolume") || !PlayerPrefs.HasKey("SFXVolume") || !PlayerPrefs.HasKey("MusicVolume") 
                                            || !PlayerPrefs.HasKey("EnvVolume") || !PlayerPrefs.HasKey("FeedbackVolume"))
        {
            PlayerPrefs.SetFloat("MasterVolume", 0);
            PlayerPrefs.SetFloat("SFXVolume", 0);
            PlayerPrefs.SetFloat("MusicVolume", 0);
            PlayerPrefs.SetFloat("FeedbackVolume", 0);
            PlayerPrefs.SetFloat("EnvVolume", 0);
            //PlayerPrefs.SetInt("QualityIndex", 5); These are managed
            //PlayerPrefs.SetInt("Fullscreen", );    by unity itself
            //PlayerPrefs.SetString("Resolution", ); no need to manage them here
            PlayerPrefs.Save(); 
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", PlayerPrefs.GetFloat("MasterVolume"));
            audioMixer.SetFloat("FeedbackVolume", PlayerPrefs.GetFloat("FeedbackVolume"));
            audioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
            audioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
            audioMixer.SetFloat("EnvVolume", PlayerPrefs.GetFloat("EnvVolume"));
            //QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("QualityIndex"));
        }

        //set flag 
        ApplicationModel.prefsLoaded = true;


    }

    //load new game --> load first level
    public void StartNewGame()
    {
        //play a sound in menu
        PlaySound("forward");

        //set level reference to first level,
        ApplicationModel.currentLevel = LevelAssociations.LEVEL1;
        //load scene called "test_scene" (will be replaced for CA2)
        UnityEngine.SceneManagement.SceneManager.LoadScene("test_scene");
       // UnityEngine.SceneManagement.SceneManager.LoadScene("destroyed_city");
    }

    //show save menu, hide main menu
    public void ShowSaveMenu()
    {
        //play a sound in menu
        PlaySound("forward");

        saveMenu.SetActive(true);
        mainMenu.SetActive(false);

        //load the savegame from the file and display it on the savegame canvas
        svScript.displaySaveData();

    }

    //show options menu, hide main menu
    public void ShowOptionsMenu()
    {
        //play a sound in menu
        PlaySound("forward");

        //set slider to value of audio mixer, so it doesn't jump when the user uses the slider the first time
        //in menu
        float outVal;
        audioMixer.GetFloat("MasterVolume", out outVal);
        masterSlider.value = outVal;
        audioMixer.GetFloat("MusicVolume", out outVal);
        musicSlider.value = outVal;
        audioMixer.GetFloat("FeedbackVolume", out outVal);
        feedbackSlider.value = outVal;
        audioMixer.GetFloat("SFXVolume", out outVal);
        sfxSlider.value = outVal;
        audioMixer.GetFloat("EnvVolume", out outVal);
        envSlider.value = outVal;

        //clear the resolution dropdown menu if used previously
        resolutionDropdown.ClearOptions();

        //set the dropdowns to match the playerpref settings
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        resolutionDropdown.AddOptions(resolutions); //populate dropdown
        resolutionDropdown.value = currentResolutionIndex; //set dropdown to current resolution
        resolutionDropdown.RefreshShownValue(); //refresh dropdown to show changed value

        //set the fullscreen toggle to display right setting
        fullscreenToggle.isOn = Screen.fullScreen;

        optionsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    //show main menu from any menu
    public void ShowMainMenu()
    {
        //play a sound in menu
        PlaySound("back");

        //disable any menu that is not the main menu --> call from either sub menu to get back to main menu
        //show main menu
        optionsMenu.SetActive(false);
        saveMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    //quit the game from main menu
    public void QuitGame()
    {
        //show debug log in editor (and in debug log in appdata)
        //since the game won't quit when in editor
        Debug.Log("quit");
        Application.Quit();
    }

    //adjust all volumes through master volume group
    //save the new value to playerprefs
    public void AdjustMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    //adjust feedback sound volume directly
    //save the new value to playerprefs
    public void AdjustFeedbackVolume(float volume)
    {
        audioMixer.SetFloat("FeedbackVolume", volume);
        PlayerPrefs.SetFloat("FeedbackVolume", volume);
        PlayerPrefs.Save();
    }

    //adjust music volume directly
    //save the new value to playerprefs
    public void AdjustMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    //adjust effects volume directly
    //save the new value to playerprefs
    public void AdjustSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    //adjust environment volume directly
    //save the new value to playerprefs
    public void AdjustEnvVolume(float volume)
    {
        audioMixer.SetFloat("EnvVolume", volume);
        PlayerPrefs.SetFloat("EnvVolume", volume);
        PlayerPrefs.Save();
    }

    //hide the menu, disable pause mode
    public void ClosePauseMenu()
    {
        //play a sound in menu
        PlaySound("back");

        //set game unpaused by enabling timestep
        Time.timeScale = 1;
        //disable menus
        DisableMenu();
    }

    //ge to mainmenu from any kind of level 
    public void LoadMainMenu()
    {
        //play a sound in menu
        PlaySound("forward");

        //unpause time in game, paused flag will be reloaded from scratch so does not have to be set
        Time.timeScale = 1;
        //load the main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    //disable all menues and set the game unpaused flag
    public void DisableMenu()
    {
        //play a sound in menu
        PlaySound("back");

        //hide menus
        mainMenu.SetActive(false);
        optionsMenu.SetActive(false);
        saveMenu.SetActive(false);

        //set flag
        this.paused = false;
    }

    public void SetQuality(int qualIndex)
    {
        QualitySettings.SetQualityLevel(qualIndex);
        //PlayerPrefs.SetInt("QualityIndex", qualIndex);
        //PlayerPrefs.Save();

    }

    //set the game to run in fullscreen or windowed mode
    public void SetFullscreen(bool isFullscreen)
    {
        Debug.Log("is fullscreen: " + isFullscreen);
        //set screen settings
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        //get resolution that was selected
        Resolution[] res = Screen.resolutions;
        //set new resolution from selected resolution and current screen mode (windowed or fullscreen)
        Screen.SetResolution(res[resolutionIndex].width, res[resolutionIndex].height, Screen.fullScreen);

        Debug.Log(res[resolutionIndex].width + "x" + res[resolutionIndex].height);
    }

    //set game paused from outside this script
    public void SetPaused(bool paused)
    {
        this.paused = paused;
    }

    //return if game is paused
    public bool GetPaused()
    {
        return this.paused;
    }

    //play sound in menu
    void PlaySound(string button)
    {
        //going back from a menu
        if (button == "back")
            audioSource.clip = feedbackSounds[1];
        //going to something from a menu
        else if (button == "forward")
            audioSource.clip = feedbackSounds[2];
        //all other feedback in menu
        else
            audioSource.clip = feedbackSounds[3];

        audioSource.PlayOneShot(audioSource.clip);
    }
}
