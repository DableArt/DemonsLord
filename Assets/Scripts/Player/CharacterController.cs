using UnityEngine;

public class TopDownCharacterController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    
    
    public bool rotateToMovement = true;

    private Rigidbody2D rb;
    private Vector2 input;
    private float currentSpeed;
    private Animator anim;
    private SpriteRenderer sr;

    private Vector2 lastMoveDir = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;
        sr = GetComponent<SpriteRenderer>();

        currentSpeed = moveSpeed;
    }
   
    
    void Update()
    {
        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");
        input = input.normalized;

        bool isMoving = input.sqrMagnitude > 0.001f;

        if (isMoving)
            lastMoveDir = input;

        // --- Параметры для Animator ---
        // Должны существовать в Animator: IsMoving (bool), MoveX (float), MoveY (float)
        anim.SetBool("IsMoving", isMoving);

        Vector2 dirForAnim = isMoving ? input : lastMoveDir;

        if (dirForAnim.x > 0.01f) sr.flipX = false;
        else if (dirForAnim.x < -0.01f) sr.flipX = true;

        anim.SetFloat("MoveX", dirForAnim.x);
        anim.SetFloat("MoveY", dirForAnim.y);


    }

   void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * currentSpeed * Time.fixedDeltaTime);

        
    }
}
