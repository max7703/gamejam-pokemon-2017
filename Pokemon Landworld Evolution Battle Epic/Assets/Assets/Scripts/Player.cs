using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

	public int wallDamage = 1;
	public int pointsPerFood = 10;
	public int pointsPerSoda = 20;
	public float restartLevelDelay = 1f;
	private Text healthText;
    private Text scoreText;
    public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;

	private Animator animator;
	private int health;
    private int score;
    private Vector2 touchOrigin = -Vector2.one;

	public int posX;
	public int posY;

	// Use this for initialization
	protected override void Start () {
		animator = GetComponent<Animator> ();

        healthText = GameObject.Find("HealthText").GetComponent<Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();

        health = GameManager.instance.playerHealthPoints;
        score = GameManager.instance.score;

        healthText.text = "Health: " + health;

        //scoreText.text = "Score: " + GameManager.instance.score;
		posX = (int)this.transform.position.x;
		posY = (int)this.transform.position.y;

        base.Start ();
	}

	private void OnDisable()
	{
        GameManager.instance.score = score;
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
        {
            if(horizontal != 0)
            {
                if(horizontal>0)
                {
                    animator.SetTrigger("MoveRight");
                }
                else
                {
                    animator.SetTrigger("MoveLeft");
                }
            }
            else
            {
                if (vertical > 0)
                {
                    animator.SetTrigger("MoveUp");
                }
                else
                {
                    animator.SetTrigger("MoveDown");
                }
            }
            AttemptMove<Wall>(posX + horizontal, posY + vertical);
        }

        CheckIfVictory();
    }

    protected override void AttemptMove <T> (int xPos, int yPos)
	{
        health--;

		healthText.text = "Health: " + health;

        scoreText.text = "Score: " + score;

        base.AttemptMove <T> (xPos, yPos);

		RaycastHit2D hit;
		if (Move ( xPos, yPos, out hit)) {
			posX = xPos;
			posY = yPos;
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
            score += pointsPerFood;
			SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
			other.gameObject.SetActive (false);
		} else if (other.tag == "Soda") {
            health += pointsPerSoda;
            score += pointsPerSoda;
			SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
			other.gameObject.SetActive (false);
		}
        healthText.text = "Health: " + health;
        scoreText.text = "Score: " + score;
    }

	protected override void OnCantMove <T> (T component)
	{
		Wall hitWall = component as Wall;
		hitWall.DamageWall (wallDamage);
		animator.SetTrigger ("Attack");
	}

	private void Restart()
	{
        SceneManager.LoadScene(1);
    }

	public void LooseHealth (int loss)
	{
		animator.SetTrigger ("GetHit");
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
        if (score >= 100)
        {
            //SoundManager.instance.PlaySingle(victorySound);
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.Victory();
        }
    }
}
