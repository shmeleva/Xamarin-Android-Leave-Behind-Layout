﻿using System;
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

    public enum LeaveBehindLayoutStickiness
    {
        None = -2,
        Self = -1
    }


    public class LeaveBehindLayoutParameters : ViewGroup.LayoutParams
    {
        public LeaveBehindLayoutGravity Gravity { get; }

        public int Stickiness { get; }
        public float StickinessSensitivity { get; }


        public LeaveBehindLayoutParameters(ViewGroup.LayoutParams source) : base(source)
        {
        }

        public LeaveBehindLayoutParameters(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            var styledAttributes = c.ObtainStyledAttributes(attrs, Resource.Styleable.LeaveBehindLayout);

            Gravity = (LeaveBehindLayoutGravity)styledAttributes.GetInt(Resource.Styleable.LeaveBehindLayout_gravity, (int)LeaveBehindLayoutGravity.Center);

            Stickiness = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_stickiness, (int)LeaveBehindLayoutStickiness.Self);
            StickinessSensitivity = styledAttributes.GetFloat(Resource.Styleable.LeaveBehindLayout_stickiness_sensitivity, 0.9f);

            styledAttributes.Recycle();
        }

        public LeaveBehindLayoutParameters(int width, int height) : base(width, height)
        {
        }
    }
}