using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;

namespace BlazorComponentStructureVisualizer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoadB_Click(object sender, RoutedEventArgs e)
    {
        var path = PathTB.Text;
        if (Directory.Exists(path))
        {
            ComponentInfos.Clear();

            foreach (var fileName in Directory.GetFiles(path, "*.g.cs"))
            {
                var compInfo = LoadComponentInfo(fileName);
                ComponentInfos.Add(compInfo.FullName, compInfo);
            }
        }


        Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
        foreach (var compInfo in ComponentInfos) 
        {
            AddNode(graph, compInfo.Value);
        }

        foreach (var compInfo in ComponentInfos)
        {
            AddEdges(graph, compInfo.Value);
        }

        graphControl.Graph = graph;
    }

    private void AddEdges(Graph graph, ComponentInfo compInfo)
    {
        foreach (var subCompName in compInfo.SubComponents)
        {
            if (!ComponentInfos.ContainsKey(subCompName))
            {
                if (!ExternalComponents.Contains(subCompName))
                {
                    ExternalComponents.Add(subCompName);
                    var extNode = graph.AddNode(subCompName);
                    extNode.LabelText = subCompName.Split('.').Last();
                    extNode.Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
                }
            }
            
            graph.AddEdge(compInfo.FullName, subCompName);
        }
    }

    private void AddNode(Graph graph, ComponentInfo compInfo) => graph.AddNode(compInfo.FullName).LabelText = compInfo.ClassName;

    private ComponentInfo LoadComponentInfo(string fileName)
    {
        ComponentInfo result = new();
        result.FileName = fileName;
        var lines = File.ReadAllLines(fileName);
        foreach (var line in lines)
        {
            if (line.TrimStart().StartsWith("namespace "))
            {
                result.NameSpace = line.TrimStart().Substring("namespace ".Length);
            }

            else if (line.TrimStart().StartsWith("public partial class "))
            {
                var classDecl = line.TrimStart().Substring("public partial class ".Length);
                var declParts = classDecl.Split(':');
                result.ClassName = declParts[0].Trim();
                result.BaseType = declParts[1].Trim();
            }
            else if (line.TrimStart().StartsWith("__builder.OpenComponent<global::"))
            {
                var classname = line.TrimStart().Substring("__builder.OpenComponent<global::".Length);
                var endIdx = classname.LastIndexOf('>');
                result.SubComponents.Add(classname.Substring(0, endIdx));
            }
        }
        return result;
    }

    private Dictionary<string, ComponentInfo> ComponentInfos = new();
    private HashSet<string> ExternalComponents = new();
}

public class ComponentInfo
{
    public string FileName { get; set; }
    public string ClassName { get; set; }
    public string FullName => $"{NameSpace}.{ClassName}";
    public string NameSpace { get; set; }
    public string Name { get; set; }
    public HashSet<string> SubComponents { get; } = new();
    public string BaseType { get; set; }
}