using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject uiPanel; 
    Transform playerTr; 
    Animator animator;
    bool isIntroComplete = false;
    GameObject woman;
    Vector3 startPos; // Woman 이 있는 위치
    Vector3 defaultPos = new Vector3(0, 0, -10); // 0,0, -10 기본위치
    float waitTime = 9.3f; //처음 Woman보여주는 시간
    // float speed = 0.1f; // 카메라 내려오는 속도  journeyTime으롷 대신한다.

    private AudioSource audioSource;
    public GameObject titleImage;

    void Start()
    {
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();

        // 음악 재생
        if (audioSource != null)
        {
            audioSource.Play();
        }

        StartCoroutine(HideUIPanelAfterDelay(9f));

        GameObject player = GameObject.Find("Player");
        playerTr = player.transform;

        // 카메라 시작위치 초기화
        woman = GameObject.Find("Woman");
        if(woman !=null){
            Vector3 temp = woman.transform.position;
            startPos = new Vector3(temp.x, temp.y+4, -10);
        }
        transform.position = startPos;
        animator = transform.GetComponent<Animator>();

        // Apply Root Motion을 꺼서 애니메이션이 이동을 방해하지 않도록 설정
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        StartCoroutine(MoveToPosition());
    }

    IEnumerator HideUIPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 대기
        if (uiPanel != null)
        {
            uiPanel.SetActive(false); // UI 패널 비활성화
        }
        Destroy(titleImage);
    }

    // 코루틴으로 1초 대기 후 천천히 내려오는 함수
    IEnumerator MoveToPosition()
    {
        // 1초 대기
        yield return new WaitForSeconds(waitTime);

        // 천천히 startPos에서 defaultPos로 이동
        float elapsedTime = 0f;
        float journeyTime = 6f; // 천천히 내려오는 시간 (2초)

        while (elapsedTime < journeyTime)
        {
            transform.position = Vector3.Lerp(startPos, defaultPos, elapsedTime / journeyTime);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 도착하면 위치를 정확히 defaultPos로 설정
        transform.position = defaultPos;

        isIntroComplete = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!isIntroComplete) return;

        if (playerTr.position.y <= 4.5f)
        {
            // transform.position = new Vector3(startPos.x, startPos.y, startPos.z);
            transform.position = defaultPos;
            print("카메라 위치: " + transform.position);
            print("플레이어 위치: " + playerTr.position);
        } else if (playerTr.position.y >4.5f && playerTr.position.y <= 8f){
                
            transform.position = new Vector3(transform.position.x, 4.5f,transform.position.z);
        } 
        // else if(playerTr.position.y > 8f && playerTr.position.y <=14f) 
        // {
        //     transform.position = new Vector3(transform.position.x, 10f, transform.position.z);
        // }
        // else if (playerTr.position.y > 14f && playerTr.position.y <16f)
        // {
        //     transform.position = new Vector3(transform.position.x, 16f, transform.position.z);
        // }
        else
        {
            transform.position = new Vector3(playerTr.position.z, playerTr.position.y, -10);
        }
    }
}
