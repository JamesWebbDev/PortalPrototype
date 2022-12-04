using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    #region Public Button Methods

    public void LoadScene(int index)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        if (index > sceneCount - 1)
        {
            Debug.LogError($"Scene at Index {index} couldn't be found!");
            return;
        }

        SceneManager.LoadScene(index);
    }

    #endregion

}
