using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentDatabaseTask
{
    public class InvalidGradeException : Exception
    {
        public InvalidGradeException(string message) : base(message) { }
    }

    public class InvalidRecordException : Exception
    {
        public InvalidRecordException(string message) : base(message) { }
    }

    public class Student
    {
        private string fullName;
        private string recordBookNumber;
        private int[] grades;

        public string FullName => fullName;
        public string RecordBookNumber => recordBookNumber;
        public int[] Grades => (int[])grades.Clone();

        public double AverageGrade
        {
            get
            {
                double sum = 0;
                foreach (int grade in grades)
                {
                    sum += grade;
                }
                return sum / grades.Length;
            }
        }

        public Student(string fullName, string recordBookNumber, int[] grades)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new InvalidRecordException("Прізвище та ім'я не можуть бути порожніми.");
            }

            if (string.IsNullOrWhiteSpace(recordBookNumber))
            {
                throw new InvalidRecordException("Номер залікової книжки не може бути порожнім.");
            }

            if (grades == null || grades.Length != 5)
            {
                throw new InvalidRecordException("Повинно бути рівно 5 оцінок.");
            }

            foreach (int grade in grades)
            {
                if (grade < 0 || grade > 100)
                {
                    throw new InvalidGradeException($"Оцінка {grade} є некоректною. Має бути в діапазоні від 0 до 100.");
                }
            }

            this.fullName = fullName;
            this.recordBookNumber = recordBookNumber;
            this.grades = grades;
        }

        public bool HasUnsatisfactoryGrades()
        {
            foreach (int grade in grades)
            {
                if (grade < 60)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class StudentGroupManager
    {
        private List<Student> students;

        public StudentGroupManager()
        {
            students = new List<Student>();
        }

        public void AddStudent(Student student)
        {
            students.Add(student);
        }

        public void LoadDefaultStudents()
        {
            students.Clear();
            students.Add(new Student("Коваленко Максим", "ЗК-101", new int[] { 85, 90, 78, 92, 88 }));
            students.Add(new Student("Шевченко Ірина", "ЗК-102", new int[] { 55, 62, 70, 58, 65 }));
            students.Add(new Student("Мельник Андрій", "ЗК-103", new int[] { 95, 98, 92, 96, 90 }));
            students.Add(new Student("Бойко Тетяна", "ЗК-104", new int[] { 72, 68, 75, 80, 77 }));
            students.Add(new Student("Кравченко Дмитро", "ЗК-105", new int[] { 45, 50, 58, 60, 52 }));
            students.Add(new Student("Ковальчук Олена", "ЗК-106", new int[] { 88, 85, 90, 92, 86 }));
            students.Add(new Student("Павленко Сергій", "ЗК-107", new int[] { 78, 80, 82, 75, 79 }));
            students.Add(new Student("Лисенко Марія", "ЗК-108", new int[] { 60, 65, 59, 62, 63 }));
            students.Add(new Student("Романенко Василь", "ЗК-109", new int[] { 92, 90, 95, 94, 91 }));
            students.Add(new Student("Ткаченко Наталія", "ЗК-110", new int[] { 48, 55, 50, 52, 59 }));
        }

        public void SortByAverageGradeAscending()
        {
            students = students.OrderBy(s => s.AverageGrade).ToList();
        }

        public void PrintTable()
        {
            Console.WriteLine(new string('-', 95));
            Console.WriteLine($"| {"Прізвище та ім’я",-25} | {"№ залікової",-12} | {"Предмети (1-5)",-30} | {"Середній бал",-12} |");
            Console.WriteLine(new string('-', 95));

            foreach (var student in students)
            {
                string gradesStr = string.Join(", ", student.Grades);
                Console.WriteLine($"| {student.FullName,-25} | {student.RecordBookNumber,-12} | {gradesStr,-30} | {student.AverageGrade,12:F2} |");
            }
            Console.WriteLine(new string('-', 95));
        }

        public double GetUnsatisfactoryPercentage()
        {
            if (students.Count == 0) return 0;

            int countWithUnsatisfactory = 0;
            foreach (var student in students)
            {
                if (student.HasUnsatisfactoryGrades())
                {
                    countWithUnsatisfactory++;
                }
            }

            return (double)countWithUnsatisfactory / students.Count * 100;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            StudentGroupManager manager = new StudentGroupManager();
            manager.LoadDefaultStudents();

            while (true)
            {
                Console.WriteLine("\n=== БАЗА ДАНИХ УСПІШНОСТІ СТУДЕНТІВ ===");
                Console.WriteLine("1. Вивести студентів, впорядкованих за зростанням середнього балу (таблиця)");
                Console.WriteLine("2. Визначити відсоток студентів з незадовільними оцінками (<60)");
                Console.WriteLine("3. Додати нового студента вручну (з перевіркою виключень)");
                Console.WriteLine("0. Вихід");
                Console.Write("Ваш вибір: ");

                string choice = Console.ReadLine();

                if (choice == "0") break;

                switch (choice)
                {
                    case "1":
                        manager.SortByAverageGradeAscending();
                        Console.WriteLine("\nТаблиця студентів (за зростанням середнього балу):");
                        manager.PrintTable();
                        break;

                    case "2":
                        double percent = manager.GetUnsatisfactoryPercentage();
                        Console.WriteLine($"\nВідсоток студентів, що мають хоча б одну незадовільну оцінку (<60): {percent:F2}%");
                        break;

                    case "3":
                        try
                        {
                            Console.Write("Введіть прізвище та ім'я студента: ");
                            string name = Console.ReadLine();

                            Console.Write("Введіть номер залікової книжки: ");
                            string recordBook = Console.ReadLine();

                            int[] grades = new int[5];
                            for (int i = 0; i < 5; i++)
                            {
                                Console.Write($"Введіть оцінку {i + 1} (0-100): ");
                                if (!int.TryParse(Console.ReadLine(), out grades[i]))
                                {
                                    throw new InvalidGradeException("Введено неціле число для оцінки.");
                                }
                            }

                            Student newStudent = new Student(name, recordBook, grades);
                            manager.AddStudent(newStudent);
                            Console.WriteLine("\nСтудента успішно додано до бази даних!");
                        }
                        catch (InvalidGradeException ex)
                        {
                            Console.WriteLine($"\nПомилка оцінки: {ex.Message}");
                        }
                        catch (InvalidRecordException ex)
                        {
                            Console.WriteLine($"\nПомилка запису: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"\nНепередбачена помилка: {ex.Message}");
                        }
                        break;

                    default:
                        Console.WriteLine("Некоректний вибір. Спробуйте ще раз.");
                        break;
                }
            }
        }
    }
}