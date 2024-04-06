using SadChromaLib.UI;

namespace ScriptEditor;

[GlobalClass]
public sealed partial class ModalOverlay: ColorRect
{
    Tween _tween;
    bool _cursorInside;

    public override void _EnterTree()
    {
        _cursorInside = false;
        Visible = false;

        // Bind signals //

        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;

        DialogView.OnActiveChanged += OnModalRefChanged;
    }

    public override void _ExitTree() {
        DialogView.OnActiveChanged -= OnModalRefChanged;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_cursorInside ||
            @event is not InputEventMouseButton buttonEv ||
            buttonEv.Pressed ||
            buttonEv.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        DialogView currentModal = DialogView.GetActiveDialogRef();
        currentModal?.SetVisibility(false);
    }

    #region Events

    private void OnModalRefChanged(DialogView modal)
    {
        if (modal is null) {
            TweenUtils.StartSlideUp(ref _tween, this, 0.25f, Hide);
            return;
        }

        Visible = true;

        TweenUtils.StartSlideDown(ref _tween, this, 0.25f);
    }

    private void OnMouseEnter() => _cursorInside = true;
    private void OnMouseExit() => _cursorInside = false;

    #endregion
}