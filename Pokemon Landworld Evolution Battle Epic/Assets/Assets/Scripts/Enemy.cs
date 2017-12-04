using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private Player player;


	// The path the ennemy will have to take
	public Vector2[] path = new Vector2[4];
	public int nextPoint = 1;
	public States state = States.Patrol;
    private int turnsLeft;

	public int posX;
	public int posY;

    TileType[][] tiles;

    private Vector2 returnPoint;
	
	protected override void Start () {
		GameManager.instance.AddEnemyToList (this);
		animator = GetComponent<Animator> ();
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
		base.Start ();
		posX = (int)this.transform.position.x;
		posY = (int)this.transform.position.y;
        moveTime = 0.05f;
        GameObject manager = GameObject.FindGameObjectWithTag("manager");
        tiles = manager.GetComponent<BoardCreator>().getMap();
    }

    Vector2 follow_simple(Vector2 start, Vector2 target, TileType[][] map)
    {
		if (start == target)
			return target;
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
                parents[i][j] = new Vector2(-1f, -1f);
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
            if (path.Count > 1)
                return path[1];//on suppose que le joueur n'a pas cliqué sur le personnage
            else
                return path[0];
        }
        //sinon on bouge pas
        return new Vector2(0f, 0f);
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
        if (state == States.Patrol)
        {
            if (Mathf.Abs(player.posX - posX) + Mathf.Abs(player.posY - posY) <= 3)
            {
                state = States.Chase;
                turnsLeft = 4;
                returnPoint = new Vector2(posX, posY);
            }
            else
            {
                if (Mathf.Abs(target.x - transform.position.x) < float.Epsilon)
                    yDir = target.y > transform.position.y ? 1 : -1;
                else
                    xDir = target.x > transform.position.x ? 1 : -1;
                int xPos = posX + xDir;
                int yPos = posY + yDir;
                AttemptMove<Player>(xPos, yPos);
                if (posX == path[nextPoint].x && posY == path[nextPoint].y)
                {
                    nextPoint++;
                    if (nextPoint == 4)
                        nextPoint = 0;
                }
            }
        }
        if (state == States.Chase)
        {
            if (turnsLeft == 0)
            { 
                state = States.Return;
            }
            else
            {
                Vector2 start = new Vector2(posX, posY);
                Vector2 target = new Vector2(player.posX, player.posY);
                Vector2 nextPos = follow_simple(start, target, tiles);
                if (nextPos != new Vector2(0f, 0f))
                {
                    AttemptMove<Player>((int)nextPos.x, (int)nextPos.y);
                    if (nextPos != target)
                    {
                        posX = (int)nextPos.x;
                        posY = (int)nextPos.y;
                    }
                }
                turnsLeft--;
            }
            
        }
        if(state == States.Return)
        {
            Vector2 start = new Vector2(posX, posY);
            Vector2 nextPos = follow_simple(start, returnPoint, tiles);
            if (nextPos != new Vector2(0f, 0f))
            {
                AttemptMove<Player>((int)nextPos.x, (int)nextPos.y);
                posX = (int)nextPos.x;
                posY = (int)nextPos.y;
            }
            if (returnPoint == nextPos)
                state = States.Patrol;
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
