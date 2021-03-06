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
using Resources = Android.Content.Res.Resources;

namespace Xamarin.Android.LeaveBehind.Library
{
    public class SwipeEventArgs : EventArgs
    {
        public bool IsSwipedRight { get; set; }
    }

    public class LeaveBehindLayout : ViewGroup
    {
        private WeakReference<ObjectAnimator> _resetAnimator;


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
        

        public bool IsRightViewSwipeEnabled => ((LeaveBehindLayoutParameters)RightView?.LayoutParameters)?.SwipeEnabled ?? false;

        public bool IsLeftViewSwipeEnabled => ((LeaveBehindLayoutParameters)LeftView?.LayoutParameters)?.SwipeEnabled ?? false;

        public bool IsSwipeEnabled => IsRightViewSwipeEnabled || IsLeftViewSwipeEnabled;


        public event EventHandler<SwipeEventArgs> SwipeStarted;

        public event EventHandler<SwipeEventArgs> Clamped;

        public event EventHandler<SwipeEventArgs> LeftViewSticked;

        public event EventHandler<SwipeEventArgs> RightViewSticked;


        public LeaveBehindLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            VelocityThreshold = TypedValue.ApplyDimension(ComplexUnitType.Dip, 1500f, Resources.DisplayMetrics);
            TouchSlop = ViewConfiguration.Get(context).ScaledTouchSlop;

            var viewDragHelperCallback = new DragCallback(this);
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

                _resetAnimator = new WeakReference<ObjectAnimator>(animator);
            }
            else
            {
                OffsetChildren(null, -CenterView.Left);
            }

            void FinishResetAnimator()
            {
                if (_resetAnimator == null)
                {
                    return;
                }

                if (_resetAnimator.TryGetTarget(out var animator))
                {
                    _resetAnimator.SetTarget(null);

                    if (animator.IsRunning)
                    {
                        animator.End();
                    }
                }
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
                    case Gravity.Left:
                        LeftView = child;
                        break;
                    case Gravity.Center:
                        CenterView = child;
                        break;
                    case Gravity.Right:
                        RightView = child;
                        break;
                }
            }

            if (CenterView == null)
            {
                throw new NullReferenceException("Surface view is missing.");
            }

            var childTop = PaddingTop;
            foreach (var child in Children().Where(x => x.Visibility != ViewStates.Gone))
            {
                var childLeft = GetChildLeft(child);
                child.Layout(childLeft, childTop, childLeft + child.MeasuredWidth, childTop + child.MeasuredHeight);
            }

            int GetChildLeft(View child)
            {
                var layoutParameters = (LeaveBehindLayoutParameters)child.LayoutParameters;
                switch (layoutParameters.Gravity)
                {
                    case Gravity.Left:
                        return CenterView.Left - child.MeasuredWidth;
                    case Gravity.Right:
                        return CenterView.Right;
                    default:
                        return child.Left;
                }
            }
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

        public void StartScrollAnimation(View view, int finalLeft, bool isClamped, bool isSwipedRight)
        {
            if (ViewDragHelper.SettleCapturedViewAt(finalLeft, view.Top))
            {
                ViewCompat.PostOnAnimation(view, new SettleRunnable(this, view, isClamped, isSwipedRight));
            }
            else if (isClamped)
            {
                Clamped?.Invoke(this, new SwipeEventArgs
                {
                    IsSwipedRight = isSwipedRight
                });
            }
        }


        private List<(View view, bool state)> _nestedScrollingParents = new List<(View view, bool state)>();

        private void SaveNestedScrollingParentsState()
        {
            _nestedScrollingParents = Parents()
                .Where(x => x is INestedScrollingParent)
                .Select(x => (view: (View)x, state: ((View)x).Enabled))
                .ToList();
        }

        private void RestoreNestedScrollingParentsState()
        {
            if (_nestedScrollingParents == null)
            {
                return;
            }
            var nestedScrollingParents = _nestedScrollingParents;
            _nestedScrollingParents = null;

            foreach (var (view, state) in nestedScrollingParents)
            {
                view.Enabled = state;
            }
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
            => IsSwipeEnabled ? ViewDragHelper.ShouldInterceptTouchEvent(ev) : base.OnInterceptTouchEvent(ev);


        enum TouchState
        {
            Wait = 0,
            Swipe = 1,
            Skip = 2
        }

        private TouchState touchState = TouchState.Wait;

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
                    touchState = TouchState.Wait;
                    touchX = ev.GetX();
                    touchY = ev.GetY();
                    break;
                case MotionEventActions.Move:
                    if (touchState == TouchState.Wait)
                    {
                        float dx = Math.Abs(ev.GetX() - touchX);
                        float dy = Math.Abs(ev.GetY() - touchY);

                        bool isLeftToRight = (ev.GetX() - touchX) > 0;

                        if ((isLeftToRight && !IsRightViewSwipeEnabled) || (!isLeftToRight && !IsLeftViewSwipeEnabled))
                        {
                            return defaultResult;
                        }

                        if (dx >= TouchSlop || dy >= TouchSlop)
                        {
                            touchState = dy == 0 || dx / dy > 1f ? TouchState.Swipe : TouchState.Skip;
                            if (touchState == TouchState.Swipe)
                            {
                                RequestDisallowInterceptTouchEvent(true);

                                SaveNestedScrollingParentsState();
                                SwipeStarted?.Invoke(this, new SwipeEventArgs { IsSwipedRight = ev.GetX() > touchX });
                            }
                        }
                    }
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (touchState == TouchState.Swipe)
                    {
                        RestoreNestedScrollingParentsState();
                        RequestDisallowInterceptTouchEvent(false);
                    }
                    touchState = TouchState.Wait;
                    break;
            }

            if (ev.ActionMasked != MotionEventActions.Move || touchState == TouchState.Swipe)
            {
                ViewDragHelper.ProcessTouchEvent(ev);
            }

            return true;
        }


        #region Enumerators

        private IEnumerable<View> Children()
        {
            var childCount = ChildCount;
            for (var i = 0; i < childCount; i++)
            {
                yield return GetChildAt(i);
            }
        }

        private IEnumerable<IViewParent> Parents()
        {
            var parent = Parent;
            while (parent != null)
            {
                yield return parent;
                parent = parent.Parent;
            }
        }

        #endregion


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


        class SettleRunnable : Java.Lang.Object, IRunnable
        {
            private LeaveBehindLayout _layout;
            private View _view;
            private bool _isClamped;
            private bool _isSwipedRight;

            public SettleRunnable(LeaveBehindLayout layout, View view, bool isClamped, bool isSwipedRight)
            {
                _layout = layout;
                _view = view;
                _isClamped = isClamped;
                _isSwipedRight = isSwipedRight;
            }

            public void Run()
            {
                if (_layout.ViewDragHelper.ContinueSettling(true))
                {
                    ViewCompat.PostOnAnimation(_view, this);
                }
                else if (_isClamped)
                {
                    _layout.Clamped?.Invoke(_layout, new SwipeEventArgs
                    {
                        IsSwipedRight = _isSwipedRight
                    });
                }
            }
        }


        class DragCallback : ViewDragHelper.Callback
        {
            private int _initialLeft;
            private LeaveBehindLayout _leaveBehindLayout;

            private int ParentWidth => _leaveBehindLayout.Width;

            private View CenterView => _leaveBehindLayout.CenterView;
            private View LeftView => _leaveBehindLayout.LeftView;
            private View RightView => _leaveBehindLayout.RightView;


            public DragCallback(LeaveBehindLayout leaveBehindLayout)
                => _leaveBehindLayout = leaveBehindLayout;


            #region ViewDragHelper.Callback

            public override bool TryCaptureView(View child, int pointerId)
            {
                _initialLeft = child.Left;
                return true;
            }

            public override int ClampViewPositionHorizontal(View child, int left, int dx)
                => (dx > 0) ? ClampMoveRight(child, left) : ClampMoveLeft(child, left);

            public override int GetViewHorizontalDragRange(View child) => ParentWidth;

            public override void OnViewReleased(View releasedChild, float xVelocity, float yVelocity)
            {
                int dx = releasedChild.Left - _initialLeft;
                if (dx == 0)
                {
                    return;
                }

                var isSwipedRight = dx > 0;
                var handled = TryHandleViewRelease();

                if (!handled)
                {
                    var finalLeft = releasedChild.Left - CenterView.Left;
                    _leaveBehindLayout.StartScrollAnimation(releasedChild, finalLeft, isClamped: false, isSwipedRight: isSwipedRight);
                }

                bool TryHandleViewRelease()
                {
                    if (isSwipedRight)
                    {
                        return xVelocity >= 0 ? OnRightSwipeReleased(releasedChild, dx, xVelocity) : OnLeftSwipeReleased(releasedChild, dx, xVelocity);
                    }
                    else
                    {
                        return xVelocity <= 0 ? OnLeftSwipeReleased(releasedChild, dx, xVelocity) : OnRightSwipeReleased(releasedChild, dx, xVelocity);
                    }
                }
            }

            public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
            {
                if (dx == 0)
                {
                    return;
                }

                _leaveBehindLayout.OffsetChildren(changedView, dx);

                var isSwipedRight = dx > 0;

                if (LeftView != null)
                {
                    var leftViewLayoutParameters = (LeaveBehindLayoutParameters)LeftView.LayoutParameters;
                    if (leftViewLayoutParameters.TryGetStickingPoint(LeftView.Width, out var stickingPoint))
                    {
                        if (isSwipedRight)
                        {
                            if (LeftView.Right - stickingPoint > 0 && LeftView.Right - stickingPoint - dx <= 0)
                            {
                                _leaveBehindLayout.LeftViewSticked?.Invoke(this, new SwipeEventArgs { IsSwipedRight = true });
                            }
                        }
                        else
                        {
                            if (LeftView.Right - stickingPoint <= 0 && LeftView.Right - stickingPoint - dx > 0)
                            {
                                _leaveBehindLayout.LeftViewSticked?.Invoke(this, new SwipeEventArgs { IsSwipedRight = false });
                            }
                        }
                    }
                }

                if (RightView != null)
                {
                    var rightViewLayoutParameters = (LeaveBehindLayoutParameters)RightView.LayoutParameters;
                    if (rightViewLayoutParameters.TryGetStickingPoint(RightView.Width, out var stickingPoint))
                    {
                        if (isSwipedRight)
                        {
                            if (RightView.Left + stickingPoint > ParentWidth && RightView.Left + stickingPoint - dx <= ParentWidth)
                            {
                                _leaveBehindLayout.RightViewSticked?.Invoke(this, new SwipeEventArgs { IsSwipedRight = true });
                            }
                        }
                        else
                        {
                            if (RightView.Left + stickingPoint <= ParentWidth && RightView.Left + stickingPoint - dx > ParentWidth)
                            {
                                _leaveBehindLayout.RightViewSticked?.Invoke(this, new SwipeEventArgs { IsSwipedRight = false });
                            }
                        }
                    }
                }
            }

            #endregion


            private bool IsLeftViewClamped(LeaveBehindLayoutParameters layoutParameters)
            {
                if (LeftView == null)
                {
                    return false;
                }

                switch (layoutParameters.ClampingPoint)
                {
                    case (int)ClampingPoint.Parent:
                        return LeftView.Right >= ParentWidth;
                    case (int)ClampingPoint.View:
                        return LeftView.Right >= LeftView.Width;
                    default:
                        return LeftView.Right >= layoutParameters.ClampingPoint;
                }
            }

            private bool IsRightViewClamped(LeaveBehindLayoutParameters layoutParameters)
            {
                if (RightView == null)
                {
                    return false;
                }

                switch (layoutParameters.ClampingPoint)
                {
                    case (int)ClampingPoint.Parent:
                        return RightView.Right <= ParentWidth;
                    case (int)ClampingPoint.View:
                        return RightView.Right <= RightView.Width;
                    default:
                        return RightView.Left + layoutParameters.ClampingPoint <= ParentWidth;
                }
            }

            private int ClampMoveRight(View child, int left)
            {
                if (LeftView == null)
                {
                    return (child == CenterView) ? Math.Min(left, 0) : Math.Min(left, ParentWidth);
                }

                var layoutParameters = child.LayoutParameters as LeaveBehindLayoutParameters;
                switch (layoutParameters.ClampingPoint)
                {
                    case (int)ClampingPoint.Parent:
                        return Math.Min(left, ParentWidth + child.Left - LeftView.Right);
                    case (int)ClampingPoint.View:
                        return Math.Min(left, child.Left - LeftView.Left);
                    default:
                        return Math.Min(left, child.Left - LeftView.Right + layoutParameters.ClampingPoint);
                }
            }

            private int ClampMoveLeft(View child, int left)
            {
                if (RightView == null)
                {
                    return (child == CenterView) ? Math.Max(left, 0) : Math.Max(left, -child.Width);
                }

                var layoutParameters = child.LayoutParameters as LeaveBehindLayoutParameters;
                switch (layoutParameters.ClampingPoint)
                {
                    case (int)ClampingPoint.Parent:
                        return Math.Max(child.Left - RightView.Left, left);
                    case (int)ClampingPoint.View:
                        return Math.Max(left, ParentWidth - RightView.Left + child.Left - RightView.Width);
                    default:
                        return Math.Max(left, ParentWidth - RightView.Left + child.Left - layoutParameters.ClampingPoint);
                }

            }

            private bool OnRightSwipeReleased(View child, int dx, float xVelocity)
            {
                if (xVelocity > _leaveBehindLayout.VelocityThreshold)
                {
                    var left = (CenterView.Left < 0) ? child.Left - CenterView.Left : ParentWidth;
                    var moveToInitialPosition = CenterView.Left < 0;
                    StartScrollAnimation(ClampMoveRight(child, left), isClamped: !moveToInitialPosition);
                    return true;
                }

                if (LeftView == null)
                {
                    StartScrollAnimation(child.Left - CenterView.Left, isClamped: false);
                    return true;
                }

                var layoutParameters = (LeaveBehindLayoutParameters)LeftView.LayoutParameters;

                if (dx > 0 && xVelocity >= 0)
                {
                    if (IsLeftViewClamped(layoutParameters))
                    {
                        _leaveBehindLayout.Clamped?.Invoke(this, new SwipeEventArgs { IsSwipedRight = true });
                        return true;
                    }

                    var clampingPoint = layoutParameters.GetClampingPoint(LeftView.Width, ParentWidth);
                    if (layoutParameters.ClampingPointEpsilonDefined && LeftView.Right > clampingPoint - layoutParameters.ClampingPointEpsilon)
                    {
                        int left = CenterView.Left < 0 ? child.Left - CenterView.Left : ParentWidth;
                        StartScrollAnimation(ClampMoveRight(child, left), isClamped: true);
                        return true;
                    }
                }

                if (layoutParameters.TryGetStickingPoint(RightView.Width, out var stickingPoint))
                {
                    if (IsInEpsilonNeighbourhood(CenterView.Left - stickingPoint, layoutParameters.StickingPointEpsilon))
                    {
                        var isClamped = layoutParameters.ClampingPoint == (int)ClampingPoint.View && stickingPoint == LeftView.Width
                            || layoutParameters.ClampingPoint == stickingPoint
                            || layoutParameters.ClampingPoint == (int)ClampingPoint.Parent && stickingPoint == ParentWidth;

                        StartScrollAnimation(child.Left - CenterView.Left + stickingPoint, isClamped: isClamped);
                        return true;
                    }
                }

                return false;


                void StartScrollAnimation(int finalLeft, bool isClamped)
                    => _leaveBehindLayout.StartScrollAnimation(child, finalLeft, isClamped, isSwipedRight: true);
            }

            private bool OnLeftSwipeReleased(View child, int dx, float xVelocity)
            {
                if (-xVelocity > _leaveBehindLayout.VelocityThreshold)
                {
                    var left = CenterView.Left > 0 ? child.Left - CenterView.Left : -ParentWidth;
                    var moveToOriginalPosition = CenterView.Left > 0;

                    StartScrollAnimation(ClampMoveLeft(child, left), isClamped: !moveToOriginalPosition);
                    return true;
                }

                if (RightView == null)
                {
                    StartScrollAnimation(child.Left - CenterView.Left, false);
                    return true;
                }

                var layoutParameters = (LeaveBehindLayoutParameters)RightView.LayoutParameters;

                if (dx < 0 && xVelocity <= 0)
                {
                    if (IsRightViewClamped(layoutParameters))
                    {
                        _leaveBehindLayout.Clamped?.Invoke(this, new SwipeEventArgs { IsSwipedRight = false });
                        return true;
                    }

                    var clampingPoint = layoutParameters.GetClampingPoint(RightView.Width, ParentWidth);
                    if (layoutParameters.ClampingPointEpsilonDefined && RightView.Left + clampingPoint - layoutParameters.ClampingPointEpsilon < ParentWidth)
                    {
                        int left = CenterView.Left > 0 ? child.Left - CenterView.Left : -ParentWidth;
                        StartScrollAnimation(ClampMoveLeft(child, left), true);
                        return true;
                    }
                }

                if (layoutParameters.TryGetStickingPoint(RightView.Width, out var stickingPoint))
                {
                    if (IsInEpsilonNeighbourhood(CenterView.Right + stickingPoint - ParentWidth, layoutParameters.StickingPointEpsilon))
                    {
                        var isClamped = layoutParameters.ClampingPoint == (int)ClampingPoint.View && stickingPoint == RightView.Width
                            || layoutParameters.ClampingPoint == stickingPoint
                            || layoutParameters.ClampingPoint == (int)ClampingPoint.Parent && stickingPoint == ParentWidth;

                        StartScrollAnimation(child.Left - RightView.Left + ParentWidth - stickingPoint, isClamped);
                        return true;
                    }
                }

                return false;


                void StartScrollAnimation(int finalLeft, bool isClamped)
                    => _leaveBehindLayout.StartScrollAnimation(child, finalLeft, isClamped, isSwipedRight: false);
            }


            private static int PxToDp(int px) => (int)(px / Resources.System.DisplayMetrics.Density);

            private static bool IsInEpsilonNeighbourhood(int value, int epsilon) => value >= -epsilon && value <= epsilon;
        }
    }
}
