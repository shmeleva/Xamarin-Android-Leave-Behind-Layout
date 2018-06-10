using Android.Content;
using Android.Util;
using Android.Views;

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

    public enum LeaveBehindLayoutClamp
    {
        Self = -2,
        Parent = -1
    }

    public enum LeaveBehindLayoutBringToClamp
    {
        No = -1
    }


    public class LeaveBehindLayoutParameters : ViewGroup.LayoutParams
    {
        public LeaveBehindLayoutGravity Gravity { get; }

        public int Stickiness { get; }
        public float StickinessSensitivity { get; }

        public int Clamp { get; }
        public int BringToClamp { get; }

        //public bool SwipeEnabled { get; }


        public LeaveBehindLayoutParameters(ViewGroup.LayoutParams source) : base(source)
        {
        }

        public LeaveBehindLayoutParameters(Context c, IAttributeSet attrs) : base(c, attrs)
        {
            var styledAttributes = c.ObtainStyledAttributes(attrs, Resource.Styleable.LeaveBehindLayout);

            Gravity = (LeaveBehindLayoutGravity)styledAttributes.GetInt(Resource.Styleable.LeaveBehindLayout_gravity, (int)LeaveBehindLayoutGravity.Center);

            Stickiness = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_stickiness, (int)LeaveBehindLayoutStickiness.Self);
            StickinessSensitivity = styledAttributes.GetFloat(Resource.Styleable.LeaveBehindLayout_stickinessSensitivity, 0.9f);

            Clamp = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_clamp, (int)LeaveBehindLayoutClamp.Self);
            BringToClamp = (int)styledAttributes.GetDimension(Resource.Styleable.LeaveBehindLayout_bringToClamp, (int)LeaveBehindLayoutBringToClamp.No);

            //SwipeEnabled = styledAttributes.GetBoolean(Resource.Styleable.LeaveBehindLayout_swipeEnabled, true);

            styledAttributes.Recycle();
        }

        public LeaveBehindLayoutParameters(int width, int height) : base(width, height)
        {
        }
    }
}