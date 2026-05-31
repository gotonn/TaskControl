using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskControl.Models;
using TaskControl.Models.Enums;

namespace TaskControl.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var migrations = context.Database.GetMigrations();
        if (migrations.Any())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        await SeedRolesAsync(roleManager);
        var users = await SeedUsersAsync(userManager);
        await SeedTeamsAsync(context, users);
        await SeedProjectsAsync(context, users);
        await SeedTasksAsync(context, users);
        await RefreshDemoTimelineAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { "Admin", "Manager", "Executor" })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task<Dictionary<string, ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var result = new Dictionary<string, ApplicationUser>();

        result["admin"] = await EnsureUserAsync(userManager, "admin@taskcontrol.local", "Admin123!", "Ірина Мельник", "Системний адміністратор", "Адміністрація", new[] { "Admin" });
        result["olena"] = await EnsureUserAsync(userManager, "manager.olena@taskcontrol.local", "Manager123!", "Олена Коваль", "Керівник проєктів", "Проєктний офіс", new[] { "Manager" });
        result["andriy"] = await EnsureUserAsync(userManager, "manager.andriy@taskcontrol.local", "Manager123!", "Андрій Савчук", "Керівник підтримки", "Підтримка клієнтів", new[] { "Manager" });
        result["daria"] = await EnsureUserAsync(userManager, "manager.daria@taskcontrol.local", "Manager123!", "Дарія Романюк", "Операційний менеджер", "Операційний відділ", new[] { "Manager" });
        result["nazar"] = await EnsureUserAsync(userManager, "executor.nazar@taskcontrol.local", "User123!", "Назар Білик", "Backend-розробник", "Розробка", new[] { "Executor" });
        result["ira"] = await EnsureUserAsync(userManager, "executor.ira@taskcontrol.local", "User123!", "Ірина Луценко", "Frontend-розробниця", "Розробка", new[] { "Executor" });
        result["maksym"] = await EnsureUserAsync(userManager, "executor.maksym@taskcontrol.local", "User123!", "Максим Мороз", "QA-інженер", "Контроль якості", new[] { "Executor" });
        result["sofia"] = await EnsureUserAsync(userManager, "executor.sofia@taskcontrol.local", "User123!", "Софія Гнатюк", "Бізнес-аналітикиня", "Аналітика", new[] { "Executor" });
        result["roman"] = await EnsureUserAsync(userManager, "executor.roman@taskcontrol.local", "User123!", "Роман Шевчук", "Спеціаліст підтримки", "Підтримка клієнтів", new[] { "Executor" });
        result["viktoria"] = await EnsureUserAsync(userManager, "executor.viktoria@taskcontrol.local", "User123!", "Вікторія Ткач", "Контент-менеджерка", "Комунікації", new[] { "Executor" });
        result["taras"] = await EnsureUserAsync(userManager, "executor.taras@taskcontrol.local", "User123!", "Тарас Клим", "DevOps-інженер", "Інфраструктура", new[] { "Executor" });
        result["marta"] = await EnsureUserAsync(userManager, "executor.marta@taskcontrol.local", "User123!", "Марта Онищук", "Офіс-координаторка", "Операційний відділ", new[] { "Executor" });

        return result;
    }

    private static async Task<ApplicationUser> EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string fullName, string position, string department, string[] roles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                Position = position,
                Department = department,
                CreatedAt = DateTime.UtcNow.AddDays(-40),
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        foreach (var role in roles)
        {
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        return user;
    }

    private static async Task SeedTeamsAsync(ApplicationDbContext context, Dictionary<string, ApplicationUser> users)
    {
        if (await context.Teams.AnyAsync())
        {
            return;
        }

        var teams = new[]
        {
            new Team { Name = "Команда цифрових продуктів", Description = "Розробка вебсервісів, внутрішніх інструментів і клієнтських порталів", ManagerId = users["olena"].Id, Color = "#2563eb" },
            new Team { Name = "Команда клієнтської підтримки", Description = "Обробка звернень, база знань, контроль SLA та покращення сервісу", ManagerId = users["andriy"].Id, Color = "#16a34a" },
            new Team { Name = "Операційна команда", Description = "Оптимізація внутрішніх процесів, документообіг і звітність", ManagerId = users["daria"].Id, Color = "#9333ea" }
        };

        context.Teams.AddRange(teams);
        await context.SaveChangesAsync();

        AddMember(context, teams[0], users["olena"], TeamMemberRole.Manager);
        AddMember(context, teams[0], users["nazar"], TeamMemberRole.Member);
        AddMember(context, teams[0], users["ira"], TeamMemberRole.Member);
        AddMember(context, teams[0], users["maksym"], TeamMemberRole.Member);
        AddMember(context, teams[0], users["sofia"], TeamMemberRole.Member);
        AddMember(context, teams[0], users["taras"], TeamMemberRole.Member);

        AddMember(context, teams[1], users["andriy"], TeamMemberRole.Manager);
        AddMember(context, teams[1], users["roman"], TeamMemberRole.Member);
        AddMember(context, teams[1], users["viktoria"], TeamMemberRole.Member);
        AddMember(context, teams[1], users["maksym"], TeamMemberRole.Member);
        AddMember(context, teams[1], users["sofia"], TeamMemberRole.Member);

        AddMember(context, teams[2], users["daria"], TeamMemberRole.Manager);
        AddMember(context, teams[2], users["marta"], TeamMemberRole.Member);
        AddMember(context, teams[2], users["taras"], TeamMemberRole.Member);
        AddMember(context, teams[2], users["viktoria"], TeamMemberRole.Member);
        AddMember(context, teams[2], users["roman"], TeamMemberRole.Member);

        await context.SaveChangesAsync();
    }

    private static void AddMember(ApplicationDbContext context, Team team, ApplicationUser user, TeamMemberRole role)
    {
        context.TeamMembers.Add(new TeamMember
        {
            TeamId = team.Id,
            UserId = user.Id,
            Role = role,
            JoinedAt = DateTime.UtcNow.AddDays(-30)
        });
    }

    private static async Task SeedProjectsAsync(ApplicationDbContext context, Dictionary<string, ApplicationUser> users)
    {
        if (await context.WorkProjects.AnyAsync())
        {
            return;
        }

        var productTeam = await context.Teams.FirstAsync(t => t.Name == "Команда цифрових продуктів");
        var supportTeam = await context.Teams.FirstAsync(t => t.Name == "Команда клієнтської підтримки");
        var opsTeam = await context.Teams.FirstAsync(t => t.Name == "Операційна команда");

        var projects = new[]
        {
            new WorkProject { Name = "Впровадження CRM для відділу продажів", Description = "Створення централізованого середовища для обліку клієнтів, звернень і задач менеджерів", TeamId = productTeam.Id, CreatedById = users["olena"].Id, Status = WorkProjectStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-26), EndDate = DateTime.UtcNow.Date.AddDays(35), Color = "#2563eb" },
            new WorkProject { Name = "Редизайн корпоративного порталу", Description = "Оновлення інтерфейсу, навігації, адаптивності та сторінок самообслуговування користувачів", TeamId = productTeam.Id, CreatedById = users["olena"].Id, Status = WorkProjectStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-18), EndDate = DateTime.UtcNow.Date.AddDays(42), Color = "#0ea5e9" },
            new WorkProject { Name = "Автоматизація обробки заявок", Description = "Оптимізація маршрутизації звернень, контроль статусів та інтеграція з базою знань", TeamId = supportTeam.Id, CreatedById = users["andriy"].Id, Status = WorkProjectStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-22), EndDate = DateTime.UtcNow.Date.AddDays(28), Color = "#16a34a" },
            new WorkProject { Name = "Запуск внутрішньої бази знань", Description = "Підготовка категорій, статей, правил оновлення матеріалів і процесу перевірки якості контенту", TeamId = supportTeam.Id, CreatedById = users["andriy"].Id, Status = WorkProjectStatus.Planned, StartDate = DateTime.UtcNow.Date.AddDays(-5), EndDate = DateTime.UtcNow.Date.AddDays(50), Color = "#22c55e" },
            new WorkProject { Name = "Підготовка квартальної звітності", Description = "Збір показників, перевірка джерел, підготовка візуалізацій і фінального пакета звітів", TeamId = opsTeam.Id, CreatedById = users["daria"].Id, Status = WorkProjectStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-12), EndDate = DateTime.UtcNow.Date.AddDays(16), Color = "#9333ea" },
            new WorkProject { Name = "Оптимізація внутрішніх процесів", Description = "Опис регламентів, контроль повторюваних задач, зменшення ручної роботи та погоджень", TeamId = opsTeam.Id, CreatedById = users["daria"].Id, Status = WorkProjectStatus.Active, StartDate = DateTime.UtcNow.Date.AddDays(-35), EndDate = DateTime.UtcNow.Date.AddDays(60), Color = "#a855f7" }
        };

        context.WorkProjects.AddRange(projects);
        await context.SaveChangesAsync();
    }

    private static async Task SeedTasksAsync(ApplicationDbContext context, Dictionary<string, ApplicationUser> users)
    {
        if (await context.TaskItems.AnyAsync())
        {
            return;
        }

        var projects = await context.WorkProjects.Include(p => p.Team).OrderBy(p => p.Id).ToListAsync();
        var assignees = new[] { users["nazar"], users["ira"], users["maksym"], users["sofia"], users["roman"], users["viktoria"], users["taras"], users["marta"] };
        var creators = new[] { users["olena"], users["andriy"], users["daria"] };
        var statuses = new[] { TaskItemStatus.New, TaskItemStatus.Assigned, TaskItemStatus.InProgress, TaskItemStatus.Review, TaskItemStatus.Completed, TaskItemStatus.InProgress, TaskItemStatus.Assigned, TaskItemStatus.Completed, TaskItemStatus.Review, TaskItemStatus.InProgress, TaskItemStatus.Cancelled, TaskItemStatus.New };
        var priorities = new[] { TaskPriority.Medium, TaskPriority.High, TaskPriority.Low, TaskPriority.Critical, TaskPriority.Medium, TaskPriority.High, TaskPriority.Medium, TaskPriority.Low, TaskPriority.High, TaskPriority.Critical, TaskPriority.Medium, TaskPriority.Low };
        var templates = new[]
        {
            ("Проаналізувати вимоги та сформувати перелік сценаріїв", "Необхідно переглянути поточні потреби користувачів, виділити основні сценарії роботи та підготувати структурований перелік вимог для команди."),
            ("Підготувати макет ключового екрана", "Потрібно створити зручний прототип екрана з акцентом на зрозумілу навігацію, видимі статуси та швидкий доступ до дій."),
            ("Реалізувати серверну логіку модуля", "Потрібно реалізувати основні методи обробки даних, перевірку прав доступу, валідацію та збереження інформації в базі даних."),
            ("Налаштувати валідацію форм", "Необхідно додати перевірки обов'язкових полів, коректності дат, довжини тексту та зрозумілі повідомлення для користувача."),
            ("Провести тестування основних сценаріїв", "Потрібно перевірити створення, редагування, пошук, зміну статусів і поведінку системи при некоректних даних."),
            ("Підготувати набір демонстраційних даних", "Потрібно наповнити систему прикладами, які показують різні статуси, пріоритети, дедлайни, виконавців і коментарі."),
            ("Оновити сторінку списку та фільтри", "Потрібно зробити список зручним для щоденної роботи: пошук, фільтрація, сортування, картки статусів і швидкі переходи."),
            ("Додати сповіщення для відповідальних осіб", "Потрібно забезпечити повідомлення користувачів про призначення, зміну статусу, нові коментарі та ризик прострочення."),
            ("Перевірити бізнес-логіку дедлайнів", "Необхідно перевірити правильність визначення прострочених завдань, ризиків і відображення критичних задач."),
            ("Підготувати коротку інструкцію для користувачів", "Потрібно описати, як створювати задачі, змінювати статуси, працювати з Kanban-дошкою та переглядати аналітику."),
            ("Оптимізувати структуру сторінки деталей", "Потрібно розмістити опис, історію змін, коментарі, дедлайн, відповідального та статус так, щоб інформація легко читалася."),
            ("Звірити дані аналітичної панелі", "Потрібно перевірити, чи правильно рахуються активні, виконані, прострочені задачі та завантаженість учасників.")
        };

        var random = new Random(42);
        var createdTasks = new List<TaskItem>();

        for (var projectIndex = 0; projectIndex < projects.Count; projectIndex++)
        {
            var project = projects[projectIndex];
            for (var i = 0; i < templates.Length; i++)
            {
                var template = templates[i];
                var status = statuses[(i + projectIndex) % statuses.Length];
                var priority = priorities[(i + projectIndex * 2) % priorities.Length];
                var assignee = assignees[(i + projectIndex * 3) % assignees.Length];
                var creator = creators[projectIndex % creators.Length];
                var isPastOpen = i % 7 == 0 || i % 11 == 0;
                var deadline = status == TaskItemStatus.Completed
                    ? DateTime.UtcNow.Date.AddDays(-random.Next(1, 15)).AddHours(17)
                    : isPastOpen
                        ? DateTime.UtcNow.Date.AddDays(-random.Next(1, 6)).AddHours(18)
                        : DateTime.UtcNow.Date.AddDays(random.Next(1, 36)).AddHours(random.Next(10, 19));
                var progress = status switch
                {
                    TaskItemStatus.New => 0,
                    TaskItemStatus.Assigned => random.Next(0, 20),
                    TaskItemStatus.InProgress => random.Next(25, 75),
                    TaskItemStatus.Review => random.Next(80, 96),
                    TaskItemStatus.Completed => 100,
                    TaskItemStatus.Cancelled => random.Next(0, 40),
                    _ => 0
                };

                var task = new TaskItem
                {
                    Title = $"{template.Item1}: {project.Name}",
                    Description = template.Item2 + " Очікуваний результат повинен бути зафіксований у системі, щоб керівник міг відстежити виконання та прийняти рішення щодо наступних дій.",
                    Priority = priority,
                    Status = status,
                    Deadline = deadline,
                    EstimatedHours = random.Next(2, 24),
                    ProgressPercent = progress,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(2, 32)).AddHours(-random.Next(1, 8)),
                    UpdatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 10)).AddHours(-random.Next(1, 9)),
                    CompletedAt = status == TaskItemStatus.Completed ? deadline.AddHours(-random.Next(2, 12)) : null,
                    CreatedById = creator.Id,
                    AssignedToId = assignee.Id,
                    ProjectId = project.Id,
                    TeamId = project.TeamId
                };

                createdTasks.Add(task);
            }
        }

        context.TaskItems.AddRange(createdTasks);
        await context.SaveChangesAsync();

        foreach (var task in createdTasks)
        {
            context.TaskHistory.Add(new TaskHistory
            {
                TaskItemId = task.Id,
                UserId = task.CreatedById,
                Action = "Створено завдання",
                NewValue = task.Title,
                CreatedAt = task.CreatedAt
            });

            context.TaskHistory.Add(new TaskHistory
            {
                TaskItemId = task.Id,
                UserId = task.CreatedById,
                Action = "Призначено виконавця",
                NewValue = task.AssignedToId,
                CreatedAt = task.CreatedAt.AddMinutes(12)
            });

            if (task.Status != TaskItemStatus.New)
            {
                context.TaskHistory.Add(new TaskHistory
                {
                    TaskItemId = task.Id,
                    UserId = task.AssignedToId,
                    Action = "Змінено статус",
                    OldValue = "Нове",
                    NewValue = task.Status.ToString(),
                    CreatedAt = task.UpdatedAt
                });
            }
        }

        var commentTexts = new[]
        {
            "Прийнято в роботу, після уточнення деталей оновлю статус виконання.",
            "Потрібно узгодити один момент перед фінальним завершенням.",
            "Основна частина вже виконана, залишилось перевірити крайні випадки.",
            "Додано проміжний результат, можна переглянути і залишити зауваження.",
            "Є ризик не встигнути без уточнення пріоритетів, прошу підтвердити порядок виконання."
        };

        for (var i = 0; i < createdTasks.Count; i++)
        {
            var task = createdTasks[i];
            if (i % 2 == 0)
            {
                context.TaskComments.Add(new TaskComment
                {
                    TaskItemId = task.Id,
                    UserId = task.AssignedToId,
                    Text = commentTexts[i % commentTexts.Length],
                    CreatedAt = task.UpdatedAt.AddHours(1)
                });
            }

            if (i % 5 == 0)
            {
                context.TaskAttachments.Add(new TaskAttachment
                {
                    TaskItemId = task.Id,
                    UploadedById = task.AssignedToId,
                    FileName = $"result-{task.Id}.pdf",
                    FilePath = $"/uploads/demo/result-{task.Id}.pdf",
                    UploadedAt = task.UpdatedAt.AddHours(2)
                });
            }

            if (i < 28)
            {
                context.Notifications.Add(new Notification
                {
                    UserId = task.AssignedToId,
                    Title = "Призначене завдання",
                    Message = $"Вам призначено завдання «{task.Title}»",
                    Type = NotificationType.TaskAssigned,
                    TaskItemId = task.Id,
                    IsRead = i % 3 == 0,
                    CreatedAt = task.CreatedAt.AddMinutes(15)
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task RefreshDemoTimelineAsync(ApplicationDbContext context)
    {
        var demoProjectNames = new HashSet<string>
        {
            "Впровадження CRM для відділу продажів",
            "Редизайн корпоративного порталу",
            "Автоматизація обробки заявок",
            "Запуск внутрішньої бази знань",
            "Підготовка квартальної звітності",
            "Оптимізація внутрішніх процесів"
        };

        var tasks = await context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .Include(t => t.History)
            .Where(t => demoProjectNames.Contains(t.Project.Name) && t.Title.Contains(":"))
            .OrderBy(t => t.ProjectId)
            .ThenBy(t => t.Id)
            .ToListAsync();

        if (tasks.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var today = now.Date;
        var statusPattern = new[]
        {
            TaskItemStatus.New,
            TaskItemStatus.Assigned,
            TaskItemStatus.InProgress,
            TaskItemStatus.Review,
            TaskItemStatus.Completed,
            TaskItemStatus.Assigned,
            TaskItemStatus.InProgress,
            TaskItemStatus.Review,
            TaskItemStatus.Completed,
            TaskItemStatus.InProgress,
            TaskItemStatus.Cancelled,
            TaskItemStatus.Assigned
        };

        for (var i = 0; i < tasks.Count; i++)
        {
            var task = tasks[i];
            var status = statusPattern[i % statusPattern.Length];
            var shouldBeOverdue = i % 17 == 0;

            if (shouldBeOverdue && status is not TaskItemStatus.Completed and not TaskItemStatus.Cancelled)
            {
                status = TaskItemStatus.InProgress;
                task.Deadline = today.AddDays(-((i % 3) + 1)).AddHours(16);
                task.ProgressPercent = 35 + i % 35;
            }
            else
            {
                task.Deadline = status switch
                {
                    TaskItemStatus.New => today.AddDays(4 + i % 8).AddHours(10 + i % 7),
                    TaskItemStatus.Assigned => today.AddDays(1 + i % 10).AddHours(11 + i % 6),
                    TaskItemStatus.InProgress => today.AddDays(2 + i % 12).AddHours(12 + i % 5),
                    TaskItemStatus.Review => today.AddDays(1 + i % 6).AddHours(14 + i % 4),
                    TaskItemStatus.Completed => today.AddDays(-((i % 5) + 1)).AddHours(17),
                    TaskItemStatus.Cancelled => today.AddDays(6 + i % 12).AddHours(15),
                    _ => today.AddDays(3).AddHours(12)
                };

                task.ProgressPercent = status switch
                {
                    TaskItemStatus.New => 0,
                    TaskItemStatus.Assigned => 10 + i % 15,
                    TaskItemStatus.InProgress => 35 + i % 40,
                    TaskItemStatus.Review => 82 + i % 14,
                    TaskItemStatus.Completed => 100,
                    TaskItemStatus.Cancelled => 20 + i % 25,
                    _ => task.ProgressPercent
                };
            }

            task.Status = status;
            task.CreatedAt = today.AddDays(-((i % 18) + 4)).AddHours(9 + i % 6);
            task.UpdatedAt = status == TaskItemStatus.New
                ? task.CreatedAt
                : now.AddDays(-(i % 4)).AddHours(-(i % 8));
            task.CompletedAt = status == TaskItemStatus.Completed ? task.Deadline.AddHours(-2) : null;

            foreach (var comment in task.Comments)
            {
                comment.CreatedAt = task.UpdatedAt.AddHours(1);
            }

            foreach (var history in task.History)
            {
                history.CreatedAt = task.CreatedAt.AddMinutes(15 + i % 40);
            }

        }

        var taskById = tasks.ToDictionary(t => t.Id);
        var taskIds = taskById.Keys.ToList();
        var notifications = await context.Notifications
            .Where(n => n.TaskItemId.HasValue && taskIds.Contains(n.TaskItemId.Value))
            .ToListAsync();

        foreach (var notification in notifications)
        {
            var task = taskById[notification.TaskItemId!.Value];
            notification.CreatedAt = task.CreatedAt.AddMinutes(20);
            notification.Title = "Призначене завдання";
            notification.Message = $"Вам призначено завдання «{task.Title}»";
        }

        await context.SaveChangesAsync();
    }
}
