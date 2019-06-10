using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using log4net;
using System.Reflection;

namespace SleepApp
{
	static class Program
	{
		public static int SleepCheckTime
		{
			get
			{
				lock (_sleepCheckTimeLock)
				{
					return _sleepCheckTime;
				}
			}
			set
			{
				lock (_sleepCheckTimeLock)
				{
					_sleepCheckTime = value;
				}
			}
        }

        public static int SleepVisibleTime
        {
            get
            {
                lock (_sleepVisibleTimeLock)
                {
                    return _sleepVisibleTime;
                }
            }
            set
            {
                lock (_sleepVisibleTimeLock)
                {
                    _sleepVisibleTime = value;
                }
            }
        }

        public static int PermissibleRangeX
        {
            get
            {
                lock (_permissibleRangeXLock)
                {
                    return _permissibleRangeX;
                }
            }
            set
            {
                lock (_permissibleRangeXLock)
                {
                    _permissibleRangeX = value;
                }
            }
        }

        public static int PermissibleRangeY
        {
            get
            {
                lock (_permissibleRangeYLock)
                {
                    return _permissibleRangeY;
                }
            }
            set
            {
                lock (_permissibleRangeYLock)
                {
                    _permissibleRangeY = value;
                }
            }
        }

        public static PowerState SleepMode
        {
            get
            {
                lock (_sleepModeLock)
                {
                    return _sleepMode;
                }
            }
            set
            {
                lock (_sleepModeLock)
                {
                    _sleepMode = value;
                }
            }
        }

        private static int _sleepCheckTime;
        private static int _sleepVisibleTime;
        private static int _permissibleRangeX;
        private static int _permissibleRangeY;
        private static PowerState _sleepMode;
        private static object _sleepCheckTimeLock = new object();
        private static object _sleepVisibleTimeLock = new object();
        private static object _permissibleRangeXLock = new object();
        private static object _permissibleRangeYLock = new object();
        private static object _sleepModeLock = new object();

        public static string APP_NAME = "SleepApp";
        public static ILog logger = LogManager.GetLogger(Assembly.GetExecutingAssembly().FullName);

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
		static void Main()
		{
			// Mutex名
			string mutexName = APP_NAME;
			// Mutexオブジェクトを作成する
			bool createdNew;
			System.Threading.Mutex mutex = new System.Threading.Mutex(true, mutexName, out createdNew);

			//ミューテックスの初期所有権が付与されたか調べる
			if (createdNew == false)
			{
				//されなかった場合は、すでに起動していると判断して終了
				//MessageBox.Show("多重起動はできません。");
				mutex.Close();
				return;
			}

			try
			{
                // 設定読み込み
				ReadAllSettings();

                // イベントログ書き込み準備
                logger.Info("情報：アプリケーション起動");

                // アプリ起動
                Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new SleepForm());
			}
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
			finally
			{
				//ミューテックスを解放する
				mutex.ReleaseMutex();
				mutex.Close();
			}
		}

		private static void ReadAllSettings()
		{
            SleepCheckTime = 3600;
            SleepVisibleTime = 60;
            PermissibleRangeX = 50;
            PermissibleRangeY = 50;
            SleepMode = PowerState.Suspend;

            foreach (string key in ConfigurationManager.AppSettings.AllKeys)
			{
				string value = ConfigurationManager.AppSettings[key];

				if (key == SettingKeys.SleepCheckTime.ToString())
				{
					SleepCheckTime = int.Parse(value);
				}

                else if (key == SettingKeys.PermissibleRangeX.ToString())
                {
                    PermissibleRangeX = int.Parse(value);
                }

                else if (key == SettingKeys.PermissibleRangeY.ToString())
                {
                    PermissibleRangeY = int.Parse(value);
                }
                else if (key == SettingKeys.SleepMode.ToString())
                {
                    if (value == PowerState.Suspend.ToString())
                    {
                        SleepMode = PowerState.Suspend;
                    }
                    else
                    {
                        SleepMode = PowerState.Hibernate;
                    }
                }
                else if (key == SettingKeys.SleepVisibleTime.ToString())
                {
                    SleepVisibleTime = int.Parse(value);
                }
            }
		}

		public enum SettingKeys
		{
			SleepCheckTime,
            PermissibleRangeX,
            PermissibleRangeY,
            SleepMode,
            SleepVisibleTime
        }
	}
}
