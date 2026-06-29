using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using X.SuperResolution.ViewModels;

namespace X.SuperResolution;

public class ViewLocator : IDataTemplate
{
    private static readonly ConcurrentDictionary<Type, Type> _cache = [];

    private static readonly Assembly _current_assembly = Assembly.GetExecutingAssembly();

    public Control Build(object param)
    {
        if (param is null)
        {
            return null;
        }

        var view_model_type = param.GetType();

        var view_type = _cache.GetOrAdd(view_model_type, type =>
        {
            var name_space = type.Namespace;
            if (name_space is null) return null;

            var name = type.Name;
            var view_name_space = name_space.Replace("ViewModel", "View", StringComparison.Ordinal);

            var view_name = name.Replace("ViewModel", "Page", StringComparison.Ordinal);
            var full_name = $"{view_name_space}.{view_name}";

            var view = _current_assembly.GetType(full_name);

            if (view is not null) return view;

            view_name = name.Replace("ViewModel", "Content", StringComparison.Ordinal);
            view = _current_assembly.GetType($"{view_name_space}.{view_name}");

            return view;
        });

        if (view_type is null) return new TextBlock { Text = "View not found: " + view_model_type.FullName };

        var control = (Control)Activator.CreateInstance(view_type)!;
        control.DataContext = param;

        control.Unloaded += (s, e) =>
        {
            if (control.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            control.DataContext = null;
        };

        return control;
    }

    public bool Match(object data)
    {
        return data is ViewModelBase;
    }
}