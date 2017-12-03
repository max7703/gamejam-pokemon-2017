using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMoveInStartPlain : MovingObject
{

    public int wallDamage = 1;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private Vector2 touchOrigin = -Vector2.one;

    public int posX;
    public int posY;

    // Use this for initialization
    protected override void Start()
    {
        animator = GetComponent<Animator>();

        //scoreText.text = "Score: " + GameManager.instance.score;
        posX = (int)this.transform.position.x;
        posY = (int)this.transform.position.y;

        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            var posVec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var x = Mathf.RoundToInt(posVec.x) - transform.position.x;
            var y = Mathf.RoundToInt(posVec.y) - transform.position.y;
            if (Mathf.Abs(x) > Mathf.Abs(y))
            {
                horizontal = x > 0 ? 1 : -1;
            }
            else
            {
                vertical = y > 0 ? 1 : -1;
            }
        }

        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
        {
            if (horizontal != 0)
            {
                if (horizontal > 0)
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
    }

    protected override void AttemptMove<T>(int xPos, int yPos)
    {
        base.AttemptMove<T>(xPos, yPos);

        RaycastHit2D hit;
        if (Move(xPos, yPos, out hit))
        {
            posX = xPos;
            posY = yPos;
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

    }

    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);
        animator.SetTrigger("Attack");
    }

    private void Restart()
    {
        SceneManager.LoadScene(1);
    }
}
