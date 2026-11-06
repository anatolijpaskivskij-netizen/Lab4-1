using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileDatabaseApp
{
    // Власне виключення для перевірки полів файлу
    public class InvalidFileFieldException : Exception
    {
        public InvalidFileFieldException(string message) : base(message) { }
    }

    public class FileRecord
    {
        // Закриті поля класу (інкапсуляція)
        private string fileName;
        private string extension;
        private long size;
        private DateTime creationDate;
        private string attributes;

        // Властивості з валідацією через get та set
        public string FileName
        {
            get => fileName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidFileFieldException("Ім'я файла не може бути порожнім.");
                fileName = value.Trim();
            }
        }

        public string Extension
        {
            get => extension;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidFileFieldException("Розширення не може бути порожнім.");
                extension = value.Trim().StartsWith(".") ? value.Trim() : "." + value.Trim();
            }
        }

        public long Size
        {
            get => size;
            set
            {
                if (value < 0)
                    throw new InvalidFileFieldException("Розмір файла не може бути від'ємним.");
                size = value;
            }
        }

        public DateTime CreationDate
        {
            get => creationDate;
            set
            {
                if (value > DateTime.Now)
                    throw new InvalidFileFieldException("Дата створення не може бути в майбутньому.");
                creationDate = value;
            }
        }

        public string Attributes
        {
            get => attributes;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidFileFieldException("Атрибути не можуть бути порожніми.");
                attributes = value.Trim();
            }
        }

        // Конструктор без параметрів
        public FileRecord()
        {
            fileName = "default";
            extension = ".txt";
            size = 0;
            creationDate = DateTime.Now;
            attributes = "Archive";
        }

        // Конструктор з параметрами (ініціалізація через властивості)
        public FileRecord(string fileName, string extension, long size, DateTime creationDate, string attributes)
        {
            FileName = fileName;
            Extension = extension;
            Size = size;
            CreationDate = creationDate;
            Attributes = attributes;
        }

        // Перетворення об'єкта в рядок для збереження у файл
        public override string ToString()
        {
            return $"{FileName};{Extension};{Size};{CreationDate:yyyy-MM-dd HH:mm:ss};{Attributes}";
        }

        // Створення об'єкта з рядка файлу
        public static FileRecord FromString(string line)
        {
            string[] parts = line.Split(';');
            if (parts.Length != 5) throw new FormatException("Невірний формат рядка файлу.");

            return new FileRecord(
                parts[0],
                parts[1],
                long.Parse(parts[2]),
                DateTime.Parse(parts[3]),
                parts[4]
            );
        }
    }

    public class DatabaseManager
    {
        private List<FileRecord> records;
        private string dbFilePath;

        public DatabaseManager(string dbFilePath)
        {
            this.dbFilePath = dbFilePath;
            records = new List<FileRecord>();
            LoadFromFile();
            // Якщо файл порожній або відсутній, створюємо 10 дефолтних записів
            if (records.Count == 0)
            {
                CreateDefaultRecords();
                SaveToFile();
            }
        }

        // Створення не менше 10 початкових записів
        private void CreateDefaultRecords()
        {
            records.Add(new FileRecord("document", ".docx", 15400, new DateTime(2025, 5, 10, 14, 30, 0), "Archive"));
            records.Add(new FileRecord("image", ".png", 2048000, new DateTime(2026, 1, 15, 10, 20, 0), "Readonly"));
            records.Add(new FileRecord("script", ".cs", 4500, new DateTime(2026, 3, 1, 18, 0, 0), "Archive"));
            records.Add(new FileRecord("archive", ".zip", 10485760, new DateTime(2024, 11, 20, 9, 15, 0), "Hidden"));
            records.Add(new FileRecord("notes", ".txt", 1200, new DateTime(2026, 4, 5, 12, 0, 0), "Archive"));
            records.Add(new FileRecord("table", ".xlsx", 32000, new DateTime(2025, 8, 19, 16, 45, 0), "Archive"));
            records.Add(new FileRecord("presentation", ".pptx", 5120000, new DateTime(2025, 12, 1, 11, 10, 0), "Readonly"));
            records.Add(new FileRecord("song", ".mp3", 8388608, new DateTime(2023, 6, 22, 20, 0, 0), "Archive"));
            records.Add(new FileRecord("movie", ".mp4", 1073741824, new DateTime(2025, 2, 14, 22, 30, 0), "Hidden"));
            records.Add(new FileRecord("page", ".html", 3400, new DateTime(2026, 3, 20, 8, 50, 0), "Archive"));
        }

        // Виведення інформації з файла на екран (завантаження)
        public void LoadFromFile()
        {
            if (!File.Exists(dbFilePath)) return;
            records.Clear();
            string[] lines = File.ReadAllLines(dbFilePath, Encoding.UTF8);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    try
                    {
                        records.Add(FileRecord.FromString(line));
                    }
                    catch { }
                }
            }
        }

        // Збереження у текстовий файл
        public void SaveToFile()
        {
            List<string> lines = new List<string>();
            foreach (var record in records)
            {
                lines.Add(record.ToString());
            }
            File.WriteAllLines(dbFilePath, lines, Encoding.UTF8);
        }

        // Додавання запису
        public void AddRecord(FileRecord record)
        {
            records.Add(record);
            SaveToFile();
        }

        // Редагування запису
        public void EditRecord(int index, FileRecord record)
        {
            if (index < 0 || index >= records.Count)
                throw new ArgumentOutOfRangeException("Індексу не існує.");
            records[index] = record;
            SaveToFile();
        }

        // Знищення (видалення) запису
        public void DeleteRecord(int index)
        {
            if (index < 0 || index >= records.Count)
                throw new ArgumentOutOfRangeException("Індексу не існує.");
            records.RemoveAt(index);
            SaveToFile();
        }

        public List<FileRecord> GetAllRecords()
        {
            return records;
        }

        // Сортування за параметром сортування варіанта 8 (Розширення)
        public void SortByExtension()
        {
            records = records.OrderBy(r => r.Extension).ThenBy(r => r.FileName).ToList();
            SaveToFile();
        }

        // Пошук за параметром пошуку варіанта 8 (Дата створення)
        public List<FileRecord> SearchByCreationDate(DateTime targetDate)
        {
            return records.Where(r => r.CreationDate.Date == targetDate.Date).ToList();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            string dbFile = "files_db.txt";
            DatabaseManager db = new DatabaseManager(dbFile);

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== БАЗА ДАНИХ: СПИСОК ФАЙЛІВ (Варіант 8) ===");
                Console.WriteLine("[о] - Вивести всі записи на екран з файла");
                Console.WriteLine("[д] - Додати новий запис");
                Console.WriteLine("[р] - Редагувати запис");
                Console.WriteLine("[в] - Знищити (видалити) запис");
                Console.WriteLine("[с] - Сортувати за розширенням (параметр сортування)");
                Console.WriteLine("[п] - Пошук за датою створення (параметр пошуку)");
                Console.WriteLine("[Enter] - Вихід");
                Console.Write("\nОберіть дію (натисніть відповідну клавішу): ");

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                Console.WriteLine();

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    break;
                }

                char key = char.ToLower(keyInfo.KeyChar);

                try
                {
                    switch (key)
                    {
                        case 'о':
                            PrintRecords(db.GetAllRecords());
                            break;

                        case 'д':
                            FileRecord newRec = InputFileRecord();
                            db.AddRecord(newRec);
                            Console.WriteLine("\nЗапис успішно додано!");
                            break;

                        case 'р':
                            PrintRecords(db.GetAllRecords());
                            Console.Write("\nВведіть номер запису для редагування (індекс починається з 0): ");
                            if (int.TryParse(Console.ReadLine(), out int editIdx))
                            {
                                FileRecord editedRec = InputFileRecord();
                                db.EditRecord(editIdx, editedRec);
                                Console.WriteLine("\nЗапис успішно оновлено!");
                            }
                            else
                            {
                                Console.WriteLine("Некоректний індекс.");
                            }
                            break;

                        case 'в':
                            PrintRecords(db.GetAllRecords());
                            Console.Write("\nВведіть номер запису для видалення: ");
                            if (int.TryParse(Console.ReadLine(), out int delIdx))
                            {
                                db.DeleteRecord(delIdx);
                                Console.WriteLine("\nЗапис успішно видалено!");
                            }
                            else
                            {
                                Console.WriteLine("Некоректний індекс.");
                            }
                            break;

                        case 'с':
                            db.SortByExtension();
                            Console.WriteLine("\nБазу даних успішно впорядковано за розширенням!");
                            PrintRecords(db.GetAllRecords());
                            break;

                        case 'п':
                            Console.Write("Введіть дату створення для пошуку (у форматі РРРР-ММ-ДД): ");
                            if (DateTime.TryParse(Console.ReadLine(), out DateTime searchDate))
                            {
                                var found = db.SearchByCreationDate(searchDate);
                                Console.WriteLine($"\nРезультати пошуку за датою {searchDate:yyyy-MM-dd}:");
                                PrintRecords(found);
                            }
                            else
                            {
                                Console.WriteLine("Некоректний формат дати.");
                            }
                            break;

                        default:
                            Console.WriteLine("Невідома команда. Спробуйте ще раз.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nПомилка: {ex.Message}");
                }

                Console.WriteLine("\nНатисніть будь-яку клавішу, щоб продовжити...");
                Console.ReadKey();
            }
        }

        static void PrintRecords(List<FileRecord> list)
        {
            if (list.Count == 0)
            {
                Console.WriteLine("Список порожній.");
                return;
            }

            Console.WriteLine(new string('-', 95));
            Console.WriteLine($"| {"№",-3} | {"Ім'я файла",-15} | {"Розширення",-12} | {"Розмір (байт)",-15} | {"Дата створення",-20} | {"Атрибути",-12} |");
            Console.WriteLine(new string('-', 95));

            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                Console.WriteLine($"| {i,-3} | {r.FileName,-15} | {r.Extension,-12} | {r.Size,-15} | {r.CreationDate,-20:yyyy-MM-dd HH:mm:ss} | {r.Attributes,-12} |");
            }
            Console.WriteLine(new string('-', 95));
        }

        static FileRecord InputFileRecord()
        {
            Console.Write("Введіть ім'я файла: ");
            string name = Console.ReadLine();

            Console.Write("Введіть розширення (наприклад, txt або .txt): ");
            string ext = Console.ReadLine();

            Console.Write("Введіть розмір у байтах: ");
            if (!long.TryParse(Console.ReadLine(), out long size))
            {
                throw new InvalidFileFieldException("Розмір має бути цілим числом.");
            }

            Console.Write("Введіть дату створення (у форматі РРРР-ММ-ДД ГГ:ХХ:СС або просто РРРР-ММ-ДД): ");
            if (!DateTime.TryParse(Console.ReadLine(), out DateTime date))
            {
                throw new InvalidFileFieldException("Невірний формат дати.");
            }

            Console.Write("Введіть атрибути (наприклад, Archive): ");
            string attrs = Console.ReadLine();

            return new FileRecord(name, ext, size, date, attrs);
        }
    }
}