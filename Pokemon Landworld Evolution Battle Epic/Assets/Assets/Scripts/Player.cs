using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject {

    public enum direction
    {
        up, down, left, right, none
    }

    public class pathElm
    {
        public float dist;
        public Vector2 pos;
        public direction prev;
    }

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

    private Vector2 target;
    private bool mousemovement;

    Vector2 follow_simple(Vector2 start, Vector2 target, TileType[][] map)
    {
        Debug.Log("start");
        Debug.Log(start);
        Debug.Log("target");
        Debug.Log(target);
        Debug.Log("length");
        Debug.Log(map.Length);
        Debug.Log(map[0].Length);
        //file des éléments en attente de traitement
        Queue queue = new Queue();
        //map des parents pour chaque case visitée
        Vector2[][] parents = new Vector2[map.Length][];
        for (int i = 0; i < map.Length; i++)
            parents[i] = new Vector2[map[i].Length];
        for (int i = 0; i < parents.Length; i++)
            for (int j = 0; j < parents[i].Length; j++)
                parents[i][j] = new Vector2(-1f,-1f);
        //enfilage du point de départ
        queue.Enqueue(start);
        bool found = false;
        //tant que la file est pas vide et que l'on a pas trouvé de chemin
        Debug.Log(parents[0][0]);
        while (queue.Count > 0 && !found)
        {
            Debug.Log(queue.Count);
            //on défile le premier élément et on le récupère
            Vector2 curr = (Vector2)queue.Dequeue();
            //on enfile les voisins si ce ne sont pas des murs et qu'ils n'ont pas déjà de parent
            if (curr.x > 0 && map[(int)curr.x - 1][(int)curr.y] != TileType.Wall && parents[(int)curr.x - 1][(int)curr.y] == new Vector2(-1f, -1f))
            {
                queue.Enqueue(new Vector2(curr.x - 1, curr.y));
                parents[(int)curr.x - 1][(int)curr.y] = curr;
            }
            if (curr.x < map.Length - 1 && map[(int)curr.x + 1][(int)curr.y] != TileType.Wall && parents[(int)curr.x + 1][(int)curr.y] == new Vector2(-1f, -1f))
            {
                queue.Enqueue(new Vector2(curr.x + 1, curr.y));
                parents[(int)curr.x + 1][(int)curr.y] = curr;
            }
            if (curr.y > 0 && map[(int)curr.x][(int)curr.y - 1] != TileType.Wall && parents[(int)curr.x][(int)curr.y - 1] == new Vector2(-1f, -1f))
            {
                queue.Enqueue(new Vector2(curr.x, curr.y - 1));
                parents[(int)curr.x][(int)curr.y - 1] = curr;
            }
            if (curr.y < map[0].Length - 1 && map[(int)curr.x][(int)curr.y + 1] != TileType.Wall && parents[(int)curr.x][(int)curr.y + 1] == new Vector2(-1f, -1f))
            {
                queue.Enqueue(new Vector2(curr.x, curr.y + 1));
                parents[(int)curr.x][(int)curr.y + 1] = curr;
            }
            //si on a trouvé la cible on sort (cette condition pourrait être placée dans l'enfilage des voisins pour améliorer les performances)
            if (curr.x == target.x && curr.y == target.y)
                found = true;
        }
        //si on a trouvé un chemin on le construit en partant de la fin et en se servant de la map de parents
        List<Vector2> path = new List<Vector2>();
        if (found)
        {
            Debug.Log("found");
            Vector2 curr = target;
            while (curr != start)
            {
                path.Insert(0, curr);
                curr = parents[(int)curr.x][(int)curr.y];
            }
            Debug.Log(path[0]);
            path.Insert(0, curr);
            Debug.Log(path[0]);
            //on retourne le vecteur allant du point de départ vers la première étape du chemin
            return path[1] ;//on suppose que le joueur n'a pas cliqué sur le personnage
        }
        //sinon on bouge pas
        return new Vector2(0f, 0f);
    }


    // Use this for initialization
    protected override void Start() {
        animator = GetComponent<Animator>();

        healthText = GameObject.Find("HealthText").GetComponent<Text>();
        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();

        health = GameManager.instance.playerHealthPoints;
        score = GameManager.instance.score;

        healthText.text = "Health: " + health;

        //scoreText.text = "Score: " + GameManager.instance.score;
        posX = (int)this.transform.position.x;
        posY = (int)this.transform.position.y;

        base.Start();
    }

    private void OnDisable()
    {
        GameManager.instance.score = score;
        GameManager.instance.playerHealthPoints = health;
    }

    // Update is called once per frame
    void Update() {
        if (!GameManager.instance.playersTurn || mousemovement)
        {
            if (mousemovement && GameManager.instance.playersTurn)
            {
                GameManager.instance.playersTurn = false;
                MouseMove();
            }
            return;
        }

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) && !mousemovement) {
            mousemovement = true;
			var posVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(Input.mousePosition);
			var x = Mathf.RoundToInt(posVec.x) ;
			var y = Mathf.RoundToInt (posVec.y);
            target = new Vector2(x, y);
            MouseMove();
			/*if (Mathf.Abs(x) > Mathf.Abs(y)) {
				horizontal = x > 0 ? 1 : -1;
			} else {
				vertical = y > 0 ? 1 : -1;
			}*/
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

	private void MouseMove ()
	{
        if (this.transform.position.x != posX || this.transform.position.y != posY)
            return;
        int horizontal = 0;
        int vertical = 0;
        Debug.Log (target.x);
		Debug.Log (target.y);
		GameObject manager = GameObject.FindGameObjectWithTag ("manager");
		TileType[][] tiles = manager.GetComponent<BoardCreator> ().getMap ();
        List<Enemy> enemies = manager.GetComponent<GameManager>().enemies;
        for (int i = 0; i < enemies.Count; i++)
        {
            tiles[enemies[i].posX][enemies[i].posY] = TileType.Wall;
        }
        if (tiles[(int)target.x][(int)target.y] == TileType.Wall) { }
        else
        {
            tiles = manager.GetComponent<BoardCreator>().getMap();
            Vector2 playerPos = new Vector2(posX, posY);
            Vector2 targetPos = new Vector2(target.x, target.y);
            Debug.Log("debut pathfinding");
            Vector2 nextPos = follow_simple(playerPos, targetPos, tiles);
            Debug.Log("path found");
            Debug.Log(nextPos);
            if (nextPos != new Vector2(0f, 0f))
            {
                AttemptMove<Wall>((int)nextPos.x, (int)nextPos.y);
                posX = (int)nextPos.x;
                posY = (int)nextPos.y;
            }
            else
            {
                if (targetPos == new Vector2(0f, 0f)) {
                    AttemptMove<Wall>((int)nextPos.x, (int)nextPos.y);
                    posX = (int)nextPos.x;
                    posY = (int)nextPos.y;
                }
            }

            if (posX == target.x && posY == target.y)
                mousemovement = false;

        }
        for (int i = 0; i < enemies.Count; i++)
        {
            tiles[enemies[i].posX][enemies[i].posY] = TileType.Floor;
        }
    }
			/*int horizontal = 0, vertical = 0;
			x -= posX;
			y -= posY;
			if (Mathf.Abs(x) > Mathf.Abs(y)) {
				horizontal = x > 0 ? 1 : -1;
			} else {
				vertical = y > 0 ? 1 : -1;
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
				AttemptMove<Wall>(posX + horizontal, posY + vertical);*/

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
        SceneManager.LoadScene("Main");
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
