using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Xamarin.Android.LeaveBehind.Library
{
    public enum LeaveBehindLayoutGravity
    {
        Left = -1,
        Center = 0,
        Right = 1
    }

    public class LeaveBehindLayoutParameters : ViewGroup.LayoutParams
    {
        public LeaveBehindLayoutGravity Gravity { get; }

        public LeaveBehindLayoutParameters(ViewGroup.LayoutParams source) : base(source)
        {
        }

        public LeaveBehindLayoutParameters(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            var styledAttributes = c.ObtainStyledAttributes(attrs, Resource.Styleable.LeaveBehindLayout);

            Gravity = (LeaveBehindLayoutGravity)styledAttributes.GetInt(Resource.Styleable.LeaveBehindLayout_gravity, (int)LeaveBehindLayoutGravity.Center);
            // ...

            styledAttributes.Recycle();
        }

        public LeaveBehindLayoutParameters(int width, int height) : base(width, height)
        {
        }
    }
}