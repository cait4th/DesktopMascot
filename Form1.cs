using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DesktopMascot.src;

namespace DesktopMascot
{
/// <summary>
/// Formの設定　
/// </summary>
    public partial class Form1 : Form
    {
        private MainProcess mainProcess = null;

        public Form1()
        {
            InitializeComponent();
            mainProcess = new MainProcess(Handle);

            ClientSize = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            mainProcess.Initialize();
        }

        /// <summary>
        /// ループ実行部
        /// </summary>
        public void Form1Main()
        {
            mainProcess.Run();

            if (mainProcess.FinFlag == true)
            {
                this.Close();
            }
        }

        /// <summary>
        /// フォーム終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainProcess.Finalize();
        }

        /// <summary>
        /// フォームの設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Shown(object sender, EventArgs e)
        {
            //フォームの枠を非表示
            FormBorderStyle = FormBorderStyle.None;

            Text = "DesktopMascot";
            WindowState = FormWindowState.Maximized;
            TopMost = true;

            //透過色を設定
            TransparencyKey = Color.FromArgb(1, 1, 1);
        }
    }
}
