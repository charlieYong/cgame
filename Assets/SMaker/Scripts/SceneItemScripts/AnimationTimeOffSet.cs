using UnityEngine;
using System.Collections;

public class AnimationTimeOffSet : MonoBehaviour {

    public AnimationClip clip;
    public float speed = 1;
    public float timeOffSet;
    public bool isRandom;
    void Start()
    {
        Animation anim = GetComponent<Animation>();
        if (anim == null||anim.GetClipCount()==0) return;
        if (clip == null)
        {
            clip = anim.clip;
        }
        if (clip == null) return;
        AnimationState animState = anim[clip.name];
        anim.Play(clip.name);
        animState.wrapMode = WrapMode.Loop;
        animState.speed = speed;
        if (isRandom)
        {
            animState.time = Random.Range(0, animState.length);
        }
        else
        {
            animState.time = timeOffSet;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
