using UnityEngine;

namespace MusicReplacer.MusicReplacementMenu;

public class MusicMenuEnabler : MonoBehaviour
{
    public static MusicMenuEnabler Main { get; private set; }

    public NUIMainMenu mainMenu;
    public GameObject homeTab;
    public GameObject mainMenuTab;
    public GameObject musicMenu;
    public GameObject levelMusicTab;

    private void Awake()
    {
        Main = this;
    }

    public void SetTab(Tab tab)
    {
        musicMenu.SetActive(tab == Tab.MusicReplacer);
        mainMenuTab.SetActive(tab == Tab.MainMenu);
        mainMenu.enabled = tab  == Tab.MainMenu;
        homeTab.SetActive(tab == Tab.MusicEditorHome);
        levelMusicTab.SetActive(tab == Tab.LevelMusic);
    }

    public enum Tab
    {
        MainMenu,
        MusicEditorHome,
        MusicReplacer,
        LevelMusic
    }
}