using SadChromaLib.UI;

namespace ScriptEditor;

public partial class FancyModalView: DialogView
{
    Tween _tween;

    public override async void _EnterTree()
    {
        Visible = true;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Control rootControl = GetRootControl();
        rootControl.PivotOffset = rootControl.Size / 2;

        base.SetVisibility(false);
    }

    public override void SetVisibility(bool visible)
    {
        if (visible) {
            TweenUtils.StartScaleIn(ref _tween, GetRootControl(), 0.33f);
            base.SetVisibility(true);
        }
        else {
            ResignActive();

            TweenUtils.StartScaleOut(ref _tween, GetRootControl(), 0.33f, () =>
                base.SetVisibility(false));
        }
    }
}