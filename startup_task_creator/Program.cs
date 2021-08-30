using System;
using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;

namespace StartupTaskCreator
{
    class Program
    {
        private static Task startupTask = null;
        private static string taskName = null;

        // args[0]: 1 -> create task, 0 -> delete task
        // args[1]: app name
        // args[2]: path to exe
        static void Main(string[] args)
        {
            taskName = $"{args[1]} - Autorun for {Environment.UserName}";
            startupTask = TaskService.Instance.GetTask($@"\{taskName}");

            int.TryParse(args[0], out int arg);
            if (arg == 1)
            {
                if (startupTask == null)
                {
                    Debug.WriteLine("Creating task");

                    TaskDefinition taskDefinition = TaskService.Instance.NewTask();
                    taskDefinition.RegistrationInfo.Author = "MicMuter";
                    taskDefinition.RegistrationInfo.Description = "Starts MicMuter on log in";
                    taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;

                    LogonTrigger trigger = new LogonTrigger
                    {
                        UserId = Environment.UserName
                    };
                    taskDefinition.Triggers.Add(trigger);

                    taskDefinition.Actions.Add(args[2]);

                    startupTask = TaskService.Instance.RootFolder.RegisterTaskDefinition($"{args[1]} - Autorun for {Environment.UserName}", taskDefinition);
                }
                startupTask.Enabled = true;
                Debug.WriteLine($"Startup Task Path: {startupTask.Path}, Enabled: {startupTask.Enabled}");
            }
            else if (arg == 0)
            {
                if (taskName != null)
                {
                    startupTask.Folder.DeleteTask(taskName, false);
                }
            }
            if (startupTask != null) startupTask.Dispose();
        }
    }
}
