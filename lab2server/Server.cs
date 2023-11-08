using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using classPerson;
using lab2server;
using NLog;


class AsyncUdpServer
{
    private const int port = 8001;
    private const string path = "C:\\Users\\Пользователь\\Desktop\\files\\labs files\\labs ais\\lab3\\lab2server\\data.csv";
    private static Logger logger = LogManager.GetCurrentClassLogger();


    static async Task Main()
    {
        UdpClient server = new UdpClient(port);
        Console.WriteLine("server has been launched");

        while (true)
        {
            UdpReceiveResult result = await server.ReceiveAsync();     // получение команды
            string request = Encoding.UTF8.GetString(result.Buffer);
            string response = "";






            if (request == "esc")                                     //Обработка запроса
            {
                break;
            }

            if (request == "showps")                     // show all data
            {
                response = getAllData();
                logger.Info("All records were successfully shown");

            }



            else if (request.StartsWith("chnum"))                            // choose by number
            {
                if (int.TryParse(request.Substring(5), out int num) && num > 0)
                {
                    response = chooseByNum(num);
                    logger.Info("Record number " + num.ToString() + " was successfully shown");
                }
                else
                {
                    response = "Number of record out of range or not exist";
                    logger.Error("User command hasn't number of record or number was out of range");
                }
            }


            else if (request.StartsWith("addp") && request.Length > 4)                         //Добавление записи
            {
                string data = request.Substring(5);

                if (addPerson(data))
                {
                    response = "person was added successful";
                    logger.Info("Record was added successful");
                }

                else
                {
                    response = "Ошибка";
                    logger.Error("Recieved unappropriate data or empty string from user");
                }
            }

            else if (request.StartsWith("dnum"))            //удаление записи по номеру
            {
                if (int.TryParse(request.Substring(4), out int num) && num > 0 && deleteByNum(num))
                {
                    response = "record was deleted successful";
                    logger.Info("1 record was deleted successful");
                }
                else
                {
                    response = "Record number outta range, not exist or wrong input";
                    logger.Error("Record number out of range or not exist");
                }
            }

            else
            {                                                        // uknown command
                response = "Unknown command. Try again";
                logger.Error("Recieved unknown command");
            }


            byte[] responseData = Encoding.UTF8.GetBytes(response);
            await server.SendAsync(responseData, responseData.Length, result.RemoteEndPoint);    //Отправление результата клиенту
        }
    }












    private static string getAllData()                   //вывести все
    {
        List<string> lines = new List<string>();
        using (PersonContext db = new PersonContext())
        {
            var people = db.People;
            foreach (Person u in people)
            {
                lines.Add(u.Id + " " + u.Name + " " + u.Surname + " " + u.Sex + " " + u.Age);
                Console.WriteLine("{0} {1} {2} - {3} - {4}", u.Id, u.Name, u.Surname, u.Sex, u.Age);
            }
        }
        return string.Join(Environment.NewLine, lines);
    }

    private static string chooseByNum(int num)                // вывод по номеру
    {
        using (PersonContext db = new PersonContext())
        {
            var person = db.People.Find(num);
            if (person != null)
            {
                return person.Id + " " + person.Name + " " + person.Surname + " " + person.Sex + " " + person.Age.ToString();
            }
            else
            {
                return "There is no person with this number";
            }
        }
    }


    private static bool deleteByNum(int num)
    {

        using (PersonContext db = new PersonContext())
        {
            var person = db.People.Find(num);
            if (person != null)
            {
                db.People.Remove(person);
                db.SaveChanges();
                return true;
            }
            return false;
        }

    }


    private static bool addPerson(string data)
    {
        string[] info = data.Split(' ');

        if (info.Length == 4 && bool.TryParse(info[2], out _) && int.TryParse(info[3], out _))        //проверяем что пользователь ввел 4 значения и все значения в нужных типах данных
        {
            Person person = new Person();
            person.Name = info[0];
            person.Surname = info[1];
            person.Sex = bool.Parse(info[2]);
            person.Age = int.Parse(info[3]);


            writePersonToDb(person);                                                                //пихаем персону в базу
            return true;
        }

        else
        {
            return false;
        }

    }


    private static void writePersonToDb(Person person)                                         //для запихивания персоны в базу
    {
        using (PersonContext db = new PersonContext())
        {
            db.People.Add(person);
            db.SaveChanges();
        }

    }

}

