using System.Collections;

namespace Quant.NET.DataFrame;

public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _dictionary;
    private readonly List<TKey> _keys;

    public OrderedDictionary()
    {
        _dictionary = new Dictionary<TKey, TValue>();
        _keys = new List<TKey>();
    }

    public TValue this[TKey key]
    {
        get => _dictionary[key];
        set
        {
            if (!_dictionary.ContainsKey(key))
            {
                _keys.Add(key);
            }
            _dictionary[key] = value;
        }
    }

    public ICollection<TKey> Keys => _keys.AsReadOnly();

    public ICollection<TValue> Values
    {
        get
        {
            List<TValue> values = new List<TValue>();
            foreach (var key in _keys)
            {
                values.Add(_dictionary[key]);
            }
            return values.AsReadOnly();
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return Remove(item.Key);
    }

    public int Count => _dictionary.Count;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        if (!_dictionary.ContainsKey(key))
        {
            _keys.Add(key);
        }
        _dictionary.Add(key, value);
    }

    public bool Remove(TKey key)
    {
        if (_dictionary.Remove(key))
        {
            _keys.Remove(key);
            return true;
        }
        return false;
    }

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

    public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

    public void Clear()
    {
        _dictionary.Clear();
        _keys.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.ContainsKey(item.Key) && _dictionary[item.Key].Equals(item.Value);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        foreach (var key in _keys)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var key in _keys)
        {
            yield return new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class OrderedDictionaryExtensions
{
    public static OrderedDictionary<TKey, TValue> ToOrderedDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector)
    {
        var orderedDictionary = new OrderedDictionary<TKey, TValue>();

        foreach (var item in source)
        {
            orderedDictionary.Add(keySelector(item), valueSelector(item));
        }

        return orderedDictionary;
    }
}