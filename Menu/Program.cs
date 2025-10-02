using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Menu
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var db = new EmployeeContext())
            {
                InitializeDatabase(db);

                string[] menuItems = { "New", "Display", "Sort", "Search", "Delete", "Add Department", "Assign Employee to Department", "Exit" };
                int selectedItem = 0;
                bool inMenu = true;
                ConsoleKey key;

                do
                {
                    if (inMenu)
                    {
                        DisplayMenu(menuItems, selectedItem);
                        key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.UpArrow)
                        {
                            selectedItem = (selectedItem - 1 + menuItems.Length) % menuItems.Length;
                        }
                        else if (key == ConsoleKey.DownArrow)
                        {
                            selectedItem = (selectedItem + 1) % menuItems.Length;
                        }
                        else if (key == ConsoleKey.Enter)
                        {
                            switch (menuItems[selectedItem])
                            {
                                case "Exit":
                                    Console.Clear();
                                    Console.WriteLine("Exiting program...");
                                    return;
                                case "New":
                                    InputEmployeeData(db);
                                    break;
                                case "Display":
                                    DisplayEmployees(db);
                                    break;
                                case "Sort":
                                    SortEmployees(db);
                                    break;
                                case "Search":
                                    SearchEmployees(db);
                                    break;
                                case "Delete":
                                    DeleteEmployee(db);
                                    break;
                                case "Add Department":
                                    AddDepartment(db);
                                    break;
                                case "Assign Employee to Department":
                                    AssignEmployeeToDepartment(db);
                                    break;
                            }

                            inMenu = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("\nPress Enter to return to the menu.");
                        key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Enter)
                        {
                            inMenu = true;
                        }
                    }
                } while (true);
            }
        }

        static void InitializeDatabase(EmployeeContext db)
        {
            db.Database.EnsureCreated();
        }

        static void DisplayMenu(string[] menuItems, int selectedItem)
        {
            Console.Clear();
            int windowHeight = Console.WindowHeight;
            int windowWidth = Console.WindowWidth;
            int spacing = windowHeight / (menuItems.Length + 1);
            int startCol = windowWidth / 2 - 10;

            for (int i = 0; i < menuItems.Length; i++)
            {
                int rowPosition = spacing * (i + 1);
                Console.SetCursorPosition(startCol, rowPosition);
                if (i == selectedItem)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ResetColor();
                }
                Console.WriteLine(menuItems[i]);
            }
        }

        static void InputEmployeeData(EmployeeContext db)
        {
            Console.Clear();
            Console.WriteLine($"Enter Employee Data:");

            Console.Write("Enter Employee Name: ");
            string name = Console.ReadLine();

            Console.Write("Enter Employee Salary: ");
            double salary;
            while (!double.TryParse(Console.ReadLine(), out salary) || salary < 0)
            {
                Console.Write("Invalid input. Please enter a valid positive salary: ");
            }

            Console.Write("Enter Employee Gender (1 for Male, 2 for Female): ");
            Gender gender;
            int genderInput;
            while (!int.TryParse(Console.ReadLine(), out genderInput) || !Enum.IsDefined(typeof(Gender), genderInput))
            {
                Console.Write("Invalid input. Please enter 1 for Male or 2 for Female: ");
            }
            gender = (Gender)genderInput;

            Console.Write("Enter Employee Age: ");
            int age;
            while (!int.TryParse(Console.ReadLine(), out age) || age < 0)
            {
                Console.Write("Invalid input. Please enter a valid positive age: ");
            }

            Console.WriteLine("Available Departments:");
            var departments = db.Departments.ToList();
            foreach (var dept in departments)
            {
                Console.WriteLine($"{dept.Id}: {dept.Name}");
            }

            int departmentId;
            while (true)
            {
                Console.Write("Assign Employee to Department (Enter ID): ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out departmentId))
                {
                    var department = db.Departments.Find(departmentId);
                    if (department != null)
                    {
                        
                        Employee newEmployee = new Employee
                        {
                            Name = name,
                            Salary = salary,
                            Gender = gender,
                            Age = age,
                            Department = department 
                        };

                        db.Employees.Add(newEmployee);
                        db.SaveChanges();
                        Console.WriteLine("Employee data has been saved.");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Department not found. Please enter a valid Department ID.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid Department ID.");
                }
            }
        }

        static void DisplayEmployees(EmployeeContext db)
        {
            Console.Clear();
            var employees = db.Employees.OrderBy(e => e.SortOrder).Include(e => e.Department).ToList();
            foreach (var emp in employees)
            {
                emp.DisplayData();
            }
        }

        static void SearchEmployees(EmployeeContext db)
        {
            Console.Clear();
            Console.WriteLine("Search by: \n1. ID \n2. Name");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            Console.WriteLine();

            switch (keyInfo.KeyChar)
            {
                case '1':
                    int searchId;
                    while (true)
                    {
                        Console.Write("Enter Employee ID to search: ");
                        if (int.TryParse(Console.ReadLine(), out searchId) && searchId > 0)
                            break;
                        else
                            Console.WriteLine("Invalid ID. Please enter a positive integer.");
                    }

                    var foundById = db.Employees.Include(e => e.Department).FirstOrDefault(e => e.Id == searchId);
                    if (foundById != null)
                    {
                        Console.WriteLine("Employee found:");
                        DisplayEmployee(foundById);
                    }
                    else
                    {
                        Console.WriteLine("No employee found with the given ID.");
                    }
                    break;

                case '2':
                    Console.Write("Enter Employee Name to search: ");
                    string searchName = Console.ReadLine().Trim();
                    var foundByName = db.Employees.Include(e => e.Department)
                        .Where(e => e.Name.Contains(searchName)).ToList();

                    if (foundByName.Count > 0)
                    {
                        Console.WriteLine($"\nFound {foundByName.Count} employee(s) with the name '{searchName}':");
                        foreach (var emp in foundByName)
                        {
                            DisplayEmployee(emp);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No employee found with the given name.");
                    }
                    break;

                default:
                    Console.WriteLine("Invalid search option selected.");
                    break;
            }
        }

        static void DisplayEmployee(Employee employee)
        {
            Console.WriteLine($"ID: {employee.Id}");
            Console.WriteLine($"Name: {employee.Name}");
            Console.WriteLine($"Salary: {employee.Salary}");
            Console.WriteLine($"Gender: {employee.Gender}");
            Console.WriteLine($"Age: {employee.Age}");
            Console.WriteLine($"Department: {(employee.Department != null ? employee.Department.Name : "None")}");
            Console.WriteLine("--------------------------");
        }
        static void SortEmployees(EmployeeContext db)
        {
            Console.Clear();
            Console.WriteLine("Sort by: \n1. Salary \n2. Name");
            ConsoleKeyInfo sortByKey = Console.ReadKey();
            Console.WriteLine();

            Console.WriteLine("Sort Order: \n1. Ascending \n2. Descending");
            ConsoleKeyInfo sortOrderKey = Console.ReadKey();
            Console.WriteLine();

            bool ascending = sortOrderKey.KeyChar == '1';
            IQueryable<Employee> sortedEmployees;

            switch (sortByKey.KeyChar)
            {
                case '1':
                    sortedEmployees = ascending ? db.Employees.OrderBy(e => e.Salary) : db.Employees.OrderByDescending(e => e.Salary);
                    break;
                case '2':
                    sortedEmployees = ascending ? db.Employees.OrderBy(e => e.Name) : db.Employees.OrderByDescending(e => e.Name);
                    break;
                default:
                    Console.WriteLine("Invalid sort option.");
                    return;
            }

            // Update SortOrder property based on the sort
            int order = 1;
            foreach (var employee in sortedEmployees)
            {
                employee.SortOrder = order++;
            }

            db.SaveChanges();  // Save the changes to the database
            Console.WriteLine("Employees sorted and order saved.");
        }

        static void DeleteEmployee(EmployeeContext db)
        {
            Console.Clear();
            Console.Write("Enter Employee ID to delete: ");
            int id = int.Parse(Console.ReadLine());

            var employee = db.Employees.Find(id);
            if (employee != null)
            {
                db.Employees.Remove(employee);
                db.SaveChanges();
                Console.WriteLine("Employee deleted successfully.");
            }
            else
            {
                Console.WriteLine("Employee not found.");
            }
        }

        static void AddDepartment(EmployeeContext db)
        {
            Console.Clear();
            Console.Write("Enter Department Name: ");
            string deptName = Console.ReadLine();

            var newDepartment = new Department { Name = deptName };
            db.Departments.Add(newDepartment);
            db.SaveChanges();
            Console.WriteLine("Department has been added.");
        }

        static void AssignEmployeeToDepartment(EmployeeContext db)
        {
            Console.Clear();
            Console.WriteLine("Select an Employee to Assign to a Department:");

            var employees = db.Employees.ToList();
            if (employees.Count == 0)
            {
                Console.WriteLine("No employees available.");
                return;
            }

            for (int i = 0; i < employees.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {employees[i].Name}");
            }

            Console.Write("Enter the number of the employee to assign: ");
            int employeeIndex = int.Parse(Console.ReadLine()) - 1;

            if (employeeIndex < 0 || employeeIndex >= employees.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedEmployee = employees[employeeIndex];

            Console.WriteLine("Select a Department:");

            var departments = db.Departments.ToList();
            if (departments.Count == 0)
            {
                Console.WriteLine("No departments available.");
                return;
            }

            for (int i = 0; i < departments.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {departments[i].Name}");
            }

            Console.Write("Enter the number of the department to assign: ");
            int departmentIndex = int.Parse(Console.ReadLine()) - 1;

            if (departmentIndex < 0 || departmentIndex >= departments.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }

            var selectedDepartment = departments[departmentIndex];
            selectedEmployee.Department = selectedDepartment;

            db.SaveChanges();
            Console.WriteLine($"{selectedEmployee.Name} has been assigned to the {selectedDepartment.Name} department.");
        }
    }
}
