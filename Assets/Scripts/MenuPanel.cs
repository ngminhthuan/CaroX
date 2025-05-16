using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] Button _PvP_Btn;
    [SerializeField] Button _PvsAI_MinMax_Btn;
    [SerializeField] Button _PvsAI_Monte_Btn;
    [SerializeField] Button _AI_MinmaxVsAI_Monte;
    [SerializeField] Button _quitBtn;
    // Start is called before the first frame update
    void Start()
    {
        this.RegisterBtn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterBtn()
    {
        this._PvP_Btn.onClick.AddListener(() =>
        {
            BoardManager.Instance.CreateBoard(PlayerType.Human,PlayerType.Human);
        });
        this._PvsAI_MinMax_Btn.onClick.AddListener(() =>
        {
            BoardManager.Instance.CreateBoard(PlayerType.Human, PlayerType.AI_MinMax);
        });
        this._PvsAI_Monte_Btn.onClick.AddListener(() =>
        {
            BoardManager.Instance.CreateBoard(PlayerType.Human, PlayerType.AI_Monte);
        });
        this._AI_MinmaxVsAI_Monte.onClick.AddListener(() =>
        {
            BoardManager.Instance.CreateBoard(PlayerType.AI_MinMax, PlayerType.AI_Monte);
        });
        this._quitBtn.onClick.AddListener(() => { 
            Application.Quit();
        });
    }
}
