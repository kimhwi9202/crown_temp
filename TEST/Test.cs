using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class Test : MonoBehaviour {
    public Image img1;
    public Image img2;
    public GameObject effect;
    public GameObject effect2;
    public Camera camera;



    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("mouse = " + Input.mousePosition);
            //if(FX.I != null) FX.I.test();
            test(img1.gameObject);
        }
    }


    void test(GameObject target)
    {
        //타겟의 포지션을 월드좌표에서 ViewPort좌표로 변환하고 다시 ViewPort좌표를 NGUI월드좌표로 변환합니다.
        Vector3 pos = camera.ViewportToWorldPoint(camera.WorldToViewportPoint(img1.GetComponent<Transform>().position));
        //Z는 0으로...
        pos.z = 90f;

        //타겟의 포지션을 월드좌표에서 ViewPort좌표로 변환하고 다시 ViewPort좌표를 NGUI월드좌표로 변환합니다.
        Vector3 pos2 = camera.ViewportToWorldPoint(camera.WorldToViewportPoint(img2.GetComponent<Transform>().position));
        //Z는 0으로...
        pos2.z = 90f;

        effect.GetComponent<Transform>().position = pos2;
        effect.SetActive(true);

        effect2.GetComponent<Transform>().position = pos2;
        effect2.SetActive(true);

        effect2.GetComponent<Transform>().DOMove(pos, 4f).SetEase(Ease.OutQuad).OnComplete(() =>
        {

        });
    
    }
}
