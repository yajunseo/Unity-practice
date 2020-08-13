﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{
    public List<GameObject> boxNoteList = new List<GameObject>();

    int[] judgementRecord = new int[5];

    [SerializeField] Transform Center = null;
    [SerializeField] RectTransform[] timingRect = null;
    Vector2[] timingBoxs = null;

    EffectManager theEffect;
    ScoreManager theScoreManager;
    ComboManager theComboManager;
    StageManager theStageManager;
    PlayerController thePlayer;
    StatusManager theStautsManager;
    AudioManger theAudioManager;
 
    // Start is called before the first frame update
    void Start()
    {
        theEffect = FindObjectOfType<EffectManager>();
        theScoreManager = FindObjectOfType<ScoreManager>();
        theComboManager = FindObjectOfType<ComboManager>();
        theStageManager = FindObjectOfType<StageManager>();
        thePlayer = FindObjectOfType<PlayerController>();
        theStautsManager = FindObjectOfType<StatusManager>();
        theAudioManager = FindObjectOfType<AudioManger>();

        // 타이밍 박스 설정
        timingBoxs = new Vector2[timingRect.Length];
        for(int i=0;i<timingRect.Length;i++)
        {
            timingBoxs[i].Set(Center.localPosition.x - timingRect[i].rect.width / 2,
                             Center.localPosition.x + timingRect[i].rect.width / 2);
        }
    }

    public bool CheckTiming()
    {
        for(int i=0;i<boxNoteList.Count;i++)
        {
            float t_notePosX = boxNoteList[i].transform.localPosition.x;

            for(int x=0;x<timingBoxs.Length;x++)
            {
                if(timingBoxs[x].x<=t_notePosX && t_notePosX <= timingBoxs[x].y)
                {
                    // 노트 제거
                    boxNoteList[i].GetComponent<Note>().HideNote();
                    boxNoteList.RemoveAt(i);

                    // 이펙트 연출
                    if (x<timingBoxs.Length - 1)
                        theEffect.NoteHitEffect();

                    if (CheckCanNextPlate())
                    {
                        theScoreManager.IncreaseScore(x); // 점수 증가
                        theStageManager.ShowNewPlate();   // 판때기 등장
                        theEffect.JudgementEffect(x);  // 판정 연출
                        judgementRecord[x]++; // 판정 기록
                        theStautsManager.CheckShield(); // 쉴드 체크
                    }

                    else
                    {
                        theEffect.JudgementEffect(5);
                    }

                    theAudioManager.PlaySFX("Clap");

                    return true;
                }
            }
        }
        theComboManager.ResetCombo();
        theEffect.JudgementEffect(4);
        MissRecord();
        return false;
    }

    bool CheckCanNextPlate()
    {
        if(Physics.Raycast(thePlayer.desPos, Vector3.down, out RaycastHit t_hitInfo, 1.1f))
        {
            if(t_hitInfo.transform.CompareTag("BasicPlate"))
            {
                BasicPlate t_plate = t_hitInfo.transform.GetComponent<BasicPlate>();
                if (t_plate.flag)
                {
                    t_plate.flag = false;
                    return true;
                }
            }
        }
        return false;
    }

    public int[] GetJudgementRecord()
    {
        return judgementRecord;
    }

    public void MissRecord()
    {
        judgementRecord[4]++; // 판정 기록
        theStautsManager.ResetShieldCombp();
    }
}
