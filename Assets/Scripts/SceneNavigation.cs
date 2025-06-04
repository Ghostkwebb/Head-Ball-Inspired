using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneNavigation : MonoBehaviour
{

    public void LoadMainGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void LoadCpuVsCpu()
    {
        SceneManager.LoadScene("cpyVScpu"); 
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}



//Difficulty [1-10]
//Shadows
//pause screen
//update fucntion should have functions not logics
//variables instead of hardcode
//Effect on kick
//Sound effects

//android port
//goal text when goal achieved

