using SadChromaLib.UI;

namespace ScriptEditor;

public partial class FancyModalView: DialogView
{
    Vector2 _originalRootControlPosition;
    Tween _tween;

    public override async void _EnterTree()
    {
        Visible = true;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Control rootControl = GetRootControl();
        rootControl.PivotOffset = rootControl.Size / 2;

        _originalRootControlPosition = rootControl.Position;
        base.SetVisibility(false);
    }

    public override void SetVisibility(bool visible)
    {
        if (visible) {
            StartAnimate(true);
            base.SetVisibility(true);
        }
        else {
            ResignActive();
            StartAnimate(false, () => base.SetVisibility(false));
        }
    }

    #region Animation

    private void StartAnimate(bool inAnim, Action completionCallback = null)
    {
        const float duration = 0.33f;
        const float yDisplaceAmount = 140.0f;
        const float hiddenScale = 0.9f;

        Color transparent = new(1.0f, 1.0f, 1.0f, 0.0f);
        Vector2 displacedPosition = _originalRootControlPosition.Offset(0.0f, yDisplaceAmount);

        Control rootControl = GetRootControl();

        rootControl.Modulate = inAnim ? transparent : Colors.White;
        rootControl.Position = inAnim ? displacedPosition : _originalRootControlPosition;
        rootControl.Scale = (inAnim ? hiddenScale : 1.0f).AsVector2();

        TweenUtils.StartOrReuse(ref _tween, rootControl, (Tween t) => {
            t.Parallel()
                .TweenProperty(
                    @object: rootControl,
                    property: Control.PropertyName.Position.ToString(),
                    finalVal: inAnim ? _originalRootControlPosition : displacedPosition,
                    duration: duration)
                .SetEase(Tween.EaseType.Out);

            t.Parallel()
                .TweenProperty(
                    @object: rootControl,
                    property: CanvasItem.PropertyName.Modulate.ToString(),
                    finalVal: inAnim ? Colors.White : transparent,
                    duration: duration)
                .SetEase(Tween.EaseType.Out);

            t.Parallel()
                .TweenProperty(
                    @object: rootControl,
                    property: Control.PropertyName.Scale.ToString(),
                    finalVal: inAnim ? Vector2.One : hiddenScale.AsVector2(),
                    duration: duration)
                .SetEase(Tween.EaseType.Out);

            if (completionCallback is null)
                return;

            t.TweenInterval(duration);
            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    #endregion
}