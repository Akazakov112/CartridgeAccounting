using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace CartAccClient.Model
{
    /// <summary>
    /// Класс проверки на запуск копии программы.
    /// </summary>
    public static class RunOnlyOne
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern int ShowWindow(int hwnd, int nCmdShow);

        private static Mutex syncObject;

        private static readonly string AppPath = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// Находит запущенную копию приложения и разворачивает окно
        /// </summary>
        /// <param name="UniqueValue">Уникальное значение для каждой программы (можно имя)</param>
        /// <returns>true - если приложение было запущено</returns>
        public static bool CheckRunProgram(string UniqueValue)
        {
            syncObject = new Mutex(true, UniqueValue, out bool applicationRun);
            if (!applicationRun)
            {
                // Восстановить/развернуть окно.                            
                try
                {
                    Process[] procs = Process.GetProcessesByName(AppPath);

                    foreach (Process proc in procs)
                        if (proc.Id != Process.GetCurrentProcess().Id)
                        {
                            // Нормально развернутое.
                            ShowWindow((int)proc.MainWindowHandle, 1);
                            // Максимально развернутое.
                            //ShowWindow((int)proc.MainWindowHandle, 3);
                            SetForegroundWindow(proc.MainWindowHandle);
                            break;
                        }
                }
                catch
                {
                    return false;
                }
            }
            return !applicationRun;
        }
    }
}
