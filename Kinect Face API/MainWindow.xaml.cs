using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using Microsoft.Kinect;

namespace Kinect4
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        // 連接kinect
        private KinectSensor sensor;
        private ColorFrameReader colorFrameReader;
        private FrameDescription frameDescription;
        private WriteableBitmap wbData;     // 儲存影像的記憶體區塊
        private byte[] byteData;    //儲存影像每一pixel之RGB值的陣列參考

        string filePath;    //欲偵測人臉圖檔完整名稱
        BitmapImage bitmapSource;   //BitmapImage 物件參考

        //使用 subscription key
        private readonly IFaceServiceClient faceServiceClent =
            new FaceServiceClient("4b84a43021ee4799bb07ef07a1fe91f5");


        public MainWindow()
        {
            InitializeComponent();
            sensor = KinectSensor.GetDefault();
            colorFrameReader = sensor.ColorFrameSource.OpenReader();
            colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            frameDescription =
                sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            // wbData參考一個WriteableBitmap 物件(儲存影像的記憶體區塊)
            wbData = new WriteableBitmap(
                frameDescription.Width, frameDescription.Height, 96, 96, PixelFormats.Bgr32, null);
            // byteData參考一個儲存影像每一個pixel之rgb值得byte陣列
            byteData = new byte[frameDescription.Width * frameDescription.Height * 4];
            //啟動Kinect Sensor
            sensor.Open();
            //顯示訊息
            Result.Text = "Kinect 傳送影像";
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                //如果影格資料不存在，就直接離開事件處理函式
                if (colorFrame == null)
                    return;
                Int32Rect bitmapRect = new Int32Rect(0, 0, frameDescription.Width, frameDescription.Height);
                //將影格每一個pixel之RGB值儲存至byteData陣列
                colorFrame.CopyConvertedFrameDataToArray(byteData, ColorImageFormat.Bgra);
                //將byteData陣列的資料存入wbData
                wbData.WritePixels(bitmapRect, byteData, (int)
                    (frameDescription.Width * frameDescription.BytesPerPixel), 0);
            }
            //throw new NotImplementedException();
        }

        private void Kinect04_Loaded(object sender, RoutedEventArgs e)
        {
            //指定FaceImage影像來源為wbData(WritebleBitmap物件)
            FaceImage.Source = wbData;
        }

        private void Kinect04_unloaded(object sender, RoutedEventArgs e)
        {
            //取消監聽ColorFrameReader 之 FrameArrived事件
            colorFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
            //停止Kinect Sensor
            sensor.Close();
        }

        private void Kinect_Click(object sender, RoutedEventArgs e)
        {
            //指定FaceImage影像來源為wbData(WritebleBitmap物件)
            FaceImage.Source = wbData;
            //Kinect 重新傳送資料
            sensor.Open();
            //顯示訊息
            Result.Text = "Kinect 傳送影像";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //影像檔一系統時間命名
            string fileName = "C" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
            using (FileStream saveImage = new FileStream(fileName, FileMode.CreateNew))
            {
                //從ColorImage.Source處取出一張影像，轉為BitmapSource格式
                //儲存到imageSource
                BitmapSource imageSource = (BitmapSource)FaceImage.Source;
                //挑選Joint Photographic Experts Group(JPEG)影像編碼器
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                //將取出的影像加到編碼器的影像集
                encoder.Frames.Add(BitmapFrame.Create(imageSource));
                //儲存影像與後續影像清除工作
                encoder.Save(saveImage);
                saveImage.Flush();
                saveImage.Close();
                saveImage.Dispose();
            }
            //顯示訊息
            Result.Text = "儲存影像檔案" + fileName;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var openDlg = new Microsoft.Win32.OpenFileDialog();

            //取得程式所在目錄
            string currentpath = Directory.GetCurrentDirectory();
            openDlg.InitialDirectory = currentpath;

            openDlg.Filter = "JPEG Image(*.jpg)|*.jpg";
            bool? result = openDlg.ShowDialog(this);
            if (!(bool)result)
            {
                return;
            }
            filePath = openDlg.FileName;
            MessageBox.Show(filePath);
            //選定欲偵測人臉圖檔後,停止Kinect傳送影像
            sensor.Close();
            //FaceImage秀出欲偵測人臉圖檔之影像
            Uri fileUri = new Uri(filePath);
            bitmapSource = new BitmapImage();
            bitmapSource.BeginInit();
            bitmapSource.UriSource = fileUri;
            bitmapSource.EndInit();
            FaceImage.Source = bitmapSource;
            //顯示訊息
            Result.Text = "選定圖檔" + filePath;
        }

        private async void Detect_Click(object sender, RoutedEventArgs e)
        {
            //如果讓位指定欲偵測人臉圖檔，則甚麼都不做
            if (filePath == null)
                return;

            //如果有指定欲偵測人臉圖檔，則顯示訊息、上船圖檔至雲端進行人臉偵測
            Result.Text = "偵測中...";
            //指定傳回诶個人臉之年齡、性別、微笑值三個屬性
            var requiredFaceAttributes = new FaceAttributeType[]
            {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile
            };
            FaceRectangle[] faceRects;
            FaceAttributes[] attributes;

            using (Stream imageFileStream = File.OpenRead(filePath))
            {
                var faces = await faceServiceClent.DetectAsync(
                    imageFileStream,
                    returnFaceAttributes: requiredFaceAttributes);
                faceRects = faces.Select(Face => Face.FaceRectangle).ToArray();
                attributes = faces.Select(Face => Face.FaceAttributes).ToArray();
            }

            //接收傳回資料後，加工處理年齡、性別、微笑值資料
            int female = 0, male = 0, adult = 0, child = 0;
            double youngest = 120, oldest = 0, smilest = 0;
            foreach (var attribute in attributes)
            {
                var gender = attribute.Gender;
                if (gender == "male")
                    male++;
                else if (gender == "female")
                    female++;
                else
                    MessageBox.Show("Unknown Gender!");

                var age = attribute.Age;
                if (age >= 20)
                    adult++;
                else
                    child++;
                if (age < youngest)
                    youngest = age;
                if (age > oldest)
                    oldest = age;
                var smile = attribute.Smile;
                if (smile > smilest)
                    smilest = smile;
            }

            //顯示訊息
            Result.Text = String.Format("偵測完成! {0} 個人臉被偵測出", faceRects.Length);

            if (faceRects.Length > 0)
            {
                string data = "\n{0} 個男性   {1} 個女性  "
                    + "{2} 個成年人   {3} 個未成年"
                    + "\n年齡為最輕 {4}   年齡最長為 {5}"
                    + "\n微笑值最高為  {6}  ";
                Result.Text += String.Format(data, male, female, adult, child, youngest, oldest, smilest);
            }

            //在圖像上依年齡、性別、微笑值資料畫出不同顏色、大小的方框
            if (faceRects.Length > 0)
            {
                DrawingVisual visual = new DrawingVisual();
                DrawingContext drawingContext = visual.RenderOpen();
                drawingContext.DrawImage(bitmapSource,
                    new Rect(0, 0, bitmapSource.Width, bitmapSource.Height));
                double dpi = bitmapSource.DpiX;
                double resizeFactor = 96 / dpi;

                FaceRectangle faceRect;
                for (int i = 0; i < faceRects.Length; i++)
                {
                    faceRect = faceRects[i];
                    if (attributes[i].Smile == smilest)
                    {
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Gold, 5),   //微笑最高
                            new Rect(
                                (faceRect.Left - 3) * resizeFactor,
                                (faceRect.Top - 3) * resizeFactor,
                                (faceRect.Width + 6) * resizeFactor,
                                (faceRect.Height + 6) * resizeFactor
                                )
                        );
                    }
                    if (attributes[i].Age == youngest)
                    {
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Silver, 5),     //最年輕
                            new Rect(
                                (faceRect.Left - 6) * resizeFactor,
                                (faceRect.Top - 6) * resizeFactor,
                                (faceRect.Width + 12) * resizeFactor,
                                (faceRect.Height + 12) * resizeFactor
                                )
                            );
                    }
                    if (attributes[i].Age == oldest)
                    {
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Black, 5),      //最老
                            new Rect(
                                (faceRect.Left - 9) * resizeFactor,
                                (faceRect.Top - 9) * resizeFactor,
                                (faceRect.Width + 18) * resizeFactor,
                                (faceRect.Height + 18) * resizeFactor
                                )
                            );
                    }
                }
                for (int i = 0; i < faceRects.Length; i++)
                {
                    faceRect = faceRects[i];
                    if (attributes[i].Gender == "male" && attributes[i].Age >= 20)
                    {
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Blue, 5),   //男，成年
                            new Rect(
                                faceRect.Left * resizeFactor,
                                faceRect.Top * resizeFactor,
                                faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor
                                )
                            );
                    }
                    else if (attributes[i].Gender == "male" && attributes[i].Age < 20)
                    {
                        drawingContext.DrawRectangle(
                            Brushes.Transparent,
                            new Pen(Brushes.Green, 5),      //男，未成年
                            new Rect(
                                faceRect.Left * resizeFactor,
                                faceRect.Top * resizeFactor,
                                faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor
                                )
                            );
                    }
                    else if (attributes[i].Gender == "female" && attributes[i].Age >= 20)
                    {
                        drawingContext.DrawRectangle(
                        Brushes.Transparent,
                            new Pen(Brushes.Red, 5),    //女，成年
                            new Rect(
                                faceRect.Left * resizeFactor,
                                faceRect.Top * resizeFactor,
                                faceRect.Width * resizeFactor,
                                faceRect.Height * resizeFactor
                                )
                            );
                    }
                    else
                    {
                        drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(Brushes.Red, 5),    //女，未成年
                        new Rect(
                            faceRect.Left * resizeFactor,
                            faceRect.Top * resizeFactor,
                            faceRect.Width * resizeFactor,
                            faceRect.Height * resizeFactor
                            )
                        );
                    }
                }
                drawingContext.Close();
                RenderTargetBitmap faceWithRectBitmap = new RenderTargetBitmap(
                    (int)(bitmapSource.PixelWidth * resizeFactor),
                    (int)(bitmapSource.PixelHeight * resizeFactor),
                    96,
                    96,
                    PixelFormats.Pbgra32);
                faceWithRectBitmap.Render(visual);
                FaceImage.Source = faceWithRectBitmap;
            }

        }
    }
}
