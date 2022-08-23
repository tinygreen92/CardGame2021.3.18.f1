using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;
using Google;
using PlayNANOO;
using PlayNANOO.CloudCode;
using UnityEngine;
using GoogleGame;
using EasyMobile;

public enum NameType
{
    NONE,
    NOT_EXIST,
    ALREADY_EXIST,
    PASS
}

public enum AccountType
{
    NONE,
    NEWBIE,
    OLDDER,
    DUPLICATION,
    BANUSSER,
    DATA_CONFLICT,
    UNKNOWN_ERROR
}

namespace GoogleGame
{
    public class NanooManager : MonoBehaviour
    {
        /// <summary>
        /// 파괴되지 않는 나누 하위의 팝업
        /// </summary>
        [HideInInspector] public PopBind popBind;

        Plugin plugin;

        private static NanooManager instance;

        public static NanooManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// 나누 로그인되면 true
        /// </summary>
        bool IsNannoLogin
        {
            get
            {
                if (PlayerPrefs.GetInt("isNannoLogin", 0) == 1)
                {
                    return true;
                }

                return false;
            }
            set
            {
                PlayerPrefs.SetInt("isNannoLogin", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        //GoogleSignInConfiguration googleSignInConfiguration;


        /// <summary>
        /// 잡았다 치터! 로그 작성
        /// </summary>
        private void SavesAlterationDetected()
        {
            NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Obscured Cheating Detected! 랭킹 격리합니다.");
        }

        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
                return;
            }

            // 나누 세팅
            plugin = Plugin.GetInstance();
            plugin.SetLanguage(Configure.PN_LANG_KO);
            GameObject.Find("PlayNANOO").transform.SetParent(transform);
            // 보호된 pp 수정시 발동
            ObscuredPrefs.OnAlterationDetected += SavesAlterationDetected;

            popBind = GetComponentInChildren<PopBind>();

            /// 덱 세이버 한번만 초기화
            MyDeckSaver.MyStart();

            //googleSignInConfiguration = new GoogleSignInConfiguration
            //{
            //    RequestIdToken = true,
            //    WebClientId = "497323057317-9g25c4o3tvqi8q2en2b1asvnafs2tdh5.apps.googleusercontent.com",
            //};
        }

        /// <summary>
        /// 완충 작업 뒤에 앱 재실행
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
        

        /// <summary>
        /// 01.LoginScene - NanooManager에 붙어있는 각종 디텍터 Detector 로그 찍기
        /// </summary>
        public void ȕȖȕȕȕȕȖȕȕȖȖȕȖȖȕȖȖȖȖȖȖȕȕȖȖȖȕȖȖȕȖȕȕȖȖȕȕȕȖȕȖȕȖȕȕ(int index)
        {
            switch (index)
            {
                case 0:
                    NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Injection Detector 감지!");
                    break;
                case 1:
                    NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Obscured Cheating Detector 감지!");
                    break;
                case 2:
                    NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Speed Hack Detector 감지!");
                    break;
                case 3:
                    NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Time Cheating Detector 감지!");
                    break;
                default:
                    NanooNanooPumpThisGame("abc_Gotcha_Cheater", "Unknown Detected : 의심");
                    break;
            }
        }

        /// <summary>
        /// abc_Gotcha_Cheater : ACKt 감지했을때 나누로 로그 넘겨줌
        /// </summary>
        /// <param name="eventCode"></param>
        public void NanooNanooPumpThisGame(string eventCode, string bodyMsg)
        {
            var messages = new PlayNANOO.Monitor.LogMessages();
            messages.Add(Configure.PN_LOG_INFO, bodyMsg);

            plugin.LogWrite(new PlayNANOO.Monitor.LogWrite() {EventCode = eventCode, EventMessages = messages});
        }

        /// <summary>
        /// 토큰 액션 키 출력
        /// </summary>
        public void AccountTokenStatus()
        {
            plugin.AccountTokenStatus((status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogError("Account Token Status");
                    Debug.LogError(values["status"]);
                }
                else
                {
                    Debug.LogError("AccountTokenStatus Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }

        /// <summary>
        /// 토큰 해지
        /// </summary>
        public void AccountTokenSignOut()
        {
            plugin.AccountTokenSignOut((status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogError("Account Token SignOut");
                    Debug.LogError( "토큰 해지" + values["status"]);
                }
                else
                {
                    Debug.LogError("AccountTokenSignOut Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }


        /// <summary>
        /// 나누 토큰 계정 탈퇴
        /// </summary>
        public void AccountWithDrawal()
        {
            // TODO : 임시로 토큰 초기화 버튼 넣어둠
            popBind.Pop_1Button_Init(RealAccountWithDrawal, "이짓은 되돌릴 수 없음!", "정말로 진짜로 리얼로 계정 삭제 합니까?");
            popBind.ShowThisPop();
        }
        
        /// <summary>
        /// 경고 팝업 후에 나오는 토큰 계정 탈퇴 기능
        /// </summary>
        void RealAccountWithDrawal()
        {
            plugin.AccountWithDrawal((status, errorCode, jsonString, values) => {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogError($"토큰 탈퇴 결과 : {values["Status"]}");
                    AllDataReset();
                }
                else
                {
                    Debug.LogError($"토큰 탈퇴 Fail 왜?? : {values["Status"]}");
                }
            });   
        }
        

        /// <summary>
        /// TODO : PlayerPrefs 에 구글 guid 저장한다.
        /// </summary>
        internal ObscuredString PlayerID
        {
            get
            {
                return ObscuredPrefs.GetString("PlayerID", "");
            }
            set
            {
                ObscuredPrefs.SetString("PlayerID", value);
                ObscuredPrefs.Save();
            }
        }

        ObscuredString NickName { get; set; }
        //string PlayerName { get; set; }


        //internal void CreateDataAccunt(string _playerID)
        //{
        //    plugin.AccountLink(_playerID, (status, errorCode, jsonString, values) =>
        //    {
        //        Debug.LogWarning(values["nickname"].ToString());

        //        isNannoLogin = true;

        //        MySceneManager.Instance.ChangeScene("02.MainScene");
        //    });
        //}

        /// <summary>
        /// 코루틴에서 돌릴 계정 정보
        /// </summary>
        internal AccountType UserType = AccountType.NONE;

        /// <summary>
        /// 플레이어 ID
        /// </summary>
        /// <param name="_playerID"></param>
        internal void NanooInit(string _playerID)
        {
            string currentPlayerID = PlayerID;

            /// 플레이하던 데이터가 로컬에 없다면?
            if (currentPlayerID == "")
            {
                /// 지금 로그인한 계정을 로컬에 등록해준다.
                PlayerID = _playerID;
                Debug.LogError("currentPlayerID == null");
                Debug.LogError($"이 기기에 로그인정보가 없다. -> {PlayerID} 로컬 계정 등록");
                ///unKnown 이면 밑에 플로우로
                ///UserType = AccountType.NEWBIE;
            }
            else if (currentPlayerID != _playerID)
            {
                /// TODO : 기기에 다른 계정의 데이터가 남아있다면 나누로그인 막는다.
                Debug.LogError("currentPlayerID != _playerID");
                Debug.LogError($"기기에 다른 {currentPlayerID}계정의 데이터가 남아있다. \n 현재 로그인 시도 계정은 {_playerID}이다.");

                UserType = AccountType.DATA_CONFLICT;
                return;
            }

            Debug.LogError("currentPlayerID == _playerID");
            Debug.LogError($"구글 플레이 아이디 : {_playerID}");
            //PlayerName = _playerName;

            /// PlayerID == 1000 이면 초대 받지 않은 자이다.
            if (_playerID == "1000")
            {
                Debug.LogError($"초대 받지 않은 사용자다.");

                UserType = AccountType.BANUSSER;
                return;
            }

            // 나누에 계정 생성하는 거 (구글 계정 연동 대체 signin)
            plugin.AccountLink(_playerID, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    /// 나누 로그인 성공하면 배드포니 발동
                    BadPony();

                    NickName = values["nickname"].ToString();

                    /// 메인 화면으로 넘기기
                    if (NickName != "unknown")
                    {
                        /// 나누 로그인 성공!
                        Debug.LogError($"나누 로그인 성공!! {NickName}님! guid : {_playerID}");
                        Debug.LogError("접속 지역? : " + values["country"].ToString());

                        /// 우편함 호출
                        Postbox();
                        IsNannoLogin = true;

                        /// unknown 이면 기존 유저 로그인 플로우
                        UserType = AccountType.OLDDER;
                        MySceneManager.Instance.JumpScemaLocation(MyLocation.MainScene);
                    }
                    else
                    {
                        /// 닉네임이 unknown라면 준비된 닉네임 생성 UI가 드러난다.
                        Debug.LogError("NEWBIE일때 튜토리얼 -> 닉네임 생성창 출력");
                        UserType = AccountType.NEWBIE;
                    }
                }
                else
                {
                    Debug.LogError($"AccountLink Fail 링크 실패 : {errorCode}");
                    switch (errorCode)
                    {
                        case "30006":
                            //플레이어 정보가 다른 디바이스에서 사용 중 입니다.
                            Debug.LogError("플레이어 정보가 다른 디바이스에서 사용 중 입니다.");
                            /// TODO : 토큰 중복 해결법? 기존 기기에서 로그아웃하기 or 운영자에게 요청하기 (나누 콘솔에서 해제 가능)
                            UserType = AccountType.DUPLICATION;
                            
                            IsDuplicate = true;
                            Debug.LogError("중복 로그인 감지 Duplicate connection has been detected.");
                            popBind.Pop_RealExit_Init(RestartAppForAOS, "중복 로그인 시도 감지", "다른 곳에서 로그인 하지마삼");
                            
                            break;
                        case "70002":
                            // 서비스 접근이 차단 되었습니다.
                            Debug.LogError("서비스 접근이 차단 되었습니다. 밴유저");
                            UserType = AccountType.BANUSSER;
                            /// TODO : 차단됐을 때 메시지 출력
                            BlockReason();
                            break;
                        default:
                            // 다른 원인으로 인한 로그인 실패
                            Debug.LogError("서비스 제공자에게 문의바랍니다.");
                            UserType = AccountType.UNKNOWN_ERROR;
                            break;
                    }
                }
            });
        }


        /// <summary>
        /// 나쁜 어플 찾아내기
        /// </summary>
        void BadPony()
        {
#if !UNITY_EDITOR
            if (
                IsNotBatpony(NanooSaveKey.BADPONY_00) ||
                IsNotBatpony(NanooSaveKey.BADPONY_01) ||
                IsNotBatpony(NanooSaveKey.BADPONY_02) ||
                IsNotBatpony(NanooSaveKey.BADPONY_03) ||
                IsNotBatpony(NanooSaveKey.BADPONY_04) ||
                IsNotBatpony(NanooSaveKey.BADPONY_05) ||
                IsNotBatpony(NanooSaveKey.BADPONY_06) ||
                IsNotBatpony(NanooSaveKey.BADPONY_07) ||
                IsNotBatpony(NanooSaveKey.BADPONY_08) ||
                IsNotBatpony(NanooSaveKey.BADPONY_09) ||
                IsNotBatpony(NanooSaveKey.BADPONY_10) ||
                IsNotBatpony(NanooSaveKey.BADPONY_11) ||
                IsNotBatpony(NanooSaveKey.BADPONY_12) ||
                IsNotBatpony(NanooSaveKey.BADPONY_13)
            )
            {
                /// TODO : 핵 감지시 서버에 긴급 기록
                // BADPONY_00 = "com.xigmagames.thebonfireFL" <- 테스트용 
                NanooNanooPumpThisGame("abc_Gotcha_CodeMonkey", "게임 치트 애플리케이션 감지");
            }

#endif
        }


        /// <summary>
        /// NanooSaveKey.BADPONY_~ 삽입해서 걸러내기
        /// </summary>
        /// <param name="bundleID"></param>
        /// <returns></returns>
        bool IsNotBatpony(string bundleID)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");

            AndroidJavaObject launchIntent = null;

            //if the app is installed, no errors. Else, doesn't get past next line
            try
            {
                launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleID);
            }
            catch (Exception ex)
            {
                Debug.Log("exception" + ex.Message);
            }

            return (launchIntent != null);
        }


        /// <summary>
        /// 차단 정보 조회
        /// </summary>
        void BlockReason()
        {
            plugin.BlockReason((status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    foreach (Dictionary<string, object> value in (ArrayList)values["Items"])
                    {
                        Debug.LogWarning("차단 사유 : " + value["Reason"]);
                        Debug.LogWarning("영구차단 Y/N : " + value["Permanent"]);
                        Debug.LogWarning("차단 만료 날짜 : " + value["ExpireDate"]);
                        Debug.LogWarning("차단 만료까지 남은 초 : " + value["TimeUntilExpire"]);
                        foreach (string service in (string[])value["Services"])
                        {
                            Debug.LogWarning("차단 서비스 코드 : " + service);
                        }
                    }
                }
                else
                {
                    Debug.LogError("BlockReason Fail");
                    //NanooCaseFlow(errorCode);
                }
            });
        }

        #region <구글 로그인 시도> - 권한 인증 문제 발생

        ///// <summary>
        ///// 구글 로그인 시도
        ///// </summary>
        //void G_NanooSignIn()
        //{
        //    GoogleSignIn.Configuration = googleSignInConfiguration;
        //    GoogleSignIn.Configuration.UseGameSignIn = false;
        //    GoogleSignIn.Configuration.RequestIdToken = true;
        //    GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        //}

        //void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        //{
        //    if (task.IsFaulted)
        //    {
        //        using (IEnumerator<System.Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
        //        {
        //            if (enumerator.MoveNext())
        //            {
        //                GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
        //                Debug.LogError("Got Error: " + error.Status + " " + error.Message);
        //            }
        //            else
        //            {
        //                Debug.LogError("Got Unexpected Exception?!?" + task.Exception);
        //            }
        //        }
        //    }
        //    else if (task.IsCanceled)
        //    {
        //        Debug.LogError("OnAuthenticationFinished Canceled");
        //    }
        //    else
        //    {
        //        plugin.AccountSocialSignIn(task.Result.IdToken, Configure.PN_ACCOUNT_GOOGLE, (status, errorCode, jsonString, values) =>
        //        {
        //            if (status == Configure.PN_API_STATE_SUCCESS)
        //            {
        //                NickName = values["nickname"].ToString();

        //                /// 구글 로그인 성공!
        //                //Debug.LogWarning(values["access_token"].ToString());
        //                //Debug.LogWarning(values["refresh_token"].ToString());
        //                //Debug.LogWarning(values["uuid"].ToString());
        //                //Debug.LogWarning(values["openID"].ToString());
        //                Debug.LogWarning($"구글 로그인 성공!! {NickName}님!");
        //                //Debug.LogWarning(values["linkedID"].ToString());
        //                //Debug.LogWarning(values["linkedType"].ToString());
        //                //Debug.LogWarning(values["country"].ToString());
        //                isNannoLogin = true;
        //                /// 우편함 호출
        //                plugin.Postbox.EventAction(OnPostboxActionCallback);
        //                plugin.Postbox.SubscribeAction(OnPostboxSubscribeActionCallback);
        //                /// 메인 화면으로 넘기기
        //                if (NickName != "unknown")
        //                {
        //                    MySceneManager.Instance.ChangeScene("02.MainScene");
        //                }
        //            }
        //            else
        //            {
        //                Debug.LogError("AccountSocialSignIn Fail");
        //            }
        //        });
        //    }
        //}

        #endregion

        /// <summary>
        /// 닉네임 중복 체크 0 = 초기상태, 1은 생성 가능, 2는 중복있으므로 생성불가
        /// </summary>
        internal NameType isNickNameExist = 0;

        /// <summary>
        /// 닉네임 중복 체크
        /// </summary>
        public void AccountNicknameExists(string nickname)
        {
            plugin.AccountNicknameExists(nickname, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogWarning(values["status"].ToString());
                    if (values["status"].ToString() == "NOT_EXISTS")
                        isNickNameExist = NameType.NOT_EXIST;
                    else
                        isNickNameExist = NameType.ALREADY_EXIST;
                }
                else
                {
                    Debug.LogError("AccountNicknameExists Fail 닉네임 중복체크 몬가 이상하다");
                    NanooCaseFlow(errorCode);
                }
            });
        }

        /// <summary>
        /// 계정 생성 및 수정
        /// </summary>
        public void AccountNicknamePut(string nickname)
        {
            plugin.AccountNickanmePut(nickname, true, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    NickName = values["nickname"].ToString();

                    Debug.LogWarning($"닉네임 생성 완료 <{NickName}>");
                    /// IsOldUser 일단 생성만 시켜. -> "계약 동의"
                    PlayerPrefs.SetInt("IsOldUser", 0);
                    PlayerPrefs.Save();
                    /// 닉네임 생성시 기본 카드 5장 생성
                    CurrencyInitOC();
                }
                else
                {
                    Debug.LogError("AccountNicknamePut Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }


        /*
         * 
         *  테스트용 메서드
         * 
         */

        /// <summary>
        /// 내가 소유한 카드의 총합 (5보다 커야함.)
        /// </summary>
        const string CRRENCY_STRING = "OC";

        /// <summary>
        /// 내가 획득한 카드 1증가하면 얘도 증가.
        /// </summary>
        public void CurrencyCharge()
        {
            plugin.CurrencyCharge(CRRENCY_STRING, 1, (status, errorMessage, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    /// 1 증가된 amout
                    MyDeckSaver.UpdateOCcount(values["amount"]);
                }
                else
                {
                    Debug.LogError("CurrencyCharge Fail");
                    NanooCaseFlow(errorMessage);
                }
            });
        }

        /// <summary>
        /// 서버에 저장된 카드 갯수를 로컬 갯수로 덮어쓰기 해준다.
        /// </summary>
        /// <param name="currencyValue"></param>
        public void CurrencyOverWrite(int currencyValue)
        {
            plugin.CurrencySet(CRRENCY_STRING, currencyValue, (status, errorMessage, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    MyDeckSaver.UpdateOCcount(values["amount"]);
                }
                else
                {
                    Debug.LogError("CurrencyCharge Fail");
                    NanooCaseFlow(errorMessage);
                }
            });
        }

        /// <summary>
        /// 서버에 등록된 내가 획득한 카드 수량을 초기상태로(5장) 초기화.
        /// 계정 생성시 실행되어야함
        /// </summary>
        public void CurrencyInitOC()
        {
            plugin.CurrencySet(CRRENCY_STRING, 5, (status, errorMessage, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogWarning("리소스 폴더에서 __DATA4__ 5장 생성 : " + values["amount"]);
                    isNickNameExist = NameType.PASS;
                }
                else
                {
                    Debug.LogError("CurrencySet Fail");
                    NanooCaseFlow(errorMessage);
                }
            });
        }

        /// <summary>
        /// 나누에서 "OC" Owned Card 항목 불러온다.
        /// </summary>
        public void CurrencyGet()
        {
            plugin.CurrencyGet(CRRENCY_STRING, (status, errorMessage, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    MyDeckSaver.UpdateOCcount(values["amount"]);
                }
                else
                {
                    Debug.LogError("CurrencyGet Fail");
                    NanooCaseFlow(errorMessage);
                }
            });
        }

        /// <summary>
        /// 고객 센터 등록할때 플레이어 정보 넘겨줌
        /// </summary>
        public void OpenHelpDesk()
        {
            popBind.Pop_1Button_Init(null, "린 윈도우 테스트", "린 윈도우 테스트");
            popBind.ShowThisPop();

            plugin.SetHelpDeskOptional("플레이어 아이디", PlayerID);
            //plugin.SetHelpDeskOptional("OptionTest2", "ValueTest2");
            plugin.OpenHelpDesk();
        }

        /// <summary>
        /// 세이브 플러스
        /// </summary>
        public void StorageSavePlus()
        {
            plugin.Storage.SavePlus(PlayerID + "_TEST_KEY_001", "TEST_VALUE_001", true,
                (status, errorCode, jsonString, values) =>
                {
                    if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                    {
                        Debug.LogWarning("ActionKey : " + values["ActionKey"]);
                    }
                    else
                    {
                        Debug.LogError("StorageSavePlus Fail : 액션키 오류 -> 액세스 토큰 초기화 하거나, 이전 데이터 로드하고 세이브 하기");
                        popBind.Pop_1Button_Init(null, "StorageSavePlus Fail", "TODO : 액세스 토큰 초기화 하거나, 이전 데이터 로드하고");
                        popBind.ShowThisPop();
                    }
                });
        }

        /// <summary>
        /// 로드 플러스
        /// </summary>
        public void StorageLoadPlus()
        {
            plugin.Storage.LoadPlus(PlayerID + "_TEST_KEY_001", (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogWarning("StorageKey : " + values["StorageKey"]);
                    Debug.LogWarning("StorageValue : " + values["StorageValue"]);
                    Debug.LogWarning("ActionKey : " + values["ActionKey"]);
                }
                else
                {
                    Debug.LogError("StorageLoadPlus Fail : 액션키 오류? 로드후 세이브 해보셈");
                    popBind.Pop_1Button_Init(null, "StorageLoadPlus Fail", "TODO : 액션키 오류? 로드후 세이브 해보셈");
                    popBind.ShowThisPop();
                }
            });
        }

        /// <summary>
        /// 나누에서 받아오는 서버 시간.
        /// </summary>
        public ObscuredInt NanooTimestamp { get; set; }

        public string ISO_8601_date { get; set; }

        /// <summary>
        /// 서버 시간 불러오고 코루틴으로 비교
        /// </summary>
        public void GetServerTime()
        {
            NanooTimestamp = 0;
            ISO_8601_date = string.Empty;
            plugin.ServerTime((state, message, rawData, dictionary) =>
            {
                if (state.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogWarning(dictionary["timezone"]);
                    Debug.LogWarning(dictionary["timestamp"]);
                    Debug.LogError(dictionary["ISO_8601_date"]);
                    NanooTimestamp = int.Parse((string)dictionary["timestamp"]);
                    ISO_8601_date = (string)dictionary["ISO_8601_date"];
                }
                else
                {
                    Debug.LogError("Fail");
                    ShowNetworkWarnning();
                }
            });
        }


        /// <summary>
        /// (클라우드 코드로 옮기면서 안씀)가챠 확률 가져오기
        /// </summary>
        public void LoadCardGachaProb()
        {
            plugin.Storage.LoadPublic("GACHA_PROB", (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    Debug.LogError($"키:{values["StorageKey"]}");
                    PlayerPrefs.SetString("__MY_DATA7__", values["StorageValue"].ToString());
                    PlayerPrefs.Save();
                }
                else
                {
                    Debug.LogError("LoadOtherUserPlus Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }
        
        /// <summary>
        /// 나누에서 받아오는 가차 결과 인덱스
        /// </summary>
        internal ObscuredInt GotChaCardResult { get; set; }

        /// <summary>
        /// 클라우드 코드 작동시켜주세요
        /// </summary>
        /// <param name="GachaKey"0123 으로 등급별 뽑기</param>
        public void GetGotChaProbability(string GachaKey)
        {
            /// 초기화 값이 0 미만이 되어야한다.
            GotChaCardResult = -1;
            
            var parameters = new CloudCodeExecution()
            {
                TableCode = "royalcarddefense-CS-EF74602F-146EE97F",
                FunctionName = GachaKey,
                FunctionArguments = new { 
                    inputValue1 = "inputValue1",
                }
            };

            plugin.CloudCode.Run(parameters, (state, message, rawData, dictionary) =>
            {
                if (state.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    PlayNANOO.SimpleJSON.JSONNode node =
                        PlayNANOO.SimpleJSON.JSONNode.Parse(dictionary["Result"].ToString());
                    //Debug.LogError(node["Function"]["Name"].Value);
                    //Debug.LogError(node["Function"]["Version"].Value);
                    GotChaCardResult = node["Result"].AsInt;
                    Debug.LogError("뽑은 카드 인덱스 " + GotChaCardResult);
                }
                else
                {
                    Debug.LogError("GetGotChaProbability Fail");
                }
            });
        }


        /// <summary>
        /// NanooSaveKey.거시기 로 호출하는 나누 데이터 담아주는 스트링
        /// </summary>
        internal ObscuredString PublicDataContainer { get; set; }

        /// <summary>
        /// 퍼블릭 데이터 3가지 분류해서 JSON이나 스트링으로 긁어오기
        /// </summary>
        /// <param name="saveKey"></param>
        public void LoadPublicData(string saveKey)
        {
            plugin.Storage.LoadPublic(saveKey, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    switch (values["StorageKey"])
                    {
                        // 서버 공지사항 불러오기
                        case NanooSaveKey.NOTICE:
                            Debug.LogError(values["StorageValue"]);
                            break;
                        // 공용 데이터 어떻게 쓰지?
                        case NanooSaveKey.PUBLIC_DATA:
                            break;
                        // TODO : 서버 닫을때 -1 / 아니라면 최신 클라이언트 버전을 맞춰줌
                        case NanooSaveKey.SEVER_VERSION:

                            break;

                        // XX 님이 집행검을 뽑았습니다!! 메시지용
                        case NanooSaveKey.SEVER_MSG_ITEM:
                            PublicDataContainer = values["StorageValue"].ToString();
                            break;

                        // NanooSaveKey 에 정해지지 않은 값으로 호출했다 = 에러
                        default:
                            Debug.LogError("StorageKey 에러났음");
                            NanooCaseFlow(errorCode);
                            break;
                    }
                }
                else
                {
                    Debug.LogError("LoadPublicData Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }

        /// <summary>
        /// 알람 테스트용 스트링 가져오기
        /// </summary>
        internal ObscuredString AlramString { get; set; }

        /// <summary>
        /// 개발용 알람 시간 설정
        /// </summary>
        public void LoadAlramString()
        {
            plugin.Storage.LoadPublic(NanooSaveKey.ALRAM, (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    AlramString = $"{values["StorageValue"]}";
                    Debug.LogError(values["StorageValue"]);
                }
                else
                {
                    Debug.LogError("LoadPublicData Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }

        /// <summary>
        /// JSON 서버에서 가져오는 카드 인벤토리
        /// </summary>
        internal ObscuredString TmpCardJson { get; set; }

        /// <summary>
        /// 카드 json 불러오기
        /// </summary>
        public void LoadJsonCard()
        {
            plugin.Storage.LoadPublic("TEST_JSON", (status, errorCode, jsonString, values) =>
            {
                if (status.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    TmpCardJson = $"{values["StorageValue"]}";
                    Debug.LogError($"키:{values["StorageKey"]}");
                }
                else
                {
                    Debug.LogError("LoadOtherUserPlus Fail");
                    NanooCaseFlow(errorCode);
                }
            });
        }

        /// <summary>
        /// 우편함 조회
        /// </summary>
        public void Postbox()
        {
            plugin.PostboxItem((state, message, rawData, dictionary) =>
            {
                if (state.Equals(Configure.PN_API_STATE_SUCCESS))
                {
                    ArrayList items = (ArrayList)dictionary["item"];
                    foreach (Dictionary<string, object> item in items)
                    {
                        //Debug.LogWarning(item["uid"]);
                        Debug.LogWarning(item["message"]);
                        Debug.LogWarning(item["item_code"]);
                        Debug.LogWarning(item["item_count"]);
                        //Debug.LogWarning(item["expire_sec"]);
                        BanUserDribbble(item["item_code"]); // 혹시 방금 받은 우편이 옐로 / 레드 카드인가요?
                    }
                }
                else
                {
                    Debug.LogError("Postbox Fail");
                    ShowNetworkWarnning();
                }
            });
        }

        /// <summary>
        /// 우편함 조회할때 옐로/레드 카드 받은 사람 처리. Red / Yello
        /// </summary>
        void BanUserDribbble(object values)
        {
            switch (values)
            {
                case "CODE_YELLO":
                    /// 경고 팝업 출력
                    Debug.LogError("CODE_YELLO");
                    break;

                case "CODE_RED":
                    /// 게임 강제 종료 + 로컬 데이터 파괴
                    Debug.LogError("CODE_RED");
                    AllDataReset();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 네트워크 에러 팝업 -> 앱 재실행 팝업 노출
        /// </summary>
        internal void ShowNetworkWarnning()
        {
            popBind.Pop_1Button_Init(null, "네트워크 불안정", "네트워크 상태를 확인해주세요.");
            popBind.ShowThisPop();
        }

        /// <summary>
        /// 차단된 계정입니다. 앱 재실행 -> 앱 재실행 말고 불쾌감 감소 가능?
        /// </summary>
        internal void DarkUserWarnning()
        {
            popBind.Pop_RealExit_Init(RestartAppForAOS, "알림", "차단된 계정입니다. 앱 재실행");
        }


        /// <summary>
        /// 케이스 별로 쪼개서 해당 에러내용 팝업 뿌려줌
        /// </summary>
        /// <param name="strCode">나누에서 받아오는 에러코드</param>
        void NanooCaseFlow(string strCode)
        {
            switch (strCode)
            {
                case "30000":
                    // NotFoundAccountException 플레이어 정보가 존재하지 않음
                    break;
                case "30001":
                    // NotMatchAccountException 플레이어 정보가 일치 하지 않음
                    break;
                case "30002":
                    // ExpiredTokenException 토큰 만료됨
                    break;
                case "30005":
                    // NullTokenException 토큰 NULL
                    break;
                case "70002":
                    // 차단된 계정임
                    DarkUserWarnning();
                    break;
                default:
                    ShowNetworkWarnning();
                    break;
            }
        }


        /// <summary>
        /// 나누 OC와 로컬 OC가 불일치할 경우
        /// </summary>
        internal void ShowPopOwnedCardError()
        {
            StartCoroutine(NoNoLocalCard());
        }

        IEnumerator NoNoLocalCard()
        {
            GetServerTime();

            while (ISO_8601_date == string.Empty)
            {
                yield return null;
                yield return null;
                yield return null;
            }
            popBind.Pop_RealExit_Init(RestartAppForAOS, "나누 OC와 로컬 OC의 불일치", $"발생 시각 : {ISO_8601_date}");
        }
        
        /// <summary>
        /// 싹 지우고 타이틀 화면 LoginScene 으로 보내기
        /// </summary>
        void AllDataReset()
        {
            Debug.LogWarning("모든 로컬 데이터 삭제");

            /// 나누 토큰 해지
            AccountTokenSignOut();
            PlayerPrefs.DeleteAll();
            GameServices.SignOut();
            // 타이틀 화면으로 보내기
            MySceneManager.Instance.JumpScemaLocation(MyLocation.LoginScene);
        }

        /// <summary>
        /// 1초 타이머
        /// </summary>
        private ObscuredFloat secondCountdown = 0;


        void Update()
        {
            // 1초마다 실행
            if (secondCountdown > 0.9f)
            {
                /// 나누 중복 로그인 감지
                plugin.AccountCheckDuplicate(OnCheckAccountDuplicate);
                secondCountdown = 0;
            }
            else
            {
                secondCountdown += Time.deltaTime;
            }
        }


        /// <summary>
        /// 중복 로그인 탐지용
        /// </summary>
        public ObscuredBool IsDuplicate { get; set; }

        /// <summary>
        /// 중복로그인 탐지시 팝업 출력
        /// </summary>
        /// <param name="isDup"></param>
        void OnCheckAccountDuplicate(bool isDup)
        {
            if (IsDuplicate || !isDup)
            {
                return;
            }
            
            IsDuplicate = true;
            Debug.LogError("중복 로그인 감지 Duplicate connection has been detected.");
            // TODO : 임시로 토큰 초기화 버튼 넣어둠
            popBind.Pop_2Button_Init(DupliTrrigerOn, AccountTokenSignOut, "중복 로그인 시도 감지", "다른 곳에서 로그인 하지마삼");
            popBind.ShowThisPop();

        }

        private void DupliTrrigerOn() => IsDuplicate = false;

        public void OnApplicationFocus(bool focus)
        {
            if (IsNannoLogin && focus)
            {
                /// TODO : 나누 우편함 새로고침 주기 설정 바람
                Postbox();
            }
        }
    }
}