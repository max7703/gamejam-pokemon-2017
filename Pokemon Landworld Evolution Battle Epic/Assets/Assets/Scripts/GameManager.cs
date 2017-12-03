using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public float levelStartDelay = 2f;
	public float turnDelay = .1f;
	public static GameManager instance = null;
	public int playerHealthPoints = 100;
	[HideInInspector] public bool playersTurn = true;
    public int score = 0;

	private Text levelText;
	private GameObject levelImage;
	public int level = 0;
	private List<Enemy> enemies;
    private GameObject buttonRestart;
    private bool enemiesMoving;
	private bool doingSetup;

	// Use this for initialization
	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

        //DontDestroyOnLoad(gameObject);
        ////Destroy(gameObject);
        enemies = new List<Enemy> ();
		InitGame ();
	}

	void InitGame()
	{
		doingSetup = true;

        levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text> ();
		levelText.text = "Cave";
		levelImage.SetActive (true);
		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear ();

        buttonRestart = GameObject.Find("RestartButton");
        buttonRestart.SetActive(false);
    }

	private void HideLevelImage()
	{
		levelImage.SetActive (false);
		doingSetup = false;
	}

	public void GameOver()
	{
        buttonRestart.SetActive(true);
        levelText.text = "Game Over";
        levelImage.SetActive (true);
		enabled = false;
        Time.timeScale = 0;
        //SceneManager.LoadScene("Main");
    }
	
	// Update is called once per frame
	void Update () {
		if (playersTurn || enemiesMoving || doingSetup)
			return;

		StartCoroutine (MoveEnemies ());
	}

	public void AddEnemyToList(Enemy script)
	{
		enemies.Add (script);
	}

	IEnumerator MoveEnemies()
	{
		enemiesMoving = true;
		yield return new WaitForSeconds (turnDelay);
		if (enemies.Count == 0) {
			yield return new WaitForSeconds (turnDelay);
		}

		for (int i = 0; i < enemies.Count; i++) {
			enemies [i].MoveEnemy ();
			yield return new WaitForSeconds (enemies [i].moveTime);
		}

		playersTurn = true;
		enemiesMoving = false;
	}

    public void Victory()
    {
        buttonRestart.SetActive(true);
        levelText.text = "Victory !!!";
        levelImage.SetActive(true);
        Time.timeScale = 0;
        //SceneManager.LoadScene("Main");
    }
}
