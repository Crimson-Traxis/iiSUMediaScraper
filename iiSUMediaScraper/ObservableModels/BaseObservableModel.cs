using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace iiSUMediaScraper.ObservableModels;

public class BaseObservableModel<T> : ObservableObject, IBaseObservableModel<T>
{
    private readonly Dictionary<string, IBoundCollectionWithUnbind> unbindLookup;

    private readonly Dictionary<string, Action> callbackLookup;

    protected readonly T _baseModel;

    public BaseObservableModel(T baseModel)
    {
        _baseModel = baseModel;

        unbindLookup = [];
    }

    protected BoundCollection<C> RegisterObservableCollection<L, C>(string propertyName, IList<L> original, ObservableCollection<C> newCollection, Func<L, C> creator, Func<C, L>? converter)
    {
        return RegisterObservableCollection(propertyName, original, newCollection, creator, null, converter);
    }

    protected BoundCollection<C> RegisterObservableCollection<L, C>(string propertyName, IList<L> original, ObservableCollection<C> newCollection, Func<L, C> creator, Action<C>? initializer, Func<C, L>? converter)
    {
        if (creator != null)
        {
            foreach (L? item in original)
            {
                C? newItem = creator(item);

                if (initializer != null)
                {
                    initializer(newItem);
                }

                newCollection.Add(newItem);
            }
        }

        original.Clear();

        foreach (C? item in newCollection)
        {
            if (converter != null)
            {
                original.Add(converter(item));
            }
        }

        var boundCollection = new BoundCollectionWithUnbind<C>(newCollection, original);

        var callback = new NotifyCollectionChangedEventHandler((sender, args) =>
        {
            original.Clear();

            foreach (C? item in newCollection)
            {
                if (converter != null)
                {
                    original.Add(converter(item));
                }
                else if (item is L l)
                {
                    original.Add(l);
                }
            }

            foreach (Action<NotifyCollectionChangedEventArgs> action in boundCollection.Actions)
            {
                action(args);
            }

            foreach (string property in boundCollection.Properties)
            {
                OnPropertyChanged(property);
            }
        });

        Action unbind = () =>
        {
            newCollection.CollectionChanged -= callback;

            boundCollection.ClearCallbacks();
            boundCollection.ClearPropertyChanges();
        };

        boundCollection.Unbind = unbind;

        if (unbindLookup.TryGetValue(propertyName, out IBoundCollectionWithUnbind? listHelper))
        {
            unbindLookup.Remove(propertyName);
        }

        unbindLookup.TryAdd(propertyName, boundCollection);

        newCollection.CollectionChanged += callback;

        return boundCollection;
    }

    protected BoundCollection<C> RegisterObservableCollection<C>(string propertyName, IList<C> original, ObservableCollection<C> newCollection)
    {
        foreach (C? item in original)
        {
            newCollection.Add(item);
        }

        var boundCollection = new BoundCollectionWithUnbind<C>(newCollection, original);

        var callback = new NotifyCollectionChangedEventHandler((sender, args) =>
        {
            original.Clear();

            foreach (C? item in newCollection)
            {
                original.Add(item);
            }

            foreach (Action<NotifyCollectionChangedEventArgs> action in boundCollection.Actions)
            {
                action(args);
            }

            foreach (string property in boundCollection.Properties)
            {
                OnPropertyChanged(property);
            }
        });

        Action unbind = () =>
        {
            newCollection.CollectionChanged -= callback;

            boundCollection.ClearCallbacks();
            boundCollection.ClearPropertyChanges();
        };

        boundCollection.Unbind = unbind;

        if (unbindLookup.TryGetValue(propertyName, out IBoundCollectionWithUnbind? listHelper))
        {
            unbindLookup.Remove(propertyName);
        }

        unbindLookup.TryAdd(propertyName, boundCollection);

        newCollection.CollectionChanged += callback;

        return boundCollection;
    }

    protected BoundCollection<C> RegisterBaseModelObservableCollection<L, C>(string propertyName, IList<L> original, ObservableCollection<C> newCollection)
        where C : IBaseObservableModel<L>
    {
        return RegisterBaseModelObservableCollection(propertyName, original, newCollection, null, null);
    }

    protected BoundCollection<C> RegisterBaseModelObservableCollection<L, C>(string propertyName, IList<L> original, ObservableCollection<C> newCollection, Func<L, C>? creator)
        where C : IBaseObservableModel<L>
    {
        return RegisterBaseModelObservableCollection(propertyName, original, newCollection, creator, null);
    }

    protected BoundCollection<C> RegisterBaseModelObservableCollection<L, C>(string propertyName, IList<L> original, ObservableCollection<C> newCollection, Func<L, C>? creator, Action<C>? initializer)
       where C : IBaseObservableModel<L>
    {
        if (creator != null)
        {
            foreach (L? item in original)
            {
                C newItem = creator(item);

                if (initializer != null)
                {
                    initializer(newItem);
                }

                newCollection.Add(newItem);
            }
        }

        original.Clear();

        foreach (C item in newCollection)
        {
            original.Add(item.BaseModel);
        }

        var boundCollection = new BoundCollectionWithUnbind<C>(newCollection, original);

        var callback = new NotifyCollectionChangedEventHandler((sender, args) =>
        {
            original.Clear();

            foreach (C item in newCollection)
            {
                original.Add(item.BaseModel);
            }

            foreach (Action<NotifyCollectionChangedEventArgs> action in boundCollection.Actions)
            {
                action(args);
            }

            foreach (string property in boundCollection.Properties)
            {
                OnPropertyChanged(property);
            }
        });

        Action unbind = () =>
        {
            newCollection.CollectionChanged -= callback;

            boundCollection.ClearCallbacks();
            boundCollection.ClearPropertyChanges();
        };

        boundCollection.Unbind = unbind;

        if (unbindLookup.TryGetValue(propertyName, out IBoundCollectionWithUnbind? listHelper))
        {
            unbindLookup.Remove(propertyName);
        }

        unbindLookup.TryAdd(propertyName, boundCollection);

        newCollection.CollectionChanged += callback;

        return boundCollection;
    }

    protected void UnRegisterCollection<C>(string propertyName, Action<C> cleanup)
    {
        if (unbindLookup.TryGetValue(propertyName, out IBoundCollectionWithUnbind? listHelper))
        {
            if (listHelper.Unbind != null)
            {
                listHelper.Unbind();
            }

            if (listHelper.ObservableCollection != null)
            {
                foreach (object? item in listHelper.ObservableCollection)
                {
                    cleanup((C)item);
                }
            }
        }
    }

    protected void UnRegisterCollection(string propertyName)
    {
        if (unbindLookup.TryGetValue(propertyName, out IBoundCollectionWithUnbind? listHelper))
        {
            if (listHelper.Unbind != null)
            {
                listHelper.Unbind();
            }
        }
    }

    public virtual T BaseModel => _baseModel;

    private interface IBoundCollectionWithUnbind
    {
        ICollection? ObservableCollection { get; }

        IEnumerable? OriginalCollection { get; }

        Action? Unbind { get; set; }
    }

    private class BoundCollectionWithUnbind<C> : BoundCollection<C>, IBoundCollectionWithUnbind
    {
        public BoundCollectionWithUnbind(ICollection? observableCollection, IEnumerable? originalCollection) : base(observableCollection, originalCollection)
        {

        }

        public Action? Unbind { get; set; }
    }
}

public interface IBaseObservableModel<T>
{
    public T BaseModel { get; }
}