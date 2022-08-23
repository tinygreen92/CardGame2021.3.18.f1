using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using EasyMobile;
using UnityEngine;
using LocalNotification = EasyMobile.LocalNotification;
using RemoteNotification = EasyMobile.RemoteNotification;
using GoogleGame;

public enum EFrameRate
{
    LOW = 30,
    HIGH = 60,
}
namespace GoogleGame
{
    public class GameManager : MonoBehaviour
    {
        #region [Static Variable]

        /// <summary>
        /// 오토 플레이에서 임시로 쓸거
        /// </summary>
        public static bool isAutoPlay;
        
        /// <summary>
        /// Lean 팝업 올라가면 true 내려가면 false 인기라
        /// </summary>
        public static bool isPopPlay;
        
        

        #endregion
        
        
        
        /// <summary>
        /// 코루틴 타이머
        /// </summary>
        [HideInInspector] 
        public Timer coTimer;
        
        public static GameManager instance;
        private void Awake()
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

            /// 코루틴 타이머
            if (coTimer == null)
            {
                coTimer = GetComponent<Timer>();
            }

            Notifications.GrantDataPrivacyConsent();
            QualitySettings.vSyncCount = 0;
            /// 모바일 타겟 프레임 -> 사용자가 선택할 수 있게 해주자 나중에
            Application.targetFrameRate = (int)EFrameRate.HIGH; 
            /// 화면 안꺼짐
            Screen.sleepTimeout = SleepTimeout.NeverSleep;  
            // 터치 입력을 오직 하나만 받는다.
            Input.multiTouchEnabled = false;
            // //  /// 디버그 로그 꺼줌
            // UnityEngine.Debug.unityLogger.logEnabled = false;   
            /// 디버그 로그 꺼줌
        }
        
        void Update()
        {
            // TODO : 뒤로가기 
#if UNITY_ANDROID
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                // Ask if user wants to exit
                NativeUI.AlertPopup alert = NativeUI.ShowTwoButtonAlert("뒤로가기",
                                                "Do you want to exit?",
                                                "Yes",
                                                "No");

                if (alert != null)
                    alert.OnComplete += delegate (int button)
                    {
                        if (button == 0) Application.Quit();
                    };
            }

#endif
        }

        [System.Obsolete]
        private void OnApplicationQuit()
        {
            Application.CancelQuit();
        }


#region <로컬 푸쉬 관련>
        const string DEFAULT_CATEGORY_ID = "notification.category.default";


        // Subscribes to notification events.
        void OnEnable()
        {
            Notifications.LocalNotificationOpened += OnLocalNotificationOpened;
            Notifications.RemoteNotificationOpened += OnRemoteNotificationOpened;
        }

        // Unsubscribes notification events.
        void OnDisable()
        {
            Notifications.LocalNotificationOpened -= OnLocalNotificationOpened;
            Notifications.RemoteNotificationOpened -= OnRemoteNotificationOpened;
        }

        // This handler will be called when a local notification is opened.
        void OnLocalNotificationOpened(LocalNotification delivered)
        {
            // The actionId will be empty if the notification was opened with the default action.
            // Otherwise it contains the ID of the selected action button.
            if (!string.IsNullOrEmpty(delivered.actionId))
            {
                Debug.Log("Action ID: " + delivered.actionId);
            }

            // Whether the notification is delivered when the app is in foreground.
            Debug.Log("Is app in foreground: " + delivered.isAppInForeground.ToString());

            // Gets the notification content.
            NotificationContent content = delivered.content;

            // Take further actions if needed...
        }

        // This handler will be called when a remote notification is opened.
        void OnRemoteNotificationOpened(RemoteNotification delivered)
        {
            // The actionId will be empty if the notification was opened with the default action.
            // Otherwise it contains the ID of the selected action button.
            if (!string.IsNullOrEmpty(delivered.actionId))
            {
                Debug.Log("Action ID: " + delivered.actionId);
            }

            // Whether the notification is delivered when the app is in foreground.
            Debug.Log("Is app in foreground: " + delivered.isAppInForeground.ToString());

            // Gets the notification content.
            NotificationContent content = delivered.content;

            // If Firebase Messaging service is in use you can access the original Firebase
            // payload like below. If Firebase is not in use this will be null.
            FirebaseMessage fcmPayload = delivered.firebasePayload;

            // Take further actions if needed...
        }

        /// <summary>
        /// 몇 초 뒤에 로컬 푸쉬 들어간다
        /// </summary>
        /// <param name="sec"></param>
        internal void TEST_LocalNoti(int sec)
        {
            /// 보류 중인 모든 로컬 알림 취소
            Notifications.CancelAllPendingLocalNotifications();
            //var tmpStr = NanooManager.Instance.NickName;
            ScheduleLocalNotification(0,0, sec);
        }

        /// <summary>
        /// 설정된 시간에 알람 울리게. 01:10:00 AM
        /// </summary>
        internal void TEST_AlramNoti(string timeStr)
        {
            /// 보류 중인 모든 로컬 알림 취소
            Notifications.CancelAllPendingLocalNotifications();

            // 앱을 잠시 쉴 때 지정된 시간에 알림
            DateTime specifiedTime1 = Convert.ToDateTime(timeStr);
            TimeSpan sTime1 = specifiedTime1 - DateTime.Now;

            Debug.LogError("실행 되냐? sTime1 : " + sTime1);

            if (sTime1.Ticks > 0)
            {
                if (!Notifications.IsInitialized())
                {
                    return;
                }

                NotificationContent content = new NotificationContent();

                content.title = "특정 시간 알람";
                content.body = $"{timeStr} 에 알람 떴나?";
                content.categoryId = DEFAULT_CATEGORY_ID;

                // Schedule the notification.
                Notifications.ScheduleLocalNotification(sTime1, content);
            }
        }

        /// <summary>
        /// seconds 후에 알람 표시
        /// </summary>
        /// <param name="seconds"></param>
        void ScheduleLocalNotification(int hour, int min , int seconds)
        {
            if (!Notifications.IsInitialized())
            {
                return;
            }

            NotificationContent content = new NotificationContent();

            content.title = "타이틀";
            content.body = "바디";
            content.categoryId = DEFAULT_CATEGORY_ID;

            // Set the delay time as a TimeSpan.
            TimeSpan delay = new TimeSpan(hour, min, seconds);
            ///DateTime triggerDate = DateTime.Now + new TimeSpan(0, 0, delaySeconds);

            // Schedule the notification.
            Notifications.ScheduleLocalNotification(delay, content);
        }

        /// <summary>
        /// 보류 중인 로컬 푸쉬 보여준다.
        /// </summary>
        void GetPendingLocalNotifications()
        {
            Notifications.GetPendingLocalNotifications(GetPendingLocalNotificationsCallback);
        }

        // Callback.
        void GetPendingLocalNotificationsCallback(NotificationRequest[] pendingRequests)
        {
            foreach (var request in pendingRequests)
            {
                NotificationContent content = request.content;        // notification content

                Debug.LogError($"Notification {request.id} : {content.title} / {content.body}");    // notification request ID
                                                                                                    //Debug.LogError("Notification title: " + content.title);
                                                                                                    //Debug.LogError("Notification body: " + content.body);
            }
        }

        /// <summary>
        /// 제목이랑 내용 집어 넣기 샘플
        /// </summary>
        /// <returns></returns>
        NotificationContent PrepareNotificationContent()
        {
            NotificationContent content = new NotificationContent();

            // You can optionally attach custom user information to the notification
            // in form of a key-value dictionary.
            content.userInfo = new Dictionary<string, object>();
            content.userInfo.Add("string", "OK");
            content.userInfo.Add("number", 3);
            content.userInfo.Add("bool", true);

            // Provide the notification title.
            content.title = "Demo Notification";

            // You can optionally provide the notification subtitle, which is visible on iOS only.
            content.subtitle = "Demo Subtitle";

            // Provide the notification message.
            content.body = $"This is a demo notification {content.userInfo["string"]}";

            // You can optionally assign this notification to a category using the category ID.
            // If you don't specify any category, the default one will be used.
            // Note that it's recommended to use the category ID constants from the EM_NotificationsConstants class
            // if it has been generated before. In this example, UserCategory_notification_category_test is the
            // generated constant of the category ID "notification.category.test".
            content.categoryId = DEFAULT_CATEGORY_ID;

            // If you want to use default small icon and large icon (on Android),
            // don't set the smallIcon and largeIcon fields of the content.
            // If you want to use custom icons instead, simply specify their names here (without file extensions).
            ///content.smallIcon = "YOUR_CUSTOM_SMALL_ICON";
            ///content.largeIcon = "YOUR_CUSTOM_LARGE_ICON";

            return content;
        }


#endregion





    }
}