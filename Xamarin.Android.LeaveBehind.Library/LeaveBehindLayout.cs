﻿using Android.Animation;
using Android.Content;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Android.Views.Animations;

using Java.Lang;

using System;
using System.Collections.Generic;
using System.Linq;

using Math = System.Math;


namespace Xamarin.Android.LeaveBehind.Library
{
    public class SwipeEventArgs : EventArgs
    {
        public bool MoveRight { get; set; }
    }

    public class LeaveBehindLayoutDragCallback : ViewDragHelper.Callback
    {
        private int initialLeft;
        private LeaveBehindLayout leaveBehindLayout;


        public int Width => leaveBehindLayout.Width;


        public LeaveBehindLayoutDragCallback(LeaveBehindLayout leaveBehindLayout)
        {
            this.leaveBehindLayout = leaveBehindLayout;
        }


        public override bool TryCaptureView(View child, int pointerId)
        {
            initialLeft = child.Left;
            return true;
        }

        public override int ClampViewPositionHorizontal(View child, int left, int dx) // +
            => (dx > 0) ? ClampMoveRight(child, left) : ClampMoveLeft(child, left);

        public override int GetViewHorizontalDragRange(View child) => Width; // +

        public override void OnViewReleased(View releasedChild, float xVelocity, float yVelocity)
        {
            int dx = releasedChild.Left - initialLeft;
            if (dx == 0)
            {
                return;
            }

            var handled = TryHandleViewRelease();
            if (!handled)
            {
                leaveBehindLayout.StartScrollAnimation(releasedChild, releasedChild.Left - leaveBehindLayout.CenterView.Left, false, dx > 0);
            }

            bool TryHandleViewRelease()
            {
                if (dx > 0)
                {
                    return xVelocity >= 0
                        ? OnMoveRightReleased(releasedChild, dx, xVelocity)
                        : OnMoveLeftReleased(releasedChild, dx, xVelocity);
                }
                else
                {
                    return xVelocity <= 0
                        ? OnMoveLeftReleased(releasedChild, dx, xVelocity)
                        : OnMoveRightReleased(releasedChild, dx, xVelocity);
                }
            }
        }

        private bool LeftViewClampReached(LeaveBehindLayoutParameters leftViewLP) //+
        {
            var leftView = leaveBehindLayout.LeftView;
            if (leftView == null)
            {
                return false;
            }

            switch (leftViewLP.Clamp)
            {
                case (int)LeaveBehindLayoutClamp.Parent:
                    return leftView.Right >= Width;
                case (int)LeaveBehindLayoutClamp.Self:
                    return leftView.Right >= leftView.Width;
                default:
                    return leftView.Right >= leftViewLP.Clamp;
            }
        }

        private bool RightViewClampReached(LeaveBehindLayoutParameters lp) // +
        {
            var rightView = leaveBehindLayout.RightView;
            if (rightView == null)
            {
                return false;
            }

            switch (lp.Clamp)
            {
                case (int)LeaveBehindLayoutClamp.Parent:
                    return rightView.Right <= Width;
                case (int)LeaveBehindLayoutClamp.Self:
                    return rightView.Right <= Width; // ???
                default:
                    return rightView.Left + lp.Clamp <= Width;
            }
        }

        public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
        {
            leaveBehindLayout.OffsetChildren(changedView, dx);

            int stickyBound;

            if (dx > 0) // Right direction.
            {
                if (leaveBehindLayout.LeftView != null)
                {
                    stickyBound = GetStickyBound(leaveBehindLayout.LeftView);
                    if (stickyBound != (int)LeaveBehindLayoutStickiness.None)
                    {
                        if (leaveBehindLayout.LeftView.Right - stickyBound > 0 && leaveBehindLayout.LeftView.Right - stickyBound - dx <= 0)
                        {
                            // !
                            // leaveBehindLayout.LeftStickyEdgeReached?.Invoke(this, new SwipeEventArgs { MoveRight = true });
                        }
                    }
                }

                if (leaveBehindLayout.RightView != null)
                {
                    stickyBound = GetStickyBound(leaveBehindLayout.RightView);
                    if (stickyBound != (int)LeaveBehindLayoutStickiness.None)
                    {
                        if (leaveBehindLayout.RightView.Left + stickyBound > Width && leaveBehindLayout.RightView.Left + stickyBound - dx <= Width)
                        {
                            // !
                            // leaveBehindLayout.RightStickyEdgeReached?.Invoke(this, new SwipeEventArgs { MoveRight = true });
                        }
                    }
                }
            }
            else if (dx < 0) // Left direction.
            {
                if (leaveBehindLayout.LeftView != null)
                {
                    stickyBound = GetStickyBound(leaveBehindLayout.LeftView);
                    if (stickyBound != (int)LeaveBehindLayoutStickiness.None)
                    {
                        if (leaveBehindLayout.LeftView.Right - stickyBound <= 0 && leaveBehindLayout.LeftView.Right - stickyBound - dx > 0)
                        {
                            // !
                            // leaveBehindLayout.LeftStickyEdgeReached?.Invoke(this, new SwipeEventArgs { MoveRight = false });
                        }
                    }
                }

                if (leaveBehindLayout.RightView != null)
                {
                    stickyBound = GetStickyBound(leaveBehindLayout.RightView);
                    if (stickyBound != (int)LeaveBehindLayoutStickiness.None)
                    {
                        if (leaveBehindLayout.RightView.Left + stickyBound <= Width && leaveBehindLayout.RightView.Left + stickyBound - dx > Width)
                        {
                            // !
                            // leaveBehindLayout.RightStickyEdgeReached?.Invoke(this, new SwipeEventArgs { MoveRight = false });
                        }
                    }
                }
            }
        }

        private int GetStickyBound(View view)
        {
            var lp = view.LayoutParameters as LeaveBehindLayoutParameters;
            if (lp.Stickiness == (int)LeaveBehindLayoutStickiness.None)
            {
                return (int)LeaveBehindLayoutStickiness.None;
            }

            return lp.Stickiness == (int)LeaveBehindLayoutStickiness.Self
                ? view.Width
                : lp.Stickiness;
        }

        private int ClampMoveRight(View child, int left) // +
        {
            var leftView = leaveBehindLayout.LeftView;
            if (leftView == null)
            {
                return (child == leaveBehindLayout.CenterView)
                    ? Math.Min(left, 0)
                    : Math.Min(left, Width);
            }

            var lp = child.LayoutParameters as LeaveBehindLayoutParameters;
            switch (lp.Clamp)
            {
                case (int)LeaveBehindLayoutClamp.Parent:
                    return Math.Min(left, Width + child.Left - leftView.Right);
                case (int)LeaveBehindLayoutClamp.Self:
                    return Math.Min(left, child.Left - leftView.Left);
                default:
                    return Math.Min(left, child.Left - leftView.Right + lp.Clamp);
            }
        }

        private int ClampMoveLeft(View child, int left) // +
        {
            var rightView = leaveBehindLayout.RightView;
            if (rightView == null)
            {
                return (child == leaveBehindLayout.CenterView)
                    ? Math.Max(left, 0)
                    : Math.Max(left, -child.Width);
            }

            var lp = child.LayoutParameters as LeaveBehindLayoutParameters;
            switch (lp.Clamp)
            {
                case (int)LeaveBehindLayoutClamp.Parent:
                    return Math.Max(child.Left - rightView.Left, left);
                case (int)LeaveBehindLayoutClamp.Self:
                    return Math.Max(left, Width - rightView.Left + child.Left - rightView.Width);
                default:
                    return Math.Max(left, Width - rightView.Left + child.Left - lp.Clamp);
            }

        }

        private bool OnMoveRightReleased(View child, int dx, float xVelocity)
        {
            var centerView = leaveBehindLayout.CenterView;
            var leftView = leaveBehindLayout.LeftView;

            if (xVelocity > leaveBehindLayout.VelocityThreshold)
            {
                var left = (centerView.Left < 0) ? child.Left - centerView.Left : Width;
                var moveToInitial = centerView.Left < 0;
                leaveBehindLayout.StartScrollAnimation(child, ClampMoveRight(child, left), !moveToInitial, true);
                return true;
            }

            if (leftView == null)
            {
                leaveBehindLayout.StartScrollAnimation(child, child.Left - centerView.Left, false, true);
                return true;
            }

            var lp = (LeaveBehindLayoutParameters)leftView.LayoutParameters;

            if (dx > 0 && xVelocity >= 0 && LeftViewClampReached(lp))
            {
                // ...
                // leaveBehindLayout.ClampReached?.Invoke(this, new SwipeEventArgs { MoveRight = true });
                return true;
            }

            if (dx > 0 && xVelocity >= 0 && lp.BringToClamp != (int)LeaveBehindLayoutBringToClamp.No && leftView.Right > lp.BringToClamp)
            {
                int left = centerView.Left < 0 ? child.Left - centerView.Left : Width;
                leaveBehindLayout.StartScrollAnimation(child, ClampMoveRight(child, left), true, true);
                return true;
            }

            if (lp.Stickiness != (int)LeaveBehindLayoutStickiness.None)
            {
                int stickyBound = lp.Stickiness == (int)LeaveBehindLayoutStickiness.Self
                    ? leftView.Width
                    : lp.Stickiness;

                float amplitude = stickyBound * lp.StickinessSensitivity;

                if (IsBetween(-amplitude, amplitude, centerView.Left - stickyBound))
                {
                    var toClamp = (lp.Clamp == (int)LeaveBehindLayoutClamp.Self && stickyBound == leftView.Width) ||
                            lp.Clamp == stickyBound ||
                            (lp.Clamp == (int)LeaveBehindLayoutClamp.Parent && stickyBound == Width);
                    leaveBehindLayout.StartScrollAnimation(child, child.Left - centerView.Left + stickyBound, toClamp, true);
                    return true;
                }
            }

            return false;
        }

        private bool OnMoveLeftReleased(View child, int dx, float xvel)
        {
            var centerView = leaveBehindLayout.CenterView;
            var rightView = leaveBehindLayout.RightView;

            if (-xvel > leaveBehindLayout.VelocityThreshold)
            {
                int left = centerView.Left > 0 ? child.Left - centerView.Left : -Width;
                var moveToOriginal = centerView.Left > 0;
                leaveBehindLayout.StartScrollAnimation(child, ClampMoveLeft(child, left), !moveToOriginal, false);
                return true;
            }

            if (rightView == null)
            {
                leaveBehindLayout.StartScrollAnimation(child, child.Left - centerView.Left, false, false);
                return true;
            }

            var lp = (LeaveBehindLayoutParameters)rightView.LayoutParameters;

            if (dx < 0 && xvel <= 0 && RightViewClampReached(lp))
            {
                // ...
                // leaveBehindLayout.ClampReached?.Invoke(this, new SwipeEventArgs { MoveRight = false });
                return true;
            }

            if (dx < 0 && xvel <= 0 && lp.BringToClamp != (int)LeaveBehindLayoutBringToClamp.No && rightView.Left + lp.BringToClamp < Width)
            {
                int left = centerView.Left > 0 ? child.Left - centerView.Left : -Width;
                leaveBehindLayout.StartScrollAnimation(child, ClampMoveLeft(child, left), true, false);
                return true;
            }

            if (lp.Stickiness != (int)LeaveBehindLayoutStickiness.None)
            {
                int stickyBound = lp.Stickiness == (int)LeaveBehindLayoutStickiness.Self
                    ? rightView.Width
                    : lp.Stickiness;

                float amplitude = stickyBound * lp.StickinessSensitivity;

                if (IsBetween(-amplitude, amplitude, centerView.Right + stickyBound - Width))
                {
                    var toClamp = (lp.Clamp == (int)LeaveBehindLayoutClamp.Self && stickyBound == rightView.Width) ||
                            lp.Clamp == stickyBound ||
                            (lp.Clamp == (int)LeaveBehindLayoutClamp.Parent && stickyBound == Width);
                    leaveBehindLayout.StartScrollAnimation(child, child.Left - rightView.Left + Width - stickyBound, toClamp, false);
                    return true;
                }
            }

            return false;
        }

        private bool IsBetween(float left, float right, float check)
        {
            return check >= left && check <= right;
        }
    }

    public class LeaveBehindLayout : ViewGroup
    {
        private WeakReference<ObjectAnimator> resetAnimator;


        public ViewDragHelper ViewDragHelper { get; }


        public View LeftView { get; private set; }

        public View RightView { get; private set; }

        public View CenterView { get; private set; }


        public float VelocityThreshold { get; }

        public float TouchSlop { get; }

        public int Offset
        {
            get => CenterView == null ? 0 : CenterView.Left;
            set => OffsetChildren(null, value - CenterView.Left);
        }
        

        public bool IsLeftSwipeEnabled { get; set; } = true;

        public bool IsRightSwipeEnabled { get; set; } = true;

        public bool IsSwipeEnabled => IsLeftSwipeEnabled || IsRightSwipeEnabled;


        public event EventHandler<SwipeEventArgs> SwipeStarted;

        public event EventHandler<SwipeEventArgs> ClampReached;

        public event EventHandler<SwipeEventArgs> LeftStickyEdgeReached;

        public event EventHandler<SwipeEventArgs> RightStickyEdgeReached;


        public LeaveBehindLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            VelocityThreshold = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1500f, Resources.DisplayMetrics);
            TouchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;

            var viewDragHelperCallback = new LeaveBehindLayoutDragCallback(this);
            ViewDragHelper = ViewDragHelper.Create(this, 1.0f, viewDragHelperCallback);
        }


        public void Reset(bool animate = true)
        {
            FinishResetAnimator();
            ViewDragHelper.Abort();

            if (animate)
            {
                var animator = new ObjectAnimator { PropertyName = "offset" };
                animator.SetTarget(this);
                animator.SetInterpolator(new AccelerateInterpolator());
                animator.SetIntValues(CenterView.Left, 0);
                animator.SetDuration(200);
                animator.Start();

                resetAnimator = new WeakReference<ObjectAnimator>(animator);
            }
            else
            {
                OffsetChildren(null, -CenterView.Left);
            }

            void FinishResetAnimator()
            {
                if (resetAnimator == null)
                {
                    return;
                }

                if (resetAnimator.TryGetTarget(out var animator))
                {
                    resetAnimator.SetTarget(null);

                    if (animator.IsRunning)
                    {
                        animator.End();
                    }
                }
            }
        }

        public IEnumerable<View> Children()
        {
            var childCount = ChildCount;
            for (var i = 0; i < childCount; i++)
            {
                yield return GetChildAt(i);
            }
        }

        public IEnumerable<IViewParent> Parents()
        {
            var parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var maximumChildHeight = 0;

            if (MeasureSpec.GetMode(heightMeasureSpec) == MeasureSpecMode.Exactly)
            {
                MeasureChildren(widthMeasureSpec, heightMeasureSpec);
            }
            else
            {
                foreach (var child in Children())
                {
                    MeasureChild(child, widthMeasureSpec, heightMeasureSpec);
                    maximumChildHeight = Math.Max(maximumChildHeight, child.MeasuredHeight);
                }

                if (maximumChildHeight > 0)
                {
                    heightMeasureSpec = MeasureSpec.MakeMeasureSpec(maximumChildHeight, MeasureSpecMode.Exactly);
                    MeasureChildren(widthMeasureSpec, heightMeasureSpec);
                }
            }

            foreach (var child in Children().Where(x => x.Visibility != ViewStates.Gone))
            {
                maximumChildHeight = Math.Max(maximumChildHeight, child.MeasuredHeight);
            }

            var minimumHeight = Math.Max(maximumChildHeight + PaddingTop + PaddingBottom, SuggestedMinimumHeight);
            
            SetMeasuredDimension(
                ResolveSize(SuggestedMinimumWidth, widthMeasureSpec),
                ResolveSize(minimumHeight, heightMeasureSpec));
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            foreach (var child in Children())
            {
                var layoutParameters = (LeaveBehindLayoutParameters)child.LayoutParameters;
                switch (layoutParameters.Gravity)
                {
                    case LeaveBehindLayoutGravity.Left:
                        LeftView = child;
                        break;
                    case LeaveBehindLayoutGravity.Center:
                        CenterView = child;
                        break;
                    case LeaveBehindLayoutGravity.Right:
                        RightView = child;
                        break;
                }
            }

            if (CenterView == null)
            {
                throw new NullReferenceException("Surface view is missing.");
            }

            foreach (var child in Children().Where(x => x.Visibility != ViewStates.Gone))
            {
                var childLeft = GetChildLeft(child);
                var childTop = GetChildTop(child);
                child.Layout(childLeft, childTop, childLeft + child.MeasuredWidth, childTop + child.MeasuredHeight);
            }

            int GetChildLeft(View child)
            {
                var layoutParameters = (LeaveBehindLayoutParameters)child.LayoutParameters;
                switch (layoutParameters.Gravity)
                {
                    case LeaveBehindLayoutGravity.Left:
                        return CenterView.Left - child.MeasuredWidth;
                    case LeaveBehindLayoutGravity.Right:
                        return CenterView.Right;
                    default:
                        return child.Left;
                }
            }

            int GetChildTop(View child) => PaddingTop;
        }

        public void OffsetChildren(View skip, int dx)
        {
            if (dx == 0)
            {
                return;
            }

            foreach (var child in Children().Where(x => x != skip))
            {
                child.OffsetLeftAndRight(dx);
                Invalidate(child.Left, child.Top, child.Right, child.Bottom);
            }
        }

        class SettleRunnable : Java.Lang.Object, IRunnable
        {
            private LeaveBehindLayout leaveBehindLayout;
            private View view;
            private bool moveToClamp;
            private bool moveToRight;

            public SettleRunnable(LeaveBehindLayout leaveBehindLayout, View view, bool moveToClamp, bool moveToRight)
            {
                this.leaveBehindLayout = leaveBehindLayout;
                this.view = view;
                this.moveToClamp = moveToClamp;
                this.moveToRight = moveToRight;
            }

            public void Run()
            {
                if (leaveBehindLayout.ViewDragHelper != null && leaveBehindLayout.ViewDragHelper.ContinueSettling(true))
                {
                    ViewCompat.PostOnAnimation(view, this);
                }
                else
                {
                    if (moveToClamp)
                    {
                        leaveBehindLayout.ClampReached?.Invoke(leaveBehindLayout, new SwipeEventArgs { MoveRight = moveToRight });
                    }
                }
            }
        }

        public void StartScrollAnimation(View view, int targetX, bool moveToClamp, bool moveToRight)
        {
            if (ViewDragHelper.SettleCapturedViewAt(targetX, view.Top))
            {
                ViewCompat.PostOnAnimation(view, new SettleRunnable(this, view, moveToClamp, moveToRight));
            }
            else
            {
                if (moveToClamp)
                {
                    ClampReached?.Invoke(this, new SwipeEventArgs { MoveRight = moveToRight });
                }
            }
        }


        private List<(View view, bool state)> nestedScrollingParents = new List<(View view, bool state)>();

        private void SaveNestedScrollingParentsState()
        {
            nestedScrollingParents = Parents()
                .Where(x => x is INestedScrollingParent)
                .Select(x => (view: (View)x, state: ((View)x).Enabled))
                .ToList();
        }

        private void RestoreNestedScrollingParentsState()
        {
            if (nestedScrollingParents == null)
            {
                return;
            }

            foreach (var (view, state) in nestedScrollingParents)
            {
                view.Enabled = state;
            }

            nestedScrollingParents = null;
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
            => IsSwipeEnabled ? ViewDragHelper.ShouldInterceptTouchEvent(ev) : base.OnInterceptTouchEvent(ev);


        private static int TOUCH_STATE_WAIT = 0;
        private static int TOUCH_STATE_SWIPE = 1;
        private static int TOUCH_STATE_SKIP = 2;

        private int touchState = TOUCH_STATE_WAIT;
        private float touchX;
        private float touchY;

        public override bool OnTouchEvent(MotionEvent ev)
        {
            var defaultResult = base.OnTouchEvent(ev);

            if (!IsSwipeEnabled)
            {
                return defaultResult;
            }

            switch (ev.ActionMasked)
            {
                case MotionEventActions.Down:
                    touchState = TOUCH_STATE_WAIT;
                    touchX = ev.GetX();
                    touchY = ev.GetY();
                    break;
                case MotionEventActions.Move:
                    if (touchState == TOUCH_STATE_WAIT)
                    {
                        float dx = Math.Abs(ev.GetX() - touchX);
                        float dy = Math.Abs(ev.GetY() - touchY);

                        bool isLeftToRight = (ev.GetX() - touchX) > 0;

                        if ((isLeftToRight && !IsLeftSwipeEnabled) || (!isLeftToRight && !IsRightSwipeEnabled))
                        {
                            return defaultResult;
                        }

                        if (dx >= TouchSlop || dy >= TouchSlop)
                        {
                            touchState = dy == 0 || dx / dy > 1f ? TOUCH_STATE_SWIPE : TOUCH_STATE_SKIP;
                            if (touchState == TOUCH_STATE_SWIPE)
                            {
                                RequestDisallowInterceptTouchEvent(true);

                                SaveNestedScrollingParentsState();
                                SwipeStarted?.Invoke(this, new SwipeEventArgs { MoveRight = ev.GetX() > touchX });
                            }
                        }
                    }
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (touchState == TOUCH_STATE_SWIPE)
                    {
                        RestoreNestedScrollingParentsState();
                        RequestDisallowInterceptTouchEvent(false);
                    }
                    touchState = TOUCH_STATE_WAIT;
                    break;
            }

            if (ev.ActionMasked != MotionEventActions.Move || touchState == TOUCH_STATE_SWIPE)
            {
                ViewDragHelper.ProcessTouchEvent(ev);
            }

            return true;
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
