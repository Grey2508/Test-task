using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace BotVK
{
    class Program
    {
        static void Main(string[] args)
        {
            VkApi api = new VkApi();
            Console.WriteLine("Привет, пользователь! Нам необходимо авторизоваться в ВК, поэтому введи свой email или телефон");
            string line = Console.ReadLine();
            bool auth = false;
            do
            {
                string login = line;

                Console.WriteLine("Хорошо! А теперь введи свой пароль. Я никому его не скажу");
                string password = Console.ReadLine();

                Console.Clear();

                try//не предусмотрена обратная связь вслучае неудачи авторизации
                {
                    api.Authorize(new ApiAuthParams
                    {
                        ApplicationId = 6707209,
                        Login = login,
                        Password = password,
                        Settings = Settings.All
                    });

                    auth = true;
                }
                catch
                {
                    Console.WriteLine("Упс! Что-то пошло не так и авторизоваться не получилось.");
                    Console.WriteLine("Давай попробуем еще раз! Введи свой email или телефон.");
                    Console.WriteLine("Если хочешь завершить работу программы, введи пустую строку");

                    line = Console.ReadLine();
                }
            }
            while (line != "" && !auth);

            if (!auth)
                return;

            var pi = api.Account.GetProfileInfo(); //информация о текущем профиле
            Console.WriteLine(string.Format("Привет, {0} {1}!", pi.FirstName, pi.LastName));

            string sID = "";
            do
            {
                Console.WriteLine("");
                Console.WriteLine("Теперь давай кого-нибудь найдем. Введи ID аккаунта");
                sID = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(sID)) //выход, если введена пустая строка
                    continue;

                long id = -1;

                if (!long.TryParse(sID, out id)) //Попытка преоброзавать введнный ID  в число
                {
                    if (sID.StartsWith("id"))
                        long.TryParse(sID.Substring(2), out id); //Возможно был введен в форме "id________"
                }

                List<User> u = new List<User>();

                if (id > 0 && id < long.MaxValue)
                {
                    try
                    {
                        List<long> ids = new List<long>();
                        ids.Add(id);
                        u = api.Users.Get(ids).ToList();
                    }
                    catch (VkNet.Exception.InvalidUserIdException ex)
                    {
                        Console.WriteLine("Введен недействительный ID");
                        continue;
                    }
                }
                else
                    u = api.Users.Search(new UserSearchParams { Query = sID }).ToList(); //поиск пользователя по введеной строке

                if (u.Count == 1)
                {
                    string userName = string.Format("{0} {1}", u[0].FirstName, u[0].LastName);

                    try
                    {
                        Console.WriteLine(string.Format("А я знаю, кто это! Это {0}!", userName));
                        var posts = api.Wall.Get(new WallGetParams { OwnerId = u[0].Id, Count = 5 });
                        
                        FrequencyLetters fl = new FrequencyLetters();

                        for (int i = 0; i < posts.WallPosts.Count; i++)
                        {
                            var post = posts.WallPosts[i];

                            fl.Add(post.Text);
                        }

                        Console.WriteLine(fl.toJSON());

                        api.Wall.Post(new WallPostParams
                        {
                            OwnerId = api.UserId,
                            Message = string.Format("{0}, статистика для последних 5 постов:\r\n{1}", userName, fl.toJSON())
                        });
                    }
                    catch (VkNet.Exception.UserDeletedOrBannedException ex) //нет таких признаков в результатах поиска
                    {
                        Console.WriteLine("Пользователь удален или заблокирован");
                    }
                }
                else if (u.Count == 0)
                    Console.WriteLine("Никого найти не удалось");
                else
                    Console.WriteLine("Найдено больше 1 человека");

            }
            while (sID != "");

            Console.WriteLine("\r\r\rДля завершения работы нажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}
