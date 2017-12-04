using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Loader : MonoBehaviour {

	public GameObject gameManager;

	// Use this for initialization
	void Awake () {
		if (GameManager.instance == null)
			Instantiate (gameManager);
	}

    public void LoadLevel(string level)
    {
        SceneManager.LoadScene(level);
        //SoundManager.instance.efxSource.Stop();
    }
}
