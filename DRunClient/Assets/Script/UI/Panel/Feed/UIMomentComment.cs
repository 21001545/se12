using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
    public class UIMomentComment : UISingletonPanel<UIMomentComment>, IEnhancedScrollerDelegate
    {
        public class CommentType
        {
            public static readonly int comment = 0;
            public static readonly int view_replies = 1;
            public static readonly int content = 2;
        }

        public class CommentDataBase
        {
            // 뭐 없네. is-as 보단 type 체킹이 더 빠르겟지
            public int messageType;

            // 데이터별로 높이가 다를 수 있군!
            public float height;

            // 높이 계산이 끝낫나? 최적화 때문에라도 중복해서 하지 말자.
            public bool alreadyCalculate = true;

            public ClientMomentComment momentComment;

            public ClientMoment moment;

            public CommentDataBase(int messageType, float height, bool alreadyCalculate)
            {
                this.messageType = messageType;
                this.height = height;
                this.alreadyCalculate = alreadyCalculate;
            }
        }

        public class CommentData : CommentDataBase
        {
            public CommentData(ClientMomentComment momentComment) : base(CommentType.comment, 60, false)
            {
                this.momentComment = momentComment;
            }
        }

        public class ViewRepliesData : CommentDataBase
        {
            public ViewRepliesData(ClientMomentComment momentComment) : base(CommentType.view_replies, 36, true)
            {
                this.momentComment = momentComment;
            }
        }
        public class ContentData: CommentDataBase
        {
            public ContentData(ClientMoment moment) : base(CommentType.content, 110, false)
            {
                this.moment = moment;
            }
        }

        [SerializeField]
        private EnhancedScroller _scroller;

        [SerializeField]
        private UIPhotoThumbnail _myThumbnail;

        [SerializeField]
        private TMP_InputField tf_comment;

        [SerializeField]
        private Button btn_send;

        [SerializeField]
        private GameObject go_loading;

        [SerializeField]
        private GameObject go_reply;

        [SerializeField]
        private TMP_Text txt_reply;

        [SerializeField]
        private UIMomentCommentCell _commentCellPrefab;

        [SerializeField]
        private UIMomentCommentViewRepliesCell _viewRepliesCellPrefab;

        [SerializeField]
        private UIMomentCommentContentCell _contentCellPrefab;

        private ClientNetwork Network => ClientMain.instance.getNetwork();
        private static RefStringCollection sc => GlobalRefDataContainer.getStringCollection();

        private ClientMoment _moment;
        private List<CommentDataBase> _scrollData = new List<CommentDataBase>();
        private List<ClientMomentComment> _commentList = new List<ClientMomentComment>();
        private ClientMomentComment _commentTarget;
        private ClientMomentComment _addedComment;

        private bool _validInputFIeld = false;
        private float _keyboardHeight = 0.0f; // 현재 키보드 높이
        private float _inputFieldHeight = 0.0f; // 인풋필드 높이
        private int _inputFieldLineCount = 0;

        public class InputMode
        {
            public const int add_comment = 1;
            public const int add_comment_to_target = 2;
            public const int modify_comment = 3;
        }

        private int _inputMode;

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            _scroller.Delegate = this;
        }

        //public void setup(ClientMoment moment)
        //{
        //	_moment = moment;
        //	_commentTarget = null;

        //	txt_comment_target.gameObject.SetActive(false);

        //	setInputMode(InputMode.add_comment, null);
        //}

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            base.open(param, transitionType, closeType);

            _moment = (ClientMoment)param.get("moment");

            _scrollData = new List<CommentDataBase>();
            _commentList = new List<ClientMomentComment>();

            _commentTarget = null;
            _addedComment = null;

            //txt_comment_target.gameObject.SetActive(false);

            setInputMode(InputMode.add_comment, null);
            tf_comment.ActivateInputField();
        }

        public override void update()
        {
            base.update();

            if (gameObject.activeSelf)
            {
                if (tf_comment.isFocused)
                {
                    // 812.0f -> height of canvas scaler 
                    var height = (TouchScreenKeyboardUtil.getInstance().getHeight() / Screen.height) * 812.0f;

                    // 32.0f 만큼 차이가 난다.
                    if (height - 32.0f > 0 && rt_pivot.sizeDelta.y != -(height - 32))
                    {
                        height = height - 32.0f;

                        if (_keyboardHeight != height)
                        {
                            _keyboardHeight = height;

                            var scrollPosition = 0.0f;
                            var prevNormalPosition = _scroller.NormalizedScrollPosition;
                            var prevScrollPosition = _scroller.ScrollPosition;
                            var prevScrollRectSize = _scroller.ScrollRectSize;

                            rt_inputField.anchoredPosition = new Vector2(0, _keyboardHeight);
                            rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -92.0f -(_keyboardHeight + _inputFieldHeight));

                            if (prevNormalPosition < 1.0f)
                            {
                                // 스크롤 유지.
                                scrollPosition = prevScrollPosition + (prevScrollRectSize - _scroller.ScrollRectSize);
                            }
                            else
                            {
                                // 무조건 아래로 스크롤
                                scrollPosition = _scroller.ScrollSize;
                            }
                            _scroller.SetScrollPositionImmediately(scrollPosition);
                        }
                    }
                }
                else if (rt_pivot.sizeDelta.y != -92.0f)
                {
                    var scrollPosition = 0.0f;
                    var prevNormalPosition = _scroller.NormalizedScrollPosition;
                    var prevScrollPosition = _scroller.ScrollPosition;
                    var prevScrollRectSize = _scroller.ScrollRectSize;

                    _keyboardHeight = 0.0f;

                    rt_inputField.anchoredPosition = new Vector2(0, 0);
                    rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -92.0f -(_keyboardHeight + _inputFieldHeight));

                    if (prevNormalPosition < 1.0f)
                    {
                        // 스크롤 유지.
                        scrollPosition = prevScrollPosition - (_scroller.ScrollRectSize - prevScrollRectSize);
                    }
                    else
                    {
                        // 무조건 아래로 스크롤
                        scrollPosition = _scroller.ScrollSize;
                    }

                    _scroller.SetScrollPositionImmediately(scrollPosition);
                }
            }
        }

        public override void onTransitionEvent(int type)
        {
            if (type == TransitionEventType.start_open)
            {
                setupUI();
                queryComment();
            }
        }

        private void setupUI()
        {
            _myThumbnail.setImageFromCDN(ClientMain.instance.getViewModel().Profile.Profile.getPicktureURL(GlobalConfig.fileserver_url));
        }
        //
        //
        //format.time.days_ago
        //format.time.months_ago
        //format.time.years_ago


        public static string formatTime(TimeSpan span)
        {
            if (span.TotalMinutes < 1.0f)
            {
                return sc.get("format.time.just_before", 0);
            }
            else if (span.TotalHours < 1.0f)
            {
                return sc.getFormat("format.time.minutes_ago", 0, (int)span.TotalMinutes);
            }
            else if (span.TotalDays < 1.0f)
            {
                return sc.getFormat("format.time.hours_ago", 0, (int)span.TotalHours);
            }
            else
            {
                return sc.getFormat("format.time.days_ago", 0, (int)span.TotalDays);
            }
        }

        public void insertSubComment(List<ClientMomentComment> comments, ClientMomentComment targetComment)
        {
            int index = 0;

            for (index = 0; index < _commentList.Count; ++index)
            {
                if (_commentList[index].sub_id == 1 && _commentList[index].slot_id == targetComment.slot_id)
                {
                    break;
                }
            }

            _commentList.InsertRange(index + 1, comments);

            reloadScroller();
        }

        private void queryComment()
        {
            go_loading.gameObject.SetActive(true);

            QueryMomentCommentProcessor processor = QueryMomentCommentProcessor.create(_moment, 0, 30);
            processor.run(result =>
            {
                go_loading.gameObject.SetActive(false);

                if (result.succeeded())
                {
                    _commentList = processor.getCommentList();
                    reloadScroller();

                    if (_addedComment != null)
                    {
                        // data index를 찾아야 하는데..
                        var index = 0;
                        foreach (var comment in _commentList)
                        {
                            if (comment.slot_id == _addedComment.slot_id && comment.sub_id == _addedComment.sub_id)
                            {
                                break;
                            }

                            ++index;
                        }

                        // 이거 내려가야할 것 같은데..?
                        _scroller.SetScrollPositionImmediately(_scroller.GetScrollPositionForDataIndex(index, 0));
                        _addedComment = null;
                    }
                }

            });
        }

        public void setInputMode(int mode, ClientMomentComment comment_target)
        {
            _inputMode = mode;
            _commentTarget = comment_target;

            go_reply.SetActive(_inputMode == InputMode.add_comment_to_target);

            if (_inputMode == InputMode.add_comment)
            {
                tf_comment.text = "";
            }
            else if (_inputMode == InputMode.add_comment_to_target)
            {
                
                txt_reply.text = sc.getFormat("feed.comment.target", 0, $"@{_commentTarget._profile.name}");

                tf_comment.text = $"<color=#298f95>@{_commentTarget._profile.name}</color> ";
                tf_comment.ActivateInputField();

                // 캐럿을 맨 뒤로 옮기자.
                tf_comment.caretPosition = tf_comment.textComponent.textInfo.characterCount - 1;


            }
            else if (_inputMode == InputMode.modify_comment)
            {
                tf_comment.text = _commentTarget.message;
                tf_comment.ActivateInputField();

                // 캐럿을 맨 뒤로 옮기자.
                tf_comment.caretPosition = tf_comment.textComponent.textInfo.characterCount -1;
            }
        }

        public void onClickReplyClose()
        {
            go_reply.SetActive(false);
        }

        public void onValueChangedInputField(string message)
        {
            if (message == "\n")
            {
                tf_comment.text = "";
                btn_send.interactable = false;
                return;
            }

            if (_commentTarget != null && message.EndsWith("</color>"))
            {
                setInputMode(InputMode.add_comment, null);
                return;
            }
            btn_send.interactable = message.Length > 0;
            _validInputFIeld = false;
        }

        [SerializeField]
        private RectTransform rt_inputField;

        [SerializeField]
        private RectTransform rt_pivot;

        IEnumerator UpdateCoroutine()
        {
            yield return new WaitForEndOfFrame();

            if (_inputFieldLineCount == tf_comment.textComponent.textInfo.lineCount)
                yield  break ;

            if (_inputFieldLineCount >= 4 && _inputFieldLineCount < tf_comment.textComponent.textInfo.lineCount)
                yield break;

            _inputFieldLineCount = tf_comment.textComponent.textInfo.lineCount;

            // 메시지의 길이에 따라 InputField의 높이를 조절해보자        
            var bounds = tf_comment.textComponent.textBounds;

            // 상하 패딩이 각각 12씩 24 px
            // 키보드 최소 높이는 44.0f
            // 4줄 기준 101.0f 네... 음.. 
            // 상단 상하 패딩이 8씩 16 px.
            var height = Mathf.Min(101.0f, Mathf.Max(44.0f, Mathf.Floor(bounds.size.y) + 24.0f)) + 16.0f;

            rt_inputField.sizeDelta = new Vector2(rt_inputField.sizeDelta.x, 32.0f + 40.0f + height);

            var scrollPosition = 0.0f;
            var prevNormalPosition = _scroller.NormalizedScrollPosition;
            var prevScrollPosition = _scroller.ScrollPosition;
            var prevScrollRectSize = _scroller.ScrollRectSize;

            _inputFieldHeight = height - 60.0f;
            rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -92.0f - (_keyboardHeight + _inputFieldHeight));

            if (prevNormalPosition < 1.0f)
            {
                // 스크롤 유지.
                scrollPosition = prevScrollPosition + (prevScrollRectSize - _scroller.ScrollRectSize);
            }
            else
            {
                // 무조건 아래로 스크롤
                scrollPosition = _scroller.ScrollSize;
            }
            _scroller.SetScrollPositionImmediately(scrollPosition);
        }
        public void LateUpdate()
        {
            if (_validInputFIeld)
                return;

            _validInputFIeld = true;

            StartCoroutine(UpdateCoroutine());

        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

        public void onClickSend()
        {
            if (tf_comment.text.Length == 0)
            {
                return;
            }

            tf_comment.DeactivateInputField();

            if (_inputMode == InputMode.modify_comment)
            {
                reqModifyComment();
            }
            else
            {
                reqAddComment();
            }
        }

        public void onClickResetInputMode()
        {
            setInputMode(InputMode.add_comment, null);
        }

        public void onClickWriterPhoto()
        {
            UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
            param.accountID = _moment.account_id;

            UIMomentComment.getInstance().close();
            UIProfile.getInstance().open(param);

            ClientMain.instance.getPanelNavigationStack().push(this, UIProfile.getInstance());
        }

        private void reqModifyComment()
        {
            btn_send.interactable = false;

            ClientMomentComment modify_comment = _commentTarget;
            string message = tf_comment.text;

            MapPacket req = Network.createReq(CSMessageID.Moment.ModifyCommentReq);
            req.put("moment_account_id", _moment.account_id);
            req.put("moment_id", _moment.id);
            req.put("message", message);
            req.put("slot_id", _commentTarget.slot_id);
            req.put("sub_id", _commentTarget.sub_id);

            _inputMode = InputMode.add_comment;
            _commentTarget = null;
            tf_comment.text = "";

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    modify_comment.message = message;
                    reloadScroller();
                }
            });
        }

        private void reqAddComment()
        {
            btn_send.interactable = false;

            MapPacket req = Network.createReq(CSMessageID.Moment.AddCommentReq);

            req.put("moment_account_id", _moment.account_id);
            req.put("moment_id", _moment.id);
            req.put("message", tf_comment.text);

            var newComment = new ClientMomentComment();
            if (_commentTarget != null)
            {
                req.put("slot_id", _commentTarget.slot_id);
                req.put("sub_id", 1);
                newComment.slot_id = _commentTarget.slot_id;
                newComment.sub_id = 1;
            }
            else
            {
                req.put("slot_id", 0);
                req.put("sub_id", 0);

                newComment.slot_id = 0;
                newComment.sub_id = 0;
            }

            _inputMode = InputMode.add_comment;

            newComment._isPosting = true;
            newComment.comment_account_id = ClientMain.instance.getNetwork().getAccountID();
            newComment.message = tf_comment.text;

            newComment._profile = ClientMain.instance.getViewModel().Profile.Profile;
            newComment._moment = _moment;

            // 알맞게 끼워놔보자
            if (_commentTarget == null )
            {
                _commentList.Add(newComment);
            }
            else
            {
                for (int i = 0; i < _commentList.Count; i++)
                {
                    if (_commentList[i] == _commentTarget)
                    {
                        _commentList.Insert(i + 1, newComment);
                        break;
                    }
                }
            }

            if (_commentTarget != null)
                _commentTarget.sub_count++;

            setInputMode(InputMode.add_comment, null);

            reloadScroller();

            _scroller.ScrollPosition = _scroller.ScrollSize;

            Network.call(req, ack =>
            {
                if (ack.getResult() != ResultCode.ok)
                {
                    return;
                }
                else
                {
                    ClientMoment moment = (ClientMoment)ack.get("moment");
                    _moment.comment_count = moment.comment_count;

                    var comment = (ClientMomentComment)ack.get("comment");
                    newComment.slot_id = comment.slot_id;
                    newComment.sub_id = comment.sub_id;
                    newComment.comment_account_id = comment.comment_account_id;
                    newComment.status = comment.status;
                    newComment.message = comment.message;
                    newComment.like_count = comment.like_count;
                    newComment.sub_count = comment.sub_count;
                    newComment.update_time = comment.update_time;
                    newComment._isPosting = false;

                    _scroller.ReloadData();
                    _scroller.ScrollPosition = _scroller.ScrollSize;
                }
            });
        }

        public void deleteComment(UIMomentCommentCell cellView)
        {
            ClientMomentComment targetComment = cellView.getComment();

            MapPacket req = Network.createReq(CSMessageID.Moment.DeleteCommandReq);
            req.put("moment_account_id", _moment.account_id);
            req.put("moment_id", _moment.id);
            req.put("slot_id", targetComment.slot_id);
            req.put("sub_id", targetComment.sub_id);

            bool isDeleteTopLevel = targetComment.sub_id == 1;

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    ClientMoment moment = (ClientMoment)ack.get("moment");
                    int new_comment_count = moment.comment_count;               // 갱신된 모먼트 전체 댓글 수

                    _scrollData.Clear();

                    // sub 만 지움
                    if (isDeleteTopLevel == false)
                    {
                        ClientMomentComment top_level_comment = (ClientMomentComment)ack.get("comment");
                        int new_sub_comment_count = top_level_comment.sub_count;    // 상위 코멘트에 기록된 하위 댓글 수

                        _commentList.Remove(targetComment);
                        reloadScroller();
                    }
                    // 하위 sub 목록 다 지움
                    else
                    {
                        List<ClientMomentComment> deleteList = new List<ClientMomentComment>();
                        foreach (ClientMomentComment comment in _commentList)
                        {
                            if (comment.slot_id == targetComment.slot_id)
                            {
                                deleteList.Add(comment);
                            }
                        }

                        UIToastNotification.spawn(sc.getFormat("moment.comment.delete", 0, 1 + targetComment.sub_count), ColorChart.secondary_300);

                        foreach (ClientMomentComment comment in deleteList)
                        {
                            _commentList.Remove(comment);
                        }

                        reloadScroller();
                    }

                    //comment.status = ClientMomentComment.Status.deleted;
                    //cellView.setup(comment);
                }
            });
        }

        public void likeComment(UIMomentCommentCell cellView)
        {
            ClientMomentComment comment = cellView.getComment();

            int msg_id = comment._isLiked ? CSMessageID.Moment.CommentUnlikeReq : CSMessageID.Moment.CommentLikeReq;
            bool result_like = !comment._isLiked;

            MapPacket req = Network.createReq(msg_id);
            req.put("moment_account_id", _moment.account_id);
            req.put("moment_id", _moment.id);
            req.put("slot_id", comment.slot_id);
            req.put("sub_id", comment.sub_id);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    ClientMomentComment result_comment = (ClientMomentComment)ack.get("comment");
                    comment.like_count = result_comment.like_count;
                    comment._isLiked = result_like;

                    cellView.setup(comment);
                }
            });
        }

        private void reloadScrollerData(bool keepScrollPosition)
        {
            float prev_pos = _scroller.ScrollPosition;
            reloadScroller();

            if (keepScrollPosition)
            {
                _scroller.ScrollPosition = prev_pos;
            }
        }

        public void onClickViewReplies(ClientMomentComment comment)
        {
            // 쵀대 3개만 가져오자.
            int requestCount = Math.Min(comment.sub_count - comment._loaded_sub_count, 3);
            var processor = QueryMomentCommentProcessor.create(comment, (comment.sub_count - comment._loaded_sub_count - requestCount), requestCount);
            processor.run(result =>
                   {
                       if (result.succeeded())
                       {
                           var list = processor.getCommentList();
                           comment._loaded_sub_count += list.Count;
                           insertSubComment(list, comment);
                       }
                   });
        }

        #region scroller delgate

        private bool _calculateCellSize = false;
        private void reloadScroller()
        {
            var rectTransform = _scroller.GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;

            bool needTotalCalculate = _scrollData.Count == 0;

            // _scrollData와 _commentList를 동기화 해야한다.

            // _scrollData에는 
            // commentData - (viewRepliesData가 있을수도?) - commentData 순으로 되어 있으므로.. 순회하면서 비어있는걸 갱신하자
            int dataIndex = 0;
            int prevSlotID = 1;
            for (int i = 0; i < _commentList.Count; ++i)
            {
                var comment = _commentList[i];
                var id = comment.slot_id;

                for (; dataIndex < _scrollData.Count; ++dataIndex)
                {
                    if (_scrollData[dataIndex].messageType == CommentType.comment)
                        break;
                }

                ClientMomentComment data = dataIndex < _scrollData.Count ? _scrollData[dataIndex].momentComment : null;

                if (data != null )
                {
                    // 같은 그룹 코멘트
                    if (data.slot_id == id )
                    {
                        if (data.sub_id == comment.sub_id)
                        {
                            if (comment._loaded_sub_count == comment.sub_count)
                            {
                                if (dataIndex + 1 < _scrollData.Count && _scrollData[dataIndex + 1].messageType == CommentType.view_replies)
                                {
                                    // 요거 지워야 하는데,
                                    _scrollData.RemoveAt(dataIndex + 1);
                                }
                            }
                            ++dataIndex;
                        }
                        else
                        {
                            // 대댓글
                            _scrollData.Insert(dataIndex, new CommentData(comment));
                            ++dataIndex;
                        }
                    }
                    else
                    {
                        if ( prevSlotID == id )
                        {
                            _scrollData.Insert(dataIndex, new CommentData(comment));
                            ++dataIndex;
                        }
                    }

                }
                else
                {
                    _scrollData.Add(new CommentData(comment));
                    ++dataIndex;

                    if (comment.sub_count > 0)
                    {
                        _scrollData.Add(new ViewRepliesData(comment));
                        ++dataIndex;
                    }
                }
                prevSlotID = comment.slot_id;
            }

            if ( _scrollData.Count == 0 || _scrollData[0].messageType != CommentType.content)
            {
                _scrollData.Insert(0, new ContentData(_moment));
            }
            var scrollPosition = 0.0f;
            var prevNormalPosition = _scroller.NormalizedScrollPosition;
            var prevScrollPosition = _scroller.ScrollPosition;
            var prevScrollRectSize = _scroller.ScrollRectSize;

            // 갱신.
            if (needTotalCalculate )
            {
                _scroller.ScrollPosition = 0;
                rectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);
            }

            _calculateCellSize = true;
            _scroller.ReloadData();

            if (needTotalCalculate)
                rectTransform.sizeDelta = size;

            _calculateCellSize = false;
            _scroller.ReloadData();

            if (needTotalCalculate)
            {
                _scroller.Container.anchoredPosition = Vector2.zero;
            }

            scrollPosition = prevScrollPosition + (prevScrollRectSize - _scroller.ScrollRectSize);
            _scroller.SetScrollPositionImmediately(scrollPosition);
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            CommentDataBase data = _scrollData[dataIndex];

            if (data.messageType == CommentType.view_replies)
            {
                UIMomentCommentViewRepliesCell cellView = scroller.GetCellView(_viewRepliesCellPrefab) as UIMomentCommentViewRepliesCell;

                if (_calculateCellSize == false)
                    cellView.setup(data.momentComment, onClickViewReplies);

                return cellView;
            }
            else if ( data.messageType == CommentType.content)
            {
                UIMomentCommentContentCell cellView = scroller.GetCellView(_contentCellPrefab) as UIMomentCommentContentCell;

                if (_calculateCellSize == true || data.alreadyCalculate == false)
                {
                    cellView.setup(((ContentData)data).moment);
                    data.height = cellView.getComponentSize().y;
                    data.alreadyCalculate = true;
                }

                return cellView;
                
            }
            else if (data.messageType == CommentType.comment)
            {
                UIMomentCommentCell cellView = scroller.GetCellView(_commentCellPrefab) as UIMomentCommentCell;
                if (_calculateCellSize == false || data.alreadyCalculate == false)
                {
                    cellView.setup(data.momentComment);
                    data.height = cellView.getComponentSize().y;
                    data.alreadyCalculate = true;
                }
                return cellView;
            }

            return null;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            CommentDataBase data = _scrollData[dataIndex];
            return data.height;
            //         if ( comment.status == ClientMomentComment.Status.deleted)
            //{
            //	return 30;
            //}
            //else
            //{
            //	if (comment._loaded_sub_count == comment.sub_count)
            //             {
            //		return 66;
            //             }
            //	else
            //             {
            //		return 86;
            //	}
            //}
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _scrollData.Count;
        }


        #endregion
    }
}

