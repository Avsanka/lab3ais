using System;
using System.Collections.Generic;
using System.Text;

namespace classPerson
{
    class Person
    {
        private string name;

        public string Name { get { return name; } set { name = value; } }


        private string surname;
        public string Surname { get { return surname; } set { surname = value; } }


        private bool sex;
        public bool Sex { get { return sex; } set { sex = value; } }


        private int age;
        public int Age
        {
            get { return age; }
            set
            {
                if (value < 0)
                    age = 0;
                else
                    age = value;
            }
        }

        public Person(string name, string surname, bool sex, int age)
        {
            this.name = name;
            this.surname = surname;
            this.sex = sex;
            this.age = age;
        }


    }
}