// copyright Runette Software Ltd, 2020. All rights reserved
using System.Collections.Generic;
using UnityEngine;
using GeoJSON.Net.Geometry;
using GeoJSON.Net.Feature;
using GeoJSON.Net;
using System.Threading.Tasks;
using Project;
using UnityEngine.UI;

namespace Virgis
{

    public class PointLayer : Layer<GeographyCollection, FeatureCollection>
    {
        // The prefab for the data points to be instantiated
        public GameObject SpherePrefab;
        public GameObject CubePrefab;
        public GameObject CylinderPrefab;
        public GameObject LabelPrefab;

        // used to read the GeoJSON file for this layer
        private GeoJsonReader geoJsonReader;

        private GameObject PointPrefab;

        public override async Task _init(GeographyCollection layer)
        {
            geoJsonReader = new GeoJsonReader();
            await geoJsonReader.Load(layer.Source);
            features = geoJsonReader.getFeatureCollection();
        }

        public override void _add(MoveArgs args)
        {
            throw new System.NotImplementedException();
        }

        public override void _draw()
        {
            Dictionary<string, Unit> symbology = layer.Properties.Units;
            float displacement = 1.0f;
            if (symbology.ContainsKey("point") && symbology["point"].ContainsKey("Shape"))
            {
                Shapes shape = symbology["point"].Shape;
                switch (shape)
                {
                    case Shapes.Spheroid:
                        PointPrefab = SpherePrefab;
                        break;
                    case Shapes.Cuboid:
                        PointPrefab = CubePrefab;
                        break;
                    case Shapes.Cylinder:
                        PointPrefab = CylinderPrefab;
                        displacement = 1.5f;
                        break;
                    default:
                        PointPrefab = SpherePrefab;
                        break;
                }
            }
            else
            {
                PointPrefab = SpherePrefab;
            }

            foreach (Feature feature in features.Features)
            {
                // Get the geometry
                MultiPoint mPoint = null;
                if (feature.Geometry.Type == GeoJSONObjectType.Point)
                {
                    mPoint = new MultiPoint(new List<Point>() { feature.Geometry as Point });
                }
                else if (feature.Geometry.Type == GeoJSONObjectType.MultiPoint)
                {
                    mPoint = feature.Geometry as MultiPoint;
                }

                Dictionary<string, object> properties = feature.Properties as Dictionary<string, object>;
                string gisId = feature.Id;
                foreach (Point geometry in mPoint.Coordinates)
                {
                    Position in_position = geometry.Coordinates as Position;
                    Vector3 position = Tools.Ipos2Vect(in_position);

                    //instantiate the prefab with coordinates defined above
                    GameObject dataPoint = Instantiate(PointPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    dataPoint.transform.parent = gameObject.transform;

                    // add the gis data from geoJSON
                    Datapoint com = dataPoint.GetComponent<Datapoint>();
                    com.gisId = gisId;
                    com.gisProperties = properties;

                    //Set the symbology
                    if (symbology.ContainsKey("point"))
                    {
                        dataPoint.SendMessage("SetColor", (Color)symbology["point"].Color);
                        dataPoint.transform.localScale = symbology["point"].Transform.Scale;
                        dataPoint.transform.localRotation = symbology["point"].Transform.Rotate;
                        dataPoint.transform.localPosition = symbology["point"].Transform.Position;
                        dataPoint.transform.position = position;
                    }

                    //Set the label
                    GameObject labelObject = Instantiate(LabelPrefab, Vector3.zero, Quaternion.identity);
                    labelObject.transform.parent = dataPoint.transform;
                    labelObject.transform.localPosition = Vector3.up * displacement;
                    Text labelText = labelObject.GetComponentInChildren<Text>();

                    if (symbology.ContainsKey("point") && symbology["point"].ContainsKey("Label") && symbology["point"].Label != null && properties.ContainsKey(symbology["point"].Label))
                    {
                        labelText.text = (string)properties[symbology["point"].Label];
                    }
                }
            };
        }

        public override void ExitEditsession()
        {
            BroadcastMessage("EditEnd", SendMessageOptions.DontRequireReceiver);
        }

        public override void _cp() { }
        public override void _save()
        {
            Datapoint[] pointFuncs = gameObject.GetComponentsInChildren<Datapoint>();
            List<Feature> features = new List<Feature>();
            foreach (Datapoint pointFunc in pointFuncs)
            {
                features.Add(new Feature(new Point(Tools.Vect2Ipos(pointFunc.gameObject.transform.position)), pointFunc.gisProperties, pointFunc.gisId));
            }
            FeatureCollection FC = new FeatureCollection(features);
            geoJsonReader.SetFeatureCollection(FC);
            geoJsonReader.Save();
        }


        public override void Translate(MoveArgs args)
        {
            gameObject.BroadcastMessage("TranslateHandle", args, SendMessageOptions.DontRequireReceiver);
            changed = true;
        }

        public override void MoveAxis(MoveArgs args)
        {

        }

        /*public override VirgisComponent GetClosest(Vector3 coords)
        {
            throw new System.NotImplementedException();
        }*/

    }
}