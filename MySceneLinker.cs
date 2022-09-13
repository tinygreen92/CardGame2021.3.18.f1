using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleGame;

namespace GoogleGame
{
    public class MySceneLinker : MonoBehaviour
    {
        private string persistentDataPath;

        private void Awake()
        {
            persistentDataPath = Application.persistentDataPath;
        }

        /// <summary>
        /// 버튼으로 써서 다른 씬으로 넘기는 코드 -> 레거시 코드 재활용해준다
        /// </summary>
        /// <param name="nameScene"></param>
        public void ClickedNexeScene(string nameScene)
        {
            /// 게임 속도가 통상이 아닐 경우 씬 전환시 1로 바꿔줌
            if (!Time.timeScale.Equals(1)) Time.timeScale = 1;

            switch (nameScene)
            {
                case "01.LoginScene":
                    MySceneManager.Instance.locations[0].Enter();
                    break;

                case "02.MainScene":
                    MySceneManager.Instance.locations[1].Enter();
                    break;

                case "03.BattleScene":
                    MySceneManager.Instance.locations[2].Enter();
                    break;
                
            }
        }

        #region <-- 오디오 매니저 인스턴트 테스트용 -->

        public void PlayBGM(string strBgm)
        {
            AudioManager.Instance.PlayBGM(strBgm);
        }
        
        public void PlaySFX(string strSFX)
        {
            AudioManager.Instance.PlaySFX(strSFX);
        }

        #endregion

        #region <푸쉬 기능 테스트용>

        /// <summary>
        /// 10초 뒤에 로컬 푸쉬
        /// </summary>
        public void TEST_BTN_pushpush()
        {
            GameManager.instance.TEST_LocalNoti(10);
        }

        /// <summary>
        /// 지정된 시각에 알람 울리기
        /// </summary>
        public void TEST_BTN_AlramPush()
        {
            StartCoroutine(nameof(Alaallalaal));
        }

        IEnumerator Alaallalaal()
        {
            yield return null;
            NanooManager.Instance.LoadAlramString();

            while (NanooManager.Instance.AlramString == null)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            GameManager.instance.TEST_AlramNoti(NanooManager.Instance.AlramString);
        }

        #endregion
        
        #region <나누기능 땡겨오기>

        /// <summary>
        /// 서버 시간과 로컬 시간 비교
        /// </summary>
        public void NANOO_TimeChecking()
        {
            NanooManager.Instance.GetServerTime();
            StartCoroutine(LoadNanooTime());
        }

        /// <summary>
        /// 나누에서 데이터 가져오기 성공하면  playerPrefs에 저장해줌
        /// </summary>
        IEnumerator LoadNanooTime()
        {
            // 나누에서 받아오는 서버 시간. 받을 때까지 대기
            while (NanooManager.Instance.NanooTimestamp == 0)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            /// 서버시간과 로컬시간의 간극은 +- 150초 미만으로 제어한다.
            ObscuredInt bojung = 150;
            /// 로컬 타임스태프 계산
            var now = DateTime.Now.ToLocalTime();
            var span = (now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            ObscuredInt localTimestamp = (ObscuredInt)span.TotalSeconds;
            ObscuredInt severTimestamp = NanooManager.Instance.NanooTimestamp;
            Debug.LogWarning($"로컬 : {localTimestamp} / 서버 : {severTimestamp}");

            if (localTimestamp - bojung < severTimestamp && localTimestamp + bojung > severTimestamp)
            {
                Debug.LogWarning("로컬 시간과 서버 시간 별 차이 안난다. 정상이다.");
            }
            else
            {
                // TODO : 기기 시간 조작이 의심된다 팝업
                NanooManager.Instance.ShowNetworkWarnning();
            }
        }


        public void NANOO_BTN_StorageSavePlus() => NanooManager.Instance.StorageSavePlus();
        public void NANOO_BTN_StorageLoadPlus() => NanooManager.Instance.StorageLoadPlus();
        public void NANOO_BTN_Postbox() => NanooManager.Instance.Postbox();
        public void NANOO_BTN_TokenStatus() => NanooManager.Instance.AccountTokenStatus();
        public void NANOO_BTN_TokenSignOut() => NanooManager.Instance.AccountTokenSignOut();
        public void NANOO_BTN_OpenHelpDesk() => NanooManager.Instance.OpenHelpDesk();
        
        public void NANOO_BTN_ShopInfo() => NanooManager.Instance.OpenShopInfo();

        public void NANOO_BTN_AccountWithDrawal() => NanooManager.Instance.AccountWithDrawal();


        public void TEST_AutoPlay() => AutoPlay.instance.TestBtn_AutoPlay();


        /// <summary>
        /// 나누 공지사항 불러오기
        /// </summary>
        public void Nanoo_Btn_OtherPlayerLoad()
        {
            NanooManager.Instance.LoadPublicData(NanooSaveKey.NOTICE);
        }


        /// <summary>
        /// 클라우드 코드로 가챠하기
        /// </summary>
        public void Nanoo_Btn_GetGachaProb()
        {
            /// 테스트용으로 레어 뽑기
            NanooManager.Instance.GetGotChaProbability(NanooSaveKey._Rare);
        }

        #endregion


        /// <summary>
        /// 나누에서 데이터 가져오기 성공하면  playerPrefs에 저장해줌
        /// </summary>
        IEnumerator LoadJsonCo()
        {
            // 나누에서 편집한 카드 인벤토리 받을 때까지 대기
            while (NanooManager.Instance.TmpCardJson == null)
            {
                yield return null;
                yield return null;
                yield return null;
            }

            // 서버에서 카드 인벤토리 불러와서 덮어쓰기
            PlayerPrefs.SetString("__MY_DATA2__", NanooManager.Instance.TmpCardJson);
            yield return null;
            PlayerPrefs.Save();
            // 이제 필요없으니 저장소 비워줌
            // null 값만 아니면 됨 -> InventoryManager.SetStartCard()
            NanooManager.Instance.TmpCardJson = "-1";
        }

        /// <summary>
        /// 데이터 박스 이상없으면 씬을 시작함
        /// </summary>
        public void MainSceneStater()
        {
            // 서버에서 받아온 로컬 데이터 있는지 검증
            if (PlayerPrefs.HasKey("__MY_DATA2__"))
            {
                // playerPrefs에 데이터 있다
                Debug.LogError("로컬로 시작 MainSceneStater");
                StartCoroutine(LoadConfig());
            }
            else
            {
                /// playerPrefs에 __MY_DATA2__.json 파일이 없다
                Debug.LogError("서버로 시작 MainSceneStater");
                NanooManager.Instance.LoadJsonCard(); // plugin.Storage.LoadPublic("TEST_JSON"...)
                StartCoroutine(nameof(LoadJsonCo));
            }
        }

        /// <summary>
        /// 서버에서 받아온 데이터가 최신이면 바로 진행함
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadConfig()
        {
            // Start a thread on the first frame
            string output = string.Empty;
            bool done = false;
            new Thread(() =>
            {
                // Load and parse the JSON without worrying about frames
                output = File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.LEFT_00)) +
                         File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.RIGHT_00));
                done = true;
            }).Start();

            // Do nothing on each frame until the thread is done
            while (!done)
            {
                yield return null;
            }

            // _DATA4_MyCard_Owned 에 연결된 prefs 에 카드 숫자 MyTempCard 덮어쓰기
            PlayerPrefs.SetString("__MY_DATA4__", output);
            Debug.LogWarning("__MY_DATA4__ 합치기 완료");
            yield return null;

            // Start a thread on the first frame
            output = string.Empty;
            done = false;
            new Thread(() =>
            {
                // Load and parse the JSON without worrying about frames
                output = File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.LEFT_02)) +
                         File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.RIGHT_02));
                done = true;
            }).Start();

            // Do nothing on each frame until the thread is done
            while (!done)
            {
                yield return null;
            }

            // _DATA2_InventoryDataBox 에 연결된 prefs 에 로컬 세이브 파일 덮어쓰기
            PlayerPrefs.SetString("__MY_DATA2__", output);
            Debug.LogWarning("__MY_DATA2__ 합치기 완료");

            yield return null;

            // Start a thread on the first frame
            output = string.Empty;
            done = false;
            new Thread(() =>
            {
                // Load and parse the JSON without worrying about frames
                output = File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.LEFT_03)) +
                         File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.RIGHT_03));
                done = true;
            }).Start();

            // Do nothing on each frame until the thread is done
            while (!done)
            {
                yield return null;
            }

            // _DATA6_ 아이템 인벤토리 데이터 덮어쓰기
            PlayerPrefs.SetString("__MY_DATA6__", output);
            Debug.LogWarning("__MY_DATA6__ 합치기 완료");
            yield return null;


            // // Start a thread on the first frame
            // output = string.Empty;
            // done = false;
            // new Thread(() => {
            //     // Load and parse the JSON without worrying about frames
            //     output = File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.LEFT_04)) + 
            //              File.ReadAllText(Path.Combine(persistentDataPath, MyPersistentDataPath.RIGHT_04));
            //     done = true;
            // }).Start();
            //
            // // Do nothing on each frame until the thread is done
            // while (!done)
            // {
            //     yield return null;
            // }
            // // __MY_DATA7__ 가챠 확률 데이터 덮어쓰기
            // PlayerPrefs.SetString("__MY_DATA7__", output);
            // Debug.LogWarning("__MY_DATA7__ 합치기 완료");
            // yield return null;


            PlayerPrefs.Save();
            Debug.LogWarning("쪼개진 로컬 데이터를 DataBox PlayerPref로 합치기 완료");
            // TODO : "나누에서 JSON 받아오기 빌드용" 에서 다른 조건으로 바꿔줘야함
            NanooManager.Instance.TmpCardJson = "-1"; // null 값만 아니면 됨
        }


        /// <summary>
        /// 암호화된 문자열 절반으로 자른 후 로컬 저장
        /// </summary>
        /// <param name="input">Databox 에서 인식가능한 원본</param>
        /// <param name="outputLeft">왼쪽 타노스</param>
        /// <param name="outputRight">오른쪽 타노스</param>
        public void DoThanosCreator(string input, string outputLeft, string outputRight)
        {
            int halfLength = input.Length / 2;
            File.WriteAllText(Path.Combine(persistentDataPath, outputLeft), input.Substring(0, halfLength));
            File.WriteAllText(Path.Combine(persistentDataPath, outputRight),
                input.Substring(halfLength, (input.Length - halfLength)));
        }
    }
}