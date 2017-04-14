using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace RajarshiSoftwares
{
    #region GameSearch - Version 1.1
    /// <summary>
    /// Class to search for applications that are currently executing in memory
    /// Developed by GameMaster Greatee aka Rajarshi Vaidya
    /// </summary>
    public class GameSearch
    {
        public delegate void GameFound(Memory angel);
        public delegate void GameLost(string GameName);
        public delegate void Error(string errorInfo);

        /// <summary>
        /// Triggered whenever game is found
        /// </summary>
        public GameFound gameFound;
        /// <summary>
        /// Triggered whenever game is lost
        /// </summary>
        public GameLost gameLost;
        /// <summary>
        /// Triggered whenever there are multiple instances of the game(Game Running status changes to false)
        /// </summary>
        public Error error;

        /// <summary>
        /// Gets or Sets the name of the application or game
        /// </summary>
        public string GameName { get; set; }

        public bool Running { get; private set; }
        Dispatcher currentDispatcher;

        /// <summary>
        /// Default constructor for the Game searching class
        /// </summary>
        /// <param name="GameName">The name of the game in string(Must contain exe name without the suffix .exe)</param>
        /// <param name="currentDispatcher">The main UI's Dispatcher so as to route events directly</param>
        public GameSearch(string GameName, Dispatcher currentDispatcher)
        {
            this.GameName = GameName;
            this.currentDispatcher = currentDispatcher;
            Thread mainThread = new Thread(() =>
            {
                for (int i = 0; i < 1;)
                {
                    if (Running)
                        FindProcess();
                    else
                        Thread.Sleep(100);
                }
            });
            mainThread.Start();
        }

        /// <summary>
        /// Default constructor for the Game searching class
        /// </summary>
        /// <param name="GameName">The name of the game in string(Must contain exe name without the suffix .exe)</param>
        /// <param name="currentDispatcher">The main UI's Dispatcher so as to route events</param>
        /// <param name="GameSearchStatus">Modify start parameters of the multi-threading system</param>
        public GameSearch(string GameName, Dispatcher currentDispatcher, bool GameSearchStatus)
            : this(GameName, currentDispatcher)
        {
            Running = GameSearchStatus;
        }

        /// <summary>
        /// Start or stop the functioning of the game search algorithm
        /// </summary>
        /// <param name="newStatus">The new status</param>
        public void ChangeStatus(bool newStatus)
        {
            if (Running != newStatus)
                Running = newStatus;
        }

        bool isGameRunning = false;

        private void FindProcess()
        {
            Process[] myProcesses = Process.GetProcessesByName(GameName);
            if (!isGameRunning && myProcesses.Count() == 1)
            {
                Memory angel = new Memory();
                angel.ReadProcess = myProcesses[0];
                angel.Open();
                isGameRunning = true;
                currentDispatcher.Invoke(new Action(() => { gameFound?.Invoke(angel); }));
            }
            else if (isGameRunning && myProcesses.Count() == 0)
            {
                isGameRunning = false;
                currentDispatcher.Invoke(new Action(() => { gameLost?.Invoke(GameName); }));
            }
            else if (myProcesses.Count() > 1)
            {
                if (isGameRunning)
                    isGameRunning = false;
                currentDispatcher.Invoke(new Action(() => { error?.Invoke("Multiple instances found for the application."); }));
            }
        }
    }
    #endregion
}