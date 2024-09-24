using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    float horizontal;
    float speed = 5f;
    float jumpingPower = 8f;
    bool isFacingRight = true;

    bool isGrounded;
    bool isWalled; 

    bool isWallSliding;
    bool isWallJumping;

    float wallJumpingDirection;
    Vector2 wallJumpingPower = new Vector2(1.5f, 3.5f); // 대각선 45도로 

    float wallJumpDelay = 1.3f; // 1.3초 동안 점프가능, 그 후에는 점프안됨
    bool canWallJump = false; // 점프 대기 시간 동안 점프 가능한지 여부
    float wallSlideTimer; // 슬라이딩 타이머
    float wallSlidingStartDelay = 1.3f; // 슬라이딩 시작 지연시간

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    public Sprite endSprite; // 게임끝났을 때 보여줄 스프라이트
    public GameObject fireworksPrefab;

    public Animator playerAnimator;

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // 점프 처리
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        // 벽 슬라이드와 점프 로직 처리
        WallSlide();
        WallJump();

        // 플립 처리 (방향 전환)
        if (!isWallJumping)
        {
            Flip();
        }

        // playerAnimator에 xVelocity,yVelocity, isGround전달
        playerAnimator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x)); // <--- x축 속도 전달 (절대값으로)
        playerAnimator.SetFloat("yVelocity", rb.velocity.y); // <--- y축 속도 전달 (수직 속도)
        playerAnimator.SetBool("isGrounded", isGrounded); // <--- isGrounded 값을 Animator에 전달
    }

    void FixedUpdate()
    {
        // 땅 체크 및 벽 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        isWalled = Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

        if(isWalled){
            isWallSliding = false;
            wallSlideTimer = 0f; //  벽을 떠나면 타이머 초기화
            canWallJump = true; // 벽 점프 가능 상태로 변경
        }

        // 벽 점프 중이 아닐 때만 이동 처리
        if (!isWallJumping)
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            }
            else
            {
                // 공중에 있을 때는 서서히 감속
                float airResistance = 0.95f; // 공중에서의 감속비율
                if (horizontal == 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x * airResistance, rb.velocity.y);
                }
                else
                {
                    rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
                }
            }
        }
    }

    void WallSlide()
    {
        if (isWalled && !isGrounded && horizontal != 0f) //벽. 땅위. 좌우버튼 안눌림
        {
            // 벽에 붙은 후 슬라이딩 처리
            wallSlideTimer += Time.deltaTime;

            if(wallSlideTimer >= wallSlidingStartDelay){
                isWallSliding = true;
            } 
        }
    }

    void WallJump()
    {
        if (isWalled && canWallJump) // 벽에 붙어 있고 점프 가능할 때
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isWallJumping = true;
                wallJumpingDirection = -transform.localScale.x; // 반대 방향 설정

                // 반대 방향으로 비스듬히 점프
                rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x , wallJumpingPower.y);

                // 플레이어 방향 전환
                if (transform.localScale.x != wallJumpingDirection)
                {
                    isFacingRight = !isFacingRight;
                    Vector3 localScale = transform.localScale;
                    localScale.x *= -1f;
                    transform.localScale = localScale;
                }

                // 반대편 벽에 닿았을 때 슬라이딩이 되지 않도록 슬라이드 타이머 초기화
                wallSlideTimer = 0f;

                StartCoroutine(EndWallJump());
            }
        }
    }

    IEnumerator EndWallJump()
    {
        // 일정 시간 후 벽 점프 상태 종료
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 벽에 부딪쳤을 때 점프 유예시간 설정
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            // 벽에 충돌한 Y좌표 저장
            float contactY = collision.contacts[0].point.y;

            // 플레이어 Y좌표를 contactY로 설정하여 슬라이드 업 방지
            if (transform.position.y > contactY)
            {
                transform.position = new Vector2(transform.position.x, contactY);
                rb.velocity = new Vector2(rb.velocity.x, 0); // Y축 속도 0으로 해서 정지시킴
            }
            StartCoroutine(ResetGravityAfterDelay()); // 중력초기화
            canWallJump = true; // 벽 점프 가능 상태로 변경
            StartCoroutine(WallJumpCooldown());
            
        }
        if (collision.gameObject.tag =="Woman"){
            SpriteRenderer spriteRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
            if(spriteRenderer !=null){
                spriteRenderer.sprite = endSprite;
            }

            // 폭죽효과 실행
            StartCoroutine(DoFireworks(collision.transform.position));
        }
        if (collision.gameObject.tag =="Dead"){
            Destroy(transform);
        }

    }

    IEnumerator WallJumpCooldown()
    {
        // wallJumpDelay(1.5초) 후에 점프 가능 상태 해제
        yield return new WaitForSeconds(wallJumpDelay);
        canWallJump = false;
    }

    IEnumerator ResetGravityAfterDelay()
    {
        rb.gravityScale = 0; // 중력을 0으로 설정
        yield return new WaitForSeconds(wallSlidingStartDelay);
        rb.gravityScale = 1; // 중력을 다시 1로 설정
    }

    void Flip()
    {
        // 플레이어의 방향을 키 입력에 따라 전환
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    IEnumerator DoFireworks(Vector3 position){
        //파티클을 생성하고 잠시 후 파괴
        GameObject fireworks = Instantiate(fireworksPrefab, position, Quaternion.identity);
        fireworks.GetComponent<ParticleSystem>().Play();
        // 2초후 파티클 파괴
        yield return new WaitForSeconds(2f);
        Destroy(fireworks);

    }
}
