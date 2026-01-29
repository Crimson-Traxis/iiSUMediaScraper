using System.Collections;
using System.Collections.Specialized;

namespace iiSUMediaScraper.ObservableModels;

public class BoundCollection<T>
{
    private readonly List<Action<NotifyCollectionChangedEventArgs>> _actions;

    private readonly List<string> _properties;

    public BoundCollection(ICollection? observableCollection, IEnumerable? originalCollection)
    {
        ObservableCollection = observableCollection;
        OriginalCollection = originalCollection;

        _actions = [];
        _properties = [];
    }

    public void AddCallback(Action<NotifyCollectionChangedEventArgs> action)
    {
        _actions.Add(action);
    }

    public void ClearCallbacks()
    {
        _actions.Clear();
    }

    public void AddOnPropertyChanged(string propertyName)
    {
        _properties.Add(propertyName);
    }

    public void ClearPropertyChanges()
    {
        _properties.Clear();
    }

    public ICollection? ObservableCollection { get; private set; }

    public IEnumerable? OriginalCollection { get; private set; }

    public IEnumerable<Action<NotifyCollectionChangedEventArgs>> Actions => _actions.ToList();

    public IEnumerable<string> Properties => _properties;
}
