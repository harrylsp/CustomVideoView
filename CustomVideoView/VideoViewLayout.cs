using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.Media;
using Android.Graphics;
using System.Timers;

namespace CustomVideoView
{
    public class VideoViewLayout : FrameLayout, View.IOnClickListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnInfoListener, MediaPlayer.IOnPreparedListener, SeekBar.IOnSeekBarChangeListener
    {
        #region 控件
        /// <summary>
        /// 布局控件
        /// </summary>
        FrameLayout frameLayout_Video;
        /// <summary>
        /// 视频进度条控制栏
        /// </summary>
        LinearLayout linearLayout_VideoProgress;
        /// <summary>
        /// 视频对象
        /// </summary>
        private ExVideoView videoView;
        /// <summary>
        /// 视频第一帧缩略图
        /// </summary>
        private ImageView imageView_VideoThumbnail;
        /// <summary>
        /// 视频缓冲效果
        /// </summary>
        private ProgressBar progressBar_VideoProgressBar;
        /// <summary>
        /// 视频播放按钮
        /// </summary>
        private ImageView imageView_VideoPlay;
        /// <summary>
        /// 视频暂停按钮
        /// </summary>
        private ImageView imageView_VideoPause;
        /// <summary>
        /// 显示当前播放进度
        /// </summary>
        private TextView textView_VideoCurrentTime;
        /// <summary>
        /// 显示总播放时长
        /// </summary>
        private TextView textView_VideoTotalTime;
        /// <summary>
        /// 进度条
        /// </summary>
        private SeekBar seekBar_VideoSeekBar;
        /// <summary>
        /// 当前进度条值(当前播放时长)
        /// </summary>
        private int seekBar_VideoCurrentPoint;
        /// <summary>
        /// 进度条总数值(播放时长)
        /// </summary>
        private int seekBar_VideoTotal = 0;
        /// <summary>
        /// 是否继续播放
        /// </summary>
        private bool continuePlay = false;
        /// <summary>
        /// 视频网络地址
        /// </summary>
        private string url = "https://tbm.alicdn.com/4avTDEcUOjPu5HXisqp/GXle5GDB7ILjZ5T9n9q%40%40hd.mp4";
        #endregion

        #region 委托
        /// <summary>
        /// 更新视频播放进度
        /// </summary>
        public Action UpdateVideoCurrentProgress;
        /// <summary>
        /// 隐藏视频 暂停、播放、进度条
        /// </summary>
        public Action HideVideoStatusBar;
        #endregion

        VideoViewHandler handler;

        /// <summary>
        /// 是否加载完视频
        /// </summary>
        private bool isLoadVideoComplete = false;
        /// <summary>
        /// 记录视频范围最后一次点击的时间点，只执行最后一次操作所产生的定时器
        /// 为了避免在定时器等待过程中，触发多个定时器时而执行多次操作
        /// </summary>
        private DateTime lastClickTime;
        /// <summary>
        /// 是否第一次开始播放视频
        /// </summary>
        private bool firstStartVideo = false;

        #region 构造函数
        public VideoViewLayout(Context context) : this(context, null)
        {
            //this(context, null);
        }

        public VideoViewLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            // 导入布局  
            LayoutInflater.From(context).Inflate(Resource.Layout.VideoViewLayout, this, true);
            // 初始化布局
            InitView();

        }
        #endregion


        #region 自定义函数
        /// <summary>
        /// 初始化控件
        /// </summary>
        public void InitView()
        {
            frameLayout_Video = FindViewById<FrameLayout>(Resource.Id.VideoView_FrameLayout_video);
            linearLayout_VideoProgress = FindViewById<LinearLayout>(Resource.Id.VideoView_Layout_VideoProgress);
            videoView = FindViewById<ExVideoView>(Resource.Id.VideoView_VideoView_video);
            imageView_VideoThumbnail = FindViewById<ImageView>(Resource.Id.VideoView_ImageView_Thumbnail);
            progressBar_VideoProgressBar = FindViewById<ProgressBar>(Resource.Id.VideoView_ProgressBar_PB);
            imageView_VideoPlay = FindViewById<ImageView>(Resource.Id.VideoView_ImageView_Play);
            imageView_VideoPause = FindViewById<ImageView>(Resource.Id.VideoView_ImageView_Pause);
            textView_VideoCurrentTime = FindViewById<TextView>(Resource.Id.VideoView_TextView_VideoCurrentTime);
            seekBar_VideoSeekBar = FindViewById<SeekBar>(Resource.Id.VideoView_SeekBar_SB);
            textView_VideoTotalTime = FindViewById<TextView>(Resource.Id.VideoView_TextView_VideoTotalTime);
            
            InitEvent();

            InitData();
        }

        /// <summary>
        /// 初始化事件
        /// </summary>
        private void InitEvent()
        {
            handler = new VideoViewHandler(this);

            // 视频播放完监听
            videoView.SetOnCompletionListener(this);
            // 注册事件监听器
            videoView.SetOnInfoListener(this);
            // 注册准备播放监听器
            videoView.SetOnPreparedListener(this);

            // 视频播放进度条
            seekBar_VideoSeekBar.SetOnSeekBarChangeListener(this);

            UpdateVideoCurrentProgress = () =>
            {
                try
                {
                    if (videoView != null)
                    {
                        seekBar_VideoCurrentPoint = videoView.CurrentPosition / 1000;

                        textView_VideoCurrentTime.Text = (seekBar_VideoCurrentPoint / 60).ToString("00") + " : " + (seekBar_VideoCurrentPoint % 60).ToString("00");

                        seekBar_VideoSeekBar.Progress = seekBar_VideoCurrentPoint;
                    }
                }
                catch (Exception)
                {
                    continuePlay = false;
                }


            };

            HideVideoStatusBar = () =>
            {
                imageView_VideoPlay.Visibility = ViewStates.Gone;
                imageView_VideoPause.Visibility = ViewStates.Gone;
                linearLayout_VideoProgress.Visibility = ViewStates.Gone;
            };

            frameLayout_Video.SetOnClickListener(this);
            imageView_VideoPlay.SetOnClickListener(this);
            imageView_VideoPause.SetOnClickListener(this);

        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData()
        {
            // 第一帧图片
            Bitmap bitmap = null;
            // 获取视频第一帧
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();

            try
            {
                Android.Net.Uri uri = Android.Net.Uri.Parse(url);

                if (Build.VERSION.SdkInt >= Build.VERSION_CODES.IceCreamSandwich)
                {
                    retriever.SetDataSource(uri.ToString(), new Dictionary<string, string>());
                }
                else
                {
                    retriever.SetDataSource(uri.ToString());
                }

                // 获取第一帧图片
                bitmap = retriever.GetFrameAtTime(0, MediaMetadataRetriever.OptionClosest);
                imageView_VideoThumbnail.SetImageBitmap(bitmap);

                progressBar_VideoProgressBar.Visibility = ViewStates.Gone;
                imageView_VideoPlay.Visibility = ViewStates.Visible;
                imageView_VideoPause.Visibility = ViewStates.Gone;

                // 进度条
                seekBar_VideoTotal = Convert.ToInt32(retriever.ExtractMetadata((int)MetadataKey.Duration)) / 1000;
                seekBar_VideoSeekBar.Max = Convert.ToInt32(retriever.ExtractMetadata((int)MetadataKey.Duration)) / 1000;
                seekBar_VideoSeekBar.Enabled = true;

                textView_VideoTotalTime.Text = (seekBar_VideoTotal / 60).ToString("00") + " : " + (seekBar_VideoTotal % 60).ToString("00");

                retriever.Release();

            }
            catch (Exception)
            {
                retriever.Release();
            }
        }

        /// <summary>
        /// 视频播放进度跟踪
        /// </summary>
        public void PlayVdieo()
        {
            Java.Lang.Runnable run = new Java.Lang.Runnable(() =>
            {
                while (continuePlay)
                {
                    Java.Lang.Thread.Sleep(1000);

                    Message message = new Message();
                    message.What = 1;

                    //发送信息给handler
                    handler.SendMessage(message);
                }
            });
            new Java.Lang.Thread(run).Start();
        }

        /// <summary>
        /// 线程更新操作
        /// </summary>
        private void NotifyData()
        {
            UpdateVideoCurrentProgress?.Invoke();
        }

        /// <summary>
        /// 视频区域点击时
        /// 视频辅助控件 的显示与隐藏(暂停 播放 进度条 全屏)
        /// </summary>
        public void ShowVideoStatusBar(bool isFirstStart = false)
        {
            if (!isLoadVideoComplete && !isFirstStart)
            {
                return;
            }

            // 视频播放中
            if (videoView.IsPlaying || isFirstStart)
            {
                imageView_VideoPause.Visibility = ViewStates.Visible;
                imageView_VideoPlay.Visibility = ViewStates.Gone;

            }
            else
            {
                // 视频暂停中
                imageView_VideoPause.Visibility = ViewStates.Gone;
                imageView_VideoPlay.Visibility = ViewStates.Visible;
            }

            Timer timer = new Timer(1500);
            timer.Elapsed += new ElapsedEventHandler(TimeEventHandle);
            timer.AutoReset = false; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
            timer.Enabled = true;
            timer.Start();

            // 最后点击时间
            lastClickTime = DateTime.Now.AddSeconds(1.5);

            linearLayout_VideoProgress.Visibility = ViewStates.Visible;
            
        }

        private void TimeEventHandle(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 只触发最新的产生的定时器
            if (lastClickTime.ToString("yyyyMMddHHmmss") == e.SignalTime.ToString("yyyyMMddHHmmss"))
            {
                Message msg = new Message();
                msg.What = 2;
                handler.SendMessage(msg);
            }

        }
        #endregion


        #region 视频相关接口函数实现
        /// <summary>
        /// 注册一个在媒体资源播放完毕之后回调的播放事件
        /// </summary>
        /// <param name="mp"></param>
        public void OnCompletion(MediaPlayer mp)
        {
            //throw new NotImplementedException();

            if (videoView != null)
            {
                // 重新播放视频
                videoView.Start();
            }
        }

        /// <summary>
        ///  注册一个当有信息 或者 警告出现就会回调的监听器
        /// </summary>
        /// <param name="mp"></param>
        /// <param name="what"></param>
        /// <param name="extra"></param>
        /// <returns></returns>
        public bool OnInfo(MediaPlayer mp, [GeneratedEnum] MediaInfo what, int extra)
        {
            //throw new NotImplementedException();
            if (what == MediaInfo.BufferingStart)
            {
                progressBar_VideoProgressBar.Visibility = ViewStates.Visible;
            }
            else
            {
                progressBar_VideoProgressBar.Visibility = ViewStates.Gone;
            }
            return true;
        }

        /// <summary>
        /// 注册一个当媒体资源准备播放时回调的监听器
        /// </summary>
        /// <param name="mp"></param>
        public void OnPrepared(MediaPlayer mp)
        {
            //throw new NotImplementedException();
            progressBar_VideoProgressBar.Visibility = ViewStates.Gone;

            isLoadVideoComplete = true;

        }

        #endregion

        #region 视频进度条接口函数实现

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            //throw new NotImplementedException();

            seekBar_VideoCurrentPoint = progress;
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            //throw new NotImplementedException();
            if (videoView != null && videoView.IsPlaying)
            {
                videoView.Pause();
            }
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            //throw new NotImplementedException();
            if (this != null)
            {
                try
                {
                    videoView.SeekTo(seekBar.Progress * 1000);
                    videoView.Start();
                }
                catch (Exception ex)
                {

                    throw;
                }

            }
        }


        #endregion

        #region 事件响应函数
        public void OnClick(View v)
        {
            switch (v.Id)
            {
                case Resource.Id.VideoView_FrameLayout_video:
                    ShowVideoStatusBar();
                    break;
                case Resource.Id.VideoView_ImageView_Play:
                    StartVideo();
                    break;
                case Resource.Id.VideoView_ImageView_Pause:
                    PauseVideo();
                    break;

            }
        }

        /// <summary>
        /// 播放视频
        /// </summary>
        private void StartVideo()
        {
            firstStartVideo = false;

            if (isLoadVideoComplete)
            {
                videoView.Start();

                imageView_VideoPlay.Visibility = ViewStates.Gone;
                imageView_VideoPause.Visibility = ViewStates.Visible;
            }
            else
            {
                firstStartVideo = true;

                // 隐藏缩略图
                imageView_VideoThumbnail.Visibility = ViewStates.Gone;
                progressBar_VideoProgressBar.Visibility = ViewStates.Gone;
                imageView_VideoPlay.Visibility = ViewStates.Gone;
                imageView_VideoPause.Visibility = ViewStates.Visible;

                // 初始化视频进度控制控制器
                //mediaco = new MediaController(this);

                Android.Net.Uri videoUri = Android.Net.Uri.Parse(url);
                videoView.SetVideoURI(videoUri);

                videoView.Start();
                videoView.RequestFocus();
            }

            //Timer timer = new Timer(1500);
            //timer.Elapsed += new ElapsedEventHandler(TimeEventHandle);
            //timer.AutoReset = false; //每到指定时间Elapsed事件是触发一次（false），还是一直触发（true）
            //timer.Enabled = true;
            //timer.Start();

            ShowVideoStatusBar(firstStartVideo);

            continuePlay = true;

            // 视频播放进度更新
            PlayVdieo();
        }

        /// <summary>
        ///  暂停视频
        /// </summary>
        private void PauseVideo()
        {
            videoView.Pause();

            continuePlay = false;

            ShowVideoStatusBar();
        }

        #endregion


        #region 内部类
        public class VideoViewHandler : Handler
        {
            //private T t;
            //private Action notifyData;
            private VideoViewLayout videoViewLayout;

            public VideoViewHandler(VideoViewLayout videoViewLayout)
            {
                //this.t = t;
                //this.notifyData = notifyData;
                this.videoViewLayout = videoViewLayout;
            }
            public override void HandleMessage(Message msg)
            {
                base.HandleMessage(msg);

                if (msg.What == 1)
                {
                    videoViewLayout.NotifyData();
                }

                if (msg.What == 2)
                {
                    videoViewLayout.HideVideoStatusBar();
                }

            }
        }
        #endregion

    }
}