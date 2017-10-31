using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

using DG.Tweening;

namespace Games.UI
{
    public class SpinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public static SpinButton I;

        public Button _btnSpin;
        public Image[] imgNormal;
        public Image[] imgAuto;
        public float fAutoPressedTime = 1f;

        protected float fDelayTime = 0.05f;
        protected bool bClickDown = false;
        protected bool bPressed = false;
        protected float fCurTime = 0;
        protected bool play = false;
        protected bool bLock = false;

        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onLongPress = new UnityEvent();

        protected void Awake()
        {
            I = this;
        }
        protected void Start()
        {
            bLock = false;
            play = false;
            imgNormal[0].gameObject.SetActive(false);
            imgNormal[1].gameObject.SetActive(true);
            imgAuto[0].gameObject.SetActive(false);
            imgAuto[1].gameObject.SetActive(false);
        }

        void onClickTimeEvent()
        {
            bClickDown = false;
            bPressed = true;
            imgAuto[0].gameObject.SetActive(false);
            imgAuto[1].gameObject.SetActive(true);
            onLongPress.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (bClickDown || bLock) return;
            bClickDown = true;
            play = false;

            if (bPressed)
            {
                imgAuto[1].gameObject.SetActive(false);
                imgAuto[0].gameObject.SetActive(true);
                imgAuto[0].transform.DOScale(0.9f, 0.05f).SetEase(Ease.OutCubic);
            }
            else
            {
                imgNormal[1].gameObject.SetActive(false);
                imgNormal[0].gameObject.SetActive(true); // click image
                imgNormal[0].transform.DOScale(0.98f, fDelayTime).SetEase(Ease.OutCubic);
                fCurTime = Time.time;
                play = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!bClickDown || bLock) return;
            play = false;

            if (bPressed)
            {
                imgAuto[0].transform.DOScale(1f, fDelayTime).SetEase(Ease.OutCubic).OnComplete(() => {
                    imgAuto[0].gameObject.SetActive(false);
                    imgAuto[1].gameObject.SetActive(false);
                    imgNormal[0].gameObject.SetActive(false); // click image
                    imgNormal[1].gameObject.SetActive(true); // normal image
                    bPressed = false;
                    onClick.Invoke();
                    bClickDown = false;
                });
            }
            else 
            {
                imgNormal[0].transform.DOScale(1f, fDelayTime).SetEase(Ease.OutCubic).OnComplete(() => {
                    imgNormal[0].gameObject.SetActive(false);
                    imgNormal[1].gameObject.SetActive(true);
                    onClick.Invoke();
                    bClickDown = false;
                });
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            play = false;
        }

        void FixedUpdate()
        {
            if (play && !bLock)
            {
                if ((Time.time - fCurTime) >= fAutoPressedTime)
                {
                    play = false;
                    onClickTimeEvent();
                }
            }
        }

        // func api
        public void SetLock(bool block)
        {
            bLock = block;
        }

        public bool IsPressed()
        {
            return bPressed;
        }

        public void SetStateNormalButton()
        {
            play = false;
            bClickDown = false;
            bPressed = false;
            imgAuto[0].gameObject.SetActive(false);
            imgAuto[1].gameObject.SetActive(false);
            imgNormal[0].gameObject.SetActive(false);
            imgNormal[1].gameObject.SetActive(true);
        }

        public void SetStateAutoButton()
        {
            play = false;
            bClickDown = false;
            bPressed = true;
            imgAuto[0].gameObject.SetActive(false);
            imgAuto[1].gameObject.SetActive(true);
            imgNormal[0].gameObject.SetActive(false);
            imgNormal[1].gameObject.SetActive(false);
        }
    }
}