using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMoveInStartPlain : MonoBehaviour
{
    [SerializeField]
    private LayerMask raycastLayermask;
    private float moveSpeed = 1f;
    private float gridSize = 0.24f;
    private enum Orientation
    {
        Horizontal,
        Vertical
    };
    private Orientation gridOrientation = Orientation.Horizontal;
    private bool allowDiagonals = false;
    private bool correctDiagonalSpeed = true;
    private Vector2 input;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float t;
    private float factor;
    private Animator animator;
    private Rigidbody2D body;
    private RaycastHit2D right;
    private RaycastHit2D up;
    private RaycastHit2D left;
    private RaycastHit2D down;

    public void Start()
    {
        Time.timeScale = 1;
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    public void Update()
    {
        if (!isMoving)
        {
            int horizontal = 0;
            int vertical = 0;

            horizontal = (int)Input.GetAxisRaw("Horizontal");
            vertical = (int)Input.GetAxisRaw("Vertical");

            input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (!allowDiagonals)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                {
                    gridOrientation = Orientation.Horizontal;
                    input.y = 0;
                }
                else
                {
                    gridOrientation = Orientation.Vertical;
                    input.x = 0;
                }
            }

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
            }
            else
            {
                animator.SetTrigger("DontMove");
            }

            if (input != Vector2.zero)
            {
                StartCoroutine(move(transform));
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("dedant");
        if (other.tag == "Cave")
        {
            SceneManager.LoadScene("Main");
        }
    }

    public IEnumerator move(Transform transform)
    {
        isMoving = true;
        startPosition = transform.position;
        t = 0;

        if (gridOrientation == Orientation.Horizontal)
        {
            endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y, startPosition.z + System.Math.Sign(input.y) * gridSize);

            right = Physics2D.Raycast(startPosition, Vector2.right, 0.24f, raycastLayermask);
            left = Physics2D.Raycast(startPosition, Vector2.left, 0.24f, raycastLayermask);
        }
        else
        {
            endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y + System.Math.Sign(input.y) * gridSize, startPosition.z);
            up = Physics2D.Raycast(startPosition, Vector2.up, 0.24f, raycastLayermask);
            down = Physics2D.Raycast(startPosition, Vector2.down, 0.24f, raycastLayermask);
        }

        if (right.collider != null && endPosition.x > startPosition.x)
        {
            endPosition = startPosition;
        }
        else if (left.collider != null && endPosition.x < startPosition.x)
        {
            endPosition = startPosition;
        }
        else if (up.collider != null && endPosition.y > startPosition.y)
        {
            endPosition = startPosition;
        }
        else if (down.collider != null && endPosition.y < startPosition.y)
        {
            endPosition = startPosition;
        }

        if (allowDiagonals && correctDiagonalSpeed && input.x != 0 && input.y != 0)
        {
            factor = 0.7071f;
        }
        else
        {
            factor = 1f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * (moveSpeed / gridSize) * factor;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        isMoving = false;
        yield return 0;
    }
}