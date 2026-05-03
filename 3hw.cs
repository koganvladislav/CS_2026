using System;

public interface IEntity
{
    int Id { get; }
}

public class Repository<T> where T : IEntity
{
    private Dictionary<int, T> items = new Dictionary<int, T>();
    public void Add(T item)
    {
        if (items.ContainsKey(item.Id)) throw new InvalidOperationException("Id присутствует");
        items[item.Id] = item;
    }
    public bool Remove(int id) => items.Remove(id);
    public T? GetById(int id) => items.ContainsKey(id) ? items[id] : default;
    public IReadOnlyList<T> GetAll() => new List<T>(items.Values);
    public int Count => items.Count;
    public IReadOnlyList<T> Find(Predicate<T> predicate)
    {
        var r = new List<T>();
        foreach (var x in items.Values) if (predicate(x)) r.Add(x);
        return r;
    }
}

public class Product : IEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public decimal Price { get; set; }
}

public class User : IEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public static class CollectionUtils
{
    public static List<T> Distinct<T>(List<T> source)
    {
        var r = new List<T>();
        foreach (var x in source)
        {
            if (!r.Contains(x)) r.Add(x);
        }
        return r;
    }

    public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
        List<TValue> source,
        Func<TValue, TKey> keySelector) where TKey : notnull
    {
        var r = new Dictionary<TKey, List<TValue>>();
        foreach (var x in source)
        {
            var k = keySelector(x);
            if (!r.ContainsKey(k)) r[k] = new List<TValue>();
            r[k].Add(x);
        }
        return r;
    }

    public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        Dictionary<TKey, TValue> first,
        Dictionary<TKey, TValue> second,
        Func<TValue, TValue, TValue> conflictResolver) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(first);
        foreach (var pair in second)
        {
            if (result.ContainsKey(pair.Key)) result[pair.Key] = conflictResolver(result[pair.Key], pair.Value);
            else result[pair.Key] = pair.Value;
        }
        return result;
    }

    public static T MaxBy<T, TKey>(List<T> source, Func<T, TKey> selector)
        where TKey : IComparable<TKey>
    {
        if (source.Count == 0) throw new InvalidOperationException("Пусто");
        T best = source[0];
        TKey bestKey = selector(best);
        for (int i = 1; i < source.Count; ++i)
        {
            TKey curKey = selector(source[i]);
            if (curKey.CompareTo(bestKey) > 0)
            {
                best = source[i];
                bestKey = curKey;
            }
        }
        return best;
    }
}

public class Program
{
    public static void Main()
    {
        var products = new Repository<Product>();
        products.Add(new Product { Id = 1, Title = "Хлеб", Price = 50 });
        products.Add(new Product { Id = 2, Title = "MacBook", Price = 100000 });
        products.Add(new Product { Id = 3, Title = "Молоко", Price = 150 });

        var users = new Repository<User>();
        users.Add(new User { Id = 1, Name = "Владислав" });
        users.Add(new User { Id = 2, Name = "Сергей" });

        Console.WriteLine("Id=2: " + products.GetById(2)?.Title);
        Console.WriteLine("User Id=1: " + users.GetById(1)?.Name);

        Console.WriteLine("Дороже 1000:");
        foreach (var x in products.Find(p => p.Price > 1000)) Console.WriteLine("  " + x.Title);

        try { products.Add(new Product { Id = 1, Title = "X", Price = 0 }); }
        catch (InvalidOperationException ex) { Console.WriteLine("Ошибка: " + ex.Message); }

        Console.WriteLine("Distinct: " + string.Join(" ", CollectionUtils.Distinct(new List<int> { 1, 2, 2, 3, 1, 4 })));
        Console.WriteLine("Distinct: " + string.Join(" ", CollectionUtils.Distinct(new List<string> { "a", "b", "a", "c", "a", "b", "a" })));

        var byLen = CollectionUtils.GroupBy(new List<string> { "cat", "dog", "tiger", "elephant", "mouse" }, w => w.Length);
        foreach (var p in byLen) Console.WriteLine(p.Key + ": " + string.Join(",", p.Value));

        var t1 = new Dictionary<string, int> { ["the"] = 5, ["cat"] = 2 };
        var t2 = new Dictionary<string, int> { ["the"] = 3, ["dog"] = 4 };
        foreach (var p in CollectionUtils.Merge(t1, t2, (a, b) => a + b)) Console.WriteLine(p.Key + "=" + p.Value);

        var list = new List<Product> {
            new Product { Id = 1, Title = "Хлеб", Price = 50 },
            new Product { Id = 2, Title = "MacBook", Price = 100000 },
            new Product { Id = 3, Title = "Молоко", Price = 150 }
        };
        Console.WriteLine("Самый дорогой: " + CollectionUtils.MaxBy(list, x => x.Price).Title);
    }
}
