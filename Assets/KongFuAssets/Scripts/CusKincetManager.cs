using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eState {eWaitPlayer, eWaitStart, eStart  }

public delegate void HandleStateChange(eState pState);

public class CusKincetManager
{
    public HandleStateChange mHandleStateChange;
    public int mPlayerIndex = 0;
    //处理状态。
    eState mCurState = eState.eWaitPlayer;
    public eState CurState
    {
        get {
            return mCurState;
        }
        set
        {
            if (value != mCurState)
            {
                mCurState = value;
                if (mHandleStateChange != null)
                {
                    mHandleStateChange(mCurState);
                }
            }
        }
    }

    static CusKincetManager mKinectManager;
    public static CusKincetManager Instance
    {
        get
        {
            if (mKinectManager != null)
                return mKinectManager;
            else return mKinectManager = new CusKincetManager();
                     
        }
    }
    
    
}
