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
using System.Threading;
using Grpc.Core;
using Frame.Grpc;

namespace FrameView
{
    public enum Color
    {
        Unknown = 0,
        Red = 1,
        Blue = 2
    }

    public enum Style
    {
        Unknown = 0,
        Solid = 1,
        Dashed = 2
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameData frameData { get; set; }
        private Semaphore getFrameSem = new Semaphore(0, 1, "getFrameSem");
        private Semaphore updateSem = new Semaphore(1, 1, "updateSem");

        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += (sender, e) => { this.DragMove(); };
            this.MouseDoubleClick += (sender, e) => { Application.Current.Shutdown(); };
            Channel channel = new Channel("127.0.0.1:11051", ChannelCredentials.Insecure);
            var client = new FrameService.FrameServiceClient(channel);
            var call = client.ReceiveFrameDataStream(new Google.Protobuf.WellKnownTypes.Empty());
            var responseReaderTask = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext())
                {
                    updateSem.WaitOne();
                    this.frameData = call.ResponseStream.Current;
                    getFrameSem.Release(1);
                }
            });
            Thread t = new Thread(new ThreadStart(UpdateProc));
            t.Start();
        }

        private void UpdateProc()
        {
            int prevNum = 0;
            var boxList = new List<Frame>();
            while (true)
            {
                getFrameSem.WaitOne();
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (prevNum < this.frameData.Num)
                    {
                        for (int i = 0; i < this.frameData.Num - prevNum; i++)
                        {
                            boxList.Add(new Frame(this.frameData.Frames[i].X1 * Main.Width,
                                                  this.frameData.Frames[i].Y1 * Main.Height,
                                                  this.frameData.Frames[i].X2 * Main.Width,
                                                  this.frameData.Frames[i].Y2 * Main.Height,
                                                  FrameField));
                        }
                        prevNum = this.frameData.Num;
                    }
                    if (prevNum > this.frameData.Num)
                    {
                        for (int i = prevNum - 1; i < prevNum - frameData.Num; i--)
                        {
                            boxList[i].Hide();
                            boxList[i] = null;
                            boxList.RemoveAt(i);
                        }
                        prevNum = this.frameData.Num;
                    }
                    foreach (var frame in this.frameData.Frames.Select((v, i) => new { Value = v, Index = i }))
                    {
                        boxList[frame.Index].SetCoordinte(frame.Value.X1 * Main.Width,
                                                          frame.Value.Y1 * Main.Height,
                                                          frame.Value.X2 * Main.Width,
                                                          frame.Value.Y2 * Main.Height);
                    }
                    updateSem.Release(1);
                }));
            }
        }
    }

    public class Frame
    {
        private Line _top;
        private Line _right;
        private Line _bottom;
        private Line _left;
        private Grid _grid;

        public Frame(double x1, double y1, double x2, double y2, Grid grid)
        {
            _top = new Line();
            _right = new Line();
            _bottom = new Line();
            _left = new Line();

            _top.X1 = x1;
            _top.X2 = x2;
            _top.Y1 = y1;
            _top.Y2 = y1;

            _right.X1 = x2;
            _right.X2 = x2;
            _right.Y1 = y1;
            _right.Y2 = y2;

            _bottom.X1 = x1;
            _bottom.X2 = x2;
            _bottom.Y1 = y2;
            _bottom.Y2 = y2;

            _left.X1 = x1;
            _left.X2 = x1;
            _left.Y1 = y1;
            _left.Y2 = y2;

            _grid = grid;

            _top.Stroke = System.Windows.Media.Brushes.Cyan;
            _right.Stroke = System.Windows.Media.Brushes.Cyan;
            _bottom.Stroke = System.Windows.Media.Brushes.Cyan;
            _left.Stroke = System.Windows.Media.Brushes.Cyan;

            _top.StrokeThickness = 2;
            _right.StrokeThickness = 2;
            _bottom.StrokeThickness = 2;
            _left.StrokeThickness = 2;

            grid.Children.Add(_top);
            grid.Children.Add(_right);
            grid.Children.Add(_bottom);
            grid.Children.Add(_left);
        }

        public void Show()
        {
            _grid.Children.Add(_top);
            _grid.Children.Add(_right);
            _grid.Children.Add(_bottom);
            _grid.Children.Add(_left);
        }

        public void Hide()
        {
            _grid.Children.Remove(_top);
            _grid.Children.Remove(_right);
            _grid.Children.Remove(_bottom);
            _grid.Children.Remove(_left);
        }

        public void SetCoordinte(double x1, double y1, double x2, double y2)
        {
            _top.X1 = x1;
            _top.X2 = x2;
            _top.Y1 = y1;
            _top.Y2 = y1;

            _right.X1 = x2;
            _right.X2 = x2;
            _right.Y1 = y1;
            _right.Y2 = y2;

            _bottom.X1 = x1;
            _bottom.X2 = x2;
            _bottom.Y1 = y2;
            _bottom.Y2 = y2;

            _left.X1 = x1;
            _left.X2 = x1;
            _left.Y1 = y1;
            _left.Y2 = y2;
        }
    }
}
