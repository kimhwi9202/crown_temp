using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniControl : MonoBehaviour {

    public Animator _anim = null;
    public float _endTime = 0;
    public bool _autoClose = true;
    public System.Action _complete = null;


    void Awake()
    {
        if(_anim == null) _anim = this.gameObject.GetComponent<Animator>();
    }
    public void Play()
    {
        if (_autoClose) StartCoroutine(coEndCheck());
    }

    bool AnimatorIsPlaying()
    {
        return _anim.GetCurrentAnimatorStateInfo(0).length >
               _anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && _anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    public IEnumerator coEndCheck()
    {
        yield return new WaitForSeconds(_endTime);
        if (_complete != null) _complete();
        this.gameObject.SetActive(false);
    }

}
