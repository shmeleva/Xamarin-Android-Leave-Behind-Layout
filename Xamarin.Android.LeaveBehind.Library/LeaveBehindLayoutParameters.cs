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
        None = -3,
        View = -2
    }

    public enum ClampingPoint
    {
        View = -2,
        Parent = -1
    }

    public enum ClampingPointEpsilon
    {
        None = -3
    }


    public class LeaveBehindLayoutParameters : ViewGroup.LayoutParams
    {
        public Gravity Gravity { get; }

        public int StickingPoint { get; }
        public int StickingPointEpsilon { get; }

        public int ClampingPoint { get; }
        public int ClampingPointEpsilon { get; }

        public bool ClampingPointEpsilonDefined => ClampingPointEpsilon != (int)Library.ClampingPointEpsilon.None;

        public bool SwipeEnabled { get; set; }


        public LeaveBehindLayoutParameters(ViewGroup.LayoutParams source) : base(source)
        {
        }

        public LeaveBehindLayoutParameters(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            var styledAttributes = c.ObtainStyledAttributes(attrs, Resource.Styleable.LeaveBehindLayout);

            Gravity = (Gravity)styledAttributes.GetInt(Resource.Styleable.LeaveBehindLayout_gravity, (int)Gravity.Center);

            StickingPoint = styledAttributes.GetLayoutDimension(Resource.Styleable.LeaveBehindLayout_stickingPoint, (int)Library.StickingPoint.View);
            ValidateStickingPoint();

            StickingPointEpsilon = styledAttributes.GetLayoutDimension(Resource.Styleable.LeaveBehindLayout_stickingPointEpsilon, 0);
            ValidateStickingPointEpsilon();

            ClampingPoint = styledAttributes.GetLayoutDimension(Resource.Styleable.LeaveBehindLayout_clampingPoint, (int)Library.ClampingPoint.View);
            ValidateClampingPoint();

            ClampingPointEpsilon = styledAttributes.GetLayoutDimension(Resource.Styleable.LeaveBehindLayout_clampingPointEpsilon, (int)Library.ClampingPointEpsilon.None);
            ValidateClampingPointEpsilon();

            SwipeEnabled = styledAttributes.GetBoolean(Resource.Styleable.LeaveBehindLayout_swipeEnabled, true);

            styledAttributes.Recycle();
        }

        public LeaveBehindLayoutParameters(int width, int height) : base(width, height)
        {
        }

        public bool TryGetStickingPoint(int viewWidth, out int stickingPoint)
        {
            switch (StickingPoint)
            {
                case (int)Library.StickingPoint.None:
                    stickingPoint = (int)Library.StickingPoint.None;
                    return false;
                case (int)Library.StickingPoint.View:
                    stickingPoint = viewWidth;
                    return true;
                default:
                    stickingPoint = StickingPoint;
                    return true;
            }
        }

        public int GetClampingPoint(int viewWidth, int parentWidth)
        {
            switch (ClampingPoint)
            {
                case (int)Library.ClampingPoint.Parent:
                    return parentWidth;
                case (int)Library.ClampingPoint.View:
                    return viewWidth;
                default:
                    return ClampingPoint;
            }
        }


        private void ValidateStickingPoint()
        {
            if (StickingPoint >= 0 || Enum.IsDefined(typeof(StickingPoint), StickingPoint))
            {
                return;
            }
            throw new ArgumentException(nameof(StickingPoint));
        }

        private void ValidateStickingPointEpsilon()
        {
            if (StickingPointEpsilon >= 0)
            {
                return;
            }
            throw new ArgumentException(nameof(StickingPointEpsilon));
        }

        private void ValidateClampingPoint()
        {
            if (ClampingPoint >= 0 || Enum.IsDefined(typeof(ClampingPoint), ClampingPoint))
            {
                return;
            }
            throw new ArgumentException(nameof(ClampingPoint));
        }

        private void ValidateClampingPointEpsilon()
        {
            if (ClampingPointEpsilon >= 0 || Enum.IsDefined(typeof(ClampingPointEpsilon), ClampingPointEpsilon))
            {
                return;
            }
            throw new ArgumentException(nameof(ClampingPointEpsilon));
        }
    }
}