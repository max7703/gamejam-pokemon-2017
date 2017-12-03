using UnityEngine;
using System.Collections;

public class Enemy : MovingObject {

	public enum States
	{
		Patrol , Chase , Return
	}

	public int playerDamage;
	public int health;

	private Animator animator;
	private Vector2 target;
	public AudioClip enemyAttack1;
	public AudioClip enemyAttack2;


	// The path the ennemy will have to take
	public Vector2[] path = new Vector2[4];
	public int nextPoint = 1;
	public States state = States.Patrol;

	public int posX;
	public int posY;
	
	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
//		target = GameObject.FindGameObjectWithTag ("Player").transform;
		base.Start ();
		posX = (int)this.transform.position.x;
		posY = (int)this.transform.position.y;
        moveTime = 0.05f;
	}
	public void setPath(int x1, int y1, int width, int height){
		path [0].x = x1;
		path [0].y = y1;
		path [1].x = x1 + width -1;
		path [1].y = y1;
		path [2].x = x1 + width -1;
		path [2].y = y1 + height -1;
		path [3].x = x1;
		path [3].y = y1 + height -1;
	}

	protected override void AttemptMove <T> (int xPos, int yPos)
	{
		base.AttemptMove<T> (xPos, yPos);
		if (canMove) {
			posX = xPos;
			posY = yPos;
		}
	}

	public void MoveEnemy()
	{
		target = path [nextPoint];

		int xDir = 0;
		int yDir = 0;

		if (Mathf.Abs (target.x - transform.position.x) < float.Epsilon)
			yDir = target.y > transform.position.y ? 1 : -1;
		else
			xDir = target.x > transform.position.x ? 1 : -1;
		int xPos = posX + xDir;
		int yPos = posY + yDir;
		AttemptMove <Player> (xPos, yPos);
		if (posX == path [nextPoint].x && posY == path [nextPoint].y) {
			nextPoint++;
			if (nextPoint == 4)
				nextPoint = 0;
		}
	}

	protected override void OnCantMove <T> (T component)
	{
		Player hitPlayer = component as Player;

		hitPlayer.LooseHealth (playerDamage);

		animator.SetTrigger ("Attack");

		SoundManager.instance.RandomizeSfx (enemyAttack1, enemyAttack2);
	}
}
