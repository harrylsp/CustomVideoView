using Android;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Timers;

namespace CustomVideoView
{
    public class ExVideoView : VideoView
    {
        public ExVideoView(Context context) : base(context)
        {

        }

        public ExVideoView(Context context, Android.Util.IAttributeSet attrs) : base(context, attrs)
        {
        }

        public ExVideoView(Context context, Android.Util.IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {

        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            //base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int width = GetDefaultSize(Width / 2, widthMeasureSpec);
            //int height = GetDefaultSize(Height / 2, heightMeasureSpec);
            //SetMeasuredDimension(width, height / 2);// setMeasuredDimension(width, height);
            //int width = (int)Resources.GetDimension(Resource.Dimension.ProductD_Pic_W);
            int height = (int)Resources.GetDimension(Resource.Dimension.ProductD_Pic_H);
            SetMeasuredDimension(width, height);
        }
        
    }
}