using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Lift.UI.Core.Tests.Helper;


public class WindowHelperTests
{
    private readonly ITestOutputHelper _output;

    public WindowHelperTests(ITestOutputHelper output)
    {
        _output = output;
    }


    [Fact]
    public void CanCallCenterTo()
    {
        _output.WriteLine("测试通过");
        var sta = new Thread(new ThreadStart(() =>
        {
            var window = new Window()
            {
                Width = 100,
                Height = 100
            };

            var src = new Window()
            {
                Width = 50,
                Height = 50
            };
            Action<Window> action = s => { };

            src.CenterTo(window, action);

            _output.WriteLine($"{src.Width}");
        }));

        sta.SetApartmentState(ApartmentState.STA);
        sta.IsBackground = true;
        sta.Start();

        Console.WriteLine("ok");

        Thread.Sleep(1000);
    }
}
