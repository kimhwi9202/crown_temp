using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using xLIB;

/// <summary>
/// 사운드 매니져
/// - 사운드 패키지별로 관리 가능 ( 폴더명이 패키지 키 값이다 ) , 사운드 폴더별로 관리 필요
/// - 오디오소스 버퍼는 5개만 이용 ( 다음 업글시에 캐싱기능 추가 할생각 )
/// - 오디오클립 관리가 현재 모든 패키지를 하나로 처리됨 
///   ( 다음 업글시에 패키지별로 관리 하도록 기능 향상 시킬 생각 )
/// </summary>
public class SOUND : Singleton<SOUND>
{
    private float masterVolume = 1.0f;
    // value 값은 Pack폴더이름 가질거다..현재는 사용안함
    private Dictionary<AudioSource, int> audioBufferDic = new Dictionary<AudioSource, int>();
    // 오디오클립의 폴더별 그룹리스트다 ( 폴더명은 중복되면 안되며, 사운드 클립이름도 중복하면 안된다)
    // 플레이 함수가 오디오팩 아이디를 요구하지 않기 때문에 순차적으로 검색해서 플레이 처리한다.
    private Dictionary<string, AudioClip[]> packageDic = new Dictionary<string, AudioClip[]>();

    private bool _SoundOn = true;

    #region 내부함수들
    /// <summary>
    /// Idle 상태 오디오소스 얻기
    /// </summary>
    private AudioSource GetIdleAudioSource()
    {
        foreach (var pair in audioBufferDic)
        {
            if (!pair.Key.isPlaying && pair.Value == 0)
                return pair.Key;
        }
        
        // new create
        GameObject go = new GameObject("AudioSource");
        AudioSource source = go.AddComponent<AudioSource>();
        go.transform.SetParent(this.transform);
        audioBufferDic.Add(source, 0);

        return source;
    }
    /// <summary>
    /// 플레이 중인 오디오소스 얻기
    /// </summary>
    private AudioSource GetPlayingAudioSource(string clipName)
    {
        foreach (var pair in audioBufferDic)
            if(pair.Key.clip)
                if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                    if (pair.Key.isPlaying)
                        return pair.Key;
        return null;
    }
    private AudioSource GetPauseAudioSource(string clipName)
    {
        foreach (var pair in audioBufferDic)
            if (pair.Key.clip)
                if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                    if (!pair.Key.isPlaying)
                        return pair.Key;
        return null;
    }




    /// <summary>
    /// 등록되어 있는 오디오 클립 얻기 
    /// </summary>
    private AudioClip GetAudioClip(string name)
    {
        foreach (var pair in packageDic)
        {
            if (pair.Value != null)
            {
                AudioClip clip = System.Array.Find<AudioClip>(pair.Value, (x) => { return x.name.ToLower() == name.ToLower(); });
                if (clip != null) return clip;
            }
        }
        return null;
    }

    /// <summary>
    /// 클립존재유무와 사용할수 있는 오디오소스 얻기
    /// </summary>
    private AudioSource GetIdleAudioSource(string clipName)
    {
        AudioClip clip = GetAudioClip(clipName);
        if (clip == null)
            return null;

        AudioSource source = GetIdleAudioSource();
        if (source == null)
            return null;

        source.clip = clip;

        return source;
    }
#endregion // 내부함수들


    public void Initialize()
    {
        // 버퍼
        for (int i = 0; i < 20; ++i)
        {
            GameObject go = new GameObject("AudioSource");
            audioBufferDic.Add(go.AddComponent<AudioSource>(), 0);  
            go.transform.SetParent(this.transform);

        }
        // Lobby Sound Load
        LoadResourceAudioClipsPackage("Sounds");
        SetSoundOn(PlayerPrefHelper.GetSoundOn());
        
    }

    public void SetSoundOn(bool on)
    {
        _SoundOn = on;
    }

    /// <summary>
    /// 에셋번들로 로드된 사운드 패키지 일괄 로드 ( 언로드 자동 처리된다 )
    /// 번들이름으로 패키지가 관리되믄로 번들별 사운드관리체계 필요
    /// - 키값은 외부에서 기록 보관하며 패키시 일괄삭제시 키 값 필요
    /// - 사운드명이 중복으로 로드시 가능하지만 현재 구별해서 플레이 기능없다. ( 먼저 검색된 사운드가 관리된다 )
    /// </summary>
    /// <param name="bundleName">The bundleName.</param>
    /// <returns></returns>
    public bool LoadAssetBundleAudioClipsPackage(string bundleName)
    {
        AssetBundle bundle = BUNDLE.I.GetBundle(bundleName);
        if(bundle)
        {
            AudioClip[] clips = bundle.LoadAllAssets<AudioClip>();
            if (clips.Length > 0)
            {
                packageDic.Add(bundleName, clips);
                Debug.Log(">> Last LoadBundle AudioCip To Save packageDic = " + clips.Length + ", key: " + bundleName);
            }
            return true;
        }
        return false;
    }
    /// <summary>
    /// 리소스폴더에 있는 사운드 패키지 일괄로드 ( 폴더명이 키 값이 되며, 폴더명은 중복 되면 안된다 )
    /// - 키값은 외부에서 기록 보관하며 패키시 일괄삭제시 키 값 필요
    /// - 사운드명이 중복으로 로드시 가능하지만 현재 구별해서 플레이 기능없다. ( 먼저 검색된 사운드가 관리된다 )
    /// </summary>
    /// <param name="folder">The folder.</param>
    /// <returns></returns>
    public bool LoadResourceAudioClipsPackage(string folder)
    {
        if (!packageDic.ContainsKey(folder))
        {
            AudioClip[] clips = Resources.LoadAll<AudioClip>(folder);
            if (clips.Length > 0)
            {
                packageDic.Add(folder, clips);
                Debug.Log(">> Last LoadResources AudioCip To Save packageDic = " + clips.Length + ", key: " + folder);
            }
            return true;
        }
        return false;
    }

    public void LoadLocalDirectoryAudioClipsPackage(string localPath, string folder, System.Action<bool> complete)
    {
        string path = Application.dataPath + "/" + localPath + folder;
        List<AudioClip> tempList = new List<AudioClip>();
        List<System.IO.FileInfo> localFiles = xSystem.GetFiles(path);
        // 버젼에서 제외된 번들파일 제거
        localFiles.RemoveAll(file => file.Name.EndsWith(".meta"));

        for (int i = 0; i <= localFiles.Count - 1; i++)
        {
            StartCoroutine(coLocalAudioLoad(@"file:///" + localFiles[i].FullName, localFiles[i].Name.Substring(0, localFiles[i].Name.Length - 4), (clip)=> {
                tempList.Add(clip);
                if(tempList.Count >= localFiles.Count)
                {
                    if (!packageDic.ContainsKey(folder))
                    {
                        packageDic.Add(folder, tempList.ToArray());
                        Debug.Log(">> Last LoadDirectory AudioCip To Save packageDic = " + tempList.Count + ", key: " + folder);
                    }
                    tempList.Clear();
                    if (complete != null) complete(true);
                }
            }));
        }
    }

    IEnumerator coLocalAudioLoad(string url, string clipName, System.Action<AudioClip> complete)
    {
        WWW www = new WWW(url);
        AudioClip clip = null;

        yield return www;

        if (www.error != null && www.error.Length > 0)
        {
            Debug.Log(www.error + "(" + url + ")");
        }
        else
        {
#if UNITY_IPHONE
            clip = www.GetAudioClip(false, false);//, AudioType.MPEG);
#else
            clip = www.GetAudioClip(false, false);//, AudioType.OGGVORBIS);
#endif

            while (clip != null && clip.loadState == AudioDataLoadState.Failed)
                yield return null;
            
            //Debug.Log(" Length of " + url + " is " + clip.length);

            // 5.5.2p1 버젼 클립 이름이 빠지는 버그가 있다.. 임의로 넣어준다.
            if (string.IsNullOrEmpty(clip.name))
                clip.name = clipName;
            
            if (complete != null) complete(clip);
        }
    }


    /// <summary>
    /// 사운드 패키시 삭제
    /// </summary>
    /// <param name="folder">The folder.</param>
    public void RemoveClipPackage(string folder)
    {
        if (packageDic.ContainsKey(folder))
        {
            AudioClip[] clips;
            packageDic.TryGetValue(folder, out clips);
            foreach (var pair in audioBufferDic)
            {
                if (pair.Key.clip == null) continue;
                AudioClip findClip = System.Array.Find<AudioClip>(clips, (clip) => { return clip.name.ToLower() == pair.Key.clip.name.ToLower(); });
                if (findClip && pair.Key.isPlaying)
                {
                    pair.Key.Stop();
                }
            }
            packageDic.Remove(folder);
            Debug.Log("<< Remove AudioClip packageDic key: " + folder);
        }
    }

    /// <summary>
    /// 오디오의 전체 볼륨값을 정의 0 ~ 1.0f
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        if (masterVolume > 1f) masterVolume = 1.0f;
        else if (masterVolume < 0) masterVolume = 0;
        //플레이중인 오디오 볼륨 조절 바로 조정은 나중에..필요하면.
    }

    /// <summary>
    /// 마스터볼륨및 기타 설정에 의존하지 않는 직접 플레이설정을 조정
    /// </summary>
    public void PlayStatic(string clipName, bool loop, float volume, float delayTime)
    {
        if (!_SoundOn) return;
        AudioSource source = GetIdleAudioSource(clipName);
        if (source)
        {
            source.volume = volume;
            source.loop = loop;
            if (delayTime > 0) source.PlayScheduled(AudioSettings.dspTime + delayTime);
            else source.Play();
        }
    }
    /// <summary>
    /// 마스터볼륨및 기타설정에 의존하는 일반적인 플레이 방식 
    /// </summary>
    public void Play(string clipName, bool loop = false, float delayTime = 0, float volume = 1.0f)
    {
        if (!_SoundOn) return;
        AudioSource source = GetIdleAudioSource(clipName);
        if(source != null)
        {
            source.volume = volume;// masterVolume;
            source.loop = loop;
            if (delayTime > 0)  source.PlayScheduled(AudioSettings.dspTime + delayTime);
            else source.Play();
        }
    }
    public bool IsPlay(string clipName)
    {
        foreach (var pair in audioBufferDic)
        {
            if (pair.Key.clip)
            {
                if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                {
                    if (pair.Key.isPlaying) return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 플레이중인 오디오 볼륨 조절
    /// </summary>
    public void PlayVolume(string clipName, float volume)
    {
        if (!_SoundOn) return;
        foreach (var pair in audioBufferDic)
            if (pair.Key.clip)
                if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                    if (pair.Key.isPlaying)
                        pair.Key.volume = volume;
    }

    /// <summary>
    /// 오디오 일시중지 , AudioSourceAll - 모든 오디오 버퍼 대상 여부
    /// </summary>
    public void PlayPause(string clipName, bool AudioSourceAll=false)
    {
        if (!_SoundOn) return;
        if (AudioSourceAll)
        {
            foreach (var pair in audioBufferDic)
                if (pair.Key.clip)
                    if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                        if (pair.Key.isPlaying) pair.Key.Pause();
        }
        else
        {
            foreach (var pair in audioBufferDic)
                if (pair.Key.clip)
                    if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                        if (pair.Key.isPlaying && pair.Value == 0)
                        {
                            pair.Key.Pause();
                            audioBufferDic[pair.Key] = 1;
                            break;
                        }

            //AudioSource source = GetPlayingAudioSource(clipName);
            //if (source) { if (source.isPlaying) source.Pause(); }
        }
    }
    /// <summary>
    /// 일시중지 오디오 다시 재생 , AudioSourceAll - 모든 오디오 버퍼 대상 여부
    /// </summary>
    public void PlayUnPause(string clipName, bool AudioSourceAll = false)
    {
        if (!_SoundOn) return;
        if (AudioSourceAll)
        {
            foreach (var pair in audioBufferDic)
                if (pair.Key.clip)
                    if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                        if (!pair.Key.isPlaying) pair.Key.UnPause();
        }
        else
        {
            foreach (var pair in audioBufferDic)
                if (pair.Key.clip)
                    if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                        if (!pair.Key.isPlaying && pair.Value == 1)
                        {
                            pair.Key.UnPause();
                            audioBufferDic[pair.Key] = 0;
                            break;
                        }

            //AudioSource source = GetPauseAudioSource(clipName);
            //if (source) { if (!source.isPlaying) source.UnPause(); }
        }
    }
    /// <summary>
    /// 오디오 정지 , AudioSourceAll - 모든 오디오 버퍼 대상 여부
    /// </summary>
    public void PlayStop(string clipName, bool AudioSourceAll = false)
    {
        if (AudioSourceAll)
        {
            foreach (var pair in audioBufferDic)
                if (pair.Key.clip)
                    if (pair.Key.clip.name.ToLower() == clipName.ToLower())
                        if (pair.Key.isPlaying) pair.Key.Stop();
        }
        else
        {
            AudioSource source = GetPlayingAudioSource(clipName);
            if (source) { if (source.isPlaying) source.Stop(); }
        }
    }
    /// <summary>
    /// 오디오 버퍼에서 플레이중인 모든 오디오 중지
    /// </summary>
    public void PlayAllStop()
    {
        foreach (var i in audioBufferDic)
        {
            if (i.Key.isPlaying)
            {
                i.Key.Stop();
            }
        }
    }
    
}
