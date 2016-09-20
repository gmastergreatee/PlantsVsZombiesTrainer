using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RajarshiSoftwares;

namespace PlantsVsZombies_Cheater
{
    public partial class MainWindow : Window
    {
        GameSearch search;
        string gameName = "PlantsVsZombies";
        long off1 = 0x1E846;
        long off2 = 0x91E55;
        long off3 = 0x403A4;
        long add1 = 0;
        long add2 = 0;
        long add3 = 0;
        long baseAdd = 0;
        long caveAdd = 0;
        Memory angel;
        KeyboardHook kHook;
        MemoryChanger changer;
        bool activated = false;

        bool isGameRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            search = new GameSearch(gameName, Dispatcher, false);
            search.gameFound += GameFound;
            search.gameLost += GameLost;
            search.error += Error;
            kHook = new KeyboardHook(false, true);
            kHook.KeyUp += KHook_KeyUp;
            kHook.Start();
            search.ChangeStatus(true);
        }

        private void KHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (isGameRunning)
            {
                if (!activated)
                {
                    if (e.KeyData == System.Windows.Forms.Keys.Insert)
                    {
                        activateText.Foreground = new SolidColorBrush(Colors.LightGreen);
                        activated = true;
                    }
                }
                else
                {
                    if (e.KeyData == System.Windows.Forms.Keys.D1)
                    {
                        if (changer.CheatActivated(1))
                        {
                            changer.StopCheat(1);
                            sunText.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            changer.StartCheat(1);
                            sunText.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.KeyData == System.Windows.Forms.Keys.D2)
                    {
                        if (changer.CheatActivated(2))
                        {
                            changer.StopCheat(2);
                            itemText.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            changer.StartCheat(2);
                            itemText.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                    if (e.KeyData == System.Windows.Forms.Keys.D3)
                    {
                        if (changer.CheatActivated(3))
                        {
                            changer.StopCheat(3);
                            moneyText.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            changer.StartCheat(3);
                            moneyText.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                }
            }
        }

        public void DeacAll()
        {
            activated = false;
            activateText.Foreground = new SolidColorBrush(Colors.Black);
            sunText.Foreground = new SolidColorBrush(Colors.Black);
            itemText.Foreground = new SolidColorBrush(Colors.Black);
            moneyText.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void Error(string errorInfo)
        {
            isGameRunning = false;
            statusText.Text = "Multiple instances " + gameName + ".exe";
            statusText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void GameLost(string GameName)
        {
            isGameRunning = false;
            DeacAll();
            statusText.Text = "Not Running " + GameName + ".exe";
            statusText.Foreground = new SolidColorBrush(Colors.Red);
        }

        private void GameFound(Memory angel)
        {
            this.angel = angel;
            baseAdd = angel.BaseAddressD();
            caveAdd = angel.MakeNewCodeCave();
            add1 = baseAdd + off1;
            add2 = baseAdd + off2;
            add3 = baseAdd + off3;
            statusText.Text = "Game Running";
            statusText.Foreground = new SolidColorBrush(Colors.Blue);
            changer = new MemoryChanger();
            ByteNopCheat cht1 = new ByteNopCheat(add1, angel, 6, "Unlimited Suns");
            ByteNopCheat cht2 = new ByteNopCheat(add2, angel, 2, "Unlimited Items");
            ByteNopCheat cht3 = new ByteNopCheat(add3, angel, 3, "Unlimited Money");
            changer.AddCheat(cht1);
            changer.AddCheat(cht2);
            changer.AddCheat(cht3);
            isGameRunning = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState != WindowState.Minimized)
                WindowState = WindowState.Minimized;
        }
    }
}
