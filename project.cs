using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TaskHub
{
    class TaskItem
    {
        public int Id;
        public string Title = "";
        public string Description = "";
        public string Priority = "Low";
        public DateTime Deadline;
        public string Status = "New";
        public override string ToString()
        {
            return Id + ". " + Title + " [" + Priority + ", " + Status + ", до " +
                   Deadline.ToString("yyyy-MM-dd HH:mm") + "] - " + Description;
        }
    }

    delegate bool Check(TaskItem t);

    class Store<T>
    {
        public List<T> Items = new List<T>();
    }

    class Saver : IDisposable
    {
        public async Task SaveAsync(List<TaskItem> tasks, string path)
        {
            using (var w = new StreamWriter(path))
            {
                foreach (var t in tasks)
                    await w.WriteLineAsync(t.Id + ";" + t.Title + ";" + t.Description + ";" +
                                            t.Priority + ";" + t.Deadline.ToString("o") + ";" + t.Status);
            }
        }

        public async Task<List<TaskItem>> LoadAsync(string path)
        {
            var r = new List<TaskItem>();
            if (!File.Exists(path)) return r;
            using (var rd = new StreamReader(path))
            {
                string? line;
                while ((line = await rd.ReadLineAsync()) != null)
                {
                    var p = line.Split(';');
                    r.Add(new TaskItem
                    {
                        Id = int.Parse(p[0]),
                        Title = p[1],
                        Description = p[2],
                        Priority = p[3],
                        Deadline = DateTime.Parse(p[4]),
                        Status = p[5]
                    });
                }
            }
            return r;
        }

        public void Dispose() { }
    }

    class Program
    {
        static Store<TaskItem> store = new Store<TaskItem>();
        static int nextId = 1;
        static string file = "tasks.txt";
        static async Task Main()
        {
            var cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    foreach (var t in store.Items)
                        if (t.Status != "Done" && t.Deadline < DateTime.Now)
                            Console.WriteLine("\n[!] Просрочено: " + t.Title);
                    await Task.Delay(5000);
                }
            });
            while (true)
            {
                Console.WriteLine("\n1.Создать 2.Показать 3.Редактировать 4.Удалить 5.Поиск 6.Статистика 7.Сохранить 8.Загрузить 9.Выход");
                string? c = Console.ReadLine();
                try
                {
                    if (c == "1") Create();
                    else if (c == "2") Show();
                    else if (c == "3") Edit();
                    else if (c == "4") Delete();
                    else if (c == "5") Search();
                    else if (c == "6") StatsPrint();
                    else if (c == "7") await SaveAsync();
                    else if (c == "8") await LoadAsync();
                    else if (c == "9") { cts.Cancel(); break; }
                }
                catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
            }
        }

        static void Create()
        {
            var t = new TaskItem();
            t.Id = nextId++;
            Console.Write("Название: "); t.Title = Console.ReadLine() ?? "";
            Console.Write("Описание: "); t.Description = Console.ReadLine() ?? "";
            Console.Write("Приоритет (Low/Medium/High): "); t.Priority = Console.ReadLine() ?? "Low";
            Console.Write("Дедлайн (yyyy-MM-dd HH:mm): "); t.Deadline = DateTime.Parse(Console.ReadLine() ?? "");
            Console.Write("Статус (New/InProgress/Done): "); t.Status = Console.ReadLine() ?? "New";
            store.Items.Add(t);
        }

        static void Show()
        {
            Console.WriteLine("1.Все 2.Выполненные 3.Невыполненные 4.Высокий приоритет");
            string? c = Console.ReadLine();
            Check f = t => true;
            if (c == "2") f = t => t.Status == "Done";
            else if (c == "3") f = t => t.Status != "Done";
            else if (c == "4") f = t => t.Priority == "High";
            foreach (var t in store.Items) if (f(t)) Console.WriteLine(t);
        }

        static void Edit()
        {
            Console.Write("Id: "); int id = int.Parse(Console.ReadLine() ?? "0");
            foreach (var t in store.Items)
            {
                if (t.Id == id)
                {
                    Console.Write("Название: "); t.Title = Console.ReadLine() ?? t.Title;
                    Console.Write("Описание: "); t.Description = Console.ReadLine() ?? t.Description;
                    Console.Write("Приоритет: "); t.Priority = Console.ReadLine() ?? t.Priority;
                    Console.Write("Статус: "); t.Status = Console.ReadLine() ?? t.Status;
                    return;
                }
            }
            Console.WriteLine("Не найдено");
        }

        static void Delete()
        {
            Console.Write("Id: "); int id = int.Parse(Console.ReadLine() ?? "0");
            store.Items.RemoveAll(t => t.Id == id);
        }

        static void Search()
        {
            Console.WriteLine("1.Название 2.Статус 3.Приоритет");
            string? c = Console.ReadLine();
            Console.Write("Значение: "); string q = Console.ReadLine() ?? "";
            Check f = t => false;
            if (c == "1") f = t => t.Title.Contains(q);
            else if (c == "2") f = t => t.Status == q;
            else if (c == "3") f = t => t.Priority == q;
            foreach (var t in store.Items) if (f(t)) Console.WriteLine(t);
        }

        static void StatsPrint()
        {
            int done = 0, overdue = 0;
            var byPrio = new Dictionary<string, int>();
            foreach (var t in store.Items)
            {
                if (t.Status == "Done") done++;
                if (t.Status != "Done" && t.Deadline < DateTime.Now) overdue++;
                if (!byPrio.ContainsKey(t.Priority)) byPrio[t.Priority] = 0;
                byPrio[t.Priority]++;
            }
            Console.WriteLine("Всего: " + store.Items.Count);
            Console.WriteLine("Выполнено: " + done);
            Console.WriteLine("Просрочено: " + overdue);
            foreach (var p in byPrio) Console.WriteLine(p.Key + ": " + p.Value);
        }

        static async Task SaveAsync()
        {
            using (var s = new Saver()) await s.SaveAsync(store.Items, file);
            Console.WriteLine("Сохранено");
        }

        static async Task LoadAsync()
        {
            using (var s = new Saver())
            {
                store.Items = await s.LoadAsync(file);
                foreach (var t in store.Items) if (t.Id >= nextId) nextId = t.Id + 1;
            }
            Console.WriteLine("Загружено");
        }
    }
}
