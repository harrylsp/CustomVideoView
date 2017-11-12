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

namespace CustomVideoView
{
    public class Test : LinearLayout
    {
        private ImageView iv;
        private TextView tv;
        public Test(Context context) : this(context, null)
        {
            //this(context, null);
        }

        public Test(Context context, IAttributeSet attrs) : base (context, attrs)
        {
            // 导入布局  
            LayoutInflater.From(context).Inflate(Resource.Layout.Test, this, true);
            iv = FindViewById<ImageView>(Resource.Id.iv);
            tv = FindViewById<TextView>(Resource.Id.tv);

        }

        /** 
         * 设置图片资源 
         */
        public void setImageResource(int resId)
        {
            iv.SetImageResource(resId);
        }

        /** 
         * 设置显示的文字 
         */
        public void setTextViewText(String text)
        {
            tv.Text = text;
        }
    }
}