//using MathNet.Spatial.Triangulation;
using Contour.Core;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using TriangleNet;
using TriangleNet.Meshing;
using TriangleNet.Meshing.Algorithm;
using Utils;
using Utils.Models;
using File = System.IO.File;
using PropertyChanged;
using DelaunayTriangle.TestUI.Models;
using DelaunayTriangle.TestUI.Views;
using System.IO;
using SharpGLTF.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DelaunayTriangle.TestUI.ViewModels
{

    internal class MainWindowViewModel : BindableBase
    {

        public int Presion { get; set; } = 1;
        /// <summary>
        /// 状态
        /// 0 生成点
        /// 1 生成三角面
        /// 2 生成等高线
        /// 3 生成等值线
        /// </summary>
        public bool[] Status { get; set; } = new bool[4] { false, false, false, false };
        /* 1. Point
         * 2. Tri
         * 3. Edge
         * 4. ContourLines
         */
        public Dictionary<string, ParamSetting> Settings { get; set; } = new Dictionary<string, ParamSetting>()
        {
            {"点",new ParamSetting(){Name="点",IsChecked=true,Color = Colors.Green,} },
            {"三角面",new ParamSetting(){Name="三角面",IsChecked=true,Color = Colors.Goldenrod} },
            {"主等高线",new ParamSetting(){Name="主等高线",IsChecked=true,Color = Colors.GreenYellow} },
            {"次等高线",new ParamSetting(){Name="次等高线",IsChecked=true,Color = Colors.LightGreen} },
            {"等高线涉及的边",new ParamSetting(){Name="等高线涉及的边",IsChecked=true,Color = Colors.Red}},
            {"XY轴",new ParamSetting(){Name="XY轴",IsChecked=true,Color = Colors.Black}},
            {"点信息",new ParamSetting(){Name="点坐标",IsChecked=false,Color = Colors.Red} },
        };
        #region Test
        public int? TriCount { get; set; }
        public int? PointCount { get; set; }
        public double? MaxElevation { get; set; }
        public double? MinElevation { get; set; }
        public ObservableCollection<Elevation> Elevations { get; set; } = new ObservableCollection<Elevation>() { };
        public double? Elevation { get; set; }
        /// <summary>
        /// 主等高线
        /// </summary>
        public double? MainElevation { get; set; } = 2;
        /// <summary>
        /// 次等高线
        /// </summary>
        public double? SubElevation { get; set; } = 1;

        public Canvas Canvas { get; set; } = new Canvas();
        public string FilePath { get; set; }
        public Dictionary<int, TIN_Point> Points { get; set; } = new Dictionary<int, TIN_Point>();
        Draw draw;
        IMesh Mesh;

        #region Commmands

        public DelegateCommand ImportCsvCmd { get; set; }
        public DelegateCommand GenContourLinesCmd { get; set; }
        public DelegateCommand GenTrianglesCmd { get; set; }
        public DelegateCommand GenElevationsCmd { get; set; }
        public DelegateCommand LoadDataCmd { get; set; }
        public DelegateCommand ClearCmd { get; set; }
        public DelegateCommand AddElevationCmd { get; set; }
        public DelegateCommand<Elevation> RemoveElevationCmd { get; set; }
        public DelegateCommand<object> OpenColorDialogCmd { get; set; }
        public DelegateCommand OpenDataViewCmd { get; set; }
        public DelegateCommand SaveConfigCmd { get; set; }

        #endregion
        public MainWindowViewModel()
        {
            draw = new Draw(Canvas);
            GenContourLinesCmd = new DelegateCommand(GenContourLines);
            GenTrianglesCmd = new DelegateCommand(GenTri);
            GenElevationsCmd = new DelegateCommand(GenElevations);
            ImportCsvCmd = new DelegateCommand(ImportCsv);
            ClearCmd = new DelegateCommand(Clear);
            SaveConfigCmd = new DelegateCommand(SaveConfig);
            LoadConfig();
            LoadDataCmd = new DelegateCommand(() =>
            {
                if (!File.Exists(FilePath))
                {
                    FilePath = @"F:\Document\WXWork\1688856189372130\Cache\File\2022-12\点数据2000.txt";
                }
                LoadData(FilePath);
                Status[0] = true;
            });
            OpenDataViewCmd = new DelegateCommand(OpenDataView);
            AddElevationCmd = new DelegateCommand(AddElevation);
            RemoveElevationCmd = new DelegateCommand<Elevation>(RemoveElevation);
            OpenColorDialogCmd = new DelegateCommand<object>(OpenColorDialog);
        }

        private void LoadConfig()
        {
            string file = Path.Combine(Path.GetTempPath(), "config.json");
            if (File.Exists(file))
            {
                Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(file));
                var a = dict["Elevation"];
                if (dict.ContainsKey("Elevation") && dict["Elevation"] is JArray eles)
                {
                    List<Elevation> elevations = eles.Select(x => x.ToObject<Elevation>()).ToList();
                    if (elevations != null) Elevations = new ObservableCollection<Elevation>(elevations);
                }

                if (dict.ContainsKey("Settings") && dict["Settings"] is JObject setting)
                {
                    Settings = setting.ToObject<Dictionary<string, ParamSetting>>();
                }
            }
        }
        private void SaveConfig()
        {
            string file = Path.Combine(Path.GetTempPath(), "config.json");

            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Elevation", Elevations);
            dict.Add("Settings", Settings);
            string content = JsonConvert.SerializeObject(dict);
            File.WriteAllText(file, content);
        }

        private void OpenDataView()
        {
            DataView view = new DataView(Points.Select(x => x.Value).ToList());
            view.ShowDialog();
            if (view.DialogResult == true)
            {
                Points = view.Points.ToDictionary(x => x.Id, y => y);
            }
        }

        private void OpenColorDialog(object obj)
        {
            if (obj is ParamSetting setting)
            {

                ColorDialog dialog = new ColorDialog()
                {
                    Color = setting.Color.ToDrawingColor(),
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    setting.Color = dialog.Color.ToMediaColor();
                }
            }
            else if (obj is Elevation elevation)
            {
                ColorDialog dialog = new ColorDialog()
                {
                    Color = elevation.Color.ToDrawingColor(),
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    elevation.Color = dialog.Color.ToMediaColor();
                }
            }
        }


        #region 等高线
        private void RemoveElevation(Elevation obj)
        {
            if (obj != null)
                Elevations.Remove(obj);
            if (Elevations.Count == 0) Status[2] = false;
        }

        private void AddElevation()
        {
            var list = Elevations.ToList();

            if (Elevation != null)
            {
                list.Add(new Elevation { Height = Elevation.Value, Type = 0 });
                Elevations = new ObservableCollection<Elevation>(list);
                Status[2] = true;
            }

            Elevation = null;
        }

        /// <summary>
        /// 根据间隙生成等高线
        /// </summary>
        void GenElevations()
        {
            /* 根据精度划分范围。。  
             */
            if (!Status[0]) LoadDataCmd.Execute();
            if (SubElevation == null) SubElevation = 0;

            //if (SubElevation < MainElevation) MessageBox.Show("fail");

            double space = (double)(MaxElevation - MinElevation);

            var list = new List<Elevation>();
            if (SubElevation.HasValue)
                list.AddRange(GetElevations(SubElevation.Value, 0));
            if (MainElevation.HasValue)
                list.AddRange(GetElevations(MainElevation.Value, 1));
            list.Sort((x, y) => (int)(x.Height - y.Height));

            Elevations = new ObservableCollection<Elevation>(Union(list));
            Status[2] = true;
        }
        IEnumerable<Elevation> Union(List<Elevation> list)
        {
            int i = 1;
            while (i < list.Count)
            {
                if (Math.Abs(list[i - 1].Height - list[i].Height) < Math.Pow(10, -Presion) * 0.1)
                {
                    // 优先移除SubMainType
                    var s = new List<Elevation>() { list[i], list[i - 1] };
                    var a = s.FirstOrDefault(x => x.Type == 0);
                    if (a != null)
                    {
                        list.Remove(a);
                    }
                    else i++;
                }
                else i++;
            }
            return list;
        }

        IEnumerable<Elevation> GetElevations(double space, int type)
        {
            var list = Enumerable.Range(0, (int)Math.Floor((double)((double)(MaxElevation - MinElevation) / space)))
                .Select(x => (double)(x * space + MinElevation)).Select(x =>
                {
                    return new Elevation()
                    {
                        Height = x,
                        Color = type == 0 ? Settings["次等高线"].Color : Settings["主等高线"].Color,
                        Type = type
                    };
                }
            );
            return list;
        }

        #endregion
        private void ImportCsv()
        {
            new Action<string>((m) =>
            {
                FilePath = m;
                LoadData(FilePath);

            }).MyOpenFileDialog("选择py文件", "csv", "txt");
        }

        private void Clear()
        {
            Points = new Dictionary<int, TIN_Point>();
            Canvas = new Canvas();
            TIN_Point.Clear();
            Mesh = null;
            draw = new Draw(Canvas);
            Status = Status.Select(x => false).ToArray();
            TriCount = null;
            PointCount = null;
            MaxElevation = null;
            MinElevation = null;
            Elevations = new ObservableCollection<Elevation>();
            Elevation = null;
        }

        #endregion
        void GenTri()
        {
            if (!Status[0]) LoadDataCmd.Execute();
            var queue = new ConcurrentQueue<int>();
            // Each task has it's own triangle pool and predicates instance.
            var pool = new TrianglePool();
            var predicates = new RobustPredicates();

            // The factory methods return the above instances.
            var config = new Configuration()
            {
                Predicates = () => predicates,
                TrianglePool = () => pool.Restart(),
            };

            var triangulator = new Dwyer();
            Mesh = triangulator.Triangulate(Points.Select(x => x.Value.Vertex).ToList(), config);
            TriCount = Mesh.Triangles.Count;
            var edges = Mesh.Edges.Select(x => new System.Windows.Point[] { Points[x.P1].Point2D, Points[x.P0].Point2D }).ToList();
            if (Settings["三角面"].IsChecked)
            {
                draw.SetColor(Settings["三角面"].Color);
                draw.drawLine(edges, "三角面");
            }
            Status[1] = true;
        }

        void GenContourLines()
        {
            if (!Status[0]) LoadDataCmd.Execute();
            if (!Status[1]) GenTri();
            if (!Status[2]) GenElevations();
            ContourService service = new ContourService(Mesh, Points);
            foreach (var e in Elevations)
            {

                var list = service.GetContourLines(e.Height);
                draw.SetColor(e.Color);
                if ((e.Type == 0 && Settings["主等高线"].IsChecked) ||
                    (e.Type == 1 && Settings["次等高线"].IsChecked))
                {
                    int i = 0;
                    Color[] colors = new Color[7]
                    {
                        Colors.Black,
                        Colors.Bisque,
                        Colors.Azure,
                        Colors.Cyan,
                        Colors.DarkBlue,
                        Colors.GreenYellow,
                        Colors.Navy,
                    };
                    foreach (var m in list)
                    {
#if DEBUG
                        i++;
                        draw.SetColor(m.IsClosed ? Colors.Red : Colors.Black);
#endif

                        draw.drawLine(m.Points, m.IsClosed, isShowIndex: false, i++);
                    }

                }
            }
            if (Settings["等高线涉及的边"].IsChecked)
            {
                draw.SetColor(Settings["等高线涉及的边"].Color);
                service.Lines.ForEach(m =>
                    {
                        draw.drawLine(m.Edges);
                    });
            }
            Status[3] = true;

        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <param name="filePath"></param>
        void LoadData(string filePath)
        {
            if (!File.Exists(filePath)) return;
            Points = File.ReadAllLines(filePath).Where(x => x.Length > 0 && x.Split(',', ' ').Length >= 2).Select(p =>
            {
                var line = p.Split(' ', ',');
                int n = line.Length;
                double x = Math.Round(double.Parse(line[n - 3]), Presion);
                double y = Math.Round(double.Parse(line[n - 2]), Presion);
                double z = Math.Round(double.Parse(line[n - 1]), Presion);
                return new TIN_Point(x, y, z);
            }).ToDictionary(x => x.Id, y => y);
            double minX = Points.Min(x => x.Value.Point2D.X);
            double minY = Points.Min(x => x.Value.Point2D.Y);
            TIN_Point.Clear();
            Points = Points.Select(p => new TIN_Point(p.Value.Point2D.X - minX, p.Value.Point2D.Y - minY, p.Value.Height))
             .ToDictionary(x => x.Id, y => y);

            draw.SetColor(Settings["XY轴"].Color);
            draw.SetRatio(Points.Select(x => x.Value.Point2D), Settings["XY轴"].IsChecked);
            PointCount = Points.Count;
            Status[0] = true;
            MaxElevation = Points.Max(x => x.Value.Height);
            MinElevation = Points.Min(x => x.Value.Height);
            if (Settings["点"].IsChecked)
            {
                draw.SetColor(Settings["点"].Color);
                if (Settings["点信息"].IsChecked)
                {
                    draw.SetSubColor(Settings["点信息"].Color);
                    draw.drawPoints(Points.Select(x => x.Value).ToList(),
                    isShowXY: true,
                    isShowIndex: true,
                    isShowZ: true,
                    presion: Presion);
                }
                else
                {
                    draw.drawPoints(Points.Select(x => x.Value).ToList(),
                    isShowXY: false,
                    isShowIndex: false,
                    isShowZ: false,
                    presion: Presion);
                }
            }
        }

    }


}
