﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class Entry : MonoBehaviour ,KinectGestures.GestureListenerInterface
{
    KinectManager mKm;
    public VideoPlayer mVideoplayer;
    public float mHoverTime = 2.0f;
    public Transform mPlayer;
    //提示文字。
    public Text mTipsText;

    public GameObject circleParent;

    //进度条相关
    public float mImageProgress = 0f;
    public Image mTimerImage;
    public Image mCursor;

    void Start()
    {
        if (mVideoplayer == null)
        {
            Debug.LogError("检查VideoPlayer 组件!!");
        }
        if (mTipsText == null)
            Debug.LogError("检查Tips Text 组件!!");
        mKm = KinectManager.Instance;
        CusKincetManager.Instance.mHandleStateChange += HandleStateChange;

        //cxk add
        SetGameObjectActive(circleParent, false);

    }

    private void Update()
    {
        UpdatePos();
        CheckVideoPlayerState();
    }
   
    private void CheckUserInputInfo(eState newState)
    {
        if (CusKincetManager.Instance.CurState != newState)
        {
            CusKincetManager.Instance.CurState = newState;
        }
    }
    private void UpdatePos()
    {
        var userId = KinectManager.Instance.GetUserIdByIndex(CusKincetManager.Instance.mPlayerIndex);
        //Debug.Log("userID===================" + userId);
        var bd = KinectManager.Instance.GetUserBodyData(userId);
        Vector3 vPos = bd.position;
        Vector3 mPos = new Vector3(vPos.x, mPlayer.transform.position.y, vPos.z);
        mPlayer.transform.localPosition = mPos;
        mTimerImage.fillAmount = mImageProgress / mHoverTime;

    }
    private void CheckVideoPlayerState()
    {
        if (mVideoplayer == null)
            return;
        if (Mathf.Abs((float)mVideoplayer.time - (float)mVideoplayer.length) <= 0.5f)
        {
            mVideoplayer.time = 0;
            Debug.Log("练习完成！！");
            //如果现在玩家还被追踪，就直接设置为eWaitStart.否则等待玩家。
            var userId = KinectManager.Instance.GetUserIdByIndex(CusKincetManager.Instance.mPlayerIndex);
            if (userId!=0)
            {
                CusKincetManager.Instance.CurState = eState.eWaitStart;
            }
            else
            {
                CusKincetManager.Instance.CurState = eState.eWaitPlayer;
            }
        }
    }

    private void HandleStateChange(eState pState)
    {
        switch (pState)
        {
            case eState.eWaitPlayer:
                Debug.Log("waitting player in position...");
                //UI提示玩家进入场景。
                mTipsText.text = Tips.WAIT_PLAYER;

                VideoPlayStop();
                break;
            case eState.eWaitStart:
                Debug.Log("waitting player start pratice KongFu!!");
                //UI提示玩家准备开始，通过手势开始体验。
                mTipsText.text = Tips.WAIT_START;

                VideoPlayStop();
                break;

            case eState.eStart:
                //已经开始体验，可以退出，或者视频完成后自动推出。边长eQuit状态。
                //eQuit状态要经过处理之后回到eWaitPlayer-->eWaitStart开始下一次体验。
                mTipsText.text = Tips.WAIT_PRATICING;
                VideoPlayfromStart();

                Debug.Log("player is started partice KongFu...");
                break;
            default:
                break;
        }

    }

    private void OnDestroy()
    {
        if (CusKincetManager.Instance.mHandleStateChange!=null)
        {
            CusKincetManager.Instance.mHandleStateChange -= HandleStateChange;
        }
    }

    void KinectGestures.GestureListenerInterface.UserDetected(long userId, int userIndex)
    {
        if (userIndex == CusKincetManager.Instance.mPlayerIndex)
        {
            if (CusKincetManager.Instance.CurState != eState.eWaitPlayer)
                return;

            //检测到玩家之后，进入等待。
            CusKincetManager.Instance.CurState = eState.eWaitStart;
        }
    }

    void KinectGestures.GestureListenerInterface.UserLost(long userId, int userIndex)
    {
        if (userIndex != CusKincetManager.Instance.mPlayerIndex)
            return;

        CusKincetManager.Instance.CurState = eState.eWaitPlayer;
    }

    private bool bIsStartPlayVideo = false;
    void KinectGestures.GestureListenerInterface.GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (CusKincetManager.Instance.mPlayerIndex != userIndex)
            return;

        var rhState = KinectManager.Instance.GetRightHandState(userId);
        if (rhState == KinectInterop.HandState.Closed && !bIsStartPlayVideo)
        {
            if (mImageProgress < mHoverTime)
            {
                SetGameObjectActive(circleParent, true);
                mImageProgress += Time.deltaTime;
                bIsStartPlayVideo = false;
            }
            else
            {
                CheckUserInputInfo(eState.eStart);
                SetGameObjectActive(circleParent, false);
                mImageProgress = 0f;
                bIsStartPlayVideo = true;
                VideoPlayfromStart();
                return;
            }
        }
        else if(bIsStartPlayVideo || rhState != KinectInterop.HandState.Closed)
        {
            if (mImageProgress > 0)
            {
                SetGameObjectActive(circleParent, true);
                mImageProgress -= Time.deltaTime * 2.5f;
            }
            else
            {
                //CheckUserInputInfo(eState.eWaitStart);

                SetGameObjectActive(circleParent, false);
                mImageProgress = 0f;
                bIsStartPlayVideo = false;
            }
        }
        //else if(CusKincetManager.Instance.CurState == eState.eWaitStart && rhState !=KinectInterop.HandState.Closed)
        //{
        //    if (mImageProgress>0)
        //    {
        //        SetGameObjectActive(circleParent, true);
        //        mImageProgress -= Time.deltaTime*2.0f;
        //    }
        //    else
        //    {
        //        SetGameObjectActive(circleParent, false);
        //        mImageProgress = 0f;
        //    }
        //}
        

    }

    bool KinectGestures.GestureListenerInterface.GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint, Vector3 screenPos)
    {
        //只处理第一个玩家。
        if (CusKincetManager.Instance.mPlayerIndex != userIndex)
            return false;

        if (joint == KinectInterop.JointType.HandRight)
        {
            mCursor.rectTransform.position = screenPos;
        }
        
        if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            Debug.Log("swipeleft is complete..");
            //this.CheckUserInputInfo();
        }
        return true;
    }

    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, KinectInterop.JointType joint)
    {
        return false;
    }

    //cxk add functions
    public static void SetGameObjectActive(GameObject go,bool bState)
    {
        if (go == null)
            return;
        if (go.activeSelf!=bState)
        {
            go.SetActive(bState);
        }
    }

    public void VideoPlayfromStart()
    {
        if (mVideoplayer == null)
            return;
        if (mVideoplayer.isPlaying)
        {
            mVideoplayer.Stop();
            mVideoplayer.Play();
        }
        else
        {
            mVideoplayer.Play();
        }
    }

    public void VideoPlayStop()
    {
        if (mVideoplayer == null)
            return;
        if (mVideoplayer.isPlaying)
        {
            mVideoplayer.Stop();
        }
    }
}
