using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;
using EasyMobile;
using GoogleGame;

namespace GoogleGame
{
    public class PopManager : MonoBehaviour
    {
        [Header("캐릭터 생성 버튼이 있는 UI")]
        [SerializeField] GameObject TutoImage;
        [Header("-씬에서 쓸 버튼 등록하긔")]
        [SerializeField] PopBind[] poppops;


        #region <로그인 씬 팝업에만 적용> 

        [SerializeField] InputField inputNick;
        [SerializeField] Text samesameNick;

        /// <summary>
        /// [btn] 개인정보 처리 방침 팝업
        ///         1.동의 누르면 중고 유저 박제
        ///         2. 튜토리얼로 넘어감
        /// 
        /// </summary>
        public void BecomingOld()
        {
            // 개인정보 처리 방침 팝업
            poppops[0].ShowThisPop();
        }
        
        /// <summary>
        /// 운영약관
        /// </summary>
        /// <param name="isTOP"></param>
        public void OpenPrivacyURL(bool isTOP)
        {
            if (isTOP) Application.OpenURL("https://docs.google.com/document/d/1ves_FBYhO1UoFhpPTkZpzqYA6JxI7sPAQDrCnsyzKpU/edit?usp=sharing");
            else Application.OpenURL("https://docs.google.com/document/d/1UN7PaKHUl12bNxyPvG-JQ6Saevz4HxYJVSPXO60UsBs/edit?usp=sharing");
        }
        /// <summary>
        /// [BTN]약관 거절하면 앱 종료
        /// </summary>
        public void APPdown()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //play모드를 false로.
#else
            /// 나누 토큰 해지
            NanooManager.Instance.AccountTokenSignOut();
            GameServices.SignOut();
            Application.Quit();
#endif
        }

        /// <summary>
        /// TODO : 캐릭터 생성 버튼이 있는 캔버스 팝업
        /// </summary>
        internal void TEST_TutoskipName()
        {
            Debug.LogWarning("캐릭터 생성 버튼이 있는 캔버스 팝업");
            TutoImage.SetActive(true);
        }

        /// <summary>
        /// 안드로이드 테스트용 -> GAMEMASTER로 에러메시지 검출용
        /// </summary>
        internal void GayMaster()
        {
            /// 유저 상태에 따라 팝업 선택해서 뿌려줌
            StartCoroutine(UserAccoutDetect());

            /// 유니티에서 클릭하면
            NanooManager.Instance.NanooInit("g03324484546715956101");
        }

        /// <summary>
        /// 약관 동의하는 버튼 누르면 구글 로그인 물고 진행하기
        /// </summary>
        public void AppUp()
        {
            /// 유저 상태에 따라 팝업 선택해서 뿌려줌
            StartCoroutine(UserAccoutDetect());
            
#if UNITY_EDITOR
            /// 유니티에서 클릭하면
            NanooManager.Instance.NanooInit("g03324484546715956101");
#else
            /// 안드로이드에서 클릭하면
            NanooManager.Instance.NanooInit(GameServices.LocalUser.id);
#endif

        }

        /// <summary>
        /// 약관 동의 누르면 유저 상태 들고와서 팝업 맥여줌
        /// </summary>
        /// <returns></returns>
        IEnumerator UserAccoutDetect()
        {
            NanooManager.Instance.UserType = AccountType.NONE;

            /// 상태 전이시(NONE이 아닐 때) 반복문 탈출
            while (NanooManager.Instance.UserType == AccountType.NONE)
            {
                yield return null;
                yield return null;
                yield return null;
            }
            
            switch (NanooManager.Instance.UserType)
            {
                case AccountType.NEWBIE:
                    Debug.LogError("신규 유저 계정입니다.");
                    /// 로컬 데이터가 없으면 currentPlayerID == null
                    /// -> 무조건 신규 유저처리
                    poppops[1].Pop_1Button_Init(null, "알림", "신규 유저 로그인합니다.");
                    poppops[1].ShowThisPop();

                    TEST_TutoskipName();
                    break;

                case AccountType.OLDDER:
                    /// 기존 유저는 특별한 팝업 필요없다.
                    Debug.LogError("기존 유저 계정입니다.");
                    break;
                
                case AccountType.DUPLICATION:
                    /// 토큰이 만료되기 전에 다른 기기에서 로그인했다.
                    /// // Update()에서 팝업 올려줌
                    // poppops[1].Pop_1Button_Init(APPdown, "알림", $"해당 구글 계정은 다른 기기에 로그인된 계정입니다. \n 기존 기기에 로그아웃하던가 \n 문제가 지속 될 시운영자에게 기기 풀어주세요 요청");
                    // poppops[1].ShowThisPop();
                    break;

                case AccountType.DATA_CONFLICT:
                    Debug.LogError("기기에 다른 플레이어의 정보가 발견되었습니다.");
                    /// currentPlayerID != _playerID
                    /// 기기에 다른 계정의 데이터가 남아있다 경고 팝업 표시
                    /// 닉네임이 중복되면? 문의를 받는다.
                    poppops[2].Pop_2Button_Init(AllDataReset, GPGSSignOut, "경고", "기기에 다른 계정의 데이터가 남아있다\n기기 데이터 초기화하고 새로 시작할래?\n아니면 현재 구글 로그아웃하고 기존 계정 로그인 할래?");
                    poppops[2].ShowThisPop();
                    break;
                
                case AccountType.BANUSSER:
                    poppops[1].Pop_1Button_Init(RestartAppForAOS, "알림", "차단된 계정입니다. 앱 재실행");
                    poppops[1].ShowThisPop();
                    break;

                case AccountType.UNKNOWN_ERROR:
                    poppops[1].Pop_1Button_Init(RestartAppForAOS, "에러", "원인불명으로 로그인 실패 - 앱 재실행\n 해결 안되면 문의 주세용");
                    poppops[1].ShowThisPop();
                    break;

                default:
                    break;
            }

        }
        

        /// <summary>
        /// 기기에 존재하는 다른 계정 데이터 삭제 후 기기 재부팅
        /// </summary>
        void AllDataReset()
        {
            Debug.LogWarning("모든 로컬 데이터 삭제");

            /// 나누 토큰 해지
            NanooManager.Instance.AccountTokenSignOut();
            PlayerPrefs.DeleteAll();
            GameServices.SignOut();
            /// 싹 지우고 재부팅
            RestartAppForAOS();
        }

        /// <summary>
        /// 구글 로그아웃하고 계약 해지 -> 잘못 로그인 했을 때, 기존 데이터를 살리고 싶을 때.
        /// </summary>
        void GPGSSignOut()
        {
            /// 나누 토큰 해지
            NanooManager.Instance.AccountTokenSignOut();
            Debug.LogWarning("구글 게임 서비스 로그아웃");
            GameServices.SignOut();
            /// IsOldUser 삭제되면 튜토리얼도 다시 시작
            PlayerPrefs.DeleteKey("IsOldUser");
            MySceneManager.Instance.JumpScemaLocation(MyLocation.LoginScene);
        }

        /// <summary>
        /// [inter] 로그인 실패 팝업
        /// </summary>
        internal void ShowPopLoginFail()
        {
            poppops[1].Pop_1Button_Init(RestartAppForAOS, "알림", "로그인에 실패하였습니다.\n앱을 재실행합니다.");
            poppops[1].ShowThisPop();
        }


        /// <summary>
        /// 배드 워드 대조 시킴
        /// </summary>
        internal string GetInputFiled()
        {
            /// 닉네임 체크 초기화
            NanooManager.Instance.isNickNameExist = NameType.NONE;
            return inputNick.text;
        }
        /// <summary>
        /// 인풋필드 초기화
        /// </summary>
        /// <returns></returns>
        internal void ResetInputFiled()
        {
            inputNick.text.Remove(0, inputNick.text.Length);
            inputNick.text = "";
        }
        internal void ShowSameSameText(string output)
        {
            samesameNick.text = output;
        }

        /// <summary>
        /// [BTN]생성 버튼에 붙여
        /// </summary>
        public void Create_NickName()
        {
            switch (NanooManager.Instance.isNickNameExist)
            {
                case NameType.NONE:
                    ShowSameSameText("중복 확인 버튼을 눌러주세요.");
                    break;
                case NameType.NOT_EXIST:
                    NanooManager.Instance.AccountNicknamePut(inputNick.text);
                    //ResetInputFiled();
                    StartCoroutine(nameof(InvoSceneChage));
                    break;
                case NameType.ALREADY_EXIST:
                    break;
                case NameType.PASS:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 캐릭터 닉네임 생성되고 씬 넘겨
        /// </summary>
        IEnumerator InvoSceneChage()
        {
            while (NanooManager.Instance.isNickNameExist != NameType.PASS)
            {
                yield return null;
                yield return null;
                yield return null;
            }
            
            // 우편함 한번 호출
            NanooManager.Instance.Postbox();
            Debug.LogWarning("팝플레이 오프");
            GameManager.isPopPlay = false;
            
            MySceneManager.Instance.JumpScemaLocation(MyLocation.MainScene);
        }

#endregion


        /// <summary>
        /// 안드로이드 앱 재실행 코드
        /// </summary>
        void RestartAppForAOS()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; //play모드를 false로.
#else
        AndroidJavaObject AOSUnityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject baseContext = AOSUnityActivity.Call<AndroidJavaObject>("getBaseContext");
        AndroidJavaObject intentObj = baseContext.Call<AndroidJavaObject>("getPackageManager").Call<AndroidJavaObject>("getLaunchIntentForPackage", baseContext.Call<string>("getPackageName"));
        AndroidJavaObject componentName = intentObj.Call<AndroidJavaObject>("getComponent");
        AndroidJavaObject mainIntent = intentObj.CallStatic<AndroidJavaObject>("makeMainActivity", componentName);
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        mainIntent = mainIntent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK"));
        mainIntent = mainIntent.Call<AndroidJavaObject>("addFlags", intentClass.GetStatic<int>("FLAG_ACTIVITY_CLEAR_TASK"));
        baseContext.Call("startActivity", mainIntent);
        AndroidJavaClass JavaSystemClass = new AndroidJavaClass("java.lang.System");
        JavaSystemClass.CallStatic("exit", 0);
#endif

        }
    }
}