using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DxLibDLL;
using System.Windows.Forms;
using System.IO;

namespace DesktopMascot.src
{
    /// <summary>
    /// メインプロセス(DXLib設定まわり)
    /// </summary>
    class MainProcess
    {
        private int windowWidth = Screen.PrimaryScreen.Bounds.Width;
        private int windowHeight = Screen.PrimaryScreen.Bounds.Height;

        private IntPtr windowHandle = IntPtr.Zero;

        private const string __ModelParameterFile__ = @"model.txt";

        private Random rand = new Random();

        private enum MODELPARA
        {
            modelPath,
            from,
            to,
        }
        private int modelHandle = 0;
        private int attachIndex = 0;
        private float totalTime = 0;
        private float playTime = 0;
        private float playSpeed = 0;

        /// <summary>
        /// 終了フラグ
        /// </summary>
        private bool finFlag = false;

        public bool FinFlag { get => finFlag; set => finFlag = value; }

        public MainProcess(IntPtr _windowHandle)
        {
            if (_windowHandle != IntPtr.Zero)
            {
                windowHandle = _windowHandle;
            }
            
        }

        public bool Initialize()
        {
            bool result = true;

            //ウィンドウモード変更
            DX.ChangeWindowMode(DX.TRUE);

            //logを出さない
            DX.SetOutApplicationLogValidFlag(DX.FALSE);

            //ウィンドウハンドル設定
            DX.SetUserWindow(windowHandle);

            //Zバッファの深度を設定(bit)
            DX.SetZBufferBitDepth(24);

            //裏画面のZバッファの深度を設定(bit)
            DX.SetCreateDrawValidGraphZBufferBitDepth(24);

            //画面のフルスクリーンアンチエイリアスモードの設定をする
            DX.SetFullSceneAntiAliasingMode(4, 2);

            if (DX.DxLib_Init() == -1)
            {
                result = false;
            }

            //描画先を裏に設定
            DX.SetDrawScreen(DX.DX_SCREEN_BACK);

            //mmdモデルをロード
            LoadModel();

            return result;
        }

        public bool Finalize()
        {
            bool result = true;

            // モデルハンドルの削除
            DX.MV1DeleteModel(modelHandle);

            DX.DxLib_End();

            return result;
        }

        private void LoadModel()
        {
            //モデルのパス取得
            if (File.Exists(__ModelParameterFile__))
            {
                string[] modelPara = File.ReadAllLines(__ModelParameterFile__, Encoding.GetEncoding("UTF-8"));

                string[] fromAngle = modelPara[(int)MODELPARA.from].Split(',');
                string[] toAngle = modelPara[(int)MODELPARA.to].Split(',');

                // 次に読み込むモデルの物理演算モードをリアルタイム物理演算にする
                DX.MV1SetLoadModelUsePhysicsMode(DX.DX_LOADMODEL_PHYSICS_REALTIME);

                //3Dモデル読み込み
                modelHandle = DX.MV1LoadModel(modelPara[(int)MODELPARA.modelPath]);
                //モーション選択
                attachIndex = DX.MV1AttachAnim(modelHandle, 0, -1, DX.FALSE);
                totalTime = DX.MV1GetAttachAnimTotalTime(modelHandle, attachIndex);

                //カメラアングル設定
                DX.SetCameraPositionAndTarget_UpVecY(DX.VGet(float.Parse(fromAngle[0]), float.Parse(fromAngle[1]), float.Parse(fromAngle[2])), 
                                                    DX.VGet(float.Parse(toAngle[0]), float.Parse(toAngle[1]), float.Parse(toAngle[2])));
            }

            //モーション再生位置
            playTime = 0.0f;
            //モーションスピード
            playSpeed = 0.4f;


            //奥行0.1～1000をカメラの描画範囲とする
            DX.SetCameraNearFar(0.1f, 1000.0f);

            // 物理演算の状態をリセット
            DX.MV1PhysicsResetState(modelHandle);

        }

        /// <summary>
        /// メインループ
        /// </summary>
        public void Run()
        {
            DX.ClearDrawScreen();

            //背景を設定(透過させる)
            DX.DrawBox(0, 0, windowWidth, windowHeight, DX.GetColor(1, 1, 1), DX.TRUE);


            playTime += playSpeed;

            //モーションの再生位置が終端まで来ると最初に
            if (playTime >= totalTime)
            {
                playTime = 0.0f;
                //モーション選択
                // 今までアタッチしていたアニメーションのデタッチ
                /*DX.MV1DetachAnim(modelHandle, attachIndex);
                attachIndex = DX.MV1AttachAnim(modelHandle, rand.Next(0,4), -1, DX.FALSE);
                totalTime = DX.MV1GetAttachAnimTotalTime(modelHandle, attachIndex);*/
            }
            //モーションの再生位置選択
            DX.MV1SetAttachAnimTime(modelHandle, attachIndex, playTime);

            // 物理演算を６０分の１秒経過したという設定で実行
            DX.MV1PhysicsCalculation(modelHandle, 1000.0f / 30.0f);
            /*string s = "p:" + totalTime;
            DX.DrawString(0, 0, s, DX.GetColor(255, 255, 255));*/


            DX.MV1DrawModel(modelHandle);

            //ESCキーを押したら終了
            if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) != 0)
            {
                finFlag = true;
            }

            DX.ScreenFlip();
        }
    }
}
