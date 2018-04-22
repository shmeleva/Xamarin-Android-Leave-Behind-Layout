using Android.Content;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamarin.Android.LeaveBehind.Library
{
    public class LeaveBehindLayout : ViewGroup
    {
        public View LeftView { get; }
        public View RightView { get; }
        public View CenterView { get; }


        public LeaveBehindLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }


        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {

        }

        #region LeaveBehindLayoutParameters

        protected override LayoutParams GenerateDefaultLayoutParams()
            => new LeaveBehindLayoutParameters(LayoutParams.MatchParent, LayoutParams.WrapContent);

        public override LayoutParams GenerateLayoutParams(IAttributeSet attrs)
            => new LeaveBehindLayoutParameters(Context, attrs);

        protected override LayoutParams GenerateLayoutParams(LayoutParams p)
            => new LeaveBehindLayoutParameters(p);

        protected override bool CheckLayoutParams(LayoutParams p)
            => p is LeaveBehindLayoutParameters;

        #endregion
    }
}
