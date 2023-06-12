
// 引用需要的库
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Analysis;
using DotSpatial.Symbology;


namespace Contour.Core2
{
    public class Class1
    {
        public void Start()
        {

            IFeatureSet featureSet = FeatureSet.Open(@"C:\path\to\data.shp");
            // 加载需要生成等值线的数据
            IRaster raster = Raster.Open(@"C:\path\to\data.tif");
            //IFeatureSet contourLines = raster.ToPolygons(10, 20, 1);
            //raster.CreateHillShade();
            // 生成等值线
            //IFeatureSet contourLines = raster.CreateContours(10, 20, 1);
            //Geometry geometry = new 

            //Geometry.InterpolatePoints(featureSet, 10, 20, 1);
            
        }
    }
}