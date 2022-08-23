using System;
using System.Collections;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GoogleGame;


    public enum MyColorType
{
    Red,
    Yellow,
    Blue,
    Orange,
    Purple,
    Green
}


namespace GoogleGame
{
    /// <summary>
    /// 각 카드에 붙어있는 스크립트
    /// </summary>
    public class NormalCardInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        Vector3 origPos;
        Image _image;
        Text _numText;

        [SerializeField] CardGenerator cardGenerator;
        [Header("- 드래그 카드")]
        [SerializeField] NormalCardIDaggable draggableItem;
        [Header("- 이 카드의 정보")]
        [SerializeField] Text numText;
        [Space]
        [SerializeField] NormalCard _DeepCopyCard;
        private NormalCard _OriginCard;

        /// <summary>
        /// 얘가 소환된 위치
        /// </summary>
        private int _posIndex;

        /// <summary>
        /// 설정된 헥스 코드 반환
        /// </summary>
        /// <param name="colorType"></param>
        /// <returns></returns>
        private Color MyColor(MyColorType colorType)
        {
            switch (colorType)
            {
                case MyColorType.Red:
                    return new Color(244 / 255f, 67 / 255f, 54 / 255f);
                case MyColorType.Yellow:
                    return new Color(253 / 255f, 216 / 255f, 53 / 255f);
                case MyColorType.Blue:
                    return new Color(33 / 255f, 150 / 255f, 243 / 255f);
                case MyColorType.Orange:
                    return new Color(255 / 255f, 152 / 255f, 0);
                case MyColorType.Purple:
                    return new Color(156 / 255f, 39 / 255f, 176 / 255f);
                case MyColorType.Green:
                    return new Color(76 / 255f, 175 / 255f, 80 / 255f);
                default:
                    return Color.white;
            }
        }




        /// <summary>
        /// 카드 pop 할때 기본 정보 생성
        /// </summary>
        /// <param name="card">NormalCard 넘겨주고</param>
        /// <param name="posIndex">소환된 위치 인덱스</param>
        internal void SetThisCard(NormalCard card, int posIndex)
        {
            _OriginCard = card;
            /// 수정하는 카드는 딥 카피
            _DeepCopyCard = card.DeepCopy();

            /// 기본 자식 세팅
            _numText = GetComponentInChildren<Text>();
            _numText.enabled = true;
            _image = GetComponent<Image>();
            _image.enabled = true;

            /// 위치 트랜스
            origPos = transform.position;
            numText.text = _DeepCopyCard.numType;

            /// 얘가 소환된 위치
            _posIndex = posIndex;

            switch (_DeepCopyCard.colorType)
            {
                case "Red":
                    numText.color = MyColor(MyColorType.Red);
                    break;
                case "Yellow":
                    numText.color = MyColor(MyColorType.Yellow);
                    break;
                case "Blue":
                    numText.color = MyColor(MyColorType.Blue);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 카드 터치 유지 중 일때
        /// </summary>
        public void DoOnSelected()
        {
            transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// 터치 손 뗏을 때
        /// </summary>
        public void DoFingerOn()
        {
            transform.localScale = Vector3.one;
        }


        /// <summary>
        /// Lean Drop 에 붙어있다. -> 드롭을 당할때
        /// </summary>
        public void DropCardTest()
        {
            //if (thisCard.colorNum != 5) return;

            GetComponentInChildren<TextMeshPro>().text = "11";
            GetComponentInChildren<TextMeshPro>().color = Color.magenta;
            _DeepCopyCard.colorNum = 5;
        }

        /// <summary>
        /// 드롭을 할 때
        /// </summary>
        public void DoDropCardTest()
        {
            gameObject.SetActive(false);
        }


        #region <Ipoint Interface>


        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }


        public void OnBeginDrag(PointerEventData eventData)
        {
            /// 악마 카드는 드래그 금지
            if (_DeepCopyCard.numType == "Devil")
            {
                return;
            }

            /// 노말카드 드래그 애니메이션
            //transform.localScale = Vector3.one * 1.2f;
            //transform.SetAsLastSibling();
            /// 바닥카드 그림 없애기
            OnDropEvent(false);
            /// 드래그하는 카드 세팅
            draggableItem.InitDaggable(_OriginCard, _DeepCopyCard, _posIndex);
            var tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tmpPos.z = 10;
            draggableItem.transform.position = tmpPos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            //transform.position = eventData.position;
            var tmpPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            tmpPos.z = 10;
            draggableItem.transform.position = tmpPos;
        }

        /// <summary>
        /// 노말 카드 드래그를 뗄 때 행동처리
        /// 1. 스페셜 카드에 드랍할 때
        ///   ㄴ 현재 오브젝트 없어지고, 해당 데이터 넘겨줌
        /// 2. 트래쉬 존에 드랍할 때
        ///   ㄴ 현재 오브젝트 없어지고, SP 증가시켜줌.
        /// 3. 1,2 를 만족하지 않을 때
        ///   ㄴ 드래그를 시작한 위치로 보내야 함.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnEndDrag(PointerEventData eventData)
        {
            /// 드래그하는 이미지 감춰주기 
            draggableItem.DestroyDaggable();

            /// 현재 노멀 카드 이미지 보여주기
            transform.position = origPos;
            ///
            OnDropEvent(true);
        }

        /// <summary>
        /// 이 카드를 버리기나 스페셜 카드에 떨뒀을 때나 카드 터졌을 때
        /// </summary>
        /// <param name="_bool"></param>
        void OnDropEvent(bool _bool)
        {
            _image.enabled = _bool;
            _numText.enabled = _bool;
        }


        /// <summary>
        /// 이 카드 위에 draggableItem를 드랍하면?
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrop(PointerEventData eventData)
        {
            NormalCard daggedCard = draggableItem.GetDraggingInfo();

            /// 임시 컬러 저장
            ObscuredString tmpColor = string.Empty;

            /// _DeepCopyCard 는 밑에 깔린 카드
            /// daggedCard 는 위에 덮는 카드
            /// 색깔 합치기
            switch (_DeepCopyCard.colorType)
            {
                case "Red":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Red")
                    {
                        //Debug.LogError("Red + Red");
                        tmpColor = "Red";
                    }
                    else
                    {
                        ///겹치는게 다른 색이라면? 혼합 판별
                        if (daggedCard.colorType == "Yellow")
                        {
                            //Debug.LogError("Red + Yellow");
                            tmpColor = "Orange";
                        }
                        else if (daggedCard.colorType == "Blue")
                        {
                            //Debug.LogError("Red + Blue");
                            tmpColor = "Purple";
                        }
                        else
                        {
                            //Debug.LogError("이미 합성된거라 못 합치는데용");
                            return;
                        }
                    }

                    break;
                case "Yellow":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Yellow")
                    {
                        //Debug.LogError("Yellow + Yellow");
                        tmpColor = "Yellow";
                    }
                    else
                    {
                        ///겹치는게 다른 색이라면?
                        if (daggedCard.colorType == "Red")
                        {
                            //Debug.LogError("Yellow + Red");
                            tmpColor = "Orange";
                        }
                        else if (daggedCard.colorType == "Blue")
                        {
                            //Debug.LogError("Yellow + Blue");
                            tmpColor = "Green";
                        }
                        else
                        {
                            //Debug.LogError("이미 합성된거라 못 합치는데용");
                            return;
                        }
                    }

                    break;
                case "Blue":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Blue")
                    {
                        //Debug.LogError("Blue + Blue");
                        tmpColor = "Blue";
                    }
                    else
                    {
                        ///겹치는게 다른 색이라면?
                        if (daggedCard.colorType == "Red")
                        {
                            //Debug.LogError("Blue + Red");
                            tmpColor = "Purple";
                        }
                        else if (daggedCard.colorType == "Yellow")
                        {
                            //Debug.LogError("Blue + Yellow");
                            tmpColor = "Green";
                        }
                        else
                        {
                            //Debug.LogError("이미 합성된거라 못 합치는데용");
                            return;
                        }
                    }

                    break;


                case "Orange":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Orange")
                    {
                        //Debug.LogError("Orange + Orange");
                        tmpColor = "Orange";
                    }
                    else
                    {
                        //Debug.LogError("이미 합성된거라 못 합치는데용");
                        return;
                    }
                    break;

                case "Purple":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Purple")
                    {
                        //Debug.LogError("Purple + Purple");
                        tmpColor = "Purple";
                    }
                    else
                    {
                        //Debug.LogError("이미 합성된거라 못 합치는데용");
                        return;
                    }
                    break;

                case "Green":
                    /// 같은 색이라면 합치기
                    if (daggedCard.colorType == "Green")
                    {
                        //Debug.LogError("Green + Green");
                        tmpColor = "Green";
                    }
                    else
                    {
                        //Debug.LogError("이미 합성된거라 못 합치는데용");
                        return;
                    }
                    break;

                default:
                    /// colorType = null 일 경우 리턴
                    return;
            }



            /// 숫자 합성 
            /// True = 정상적으로 2개가 합성됨
            /// False = 21초과되서 카드 2개 터짐
            if (AddTwoCardNumtype(_DeepCopyCard.numType, daggedCard.numType, out ObscuredInt result))
            {
                /// result J = -1, Q = -2, K = -3
                /// 더블 와일드 카드 처리
                if (result == -1)
                {
                    _DeepCopyCard.numType = "JJ";
                }
                else if (result == -2)
                {
                    _DeepCopyCard.numType = "QQ";
                }
                else if (result == -3)
                {
                    _DeepCopyCard.numType = "KK";
                }
                else
                {
                    /// 안 터졌을 경우
                    _DeepCopyCard.numType = result.ToString();
                }

                /// 카드 텍스트 갱신
                _numText.text = _DeepCopyCard.numType;

                /// 카드 UI 업데이트
                UpdateCardInfo(tmpColor);
            }
            else
            {
                /// 합쳐지는 조건이 아니라서 카드 2개 다 살아있음
                if (result < 0)
                {
                    /// 아무 일도 없었다
                    return;
                }

                /// 21 초과일 경우
                //Debug.LogError("카드 터졌는데용");
                /// 합성 성공이면 카드 없애줌
                /// 바닥카드 그림 없애기
                OnDropEvent(false);
                /// 드래그 카드 없애기
                cardGenerator.RemoveOwned3CardPool(draggableItem.GetRemovedCard(out int posIndex), posIndex);
                /// 현재카드도 없애기
                cardGenerator.RemoveOwned3CardPool(_OriginCard, _posIndex);
            }

        }


        /// <summary>
        /// 두 개 더해서 <string>카드 넘버 숫자 반환
        /// </summary>
        /// <param name="card_A"></param>
        /// <param name="card_B"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private ObscuredBool AddTwoCardNumtype(string card_A, string card_B, out ObscuredInt result)
        {
            int A_Value, B_Value;

            /// Ace 카드는 1 혹은 11로 한다.
            /// 카드 다시 뽑았을 때 21이 넘으면 1로 계산.
            if (card_A.Contains("A"))
            {
                A_Value = 11;
            }
            else
            {
                /// J,Q,K 처리
                A_Value = TransNumber_Not_Ace(card_A);
            }

            /// Ace 카드는 1 혹은 11로 한다.
            if (card_B.Contains("A"))
            {
                B_Value = 11;
            }
            else
            {
                /// J,Q,K 처리
                B_Value = TransNumber_Not_Ace(card_B);
            }

            /**
             *   J,Q,K 두 개 포갰을 경우만 발동하게
             */
            if (A_Value == -1 && B_Value == -1)
            {
                result = -1;
                return true;
            }
            else if (A_Value == -2 && B_Value == -2)
            {
                result = -2;
                return true;
            }
            else if (A_Value == -3 && B_Value == -3)
            {
                result = -3;
                return true;
            }


            /// 정수형 계산 하고 블랙잭 숫자 리턴
            result = A_Value + B_Value;

            /// A를 제외한 다른 영어 2개 합성했을 때?
            if (result < 0)
            {
                return false;
            }
            else if (A_Value < 0 || B_Value < 0)
            {
                result = -100;
                return false;
            }

            /// 둘다 Ace 일때는?
            if (A_Value == 11 && B_Value == 11)
            {
                result = 12;
                return true;
            }


            /// 블랙잭 = 21 이하라면 안터짐
            if (result <= 21)
            {
                return true;
            }
            else
            {
                /// 21 넘어서 버스트지만? A가 있어서? 1로 바꿔?
                if (A_Value == 11 || B_Value == 11)
                {
                    result -= 10;
                    return true;
                }
                else
                {
                    return false;
                }

            }

        }

        /// <summary>
        /// J,Q,K 받아서 숫자로 리턴 (Ace는 앞에서 거른다.)
        /// </summary>
        /// <param name="inpuStr"></param>
        /// <returns></returns>
        private ObscuredInt TransNumber_Not_Ace(string inpuStr)
        {
            /// 파서에서 정수로 떨어진다면? iData 는 J,Q,K
            if (int.TryParse(inpuStr, out int iData))
            {
                return iData;
            }
            else
            {
                /// 영문자라면? J,Q,K
                //return 10;
                if (inpuStr.Contains("J")) return -1;           // J
                else if (inpuStr.Contains("Q")) return -2;      // Q
                else if (inpuStr.Contains("K")) return -3;      // K
                else
                {
                    /// 더블 JJ, QQ, KK 라면?
                    return -100;
                }
            }

        }


        /// <summary>
        /// 색상 합성이면 글자색 바꿔주기
        /// </summary>
        /// <param name="thisColor"></param>
        private void UpdateCardInfo(string thisColor)
        {
            _DeepCopyCard.colorType = thisColor;

            switch (thisColor)
            {
                case "Orange":
                    numText.color = MyColor(MyColorType.Orange);
                    break;
                case "Purple":
                    numText.color = MyColor(MyColorType.Purple);
                    break;
                case "Green":
                    numText.color = MyColor(MyColorType.Green);
                    break;

                default:
                    break;
            }

            /// 합성 성공이면 드래그 카드 없애줌
            cardGenerator.RemoveOwned3CardPool(draggableItem.GetRemovedCard(out int posIndex), posIndex);

        }



        #endregion


    }
}