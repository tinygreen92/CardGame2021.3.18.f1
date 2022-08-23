using System.Collections;
using System.Text.RegularExpressions;
using Databox;
using EasyMobile;
using UnityEngine;
using UnityEngine.UI;
using GoogleGame;
using System;
using System.IO;

namespace GoogleGame
{
    public class LoginSceneScript : MonoBehaviour
    {
        [Header(" - 첫 화면에 버전 정보 표기할 텍스트")]
        [SerializeField] Text verText;
        [Header(" - 팝업 매니저")]
        [SerializeField] PopManager popManager;
        [Space]
        [SerializeField] GameObject LoginBtn;
        [SerializeField] Image Loading_Bar;
        [SerializeField] Text Loading_text;
        [Space]
        ///배드 울프
        [SerializeField] DataboxObject dataBox;

        /// <summary>
        /// DataBox에서 생성한 json 파일을 연결시켜둠
        /// </summary>
        [Space] [SerializeField] private TextAsset[] texts;

        /// <summary>
        /// [Btn] 디버그 로그 꺼줌
        /// </summary>
        public void TEST_DebugLogger()
        {
            UnityEngine.Debug.unityLogger.logEnabled = false;
        }

        /// <summary>
        /// [Btn] 중복확인 버튼 닉네임 중복체크
        /// </summary>
        public void NickCheck()
        {
            NanooManager.Instance.isNickNameExist = NameType.NONE;

            var tmpNick = popManager.GetInputFiled();
            /// 인풋 필드 받아와서 닉네임 걸러내기.
            if (IsContainsBadWord(tmpNick.ToUpper()))
            {
                popManager.ResetInputFiled();
                popManager.ShowSameSameText("부적절한 닉네임입니다.");
                return;
            }
            /// 통과되면?
            ///             1. 2자 이상 인지?
            if (tmpNick.Length < 2 || tmpNick.Length > 8)
            {
                popManager.ShowSameSameText("2자 이상, 8자 이하만 허용됩니다.");
                return;
            }
            ///             2. 특수 문자 포함되어있는지?
            string idChecker = Regex.Replace(tmpNick, @"[^0-9a-zA-Z가-힣]", "");

            Debug.LogWarning($"tmpNick : [{tmpNick}]");
            Debug.LogWarning($"idChecker : [{idChecker}]");
            /// 원래 문장과 다르다면 ? (false)
            if (tmpNick.Equals(idChecker) == false)
            {
                popManager.ResetInputFiled();
                popManager.ShowSameSameText("특수문자, 공백은 허용되지 않습니다.");
                return;
            }
            /// 통과되면?
            popManager.ShowSameSameText("서버 체크 중...");
            ///             1. 나누 접속 닉네임 중복 확인
            StartCoroutine(Nickchecheck(tmpNick));
        }
        
        /// <summary>
        /// 나누 닉네임 중복 체크
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        IEnumerator Nickchecheck(string nick)
        {
            yield return null;
            
            NanooManager.Instance.AccountNicknameExists(nick);
            /// 1이나 2가 나올때까지 뺑뻉이 돌림
            while (NanooManager.Instance.isNickNameExist.Equals(NameType.NONE))
            {
                yield return null;
                yield return null;
                yield return null;
            }

            if (NanooManager.Instance.isNickNameExist.Equals(NameType.NOT_EXIST))
                popManager.ShowSameSameText("사용 가능한 닉네임입니다.");
            else
                popManager.ShowSameSameText("중복된 닉네임입니다.");

        }




        /// <summary>
        /// 닉네임에 나쁜 단어가 들어있니?
        /// </summary>
        /// <param name="inputword"></param>
        /// <returns></returns>
        bool IsContainsBadWord(string inputword)
        {
            // Access data
            Debug.LogWarning("Access Bad Word DB");

            var badwordlist = dataBox.GetData<StringListType>("BAD_WORD", "BAD", "WORD");
            var length = badwordlist.Value.Count;
            for (int i = 0; i < length; i++)
            {
                if (inputword.Contains(badwordlist.Value[i].ToUpper()))
                {
                    Debug.LogWarning($"필터링 된 단어 : {badwordlist.Value[i]}");
                    return true;
                }
            }
            /// 안들어있네? 통과
            return false;
        }


        /// <summary>
        /// ▶ 로딩 완료 후 로그인 버튼 생성
        /// </summary>
        private void Start()
        {
            verText.text = $"dev build v{Application.version}";
            
            // 욕설 데이터 호출
            dataBox.LoadDatabase();
            /// 로컬 카드덱 구성 복사 -> 호출
            InitAllDataBox();
            /// 로그인 플로우
            StartCoroutine(LoginLoadScene());
        }
        
        /// <summary>
        /// 로딩 끝나면 
        /// 기존 유저 자동 로그인 시도
        /// 신규 유저 로그인 버튼 팝업 
        /// </summary>
        /// <returns></returns>
        IEnumerator LoginLoadScene()
        {
            yield return null;

            float past_time = 0;
            float percentage = 0;
            /// 
            if (Application.installerName != "com.android.vending")
            {
                Debug.LogError("TODO : 구글 스토어에서 다운 받은게 아님");
                //yield break;    // 코루틴 종료
            }

            while (percentage < 99f)
            {
                yield return null;

                past_time += Time.deltaTime;
                percentage = Mathf.Lerp(percentage, 100f, past_time);

                Loading_Bar.fillAmount = percentage / 100f;
                Loading_text.text = $"{percentage:0}%"; //로딩 퍼센트 표기
            }

            Loading_Bar.gameObject.SetActive(false);

            /// 키가 이미 존재한다 = 계약 동의버튼 누름 =  구글 로그인 한 적이 있다.
            if (PlayerPrefs.HasKey("IsOldUser"))
            {
                //popManager.TEST_TutoskipName();
                /// 바로 자동 구글 로그인 시도
                ClickedLoginSceneBtn();
            }
            else /// 계약 동의버튼 누른적 없음 -> 눌러서 구글 로그인 팝업
            {
                LoginBtn.SetActive(true);
            }
        }





        private string GetPersisentPath(string path)
        {
            return Path.Combine(Application.persistentDataPath, path);
        }


        /// <summary>
        /// 회심의 역작
        /// </summary>
        private void InitAllDataBox()
        {
            ////////////////////////////////////////////////
            // __DATA4__ MyTempCard 보안 파일이 생성 안됐다면
            ////////////////////////////////////////////////
            if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_00)) && !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_00)))
            {
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA4__"))
                {
                    PlayerPrefs.SetString("__MY_DATA4__", texts[0].text);
                    // 쪼개서 로컬 저장
                    DoThanosCreator(texts[0].text, MyPersistentDataPath.LEFT_00, MyPersistentDataPath.RIGHT_00);
                }
                // 로컬 파일은 없는데 PlayerPrefs에는 존재함
                else 
                {
                    // 쪼개서 로컬 저장
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA4__"), MyPersistentDataPath.LEFT_00, MyPersistentDataPath.RIGHT_00);
                }
            }
            else if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_00)) || !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_00)))
            {
                // 하나만 존재한다면? (외부 요인으로 손상됨)
                // TODO : 유저에게 경고 팝업을 출력시키고, 해당 섹션 Nanoo로 업로드할 것
                Debug.LogWarning("__DATA4__texts[0] 데이터 손상됨 -> TODO : 나누 팝업 작업");
            }
            else
            {
                // 둘다 존재하면 정상 로딩 된거 TODO : 정상로딩인척 하는데 프리페어런스에 없을수도 있음 ㅡㅡ
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA4__"))
                {
                    PlayerPrefs.SetString("__MY_DATA4__", texts[0].text);
                    // 쪼개서 로컬 저장
                    DoThanosCreator(texts[0].text, MyPersistentDataPath.LEFT_00, MyPersistentDataPath.RIGHT_00);
                    Debug.LogWarning("__DATA4__texts[0] 정상로딩인척 하는데 프리페어런스에 없어서 새로 만듦");
                }
                else
                {
                    Debug.LogWarning("__DATA4__texts[0] 정상 로딩");
                }
            }
            
            ////////////////////////////////////////////////
            // __DATA1__ 개인 설정 파일은 하나로 굴린다.
            ////////////////////////////////////////////////
            if (!File.Exists(GetPersisentPath(MyPersistentDataPath.CONFIG)))
            {
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA1__"))
                {
                    PlayerPrefs.SetString("__MY_DATA1__", texts[1].text);
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA1__"), MyPersistentDataPath.CONFIG);
                }
                // 로컬 파일은 없는데 PlayerPrefs에는 존재함
                else 
                {
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA1__"), MyPersistentDataPath.CONFIG);
                }
            }
            else if (!File.Exists(GetPersisentPath(MyPersistentDataPath.CONFIG)))
            {
                Debug.LogWarning("__MY_DATA1__[1] 데이터 손상됨");
            }
            else
            {
                // 정상적으로 존재한다면?
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA1__"))
                {
                    PlayerPrefs.SetString("__MY_DATA1__", texts[1].text);
                    // 쪼개서 로컬 저장
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA1__"), MyPersistentDataPath.CONFIG);
                    Debug.LogWarning("__MY_DATA1__[1] 정상로딩인척 하는데 프리페어런스에 없어서 새로 만듦");
                }
                else
                {
                    Debug.LogWarning("__MY_DATA1__[1] 정상 로딩");
                }
            }
            
            ///////////////////////////////////
            // __DATA2__ 카드 인벤토리 쪼개기
            ///////////////////////////////////
            if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_02)) && !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_02)))
            {
                DoThanosCreator(texts[2].text, MyPersistentDataPath.LEFT_02, MyPersistentDataPath.RIGHT_02);
            }
            else if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_02)) || !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_02)))
            {
                Debug.LogWarning("__DATA2__texts[2] 모든 카드 상세정보 데이터 손상됨");
            }
            else
            {
                Debug.LogWarning("__DATA2__texts[2] 모든 카드 상세정보 정상 로딩");
            }

            ///////////////////////////////////
            // __DATA6__  <아이템 인벤토리>
            ///////////////////////////////////
            if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_03)) && !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_03)))
            {
                
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA6__"))
                {
                    PlayerPrefs.SetString("__MY_DATA6__", texts[3].text);
                    DoThanosCreator(texts[3].text, MyPersistentDataPath.LEFT_03, MyPersistentDataPath.RIGHT_03);
                }
                // 로컬파일은 없는데 프리페어런스에 있는게 있다면?
                else
                {
                    // 쪼개서 로컬 저장
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA6__"), MyPersistentDataPath.LEFT_03, MyPersistentDataPath.RIGHT_03);
                }
            }
            else if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_03)) || !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_03)))
            {
                Debug.LogWarning("__DATA6__texts[3] 습득한 아이템 테이블 관리 데이터 손상됨");
            }
            else
            {
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA6__"))
                {
                    PlayerPrefs.SetString("__MY_DATA6__", texts[3].text);
                    DoThanosCreator(texts[3].text, MyPersistentDataPath.LEFT_03, MyPersistentDataPath.RIGHT_03);
                    Debug.LogWarning("__DATA6__texts[3] 정상 로딩인 척 하길래 새로 생성");
                }
                else
                {
                    Debug.LogWarning("__DATA6__texts[3] 습득한 아이템 테이블 관리 정상 로딩");
                }
                
            }
            
            
            ////////////////////////////////////////////////
            // __DATA7__ 페이크 서버시간이 저장이 안됐다면
            ////////////////////////////////////////////////
            if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_01)) && !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_01)))
            {
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA7__"))
                {
                    PlayerPrefs.SetString("__MY_DATA7__", texts[4].text);
                    // 쪼개서 로컬 저장
                    DoThanosCreator(texts[4].text, MyPersistentDataPath.LEFT_01, MyPersistentDataPath.RIGHT_01);
                }
                // 로컬 파일은 없는데 PlayerPrefs에는 존재함
                else 
                {
                    // 쪼개서 로컬 저장
                    DoThanosCreator(PlayerPrefs.GetString("__MY_DATA7__"), MyPersistentDataPath.LEFT_01, MyPersistentDataPath.RIGHT_01);
                }
            }
            else if (!File.Exists(GetPersisentPath(MyPersistentDataPath.LEFT_01)) || !File.Exists(GetPersisentPath(MyPersistentDataPath.RIGHT_01)))
            {
                // 하나만 존재한다면? (외부 요인으로 손상됨)
                // TODO : 유저에게 경고 팝업을 출력시키고, 해당 섹션 Nanoo로 업로드할 것
                Debug.LogWarning("__DATA7__texts[4] 데이터 손상됨 -> TODO : 나누 팝업 작업");
            }
            else
            {
                // 둘다 존재하면 정상 로딩 된거 TODO : 정상로딩인척 하는데 프리페어런스에 없을수도 있음 ㅡㅡ
                /// 프리페어런스 생성
                if (!PlayerPrefs.HasKey("__MY_DATA7__"))
                {
                    PlayerPrefs.SetString("__MY_DATA7__", texts[4].text);
                    // 쪼개서 로컬 저장
                    DoThanosCreator(texts[4].text, MyPersistentDataPath.LEFT_01, MyPersistentDataPath.RIGHT_01);
                    Debug.LogWarning("__DATA7__texts[4] 정상로딩인척 하는데 프리페어런스에 없어서 새로 만듦");
                }
                else
                {
                    Debug.LogWarning("__DATA7__texts[4] 정상 로딩");
                }
            }
            
            
            
            /*
             * End
             */
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 암호화된 문자열 절반으로 자른 후 로컬 저장
        /// </summary>
        /// <param name="input">Databox 에서 인식가능한 원본</param>
        /// <param name="outputLeft">왼쪽 타노스</param>
        /// <param name="outputRight">오른쪽 타노스</param>
        private void DoThanosCreator(string input, string outputLeft, string outputRight)
        {
            int halfLength = input.Length / 2;
            File.WriteAllText(GetPersisentPath(outputLeft), input.Substring(0, halfLength));
            File.WriteAllText(GetPersisentPath(outputRight), input.Substring(halfLength, (input.Length - halfLength)));
        }
        private void DoThanosCreator(string input, string outputSolo)
        {
            File.WriteAllText(GetPersisentPath(outputSolo), input);
        }

        /// <summary>
        /// [btn]   로그인 버튼 누르면 
        ///         데이터 박스 초기화 후 
        ///         구글 로그인 시도 
        /// </summary>
        public void ClickedLoginSceneBtn()
        {
#if UNITY_EDITOR
            /// 유니티 에디터에서
            Btn_GameMasterLogin(); /// 디버그 모드는 구글 로그인 무시
#else
            /// 안드로이드 폰에서
            StartCoroutine(MoveScene());
#endif

        }

        /// <summary>
        /// GAMEMASTER 로그인 버튼 누르면
        /// </summary>
        public void Btn_GameMasterLogin()
        {
            // 계약 동의 팝업 스킵하고
            popManager.GayMaster();
        }


        /// <summary>
        /// 구글/나누 로그인 하고 찐 씬으로 넘어가는
        /// </summary>
        /// <returns></returns>
        IEnumerator MoveScene()
        {
            yield return null;
            
            int trylogincnt = 10;
            while (true)
            {
                yield return null;

                /// 플레이 구글 게임에 로그인 성공했나?
                if (GameServices.IsInitialized())
                {
                    UnityEngine.Debug.LogError("gpgs userName : " + GameServices.LocalUser.userName);
                    UnityEngine.Debug.LogError("gpgs id : " + GameServices.LocalUser.id);

                    /// 계약 동의까지는 진행 했다면 -> 동의 팝업 패스하고 나누 로그인
                    if (PlayerPrefs.HasKey("IsOldUser"))
                    {
                        /// 첫 로그인 페이지 스킵하고 본 게임으로
                        popManager.AppUp();
                    }
                    /// 첫 구글 로그인이라면 -> 계약 동의 팝업 
                    else
                    {
                        popManager.BecomingOld();
                    }
                    break;
                }
                else
                {
                    /// 로그인 성공 못했으면 3초마다 10번 시도. (총 30초까지 기다려준다.)
                    GameServices.Init();
                    //OnLogin();
                    trylogincnt--;
                    yield return new WaitForSeconds(3f);
                }
                /// 10번 시도 후에도 로그인 실패했다면 로그인 실패 팝업 ? 혹은? 수동 로그인 버튼 다시 노출 시켜
                if (trylogincnt < 0)
                {
                    popManager.ShowPopLoginFail();
                    break;
                }
            }

        }

    }
}