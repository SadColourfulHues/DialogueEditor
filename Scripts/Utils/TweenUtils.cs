namespace ScriptEditor;

public static class TweenUtils
{
    public static void StartOrReuse(ref Tween tween, Node owner, Action<Tween> animActionCallback)
    {
        if (GodotObject.IsInstanceValid(tween) && tween.IsRunning()) {
            tween.Kill();
        }

        tween = owner.CreateTween();
        animActionCallback.Invoke(tween);
    }

    public static void Kill(ref Tween tween)
    {
        if (!GodotObject.IsInstanceValid(tween) || !tween.IsRunning())
            return;

        tween.Kill();
        tween = null;
    }

    public static void StartTypewriter(ref Tween tween, Label label, float wordsPerSecond = 60)
    {
        float duration = GetDurationForWordCount(label.Text, wordsPerSecond);

        // Start typewriter animation
        label.VisibleRatio = 0.0f;

        StartOrReuse(ref tween, label, (Tween t) =>
            t.TweenProperty(
                @object: label,
                property: Label.PropertyName.VisibleRatio.ToString(),
                finalVal: 1.0f,
                duration: duration
            )
        );
    }

    public static void StartTypewriter(ref Tween tween, RichTextLabel label, float wordsPerSecond = 60)
    {
        float duration = GetDurationForWordCount(label.Text, wordsPerSecond);

        // Start typewriter animation
        label.VisibleRatio = 0.0f;

        StartOrReuse(ref tween, label, (Tween t) =>
            t.TweenProperty(
                @object: label,
                property: Label.PropertyName.VisibleRatio.ToString(),
                finalVal: 1.0f,
                duration: duration
            )
        );
    }

    public static void StartScaleIn(ref Tween tween,
                                    Control item,
                                    float duration,
                                    Action completionCallback = null)
    {
        item.Scale = Vector2.Zero;

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: Control.PropertyName.Scale.ToString(),
                finalVal: Vector2.One,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    public static void StartScaleOut(ref Tween tween,
                                    Control item,
                                    float duration,
                                    Action completionCallback = null)
    {
        item.Scale = Vector2.One;

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: Control.PropertyName.Scale.ToString(),
                finalVal: Vector2.Zero,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    public static void StartFadeIn(ref Tween tween,
                                    CanvasItem item,
                                    float duration,
                                    Action completionCallback = null)
    {
        item.Modulate = new(1.0f, 1.0f, 1.0f, 0.0f);

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: CanvasItem.PropertyName.Modulate.ToString(),
                finalVal: Colors.White,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    public static void StartFadeOut(ref Tween tween,
                                    CanvasItem item,
                                    float duration,
                                    Action completionCallback = null)
    {
        Color finalColour = new(1.0f, 1.0f, 1.0f, 0.0f);
        item.Modulate = Colors.White;

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: CanvasItem.PropertyName.Modulate.ToString(),
                finalVal: finalColour,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    public static void StartSlideDown(ref Tween tween,
                                    Control item,
                                    float duration,
                                    Action completionCallback = null)
    {
        item.AnchorTop = 0.0f;
        item.AnchorBottom = 0.0f;

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: Control.PropertyName.AnchorBottom.ToString(),
                finalVal: 1.0f,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    public static void StartSlideUp(ref Tween tween,
                                    Control item,
                                    float duration,
                                    Action completionCallback = null)
    {
        item.AnchorTop = 0.0f;
        item.AnchorBottom = 1.0f;

        StartOrReuse(ref tween, item, (t) => {
            t.TweenProperty(
                @object: item,
                property: Control.PropertyName.AnchorBottom.ToString(),
                finalVal: 0.0f,
                duration: duration
            );

            if (completionCallback is null)
                return;

            t.TweenCallback(Callable.From(completionCallback));
        });
    }

    private static float GetDurationForWordCount(ReadOnlySpan<char> text, float wordsPerSecond)
    {
        // Estimate word count by counting the number of whitespaces (accuracy notwithstanding)
        bool previousIsWhitespace = false;
        float duration = 0;

        for (int i = 0; i < text.Length; ++i) {
            if (!char.IsWhiteSpace(text[i])) {
                previousIsWhitespace = false;
                continue;
            }

            if (previousIsWhitespace)
                continue;

            duration ++;
            previousIsWhitespace = true;
        }

        return duration / wordsPerSecond;
    }
}