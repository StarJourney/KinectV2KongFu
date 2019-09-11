using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eState { eWait, eStart, eQuit }
public class CusKincetManager
{ 
    public eState mCurState = eState.eWait;
    public int mHalfWidth = 1920 / 2;
    public int mHalfHeigth = 1080 / 2;
    public Vector2Int mLeftPos = Vector2Int.zero;
    public Vector2Int mRightPos = new Vector2Int(1920/2,0);

    public Rect mVideoScreenRect;

    public void KinectManager()
    {
        mVideoScreenRect = new Rect(mRightPos,new Vector2(mHalfWidth,mHalfHeigth));

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
