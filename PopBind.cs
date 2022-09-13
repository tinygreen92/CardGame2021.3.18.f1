using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;
using Lean.Gui;
using UnityEngine.Events;
using SuperBlur;

namespace GoogleGame
{
    public class PopBind : LeanWindow
    {
        /*
         * public
         */
        public Text _Title;
        public Text _Intext;
        [Header("- 버튼 1 확인")] 
        public Button _OkayBtn;
        public Button.ButtonClickedEvent _OkayEvent;
        [Space]
        [Header("- 버튼 2 확인")] 
        public Button _Summit_01;
        public Button.ButtonClickedEvent _SummitEvent;
        [Header("- 버튼 2 취소")] 
        public Button _Deny_02;
        public Button.ButtonClickedEvent _DenyEvent;

        /*
         * private 블러 맥일꺼 선언
         */
        private SuperBlurFast superBlur;
        private WaitForSeconds delaySeconds = new WaitForSeconds(1f);

        protected override void OnEnable()
        {
            if (superBlur == null)
            {
                superBlur = Camera.main.GetComponent<SuperBlurFast>();
            }
        }
        
        /// <summary>
        /// 블러 켜주면서 팝업창 오픈
        /// </summary>
        public void ShowThisPop()
        {
            if (superBlur == null) 
                superBlur = Camera.main.GetComponent<SuperBlurFast>();
            superBlur.enabled = true;
            Debug.LogWarning("팝플레이 온");
            GameManager.isPopPlay = true;
            TurnOn();
        }

        /// <summary>
        /// 블러 꺼주면서 팝업창 종료
        /// </summary>
        public void HideThisPop()
        {
            _OkayEvent.RemoveAllListeners();
            _SummitEvent.RemoveAllListeners();
            _DenyEvent.RemoveAllListeners();

            if (superBlur == null) superBlur = Camera.main.GetComponent<SuperBlurFast>();
            superBlur.enabled = false;
            Debug.LogWarning("팝플레이 오프");
            GameManager.isPopPlay = false;
            TurnOff();
        }


        #region <완충 버튼이 있는 1>

        /// <summary>
        /// 진짜로 종료하기 앞서 3초정도 여유 시간을 주는 팝업 생성
        /// </summary>
        /// <param name="okBtn">종료 / 재실행 / 구애 같은 중요한 액션</param>
        /// <param name="title"></param>
        /// <param name="intext"></param>
        internal void Pop_RealExit_Init(UnityAction okBtn, string title, string intext)
        {
            if (GameManager.isPopPlay) return;

            _Title.text = title;
            _Intext.text = intext;

            _OkayEvent = _OkayBtn.onClick;
            if (okBtn != null) _OkayEvent.AddListener(okBtn);

            if (superBlur == null) superBlur = Camera.main.GetComponent<SuperBlurFast>();
            superBlur.enabled = true;
            _OkayBtn.enabled = false;

            Debug.LogWarning("팝플레이 온");
            GameManager.isPopPlay = true;

            TurnOn();

            // 느린 버튼 텍스트 준비
            StartCoroutine(SlowButton(_OkayBtn.GetComponentInChildren<Text>()));
        }


        IEnumerator SlowButton(Text okayBtnText)
        {
            int cnt = 0;
            int maxCnt = 3;
            okayBtnText.text = $"종료...({maxCnt})";
            
            while (true)
            {
                yield return delaySeconds;
                if (cnt >= maxCnt)
                {
                    okayBtnText.text = "종료";    // TODO : 로컬라이징 필요
                    _OkayBtn.enabled = true;
                    yield break;
                }
                else
                {
                    okayBtnText.text = $"종료...({maxCnt - cnt})";
                    cnt++;
                }
            }
            
        }

        #endregion

        #region <버튼 1>

        /// <summary>
        /// 확인 1버튼 붙은거 생성
        /// </summary>
        /// <param name="okBtn">액션 메서드 붙여준다 (null = 팝업 닫음)</param>
        /// <param name="title">제목</param>
        /// <param name="intext">내용</param>
        internal void Pop_1Button_Init(UnityAction okBtn, string title, string intext)
        {
            if (GameManager.isPopPlay) return;

            _Title.text = title;
            _Intext.text = intext;
            _OkayBtn.GetComponentInChildren<Text>().text = "확인";

            _OkayEvent = _OkayBtn.onClick;
            _OkayEvent.AddListener(HideThisPop);
            if (okBtn != null) _OkayEvent.AddListener(okBtn);
        }

        #endregion


        #region <버튼 2>

        /// <summary>
        /// 확인 / 취소 2버튼 붙은거 생성
        /// </summary>
        internal void Pop_2Button_Init(UnityAction summit, UnityAction deny, string title, string intext)
        {
            if (GameManager.isPopPlay) return;

            _Title.text = title;
            _Intext.text = intext;

            _SummitEvent = _Summit_01.onClick;
            _SummitEvent.AddListener(HideThisPop);
            if (summit != null) _SummitEvent.AddListener(summit);


            _DenyEvent = _Deny_02.onClick;
            _DenyEvent.AddListener(HideThisPop);
            if (deny != null) _DenyEvent.AddListener(deny);

        }

        #endregion
    }
}