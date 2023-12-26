using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using Lift.UI.Test.Controls.PropertyGrid;

namespace Lift.UI.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = new MainViewModel();

        LiftUiPropertyGrid.SelectedObject = vm;
        PdPropertyGrid.SelectedObject = vm;

        //Task.Run(() =>
        //{
        //    Thread.Sleep(5000);

        //    Application.Current.Dispatcher.Invoke(() =>
        //    {
        //        vm.Name = "xxxx";
        //    });

        //    MessageBox.Show("ok");

        //    Thread.Sleep(3000);

        //    MessageBox.Show(vm.Name);
        //});


        foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(vm.GetType()))
        {
            var name = prop.Name;
            var ats = prop.GetAttribute<PropertyGridAttribute>()?.Ignore;
        }


        var ss = vm.GetMembers();

        foreach (var info in ss)
        {
            var name = info.Name;

            var sss = info.IsGeneratedCode();
            var s = info.GetCustomAttributes();
        }
    }
}

public static class PropertyDescriptorExtension
{
    public static T? GetAttribute<T>(this PropertyDescriptor pd)
        => (T?) (object?) pd.Attributes.OfType<Attribute>().FirstOrDefault(x => x.GetType() == typeof(T));
}


public partial class MainViewModel : ObservableObject
{
    [PropertyGrid]
    [ObservableProperty]
    private int _age = 1;

    [PropertyGrid]
    [ObservableProperty]
    private string _name = "kitty";

    [ObservableProperty]
    private bool _male = true;

    [PropertyGrid(Ignore = true)]
    public string Demo { get; set; } = "hellos";
}


/// <summary>
/// 专门针对ViewModel的反射帮助类
/// </summary>
public static class ViewModelReflectionHelper
{
    /// <summary>
    /// 
    /// </summary>
    private const string MvvmToolkitsClassName = "ObservableObject";

    /// <summary>
    /// 配合CommunicateMvvmToolkits
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string Field2Prop(this string name)
        => $"{char.ToUpper(name.Replace("_", "")[0])}{name.Replace("_", "")[1..]}";

    /// <summary>
    /// 判断有没有继承ObservableObject
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsObservableObject(this object obj)
        => obj.GetType().BaseType?.Name == MvvmToolkitsClassName;

    /// <summary>
    /// 新的GetProperties方式，这里面
    /// </summary>
    /// <returns></returns>
    public static MemberInfo[] GetMembers(this object obj)
        => obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(member => member.MemberType is MemberTypes.Field or MemberTypes.Property)
            .Where(member => !member.IsNeedSkip(obj)).ToArray();

    /// <summary>
    /// 当前这个成员是否需要跳过不看
    /// 尤其是自动生成这一部分
    /// </summary>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsNeedSkip(this MemberInfo info, object obj)
        => obj.IsObservableObject() && (info.IsGeneratedCode() || info.IsDebuggerBrowsable());

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static PropertyGridAttribute? IsPropertyGrid(this MemberInfo info)
        => info.GetCustomAttribute<PropertyGridAttribute>();

    /// <summary>
    /// 判断是不是GeneratedCodeAttribute数据对象
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsGeneratedCode(this MemberInfo info)
        => info.GetCustomAttribute<System.CodeDom.Compiler.GeneratedCodeAttribute>() is not null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static bool IsDebuggerBrowsable(this MemberInfo info)
        => info.GetCustomAttribute<System.Diagnostics.DebuggerBrowsableAttribute>() is not null;
}
