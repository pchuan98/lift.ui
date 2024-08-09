// https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/repeatbutton-styles-and-templates?view=netframeworkdesktop-4.8


using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

using DependencyPropertyGenerator;

namespace Lift.UI.Controls.Button;

/// <summary>
/// 增强版本的RepeatButton
///
/// feat：
/// 1. 增加线程优先级，更快的响应速度(精度更高的interval)（会有性能问题）
/// </summary>
[DependencyProperty<int>(
    "Delay",
    DefaultValueExpression = "GetKeyboardDelay()",
    PropertyXmlDocumentation = "<summary>\nMilliseconds\n</summary>",
    Validate = true)]
[DependencyProperty<int>(
    "Interval",
    DefaultValueExpression = "GetKeyboardSpeed()",
    PropertyXmlDocumentation = "<summary>\nMilliseconds\n</summary>",
    Validate = true)]
[DependencyProperty<DispatcherPriority>("Priority", DefaultValue = DispatcherPriority.Render)]
public partial class RepeatButtonEx : ButtonBase
{
    private static partial bool IsDelayValid(int value) => value > 0;

    private static partial bool IsIntervalValid(int value) => value > 0;

    private DispatcherTimer? _timer;

    private bool _renewTimer = false;

    private void StartTimer()
    {
        if (_renewTimer)
        {
            _timer?.Stop();
            _timer = null;
            _renewTimer = false;
        }

        if (_timer is null)
        {
            _timer = new DispatcherTimer(Priority);
            _timer.Tick += new EventHandler(OnTimeout);
        }
        else if (_timer.IsEnabled) return;

        _timer.Interval = TimeSpan.FromMilliseconds(Delay);
        _timer.Start();
    }

    private void StopTimer() => _timer?.Stop();

    private void OnTimeout(object? sender, EventArgs e)
    {
        _timer!.Interval = TimeSpan.FromMilliseconds(Interval);

        if (IsPressed) OnClick();
    }

    partial void OnPriorityChanged() => _renewTimer = true;

    private static int GetKeyboardDelay()
    {
        var num = SystemParameters.KeyboardDelay;
        if (num is < 0 or > 3)
            num = 0;
        return (num + 1) * 250;
    }

    private static int GetKeyboardSpeed()
    {
        var num = SystemParameters.KeyboardSpeed;
        if (num is < 0 or > 31)
            num = 31;
        return (31 - num) * 367 / 31 + 33;
    }

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer()
        => new RepeatButtonExAutomationPeer(this);

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            UIElementAutomationPeer.CreatePeerForElement((UIElement) this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
        base.OnClick();
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (IsPressed && ClickMode != System.Windows.Controls.ClickMode.Hover)
            StartTimer();
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (ClickMode != System.Windows.Controls.ClickMode.Hover)
            StopTimer();
    }

    /// <inheritdoc />
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);

        StopTimer();
    }

    /// <inheritdoc />
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (!HandleIsMouseOverChanged()) return;
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (!HandleIsMouseOverChanged()) return;
        e.Handled = true;
    }

    private bool HandleIsMouseOverChanged()
    {
        if (ClickMode != System.Windows.Controls.ClickMode.Hover) return false;

        if (IsMouseOver) StartTimer();
        else StopTimer();
        return true;
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key != (Key) 18
            || ClickMode == System.Windows.Controls.ClickMode.Hover)
            return;
        StartTimer();
    }

    /// <inheritdoc />
    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Key == (Key) 18
            && ClickMode == System.Windows.Controls.ClickMode.Hover)
            StopTimer();

        base.OnKeyUp(e);
    }

    /// <summary>
    /// 外部调用
    /// </summary>
    internal void OnClickCall() => OnClick();
}

/// <summary>
/// 
/// </summary>
/// <param name="owner"></param>
public class RepeatButtonExAutomationPeer(RepeatButtonEx owner)
    : ButtonBaseAutomationPeer((ButtonBase) owner), IInvokeProvider
{
    /// <inheritdoc />
    protected override string GetClassNameCore()
        => "RepeatButtonEx";

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Button;

    /// <inheritdoc />
    public override object? GetPattern(PatternInterface patternInterface)
        => patternInterface == PatternInterface.Invoke ? (object) this : base.GetPattern(patternInterface);

    /// <inheritdoc />
    void IInvokeProvider.Invoke()
    {
        if (!IsEnabled()) throw new ElementNotEnabledException();
        owner.OnClickCall();
    }
}
