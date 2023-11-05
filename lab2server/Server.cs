using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using classPerson;
using NLog;


class AsyncUdpServer
{
    private const int port = 8001;
    private const string path = "C:\\Users\\Пользователь\\Desktop\\files\\labs files\\labs ais\\lab22\\lab2\\lab2server\\data.csv";
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

            else if (request == "cl")                        //clear file
            {
                response = "file was cleared";
                clearFile();
                logger.Info("All records were successfully deleted");
            }



            else if (request.StartsWith("chnum"))                            // choose by number
            {
                if (int.TryParse(request.Substring(5), out int num) && num > 0 && num < CountFileString())
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
                if (int.TryParse(request.Substring(4), out int num) && num > 0 && num < CountFileString() && deleteByNum(num))
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
        string[] lines = File.ReadAllLines(path);
        return string.Join(Environment.NewLine, lines);
    }

    private static void clearFile()                         // отчистка файла
    {
        File.WriteAllText(path, "");
    }

    private static string chooseByNum(int num)                // вывод по номеру
    {
        string result = null;

        using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
        {
            for (int i = 0; i < num; i++)
            {
                result = sr.ReadLine();
            }
        }

        if (result == null)
            return "There is no string with this number";
        else
            return result;

    }


    private static bool addPerson(string data)
    {
        string[] info = data.Split(' ');

        if (info.Length == 4 && bool.TryParse(info[2], out _) && int.TryParse(info[3], out _))        //проверяем что пользователь ввел 4 значения и все значения в нужных типах данных
        {
            Person person = new Person(info[0], info[1], bool.Parse(info[2]), int.Parse(info[3]));    //создаем персону
            writePersonToFile(person);                                                                //пихаем персону в файл
            return true;
        }

        else
        {
            return false;
        }

    }


    private static void writePersonToFile(Person person)                                         //для запихивания персоны в файл
    {
        using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
        {
            sw.WriteLine(person.Name + ", " + person.Surname + ", " + person.Sex + ", " + person.Age);
        }

    }

    private static bool deleteByNum(int num)
    {

        List<string> records = new List<string>();
        string line = "";

        using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
        {
            while ((line = sr.ReadLine()) != null)
            {
                records.Add(line);
            }
        }

        if (num > 0 && num <= records.Count)
        {
            records.RemoveAt(num - 1);

            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                foreach (string str in records)
                {
                    sw.WriteLine(str);
                }
            }
            return true;
        }
        else
        {
            return false;
        }

    }

    private static int CountFileString()
    {
        int stringsNum = 0;

        using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
        {
            while (sr.ReadLine() != null)
            {
                stringsNum++;
            }
        }

        return stringsNum;
    }
}

