using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace CustomVideoView
{
    [Activity(Label = "CustomVideoView", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        DateTime? lastBackKeyDownTime;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            var layout = FindViewById<VideoViewLayout>(Resource.Id.VideoViewLayout);

            //var video = FindViewById<ExVideoView>(Resource.Id.video);
            //Android.Net.Uri videoUri = Android.Net.Uri.Parse("https://tbm.alicdn.com/4avTDEcUOjPu5HXisqp/GXle5GDB7ILjZ5T9n9q%40%40hd.mp4");
            //video.SetVideoURI(videoUri);

            //video.Start();
            //video.RequestFocus();
        }


        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (!lastBackKeyDownTime.HasValue || DateTime.Now - lastBackKeyDownTime.Value > new TimeSpan(0, 0, 2))
                {
                    Toast.MakeText(this.ApplicationContext, "再按一次退出应用程序", ToastLength.Short).Show();
                    lastBackKeyDownTime = DateTime.Now;
                }
                else
                {
                    GC.Collect();
                    Finish();
                }

                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

    }
}

