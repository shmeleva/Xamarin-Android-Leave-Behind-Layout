using Android.Content;
using Android.Util;
using Android.Views;
using System;

namespace Xamarin.Android.LeaveBehind.Library
{
    public enum Gravity
    {
        Left = -1,
        Center = 0,
        Right = 1
    }

    public enum StickingPoint
    {
        None = -2,
        This = -1
    }

    public enum ClampingPoint
    {
        This = -2,
        Parent = -1
    }

    public enum ClampingPointEpsilon
    {
        None = -1
    }


    public class LeaveBehindLayoutParameters : ViewGroup.LayoutParams
    {
        public Gravity Gravity { get; }

        public int StickingPoint { get; }
        public int StickingPointEpsilon { get; }

        public int ClampingPoint { get; }
        public int ClampingPointEpsilon { get; }

        //public bool SwipeEnabled { get; }


        public LeaveBehindLayoutParameters(ViewGroup.LayoutParams source) : base(source)
        {
        }

        public LeaveBehindLayoutParameters(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            var styledAttributes = c.ObtainStyledAttributes(attrs, Resource.Styleable.LeaveBehindLayout);

            Gravity = (Gravity)styledAttributes.GetInt(Resource.Styleable.LeaveBehindLayout_gravity, (int)Gravity.Center);

            StickingPoint = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_stickingPoint, (int)Library.StickingPoint.This);
            StickingPointEpsilon = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_stickingPointEpsilon, 0);

            ClampingPoint = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_clamp, (int)Library.ClampingPoint.This);
            ClampingPointEpsilon = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_bringToClamp, (int)Library.ClampingPointEpsilon.None);

            //SwipeEnabled = styledAttributes.GetBoolean(Resource.Styleable.LeaveBehindLayout_swipeEnabled, true);

            styledAttributes.Recycle();
        }

        public LeaveBehindLayoutParameters(int width, int height) : base(width, height)
        {
        }

        public bool TryGetStickingPoint(int width, out int stickingPoint)
        {
            switch (StickingPoint)
            {
                case (int)Library.StickingPoint.None:
                    stickingPoint = (int)Library.StickingPoint.None;
                    return false;
                case (int)Library.StickingPoint.This:
                    stickingPoint = width;
                    return true;
                default:
                    stickingPoint = StickingPoint;
                    return true;
            }
        }

        public int GetClampingPoint()
        {
            throw new NotImplementedException();
        }
    }
}