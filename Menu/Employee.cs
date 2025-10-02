using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menu
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Gender Gender { get; set; }
        public double Salary { get; set; }
        public int Age { get; set; }
        public int SortOrder { get; set; }
        public virtual Department Department { get; set; }
        public void DisplayData()
        {
            Console.WriteLine($"ID: {Id}\nName: {Name}\nSalary: {Salary}\nGender: {Gender}\nAge: {Age}\nDepartment: {Department?.Name}");
            Console.WriteLine($"--------------------------");
        }


    }
}
