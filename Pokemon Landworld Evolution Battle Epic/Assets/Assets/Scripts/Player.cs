using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MovingObject {

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	public Text healthText;
    public Text scoreText;
    public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;
	private int health;
	private Vector2 touchOrigin = -Vector2.one;

	// Use this for initialization
	protected override void Start () {
		animator = GetComponent<Animator> ();

        health = GameManager.instance.playerHealthPoints;

		healthText.text = "Health: " + health;

        scoreText.text = "Score: " + GameManager.instance.score;

        base.Start ();
	}

	private void OnDisable()
	{
		GameManager.instance.playerHealthPoints = health;
	}

	// Update is called once per frame
	void Update () {
		if (!GameManager.instance.playersTurn)
			return;

		int horizontal = 0;
		int vertical = 0;

		horizontal = (int)Input.GetAxisRaw ("Horizontal");
		vertical = (int)Input.GetAxisRaw ("Vertical");

		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
			var posVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			var x = Mathf.RoundToInt(posVec.x) - transform.position.x;
			var y = Mathf.RoundToInt(posVec.y) - transform.position.y;
			if (Mathf.Abs(x) > Mathf.Abs(y)) {
				horizontal = x > 0 ? 1 : -1;
			} else {
				vertical = y > 0 ? 1 : -1;
			}
		}

		if (horizontal != 0)
			vertical = 0;

		if (horizontal != 0 || vertical != 0)
			AttemptMove<Wall> (horizontal, vertical);

        CheckIfVictory();
    }

    protected override void AttemptMove <T> (int xDir, int yDir)
	{
        health--;
		healthText.text = "Health: " + health;

		base.AttemptMove <T> (xDir, yDir);

		RaycastHit2D hit;
		if (Move (xDir, yDir, out hit)) {
			SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
		}

		CheckIfGameOver ();

		GameManager.instance.playersTurn = false;
	}

	private void OnTriggerEnter2D (Collider2D other)
	{
		if (other.tag == "Exit") {
			Invoke ("Restart", restartLevelDelay);
			enabled = false;
		} else if (other.tag == "Food") {
            health += pointsPerFood;
            GameManager.instance.score += pointsPerFood;
            healthText.text = "Health: " + health;
			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
            health += pointsPerSoda;
            GameManager.instance.score += pointsPerSoda;
            healthText.text = "Health: " + health;
			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
			other.gameObject.SetActive (false);
		}
        scoreText.text = "Score: " + GameManager.instance.score;
    }

	protected override void OnCantMove <T> (T component)
	{
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);
		animator.SetTrigger ("playerChop");
	}

	private void Restart()
	{
		Application.LoadLevel (Application.loadedLevel);
	}

	public void LoseFood (int loss)
	{
		animator.SetTrigger ("playerHit");
        health -= loss;
		healthText.text = "Health: " + health;
		CheckIfGameOver ();
	}

	private void CheckIfGameOver()
	{
		if (health <= 0) {
			SoundManager.instance.PlaySingle(gameOverSound);
			SoundManager.instance.musicSource.Stop();
			GameManager.instance.GameOver ();
		}
	}

    private void CheckIfVictory()
    {
        if (GameManager.instance.level >= 10)
        {
            //SoundManager.instance.PlaySingle(victorySound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.Victory();
        }
    }
}
